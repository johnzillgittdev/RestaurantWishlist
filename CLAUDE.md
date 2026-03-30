# RestaurantWishlist — Project Context

## Overview

A **.NET MAUI** mobile app for managing a restaurant wishlist with a spin wheel for random restaurant selection. Data is stored locally on-device via SQLite; server sync and multi-user shared lists are planned for a future phase.

**Project location:** `RestaurantWishlist/`

---

## Features

- Maintain a list of restaurants with name, cuisine(s), and price range
- Spin wheel UI for randomly selecting a restaurant
- Filter by cuisine (multi-select, OR logic) and cost (multi-select), with AND logic between filters
- Add, edit, and delete restaurants
- Data persisted locally in SQLite

---

## Architecture

```
Views/MainPage           ← full-screen wheel UI + inline sheet overlays
ViewModels/MainViewModel ← all data + state; raises events for scroll mechanics
Repository/IRestaurantRepository    ← interface (seam for future API swap)
Repository/SqliteRestaurantRepository ← sqlite-net-pcl implementation
Database/DatabaseService ← SQLite connection + table setup
Models/Restaurant        ← core model
Helpers/RestaurantDisplayItem ← wrapper for CollectionView (unique per repeat)
Helpers/CuisineOption / CostOption ← chip state with ToggleCommand
Converters/              ← CuisinesListToString, CostToRangeString
Enums/Cuisine, Cost      ← 23 cuisine types, 4 cost levels
```

---

## Key Implementation Decisions

- **Storage:** sqlite-net-pcl; `List<Cuisine>` stored as comma-separated string (`CuisinesData`); database file is `wishlist.db3` in `FileSystem.AppDataDirectory`
- **Spin wheel:** `CollectionView` + `LinearItemsLayout` with `SnapPointsAlignment="Center"`; items repeated 50× (RepeatCount = 50) for infinite-scroll illusion; centered item tracked via `Scrolled` event using `index = offset / ItemHeight` where `ItemHeight = 100`
- **Silent re-centering:** When scrolled within 5 loops of the edge, wheel auto-re-centers without animation
- **Selected item:** `CollectionView.SelectedItem` drives VSM scale (1.35) / opacity (1.0 selected, 0.3 others); `RestaurantDisplayItem.IsSelected` drives edit/delete button visibility
- **Spin animation:** Button rotates 720° (CubicOut, 800ms); wheel scrolls 3–6 random loops + random offset; `IsSpinning` flag prevents duplicate spins; 1200ms settling delay
- **Sheets:** Three slide-in overlays (Add, Edit, Filter) within MainPage using `TranslateToAsync` (300ms show / 250ms hide); visibility driven by VM properties observed in code-behind; semi-transparent backdrop tappable to dismiss
- **Sheet chips:** `CuisineOption` multi-select (OR logic); `CostOption` single-select radio behavior; both use `ToggleCommand`
- **MVVM:** `CommunityToolkit.Mvvm` source generators (`ObservableProperty`, `RelayCommand`); `DisplayItemsChanged` event from VM triggers View re-center logic

---

## Data Model

```csharp
Restaurant {
    Id            Guid      // primary key
    Name          string
    CuisinesData  string    // comma-separated enum names (sqlite column)
    Cuisines      List<Cuisine>  // computed property
    Cost          Cost
    LastModified  DateTime
}
```

---

## Color Scheme

| Role | Hex |
|------|-----|
| Primary | `#9B89C4` (purple) |
| Secondary | `#E8A87C` (orange) |
| Tertiary | `#C4A96B` (gold) |
| Background | `#FAF7FF` (light purple) |
| Text | `#2D2040` (dark purple) |
| Divider | `#D8D0E8` |
| Chip | `#4A3F5C` (bg) / `#FAF7FF` (text) |
| Danger | `#E07B7B` |
| Teal | `#87B5A2` |

---

## Packages

| Package | Version | Role |
|---------|---------|------|
| `CommunityToolkit.Maui` | 14.0.1 | MAUI community toolkit |
| `CommunityToolkit.Mvvm` | 8.4.2 | MVVM source generators |
| `Microsoft.Maui.Controls` | 10.0.41 | MAUI framework |
| `sqlite-net-pcl` | 1.9.172 | SQLite ORM |
| `SQLitePCLRaw.bundle_green` | 2.1.11 | SQLite native bindings |
| `Microsoft.Extensions.Logging.Debug` | 10.0.0 | Debug logging |

**Target framework:** .NET 10 (Android, iOS)

---

## Future Phase (not in scope yet)

- ASP.NET Core Web API backend
- Server-side database (PostgreSQL + EF Core)
- User accounts and authentication
- Shared lists (multiple users contributing to one list)
- Offline-first sync between local SQLite and server

---

## Developer Notes

- Ask before making architectural decisions not covered here
- Do not start or stop any dev server — the developer runs that independently
