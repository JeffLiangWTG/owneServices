using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using AsycudaManifestHeader = Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	public partial class AsycudaManifestAdditionalTabPageUserControlTest : TestCaseWithFactory
	{
		public void TestGrid()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var asycudaManifestAdditionalTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl");
				mainTabControl.SelectedTab = asycudaManifestAdditionalTabPage;
				var headerAdditionalTabPageUserControl = asycudaManifestAdditionalTabPage.Controls.Find("mainTabControl_UserControl_VisitedPortsForManifestHeaderUserControl", true)[0] as AsycudaManifestHeaderAdditionalTabPageUserControl;
				var grid = headerAdditionalTabPageUserControl.Controls.Find("visitedPortsForManifestHeaderUserControlGrid", true)[0] as ZGrid;
				var columnStyles = ((ZGrid)headerAdditionalTabPageUserControl.Controls.Find("visitedPortsForManifestHeaderUserControlGrid", true).First()).ColumnStyles;
				var expectedList = GetExpectedVisitedPortColumnNames();
				var length = expectedList.Length;
				Assert("visitedPortsForManifestHeaderUserControlGrid.ColumnStyles.Count must have at least " + length.ToString(), length <= columnStyles.Count);
				CombineAssertions(() =>
				{
					for (int i = 0; i < expectedList.Length; i++)
					{
						var columnInfo = columnStyles[i] as ZGridColumnInfo;
						AssertNotNull(columnInfo);
						var expectedColumnName = expectedList[i];
						AssertEquals(i.ToString() + " Expected", expectedColumnName, columnInfo.ColumnName);
					}
				});
			}
		}

		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var additionalTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabControl_TabPage_VisitedPortsForManifestHeaderUserControl");
				mainTabControl.SelectedTab = additionalTabPage;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", additionalTabPage.TabVisible);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", additionalTabPage.TabVisible);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", !additionalTabPage.TabVisible);
				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				manifest.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", additionalTabPage.TabVisible);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", additionalTabPage.TabVisible);
				manifest.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", !additionalTabPage.TabVisible);
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				Assert("Itinerary tab should only be active for Carrier manifests types and for transport types AIR and SEA and only for CIKONC and VARONC manifest types", !additionalTabPage.TabVisible);
			}
		}

		string[] GetExpectedVisitedPortColumnNames()
		{
			return new string[] { VisitedPort.Schema.CY_Order, VisitedPort.Schema.CY_Data, VisitedPort.Schema.CY_Code, VisitedPort.Schema.CY_Date };
		}
	}
}
