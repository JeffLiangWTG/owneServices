using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	sealed class TradeGroupsFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			IBusinessObjectCollection collection = new CusRefTradeGroupCollection(Factory);
			FilterStripBusinessObject filterBusinessObject = new TradeGroupsFilterStripBusinessObject();
			using (var form = new ZForm())
			using (var control = new TradeGroupsFilterStripControl(collection, filterBusinessObject))
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.Grid;
				CombineAssertions(() =>
				{
					AssertEquals(9, grid.Columns.Count);
					AssertEquals("Country now displays as Country/Region or Ctry/Regn. for short caption", "Ctry/Rgn.", grid.GetColumnCaption(CusRefTradeGroupSchema.Constants.CR9_RN_NKCountryCode));
					AssertEquals("Trade Group", grid.GetColumnCaption(CusRefTradeGroupSchema.Constants.CR9_TradeGroup));
					AssertEquals("Description", grid.GetColumnCaption(CusRefTradeGroupSchema.Constants.CR9_Description));
					AssertEquals("Start Date", grid.GetColumnCaption(CusRefTradeGroupSchema.Constants.CR9_StartDate));
					AssertEquals("End Date", grid.GetColumnCaption(CusRefTradeGroupSchema.Constants.CR9_EndDate));
				});
			}
		}
	}
}
