using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RibbonToggleButtonTest : BunitTestContext
{
	[Test]
	public void RibbonToggleButtonAppliesCheckedAttribute()
	{
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});

		var viewModel = new RibbonToggleButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action.Object);
		var cut = RenderComponent<RibbonToggleButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButton = cut.Find(".ribbontogglebutton");

		action.Setup(a => a.IsActivated()).Returns(true);
		viewModel.RefreshActionAndProperties(new RefreshArgs(RefreshType.None, Array.Empty<INetworkEntity>()), null);
		cut.Render();
		Assert.That(ribbonButton.Attributes["data-checked"], Is.Not.Null);

		action.Setup(a => a.IsActivated()).Returns(false);
		viewModel.RefreshActionAndProperties(new RefreshArgs(RefreshType.None, Array.Empty<INetworkEntity>()), null);
		cut.Render();
		Assert.That(ribbonButton.Attributes["data-checked"], Is.Null);

		action.Setup(a => a.IsActivated()).Returns(true);
		viewModel.RefreshActionAndProperties(new RefreshArgs(RefreshType.None, Array.Empty<INetworkEntity>()), null);
		cut.Render();
		Assert.That(ribbonButton.Attributes["data-checked"], Is.Not.Null);
	}
}
