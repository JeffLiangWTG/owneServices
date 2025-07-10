using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefNMFCValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFN_ItemNo()
		{
			RefNMFC nmfc = Factory.New<RefNMFC>();
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_ItemNoInfo, "Please enter an " + nmfc.FN_ItemNoInfo.Description + ".");

			nmfc.FN_ItemNo = "A123";
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_ItemNoInfo, "An Item No must consist of only numbers.");

			nmfc.FN_ItemNo = "123";
			AssertNoErrors(nmfc.FN_ItemNoInfo);
		}

		public void TestCheckFN_Class()
		{
			RefNMFC nmfc = Factory.New<RefNMFC>();
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_ClassInfo, "Please enter a " + nmfc.FN_ClassInfo.Description + ".");

			nmfc.FN_Class = "A123";
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_ClassInfo, "A Class must be a numeric value.");

			nmfc.FN_Class = "123";
			AssertNoErrors(nmfc.FN_ClassInfo);

			nmfc.FN_Class = "A123";
			nmfc.FN_Class = "7.5";
			AssertNoErrors(nmfc.FN_ClassInfo);
		}

		public void TestCheckFN_Code()
		{
			RefNMFC nmfcFirst = Factory.New<RefNMFC>();
			nmfcFirst.FN_Code = "123|456";
			RefNMFC nmfc = Factory.New<RefNMFC>();
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_CodeInfo, "Please enter an " + nmfc.FN_CodeInfo.Description + ".");

			nmfc.FN_Code = "123";
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_CodeInfo, "The Article Code needs to be created as Item No|Class.");

			nmfc.FN_ItemNo = "123";
			nmfc.FN_Class = "123";
			nmfc.Validation.ValidateAll();
			AssertNoErrors(nmfc.FN_CodeInfo);

			nmfc.FN_ItemNo = "123";
			nmfc.FN_Class = "456";
			nmfc.Validation.ValidateAll();
			AssertHasError(nmfc.FN_CodeInfo, "This article with corresponding Item No and Class already exists.");
		}
	}
}
