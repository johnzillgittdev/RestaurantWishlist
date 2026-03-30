using System.ComponentModel;
using RestaurantWishlist.Helpers;
using RestaurantWishlist.ViewModels;

namespace RestaurantWishlist.Views;

public partial class MainPage : ContentPage
{
    private const double ItemHeight = 100.0;

    private readonly MainViewModel _viewModel;
    private int _currentCenteredIndex;
    private RestaurantDisplayItem? _currentSelectedItem;
    private bool _syncingSelection;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.DisplayItemsChanged += OnDisplayItemsChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }

    // ── Wheel mechanics ───────────────────────────────────────────────────────

    private void OnDisplayItemsChanged()
    {
        if (_viewModel.DisplayItems.Count == 0) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(80); // allow CollectionView to update its layout
            ScrollToMiddle(animate: false);
        });
    }

    private void ScrollToMiddle(bool animate = false)
    {
        if (_viewModel.DisplayItems.Count == 0) return;
        var midIndex = _viewModel.DisplayItems.Count / 2;
        WheelView.ScrollTo(midIndex, position: ScrollToPosition.Center, animate: animate);
        SelectItem(midIndex);
    }

    private void SelectItem(int index)
    {
        var items = _viewModel.DisplayItems;
        if (items.Count == 0) return;

        index = Math.Clamp(index, 0, items.Count - 1);
        var item = items[index];
        if (item == _currentSelectedItem) return;

        if (_currentSelectedItem is not null)
            _currentSelectedItem.IsSelected = false;

        item.IsSelected = true;
        _currentSelectedItem = item;
        _currentCenteredIndex = index;

        _syncingSelection = true;
        WheelView.SelectedItem = item;
        _syncingSelection = false;
    }

    private void OnWheelScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (_syncingSelection || _viewModel.DisplayItems.Count == 0) return;

        var index = (int)Math.Round(e.VerticalOffset / ItemHeight);
        SelectItem(index);

        // Silently re-center when within 5 loops of either edge
        var total = _viewModel.DisplayItems.Count;
        var count = _viewModel.VisibleRestaurants.Count;
        if (count == 0) return;

        var buffer = count * 5;
        if (index < buffer || index > total - buffer)
        {
            var newIndex = count * 25 + (index % count);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                WheelView.ScrollTo(newIndex, position: ScrollToPosition.Center, animate: false);
                SelectItem(newIndex);
            });
        }
    }

    private async void OnSpinTapped(object? sender, TappedEventArgs e)
    {
        if (_viewModel.IsSpinning || _viewModel.DisplayItems.Count == 0) return;

        _viewModel.IsSpinning = true;

        var count = _viewModel.VisibleRestaurants.Count;
        var loops = Random.Shared.Next(3, 6);
        var randomOffset = Random.Shared.Next(count);
        var targetIndex = Math.Min(
            _currentCenteredIndex + loops * count + randomOffset,
            _viewModel.DisplayItems.Count - 1);

        // Rotate the spin button icon, then scroll the wheel
        var spinTask = SpinButton.RotateToAsync(720, 800, Easing.CubicOut);
        WheelView.ScrollTo(targetIndex, position: ScrollToPosition.Center, animate: true);

        await spinTask;
        SpinButton.Rotation = 0;
        await Task.Delay(1200); // let the wheel finish settling

        _viewModel.IsSpinning = false;
    }

    // ── Wheel item actions ────────────────────────────────────────────────────

    private void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (_currentSelectedItem?.Restaurant is { } r)
            _viewModel.OpenEditSheet(r);
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (_currentSelectedItem?.Restaurant is { } r)
            await _viewModel.DeleteRestaurantAsync(r);
    }

    // ── Sheet animations ──────────────────────────────────────────────────────

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.IsAddSheetVisible):
                await AnimateSheet(AddSheet, _viewModel.IsAddSheetVisible, fromBottom: true);
                break;
            case nameof(MainViewModel.IsEditSheetVisible):
                await AnimateSheet(EditSheet, _viewModel.IsEditSheetVisible, fromBottom: true);
                break;
            case nameof(MainViewModel.IsFilterSheetVisible):
                await AnimateSheet(FilterSheet, _viewModel.IsFilterSheetVisible, fromBottom: false);
                break;
        }
    }

    private async Task AnimateSheet(View sheet, bool show, bool fromBottom)
    {
        var offScreen = fromBottom ? 800.0 : -800.0;

        if (show)
        {
            SheetBackdrop.IsVisible = true;
            sheet.TranslationY = offScreen;
            sheet.IsVisible = true;
            await sheet.TranslateToAsync(0, 0, 300, Easing.CubicOut);
        }
        else
        {
            await sheet.TranslateToAsync(0, offScreen, 250, Easing.CubicIn);
            sheet.IsVisible = false;

            if (!_viewModel.IsAddSheetVisible &&
                !_viewModel.IsEditSheetVisible &&
                !_viewModel.IsFilterSheetVisible)
            {
                SheetBackdrop.IsVisible = false;
            }
        }
    }

    // ── Backdrop / cancel ─────────────────────────────────────────────────────

    private void OnBackdropTapped(object? sender, TappedEventArgs e)
    {
        if (_viewModel.IsAddSheetVisible)    _viewModel.CloseAddSheet();
        if (_viewModel.IsEditSheetVisible)   _viewModel.CloseEditSheetCommand.Execute(null);
        if (_viewModel.IsFilterSheetVisible) _viewModel.CloseFilterSheet();
    }

    private void OnCancelAddClicked(object? sender, EventArgs e) => _viewModel.CloseAddSheet();
}
