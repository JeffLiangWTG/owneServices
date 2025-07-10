using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestAddressType() => AssertType<CusGoodsLocationAddress>(goodsLocation.Address);

	public void TestHeaderType() => AssertType<NctsHeader>(goodsLocation.Header);

	public void TestDepartureMovementHeaderType() => AssertType<NctsDepartureMovementHeader>(goodsLocation.DepartureMovementHeader);

	public void TestValidationType() => AssertType<CusGoodsLocationValidation>(goodsLocation.Validation);

	public void TestContactPersonDataVisible() => CombineAssertions(() =>
	{
		Assert(!goodsLocation.ContactPersonDataVisible);

		goodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		Assert(!goodsLocation.ContactPersonDataVisible);

		goodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		Assert(goodsLocation.ContactPersonDataVisible);
	});

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => goodsLocation;

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
	}

	CusGoodsLocation goodsLocation;
	NctsHeader nctsHeader;
}
