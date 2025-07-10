using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class CusRefRateCodeValidation : AutoCusRefRateCodeValidation
	{
		public CusRefRateCodeValidation(AutoCusRefRateCode parent) : base(parent) { }

		protected override void CheckCR7_RateCode()
		{
			base.CheckCR7_RateCode();
			MandatoryValidation.CheckEntered(Parent.CR7_RateCodeInfo);
		}

		protected override void CheckCR7_RateType()
		{
			base.CheckCR7_RateType();
			MandatoryValidation.CheckEntered(Parent.CR7_RateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CR7_RateTypeInfo);
		}

		protected override void CheckCR7_Description()
		{
			base.CheckCR7_Description();
			MandatoryValidation.CheckEntered(Parent.CR7_DescriptionInfo);
		}

		protected override void CheckCR7_RN_NKCountryCode()
		{
			base.CheckCR7_RN_NKCountryCode();
			var targetInfo = Parent.CR7_RN_NKCountryCodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}
	}
}
