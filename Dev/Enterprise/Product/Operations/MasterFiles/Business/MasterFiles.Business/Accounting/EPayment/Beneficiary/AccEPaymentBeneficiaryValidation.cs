using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentBeneficiaryValidation : AutoAccEPaymentBeneficiaryValidation
	{
		public AccEPaymentBeneficiaryValidation(AutoAccEPaymentBeneficiary parent) : base(parent)
		{
			Parent = parent as AccEPaymentBeneficiary;
		}

		protected new AccEPaymentBeneficiary Parent;

		protected override void CheckABF_GC_Company()
		{
			base.CheckABF_GC_Company();
			if (!Parent.ABF_GC_CompanyInfo.HasErrors() && Parent.Company == null)
			{
				Parent.ABF_GC_CompanyInfo.AddError(ResString.GetMultilingualString("EA28A52F-E069-404B-98CC-4BD8713BDEEC", "Beneficiary must specify a valid Company."));
			}
		}

		protected override void CheckABF_ProviderReference()
		{
			base.CheckABF_ProviderReference();
			MandatoryValidation.CheckEntered(Parent.ABF_ProviderReferenceInfo);
		}

		protected override void CheckABF_ProviderCode()
		{
			base.CheckABF_ProviderCode();
			MandatoryValidation.CheckEntered(Parent.ABF_ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ABF_ProviderCodeInfo);
		}

		protected override void CheckABF_RN_NKCountryCode()
		{
			base.CheckABF_RN_NKCountryCode();
			ListValidation.ErrorIfInvalidCode(Parent.ABF_RN_NKCountryCodeInfo);
		}

		protected override void CheckABF_RX_NKAccountCurrency()
		{
			base.CheckABF_RX_NKAccountCurrency();
			MandatoryValidation.CheckEntered(Parent.ABF_RX_NKAccountCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ABF_RX_NKAccountCurrencyInfo);
		}
	}
}
