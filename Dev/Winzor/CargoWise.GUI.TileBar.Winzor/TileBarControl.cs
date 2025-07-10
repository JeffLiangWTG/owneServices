using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.GUI.TileBar.JSInterop;
using CargoWise.Main.Navigation;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.ZArchitecture.Core;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace CargoWise.GUI.TileBar;

public partial class TileBarControl : UserControl, IWinzorSupportedWPFContent
{
	public TileBarControl()
	{
		AutomaticallyReRenderOnEventCallbacks = true;
		InitializeComponent();
	}

	public override bool UseParentDivForLayout => false;

	public void InitializeComponent()
	{
		if (DataContext != null
			&& DataContext.Categories != null
			&& DataContext.Categories.Count > 0)
		{
			DataContext.SelectedCategory = DataContext.Categories[0];
			DataContext.InitializeSearch(globalSearch: null);
		}
	}

	public NavigationViewModel DataContext {
		get => dataContext;
		set
		{
			if (DataContext is not null)
			{
				dataContext.SearchViewModel.PropertyChanged -= SearchValueChanged;
				dataContext.SearchViewModel.PropertyChanged -= PropertyChanged;
				dataContext.SearchViewModel.SearchResults.CollectionChanged -= SearchResultSection_CollectionChanged;
				dataContext.PropertyChanged -= PropertyChanged;
				dataContext.Categories.CollectionChanged -= CategoriesCollectionChanged;
				UnhookEventsForCategories(DataContext.Categories);
			}
			dataContext = value;
			if (DataContext is not null)
			{
				dataContext.SearchViewModel.PropertyChanged += SearchValueChanged;
				dataContext.SearchViewModel.PropertyChanged += PropertyChanged;
				dataContext.SearchViewModel.SearchResults.CollectionChanged += SearchResultSection_CollectionChanged;
				dataContext.PropertyChanged += PropertyChanged;
				dataContext.Categories.CollectionChanged += CategoriesCollectionChanged;
				HookEventsForCategories(DataContext.Categories);
			}
		}
	}
	NavigationViewModel dataContext;

	void SearchValueChanged(object sender, PropertyChangedEventArgs e)
	{
		if (nameof(SearchViewModel.SearchValue).Equals(e.PropertyName))
		{
			inputSearchValue = DataContext?.SearchViewModel.SearchValue ?? string.Empty;
		}
	}

	void CategoriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		UnhookEventsForCategories(DataContext.Categories);
		HookEventsForCategories(DataContext.Categories);
		NotifyRenderRequired();
	}

	void UnhookEventsForCategories(IEnumerable<NavigationMenuViewModel> categories)
	{
		foreach (var category in categories ?? Enumerable.Empty<NavigationMenuViewModel>())
		{
			category.Buttons.CollectionChanged -= CategoriesCollectionChanged;
			UnhookSubSectionEventsRecursively(category.Buttons.OfType<MenuSection>());
		}
	}

	void HookEventsForCategories(IEnumerable<NavigationMenuViewModel> categories)
	{
		foreach (var category in categories ?? Enumerable.Empty<NavigationMenuViewModel>())
		{
			category.Buttons.CollectionChanged += CategoriesCollectionChanged;
			HookSubSectionEventsRecursively(category.Buttons.OfType<MenuSection>());
		}
	}

	void UnhookSubSectionEventsRecursively(IEnumerable<MenuSection> menuSections)
	{
		foreach (var section in menuSections ?? Enumerable.Empty<MenuSection>())
		{
			section.Items.CollectionChanged -= CollectionChanged;
			foreach (var item in section.Items)
			{
				item.PropertyChanged -= PropertyChanged;
			}
			section.Subsections.CollectionChanged -= CollectionChanged;
			UnhookSubSectionEventsRecursively(section.Subsections.OfType<MenuSection>());
		}
	}

	void HookSubSectionEventsRecursively(IEnumerable<MenuSection> menuSections)
	{
		foreach (var section in menuSections ?? Enumerable.Empty<MenuSection>())
		{
			section.Items.CollectionChanged += CollectionChanged;
			foreach (var item in section.Items)
			{
				item.PropertyChanged += PropertyChanged;
			}
			section.Subsections.CollectionChanged += CollectionChanged;
			HookSubSectionEventsRecursively(section.Subsections.OfType<MenuSection>());
		}
	}

	void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		UnhookEventsForCategories(DataContext.Categories);
		HookEventsForCategories(DataContext.Categories);
		NotifyRenderRequired();
	}

	void SearchResultSection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		var oldSections = args.OldItems?.OfType<SearchResultSection>() ?? Enumerable.Empty<SearchResultSection>();
		UnhookEventsForSections(oldSections);

		var newSections = args.NewItems?.OfType<SearchResultSection>() ?? Enumerable.Empty<SearchResultSection>();
		HookEventsForSections(newSections);

		NotifyRenderRequired();

		void UnhookEventsForSections(IEnumerable<SearchResultSection> sections)
		{
			foreach (var section in sections)
			{
				section.Items.CollectionChanged -= SearchResultSectionItems_CollectionChanged;
				UnhookEventsForMenuItems(section.Items);
			}
		}

		void HookEventsForSections(IEnumerable<SearchResultSection> sections)
		{
			foreach (var section in sections)
			{
				section.Items.CollectionChanged += SearchResultSectionItems_CollectionChanged;
				HookEventsForMenuItems(section.Items);
			}
		}
	}

	void SearchResultSectionItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		var oldSections = args.OldItems?.OfType<Main.Navigation.MenuItem>() ?? Enumerable.Empty<Main.Navigation.MenuItem>();
		UnhookEventsForMenuItems(oldSections);

		var newSections = args.NewItems?.OfType<Main.Navigation.MenuItem>() ?? Enumerable.Empty<Main.Navigation.MenuItem>();
		HookEventsForMenuItems(newSections);

		NotifyRenderRequired();
	}

	void UnhookEventsForMenuItems(IEnumerable<Main.Navigation.MenuItem> items)
	{
		foreach (var item in items)
		{
			item.PropertyChanged -= PropertyChanged;
		}
	}

	void HookEventsForMenuItems(IEnumerable<Main.Navigation.MenuItem> items)
	{
		foreach (var item in items)
		{
			item.PropertyChanged += PropertyChanged;
		}
	}

	void PropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		NotifyRenderRequired();
	}

	async Task HandleDragEndAsync(WinzorDragEndEventArgs args, MenuSection menuSection)
	{
		var dropElementRect = await (GetJSInterop<ITileBarJSInterop>()?.GetBoundingBoxForFavoriteItemAsync(dropIndex + 1) ?? Task.FromResult<float[]>(null));
		if (dropElementRect is null)
		{
			return;
		}

		var dropMouseY = args.ClientY;
		menuSection.Drop(selectedItem, dropIndex, IsPositionAboveElement(dropMouseY, dropElementRect));
		await (GetJSInterop<ITileBarJSInterop>()?.ResetDividerVisibilityAsync() ?? Task.CompletedTask);
	}

	async Task HandleDragOverAsync(WebDragEventArgs args)
	{
		var dropElementRect = await (GetJSInterop<ITileBarJSInterop>()?.GetBoundingBoxForFavoriteItemAsync(dropIndex + 1) ?? Task.FromResult<float[]>(null));
		if (dropElementRect is null)
		{
			return;
		}

		var dropMouseY = args.ClientY;

		if (IsPositionAboveElement(dropMouseY, dropElementRect))
		{
			await (GetJSInterop<ITileBarJSInterop>()?.SetDividerVisibilityAsync(dropIndex) ?? Task.CompletedTask);
		}
		else
		{
			await (GetJSInterop<ITileBarJSInterop>()?.SetDividerVisibilityAsync(dropIndex + 1) ?? Task.CompletedTask);
		}
	}

	internal static bool IsPositionAboveElement(double dropMouseY, float[] dropElementRect)
	{
		return dropMouseY < dropElementRect[1] + (dropElementRect[2] / 2);
	}

	public Color TileBackgroundColor
	{
		get => tileBackgroundColor;
		set => UpdateProperty(ref tileBackgroundColor, value);
	}
	Color tileBackgroundColor = Color.Empty;

	public Color TileTextColor
	{
		get => tileTextColor;
		set => UpdateProperty(ref tileTextColor, value);
	}
	Color tileTextColor = Color.Empty;

	public Color TileSelectedColor
	{
		get => tileSelectedColor;
		set => UpdateProperty(ref tileSelectedColor, value);
	}
	Color tileSelectedColor = Color.Empty;

	public Color GroupBackgroundColor
	{
		get => groupBackgroundColor;
		set => UpdateProperty(ref groupBackgroundColor, value);
	}
	Color groupBackgroundColor = Color.Empty;

	public Color GroupHeaderBackgroundColor
	{
		get => groupHeaderBackgroundColor;
		set => UpdateProperty(ref groupHeaderBackgroundColor, value);
	}
	Color groupHeaderBackgroundColor = Color.Empty;

	public Color RecentPanelBackgroundColor
	{
		get => recentPanelBackgroundColor;
		set => UpdateProperty(ref recentPanelBackgroundColor, value);
	}
	Color recentPanelBackgroundColor = Color.Empty;

	public IMenuDisplayer MenuDisplayer => FindForm()?.CargoWiseClientServices?.MenuDisplayer;

	async Task TileBarInputContextMenu(WebMouseEventArgs args)
	{
		await MenuDisplayer.ShowClipboardContextMenuAsync(args, GetJSInterop<IClipboardJSInterop>(), SearchInput);
	}

	public void SetCaretPosition(int index)
	{
	}

	public int GetCaretPosition() => 0;

	string TileBarControlStyleString => $"--tile-background-color: {TileBackgroundColor.GetColorStyleValue()};--tile-text-color: {TileTextColor.GetColorStyleValue()};--tile-selected-color: {TileSelectedColor.GetColorStyleValue()};--group-background-color: {GroupBackgroundColor.GetColorStyleValue()};--group-header-background-color: {GroupHeaderBackgroundColor.GetColorStyleValue()};--recent-panel-background-color: {RecentPanelBackgroundColor.GetColorStyleValue()};{(DataContext?.IsInSearchMode ?? false ? (NoResString)"background-color: var(--group-header-background-color)" : string.Empty)}";
}
