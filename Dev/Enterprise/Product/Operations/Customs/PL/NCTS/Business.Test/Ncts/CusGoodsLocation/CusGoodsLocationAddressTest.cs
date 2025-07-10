using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestGoodsLocationType() => AssertType<CusGoodsLocation>(EuGoodsLocationAddress.GoodsLocation);

	public void TestValidationType() => AssertType<CusGoodsLocationAddressValidation>(EuGoodsLocationAddress.Validation);

	public void TestValidation() => AssertType<CusGoodsLocationAddressValidation>(goodsLocationAddress.Validation);

	public void TestE2_City() => AssertEquals(35, goodsLocationAddress.E2_CityInfo.MaxLength);

	protected override BusinessObject GetNewBusinessObject() => goodsLocationAddress;

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
		goodsLocationAddress = movementHeader.GoodsLocation.Address;
	}
	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;
	CusGoodsLocationAddress goodsLocationAddress;

	EU.NCTS.Business.CusGoodsLocationAddress EuGoodsLocationAddress => goodsLocationAddress;
}
