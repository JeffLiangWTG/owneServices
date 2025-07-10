using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SelectiveMessageErrorCollectorTest : TestCaseWithFactory
	{
		public void TestIncludeChildrenOfPassedInSeletedChildren()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "XXX";
			Assert("PreCondition", declaration.HasMessageErrors);
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = "XXX";
			Assert("PreCondition", bill.HasMessageErrors);
			var invoice = Factory.New<TestInvoice>();
			invoice.JZ_JE = declaration.PK;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			declaration.ResumeApportionment();
			((InvoiceHeaderValidation)invoice.Validation).ValidateJZ_Calc_Balance();
			Assert("PreCondition", invoice.HasMessageErrors);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsQuantity = 10m;
			Assert("PreCondition", invoiceLine.HasMessageErrors);
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			invoiceLineCharge.J7_ChargeType = "XXX";
			Assert("PreCondition", invoiceLineCharge.HasMessageErrors);

			var collector = new SelectiveMessageErrorCollector(declaration, new BusinessObject[] { invoice });
			var messageErrors = collector.ToUniqueMessageListString();
			AssertContains(declaration.JE_MessageTypeInfo.HumanReadableName + ": " + declaration.JE_MessageTypeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			AssertNotContains(bill.CU_BillTypeInfo.HumanReadableName + ": " + bill.CU_BillTypeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			AssertContains(invoice.JZ_Calc_BalanceInfo.HumanReadableName + ": " + invoice.JZ_Calc_BalanceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			AssertContains(invoiceLine.JI_CustomsQuantityInfo.HumanReadableName + ": " + invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			AssertContains(invoiceLineCharge.J7_ChargeTypeInfo.HumanReadableName + ": " + invoiceLineCharge.J7_ChargeTypeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);

			var collector2 = new SelectiveMessageErrorCollector(declaration, new BusinessObject[] { bill });
			var messageErrors2 = collector2.ToUniqueMessageListString();
			AssertContains(declaration.JE_MessageTypeInfo.HumanReadableName + ": " + declaration.JE_MessageTypeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors2);
			AssertContains(bill.CU_BillTypeInfo.HumanReadableName + ": " + bill.CU_BillTypeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors2);
			AssertNotContains(invoice.JZ_Calc_BalanceInfo.HumanReadableName + ": " + invoice.JZ_Calc_BalanceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors2);
			AssertNotContains(invoiceLine.JI_CustomsQuantityInfo.HumanReadableName + ": " + invoiceLine.JI_CustomsQuantityInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors2);
			AssertNotContains(invoiceLineCharge.J7_ChargeTypeInfo.HumanReadableName + ": " + invoiceLineCharge.J7_ChargeTypeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors2);
		}

		class TestInvoice : BaseJobComInvoiceHeader
		{
			public TestInvoice(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
			{
				var result = base.CreateNewJobComInvoiceLineCollection();
				RegisterEditableChildObject(result);
				return result;
			}
		}
	}
}
