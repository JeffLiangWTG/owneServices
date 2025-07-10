using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Business.Testing
{
	[TestFixture]
	public class DtbTransportDocAddressValidationDummyTest
	{
		[Test]
		public void AlwaysPasses()
		{
			Assert.Pass("This test always passes, it is just a dummy test to satisfy the requirement of having at least one NUnit test in the assembly.");
		}
	}

	class DtbTransportDocAddressValidationTest : TestCaseWithFactory
	{
		#region TestValidateOrganisationPK

		public void TestValidateOrganisationPK()
		{
			var nonDebtorOrg = Helper.CreateOrganisation("O1");
			var debtorOrg = Helper.CreateOrganisation("O2", true);
			var crbWithoutOrg = CreateJobDocAddress(DocAddressTypes.Codes.ClientRequestedBillingParty, ZGuid.Empty);
			var crbNonDebtor = CreateJobDocAddress(DocAddressTypes.Codes.ClientRequestedBillingParty, nonDebtorOrg.MainAddress.PK);
			var crbDebtor = CreateJobDocAddress(DocAddressTypes.Codes.ClientRequestedBillingParty, debtorOrg.MainAddress.PK);
			var nonCRB = CreateJobDocAddress(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, debtorOrg.MainAddress.PK);

			new DtbTransportDocAddressValidation(crbWithoutOrg).ValidateOrganisationPK();
			new DtbTransportDocAddressValidation(crbNonDebtor).ValidateOrganisationPK();
			new DtbTransportDocAddressValidation(crbDebtor).ValidateOrganisationPK();
			new DtbTransportDocAddressValidation(nonCRB).ValidateOrganisationPK();
			AssertNoWarnings(crbWithoutOrg.OrganisationPKInfo);
			AssertHasWarning(crbNonDebtor.OrganisationPKInfo, "The Billing Party should be marked as a Receivables organization.");
			AssertNoWarnings(crbDebtor.OrganisationPKInfo);
			AssertNoWarnings(nonCRB.OrganisationPKInfo);
		}

		#endregion

		#region TestValidateE2_OA_Address

		public void TestValidateE2_OA_Address()
		{
			var nonDebtorOrg = Helper.CreateOrganisation("O1");
			var debtorOrg = Helper.CreateOrganisation("O2", true);
			var crbWithoutOrg = CreateJobDocAddress(DocAddressTypes.Codes.ClientRequestedBillingParty, ZGuid.Empty);
			var crbNonDebtor = CreateJobDocAddress(DocAddressTypes.Codes.ClientRequestedBillingParty, nonDebtorOrg.MainAddress.PK);
			var crbDebtor = CreateJobDocAddress(DocAddressTypes.Codes.ClientRequestedBillingParty, debtorOrg.MainAddress.PK);
			var nonCRB = CreateJobDocAddress(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, debtorOrg.MainAddress.PK);
			var nonCRBWithoutOrg = CreateJobDocAddress(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, ZGuid.Empty);

			new DtbTransportDocAddressValidation(crbWithoutOrg).ValidateE2_OA_Address();
			new DtbTransportDocAddressValidation(crbNonDebtor).ValidateE2_OA_Address();
			new DtbTransportDocAddressValidation(crbDebtor).ValidateE2_OA_Address();
			new DtbTransportDocAddressValidation(nonCRB).ValidateE2_OA_Address();
			new DtbTransportDocAddressValidation(nonCRBWithoutOrg).ValidateE2_OA_Address();

			AssertNoErrors(crbWithoutOrg.E2_OA_AddressInfo);
			AssertHasWarning(crbNonDebtor.E2_OA_AddressInfo, "The Billing Party should be marked as a Receivables organization.");
			AssertNoWarnings(crbDebtor.E2_OA_AddressInfo);
			AssertNoWarnings(nonCRB.E2_OA_AddressInfo);
			AssertNoWarnings(nonCRBWithoutOrg.E2_OA_AddressInfo);
		}

		#endregion

		#region Implementation

		JobDocAddress CreateJobDocAddress(string addressType, ZGuid addressPk)
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressType = addressType;
			docAddress.E2_OA_Address = addressPk;

			return docAddress;
		}

		TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = new TransportCommonTestHelper(Factory)); }
		}
		TransportCommonTestHelper helper;

		#endregion
	}
}
