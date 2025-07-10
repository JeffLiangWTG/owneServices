using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLanguageTextPageEntry))]
	sealed class RefLanguageTextPageEntryTest : NonPersistentBusinessObjectTestCase
	{
		RefLanguageTextPage GetPageInstance(ZString caption)
		{
			return new RefLanguageTextPage(testHelper.ParentTableCode, testHelper.ColumnToTest, testHelper.GetMultilingualLanguage(caption), testHelper.GetContextObject(Factory, caption), "", testHelper.GetFilterColumns());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetPageInstance("two").All[0];
		}

		public void TestEmptyTranslationDefaultsToSystemTranslation()
		{
			var page = GetPageInstance("four");
			var four = (RefLanguageTextPageEntry)page.All.First(entry => ((RefLanguageTextPageEntry)entry).Language == Core.SharedConstants.Languages.ChineseSimplified && ((RefLanguageTextPageEntry)entry).English == "four");
			AssertEquals(testHelper.ChineseNumbers.ElementAt(4), four.Translation);
			four.Translation = "bla";
			AssertEquals("bla", four.Translation);
			four.Translation = "";
			AssertEquals(testHelper.ChineseNumbers.ElementAt(4), four.Translation);

			four = (RefLanguageTextPageEntry)page.All.First(entry => ((RefLanguageTextPageEntry)entry).Language == Core.SharedConstants.Languages.Spanish && ((RefLanguageTextPageEntry)entry).English == "four");
			AssertEquals("four", four.Translation);
			four.Translation = "bla";
			AssertEquals("bla", four.Translation);
			four.Translation = "";
			AssertEquals("four", four.Translation);
		}

		protected override void SetUp()
		{
			testHelper = new RefLanguageTextTranslationTestHelper(Factory);
			base.SetUp();
		}

		RefLanguageTextTranslationTestHelper testHelper;
	}
}
