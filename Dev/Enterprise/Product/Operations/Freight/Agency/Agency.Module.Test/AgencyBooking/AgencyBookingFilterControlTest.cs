using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Module.Testing
{
	sealed class AgencyBookingFilterControlTest : BaseAgencyTest
	{
		public void TestHoldReasonColumn()
		{
			using (var filterControl = new AgencyBookingFilterControl(new AgencyBookingCollection(Factory), new AgencyBookingFilterStrip()))
			{
				var holdReason = nameof(AgencyBooking.Job) + "+" + nameof(AgencyBooking.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		public void TestAdditionalReferenceColumn()
		{
			using (var filterControl = new AgencyBookingFilterControl())
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any((c) => c.ColumnName == "NumbersAsString" && !c.IsVisible);
				Assert("Additional Reference column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = new AgencyBookingFilterControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle("Job+JH_ProfitLossReasonCode");
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = new AgencyBookingFilterControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle("Job+JH_TotalProfitRevenueMargin");
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestWorkflowCustomFieldsColumn()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			template.P0_Name = "WF Agency Booking Test Workflow";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Integer;
			column.XC_Name = "WF Custom Field";

			Factory.Save();

			var collection = new AgencyBookingCollection(Factory);
			var filterBusinessObject = new AgencyBookingFilterStrip();

			using (var filterControl = new AgencyBookingFilterControl(collection, filterBusinessObject))
			{
				filterControl.Show();
				var columnStyle = filterControl.Grid.GetColumnStyle("__WF CUSTOM FIELD__prop__ZInt");
				AssertNotNull(columnStyle);
				AssertEquals("Caption", "WF Custom Field", columnStyle.Caption);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
				AssertEquals("ReadOnly", true, columnStyle.IsReadOnly);
			}
		}
	}
}
