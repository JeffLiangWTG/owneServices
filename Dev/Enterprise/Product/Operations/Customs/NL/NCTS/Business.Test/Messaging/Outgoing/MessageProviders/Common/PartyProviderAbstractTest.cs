using System;
using System.Reflection;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(PartyProvider))]
abstract class PartyProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : PartyProvider
{
	public void TestId() => AssertEquals("NL123456789", provider.Id);

	public void TestName() => AssertEquals(ExpectedOmitNameAndAddressWhenIDIsFound ? string.Empty : "HolderName", provider.Name);

	public void TestAddress()
	{
		if (ExpectedOmitNameAndAddressWhenIDIsFound)
		{
			AssertNull(provider.Address);
		}
		else
		{
			AssertType<AddressProvider>(provider.Address);
		}
	}

	public void TestContact() => AssertType<ContactProvider>(provider.Contact);

	public void TestOmitNameAndAddressWhenIDIsFound()
	{
		var propertyInfo = typeof(T).GetProperty("OmitNameAndAddressWhenIDIsFound", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertEquals(ExpectedOmitNameAndAddressWhenIDIsFound, (bool)propertyInfo.GetValue(provider));
	}
	protected virtual bool ExpectedOmitNameAndAddressWhenIDIsFound => false;

	protected virtual T CreateProvider(JobDocAddress address) => (T)Activator.CreateInstance(typeof(T), address);

	protected override T GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CreatePrincipal(nctsHeader);
		provider = CreateProvider(nctsHeader.Principal);
	}
	protected T provider;
	protected NctsHeader nctsHeader;

	protected virtual void CreatePrincipal(NctsHeader nctsHeader)
	{
		var party = Factory.New<OrgHeader>();
		party.OH_FullName = "HolderName";
		party.OH_Code = "HOLDER";

		var partyAddress = party.Addresses.AddNew();
		partyAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		partyAddress.OA_Address1 = "Rivierstraat 15";
		partyAddress.OA_City = "Rotterdam";

		CreateCustomsCode(party.CustomsCodes, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789");
		CreateCustomsCode(party.CustomsCodes, OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "987654321");

		nctsHeader.Principal.OrganisationPK = party.PK;
	}

	protected void CreateCustomsCode(OrgCusCodeCollection customsCodes, string codeType, string id)
	{
		var customsCode = customsCodes.AddNew();
		customsCode.OK_CodeType = codeType;
		customsCode.OK_CustomsRegNo = id;
		customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
	}
}
