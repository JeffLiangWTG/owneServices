using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TraderAtDestinationProviderTest : Customs.Business.Testing.DataProviderTestCase<TraderAtDestinationProvider>
{
	public void TestIdentificationNumber() => CombineAssertions(() =>
	{
		AssertEquals("PL EORI is present", "PL123456789000", Provider.IdentificationNumber);

		jobDocAddress.Organisation.CustomsCodes.RemoveAll();
		jobDocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, customsRegNo: "123456789000", countryCode: "ES");
		AssertEquals("ES EORI is present", "ES123456789000", GetProvider().IdentificationNumber);

		jobDocAddress.Organisation.CustomsCodes.RemoveAll();
		AssertEquals("No EORI is present", null, GetProvider().IdentificationNumber);

		jobDocAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("Address is not set", null, GetProvider().IdentificationNumber);

		jobDocAddress = null;
		AssertEquals("JobDocAddress is null", null, GetProvider().IdentificationNumber);
	});

	public void TestCommunicationLanguageAtDestination() => CombineAssertions(() =>
	{
		AssertEquals("Communication language is set to EN", "EN", Provider.CommunicationLanguageAtDestination);

		jobDocAddress.Address.Language = "EN-GB";
		AssertEquals("Communication language is set to EN-GB", "EN", GetProvider().CommunicationLanguageAtDestination);

		jobDocAddress.Address.Language = ZString.Empty;
		AssertEquals("Communication language is not set", null, GetProvider().CommunicationLanguageAtDestination);

		jobDocAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("Address is not set", null, GetProvider().CommunicationLanguageAtDestination);

		jobDocAddress = null;
		AssertEquals("JobDocAddress is null", null, GetProvider().CommunicationLanguageAtDestination);
	});

	protected override TraderAtDestinationProvider GetProvider() => new(jobDocAddress);

	protected override void SetUp()
	{
		base.SetUp();

		var organisation = Factory.New<OrgHeader>();
		organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, customsRegNo: "123456789000", countryCode: "PL");
		organisation.OH_Language = "EN";

		jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.OrganisationPK = organisation.PK;
	}

	JobDocAddress jobDocAddress;
}
