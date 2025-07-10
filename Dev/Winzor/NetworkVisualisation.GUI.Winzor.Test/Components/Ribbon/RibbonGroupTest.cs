using CargoWise.NetworkVisualisation.GUI;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RibbonGroupTest : BunitTestContext
{
	[Test]
	public void RibbonGroupDoesNotRenderWithoutViewModel()
	{
		var cut = RenderComponent<RibbonGroup>();

		Assert.That(cut.Markup, Is.Empty);

		var viewModel = new RibbonGroupViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Group"), string.Empty);
		cut.SetParametersAndRender(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		Assert.That(cut.Find(".ribbongroup"), Is.Not.Null);
	}

	[Test]
	public void RibbonGroupRendersHeader()
	{
		var viewModel = new RibbonGroupViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Group"), string.Empty);
		var cut = RenderComponent<RibbonGroup>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var header = cut.Find(".ribbongroup__header");
		Assert.That(header.TextContent, Is.EqualTo("Ribbon Group"));
	}

	[Test]
	public void RibbonGroupRendersItems()
	{
		var ribbonViewModel = new RibbonViewModel();
		var viewModel = new RibbonGroupViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Group"), string.Empty);
		viewModel.Items.Add(new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Item 1"), ResString.GetMultilingualString(string.Empty, "Item 1 ToolTip"), null));
		viewModel.Items.Add(new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Item 2"), ResString.GetMultilingualString(string.Empty, "Item 2 ToolTip"), null));
		var cut = RenderComponent<RibbonGroup>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var items = cut.Find(".ribbongroup__items");
		Assert.That(items.ChildElementCount, Is.EqualTo(2));
	}

	[Test]
	public void RibbonGroupRendersItemsAccordingToViewModel()
	{
		var ribbonViewModel = new RibbonViewModel();
		var viewModel = new RibbonGroupViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Group"), string.Empty);
		viewModel.Items.Add(new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Item 1"), ResString.GetMultilingualString(string.Empty, "Item 1 ToolTip"), null));
		viewModel.Items.Add(new RibbonMenuButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Item 2"), ResString.GetMultilingualString(string.Empty, "Item 2 ToolTip"), null));
		viewModel.Items.Add(new RibbonToggleButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Item 3"), ResString.GetMultilingualString(string.Empty, "Item 3 ToolTip"), null));
		var cut = RenderComponent<RibbonGroup>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var items = cut.Find(".ribbongroup__items");

		var ribbonButton = items.Children[0];
		var ribbonMenuButton = items.Children[1];
		var ribbonToggleButton = items.Children[2];
		Assert.That(ribbonButton.ClassName, Does.Contain("ribbonbutton"));
		Assert.That(ribbonMenuButton.ClassName, Does.Contain("ribbonmenubutton"));
		Assert.That(ribbonToggleButton.ClassName, Does.Contain("ribbontogglebutton"));
	}
}
