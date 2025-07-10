using System.Collections.Immutable;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public static class WinCESupportedLanguages
	{
		public readonly static ImmutableHashSet<string> List =
			ImmutableHashSet.Create(
				Core.SharedConstants.Languages.ChineseSimplified,
				Core.SharedConstants.Languages.ChineseTraditional,
				Core.SharedConstants.Languages.English,
				Core.SharedConstants.Languages.EnglishAmerican,
				Core.SharedConstants.Languages.EnglishBritish);
	}
}
