using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsBill))]
sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
{
	public void TestGoodsItems()
	{
		AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(nctsBill.GoodsItems);
	}

	public void TestCusInBondCargoDescType()
	{
		AssertEquals(typeof(NctsDepartureCargoDesc), ((ICusInBondCargoDescTypeProvider)nctsBill).CusInBondCargoDescType);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateBill(factory);

	protected override BusinessObject GetNewBusinessObject() => nctsBill;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsBill;

	protected override void SetUp()
	{
		base.SetUp();
		nctsBill = CreateBill(Factory);
	}
	NctsBill nctsBill;

	NctsBill CreateBill(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.Bills.AddNew();
	}
}
