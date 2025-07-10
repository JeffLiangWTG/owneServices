using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentGatePassFilterControlTest : TestCase
	{
		public void TestHideCanadaSpecificNumbersColumns()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (ZForm form = new ZForm())
			using (ShipmentGatePassModule module = new ShipmentGatePassModule())
			{
				var filter = (ShipmentGatePassFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNull("The Column House CCN should not be added", filter.FilteredGrid.GetColumnStyle("CanadaHouseCCN"));

				AssertNull("Column RNSReleaseStatus should not be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseStatus"));
				AssertNull("Column RNSReleaseDate should not be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseDate"));

				AssertNull("Column ArrivalCertificationStatus should not be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationStatus"));
				AssertNull("Column ArrivalCertificationDate should not be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationDate"));
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			using (ZForm form = new ZForm())
			using (ShipmentGatePassModule module = new ShipmentGatePassModule())
			{
				var filter = (ShipmentGatePassFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull("The Column House CCN should be added", filter.FilteredGrid.GetColumnStyle("CanadaHouseCCN"));

				AssertNotNull("Column RNSReleaseStatus should be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseStatus"));
				AssertNotNull("Column RNSReleaseDate should be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseDate"));

				AssertNotNull("Column ArrivalCertificationStatus should be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationStatus"));
				AssertNotNull("Column ArrivalCertificationDate should be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationDate"));
			}
		}

		public void TestGatePassStatusShortColumnIsNotSortable()
		{
			using (var form = new ZForm())
			using (var module = new ShipmentGatePassModule())
			{
				var filter = (ShipmentGatePassFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var column = filter.FilteredGrid.GetColumnStyle("JS_GatePassStatusShort");
				Assert("Column should not be sortable in the grid as it can cause performance problems / SqlException for too many parameters if it is sorted.", !column.IsSortable);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var form = new ZForm())
			using (var module = new ShipmentGatePassModule())
			{
				var filter = (ShipmentGatePassFilterControl)module.EmbeddedControl;
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
			using (var module = new ShipmentGatePassModule())
			{
				var filter = (ShipmentGatePassFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var column = filter.FilteredGrid.GetColumnStyle("Job+JH_TotalProfitRevenueMargin");
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}
	}
}
