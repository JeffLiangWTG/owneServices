using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GoodsItemList7Col4RowSection))]
	sealed class GoodsItemList7Col4RowSectionTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestSequenceNum()
		{
			var section = new GoodsItemList7Col4RowSection
			{
				Box32ItemNumber = "1"
			};
			NUnit.Framework.Assert.That(section.Box32ItemNumber, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "SequenceNum");
		}

		[ExpectNoExceptions]
		public void TestBrand()
		{
			var section = new GoodsItemList7Col4RowSection
			{
				Box33Brand = "brand"
			};
			NUnit.Framework.Assert.That(section.Box33Brand, NUnit.Framework.Is.EqualTo("brand").Using(CustomComparers.TypeComparison), "Brand");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestShouldHideRow()
		{
			var section = new GoodsItemList7Col4RowSection();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section.AddLineToBox33List("test detail 1");
			section.AddLineToBox33List("test detail 2");
			section.AddLineToBox33List("test detail 3");
			section.AddLineToBox33List("test detail 4");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			section.AddLineToBox34Box35List("test codes 1");
			section.AddLineToBox34Box35List("test codes 2");
			section.AddLineToBox34Box35List("test codes 3");
			section.AddLineToBox34Box35List("test codes 4");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			section.AddLineToBox36List("test price 1");
			section.AddLineToBox36List("test price 2");
			section.AddLineToBox36List("test price 3");
			section.AddLineToBox36List("test price 4");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			section.AddLineToBox37Box38Box39List("test weight 1");
			section.AddLineToBox37Box38Box39List("test weight 2");
			section.AddLineToBox37Box38Box39List("test weight 3");
			section.AddLineToBox37Box38Box39List("test weight 4");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			section.AddLineToBox40List("test FOB 1");
			section.AddLineToBox40List("test FOB 2");
			section.AddLineToBox40List("test FOB 3");
			section.AddLineToBox40List("test FOB 4");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			section.AddLineToBox41List("test statistic 1");
			section.AddLineToBox41List("test statistic 2");
			section.AddLineToBox41List("test statistic 3");
			section.AddLineToBox41List("test statistic 4");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection() { Box33BrandLine1 = "test Brand" };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection() { Box32ItemNumber = "test SequenceNum" };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection() { ShowEvenRowIsEmpty = true };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			var testImage = Image.FromFile(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"));
			section.TrademarkImage1 = new Bitmap(testImage, new Size(37, 37));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section.TrademarkImage1 = new Bitmap(testImage, new Size(36, 36));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section.TrademarkImage1 = new Bitmap(testImage, new Size(24, 24));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});

			section = new GoodsItemList7Col4RowSection();
			section.Box33BrandLine2 = "line 2";
			section.TrademarkImage2 = new Bitmap(testImage, new Size(24, 24));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");

				section.TrademarkImage2 = new Bitmap(testImage, new Size(36, 36));
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			});
		}

		[ExpectNoExceptions]
		public void TestRowCountToShow()
		{
			var section = new GoodsItemList7Col4RowSection();
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(0));

			section.AddLineToBox33List("test detail 1");
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(1));

			section.AddLineToBox33List("test detail 2");
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(2));

			section.AddLineToBox33List("test detail 3");
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(3));

			section.AddLineToBox33List("test detail 4");
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(4));

			section.AddLineToBox33List("test detail 5");
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(4));
		}

		[ExpectNoExceptions]
		public void TestBox33()
		{
			var section = new GoodsItemList7Col4RowSection();

			NUnit.Framework.Assert.That(section.DetailsFull, NUnit.Framework.Is.EqualTo(false), "DetailsFull");
			section.AddLineToBox33List("test Box33 1\ntest Box33 line 1");
			section.AddLineToBox33List("test Box33 2\ntest Box33 line 2");
			section.AddLineToBox33List("test Box33 3\ntest Box33 line 3");
			section.AddLineToBox33List("test Box33 4\ntest Box33 line 4");
			NUnit.Framework.Assert.That(section.DetailsFull, NUnit.Framework.Is.EqualTo(true), "DetailsFull");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("test Box33 1 test Box33 line 1").Using(CustomComparers.TypeComparison), "Box33DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(section.Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("test Box33 2 test Box33 line 2").Using(CustomComparers.TypeComparison), "Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(section.Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("test Box33 3 test Box33 line 3").Using(CustomComparers.TypeComparison), "Box33DescriptionOfGoods_Line3");
				NUnit.Framework.Assert.That(section.Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("test Box33 4 test Box33 line 4").Using(CustomComparers.TypeComparison), "Box33DescriptionOfGoods_Line4");
			});
		}

		[ExpectNoExceptions]
		public void TestBox34Box35()
		{
			var section = new GoodsItemList7Col4RowSection();

			section.AddLineToBox34Box35List("test Box34ItemNumber 1");
			section.AddLineToBox34Box35List("test Box34ItemNumber 2");
			section.AddLineToBox34Box35List("test Box35 1");
			section.AddLineToBox34Box35List("test Box35 2");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.Box34ImportExportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("test Box34ItemNumber 1").Using(CustomComparers.TypeComparison), "Box34ImportExportPermitNumberAndItemNumber_Line1");
				NUnit.Framework.Assert.That(section.Box34ImportExportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("test Box34ItemNumber 2").Using(CustomComparers.TypeComparison), "Box34ImportExportPermitNumberAndItemNumber_Line2");
				NUnit.Framework.Assert.That(section.Box35CCCCode, NUnit.Framework.Is.EqualTo("test Box35 1").Using(CustomComparers.TypeComparison), "Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(section.Box35BondedGoodsCodeAndAssignedNumber, NUnit.Framework.Is.EqualTo("test Box35 2").Using(CustomComparers.TypeComparison), "Box35DescriptionOfGoods_Line2");
			});
		}

		[ExpectNoExceptions]
		public void TestBox36()
		{
			var section = new GoodsItemList7Col4RowSection();

			section.AddLineToBox36List("test Box36 1");
			section.AddLineToBox36List("test Box36 2");
			section.AddLineToBox36List("test Box36 3");
			section.AddLineToBox36List("test Box36 4");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.Box36UnitPrice_Line1, NUnit.Framework.Is.EqualTo("test Box36 1").Using(CustomComparers.TypeComparison), "Box36UnitPrice_Line1");
				NUnit.Framework.Assert.That(section.Box36UnitPrice_Line2, NUnit.Framework.Is.EqualTo("test Box36 2").Using(CustomComparers.TypeComparison), "Box36UnitPrice_Line2");
				NUnit.Framework.Assert.That(section.Box36UnitPrice_Line3, NUnit.Framework.Is.EqualTo("test Box36 3").Using(CustomComparers.TypeComparison), "Box36UnitPrice_Line3");
				NUnit.Framework.Assert.That(section.Box36UnitPrice_Line4, NUnit.Framework.Is.EqualTo("test Box36 4").Using(CustomComparers.TypeComparison), "Box36UnitPrice_Line4");
			});
		}

		[ExpectNoExceptions]
		public void TestBox37Box38Box39()
		{
			var section = new GoodsItemList7Col4RowSection();

			NUnit.Framework.Assert.That(section.WeightAndQuantityFull, NUnit.Framework.Is.EqualTo(false), "WeightAndQuantityFull");
			section.AddLineToBox37Box38Box39List("test Box37NetWeight");
			section.AddLineToBox37Box38Box39List("test Box38QuantityAndUnit");
			section.AddLineToBox37Box38Box39List("test Box39 1");
			section.AddLineToBox37Box38Box39List("test Box39 2");
			NUnit.Framework.Assert.That(section.WeightAndQuantityFull, NUnit.Framework.Is.EqualTo(true), "WeightAndQuantityFull");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.Box37NetWeight, NUnit.Framework.Is.EqualTo("test Box37NetWeight").Using(CustomComparers.TypeComparison), "Box37NetWeight");
				NUnit.Framework.Assert.That(section.Box38QuantityAndUnit, NUnit.Framework.Is.EqualTo("test Box38QuantityAndUnit").Using(CustomComparers.TypeComparison), "Box38QuantityAndUnit");
				NUnit.Framework.Assert.That(section.Box39StatisticsQuantityAndUnit_Line1, NUnit.Framework.Is.EqualTo("test Box39 1").Using(CustomComparers.TypeComparison), "Box39IncoTermAndCurrency");
				NUnit.Framework.Assert.That(section.Box39StatisticsQuantityAndUnit_Line2, NUnit.Framework.Is.EqualTo("test Box39 2").Using(CustomComparers.TypeComparison), "Box39UnitPrice_Line1");
			});
		}

		[ExpectNoExceptions]
		public void TestBox40()
		{
			var section = new GoodsItemList7Col4RowSection();

			section.AddLineToBox40List("test Box40NetWeight 1");
			section.AddLineToBox40List("test Box40NetWeight 2");
			section.AddLineToBox40List("test Box40NetWeight 3");
			section.AddLineToBox40List("test Box40NetWeight 4");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.Box40FOBValue_Line1, NUnit.Framework.Is.EqualTo("test Box40NetWeight 1").Using(CustomComparers.TypeComparison), "Box40FOBValue_Line1");
				NUnit.Framework.Assert.That(section.Box40FOBValue_Line2, NUnit.Framework.Is.EqualTo("test Box40NetWeight 2").Using(CustomComparers.TypeComparison), "Box40FOBValue_Line2");
				NUnit.Framework.Assert.That(section.Box40FOBValue_Line3, NUnit.Framework.Is.EqualTo("test Box40NetWeight 3").Using(CustomComparers.TypeComparison), "Box40FOBValue_Line3");
				NUnit.Framework.Assert.That(section.Box40FOBValue_Line4, NUnit.Framework.Is.EqualTo("test Box40NetWeight 4").Using(CustomComparers.TypeComparison), "Box40FOBValue_Line4");
			});
		}

		[ExpectNoExceptions]
		public void TestBox41()
		{
			var section = new GoodsItemList7Col4RowSection();

			section.AddLineToBox41List("test Box41QuantityAndUnit 1");
			section.AddLineToBox41List("test Box41QuantityAndUnit 2");
			section.AddLineToBox41List("test Box41QuantityAndUnit 3");
			section.AddLineToBox41List("test Box41QuantityAndUnit 4");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.Box41ModeOfStatistics_Line1, NUnit.Framework.Is.EqualTo("test Box41QuantityAndUnit 1").Using(CustomComparers.TypeComparison), "Box41ModeOfStatistics_Line1");
				NUnit.Framework.Assert.That(section.Box41ModeOfStatistics_Line2, NUnit.Framework.Is.EqualTo("test Box41QuantityAndUnit 2").Using(CustomComparers.TypeComparison), "Box41ModeOfStatistics_Line2");
				NUnit.Framework.Assert.That(section.Box41ModeOfStatistics_Line3, NUnit.Framework.Is.EqualTo("test Box41QuantityAndUnit 3").Using(CustomComparers.TypeComparison), "Box41ModeOfStatistics_Line3");
				NUnit.Framework.Assert.That(section.Box41ModeOfStatistics_Line4, NUnit.Framework.Is.EqualTo("test Box41QuantityAndUnit 4").Using(CustomComparers.TypeComparison), "Box41ModeOfStatistics_Line4");
			});
		}

		[ExpectNoExceptions]
		public void TestBox36Line11Box36Line31Box40Line11Box40Line21()
		{
			var section = new GoodsItemList7Col4RowSection
			{
				IsNotPrintEmpty = true
			};
			section.AddLineToBox36List("Price1");
			section.AddLineToBox36List("Price2");
			section.AddLineToBox36List("Price3");
			section.AddLineToBox40List("FOBValue1");
			section.AddLineToBox36List("FOBValue2");
			NUnit.Framework.Assert.That(section.Box36Line11, NUnit.Framework.Is.EqualTo("Price1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box36Line31, NUnit.Framework.Is.EqualTo("(本欄空白)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box40Line11, NUnit.Framework.Is.EqualTo("(本欄空白)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box40Line21, NUnit.Framework.Is.EqualTo(ZString.Empty));

			section.IsNotPrintEmpty = false;
			NUnit.Framework.Assert.That(section.Box36Line11, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box36Line31, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box40Line11, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box40Line21, NUnit.Framework.Is.EqualTo(ZString.Empty));

			section.IsNotPrintEmpty = false;
			section.IsTotal = true;
			NUnit.Framework.Assert.That(section.Box36Line11, NUnit.Framework.Is.EqualTo("Price1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box36Line31, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box40Line11, NUnit.Framework.Is.EqualTo("(本欄空白)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box40Line21, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
