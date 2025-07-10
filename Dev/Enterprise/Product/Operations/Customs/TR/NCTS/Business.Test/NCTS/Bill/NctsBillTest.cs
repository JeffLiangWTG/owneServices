using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBill))]
	public class NctsBillTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => nctsBill;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => nctsBill;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsBill;

		public void TestGoodsItems() => AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(nctsBill.GoodsItems);

		protected override void SetUp()
		{
			base.SetUp();
			nctsBill = CreateBill(Factory);
		}
		NctsBill nctsBill;
		NctsHeader nctsHeader;

		NctsBill CreateBill(BusinessObjectFactory factory)
		{
			nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew();
		}
	}
}
