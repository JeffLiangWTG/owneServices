using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CarrierProviderTest : Customs.Business.Testing.DataProviderTestCase<CarrierProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null Orgheader", "Value cannot be null.\r\nParameter name: header", () => new CarrierProvider(null));
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Eori", ZString.Empty, Provider.IdentificationNumber);

			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("Eori exists", "PL111", GetProvider().IdentificationNumber);
		});
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			ContactPersonTestHelper.TestNewOrNull_OnePerson(organisation, () => GetProvider().ContactPerson);
			ContactPersonTestHelper.TestNewOrNull_MultiplePersonsWithAllocations(organisation, () => GetProvider().ContactPerson);
		});
	}

	protected override CarrierProvider GetProvider() => new CarrierProvider(docAddress);
	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsBill = nctsHeader.Bills.AddNew();
		organisation = Factory.New<OrgHeader>();
		organisation.Addresses.AddNew();
		organisation.OH_FullName = "Abba Baab";
		nctsBill.Consignee.OrganisationPK = organisation.PK;
		docAddress = nctsBill.Consignee;
	}
	JobDocAddress docAddress;
	OrgHeader organisation;
}
