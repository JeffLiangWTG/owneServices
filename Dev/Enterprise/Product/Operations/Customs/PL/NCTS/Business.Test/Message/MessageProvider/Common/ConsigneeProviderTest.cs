using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class ConsigneeProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsigneeProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null JobDocAddress", "Value cannot be null.\r\nParameter name: docAddress",
			() => new ConsigneeProvider(null, false));
		AssertNoExceptionThrown("All ok", () => new ConsigneeProvider(consignee, false));
	});

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Eori", ZString.Empty, Provider.IdentificationNumber);

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("Eori exists", "PL111", GetProvider().IdentificationNumber);
		});
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No IdentificationNumber", "Abba Baab", Provider.Name);

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertNull("IdentificationNumber is present", GetProvider().Name);

			consignee.OrganisationPK = ZGuid.Empty;
			AssertNull("Consignee address is empty", GetProvider().Name);
		});
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("No IdentificationNumber", Provider.Address);

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertNull("IdentificationNumber is present", GetProvider().Address);

			consignee.OrganisationPK = ZGuid.Empty;
			AssertNull("Consignee address is empty", GetProvider().Address);
		});
	}

	public void TestNameMaxLength()
	{
		const int inNCTSTPPeriod = 35;
		const int outNCTSTPPeriod = 70;

		CombineAssertions(() =>
		{
			AssertEquals("Name max length in transition period", inNCTSTPPeriod, new ConsigneeProvider(consignee, true).NameMaxLength);
			AssertEquals("Name max length outside transition period", outNCTSTPPeriod, GetProvider().NameMaxLength);
		});
	}

	protected override ConsigneeProvider GetProvider() => new ConsigneeProvider(consignee, false);

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsBill = header.Bills.AddNew();
		organisation = Factory.New<OrgHeader>();
		orgAddress = organisation.Addresses.AddNew();
		organisation.OH_FullName = "Abba Baab";
		nctsBill.Consignee.OrganisationPK = organisation.PK;
		consignee = nctsBill.Consignee;
	}
	JobDocAddress consignee;
	OrgHeader organisation;
	OrgAddress orgAddress;
	NctsHeader header;
}
