using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	class TRBillAdditionalUserControlTest : TestCaseWithFactory
	{
		public void TestIAdditionalTabPage()
		{
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;

			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl");
				billsAndPacksTabControl.SelectedTab = additionalTabPage;

				var caption = additionalTabPage.CaptionResourceString;
				using (var trUserControl = new TRBillAdditionalUserControl())
				{
					CombineAssertions("Forwarder | AIR | HAVIHR", () =>
					{
						AssertEquals("Caption", caption, ((IAdditionalTabPage)trUserControl).AdditionalTabPageCaption);
						Assert("Visible", additionalTabPage.TabVisible);
					});

					CombineAssertions("Forwarder | SEA | HAVIHR", () =>
					{
						header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
						header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
						AssertEquals("Caption", caption, ((IAdditionalTabPage)trUserControl).AdditionalTabPageCaption);
						Assert("Visible", additionalTabPage.TabVisible);
					});

					CombineAssertions("Forwarder | SEA | ATAIHR", () =>
					{
						header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
						header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
						AssertEquals("Caption", caption, ((IAdditionalTabPage)trUserControl).AdditionalTabPageCaption);
						Assert("NOT Visible", !additionalTabPage.TabVisible);
					});

					CombineAssertions("Forwarder | SEA | IMPORT", () =>
					{
						header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
						Assert("NOT Visible", !additionalTabPage.TabVisible);
					});
				}
			}

			var header2 = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header2.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header2.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;

			var bill2 = header2.Bills.AddNew();

			using (var form2 = new ManifestForm(header2))
			{
				form2.Show();
				var asycudaManifestUserControl2 = form2.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl2 = asycudaManifestUserControl2.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage2 = mainTabControl2.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl2.SelectedTab = billsAndPacksTabPage2;
				var billsAndPacksTabControl2 = mainTabControl2.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage2 = billsAndPacksTabControl2.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl");
				billsAndPacksTabControl2.SelectedTab = additionalTabPage2;

				var caption2 = additionalTabPage2.CaptionResourceString;
				using (var trUserControl2 = new TRBillAdditionalUserControl())
				{
					CombineAssertions("Carrier | AIR | HAVIHR", () =>
					{
						AssertEquals("Caption", caption2, ((IAdditionalTabPage)trUserControl2).AdditionalTabPageCaption);
						Assert("Visible", additionalTabPage2.TabVisible);
					});

					CombineAssertions("Carrier | SEA | HAVIHR", () =>
					{
						header2.AMA_TransportMode = Core.Constants.TransportModes.Sea;
						header2.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
						AssertEquals("Caption", caption2, ((IAdditionalTabPage)trUserControl2).AdditionalTabPageCaption);
						Assert("Visible", additionalTabPage2.TabVisible);
					});

					CombineAssertions("Carrier | SEA | ATAIHR", () =>
					{
						header2.AMA_TransportMode = Core.Constants.TransportModes.Sea;
						header2.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
						AssertEquals("Caption", caption2, ((IAdditionalTabPage)trUserControl2).AdditionalTabPageCaption);
						Assert("NOT Visible", !additionalTabPage2.TabVisible);
					});

					CombineAssertions("Carrier | SEA | IMPORT", () =>
					{
						header2.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
						Assert("NOT Visible", !additionalTabPage2.TabVisible);
					});
				}
			}
		}
	}
}
