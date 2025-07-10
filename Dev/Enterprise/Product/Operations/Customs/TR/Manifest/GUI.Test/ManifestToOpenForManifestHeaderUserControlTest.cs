using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	sealed class ManifestToOpenForManifestHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestManifestToOpenTabPageVisibility2()
		{
			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var additionalTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabControl_TabPage_ManifestToOpenForManifestHeaderUserControl");

				mainTabControl.SelectedTab = additionalTabPage;
				CombineAssertions(() =>
				{
					manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", additionalTabPage.TabVisible);

					manifest.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !additionalTabPage.TabVisible);

					manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", additionalTabPage.TabVisible);

					manifest.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !additionalTabPage.TabVisible);

					manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", additionalTabPage.TabVisible);

					manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !additionalTabPage.TabVisible);

					manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
					manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", additionalTabPage.TabVisible);

					manifest.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
					Assert("ManifestToOpen tab should only be active for Carrier/Forwarder manifests types and for transport types AIR and SEA and only for HAVIHR and DENIHR manifest types", !additionalTabPage.TabVisible);
				});
			}
		}
	}
}
