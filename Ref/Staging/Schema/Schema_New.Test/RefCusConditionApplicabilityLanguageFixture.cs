using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	public class RefCusConditionApplicabilityLanguageFixture
	{
		[Test]
		public void Constructor()
		{
			var lang = new RefCusConditionLanguage
			{
				ZXJ_ZX6_NKLanguage = "AAA",
				ZXJ_Comment = "BBB",
				ZXJ_Source = "CCC",
				ZXJ_AdditionalComment = "DDD"
			};

			var condAppLang = new RefCusConditionApplicabilityLanguage(lang);
			Assert.That(condAppLang.S09_ZX6_NKLanguage == lang.ZXJ_ZX6_NKLanguage);
			Assert.That(condAppLang.S09_Comment == lang.ZXJ_Comment);
			Assert.That(condAppLang.S09_Source == lang.ZXJ_Source);
			Assert.That(condAppLang.S09_AdditionalComment == lang.ZXJ_AdditionalComment);
		}
	}
}
