using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class ChangeOffBandProcessingStatusValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(ChangeOffBandProcessingStatusValidation), Applicator.Validation.AutoValidationType);
		}

		public void TestValidateStorageOffBandProcessingStatus()
		{
			AssertValidateSelectedOffBandProcessingStatus("BBB", v => v.ValidateSelectedOffBandProcessingStatus(), "Enter a valid New Off Band Processing Status.");
			AssertValidateSelectedOffBandProcessingStatus(StorageOffBandProcessingStatus.Codes.NIQ, v => v.ValidateSelectedOffBandProcessingStatus());
			AssertValidateSelectedOffBandProcessingStatus("", v => v.ValidateSelectedOffBandProcessingStatus(), "Please enter a New Off Band Processing Status.");
		}

		public void TestValidateAll()
		{
			AssertValidateSelectedOffBandProcessingStatus("BBB", v => v.ValidateAll(), "Enter a valid New Off Band Processing Status.");
			AssertValidateSelectedOffBandProcessingStatus(StorageOffBandProcessingStatus.Codes.NIQ, v => v.ValidateAll());
			AssertValidateSelectedOffBandProcessingStatus("", v => v.ValidateAll(), "Please enter a New Off Band Processing Status.");
		}

		#region Implementation

		void AssertValidateSelectedOffBandProcessingStatus(string status, Action<ChangeOffBandProcessingStatusValidation> runValidate,
			string expectedErrorMessage = "")
		{
			Applicator.SelectedOffBandProcessingStatus = status;
			runValidate(Applicator.Validation);
			if (!string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertHasError(Applicator.SelectedOffBandProcessingStatusInfo, expectedErrorMessage);
			}
			else
			{
				AssertNoErrors(Applicator.SelectedOffBandProcessingStatusInfo);
			}
		}

		ChangeOffBandProcessingStatusActionMethodApplicator Applicator => applicator ?? (applicator = new ChangeOffBandProcessingStatusActionMethodApplicator(Factory));
		ChangeOffBandProcessingStatusActionMethodApplicator applicator;

		#endregion
	}
}
