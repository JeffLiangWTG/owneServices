using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class FWBRateLinesProviderTest : TestCaseWithFactory
	{
		#region TestFWBLines_RemoveInvalidCharacters

		public void TestFWBLines_RemoveInvalidCharacters()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "***aB1%43C)=+";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : AB1 43C" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_SkipOnlyInvalidCharsLine

		public void TestFWBLines_SkipOnlyInvalidCharsLine()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "Consolidation as per";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine2.NatureAndQtyOfGoods.Text = "=========";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine3.NatureAndQtyOfGoods.Text = "7/04/2015 4:37:12 AM";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : 7 04 2015 4 37 12 AM" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_SkipEmptyOrInvalidConsolidationLines

		public void TestFWBLines_SkipEmptyOrInvalidConsolidationLines()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine1.NatureAndQtyOfGoods.Text = "Valid Consolidation";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine2.NatureAndQtyOfGoods.Text = "=========";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine3.NatureAndQtyOfGoods.Text = "";

			var rateLine4 = header.AWBRateLine4;
			rateLine4.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine4.NatureAndQtyOfGoods.Text = "==Some Valid Chars==";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : VALID CONSOLIDATION",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : SOME VALID CHARS" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_WrapDescription

		public void TestFWBLines_WrapDescription()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_WrapDescription_ManageOverflowingLines_NoNDA

		readonly string[] expectedOverflowingLinesNoNDA =
{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST 2ND LO",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : NG LINE NEEDS WRAPPI",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : NG",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : DIMS 13x12x15 IN x 2",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : DIMS 10x10x10 IN x 1",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : VOL 100 L",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : 4 SLAC",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : HERES THIRD LONG LIN",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : E HMMMM AND ANOTHER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : LONG LINE FOR WRAPPI",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : NG LAST AND FINAL LI" };

		public void TestFWBLines_WrapDescription_ManageOverflowingLines_Version10_NoNDA()
		{
			AssertWrapDescription_ManageOverflowingLinesNoNDA(FWB.Version.No10, expectedOverflowingLinesNoNDA);
		}

		public void TestFWBLines_WrapDescription_ManageOverflowingLines_Version16_NoNDA()
		{
			AssertWrapDescription_ManageOverflowingLinesNoNDA(FWB.Version.No16, expectedOverflowingLinesNoNDA);
		}

		void AssertWrapDescription_ManageOverflowingLinesNoNDA(FWB.Version version, IEnumerable<string> expected)
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine2.NatureAndQtyOfGoods.Text = "2ND LONG LINE NEEDS WRAPPING";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			rateLine3.NatureAndQtyOfGoods.Text = "DIMS 13X12X15 IN X 2";

			var rateLine4 = header.AWBRateLine4;
			rateLine4.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			rateLine4.NatureAndQtyOfGoods.Text = "DIMS 10X10X10 IN X 1";

			var rateLine5 = header.AWBRateLine5;
			rateLine5.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			rateLine5.NatureAndQtyOfGoods.Text = "VOL 100 L";

			var rateLine6 = header.AWBRateLine6;
			rateLine6.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			rateLine6.NatureAndQtyOfGoods.Text = "4 SLAC";

			var rateLine7 = header.AWBRateLine7;
			rateLine7.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine7.NatureAndQtyOfGoods.Text = "HERES THIRD LONG LINE HMMMM";

			var rateLine8 = header.AWBRateLine8;
			rateLine8.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine8.NatureAndQtyOfGoods.Text = "AND ANOTHER LONG LINE FOR WRAPPING";

			var rateLine9 = header.AWBRateLine9;
			rateLine9.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine9.NatureAndQtyOfGoods.Text = "LAST AND FINAL LINE WITH LONG TEXT";

			var rateLine10 = header.AWBRateLine10;
			rateLine10.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine10.NatureAndQtyOfGoods.Text = "DID I SAY FINAL? I WAS JOKING";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), version);

			var actual = provider.CreateFWBRateLines();

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_WrapDescription_ManageOverflowingLines_NDA

		readonly string[] expectedOverflowingLinesNDA =
{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST 2ND LO",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : NG LINE NEEDS WRAPPI",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : NG 3RD LINE NO WRAP",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : 4TH LINE NO WRAP 5TH",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : LINE NO WRAP",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : 4 SLAC",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : HERES NEXT LONG LINE",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : HMMMM AND ANOTHER LO",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : NG LINE FOR WRAPPING",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : LAST AND FINAL LINE",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : WITH LONG TEXT DID I" };

		public void TestFWBLines_WrapDescription_ManageOverflowingLinesNDA_Version10()
		{
			AssertWrapDescription_ManageOverflowingLinesNDA(FWB.Version.No10, expectedOverflowingLinesNDA);
		}

		public void TestFWBLines_WrapDescription_ManageOverflowingLinesNDA_Version16()
		{
			AssertWrapDescription_ManageOverflowingLinesNDA(FWB.Version.No16, expectedOverflowingLinesNDA.Take(11));
		}

		void AssertWrapDescription_ManageOverflowingLinesNDA(FWB.Version version, IEnumerable<string> expected)
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine2.NatureAndQtyOfGoods.Text = "2ND LONG LINE NEEDS WRAPPING";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine3.NatureAndQtyOfGoods.Text = "3RD LINE NO WRAP";

			var rateLine4 = header.AWBRateLine4;
			rateLine4.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine4.NatureAndQtyOfGoods.Text = "4TH LINE NO WRAP";

			var rateLine5 = header.AWBRateLine5;
			rateLine5.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine5.NatureAndQtyOfGoods.Text = "5TH LINE NO WRAP";

			var rateLine6 = header.AWBRateLine6;
			rateLine6.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			rateLine6.NatureAndQtyOfGoods.Text = "4 SLAC";

			var rateLine7 = header.AWBRateLine7;
			rateLine7.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine7.NatureAndQtyOfGoods.Text = "HERES NEXT LONG LINE HMMMM";

			var rateLine8 = header.AWBRateLine8;
			rateLine8.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine8.NatureAndQtyOfGoods.Text = "AND ANOTHER LONG LINE FOR WRAPPING";

			var rateLine9 = header.AWBRateLine9;
			rateLine9.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine9.NatureAndQtyOfGoods.Text = "LAST AND FINAL LINE WITH LONG TEXT";

			var rateLine10 = header.AWBRateLine10;
			rateLine10.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine10.NatureAndQtyOfGoods.Text = "DID I SAY FINAL? I WAS JOKING";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), version);

			var actual = provider.CreateFWBRateLines();

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_MergeDescription

		public void TestFWBLines_MergeDescription()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "AAAAAAAAAAAAAAAAAAAAAAAA";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine2.NatureAndQtyOfGoods.Text = "BBBB";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine3.NatureAndQtyOfGoods.Text = "CCCC";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : AAAAAAAAAAAAAAAAAAAA",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : AAAA BBBB CCCC" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_DoNotMergeDescriptionIfAllLinesFit

		public void TestFWBLines_DoNotMergeDescriptionIfAllLinesFit()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "AAAA";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine2.NatureAndQtyOfGoods.Text = "BBBB";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : AAAA",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : BBBB" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_DoNotMergeGoodsAndConsolidationLines

		public void TestFWBLines_DoNotMergeGoodsAndConsolidationLines()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine2.NatureAndQtyOfGoods.Text = "Very long description because I can";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : VERY LONG DESCRIPTIO",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : N BECAUSE I CAN" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_WrapDescription_SeparatedByNonWrappableLine

		public void TestFWBLines_WrapDescription_SeparatedByNonWrappableLine()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			rateLine2.NatureAndQtyOfGoods.Text = "VOL 100 L";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine3.NatureAndQtyOfGoods.Text = "some other long description";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 4
	WeightInLBsOrKGs    : K
	GrossWeight         : 245
	ChargeableWeight    : 245
	RateClass           : Q
	RateChargeOrDiscount: 3
	Total               : 735
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : VOL 100 L",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : SOME OTHER LONG DESC",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : RIPTION" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_SplitDescription_NoRateDescription

		public void TestFWBLines_SplitDescription_NoRateDescription()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			rateLine2.NatureAndQtyOfGoods.Text = "VOL 100 L";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine3.NatureAndQtyOfGoods.Text = "another log desctiption just becaus";

			AssertEquals("prerequsite: NatureAndQtyOfGoods has max length",
				rateLine3.ER_NatureAndQtyOfGoodsInfo.MaxLength, rateLine3.NatureAndQtyOfGoods.Text.Length);

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : VOL 100 L",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ANOTHER LOG DESCTIPT",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ION JUST BECAUS" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region TestFWBLines_Split_AllLineTypes

		public void TestFWBLines_Split_AllLineTypes()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = header.AWBRateLine1;
			rateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = header.AWBRateLine2;
			rateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			rateLine2.NatureAndQtyOfGoods.Text = "VOL 100 L";

			var rateLine3 = header.AWBRateLine3;
			rateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			rateLine3.NatureAndQtyOfGoods.Text = "DIMS 13X12X15 IN X 2";

			var rateLine4 = header.AWBRateLine4;
			rateLine4.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			rateLine4.NatureAndQtyOfGoods.Text = "4 SLAC";

			var rateLine5 = header.AWBRateLine5;
			rateLine5.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine5.NatureAndQtyOfGoods.Text = "FLYING DUCKS";

			var rateLine6 = header.AWBRateLine6;
			rateLine6.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			rateLine6.NatureAndQtyOfGoods.Text = "123456";

			var rateLine7 = header.AWBRateLine7;
			rateLine7.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin;
			rateLine7.NatureAndQtyOfGoods.Text = "Goods Origin: NL";

			var rateLine8 = header.AWBRateLine8;
			rateLine8.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			rateLine8.NatureAndQtyOfGoods.Text = "ABCD1112223";

			var provider = new FWBRateLinesProvider(header.AWBRateLines.Cast<ExportAWBRateLine>(), FWB.Version.No16);

			var actual = provider.CreateFWBRateLines();

			var expected = new[]
			{
@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : CONSOLIDATION AS PER",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ATTACHED LIST",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : VOL 100 L",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : DIMS 13x12x15 IN x 2",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : 4 SLAC",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : FLYING DUCKS",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : 123456",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : Goods Origin: NL",

@"RateLine
	CommodityItemNumber : 
	NoOfPiecesOrRCP     : 
	WeightInLBsOrKGs    : 
	GrossWeight         : 0
	ChargeableWeight    : 0
	RateClass           : 
	RateChargeOrDiscount: 0
	Total               : 0
	NatureAndQtyOfGoods : ABCD1112223" };

			AssertFWBRateLines(actual, expected);
		}

		#endregion

		#region Implementation

		void AssertFWBRateLines(IEnumerable<FWBRateLine> actual, IEnumerable<string> expected)
		{
			var actualFormattted = actual.Select(FormatFWBRateLine).ToArray();

			var diff = actualFormattted.Except(expected)
				.Concat(expected.Except(actualFormattted));

			if (diff.Any())
			{
				AssertMultilineASCIIEquals("expected vs actual",
					string.Join(System.Environment.NewLine, expected),
					string.Join(System.Environment.NewLine, actualFormattted));
			}

			Assert(true);
		}

		string FormatFWBRateLine(FWBRateLine rateLine)
		{
			var natureAndQtyOfGoods = rateLine.NatureAndQtyOfGoods != null
				? rateLine.NatureAndQtyOfGoods.Text
				: ZString.Empty;

			return string.Format(
@"RateLine
	CommodityItemNumber : {0}
	NoOfPiecesOrRCP     : {1}
	WeightInLBsOrKGs    : {2}
	GrossWeight         : {3}
	ChargeableWeight    : {4}
	RateClass           : {5}
	RateChargeOrDiscount: {6}
	Total               : {7}
	NatureAndQtyOfGoods : {8}",
				rateLine.CommodityItemNumber,
				rateLine.NoOfPiecesOrRCP,
				rateLine.WeightInLBsOrKGs,
				rateLine.GrossWeight,
				rateLine.ChargeableWeight,
				rateLine.RateClass,
				rateLine.RateChargeOrDiscount,
				rateLine.Total,
				natureAndQtyOfGoods);
		}

		#endregion
	}
}
