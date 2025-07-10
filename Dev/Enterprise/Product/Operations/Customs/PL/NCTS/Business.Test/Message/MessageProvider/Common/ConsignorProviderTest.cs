using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class ConsignorProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignorProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null JobDocAddress", "Value cannot be null.\r\nParameter name: docAddress",
			() => new ConsignorProvider(null, false));
		AssertNoExceptionThrown("All ok", () => new ConsignorProvider(consignor, false));
	});

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			ContactPersonTestHelper.TestNewOrNull_OnePerson(organisation, () => GetProvider().ContactPerson);
			ContactPersonTestHelper.TestNewOrNull_MultiplePersonsWithAllocations(organisation, () => GetProvider().ContactPerson);
		});
	}

	protected override ConsignorProvider GetProvider() => new ConsignorProvider(consignor, false);

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsBill = header.Bills.AddNew();
		organisation = Factory.New<OrgHeader>();
		nctsBill.Consignor.OrganisationPK = organisation.PK;
		consignor = nctsBill.Consignor;
	}
	JobDocAddress consignor;
	OrgHeader organisation;
	NctsHeader header;
}
