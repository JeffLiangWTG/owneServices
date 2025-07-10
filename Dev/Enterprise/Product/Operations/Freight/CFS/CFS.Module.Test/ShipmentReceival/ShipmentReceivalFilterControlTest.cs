using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentReceivalFilterControlTest : TestCase
	{
		public void TestHoldReasonColumn()
		{
			using (ZForm form = new ZForm())
			using (ShipmentReceivalModule module = new ShipmentReceivalModule())
			{
				var filter = (ShipmentReceivalFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var holdReason = nameof(CFSShipment.Job) + "+" + nameof(CFSShipment.Job.JH_HoldReason);
				var column = filter.FilteredGrid.GetColumnStyle(holdReason);
				AssertNotNull("HoldReason column should be added", column);
				Assert("HoldReason should be invisible", !column.IsVisible);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var form = new ZForm())
			using (var module = new ShipmentReceivalModule())
			{
				var filter = (ShipmentReceivalFilterControl)module.EmbeddedControl;
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
			using (var module = new ShipmentReceivalModule())
			{
				var filter = (ShipmentReceivalFilterControl)module.EmbeddedControl;
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
			using (ShipmentReceivalModule module = new ShipmentReceivalModule())
			{
				var filter = (ShipmentReceivalFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNull("The Column House CCN should not be added", filter.FilteredGrid.GetColumnStyle("CanadaHouseCCN"));
				AssertNull("The Column Load List CCN should not be added", filter.FilteredGrid.GetColumnStyle("CanadaLoadListCCN"));
				AssertNull("The Column Load List PCN should not be added", filter.FilteredGrid.GetColumnStyle("CanadaLoadListPCN"));

				AssertNull("Column RNSReleaseStatus should not be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseStatus"));
				AssertNull("Column RNSReleaseDate should not be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseDate"));

				AssertNull("Column ArrivalCertificationStatus should not be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationStatus"));
				AssertNull("Column ArrivalCertificationDate should not be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationDate"));
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			using (ZForm form = new ZForm())
			using (ShipmentReceivalModule module = new ShipmentReceivalModule())
			{
				var filter = (ShipmentReceivalFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull("The Column House CCN should be added", filter.FilteredGrid.GetColumnStyle("CanadaHouseCCN"));
				AssertNotNull("The Column Load List CCN should be added", filter.FilteredGrid.GetColumnStyle("CanadaLoadListCCN"));
				AssertNotNull("The Column Load List PCN should be added", filter.FilteredGrid.GetColumnStyle("CanadaLoadListPCN"));

				AssertNotNull("Column RNSReleaseStatus should be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseStatus"));
				AssertNotNull("Column RNSReleaseDate should be added", filter.FilteredGrid.GetColumnStyle("RNSReleaseDate"));

				AssertNotNull("Column ArrivalCertificationStatus should be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationStatus"));
				AssertNotNull("Column ArrivalCertificationDate should be added", filter.FilteredGrid.GetColumnStyle("ArrivalCertificationDate"));
			}
		}
	}
}
