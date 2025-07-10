using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketDocAddressValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestWhsDocketDocAddressValidation_ValidateAll

		public void TestWhsDocketDocAddressValidation_ValidateAll()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var validation = new WhsDocketDocAddressValidation(jobDocAddress);
			AssertExceptionThrown<InvalidOperationException>(
				"Should not be calling ValidateAll() on PiggyBacked Validation.",
				() => validation.ValidateAll()
			);
		}

		#endregion

		#region TestWhsDocketDocAddressValidation_CheckOrganisationPK

		public void TestWhsDocketDocAddressValidation_CheckOrganisationPK()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var validation = new WhsDocketDocAddressValidation(jobDocAddress);

			jobDocAddress.Organisation.OH_IsActive = true;
			validation.ValidateOrganisationPK();
			AssertNoError(jobDocAddress.OrganisationPKInfo, "This Organization is not active.");

			jobDocAddress.Organisation.OH_IsActive = false;
			validation.ValidateOrganisationPK();
			AssertHasError(jobDocAddress.OrganisationPKInfo, "This Organization is not active.");
		}

		#endregion
	}
}
