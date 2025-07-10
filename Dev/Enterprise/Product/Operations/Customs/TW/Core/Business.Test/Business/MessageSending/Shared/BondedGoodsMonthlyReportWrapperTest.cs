using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedGoodsMonthlyReportWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBondedGoodsMonthlyReport()
		{
			IBondedGoodsMonthlyReport bondedGoodsMonthlyReport = new BondedGoodsMonthlyReportWrapper(12, "ZXX");
			NUnit.Framework.Assert.That(bondedGoodsMonthlyReport.MonthNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.MonthNumeric should be");
			NUnit.Framework.Assert.That(bondedGoodsMonthlyReport.TraderReferenceID, NUnit.Framework.Is.EqualTo("ZXX").Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.TW_WHSTradeReferenceNo should be");
		}
	}
}
