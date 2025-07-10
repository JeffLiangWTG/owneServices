using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCargoDescFee))]
	public class NctsCargoDescFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertType<NctsCargoDescFeeValidation>(fee.Validation);
		}

		public void TestLookups()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertType<NctsCargoDescFeeLookups>(fee.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewNctsCargoDescFee(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewNctsCargoDescFee(factory);

		NctsCargoDescFee GetNewNctsCargoDescFee(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			return goodsItem.Fees.AddNew();
		}
	}
}
