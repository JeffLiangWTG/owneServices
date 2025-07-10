using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestValidateCH_BGMReference()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CusEntryHeader;
			entry.EntryNumber = "12345678";
			entry.Messages.AddNew();
			entry.IsActive = false;
			entry.Validation.ValidateCH_BGMReference();
			AssertNoNotifications(entry.CH_BGMReferenceInfo);
		}

		public void TestTotalAmountReturnedPrecision()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CusEntryHeader;
			entry.EntryNumber = "38132494";
			entry.CH_TotalAmountReturned = new ZDecimal(9999999999.99);
			AssertNoNotifications("Total Amount Returned can be increased to 10 digits and 2 decimals", entry.CH_TotalAmountReturnedInfo);
			entry.CH_TotalAmountReturned = new ZDecimal(10000000000.00);
			AssertHasError(entry.CH_TotalAmountReturnedInfo, "The number 10,000,000,000 is too large, the maximum value allowed for selection is 9,999,999,999.99.");
		}
	}
}
