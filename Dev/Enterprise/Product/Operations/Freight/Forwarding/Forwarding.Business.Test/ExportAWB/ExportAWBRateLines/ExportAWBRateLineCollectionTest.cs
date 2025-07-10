using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBRateLineCollectionTest : TestCase
	{
		public ExportAWBRateLineCollectionTest()
		{
		}

		BusinessObjectFactory Factory;
		ExportAWBRateLineCollection RateLines;
		protected override void SetUp()
		{
			Factory = new BusinessObjectFactory();
			ShipmentExportAWBHeader aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			RateLines = new ExportAWBRateLineCollection(aWBHeader);
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();

			RateLines.Add(rateLine);
			rateLine.ER_ChargeableWeight = 5.4M;
			rateLine.ER_NoOfPiecesOrRCP = "10";
			rateLine.ER_CommodityItemNumber = "Test";

			rateLine = Factory.New<ExportAWBRateLine>();
			RateLines.Add(rateLine);
			rateLine.ER_NoOfPiecesOrRCP = "17";
			rateLine.ER_LineCount = 4;
			rateLine.ER_CommodityItemNumber = "Test1";

			rateLine = Factory.New<ExportAWBRateLine>();
			RateLines.Add(rateLine);
			rateLine.ER_NoOfPiecesOrRCP = "15";
			rateLine.ER_ChargeableWeight = 3.3M;

			base.SetUp();
		}

		public void TestContains()
		{
			AssertEquals(ZBool.True, RateLines.Contains(ExportAWBRateLine.Schema.ER_LineCount, (ZByte)4));
			AssertEquals(ZBool.False, RateLines.Contains(ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1));

			AssertEquals(ZBool.True, RateLines.Contains(ExportAWBRateLine.Schema.ER_CommodityItemNumber, "Test"));
			AssertEquals(ZBool.True, RateLines.Contains(ExportAWBRateLine.Schema.ER_CommodityItemNumber, "Test1"));
			AssertEquals(ZBool.False, RateLines.Contains(ExportAWBRateLine.Schema.ER_CommodityItemNumber, "TESTER"));

			AssertEquals(ZBool.True, RateLines.Contains(ExportAWBRateLine.Schema.ER_ChargeableWeight, 5.5M));
			AssertEquals(ZBool.False, RateLines.Contains(ExportAWBRateLine.Schema.ER_ChargeableWeight, 0.2M));
		}

		public void TestIndexer()
		{
			AssertNotNull(RateLines[ExportAWBRateLine.Schema.ER_CommodityItemNumber, "Test"]);
			AssertNotNull(RateLines[ExportAWBRateLine.Schema.ER_CommodityItemNumber, "Test1"]);

			AssertNull(RateLines[ExportAWBRateLine.Schema.ER_CommodityItemNumber, "Something Else"]);

			AssertNotNull(RateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)4]);
			AssertNull(RateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1]);
		}

		public void TestSumOfInt()
		{
			AssertEquals(42, RateLines.SumOfInt(ExportAWBRateLine.Schema.ER_NoOfPiecesOrRCP));
		}

		public void SumOfDecimal()
		{
			AssertEquals(8.7M, RateLines.SumOfDecimal(ExportAWBRateLine.Schema.ER_ChargeableWeight));
		}

		public void TestSetReadOnly()
		{
			RateLines.SetReadOnly(ZBool.True);
			Assert(RateLines[0].ReadOnly);
			Assert(RateLines[1].ReadOnly);
			Assert(RateLines[2].ReadOnly);

			RateLines.SetReadOnly(ZBool.False);
			Assert(!RateLines[0].ReadOnly);
			Assert(!RateLines[1].ReadOnly);
			Assert(!RateLines[2].ReadOnly);
		}
	}
}
