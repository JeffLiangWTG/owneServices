using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.FDARecapPrinting.Testing
{
	[TestedType(typeof(FDARecapLineCollection))]
	sealed class FDARecapLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FDARecapLineCollection>
	{
		public void TestPopulateCollection()
		{
			var declaration = DeclarationTestHelper.GetMergedDeclarationWithFDALines(Factory);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var collection = new FDARecapLineCollection(entry);
			AssertEquals("Should be 4 FDA lines for printing", 4, collection.Count);
			AssertEquals("Country Of Origin", USConstants.MultipleValueIndicator, entry.UniqueCountryOfOrigin);
			AssertEquals("Country Of Origin for Line 1", Core.Constants.CountryCodes.Italy, collection[0].CountryOfOriginForLine);
			AssertEquals("Country Of Origin for Line 3", ZString.Empty, collection[2].CountryOfOriginForLine);
		}

		protected override FDARecapLineCollection GetCollectionToTest()
		{
			return new FDARecapLineCollection(Factory.NewWithValidTestData<CusEntryHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FDARecapLine(Factory.NewWithValidTestData<FDA>(), false);
		}
	}
}
