using System.Windows.Forms;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Playwright;
using Moq;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RibbonTabTest : BunitTestContext
{
	[Test]
	public void RibbonTabDoesNotRenderWithoutViewModel()
	{
		var cut = RenderComponent<RibbonTab>();

		Assert.That(cut.Markup, Is.Empty);

		var viewModel = new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Ribbon Tab"));
		cut.SetParametersAndRender(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		Assert.That(cut.Find(".ribbontab"), Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task RibbonTabRendersScrollItemsAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NetworkUserControl? control = null;
		var form = default(Form);

		var ribbonDataProvider = new Mock<IRibbonDataProvider>();
		ribbonDataProvider
			.Setup(n => n.GetRibbonViewModel(It.IsAny<NetworkViewModel>(), It.IsAny<NetworkUserControl>()))
			.Returns(() =>
			{
				var ribbonViewModel = new RibbonViewModel();

				var ribbonTabViewModel = new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab"));
				ribbonViewModel.Tabs.Add(ribbonTabViewModel);

				return ribbonViewModel;
			});

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			control = new NetworkUserControl(new Entity(), Mock.Of<INetworkRefresher>(), ribbonDataProvider: ribbonDataProvider.Object);
			control.SetDataContext(new DummyNetwork(), false);
			form.Controls.Add(control);
			return form;
		});
		var ribbontab = await page.WaitForSelectorAsync(".ribbontab");

		if (ribbontab != null)
		{
			AssertCssProperty(ribbontab, "overflow-x", "auto");
		}
	}
	
	void AssertCssProperty(IElementHandle element, string propertyName, string expectedValue)
	{
		Assert.That(async () => await element.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('{propertyName}')"), Is.EqualTo(expectedValue));
	}
}
