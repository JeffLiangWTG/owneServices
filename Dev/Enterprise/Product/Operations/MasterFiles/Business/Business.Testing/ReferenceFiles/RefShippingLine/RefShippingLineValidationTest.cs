using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefShippingLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRSL_IsActive()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsSystem = false;
			shippingLine.RSL_IsActive = false;
			AssertNoErrors(shippingLine);

			shippingLine.RSL_IsSystem = false;
			shippingLine.RSL_IsActive = true;
			AssertHasError(shippingLine.RSL_IsActiveInfo, "All non-system created reference files will be marked as In-Active. If you wish to register a new carrier, please raise a CR8 service request.");

			shippingLine.RSL_IsSystem = true;
			shippingLine.RSL_IsActive = true;
			AssertNoErrors(shippingLine);
		}

		public void TestCheckRSL_StandardCarrierAlphaCode()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			AssertNoErrors(shippingLine);

			shippingLine.RSL_StandardCarrierAlphaCode = "中文字符";
			AssertHasError(shippingLine.RSL_StandardCarrierAlphaCodeInfo, "Standard Carrier Alpha Code only accepts English language characters.");

			shippingLine.RSL_StandardCarrierAlphaCode = string.Empty;
			AssertNoError(shippingLine.RSL_StandardCarrierAlphaCodeInfo, "Standard Carrier Alpha Code must have 4 characters.");

			shippingLine.RSL_StandardCarrierAlphaCode = "ABC";
			AssertHasError(shippingLine.RSL_StandardCarrierAlphaCodeInfo, "Standard Carrier Alpha Code must have 4 characters.");

			shippingLine.RSL_StandardCarrierAlphaCode = "Test";
			AssertNoErrors(shippingLine);

			shippingLine.RSL_IsSystem = true;
			shippingLine.RSL_StandardCarrierAlphaCode = string.Empty;
			AssertHasError(shippingLine.RSL_StandardCarrierAlphaCodeInfo, "Standard Carrier Alpha Code must have 4 characters.");
		}

		public void TestCheckRSL_CargoWiseOneCodeOnlyAccept4EnglishCharacters()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			AssertNoErrors(shippingLine);

			shippingLine.RSL_CargoWiseOneCode = "中文字符";
			AssertHasError(shippingLine.RSL_CargoWiseOneCodeInfo, "CargoWise Code only accepts English language characters.");

			shippingLine.RSL_CargoWiseOneCode = string.Empty;
			AssertHasError(shippingLine.RSL_CargoWiseOneCodeInfo, "CargoWise Code must have 4 characters.");

			shippingLine.RSL_CargoWiseOneCode = "ABC";
			AssertHasError(shippingLine.RSL_CargoWiseOneCodeInfo, "CargoWise Code must have 4 characters.");

			shippingLine.RSL_CargoWiseOneCode = "Test";
			AssertNoErrors(shippingLine);
		}

		public void TestRSL_CarrierName()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			AssertNoErrors(shippingLine);

			shippingLine.RSL_CarrierName = string.Empty;
			AssertHasError(shippingLine.RSL_CarrierNameInfo, "Please enter a Carrier Name.");

			shippingLine.RSL_CarrierName = "TestName";
			AssertNoErrors(shippingLine);
		}

		public void TestRSL_CargoWiseOneCodeNotEmpty()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			AssertNoErrors(shippingLine);

			shippingLine.RSL_CargoWiseOneCode = string.Empty;
			AssertHasError(shippingLine.RSL_CargoWiseOneCodeInfo, "Please enter a CargoWise Code.");

			shippingLine.RSL_CargoWiseOneCode = "COD1";
			AssertNoErrors(shippingLine);
		}

		public void TestRSL_CargoWiseOneCodeNotDuplicate()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "2674";
			AssertNoErrors("Prepare original data.", shippingLine);
			Factory.Save();

			shippingLine.RSL_CargoWiseOneCode = "2674";
			AssertNoErrors("C1C of original data set to the same again should not be considered as a duplicate one.", shippingLine);

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_CargoWiseOneCode = "3aed";
			AssertNoErrors("New data with different C1C.", shippingLine2);
			Factory.Save();

			var shippingLine3 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine3.RSL_CargoWiseOneCode = "2674";
			AssertHasError("New data with duplicate C1C.", shippingLine3.RSL_CargoWiseOneCodeInfo, "CargoWise Code duplicates are allowed only if the shipping line is inactive.");
		}

		public void TestRSL_CargoWiseOneCode_AllowDuplicateShippingLineIfC1CIsMarkedInactive()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "2674";
			AssertNoErrors("Prepare original data.", shippingLine);
			Factory.Save();

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_CargoWiseOneCode = "2674";
			AssertHasError("New data with duplicate C1C.", shippingLine2.RSL_CargoWiseOneCodeInfo, "CargoWise Code duplicates are allowed only if the shipping line is inactive.");

			shippingLine2.RSL_IsActive = false;
			shippingLine2.Validation.ValidateRSL_CargoWiseOneCode();
			AssertNoErrors("New data with different C1C.", shippingLine2);
			Factory.Save();
		}

		public void TestAllowMultipleDuplicateShippingLineIfC1CIsMarkedInactive()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "2674";
			shippingLine.RSL_IsActive = false;
			AssertNoErrors("Prepare original data.", shippingLine);
			Factory.Save();

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_CargoWiseOneCode = "2674";
			shippingLine2.RSL_IsActive = false;
			AssertNoErrors("New data with duplicate C1C and inactive shipping line.", shippingLine);
			Factory.Save();
		}
	}
}
