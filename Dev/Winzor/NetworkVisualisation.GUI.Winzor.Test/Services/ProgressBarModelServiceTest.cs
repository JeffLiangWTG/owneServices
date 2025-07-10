using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class ProgressBarModelServiceTest
{
	[Test]
	public void TestGetProgressBarData()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		nvm.LowerBar = new NodeViewModel.ProgressBar(Color.Red, 0.6);
		nvm.LowerBarTooltip = "Lower bar tooltip";

		using var service = new ProgressBarModelService(nvm);
		var data = service.GetProgressBarModelData();

		Assert.Multiple(() =>
		{
			Assert.That(data.LowerBarHeight, Is.EqualTo(5));
			Assert.That(data.Percent, Is.EqualTo(0.6));
			Assert.That(data.Background, Is.EqualTo(Color.Red));
			Assert.That(data.LowerBarTooltip, Is.EqualTo("Lower bar tooltip"));
		});
	}

	[Test]
	public void TestGetProgressBarData_WhenLowerBarIsNull()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new ProgressBarModelService(nvm);
		var data = service.GetProgressBarModelData();

		Assert.Multiple(() =>
		{
			Assert.That(data.LowerBarHeight, Is.EqualTo(0d));
			Assert.That(data.Percent, Is.EqualTo(0d));
			Assert.That(data.Background, Is.EqualTo(Color.Transparent));
			Assert.That(data.LowerBarTooltip, Is.Null);
		});
	}
}
