using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class ContainerCustomColumnAdderTest : BaseAgencyTest
	{
		public void TestSet()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				using (ZGrid grid = new ZGrid())
				{
					ContainerCustomColumnAdder.Set(grid, ContainerCustomColumnAdder.TargetGridType.Editable);
					AssertEquals("Should be 2 new columns", 2, grid.ColumnStyles.Count);
					AssertEquals("Should be CustomsEntryNumberType column", "CustomsEntryNumberType", ((ZGridColumnInfo)grid.ColumnStyles[0]).ColumnName);
					AssertEquals("Should be CustomsEntryNumber column", "CustomsEntryNumber", ((ZGridColumnInfo)grid.ColumnStyles[1]).ColumnName);
				}

				using (ZGrid grid = new ZGrid())
				{
					ContainerCustomColumnAdder.Set(grid, ContainerCustomColumnAdder.TargetGridType.Module);
					AssertEquals("Should be 2 new columns", 2, grid.ColumnStyles.Count);
					AssertEquals("Should be BillContainersEntryNumberType column", "BillContainersEntryNumberType", ((ZGridColumnInfo)grid.ColumnStyles[0]).ColumnName);
					AssertEquals("Should be BillContainersEntryNumber column", "BillContainersEntryNumber", ((ZGridColumnInfo)grid.ColumnStyles[1]).ColumnName);
				}

				GlbCompany.CurrentCompany.SetCountry("US");
				using (ZGrid grid = new ZGrid())
				{
					ContainerCustomColumnAdder.Set(grid, ContainerCustomColumnAdder.TargetGridType.Editable);
					AssertEquals("Should NOT be any new columns", 0, grid.ColumnStyles.Count);
				}

				using (ZGrid grid = new ZGrid())
				{
					ContainerCustomColumnAdder.Set(grid, ContainerCustomColumnAdder.TargetGridType.Module);
					AssertEquals("Should NOT be any new columns", 0, grid.ColumnStyles.Count);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}
	}
}
