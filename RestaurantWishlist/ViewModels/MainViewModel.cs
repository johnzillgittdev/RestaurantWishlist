using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantWishlist.Enums;
using RestaurantWishlist.Helpers;
using RestaurantWishlist.Models;
using RestaurantWishlist.Repository;

namespace RestaurantWishlist.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IRestaurantRepository _repository;
    private const int RepeatCount = 50;

    // ── Data ─────────────────────────────────────────────────────────────────

    private List<Restaurant> _allRestaurants = [];
    public List<Restaurant> VisibleRestaurants { get; private set; } = [];
    public List<RestaurantDisplayItem> DisplayItems { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEmpty))]
    public partial bool IsEmpty { get; set; }

    public bool IsNotEmpty => !IsEmpty;

    [ObservableProperty] public partial bool IsFiltered { get; set; }
    [ObservableProperty] public partial bool IsSpinning { get; set; }

    // Active filter state (applied)
    private readonly HashSet<Cuisine> _filterCuisines = [];
    private readonly HashSet<Cost> _filterCosts = [];

    // ── Sheet visibility ──────────────────────────────────────────────────────

    [ObservableProperty] public partial bool IsAddSheetVisible    { get; set; }
    [ObservableProperty] public partial bool IsEditSheetVisible   { get; set; }
    [ObservableProperty] public partial bool IsFilterSheetVisible { get; set; }

    // ── Add sheet ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveNewRestaurantCommand))]
    public partial string NewName { get; set; }

    public ObservableCollection<CuisineOption> AddCuisineOptions { get; } = [];
    public ObservableCollection<CostOption> AddCostOptions { get; } = [];

    // ── Edit sheet ────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveEditCommand))]
    public partial string EditName { get; set; }

    private Restaurant? _editingRestaurant;
    public ObservableCollection<CuisineOption> EditCuisineOptions { get; } = [];
    public ObservableCollection<CostOption> EditCostOptions { get; } = [];

    // ── Filter sheet ──────────────────────────────────────────────────────────

    public ObservableCollection<CuisineOption> FilterCuisineOptions { get; } = [];
    public ObservableCollection<CostOption> FilterCostOptions { get; } = [];

    // ── Events for View ───────────────────────────────────────────────────────

    /// <summary>Raised when DisplayItems is rebuilt; View should re-center the wheel.</summary>
    public event Action? DisplayItemsChanged;

    // ── Constructor ───────────────────────────────────────────────────────────

    public MainViewModel(IRestaurantRepository repository)
    {
        _repository = repository;
        IsEmpty = true;
        NewName = string.Empty;
        EditName = string.Empty;
        InitializeChipOptions();
    }

    private void InitializeChipOptions()
    {
        foreach (var cuisine in Enum.GetValues<Cuisine>())
        {
            var addOpt = new CuisineOption(cuisine);
            addOpt.Toggled += _ => UpdateCanSaveNew();
            AddCuisineOptions.Add(addOpt);

            var editOpt = new CuisineOption(cuisine);
            EditCuisineOptions.Add(editOpt);

            FilterCuisineOptions.Add(new CuisineOption(cuisine));
        }

        foreach (var cost in Enum.GetValues<Cost>())
        {
            var addOpt = new CostOption(cost);
            addOpt.Toggled += opt => OnSingleSelectCostToggled(opt, AddCostOptions, () => UpdateCanSaveNew());
            AddCostOptions.Add(addOpt);

            var editOpt = new CostOption(cost);
            editOpt.Toggled += opt => OnSingleSelectCostToggled(opt, EditCostOptions, null);
            EditCostOptions.Add(editOpt);

            FilterCostOptions.Add(new CostOption(cost));
        }
    }

    // Single-select logic for cost chips in Add/Edit sheets.
    // When an option is toggled on, all others are deselected.
    // When an option is toggled off while it was the only selection, re-select it (radio behavior).
    private static void OnSingleSelectCostToggled(
        CostOption toggled,
        IEnumerable<CostOption> allOptions,
        Action? afterAction)
    {
        var options = allOptions.ToList();
        if (toggled.IsSelected)
        {
            foreach (var other in options.Where(o => o != toggled))
                other.IsSelected = false;
        }
        else
        {
            // Don't allow deselecting the last selected (radio button behavior)
            if (!options.Any(o => o.IsSelected))
                toggled.IsSelected = true;
        }
        afterAction?.Invoke();
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        _allRestaurants = await _repository.GetAllAsync();
        ApplyRestaurantFilter();
    }

    // ── Add sheet ─────────────────────────────────────────────────────────────

    [RelayCommand]
    public void OpenAddSheet()
    {
        ResetAddSheet();
        IsAddSheetVisible = true;
    }

    [RelayCommand]
    public void CloseAddSheet()
    {
        IsAddSheetVisible = false;
        ResetAddSheet();
    }

    [RelayCommand(CanExecute = nameof(CanSaveNew))]
    public async Task SaveNewRestaurantAsync()
    {
        var cuisines = AddCuisineOptions.Where(o => o.IsSelected).Select(o => o.Cuisine).ToList();
        var cost = AddCostOptions.First(o => o.IsSelected).Cost;
        var restaurant = new Restaurant(NewName.Trim(), cuisines, cost);

        await _repository.SaveAsync(restaurant);
        _allRestaurants.Add(restaurant);
        ApplyRestaurantFilter();
        CloseAddSheet();
    }

    private bool CanSaveNew() =>
        !string.IsNullOrWhiteSpace(NewName) &&
        AddCuisineOptions.Any(o => o.IsSelected) &&
        AddCostOptions.Any(o => o.IsSelected);

    private void UpdateCanSaveNew() => SaveNewRestaurantCommand.NotifyCanExecuteChanged();

    private void ResetAddSheet()
    {
        NewName = string.Empty;
        foreach (var o in AddCuisineOptions) o.IsSelected = false;
        foreach (var o in AddCostOptions) o.IsSelected = false;
    }

    // ── Edit sheet ────────────────────────────────────────────────────────────

    public void OpenEditSheet(Restaurant restaurant)
    {
        _editingRestaurant = restaurant;
        EditName = restaurant.Name;

        foreach (var o in EditCuisineOptions)
            o.IsSelected = restaurant.Cuisines.Contains(o.Cuisine);
        foreach (var o in EditCostOptions)
            o.IsSelected = o.Cost == restaurant.Cost;

        IsEditSheetVisible = true;
    }

    [RelayCommand]
    public void CloseEditSheet()
    {
        IsEditSheetVisible = false;
        _editingRestaurant = null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveEdit))]
    public async Task SaveEditAsync()
    {
        if (_editingRestaurant is null) return;

        _editingRestaurant.Name = EditName.Trim();
        _editingRestaurant.Cuisines = EditCuisineOptions.Where(o => o.IsSelected).Select(o => o.Cuisine).ToList();
        _editingRestaurant.Cost = EditCostOptions.First(o => o.IsSelected).Cost;
        _editingRestaurant.LastModified = DateTime.UtcNow;

        await _repository.SaveAsync(_editingRestaurant);
        ApplyRestaurantFilter();
        CloseEditSheet();
    }

    private bool CanSaveEdit() =>
        !string.IsNullOrWhiteSpace(EditName) &&
        EditCuisineOptions.Any(o => o.IsSelected) &&
        EditCostOptions.Any(o => o.IsSelected);

    // ── Filter sheet ──────────────────────────────────────────────────────────

    [RelayCommand]
    public void OpenFilterSheet()
    {
        foreach (var o in FilterCuisineOptions)
            o.IsSelected = _filterCuisines.Contains(o.Cuisine);
        foreach (var o in FilterCostOptions)
            o.IsSelected = _filterCosts.Contains(o.Cost);

        IsFilterSheetVisible = true;
    }

    [RelayCommand]
    public void ApplyFilter()
    {
        _filterCuisines.Clear();
        foreach (var o in FilterCuisineOptions.Where(o => o.IsSelected))
            _filterCuisines.Add(o.Cuisine);

        _filterCosts.Clear();
        foreach (var o in FilterCostOptions.Where(o => o.IsSelected))
            _filterCosts.Add(o.Cost);

        ApplyRestaurantFilter();
        IsFilterSheetVisible = false;
    }

    [RelayCommand]
    public void ClearFilter()
    {
        foreach (var o in FilterCuisineOptions) o.IsSelected = false;
        foreach (var o in FilterCostOptions) o.IsSelected = false;
        ApplyFilter();
    }

    [RelayCommand]
    public void CloseFilterSheet() => IsFilterSheetVisible = false;

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteRestaurantAsync(Restaurant restaurant)
    {
        await _repository.DeleteAsync(restaurant.Id);
        _allRestaurants.Remove(restaurant);
        ApplyRestaurantFilter();
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private void ApplyRestaurantFilter()
    {
        VisibleRestaurants = _allRestaurants
            .Where(r =>
                (!_filterCuisines.Any() || r.Cuisines.Any(c => _filterCuisines.Contains(c))) &&
                (!_filterCosts.Any() || _filterCosts.Contains(r.Cost)))
            .ToList();

        IsFiltered = _filterCuisines.Any() || _filterCosts.Any();
        RebuildDisplayItems();
    }

    private void RebuildDisplayItems()
    {
        var items = new List<RestaurantDisplayItem>(VisibleRestaurants.Count * RepeatCount);
        for (int i = 0; i < RepeatCount; i++)
            foreach (var r in VisibleRestaurants)
                items.Add(new RestaurantDisplayItem(r));

        DisplayItems = items;
        IsEmpty = VisibleRestaurants.Count == 0;

        OnPropertyChanged(nameof(DisplayItems));
        DisplayItemsChanged?.Invoke();
    }
}
