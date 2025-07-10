using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EnhancedSelectiveMessageErrorCollectorTest : TestCaseWithFactory
	{
		public void TestMessageErrorsAreCollectedCorrectly()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "XXX";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = "XXX";
			var invoice1 = Factory.New<TestInvoice>();
			invoice1.JZ_JE = declaration.PK;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice1.JZ_IncoTerm = "ZZZ";
			var invoice2 = Factory.New<TestInvoice>();
			invoice2.JZ_JE = declaration.PK;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_RX_NKInvoice_Currency = "ZZZ";

			var collector = new EnhancedSelectiveMessageErrorCollector(declaration, new BusinessObject[] { bill }, new BusinessObject[] { invoice1, invoice2 });
			var messageErrors = collector.ToUniqueMessageListString();
			var declarationTypeMessageError = declaration.JE_MessageTypeInfo.HumanReadableName + ":";
			var billTypeMessageError = bill.CU_BillTypeInfo.HumanReadableName + ":";
			var invoice1IncoTermMessageError = invoice1.JZ_IncoTermInfo.HumanReadableName + ":";
			var invoice2CurrencyMessageError = invoice2.JZ_RX_NKInvoice_CurrencyInfo.HumanReadableName + ":";

			CombineAssertions(() =>
			{
				// Both invoices result in MessageErrors because both are passed to EnhancedSelectiveMessageErrorCollector
				AssertContains("Declaration has message error", declarationTypeMessageError, messageErrors);
				AssertContains("Bill has message error", billTypeMessageError, messageErrors);
				AssertContains("Invoice 1 Amount message error", invoice1IncoTermMessageError, messageErrors);
				AssertContains("Invoice 2 Currency message error", invoice2CurrencyMessageError, messageErrors);

				// Only invoice2 results in MessageError because only invoice2 is passed to EnhancedSelectiveMessageErrorCollector
				collector = new EnhancedSelectiveMessageErrorCollector(declaration, new BusinessObject[] { bill }, new BusinessObject[] { invoice2 });
				messageErrors = collector.ToUniqueMessageListString();
				AssertContains("Declaration still has message error", declarationTypeMessageError, messageErrors);
				AssertContains("Bill still has message error", billTypeMessageError, messageErrors);
				AssertNotContains("Invoice 1 not selected", invoice1IncoTermMessageError, messageErrors);
				AssertContains("Invoice 2 Currency is selected", invoice2CurrencyMessageError, messageErrors);
			});
		}
	}
}
