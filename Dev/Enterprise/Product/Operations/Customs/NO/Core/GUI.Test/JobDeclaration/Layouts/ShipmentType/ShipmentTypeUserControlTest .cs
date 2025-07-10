using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(ShipmentTypeUserControl))]
sealed class ShipmentTypeUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new ShipmentTypeUserControl())
		{
			AssertEquals("ShipmentTypeLayoutUserControl test data source", typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new ShipmentTypeUserControl();
		_ = control.AssertContainsControl<ZDropEdit>("CustomsTransportModeDropEdit", x => x
			.WithBindTo(nameof(JobDeclaration.JE_CustomsTransportMode))
		);
	});

	public void TestContainerModeVisible()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("Container mode should be visible", true, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

			declaration.JE_TransportMode = declaration.TransportModeFixedCodeForTesting;
			AssertEquals("Container mode should not be visible", false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("Container mode should be visible", true, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));
		});
	}

	PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayouts().Layout);
	PanelLayout layout;
}
