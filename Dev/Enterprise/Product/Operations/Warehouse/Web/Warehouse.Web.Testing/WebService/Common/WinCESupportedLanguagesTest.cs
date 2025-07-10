using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	class WinCESupportedLanguagesSupportedLanguagesTest : TestCase
	{
		public void TestList()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Core.SharedConstants.Languages.ChineseSimplified,
				Core.SharedConstants.Languages.ChineseTraditional,
				Core.SharedConstants.Languages.English,
				Core.SharedConstants.Languages.EnglishAmerican,
				Core.SharedConstants.Languages.EnglishBritish
			}, WinCESupportedLanguages.List);
		}
	}
}
