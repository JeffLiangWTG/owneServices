namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CashAdvanceDefaultingChargeGroupValidationTest : AccJobConfigPivotValidationTest
	{
		public void TestCheckJCT_Code()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			pivot.JCT_Code = "";
			AssertHasError(pivot.JCT_CodeInfo, "Please enter a Charge Group Code.");
			pivot.JCT_Code = "123";
			AssertHasError(pivot.JCT_CodeInfo, "Enter a valid Charge Group Code.");
			pivot.JCT_Code = "FRT";
			AssertNoError(pivot.JCT_CodeInfo, "Enter a valid Charge Group Code.");
		}

		public void TestIsDuplicate()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			var collection = new CashAdvanceDefaultingChargeGroupCollection(config);
			var bizo = Factory.New<CashAdvanceDefaultingChargeGroup>();
			bizo.JCT_Code = "FRT";
			collection.Add(bizo);
			var duplicate = Factory.New<CashAdvanceDefaultingChargeGroup>();
			collection.Add(duplicate);

			duplicate.JCT_Code = "FRT";
			AssertHasRowError(duplicate, "Another record already exists for the same Charge Group.");
			duplicate.JCT_Code = "BRK";
			AssertNoRowError(duplicate, "Another record already exists for the same Charge Group.");
		}
	}
}
