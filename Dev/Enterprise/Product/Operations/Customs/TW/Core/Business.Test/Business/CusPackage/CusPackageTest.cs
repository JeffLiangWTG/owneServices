using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackage))]
	sealed class CusPackageTest : Packing.Business.Testing.PkgPackageTest
	{
		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_AlphPrefixDoubleNumeric()
		{
			AssertCalculatePackQtyFromPack("H36", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_Symbol()
		{
			AssertCalculatePackQtyFromPack("*", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeAlphaPrefix()
		{
			AssertCalculatePackQtyFromPack("A3-A5", 3);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_AlphaPrefixAdditionDelimiter()
		{
			AssertCalculatePackQtyFromPack("A3+A5+A6", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_Alpha()
		{
			AssertCalculatePackQtyFromPack("A", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_Negative()
		{
			AssertCalculatePackQtyFromPack("-1", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_AlphaPrefixMultipleHyphenDelimiter()
		{
			AssertCalculatePackQtyFromPack("A3-A5-A6", 3);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_MultipleHyphenDelimiter()
		{
			AssertCalculatePackQtyFromPack("AAA-BB2-CC3", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeFormattedWithSpaces()
		{
			AssertCalculatePackQtyFromPack("A13 - A25 ", 13);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeSymbols()
		{
			AssertCalculatePackQtyFromPack("@3-@8", 6);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeAlphPrefixDoubleNumeric()
		{
			AssertCalculatePackQtyFromPack("A35-A35", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeDoubleNumeric()
		{
			AssertCalculatePackQtyFromPack("31-39", 9);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeNegative()
		{
			AssertCalculatePackQtyFromPack("39-31", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeLowercaseAlphPrefixDoubleNumeric()
		{
			AssertCalculatePackQtyFromPack("a31-a39", 9);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeAlphPrefixLeadingZeros()
		{
			AssertCalculatePackQtyFromPack("A0001-A0004", 4);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeMultiLine()
		{
			AssertCalculatePackQtyFromPack("A0001-A0005\r\nPLT NO. 001", 5);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeSplitBySingleLineBreak()
		{
			AssertCalculatePackQtyFromPack("E0001\r\nE0003", 3);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeSplitByMultiLineBreak()
		{
			AssertCalculatePackQtyFromPack("E0001\r\nE0003\r\nPLT001", 3);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeMinusSignLeadingLineBreak()
		{
			AssertCalculatePackQtyFromPack("E0001-\r\nE0003\r\nPLT001", 3);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeLineBreakLeadingMinusSign()
		{
			AssertCalculatePackQtyFromPack("E0001\r\n-E0003\r\nPLT001", 3);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeContinuousLineBreak()
		{
			AssertCalculatePackQtyFromPack("E0001-\r\n\r\nE0003", 1);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeLeadingZerosSuffixAplha()
		{
			AssertCalculatePackQtyFromPack("0001A-0006A", 6);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_RangeLeadingZerosAndSymbolsSuffixAplha()
		{
			AssertCalculatePackQtyFromPack("00**01A-00((06A", 6);
		}

		[ExpectNoExceptions]
		public void TestCalculatePackQtyFromPack_UsePackageQty()
		{
			AssertCalculatePackQtyFromPack("H36", 100, false);
		}

		protected override bool IsTareWeightDefaulted => false;

		protected override bool IsContainerTypeValid => false;

		protected override void ResetPackageIfNeeded(PkgPackage package)
		{
			base.ResetPackageIfNeeded(package);
			((CusPackage)package).NetWeight = 0m;
			package.KP_Weight = 0m;
			package.KP_TareWeight = 0m;
		}

		[ExpectNoExceptions]
		public void TestPackableItemRelataionsCollectionType()
		{
			NUnit.Framework.Assert.That(((CusPackage)GetNewBusinessObject()).PackableItemRelataions, NUnit.Framework.Is.TypeOf<CusPackageCusPackableItemRelationCollection>());
		}

		protected override BusinessObject GetNewBusinessObject() => GeNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GeNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GeNewBusinessObject(Factory);

		BusinessObject GeNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			factory.Save();
			var cusPackingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(factory);
			factory.Save();
			var package = cusPackingList.PackageJob.Packages.AddNew();
			return package;
		}

		[ExpectNoExceptions]
		void AssertCalculatePackQtyFromPack(ZString marksAndNumbers, int expectedPackageQty, bool calculatePackQty = true)
		{
			var cusPackageJob = Factory.New<CusPackageJob>();
			cusPackageJob.IsCalculatePackQtyFromPack = calculatePackQty;
			var packages = cusPackageJob.Packages;
			var package = packages.AddNew();
			package.KP_PackageQty = 100;
			package.KP_MarksAndNumbers = marksAndNumbers;
			NUnit.Framework.Assert.That(package.KP_PackageQty, NUnit.Framework.Is.EqualTo(expectedPackageQty).Using(CustomComparers.TypeComparison));
		}
	}
}
