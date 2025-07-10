using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsShipmentDutyTaxFeeWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGoodsShipmentDutyTaxFee_AdValoremTaxBaseAmount()
		{
			IGoodsShipmentDutyTaxFee goodsShipmentDutyTaxFee = new GoodsShipmentDutyTaxFeeWrapper("A10", 0M);
			NUnit.Framework.Assert.That(goodsShipmentDutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "GoodsShipmentDutyTaxFee.AdValoremTaxBaseAmount should be");
			goodsShipmentDutyTaxFee = new GoodsShipmentDutyTaxFeeWrapper("A19", 1265.12m);
			NUnit.Framework.Assert.That(goodsShipmentDutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(1265.12m).Using(CustomComparers.TypeComparison), "GoodsShipmentDutyTaxFee.AdValoremTaxBaseAmount should be");
		}

		[ExpectNoExceptions]
		public void TestGoodsShipmentDutyTaxFee_TypeCode()
		{
			IGoodsShipmentDutyTaxFee goodsShipmentDutyTaxFee = new GoodsShipmentDutyTaxFeeWrapper("A10", 0M);
			NUnit.Framework.Assert.That(goodsShipmentDutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A10").Using(CustomComparers.TypeComparison), "GoodsShipmentDutyTaxFee.TypeCode should be");
			goodsShipmentDutyTaxFee = new GoodsShipmentDutyTaxFeeWrapper("A19", 1265.12m);
			NUnit.Framework.Assert.That(goodsShipmentDutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A19").Using(CustomComparers.TypeComparison), "GoodsShipmentDutyTaxFee.TypeCode should be");
			goodsShipmentDutyTaxFee = new GoodsShipmentDutyTaxFeeWrapper("A20", 1265.12m);
			NUnit.Framework.Assert.That(goodsShipmentDutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A20").Using(CustomComparers.TypeComparison), "GoodsShipmentDutyTaxFee.TypeCode should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertNoExceptionThrown(() =>
			{
				new GoodsShipmentDutyTaxFeeWrapper(null, 0M);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				new GoodsShipmentDutyTaxFeeWrapper("A", 0M);
			}

			);
		}
	}
}
