using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListLanguageCombinedValidation : AutoZZRefCusCodeListLanguageCombinedValidation
	{
		public ZZRefCusCodeListLanguageCombinedValidation(AutoZZRefCusCodeListLanguageCombined parent) : base(parent)
		{
		}

		protected override void CheckZXA_ZX6_NKLanguage()
		{
			base.CheckZXA_ZX6_NKLanguage();
			var tarinfo = Parent.ZXA_ZX6_NKLanguageInfo;
			ListValidation.ErrorIfInvalidCode(tarinfo);
			MandatoryValidation.CheckEntered(tarinfo);
		}

		protected override void CheckZXA_Description()
		{
			base.CheckZXA_Description();
			MandatoryValidation.CheckEntered(Parent.ZXA_DescriptionInfo);
		}
	}
}
