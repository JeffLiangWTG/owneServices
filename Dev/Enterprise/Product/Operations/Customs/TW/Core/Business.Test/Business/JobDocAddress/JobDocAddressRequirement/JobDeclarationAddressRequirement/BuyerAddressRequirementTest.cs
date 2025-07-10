using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BuyerAddressRequirement))]
	sealed class BuyerAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyName()
		{
			var warningMessage = "You have not entered a Buyer Company Name.";
			buyerDocumentaryAddress.E2_AddressOverride = true;
			ValidationTestHelper.AssertWarningIfNotEntered(buyerDocumentaryAddress.E2_CompanyNameInfo, warningMessage);

			buyerDocumentaryAddress.E2_AddressOverride = false;
			buyerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning("Organisation not set", buyerDocumentaryAddress.OrganisationPKInfo, warningMessage);
		}

		public void TestCheckE2_Address1()
		{
			buyerDocumentaryAddress.E2_AddressOverride = true;
			buyerDocumentaryAddress.Validation.ValidateE2_Address1();
			AssertNoErrors(buyerDocumentaryAddress.E2_Address1Info);
		}

		protected override void SetUp()
		{
			base.SetUp();
			buyerDocumentaryAddress = Factory.New<JobDeclaration>().Invoices.AddNew().BuyerDocumentaryAddress;
		}

		TWJobDocAddress buyerDocumentaryAddress;
	}
}
