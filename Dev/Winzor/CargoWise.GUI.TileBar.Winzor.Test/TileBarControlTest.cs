using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.GUI.TileBar.JSInterop;
using CargoWise.Main.Navigation;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using MenuItem = CargoWise.Main.Navigation.MenuItem;

namespace CargoWise.GUI.TileBar.Winzor.Test;
internal class TileBarControlTest
{
	[Test]
	public async Task TileBarControlRendersCategoriesAsTabsAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, _, _, _) = await RenderTileBarControlAsync(ctx);

		var tabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem");

		Assert.That(tabs.Count, Is.EqualTo(2));
		Assert.That(tabs[0].TextContent, Is.EqualTo("Category One"));
		Assert.That(tabs[1].TextContent, Is.EqualTo("Category Two"));
	}

	[Test]
	public async Task TileBarControlReRendersWhenCategoryAddedAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, _, _) = await RenderTileBarControlAsync(ctx);

		var tabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem");
		Assert.That(tabs.Count, Is.EqualTo(2));

		await tileBar.InvokeWinzorDispatcherAsync(() => AddCategory(tileBar.DataContext, "Three"));
		tabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem");
		Assert.That(tabs.Count, Is.EqualTo(3));
		Assert.That(tabs[2].TextContent, Is.EqualTo("Category Three"));
	}

	[Test]
	public async Task TileBarControlFirstTabSelectedOnRenderAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, _, _, _) = await RenderTileBarControlAsync(ctx);

		var selectedTabs = rendered.WaitForElements(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem--selected");
		Assert.That(selectedTabs.Count, Is.EqualTo(1));
		Assert.That(selectedTabs[0].TextContent, Is.EqualTo("Category One"));
	}

	[Test]
	public async Task TileBarControlSelectTabAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, _, _, _) = await RenderTileBarControlAsync(ctx);

		var tabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem");
		Assert.That(tabs.Count, Is.EqualTo(2));

		var selectedTabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem--selected");
		Assert.That(selectedTabs.Count, Is.EqualTo(1));
		Assert.That(selectedTabs[0].TextContent, Is.EqualTo("Category One"));

		await rendered.Find(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem:nth-child(2)").ClickAsync(new WebMouseEventArgs());
		selectedTabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem--selected");
		Assert.That(selectedTabs.Count, Is.EqualTo(1));
		Assert.That(selectedTabs[0].TextContent, Is.EqualTo("Category Two"));
	}

	[Test]
	public async Task TileBarControlRendersActiveTabContentAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() => AddSubcategory(tab1, "A"));
		await tileBar.InvokeWinzorDispatcherAsync(() => AddSubcategory(tab2, "B"));

		Assert.That(rendered.FindAll(".tilebarcontrol__subcategory").Count, Is.EqualTo(1));
		Assert.That(rendered.Find(".tilebarcontrol__subcategory > .tilebarcontrol__subcategoryletter").TextContent, Is.EqualTo("A"));

		await rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem")[1].ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".tilebarcontrol__subcategory").Count, Is.EqualTo(1));
		Assert.That(rendered.Find(".tilebarcontrol__subcategory > .tilebarcontrol__subcategoryletter").TextContent, Is.EqualTo("B"));
	}

	[Test]
	public async Task TileBarControlSubCategoryDisplaysLetterAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() => AddSubcategory(tab1, "A"));

		var subCategories = rendered.FindAll(".tilebarcontrol__subcategory");
		Assert.That(subCategories.Count, Is.EqualTo(1));

		var subCategoryLetter = rendered.Find(".tilebarcontrol__subcategory > .tilebarcontrol__subcategoryletter");
		Assert.That(subCategoryLetter.TextContent, Is.EqualTo("A"));
	}

	[Test]
	public async Task TileBarControlSubCategoryDisplaysNameAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() => AddSubcategory(tab1, "A"));

		var subCategories = rendered.FindAll(".tilebarcontrol__subcategory");
		Assert.That(subCategories.Count, Is.EqualTo(1));

		var subCategoryText = rendered.Find(".tilebarcontrol__subcategory > .tilebarcontrol__subcategorytext");
		Assert.That(subCategoryText.TextContent, Is.EqualTo("Test SubCategory A"));
	}

	[Test]
	public async Task TileBarControlSubCategorySelectedOnClickAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() => AddSubcategory(tab1, "A"));

		var subCategories = rendered.FindAll(".tilebarcontrol__subcategory");
		Assert.That(subCategories.Count, Is.EqualTo(1));
		var selectedSubCategories = rendered.FindAll(".tilebarcontrol__subcategory--selected");
		Assert.That(selectedSubCategories.Count, Is.EqualTo(0));

		await subCategories[0].ClickAsync(new WebMouseEventArgs());
		selectedSubCategories = rendered.FindAll(".tilebarcontrol__subcategory--selected");
		Assert.That(selectedSubCategories.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TileBarControlSubCategoryDisplaysSubSectionsOnClickAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() => AddSubcategory(tab1, "A"));

		var subCategories = rendered.FindAll(".tilebarcontrol__subcategory");
		Assert.That(subCategories.Count, Is.EqualTo(1));
		var subCategoryItems = rendered.FindAll(".tilebarcontrol__subcategoryitems");
		Assert.That(subCategoryItems.Count, Is.EqualTo(0));

		await subCategories[0].ClickAsync(new WebMouseEventArgs());
		subCategoryItems = rendered.FindAll(".tilebarcontrol__subcategoryitems");
		Assert.That(subCategoryItems.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TileBarControlSubCategoryRendersItemsAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSection = AddSubSection(subCategory, "1");
			AddItem(subSection, "1a");
			AddItem(subSection, "1b");
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var items = rendered.FindAll(".tilebarcontrol__subcategorysectionitems > .tilebarcontrol__subcategorysectionitem");
		Assert.That(items.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task TileBarControlSubCategoryWithMultipleSectionsHasHeadersAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "1a");
			AddItem(subSectionOne, "1b");
			var subSectionTwo = AddSubSection(subCategory, "2");
			AddItem(subSectionTwo, "2a");
			AddItem(subSectionTwo, "2b");
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var sections = rendered.FindAll(".tilebarcontrol__subcategoryitems > .tilebarcontrol__subcategorysection");
		Assert.That(sections.Count, Is.EqualTo(2));

		var sectionOneHeader = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(1) > .tilebarcontrol__subcategorysectionheader");
		Assert.That(sectionOneHeader.Count, Is.EqualTo(1));
		Assert.That(sectionOneHeader[0].TextContent, Is.EqualTo("SubSection 1"));
		var sectionOneItems = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(1) .tilebarcontrol__subcategorysectionitem");
		Assert.That(sectionOneItems.Count, Is.EqualTo(2));
		Assert.That(sectionOneItems.Select(i => i.TextContent), Does.Contain("Menu Item 1a") & Does.Contain("Menu Item 1b"));

		var sectionTwoHeader = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(2) > .tilebarcontrol__subcategorysectionheader");
		Assert.That(sectionTwoHeader.Count, Is.EqualTo(1));
		Assert.That(sectionTwoHeader[0].TextContent, Is.EqualTo("SubSection 2"));
		var sectionTwoItems = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(2) .tilebarcontrol__subcategorysectionitem");
		Assert.That(sectionTwoItems.Count, Is.EqualTo(2));
		Assert.That(sectionTwoItems.Select(i => i.TextContent), Does.Contain("Menu Item 2a") & Does.Contain("Menu Item 2b"));
	}

	[Test]
	public async Task TileBarControlSubCategoryWithOneSectionsHasNoHeadersAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "1a");
			AddItem(subSectionOne, "1b");
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var sections = rendered.FindAll(".tilebarcontrol__subcategoryitems > .tilebarcontrol__subcategorysection");
		Assert.That(sections.Count, Is.EqualTo(1));

		var sectionHeader = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(1) > .tilebarcontrol__subcategorysectionheader");
		Assert.That(sectionHeader.Count, Is.EqualTo(0));
		var sectionItems = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(1) .tilebarcontrol__subcategorysectionitem");
		Assert.That(sectionItems.Count, Is.EqualTo(2));
		Assert.That(sectionItems.Select(i => i.TextContent), Does.Contain("Menu Item 1a") & Does.Contain("Menu Item 1b"));
	}

	[Test]
	public async Task TileBarControlSubCategoryItemHasCorrectTextAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "Test!");
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var sectionItems = rendered.FindAll(".tilebarcontrol__subcategorysectionitem > button");
		Assert.That(sectionItems.Count, Is.EqualTo(1));
		Assert.That(sectionItems[0].TextContent, Is.EqualTo("Menu Item Test!"));
	}

	[Test]
	public async Task TileBarControlSubCategoryItemHasCorrectTitleAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "Test!");
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var sectionItems = rendered.FindAll(".tilebarcontrol__subcategorysectionitem > button");
		Assert.That(sectionItems.Count, Is.EqualTo(1));
		Assert.That(sectionItems[0].GetAttribute("title"), Is.EqualTo("Menu Item Test!"));
	}

	[Test]
	public async Task TileBarControlSubCategoryItemClickEventAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		var clickEventFired = new TaskCompletionSource();
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "Test!", () => clickEventFired.SetResult());
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var sectionItems = rendered.FindAll(".tilebarcontrol__subcategorysectionitem > button");
		await sectionItems[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(await clickEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task TileBarControlSubCategoryItemRightClickEventAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		var rightClickEventFired = new TaskCompletionSource();
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subCategory = AddSubcategory(tab1, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "Test!", null, () => rightClickEventFired.SetResult());
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var sectionItems = rendered.FindAll(".tilebarcontrol__subcategorysectionitem > button");
		await sectionItems[0].ClickAsync(new WebMouseEventArgs() { Button = 2 });
		Assert.That(await rightClickEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionHeaderTextAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
		});

		var sectionHeader = rendered.Find(".tilebarcontrol__section > .tilebarcontrol__sectionheader");
		Assert.That(sectionHeader.TextContent, Is.EqualTo("Section One"));
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionHeaderContextMenuAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx, clientServices);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
		});

		var sectionHeader = rendered.Find(".tilebarcontrol__section > .tilebarcontrol__sectionheader");
		await sectionHeader.ClickAsync(new WebMouseEventArgs() { Button = 2 });

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems.Count, Is.EqualTo(1));
		Assert.That(menuInteropModel.MenuItems[0].Text, Is.EqualTo("Remove All Links"));
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemsAreRenderedAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A");
			AddItem(section, "B");
			AddItem(section, "C");
		});

		var sectionItems = rendered.FindAll(".tilebarcontrol__section .tilebarcontrol__sectionitem .tilebarcontrol__sectionitemlink");
		Assert.That(sectionItems.Count, Is.EqualTo(3));
		Assert.That(sectionItems[0].TextContent, Is.EqualTo("Menu Item A"));
		Assert.That(sectionItems[1].TextContent, Is.EqualTo("Menu Item B"));
		Assert.That(sectionItems[2].TextContent, Is.EqualTo("Menu Item C"));
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemTextAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A");
		});

		var itemLink = rendered.FindAll(".tilebarcontrol__sectionitem > button:nth-child(2)");
		Assert.That(itemLink.Count, Is.EqualTo(1));
		Assert.That(itemLink[0].TextContent, Is.EqualTo("Menu Item A"));
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemTitleAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A");
		});

		var itemLink = rendered.FindAll(".tilebarcontrol__sectionitem > button:nth-child(2)");
		Assert.That(itemLink.Count, Is.EqualTo(1));
		Assert.That(itemLink[0].GetAttribute("Title"), Is.EqualTo("Menu Item A"));
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemClickAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		var clickEventFired = new TaskCompletionSource();
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A", () => clickEventFired.SetResult());
		});

		await rendered.Find(".tilebarcontrol__sectionitem > button:nth-child(2)").ClickAsync(new WebMouseEventArgs());
		Assert.That(await clickEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemRightClickAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		var rightClickEventFired = new TaskCompletionSource();
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A", null, () => rightClickEventFired.SetResult());
		});

		await rendered.Find(".tilebarcontrol__sectionitem > button:nth-child(2)").ClickAsync(new WebMouseEventArgs() { Button = 2 });
		Assert.That(await rightClickEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemFavouriteStateAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A");
		});

		var favouriteButton = rendered.FindAll(".tilebarcontrol__sectionitem > button:nth-child(1)");
		Assert.That(favouriteButton.Count, Is.EqualTo(1));
		Assert.That(favouriteButton[0].ClassList, Does.Contain("tilebarcontrol__sectionitemfavourite"));
		if (sectionType == SectionType.Favorite)
		{
			Assert.That(favouriteButton[0].ClassList, Does.Contain("button--star-active"));
		}
		else
		{
			Assert.That(favouriteButton[0].ClassList, Does.Contain("button--star-inactive"));
		}
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemFavouriteIndexAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A");
			AddItem(section, "B");
		});

		var favouriteIndex = rendered.FindAll(".tilebarcontrol__sectionitemindex");
		if (sectionType == SectionType.Favorite)
		{
			Assert.That(favouriteIndex.Count, Is.EqualTo(2));
			Assert.That(favouriteIndex[0].TextContent, Is.EqualTo("1"));
			Assert.That(favouriteIndex[1].TextContent, Is.EqualTo("2"));
		}
		else
		{
			Assert.That(favouriteIndex.Count, Is.EqualTo(0));
		}
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemFavouriteClickAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		var clickEventFired = new TaskCompletionSource();
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section = AddSection(tab1, "One", sectionType);
			AddItem(section, "A", null, null, () => clickEventFired.SetResult());
		});

		await rendered.Find(".tilebarcontrol__sectionitem > button:nth-child(1)").ClickAsync(new WebMouseEventArgs());
		Assert.That(await clickEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	async Task<(IRenderedFragment, TileBarControl, NavigationMenuViewModel, NavigationMenuViewModel)> RenderTileBarControlAsync(WinzorTestContext ctx, CargoWiseClientServices clientServices = null)
	{
		TileBarControl tileBarControl = null;
		NavigationMenuViewModel categoryOne = null;
		NavigationMenuViewModel categoryTwo = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var dataContext = new NavigationViewModel();
			tileBarControl = new TileBarControl() { DataContext = dataContext };
			categoryOne = AddCategory(dataContext, "One");
			categoryTwo = AddCategory(dataContext, "Two");
			tileBarControl.InitializeComponent();
			form.Controls.Add(tileBarControl);
			return form;
		}, clientServices ?? ctx.DefaultClientServices);
		Assert.That(tileBarControl, Is.Not.Null);
		Assert.That(categoryOne, Is.Not.Null);
		Assert.That(categoryTwo, Is.Not.Null);
		return (rendered, tileBarControl, categoryOne, categoryTwo);
	}

	public static IEnumerable<TestCaseData> SectionTestCaseSource
	{
		get
		{
			yield return new TestCaseData(SectionType.Favorite) { TestName = "{m}_Favorite" };
			yield return new TestCaseData(SectionType.RecentItem) { TestName = "{m}_RecentItem" };
			yield return new TestCaseData(SectionType.RecentModule) { TestName = "{m}_RecentModule" };
		}
	}

#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods; test code only

	NavigationMenuViewModel AddCategory(NavigationViewModel viewModel, string categoryName, bool showSearch = false)
	{
		var category = new NavigationMenuViewModel(ResString.GetMultilingualString("", $"Category {categoryName}"), $"category{categoryName}", 1, showSearch);
		viewModel.AddCategory(category);
		return category;
	}

	MenuSection AddSection(NavigationMenuViewModel category, string sectionName, SectionType sectionType)
	{
		var subcategoryButton = new MenuSection($"Section {sectionName}", $"section{sectionName}", ResString.GetMultilingualString("", $"Section {sectionName}"), sectionType);
		category.Buttons.Add(subcategoryButton);
		return subcategoryButton;
	}

	MenuSection AddSubcategory(NavigationMenuViewModel category, string subCategoryLetter)
	{
		var subcategoryButton = new MenuSection($"Test SubCategory {subCategoryLetter}", $"testSubcategory{subCategoryLetter}", ResString.GetMultilingualString("", $"Test SubCategory {subCategoryLetter}"), SectionType.Subcategory, -1, subCategoryLetter);
		category.Buttons.Add(subcategoryButton);
		return subcategoryButton;
	}

	MenuSection AddSubSection(MenuSection section, string subSectionName)
	{
		var subSection = new MenuSection($"SubSection {subSectionName}", $"subSection{subSectionName}", ResString.GetMultilingualString(string.Empty, $"SubSection {subSectionName}"));
		section.Subsections.Add(subSection);
		return subSection;
	}

	MenuItem AddItem(MenuSection section, string itemName, Action clickAction = null, Action rightClickAction = null, Action favouriteAction = null, Func<byte, bool> moveAction = null)
	{
		var item = new MenuItem($"menuItem{itemName}", ResString.GetMultilingualString(string.Empty, $"Menu Item {itemName}"), ResString.GetMultilingualString("TestModule", "Test Module"), clickAction, rightClickAction, favouriteAction, moveAction);
		section.AddItem(item);
		return item;
	}

#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods; test code only

	TileBarControl CreateTileBarControlForTesting()
	{
		var dataContext = new NavigationViewModel();
		TileBarControl tileBarControl = new TileBarControl() { DataContext = dataContext };
		var categoryOne = AddCategory(dataContext, "One");
		dataContext.SelectedCategory = categoryOne;
		var section = AddSection(categoryOne, "One", SectionType.Favorite);
		AddItem(section, "A");
		tileBarControl.InitializeComponent();
		return tileBarControl;
	}

	[TestCase, WithPlaywrightPage]
	public async Task TileBarControlTabNavigationStyleAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateTileBarControlForTesting());
		var tabNavigation = page.Locator(".tilebarcontrol__tabnavigation");
		await tabNavigation.WaitForAsync();
		await Assertions.Expect(tabNavigation).ToHaveCSSAsync("padding-top", "0px");
		await Assertions.Expect(tabNavigation).ToHaveCSSAsync("padding-right", "5px");
		await Assertions.Expect(tabNavigation).ToHaveCSSAsync("padding-bottom", "0px");
		await Assertions.Expect(tabNavigation).ToHaveCSSAsync("padding-left", "5px");
	}

	[TestCase, WithPlaywrightPage]
	public async Task TileBarControlSectionStyleAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateTileBarControlForTesting());
		var section = page.Locator(".tilebarcontrol__section");
		await section.WaitForAsync();
		Assert.That(async () => await section.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-right')"), Is.EqualTo("3px"));		
	}

	[TestCase, WithPlaywrightPage]
	public async Task TileBarControlSectionItemStyleAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateTileBarControlForTesting());
		var sectionItem = page.Locator(".tilebarcontrol__sectionitem");
		await sectionItem.WaitForAsync();
		Assert.That(async () => await sectionItem.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('gap')"), Is.EqualTo("6px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarItemsFavoriteStarButtonStyleAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateTileBarControlForTesting());
		var inactiveStarButtonImageurl = $"url(\"{ctx.ServerBaseUrl}/_content/CargoWise.GUI.TileBar/images/star-inactive.png\")";
		var activeStarButtonImageurl = $"url(\"{ctx.ServerBaseUrl}/_content/CargoWise.GUI.TileBar/images/star-active.png\")";
		var activeStarbutton = await page.WaitForSelectorAsync(".button--star-active");
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("1"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(activeStarButtonImageurl));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("12px"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-repeat')"), Is.EqualTo("no-repeat"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-position')"), Is.EqualTo("50% 50%"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("16px"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("16px"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')"), Is.EqualTo("0px none rgb(0, 0, 0)"));

		await activeStarbutton.HoverAsync();
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(inactiveStarButtonImageurl));		
	}

	[Test]
	public async Task TileBarControlOnlyFavoriteItemsDraggableAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var section1 = AddSection(tab1, "One", SectionType.Favorite);
			AddItem(section1, "A");
			var section2 = AddSection(tab2, "Two", SectionType.RecentItem);
			AddItem(section2, "B");
		});

		Assert.That(rendered.FindAll(".tilebarcontrol__sectionitem[draggable='true']").Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarControlDragFavoriteItemsToUpperHalfAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		MenuSection section = null;
		MenuItem item1 = null;
		MenuItem item2 = null;
		MenuItem item3 = null;
		MenuItem item4 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			section = AddSection(categoryOne, "One", SectionType.Favorite);
			item1 = AddItem(section, "A", null, null, null, _ => true);
			item2 = AddItem(section, "B", null, null, null, _ => true);
			item3 = AddItem(section, "C", null, null, null, _ => true);
			item4 = AddItem(section, "D", null, null, null, _ => true);
			tileBarControl.InitializeComponent();
			form.Controls.Add(tileBarControl);
			return form;
		});

		await page.WaitForSelectorAsync(".tilebarcontrol__sectionitems");
		Assert.That(section.Items, Is.EqualTo(new MenuItem[] { item1, item2, item3, item4 }));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(2)", ".tilebarcontrol__sectionitem:nth-child(6)", new()
		{
			SourcePosition = new() { X = 50, Y = 5 },
			TargetPosition = new() { X = 50, Y = 5 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item2, item1, item3, item4 }).After(1500));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(6)", ".tilebarcontrol__sectionitem:nth-child(2)", new()
		{
			SourcePosition = new() { X = 50, Y = 5 },
			TargetPosition = new() { X = 50, Y = 5 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item3, item2, item1, item4 }).After(1500));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(4)", ".tilebarcontrol__sectionitem:nth-child(6)", new()
		{
			SourcePosition = new() { X = 50, Y = 5 },
			TargetPosition = new() { X = 50, Y = 5 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item3, item2, item1, item4 }).After(1500));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(6)", ".tilebarcontrol__sectionitem:nth-child(4)", new()
		{
			SourcePosition = new() { X = 50, Y = 5 },
			TargetPosition = new() { X = 50, Y = 5 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item3, item1, item2, item4 }).After(1500));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarControlDragFavoriteItemsToLowerHalfAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		MenuSection section = null;
		MenuItem item1 = null;
		MenuItem item2 = null;
		MenuItem item3 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			section = AddSection(categoryOne, "One", SectionType.Favorite);
			item1 = AddItem(section, "A", null, null, null, _ => true);
			item2 = AddItem(section, "B", null, null, null, _ => true);
			item3 = AddItem(section, "C", null, null, null, _ => true);
			tileBarControl.InitializeComponent();
			form.Controls.Add(tileBarControl);
			return form;
		});

		await page.WaitForSelectorAsync(".tilebarcontrol__sectionitems");
		Assert.That(section.Items, Is.EqualTo(new MenuItem[] { item1, item2, item3 }));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(2)", ".tilebarcontrol__sectionitem:nth-child(6)", new()
		{
			SourcePosition = new() { X = 1, Y = 1 },
			TargetPosition = new() { X = 1, Y = 18 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item2, item3, item1 }).After(1500));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(6)", ".tilebarcontrol__sectionitem:nth-child(2)", new()
		{
			SourcePosition = new() { X = 1, Y = 1 },
			TargetPosition = new() { X = 1, Y = 18 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item2, item1, item3 }).After(1500));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(4)", ".tilebarcontrol__sectionitem:nth-child(6)", new()
		{
			SourcePosition = new() { X = 1, Y = 1 },
			TargetPosition = new() { X = 1, Y = 18 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item2, item3, item1 }).After(1500));

		await page.DragAndDropAsync(".tilebarcontrol__sectionitem:nth-child(6)", ".tilebarcontrol__sectionitem:nth-child(4)", new()
		{
			SourcePosition = new() { X = 1, Y = 1 },
			TargetPosition = new() { X = 1, Y = 18 },
			Force = true,
		});
		Assert.That(() => section.Items, Is.EqualTo(new MenuItem[] { item2, item3, item1 }).After(1500));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarControlDragFavoriteItemsRendersReorderingDividersProperlyAsync()
	{
		MenuSection section = null;
		MenuItem item1 = null;
		MenuItem item2 = null;
		MenuItem item3 = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			section = AddSection(categoryOne, "One", SectionType.Favorite);
			item1 = AddItem(section, "A", null, null, null, _ => true);
			item2 = AddItem(section, "B", null, null, null, _ => true);
			item3 = AddItem(section, "C", null, null, null, _ => true);
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		var itemRect1 = await page.Locator(".tilebarcontrol__sectionitem:nth-child(2)").BoundingBoxAsync();
		var itemRect2 = await page.Locator(".tilebarcontrol__sectionitem:nth-child(4)").BoundingBoxAsync();
		var itemRect3 = await page.Locator(".tilebarcontrol__sectionitem:nth-child(6)").BoundingBoxAsync();

		var allDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider").AllAsync();
		var hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => allDividers.Count, Is.EqualTo(4).After(3000, 100));
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(4).After(3000, 100));
		await page.Mouse.MoveAsync(itemRect1.X + 1, itemRect1.Y + 1);
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(itemRect1.X + 1, itemRect1.Y + 7, new()
		{
			Steps = 6,
		});
		var currentDivider = page.Locator(".tilebarcontrol__sectionreorderingdivider:nth-child(1):not(.tilebarcontrol__sectionreorderingdivider--hidden)");
		Assert.That(() => currentDivider, Is.Not.Null.After(3000, 100));
		hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(3).After(3000, 100));

		await page.Mouse.MoveAsync(itemRect1.X + 1, itemRect1.Y + 18, new()
		{
			Steps = 6,
		});
		currentDivider = page.Locator(".tilebarcontrol__sectionreorderingdivider:nth-child(3):not(.tilebarcontrol__sectionreorderingdivider--hidden)");
		Assert.That(() => currentDivider, Is.Not.Null.After(3000, 100));
		hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(3).After(3000, 100));

		await page.Mouse.MoveAsync(itemRect2.X + 1, itemRect2.Y + 7, new()
		{
			Steps = 6,
		});
		currentDivider = page.Locator(".tilebarcontrol__sectionreorderingdivider:nth-child(3):not(.tilebarcontrol__sectionreorderingdivider--hidden)");
		Assert.That(() => currentDivider, Is.Not.Null.After(3000, 100));
		hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(3).After(3000, 100));

		await page.Mouse.MoveAsync(itemRect2.X + 1, itemRect2.Y + 18, new()
		{
			Steps = 6,
		});
		currentDivider = page.Locator(".tilebarcontrol__sectionreorderingdivider:nth-child(5):not(.tilebarcontrol__sectionreorderingdivider--hidden)");
		Assert.That(() => currentDivider, Is.Not.Null.After(3000, 100));
		hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(3).After(3000, 100));

		await page.Mouse.MoveAsync(itemRect3.X + 1, itemRect3.Y + 7, new()
		{
			Steps = 6,
		});
		currentDivider = page.Locator(".tilebarcontrol__sectionreorderingdivider:nth-child(5):not(.tilebarcontrol__sectionreorderingdivider--hidden)");
		Assert.That(() => currentDivider, Is.Not.Null.After(3000, 100));
		hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(3).After(3000, 100));

		await page.Mouse.MoveAsync(itemRect3.X + 1, itemRect3.Y + 18, new()
		{
			Steps = 6,
		});
		currentDivider = page.Locator(".tilebarcontrol__sectionreorderingdivider:nth-child(7):not(.tilebarcontrol__sectionreorderingdivider--hidden)");
		Assert.That(() => currentDivider, Is.Not.Null.After(3000, 100));
		hiddenDividers = await page.Locator(".tilebarcontrol__sectionreorderingdivider--hidden").AllAsync();
		Assert.That(() => hiddenDividers.Count, Is.EqualTo(3).After(3000, 100));
	}

	[Test]
	public async Task TilebarControlIsRenderedWithinElementHostAsync()
	{
		using var ctx = new EnterpriseTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new ElementHost();

			var dataContext = new NavigationViewModel();
			dataContext.AddCategory(new NavigationMenuViewModel(ResString.GetMultilingualString("1", "One"), "One", 0, true));
			dataContext.AddCategory(new NavigationMenuViewModel(ResString.GetMultilingualString("2", "Two"), "Two", 1, true));

			elementHost.Child = new TileBarControl() { DataContext = dataContext };
			elementHost.Dock = DockStyle.Fill;
			panel.Controls.Add(elementHost);
			return form;
		});

		Assert.That(() => rendered.Find(".tilebarcontrol"), Throws.Nothing);
	}

	[TestCase]
	public async Task TileBarControl_CategoryShowSearch_SearchButtonVisibilityAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			AddCategory(tileBar.DataContext, "Three", true);
		});

		var tabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem");
		Assert.That(tabs[0].ClassList, Does.Contain("tilebarcontrol__tabnavigationitem--selected"));

		Assert.That(rendered.FindAll(".tilebarcontrol__search").Count, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".tilebarcontrol__searchbutton").Count, Is.EqualTo(0));

		await tabs[2].ClickAsync(new WebMouseEventArgs());
		tabs = rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem");
		Assert.That(tabs[2].ClassList, Does.Contain("tilebarcontrol__tabnavigationitem--selected"));

		Assert.That(rendered.FindAll(".tilebarcontrol__search").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".tilebarcontrol__searchbutton").Count, Is.EqualTo(1));
	}

	[TestCase]
	public async Task TileBarControl_SearchButton_ToolTipAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			AddCategory(tileBar.DataContext, "Three", true);
		});

		await rendered.FindAll(".tilebarcontrol__tabnavigation > .tilebarcontrol__tabnavigationitem")[2].ClickAsync(new WebMouseEventArgs());

		var searchButton = rendered.Find(".tilebarcontrol__searchbutton");
		Assert.That(searchButton.GetAttribute("title"), Is.EqualTo(tileBar.DataContext.SearchHint.ToString()));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarControlTabContentWillRenderScrollbarAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			var section = AddSection(categoryOne, "One", SectionType.Favorite);
			AddItem(section, "A");
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		var tabContent = await page.WaitForSelectorAsync(".tilebarcontrol__tabcontent");
		Assert.That(async () => await tabContent.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden"));
		Assert.That(async () => await tabContent.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("auto"));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarLinksHavePointerCursorAsync()
	{
		// Arrange
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var dataContext = new NavigationViewModel();
			var categoryOne = AddCategory(dataContext, "Cat1");
			var sectionOne = AddSection(categoryOne, "Section1", SectionType.RecentItem);
			AddItem(sectionOne, "Item1");
			var form = new Form();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		var addToFavoritesButton = page.Locator(".button--star-inactive");
		await addToFavoritesButton.WaitForAsync();
		var link = page.Locator(".tilebarcontrol__sectionitemlink");
		await link.WaitForAsync();

		// Act
		await addToFavoritesButton.HoverAsync();

		// Assert
		Assert.That(async () => await addToFavoritesButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('cursor')"), Is.EqualTo("pointer"));
		Assert.That(async () => await link.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('cursor')"), Is.EqualTo("pointer"));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarSearchResultsHavePointerCursorAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NavigationViewModel dataContext = null;
		TileBarControl tileBarControl = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataContext = new NavigationViewModel();
			tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One", true);
			var section = AddSection(categoryOne, "One", SectionType.Favorite);
			var item1 = AddItem(section, "A", null, null, null, _ => true);
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		await page.ClickAsync(".tilebarcontrol__searchbutton");
		await page.ClickAsync(".tilebarcontrol__searchinput");

		var searchInput = page.Locator(".tilebarcontrol__searchinput");
		await searchInput.PressAsync("A");
		await searchInput.PressAsync("Enter");
		Assert.That(tileBarControl.DataContext.SearchViewModel.SearchValue, Is.EqualTo("A"));

		await tileBarControl.InvokeWinzorDispatcherAsync(
			tileBarControl.DataContext.SearchViewModel.WaitingForSearchToComplete);

		var sectionItemLink = page.Locator(".tilebarcontrol__sectionitemlink");
		Assert.That(await sectionItemLink.GetComputedStyleAsync("cursor"), Is.EqualTo("pointer"));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarSubcategorySectionHavePointerCursorAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NavigationViewModel dataContext = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One", true);
			var subCategory = AddSubcategory(categoryOne, "Sub");
			var section = AddSubSection(subCategory, "Section 1");
			var item1 = AddItem(section, "A", null, null, null, _ => true);
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		await (await page.QuerySelectorAllAsync(".tilebarcontrol__tabnavigationitem"))[0].ClickAsync();
		await (await page.QuerySelectorAllAsync(".tilebarcontrol__subcategory"))[0].ClickAsync();

		var subcategorySectionItem = page.Locator(".tilebarcontrol__subcategorysectionitemlink");

		Assert.That(await subcategorySectionItem.GetComputedStyleAsync("cursor"), Is.EqualTo("pointer"));
	}

	[Test]
	public async Task TileBarControlEscapeAmpersandInHeaderTextAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var (rendered, tileBar, tab1, tab2) = await RenderTileBarControlAsync(ctx);
		await tileBar.InvokeWinzorDispatcherAsync(() =>
		{
			var subcategory = AddSubcategory(tab1, "A");
			AddSubSection(subcategory, "Text && Text2");
			AddSubSection(subcategory, "2");
		});

		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		var subsectionHeader = rendered.FindAll(".tilebarcontrol__subcategorysection:nth-child(1) > .tilebarcontrol__subcategorysectionheader");
		Assert.That(subsectionHeader.Count, Is.EqualTo(1));
		Assert.That(subsectionHeader[0].TextContent, Is.EqualTo("SubSection Text & Text2"));
	}

	[Test, WithPlaywrightPage]
	public async Task TileBarControlSearchBoxInputDoesNotSwallowTextWhenTypingFastAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NavigationViewModel dataContext = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl() { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One", true);
			var section = AddSection(categoryOne, "One", SectionType.Favorite);
			var item1 = AddItem(section, "A", null, null, null, _ => true);
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		await page.ClickAsync(".tilebarcontrol__searchbutton");
		await page.ClickAsync(".tilebarcontrol__searchinput");

		var searchInput = page.Locator(".tilebarcontrol__searchinput");
		// type like the fastest typist but still human
		await searchInput.PressSequentiallyAsync("customer service tickets", new() { Delay = 50 });

		Assert.That(() => dataContext.SearchViewModel.SearchValue, Is.EqualTo("customer service tickets").After(2000, 100));
	}

	[TestCaseSource(nameof(SectionTestCaseSource))]
	public async Task TileBarControlSectionItemFocusInAsync(SectionType sectionType)
	{
		using var ctx = new EnterpriseTestContext();
		TileBarControl tileBarControl = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			form.Controls.Add(textBox);

			var dataContext = new NavigationViewModel();
			tileBarControl = new TileBarControl { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			tileBarControl.InitializeComponent();
			var section = AddSection(categoryOne, "One", sectionType);
			AddItem(section, "A");

			form.Controls.Add(tileBarControl);
			return form;
		});

		Assert.That(tileBarControl.Focused, Is.False);
		await Task.Delay(100);
		await rendered.Find(".tilebarcontrol__sectionitem > button:nth-child(2)").FocusInAsync(new FocusEventArgs());
		Assert.That(tileBarControl.Focused, Is.True);
	}

	[Test]
	public async Task TileBarControlSubCategoryItemFocusInAsync()
	{
		using var ctx = new EnterpriseTestContext();
		TileBarControl tileBarControl = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			form.Controls.Add(textBox);

			var dataContext = new NavigationViewModel();
			tileBarControl = new TileBarControl { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			tileBarControl.InitializeComponent();

			var subCategory = AddSubcategory(categoryOne, "A");
			var subSectionOne = AddSubSection(subCategory, "1");
			AddItem(subSectionOne, "Test!");

			form.Controls.Add(tileBarControl);
			return form;
		});

		await Task.Delay(100);
		await rendered.Find(".tilebarcontrol__subcategory").ClickAsync(new WebMouseEventArgs());

		Assert.That(tileBarControl.Focused, Is.False);
		await rendered.FindAll(".tilebarcontrol__subcategorysectionitem > button").First().FocusInAsync(new FocusEventArgs());
		Assert.That(tileBarControl.Focused, Is.True);
	}

	[Test]
	public async Task PreloadTileBarJSInteropAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<ITileBarJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dataContext = new NavigationViewModel();
			var tileBarControl = new TileBarControl { DataContext = dataContext };
			var categoryOne = AddCategory(dataContext, "One");
			tileBarControl.InitializeComponent();
			return tileBarControl;
		});

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}
}
