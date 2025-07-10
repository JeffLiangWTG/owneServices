using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListLanguageCombinedLookups : AutoZZRefCusCodeListLanguageCombinedLookups
	{
		public ZZRefCusCodeListLanguageCombinedLookups(AutoZZRefCusCodeListLanguageCombined parent) : base(parent)
		{
		}

		public RefLanguageTypeCollection LanguageTypeList => Factory.GetCachedValue("ZZRefCusCodeListLanguageCombinedLookups.LanguageTypeList", delegate
		{
			var result = new RefLanguageTypeCollection(Factory);
			result.ApplySort(RefLanguageTypeSchema.Constants.ZX6_Language, System.ComponentModel.ListSortDirection.Ascending);
			return result;
		});
	}
}
