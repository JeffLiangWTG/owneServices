using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CusCalculationRulesFilterControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;
				AssertEquals(7, grid.VisibleColumnCount);
			}
		}
		CusCalculationRulesFilterControl GetNewFilterControl()
		{
			var collection = new CusCalculationRuleCollection<CusCalculationRule>(Factory);
			var filterBizO = new CusCalculationRulesFilterBusinessObject();
			return new CusCalculationRulesFilterControl(collection, filterBizO);
		}
	}
}
