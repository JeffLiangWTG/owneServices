using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class LoadListConsolFilterControlTest : TestCase
	{
		public void TestHoldReasonColumn()
		{
			using (ZForm form = new ZForm())
			using (LoadListConsolModule module = new LoadListConsolModule())
			{
				var filter = (LoadListConsolFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var holdReason = nameof(CFSLoadListConsol.Job) + "+" + nameof(CFSLoadListConsol.Job.JH_HoldReason);
				var column = filter.FilteredGrid.GetColumnStyle(holdReason);
				AssertNotNull("HoldReason column should be added", column);
				Assert("HoldReason should be invisible", !column.IsVisible);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var form = new ZForm())
			using (var module = new LoadListConsolModule())
			{
				var filter = (LoadListConsolFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var column = filter.FilteredGrid.GetColumnStyle("Job+JH_ProfitLossReasonCode");
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var form = new ZForm())
			using (var module = new LoadListConsolModule())
			{
				var filter = (LoadListConsolFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var column = filter.FilteredGrid.GetColumnStyle("Job+JH_TotalProfitRevenueMargin");
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestHideCanadaSpecificNumbersColumns()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (ZForm form = new ZForm())
			using (LoadListConsolModule module = new LoadListConsolModule())
			{
				var filter = (LoadListConsolFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNull("The CCN columns should not be added", filter.FilteredGrid.GetColumnStyle("CanadaCCNNumber"));
				AssertNull("The PCN columns should not be added", filter.FilteredGrid.GetColumnStyle("CanadaPCNNumber"));
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			using (ZForm form = new ZForm())
			using (LoadListConsolModule module = new LoadListConsolModule())
			{
				var filter = (LoadListConsolFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull("The CCN columns should be added", filter.FilteredGrid.GetColumnStyle("CanadaCCNNumber"));
				AssertNotNull("The PCN columns should be added", filter.FilteredGrid.GetColumnStyle("CanadaPCNNumber"));
			}
		}
	}
}
