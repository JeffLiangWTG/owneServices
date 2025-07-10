using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryPayInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestC9_PaymentReference()
		{
			var parent = Factory.New<CusEntryPayInfo>();
			parent.C9_ReceiptDate = ZDate.Today;
			parent.C9_TransactionType = "OTH";
			parent.Validation.ValidateC9_PaymentReference();
			AssertNoNotifications(parent.C9_PaymentReferenceInfo);
			parent.C9_TransactionType = "VAT";
			parent.Validation.ValidateC9_PaymentReference();
			AssertHasError(parent.C9_PaymentReferenceInfo, ValidationConstants.CusEntryPayInfo.ReceiptNumberRequiredWithDate);
		}

		public void TestC9_ReceiptDate()
		{
			CombineAssertions("For VAT", () =>
			{
				var parent = Factory.New<CusEntryPayInfo>();
				parent.C9_TransactionType = "VAT";
				parent.C9_PaymentReference = "REF001";
				parent.Validation.ValidateC9_ReceiptDate();
				AssertHasError(parent.C9_ReceiptDateInfo, ValidationConstants.CusEntryPayInfo.ReceiptDateRequiredWithNumber);
				parent.C9_ReceiptDate = ZDate.Today.AddDays(2);
				AssertHasError(parent.C9_ReceiptDateInfo, ValidationConstants.CusEntryPayInfo.ReceiptDateCannotBeGreaterThantoday);
				parent.C9_ReceiptDate = ZDate.Today;
				AssertNoErrors(parent.C9_ReceiptDateInfo);
			});
			CombineAssertions("For Others", () =>
			{
				var parent = Factory.New<CusEntryPayInfo>();
				parent.C9_TransactionType = "OTH";
				parent.C9_PaymentReference = "REF001";
				parent.Validation.ValidateC9_ReceiptDate();
				AssertNoNotifications(parent.C9_ReceiptDateInfo);
				parent.C9_ReceiptDate = ZDate.Today.AddDays(2);
				AssertNoNotifications(parent.C9_ReceiptDateInfo);
				parent.C9_ReceiptDate = ZDate.Today;
				AssertNoNotifications(parent.C9_ReceiptDateInfo);
			});
		}
	}
}
