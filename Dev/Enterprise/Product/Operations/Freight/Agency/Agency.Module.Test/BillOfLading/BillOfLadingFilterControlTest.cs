using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class BillOfLadingFilterControlTest : BaseAgencyTest
	{
		public void TestAdditionalReferenceColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any((c) => c.ColumnName == "NumbersAsString" && !c.IsVisible);
				Assert("Additional Reference column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestEstimatedTimeDepartureColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var columnInfo = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "Sailing+JX_JA_E_DEP");
				AssertNotNull("ETD column should exist", columnInfo);
				AssertEquals("ETD column caption", "ETD", columnInfo.CaptionResourceString.Caption);
				AssertEquals("ETD column default visibility is false", false, columnInfo.IsVisible);
			}
		}

		public void TestEstimatedTimeArrivaleColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var columnInfo = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "Sailing+JX_JB_E_ARV");
				AssertNotNull("ETA column should exist", columnInfo);
				AssertEquals("ETA column caption", "ETA", columnInfo.CaptionResourceString.Caption);
				AssertEquals("ETA column default visibility is false", false, columnInfo.IsVisible);
			}
		}

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle("Job+JH_ProfitLossReasonCode");
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = GetFilterControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle("Job+JH_TotalProfitRevenueMargin");
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestWorkflowCustomFieldsColumn()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;
			template.P0_Name = "WF Bill Of Lading Test Workflow";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Datetime;
			column.XC_Name = "WF Custom Field";

			Factory.Save();

			var collection = new BillOfLadingCollection(Factory);
			var filterBusinessObject = new BillOfLadingFilterStrip();

			using (var filterControl = new BillOfLadingFilterControl(collection, filterBusinessObject))
			{
				filterControl.Show();
				var columnStyle = filterControl.Grid.GetColumnStyle("__WF CUSTOM FIELD__prop__ZDateTime");
				AssertNotNull(columnStyle);
				AssertEquals("Caption", "WF Custom Field", columnStyle.Caption);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
				AssertEquals("ReadOnly", true, columnStyle.IsReadOnly);
			}
		}

		BillOfLadingFilterControl GetFilterControl() => new (new BillOfLadingCollection(Factory), new BillOfLadingFilterStrip());
	}
}
