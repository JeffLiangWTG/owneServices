using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	sealed class ZPeriodUserControlTest : TestCaseWithFactory
	{
		public void TestShowPeriodControls()
		{
			var filter = new PeriodFilter("Period", JobDeclarationSchema.JE_EntryAuthorisationDate);
			using (var form = new ZFormForTesting(filter))
			{
				form.Show();
				var periodYearEdit = form.PeriodUserControl.Controls.Find("PeriodYearEdit", true)[0];
				var periodMonthEdit = form.PeriodUserControl.Controls.Find("PeriodMonthEdit", true)[0];
				var periodLabel = form.PeriodUserControl.Controls.Find("PeriodLabel", true)[0];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				Assert(periodYearEdit.Visible);
				Assert(periodMonthEdit.Visible);
				Assert(periodLabel.Visible);
				filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
				Assert(!periodYearEdit.Visible);
				Assert(!periodMonthEdit.Visible);
				Assert(!periodLabel.Visible);
				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				Assert(periodYearEdit.Visible);
				Assert(periodMonthEdit.Visible);
				Assert(periodLabel.Visible);
				filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
				Assert(!periodYearEdit.Visible);
				Assert(!periodMonthEdit.Visible);
				Assert(!periodLabel.Visible);
			}
		}

		sealed class ZFormForTesting : ZForm
		{
			public ZFormForTesting(object dataSource) : base(dataSource)
			{
				PeriodUserControl = new ZPeriodUserControl();
				BindingSource.SetBindingMember(PeriodUserControl, ".");
				Controls.Add(PeriodUserControl);
			}

			public ZPeriodUserControl PeriodUserControl;
		}
	}
}
