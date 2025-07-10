using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackedDetailInfoTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPackedQtyDetailInfoWhenFixedDecimalPlaces()
		{
			AssertPackedQtyDetailInfo(0m, ZString.Empty, 0, 5, 0, ZString.Empty, 0);
			AssertPackedQtyDetailInfo(1m, "PK", 0, 5, 0, "1 PK", 0);
			AssertPackedQtyDetailInfo(1m, "PK", 1, 5, 0, "1 PK", 0);
			AssertPackedQtyDetailInfo(2m, "PK", 2, 5, 0, "@1 PK\r\n2 PK", 0);
			AssertPackedQtyDetailInfo(1.899m, "PK", 2, 5, 0, "@0.9495 PK\r\n1.899 PK", 4);
			AssertPackedQtyDetailInfo(1m, "PK", 3, 5, 0, "@0.33333 PK\r\n1 PK", 5);
			AssertPackedQtyDetailInfo(1m, "PK", 2, 5, 0, "@0.5 PK\r\n1 PK", 1);
		}

		[ExpectNoExceptions]
		public void TestPackedQtyDetailInfoWhenQuantityRatioDecimalPlacesIsDifferent()
		{
			AssertPackedQtyDetailInfo(1m, "PK", 3, 5, 0, "@0.33333 PK\r\n1 PK", 5);
			AssertPackedQtyDetailInfo(1m, "PK", 3, 4, 0, "@0.3333 PK\r\n1 PK", 4);
			AssertPackedQtyDetailInfo(1m, "PK", 3, 3, 0, "@0.333 PK\r\n1 PK", 3);
		}

		[ExpectNoExceptions]
		public void TestPackedQtyDetailInfoWhenGetDetailDecimalPlacesIsDifferent()
		{
			AssertPackedQtyDetailInfo(1m, "PK", 3, 5, 0, "@0.33333 PK\r\n1 PK", 5);
			AssertPackedQtyDetailInfo(1m, "PK", 3, 5, 1, "@0.3 PK\r\n1.0 PK", 5);
			AssertPackedQtyDetailInfo(1m, "PK", 3, 5, 2, "@0.33 PK\r\n1.00 PK", 5);
		}

		[ExpectNoExceptions]
		void AssertPackedQtyDetailInfo(ZDecimal packedQty, ZString packedUQ, ZInt packageQty, ZInt quantityRatioDecimalPlaces, ZInt getDetailDecimalPlaces, ZString expectedDetailInfo, int expectedMaxDecimalPlace)
		{
			var packedDetailInfo = new PackedDetailInfo(packedQty, packedUQ, packageQty, quantityRatioDecimalPlaces);
			NUnit.Framework.Assert.That(packedDetailInfo.GetDetail(getDetailDecimalPlaces), NUnit.Framework.Is.EqualTo(expectedDetailInfo));
			NUnit.Framework.Assert.That(packedDetailInfo.MaxDecimalPlace, NUnit.Framework.Is.EqualTo(expectedMaxDecimalPlace));
		}
	}
}
