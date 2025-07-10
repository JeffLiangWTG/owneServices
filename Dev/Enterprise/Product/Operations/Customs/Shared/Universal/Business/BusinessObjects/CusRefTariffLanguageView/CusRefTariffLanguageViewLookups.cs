using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefTariffLanguageViewLookups : AutoCusRefTariffLanguageViewLookups
	{
		public CusRefTariffLanguageViewLookups(AutoCusRefTariffLanguageView parent) : base(parent)
		{
		}

		public RefLanguageTypeCollection LanguageTypeList => Factory.GetCachedValue("CusRefTariffLanguageViewLookups.LanguageTypeList", delegate
		{
			var result = new RefLanguageTypeCollection(Factory);
			result.ApplySort(RefLanguageTypeSchema.Constants.ZX6_Language, System.ComponentModel.ListSortDirection.Ascending);
			return result;
		});
	}
}
