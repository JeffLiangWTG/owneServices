using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class TransportDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new TransportDetailsLayoutsUserControl())
			{
				AssertEquals("TransportDetailsUserControl test data source", typeof(JobDeclaration), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new TransportDetailsLayoutsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Shipment Final Destination - Visible", true, control.TransportDetailsFlightUserControl.Visible);
					AssertEquals("Shipment Destination - Visible", true, control.TransportDetailsNationalityUserControl.Visible);
					AssertEquals("Shipment Destination - Visible", true, control.TransportDetailsVoyageUserControl.Visible);
				});
			}
		}

		public void TestControlsVisibleForAIR()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;

				AssertEquals("OverrideValuesCheckBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("TransportDetailsVoyageUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsVoyageUserControl, declaration));
				AssertEquals("VesselCodeFindBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
				AssertEquals("MasterBillTextBox", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("TransportDetailsFlightUserControl", true, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsFlightUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportDetailsNationalityUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsNationalityUserControl, declaration));
			});
		}

		public void TestControlsVisibleForSEA()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

				AssertEquals("OverrideValuesCheckBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
				AssertEquals("OceanBillTextBox", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("TransportDetailsVoyageUserControl", true, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsVoyageUserControl, declaration));
				AssertEquals("VesselCodeFindBox", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("TransportDetailsFlightUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsFlightUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportDetailsNationalityUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsNationalityUserControl, declaration));
			});
		}

		public void TestControlsVisibleForROAD()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;

				AssertEquals("OverrideValuesCheckBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("TransportDetailsVoyageUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsVoyageUserControl, declaration));
				AssertEquals("VesselCodeFindBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("TransportDetailsFlightUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsFlightUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportDetailsNationalityUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsNationalityUserControl, declaration));
			});
		}

		public void TestControlsVisibleForFixed()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeFixedCodeForTesting;

				AssertEquals("OverrideValuesCheckBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("TransportDetailsVoyageUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsVoyageUserControl, declaration));
				AssertEquals("VesselCodeFindBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("TransportDetailsFlightUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsFlightUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("TransportDetailsNationalityUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.TransportDetailsNationalityUserControl, declaration));
			});
		}

		PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
		PanelLayout layout;
	}
}
