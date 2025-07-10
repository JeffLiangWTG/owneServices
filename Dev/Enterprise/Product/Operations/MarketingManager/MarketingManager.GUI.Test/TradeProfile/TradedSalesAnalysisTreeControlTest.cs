using System.Linq;
using Aga.Controls.Tree;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Integration;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradedSalesAnalysisTreeControlTest : TestCaseWithFactory
	{
		#region Properties

		public void TestShowShowJobValueColumn()
		{
			using (var control = new TradedSalesAnalysisTreeControlForTest())
			{
				control.ShowJobValueColumn = true;
				AssertEquals("ShowJobValueColumn.IsVisible", true, control.JobRevenueColumn_Exposed.IsVisible);
				AssertEquals("ShowJobValueColumn.IsVisible", true, control.JobCostColumn_Exposed.IsVisible);
				AssertEquals("ShowJobValueColumn.IsVisible", true, control.JobProfitColumn_Exposed.IsVisible);

				control.ShowJobValueColumn = false;
				AssertEquals("ShowJobValueColumn.IsVisible", false, control.JobRevenueColumn_Exposed.IsVisible);
				AssertEquals("ShowJobValueColumn.IsVisible", false, control.JobCostColumn_Exposed.IsVisible);
				AssertEquals("ShowJobValueColumn.IsVisible", false, control.JobProfitColumn_Exposed.IsVisible);
			}
		}

		public void TestShowTEQQuantityColumn()
		{
			using (var control = new TradedSalesAnalysisTreeControlForTest())
			{
				control.ProductCode = SystemDefinedSalesProductList.Codes.ForwardingShipment;
				control.SetTEUQuantityColumn();
				AssertEquals("ShowJobValueColumn.IsVisible", true, control.TEUQuantityColumn_Exposed.IsVisible);

				control.ProductCode = SystemDefinedSalesProductList.Codes.LinerAgency;
				control.SetTEUQuantityColumn();
				AssertEquals("ShowJobValueColumn.IsVisible", true, control.TEUQuantityColumn_Exposed.IsVisible);

				control.ProductCode = SystemDefinedSalesProductList.Codes.Transport;
				control.SetTEUQuantityColumn();
				AssertEquals("ShowJobValueColumn.IsVisible", false, control.TEUQuantityColumn_Exposed.IsVisible);
			}
		}

		public void TestProductCode_ShouldUpdateColumnVisibility()
		{
			using (var control = new TradedSalesAnalysisTreeControlForTest())
			{
				var unitCountColumn = control.TreeColumns_Exposed.Single(x => x.Header == "Unit Count");
				var palletCountColumn = control.TreeColumns_Exposed.Single(x => x.Header == "Pallet Count");
				var lineCountColumn = control.TreeColumns_Exposed.Single(x => x.Header == "Line Count");

				control.ProductCode = SystemDefinedSalesProductList.Codes.ForwardingShipment;
				AssertEquals(false, unitCountColumn.IsVisible);
				AssertEquals(false, palletCountColumn.IsVisible);
				AssertEquals(false, lineCountColumn.IsVisible);

				control.ProductCode = SystemDefinedSalesProductList.Codes.Warehouse;
				AssertEquals(true, unitCountColumn.IsVisible);
				AssertEquals(true, palletCountColumn.IsVisible);
				AssertEquals(true, lineCountColumn.IsVisible);
			}
		}

		public void TestSalesAnalysisNodeTextBoxGetValueNullRef()
		{
			var nodeTextBox = new TradedSalesAnalysisTreeControl.SalesAnalysisNodeTextBox();
			AssertNull(nodeTextBox.GetValue(new TreeNodeAdv(null)));
		}

		#endregion
	}
}
