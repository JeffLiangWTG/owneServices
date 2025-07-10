using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(WarehouseOperatorTransactionsForm))]
	class WarehouseOperatorTransactionsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new WarehouseOperatorTransactionsForm(Factory.New<CusWHSOperatorTransaction>());

		public void TestGridVisibility()
		{
			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;

			using (var form = new WarehouseOperatorTransactionsFormForTest(order))
			{
				form.Show();
				var ordersGrid = form.GroupBox.Controls.Find("ordersGrid", searchAllChildren: true).FirstOrDefault() as ZArchitecture.ZGrid;
				var receiptsGrid = form.GroupBox.Controls.Find("receiptsGrid", searchAllChildren: true).FirstOrDefault();

				AssertEquals("Orders Grid should be visible", expected: true, ordersGrid.Visible);
				var columnStyle = ordersGrid.GetColumnStyle("Receipt+WOT_CustomsEntryNumber");
				Assert("Order Customs Reference IsUnavailable", !columnStyle.IsUnavailable);
				AssertEquals("Receipt Customs Reference", columnStyle.CaptionResourceString.Caption);
				Assert(columnStyle.IsVisible);
				AssertEquals("Receipts Grid should not be visible", expected: false, receiptsGrid.Visible);
			}

			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;

			using (var form = new WarehouseOperatorTransactionsFormForTest(order))
			{
				form.Show();
				var ordersGrid = form.GroupBox.Controls.Find("ordersGrid", true).FirstOrDefault();
				var receiptsGrid = form.GroupBox.Controls.Find("receiptsGrid", true).FirstOrDefault() as ZArchitecture.ZGrid;

				AssertEquals("Orders Grid should not be visible", expected: false, ordersGrid.Visible);
				AssertEquals("Receipts Grid should be visible", expected: true, receiptsGrid.Visible);
				var columnStyle = receiptsGrid.GetColumnStyle("Order+WOT_CustomsEntryNumber");
				Assert("Receipt Customs Reference IsUnavailable", !columnStyle.IsUnavailable);
				AssertEquals("Order Customs Reference", columnStyle.CaptionResourceString.Caption);
				Assert(columnStyle.IsVisible);
			}
		}

		public void TestOrderCancellationAuditFields()
		{
			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;

			using (var form = new WarehouseOperatorTransactionsFormForTest(order))
			{
				form.Show();
				var cancelledDate = form.Controls.Find("CancelledDateEdit", searchAllChildren: true).FirstOrDefault();
				var cancelledBy = form.Controls.Find("CancelledByTextBox", searchAllChildren: true).FirstOrDefault();

				AssertEquals("Cancelled Date should not be visible when order does not have cancellation status", expected: false, cancelledDate.Visible);
				AssertEquals("Cancelled By should not be visible when order does not have cancellation status", expected: false, cancelledBy.Visible);
			}

			order.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CAN;

			using (var form = new WarehouseOperatorTransactionsFormForTest(order))
			{
				form.Show();
				var cancelledDate = form.Controls.Find("CancelledDateEdit", searchAllChildren: true).FirstOrDefault();
				var cancelledBy = form.Controls.Find("CancelledByTextBox", searchAllChildren: true).FirstOrDefault();

				AssertEquals("Cancelled Date should be visible when order has cancellation status", expected: true, cancelledDate.Visible);
				AssertEquals("Cancelled By should be visible when order has cancellation status", expected: true, cancelledBy.Visible);
			}

			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;

			using (var form = new WarehouseOperatorTransactionsFormForTest(order))
			{
				form.Show();
				var cancelledDate = form.Controls.Find("CancelledDateEdit", searchAllChildren: true).FirstOrDefault();
				var cancelledBy = form.Controls.Find("CancelledByTextBox", searchAllChildren: true).FirstOrDefault();

				AssertEquals("Cancelled Date should not be visible for a receipt", expected: false, cancelledDate.Visible);
				AssertEquals("Cancelled By should not be visible for a receipt", expected: false, cancelledBy.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		class WarehouseOperatorTransactionsFormForTest : WarehouseOperatorTransactionsForm
		{
			public WarehouseOperatorTransactionsFormForTest(CusWHSOperatorTransaction businessEntity) : base(businessEntity)
			{
			}

			public ZGroupBox GroupBox => zGroupBox2;
		}
	}
}
