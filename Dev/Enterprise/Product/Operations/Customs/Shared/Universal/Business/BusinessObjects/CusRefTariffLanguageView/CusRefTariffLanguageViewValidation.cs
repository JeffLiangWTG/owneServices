//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTariffLanguageViewValidation
//
//    This class should be used for overriding validation in AutoCusRefTariffLanguageViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class CusRefTariffLanguageViewValidation : AutoCusRefTariffLanguageViewValidation
	{
		public CusRefTariffLanguageViewValidation(AutoCusRefTariffLanguageView parent) : base(parent)
		{
		}

		protected new CusRefTariffLanguageView Parent => (CusRefTariffLanguageView)base.Parent;

		protected override void CheckZX7_ZX6_NKLanguage()
		{
			base.CheckZX7_ZX6_NKLanguage();

			var parent = Parent;
			var language = parent.ZX7_ZX6_NKLanguage;
			if (!parent.ZX7_IsSystem && !language.IsEmpty)
			{
				var propertyInfo = parent.ZX7_ZX6_NKLanguageInfo;
				ListValidation.ErrorIfInvalidCode(propertyInfo);

				var length = language.Length;
				if (length != 2 && length != 3)
				{
					propertyInfo.AddError(Res.GetString("BA319F0D-A40A-4F52-B395-C0F73ABBD7B6", "Language must be 2 or 3 characters."));
				}
			}
		}
	}
}
