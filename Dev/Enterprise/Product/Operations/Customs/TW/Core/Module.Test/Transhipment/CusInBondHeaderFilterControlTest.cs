using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Transhipment.Module.Test
{
	public sealed class CusInBondHeaderFilterControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridFields()
		{
			var cusInBondHeaderCollection = new CusInBondHeaderCollection(Factory);
			var filterBO = new CusInBondHeaderFilterStripBusinessObject();
			using (var filterControl = new CusInBondHeaderFilterControl(cusInBondHeaderCollection, filterBO))
			{
				filterControl.Show();
				var filterGrid = filterControl.FilteredGrid;
				AssertNotNull(filterGrid.GetColumnStyle(CusInBondHeader.Schema.BH_Calc_ImportMasterBillNumber));
				AssertNotNull(filterGrid.GetColumnStyle(CusInBondHeader.Schema.BH_Calc_ImportHouseBillNumber));
				AssertNotNull(filterGrid.GetColumnStyle(CusInBondHeader.Schema.EntryNumber));
				AssertNotNull(filterGrid.GetColumnStyle(CusInBondHeader.Schema.BH_JobReference));
			}
		}
	}
}
