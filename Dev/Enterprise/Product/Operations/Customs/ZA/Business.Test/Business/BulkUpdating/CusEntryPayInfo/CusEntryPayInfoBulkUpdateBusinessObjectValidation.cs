using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryPayInfoBulkUpdateBusinessObjectValidation : BusinessObjectValidationTestCase
	{
		public void TestReceiptNumber()
		{
			var parent = new CusEntryPayInfoBulkUpdateBusinessObject(Factory);
			var error = EnglishCharactersValidation.GetNotificationMessage(parent.ReceiptNumberInfo);
			parent.ReceiptNumber = "漢字";
			AssertHasError(parent.ReceiptNumberInfo, error);
			parent.ReceiptNumber = "2g1f3s";
			AssertNoError(parent.ReceiptNumberInfo, error);
			parent.ReceiptDate = ZDate.Today;
			parent.ReceiptNumber = ZString.Empty;
			AssertHasError(parent.ReceiptNumberInfo, ValidationConstants.CusEntryPayInfo.ReceiptNumberRequiredWithDate);
		}

		public void TestReceiptDate()
		{
			var parent = new CusEntryPayInfoBulkUpdateBusinessObject(Factory);
			parent.ReceiptNumber = "REF001";
			parent.Validation.ValidateReceiptDate();
			AssertHasError(parent.ReceiptDateInfo, ValidationConstants.CusEntryPayInfo.ReceiptDateRequiredWithNumber);
			parent.ReceiptDate = ZDate.Today.AddDays(2);
			AssertHasError(parent.ReceiptDateInfo, ValidationConstants.CusEntryPayInfo.ReceiptDateCannotBeGreaterThantoday);
			parent.ReceiptDate = ZDate.Today;
			AssertNoErrors(parent.ReceiptDateInfo);
		}
	}
}
