using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using MovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
sealed class NctsHeaderTest : NctsHeaderAbstractTest
{
	public void TestBillsType() => AssertType<NctsBillCollection<NctsBill>>(EuNctsHeader.Bills);

	public void TestDepartureMovementHeader() => AssertType<NctsDepartureMovementHeader>(EuNctsHeader.MovementHeader);

	public void TestArrivalMovementHeader() => AssertType<NctsArrivalMovementHeader>(EuArrivalNctsHeader.ArrivalMovementHeader);

	public void TestPrincipalDocAddressValidationType() => AssertType<TraderJobDocAddressValidation>(DocAddresses.PiggyBackedDocAddressValidation(nctsHeader.Principal));

	public void TestConsignorDocAddressValidationType() => AssertType<NctsJobDocAddressValidation>(DocAddresses.PiggyBackedDocAddressValidation(nctsHeader.Consignor));

	public void TestValidatePrincipalContactPhones()
		=> OrgContactsTestsHelper.ValidateContactsPhones(Factory, (header, _) => header.Principal, MovementType.Departure, MovementType.Arrival);

	public void TestValidateConsignorContactPhones()
		=> OrgContactsTestsHelper.ValidateContactsPhones(Factory, (header, _) => header.Consignor, MovementType.Departure, MovementType.Arrival);

	public void TestMessages() => AssertType<EDIMessageCollection>(nctsHeader.Messages);

	public void TestGuarantees_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		CombineAssertions(() =>
		{
			nctsHeader.Guarantees.AddNew();
			var result = nctsHeader.Guarantees;
			AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
			AssertType<NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
		});
	}

	public void TestAdditionalDocuments()
	{
		CombineAssertions(() =>
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(arrivalHeader.AdditionalDocuments);
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(nctsHeader.AdditionalDocuments);
		});
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsHeader;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(MovementType.Departure);
		arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(MovementType.Arrival);
	}

	NctsHeader nctsHeader;
	NctsHeader arrivalHeader;
	EU.NCTS.Business.NctsHeader EuNctsHeader => nctsHeader;
	EU.NCTS.Business.NctsHeader EuArrivalNctsHeader => arrivalHeader;
	IDocAddresses DocAddresses => nctsHeader;
}
