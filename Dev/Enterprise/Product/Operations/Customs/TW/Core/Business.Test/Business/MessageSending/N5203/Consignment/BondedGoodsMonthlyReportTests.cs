using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedGoodsMonthlyReportTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMonthNumeric()
		{
			NUnit.Framework.Assert.That(bondedGoodsMonthlyReport.MonthNumeric, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTraderReferenceID()
		{
			NUnit.Framework.Assert.That(bondedGoodsMonthlyReport.TraderReferenceID, NUnit.Framework.Is.EqualTo("XX2").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_WHSMonth = "2";
			entryInstruction.CEI_WHSTradeReferenceNo = "XX2";
			bondedGoodsMonthlyReport = new BondedGoodsMonthlyReport(entryInstruction);
		}

		IBondedGoodsMonthlyReport bondedGoodsMonthlyReport;
	}
}
