using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Entity = CargoWise.NetworkVisualisation.Business.Entity;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RibbonTest : BunitTestContext
{
	[Test]
	public void RibbonDoesNotRenderWithoutViewModel()
	{
		var cut = RenderComponent<Ribbon>();

		Assert.That(cut.Markup, Is.Empty);

		var viewModel = new RibbonViewModel();
		cut.SetParametersAndRender(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		Assert.That(cut.Find(".ribbon"), Is.Not.Null);
	}

	[Test]
	public void RibbonRendersEmptyTitleBar()
	{
		var viewModel = new RibbonViewModel();
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var titleBar = cut.Find(".ribbon__titlebar");
		Assert.That(titleBar.ChildElementCount, Is.EqualTo(0));
	}

	[Test]
	public void RibbonRendersHeaderButtonForEachTab()
	{
		var viewModel = new RibbonViewModel();
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 1")));
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 2")));
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var header = cut.Find(".ribbon__headertabs");
		Assert.That(header.ChildElementCount, Is.EqualTo(2));

		var headerButtons = cut.FindAll(".ribbon__headerbutton");
		Assert.That(headerButtons.Count, Is.EqualTo(2));
		Assert.That(headerButtons[0].TextContent, Is.EqualTo("Tab 1"));
		Assert.That(headerButtons[1].TextContent, Is.EqualTo("Tab 2"));
	}

	[Test]
	public async Task RibbonHeaderButtonsChangeSelectedTabAsync()
	{
		var viewModel = new RibbonViewModel();
		var tabViewModel1 = new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 1"));
		tabViewModel1.Groups.Add(new RibbonGroupViewModel(viewModel, ResString.GetMultilingualString(string.Empty, "Tab 1 Content"), string.Empty));
		viewModel.Tabs.Add(tabViewModel1);
		var tabViewModel2 = new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 2"));
		tabViewModel2.Groups.Add(new RibbonGroupViewModel(viewModel, ResString.GetMultilingualString(string.Empty, "Tab 2 Content"), string.Empty));
		viewModel.Tabs.Add(tabViewModel2);
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 1"));
		var selectedTabContent = cut.Find(".ribbon__content");
		Assert.That(selectedTabContent.TextContent.Trim(), Is.EqualTo("Tab 1 Content"));

		var headerButtons = cut.FindAll(".ribbon__headerbutton");
		await headerButtons[1].ClickAsync(new WebMouseEventArgs());

		selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 2"));
		selectedTabContent = cut.Find(".ribbon__content");
		Assert.That(selectedTabContent.TextContent.Trim(), Is.EqualTo("Tab 2 Content"));
	}

	[Test]
	public void RibbonRendersSearchForm()
	{
		var viewModel = new RibbonViewModel();
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var searchBox = cut.Find(".ribbon__searchbox");
		Assert.That(searchBox, Is.Not.Null);
	}

	[Test]
	public async Task RibbonRendersSearchFormUpdateSearchTermAsync()
	{
		var viewModel = new RibbonViewModel();
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var searchBox = cut.Find(".ribbon__searchbox");
		Assert.That(searchBox, Is.Not.Null);

		searchBox.InnerHtml = "Test";
		await searchBox.KeyUpAsync(new WebKeyboardEventArgs { Key = "Test" });
		Assert.That(searchBox.InnerHtml, Is.Empty);
	}

	[Test]
	public async Task RibbonRendersSearchFormPerformSearchAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var viewModel = new RibbonViewModel();
		var cut = RenderComponent<Ribbon>(parameters => parameters.Add(p => p.ViewModel, viewModel));

		NetworkUserControl? networkUserControl = null;
		var network = new DummyNetwork();
		var ribbon = new Ribbon();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		network.Entities.Add(entity1);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			networkUserControl = new NetworkUserControl(new Entity(), new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			ribbon.SearchFinder = new SearchFinderViewModel(networkUserControl?.NetworkViewModel);
		});

		Assert.That(networkUserControl, Is.Not.Null);
		Assert.That(ribbon.SearchFinder!.HasPerformedSearch, Is.False);
		var searchBox = cut.Find(".ribbon__searchbox");
		Assert.That(searchBox, Is.Not.Null);

		searchBox.InnerHtml = "job1";
		await networkUserControl.InvokeWinzorDispatcherAsync(() => ribbon.SearchFinder.PerformSearch_ForTest(searchBox.InnerHtml));
		Assert.That(ribbon.SearchFinder.HasPerformedSearch, Is.True);
	}

	[Test]
	public void FocusOnSearchBoxTriggersJSFocus()
	{
		var viewModel = new RibbonViewModel();

		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		cut.Instance.FocusOnSearchBox();
		var invocations = base.JSInterop.Invocations.Where(i => i.Identifier == "Blazor._internal.domWrapper.focus");

		Assert.That(invocations.Count, Is.EqualTo(1));
		Assert.That(invocations.First().Arguments.First(), Is.EqualTo(cut.Instance.SearchBoxElementReference));
	}

	[Test]
	public async Task RibbonRetainsSelectedTabWhenChangingViewModelAsync()
	{
		var viewModel = new RibbonViewModel();
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 1")));
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 2")));
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		await cut.FindAll(".ribbon__headerbutton").Last().ClickAsync(new WebMouseEventArgs());
		Assert.That(cut.Find(".ribbon__headerbutton--selected").TextContent, Is.EqualTo("Tab 2"));

		var newViewModel = new RibbonViewModel();
		newViewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "New Tab 1")));
		newViewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "New Tab 2")));
		cut.SetParametersAndRender(parameters => parameters
			.Add(p => p.ViewModel, newViewModel)
		);

		Assert.That(cut.Find(".ribbon__headerbutton--selected").TextContent, Is.EqualTo("New Tab 2"));
	}

	[Test]
	public async Task RibbonRightClickOpensContextMenuAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var menuDisplayerMock = new Mock<IMenuDisplayer>();

		Services.AddSingleton(menuDisplayerMock.Object);
		var viewModel = new RibbonViewModel();

		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var header = cut.Find(".ribbon");
		await header.ContextMenuAsync(new WebMouseEventArgs());

		menuDisplayerMock.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayerMock.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems, Is.Not.Empty);
		Assert.That(menuInteropModel!.MenuItems.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task RibbonRightClickOpensContextMenuToggleRibbonStyleAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var viewModel = new RibbonViewModel();
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));
		cut.Instance.ToggleRibbonView();
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:none;"));
		cut.Instance.ToggleRibbonView();
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));
	}

	[Test]
	public void RibbonTabDoubleClickTogglesRibbonStyle()
	{
		var viewModel = new RibbonViewModel();
		var tabViewModel1 = new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 1"));
		viewModel.Tabs.Add(tabViewModel1);
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 1"));
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));

		cut.Instance.OnRibbonTabClick(0, new WebMouseEventArgs { Detail = 1 });
		cut.Instance.OnRibbonTabClick(0, new WebMouseEventArgs { Detail = 2 });
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:none;"));

		cut.Instance.OnRibbonTabClick(0, new WebMouseEventArgs { Detail = 1 });
		cut.Instance.OnRibbonTabClick(0, new WebMouseEventArgs { Detail = 2 });
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));
	}

	[Test]
	public async Task RibbonTabDoubleClickOnUnselectedTabMaximizesRibbonAsync()
	{
		var viewModel = new RibbonViewModel();
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 1")));
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 2")));
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 1"));
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));

		cut.Instance.OnRibbonTabClick(0, new WebMouseEventArgs { Detail = 1 });
		cut.Instance.OnRibbonTabClick(0, new WebMouseEventArgs { Detail = 2 });
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:none;"));

		var headerButtons = cut.FindAll(".ribbon__headerbutton");
		Assert.That(headerButtons[1].TextContent, Is.EqualTo("Tab 2"));
		await headerButtons[1].ClickAsync(new WebMouseEventArgs());
		cut.Instance.OnRibbonTabClick(1, new WebMouseEventArgs { Detail = 1 });
		cut.Instance.OnRibbonTabClick(1, new WebMouseEventArgs { Detail = 2 });

		selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 2"));
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));
	}

	[Test]
	public async Task RibbonTabDoubleClickAfterTabChangeTogglesRibbonStyleAsync()
	{
		var viewModel = new RibbonViewModel();
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 1")));
		viewModel.Tabs.Add(new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab 2")));
		var cut = RenderComponent<Ribbon>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 1"));
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));

		var headerButtons = cut.FindAll(".ribbon__headerbutton");
		Assert.That(headerButtons[1].TextContent, Is.EqualTo("Tab 2"));
		await headerButtons[1].ClickAsync(new WebMouseEventArgs());

		selectedHeaderButtons = cut.FindAll(".ribbon__headerbutton--selected");
		Assert.That(selectedHeaderButtons.Count, Is.EqualTo(1));
		Assert.That(selectedHeaderButtons[0].TextContent, Is.EqualTo("Tab 2"));
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));

		cut.Instance.OnRibbonTabClick(1, new WebMouseEventArgs { Detail = 1 });
		cut.Instance.OnRibbonTabClick(1, new WebMouseEventArgs { Detail = 2 });
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:none;"));

		cut.Instance.OnRibbonTabClick(1, new WebMouseEventArgs { Detail = 1 });
		cut.Instance.OnRibbonTabClick(1, new WebMouseEventArgs { Detail = 2 });
		Assert.That(cut.Instance.ToggleRibbonStyle, Is.EqualTo("display:inline;"));
	}

	[Test]
	public async Task RibbonUpdateSearchTermNotInvokeWhenIsSearchingAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var viewModel = new RibbonViewModel();

		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };

		NetworkUserControl? networkUserControl = null;
		network.Entities.Add(entity1);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			networkUserControl = new NetworkUserControl(new Entity(), new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
		});

		var cut = RenderComponent<RibbonForTest>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
			.Add(p => p.NetworkUserControl, networkUserControl)
		);

		var searchBox = cut.Find(".ribbon__searchbox");
		Assert.That(searchBox, Is.Not.Null);

		cut.Instance.SetIsPerformingSearch(true);
		await searchBox.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "job1" });
		await searchBox.KeyUpAsync(new WebKeyboardEventArgs { Key = "Enter" });
		Assert.That(cut.Instance.SearchFinder, Is.Null);
	}

	class RibbonForTest : Ribbon
	{
		internal void SetIsPerformingSearch(bool value) => isPerformingSearch = value;
	}
}
