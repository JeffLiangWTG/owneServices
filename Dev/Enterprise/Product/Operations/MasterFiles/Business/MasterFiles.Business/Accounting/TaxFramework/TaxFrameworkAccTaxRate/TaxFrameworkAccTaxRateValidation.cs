using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class TaxFrameworkAccTaxRateValidation : AccTaxRateValidation
	{
		public TaxFrameworkAccTaxRateValidation(TaxFrameworkAccTaxRate parent)
			: base(parent)
		{ }

		protected override void CheckAT_Code()
		{
			base.CheckAT_Code();
			if (!Parent.AT_Code.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.AT_CodeInfo.AddError(Res.GetString("{C0B472F0-5B2F-4FBE-AFF8-C214C09A6412}", "Please enter alphanumeric characters only."));
			}
		}

		protected override void CheckAT_RN_NKCountry()
		{
			base.CheckAT_RN_NKCountry();
			MandatoryValidation.CheckEntered(Parent.AT_RN_NKCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AT_RN_NKCountryInfo, Countries);
		}

		protected override void CheckAT_TaxSystemCode()
		{
			base.CheckAT_TaxSystemCode();
			MandatoryValidation.CheckEntered(Parent.AT_TaxSystemCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AT_TaxSystemCodeInfo);
		}

		RefCountryCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = new RefCountryCollection(Parent.Factory);
				}
				return countries;
			}
		}

		RefCountryCollection countries;

		protected override void CheckAT_RateSource()
		{
			base.CheckAT_RateSource();
			MandatoryValidation.CheckEntered(Parent.AT_RateSourceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AT_RateSourceInfo);
		}
	}
}
