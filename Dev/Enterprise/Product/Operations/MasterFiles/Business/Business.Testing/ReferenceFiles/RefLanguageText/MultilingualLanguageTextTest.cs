using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MultilingualLanguageText))]
	sealed class MultilingualLanguageTextTest : ZMultilingualTest
	{
		public void TestGetEnglishTextIfNoLanguageAvailable()
		{
			var factory = new BusinessObjectFactory();
			var multilingualLanguage = MultilingualLanguageText.GetMultilingualText("58EA6466-0E04-4F38-AA3E-DCBB1F0E9D66", "ANY", "ANY", "Text", factory);
			AssertEquals(multilingualLanguage, "Text");
		}

		protected override ZMultilingual GetZMultilingualConcrete()
		{
			var factory = new BusinessObjectFactory();
			return MultilingualLanguageText.GetMultilingualText("58EA6466-0E04-4F38-AA3E-DCBB1F0E9D66", "ANY", "ANY", "Text", factory);
		}
	}
}
