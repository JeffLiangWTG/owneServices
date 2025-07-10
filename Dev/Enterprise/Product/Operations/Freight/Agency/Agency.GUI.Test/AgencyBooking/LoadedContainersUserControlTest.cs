using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class LoadedContainersUserControlTest : BaseAgencyTest
	{
		public void TestVerifiedByAddressDetailsColumns()
		{
			using (var containerUserControl = new LoadedContainersUserControl())
			{
				var containersGrid = (ZGrid)containerUserControl.Controls.Find("containersGrid", true)[0];
				ZGridColumnInfo columnInfo = null;
				foreach (var columnName in new string[] { "GrossWeightVerifiedByAddress+E2_AddressOverride", "GrossWeightVerifiedByAddress+E2_Address1", "GrossWeightVerifiedByAddress+E2_Address2", "GrossWeightVerifiedByAddress+E2_City", "GrossWeightVerifiedByAddress+E2_State", "GrossWeightVerifiedByAddress+E2_RN_NKCountryCode", "GrossWeightVerifiedByAddress+E2_Postcode", })
				{
					columnInfo = FindGridColumnByName(containersGrid, columnName);
					Assert(columnInfo != null);
					AssertEquals("All columns belong to VGM group", "Verified By Address Details", columnInfo.GroupName.Caption);
					AssertEquals("All columns are not visible", false, columnInfo.IsVisible);
				}
			}
		}

		ZGridColumnInfo FindGridColumnByName(ZGrid grid, string columnName)
		{
			return (
				from columnStyleInfo in grid.ColumnStyles.Cast<ZGridColumnInfo>()
				where columnStyleInfo.ColumnName == columnName
				select columnStyleInfo).FirstOrDefault();
		}
	}
}
