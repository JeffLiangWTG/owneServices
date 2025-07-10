using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLanguageTextPageEntryCollection))]
	sealed class RefLanguageTextPageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RefLanguageTextPageEntryCollection>
	{
		protected override RefLanguageTextPageEntryCollection GetCollectionToTest() => new RefLanguageTextPageEntryCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = new RefLanguageTextTranslationTestHelper(Factory);
			helper.GetContextObject(Factory, "one");
			var multiLanguage = helper.GetMultilingualLanguage("one");

			return new RefLanguageTextPageEntry(SharedConstants.Languages.English, multiLanguage);
		}
	}
}
