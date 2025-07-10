using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseOrderDetailsZPageTest : ZPageTestCase
	{
		#region TestOrderSummaryLinesGridSetup

		public void TestOrderSummaryLinesDecimalPlaces()
		{
			AssertEquals("WhsOrderLine+SupplierPart+OP_CountDecimalPlaces", ((ZCalcEditColumn)TestPage.WhsOrderLinesGridForTesting.Columns[4]).BindToDecimals);
		}

		public void TestOrderSummaryLinesGridSetup()
		{
			AssertOrderSummaryLinesColumnIsInGrid("Product", 0, typeof(ZHyperLinkColumn));
			AssertOrderSummaryLinesColumnIsInGrid("Description", 1, typeof(ZTextEditColumn));
			AssertOrderSummaryLinesColumnIsInGrid("Packs", 2, typeof(ZGroupColumn));
			AssertOrderSummaryLinesColumnIsInGrid("Qty Ordered", 3, typeof(ZCalcEditColumn));
			AssertOrderSummaryLinesColumnIsInGrid("UQ", 4, typeof(ZDropDownListColumn));
			AssertOrderSummaryLinesColumnIsInGrid("Reserved", 5, typeof(ZCalcEditColumn));
		}

		void AssertOrderSummaryLinesColumnIsInGrid(ZString headerText, int columnIndex, Type columnType)
		{
			ZDataGrid whsOrderSummaryLinesGridForTesting = TestPage.WhsOrderSummaryLinesGridForTesting;

			if (columnIndex < whsOrderSummaryLinesGridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (whsOrderSummaryLinesGridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, whsOrderSummaryLinesGridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		#endregion

		#region TestOrderLinesGridSetup

		public void TestDecimalPlaces()
		{
			AssertEquals("WhsOrderLine+SupplierPart+OP_CountDecimalPlaces", ((ZCalcEditColumn)TestPage.WhsOrderLinesGridForTesting.Columns[4]).BindToDecimals);
		}

		public void TestOrderLinesGridSetup()
		{
			AssertColumnIsInGrid("Product", 0, typeof(ZHyperLinkColumn));
			AssertColumnIsInGrid("Description", 1, typeof(ZTextEditColumn));
			AssertColumnIsInGrid("Packs", 2, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("Packs UQ", 3, typeof(ZDropDownListColumn));
			AssertColumnIsInGrid("Qty Ordered", 4, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("UQ", 5, typeof(ZDropDownListColumn));
			AssertColumnIsInGrid("Reserved", 6, typeof(ZHyperLinkColumn));
		}

		void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType)
		{
			ZDataGrid whsOrderLinesGridForTesting = TestPage.WhsOrderLinesGridForTesting;

			if (columnIndex < whsOrderLinesGridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (whsOrderLinesGridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, whsOrderLinesGridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		#endregion

		#region TestTransportRef

		public void TestTransportRef()
		{
			AssertEquals(typeof(ZHyperlink), TestPage.FindControl("TransportRef").GetType());
		}

		#endregion

		#region Implementation

		WarehouseOrderDetailsForTest TestPage
		{
			get { return Page as WarehouseOrderDetailsForTest; }
		}

		protected override ZPage GetNewZPage()
		{
			return new WarehouseOrderDetailsForTest();
		}

		#endregion
	}
}
