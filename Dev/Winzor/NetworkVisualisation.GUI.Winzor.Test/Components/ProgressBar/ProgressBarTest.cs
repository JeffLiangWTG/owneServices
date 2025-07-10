using System.Drawing;
using AngleSharp.Css.Dom;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;

namespace NetworkVisualisation.GUI.Winzor.Test;

class ProgressBarTest : BunitTestContext
{
	[Test]
	public void TestMarkup()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };

		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		parentViewModel.LowerBar = new NodeViewModel.ProgressBar(Color.Yellow, 0.75);
		var component = RenderComponent<ProgressBar>(parameters => parameters.Add(p => p.Model , new ProgressBarModel(new ProgressBarModelService(parentViewModel))));

		var progressBar = component.Find(".ncn__progressbar");
		Assert.That(progressBar.TagName, Is.EqualTo("DIV"));
		var progress = component.Find(".progressbar__progress");
		Assert.That(progress.TagName, Is.EqualTo("DIV"));
	}

	[TestCase(0.75, "-25%")]
	[TestCase(0.3, "-70%")]
	[TestCase(0, "-100%")]
	[TestCase(333, "-0")]
	[TestCase(-0.1, "-110%")]
	public void TestGetTransform_CalculatesTransformCorrectly(double progress, string expectedTransform)
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };

		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		parentViewModel.LowerBar = new NodeViewModel.ProgressBar(Color.Yellow, progress);

		var component = RenderComponent<ProgressBar>(parameters => parameters.Add(p => p.Model, new ProgressBarModel(new ProgressBarModelService(parentViewModel))));

		var progressComponent = component.Find(".progressbar__progress");
		Assert.That(progressComponent.GetStyle().GetTransform(), Is.EqualTo($"translateX({expectedTransform})"));
	}

	[Test]
	public void TestTooltip()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };

		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		parentViewModel.LowerBar = new NodeViewModel.ProgressBar(Color.Yellow, 0.75);
		parentViewModel.LowerBarTooltip = "hello";

		var component = RenderComponent<ProgressBar>(parameters => parameters.Add(p => p.Model, new ProgressBarModel(new ProgressBarModelService(parentViewModel))));

		var progress = component.Find(".ncn__progressbar");
		Assert.That(progress.GetAttribute("title"), Is.EqualTo("hello"));
	}

	[Test]
	public void TestStyle()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };

		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		parentViewModel.LowerBar = new NodeViewModel.ProgressBar(Color.Yellow, 0.75);
		parentViewModel.LowerBarTooltip = "hello";

		var component = RenderComponent<ProgressBar>(parameters => parameters.Add(p => p.Model, new ProgressBarModel(new ProgressBarModelService(parentViewModel))));

		var progress = component.Find(".ncn__progressbar");
		Assert.That(progress.GetStyle().GetHeight(), Is.EqualTo("5px"));
		Assert.That(progress.GetStyle().GetBackground(), Is.EqualTo("rgba(255, 255, 0, 1)"));
	}
}
