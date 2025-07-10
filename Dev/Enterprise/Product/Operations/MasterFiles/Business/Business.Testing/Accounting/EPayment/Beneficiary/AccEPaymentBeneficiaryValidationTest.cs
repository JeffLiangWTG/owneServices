using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccEPaymentBeneficiaryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABF_GC_Company()
		{
			beneficiary.ABF_GC_Company = ZGuid.Empty;
			AssertHasError(beneficiary.ABF_GC_CompanyInfo, "Please enter a value.");
			beneficiary.ABF_GC_Company = ZGuid.NewZGuid();
			AssertHasError(beneficiary.ABF_GC_CompanyInfo, "Beneficiary must specify a valid Company.");
			var company = Factory.New<GlbCompany>();
			beneficiary.ABF_GC_Company = company.PK;
			AssertNoErrors(beneficiary.ABF_GC_CompanyInfo);
		}

		public void TestCheckABF_ProviderReference()
		{
			beneficiary.ABF_ProviderReference = ZString.Empty;
			AssertHasError(beneficiary.ABF_ProviderReferenceInfo, "Please enter a value.");
			beneficiary.ABF_ProviderReference = "2715E17F-3C68-41B9-8D04-479F0F052BB9";
			AssertNoErrors(beneficiary.ABF_ProviderReferenceInfo);
		}

		public void TestCheckABF_ProviderCode()
		{
			beneficiary.ABF_ProviderCode = ZString.Empty;
			AssertHasError(beneficiary.ABF_ProviderCodeInfo, "Please enter a value.");
			beneficiary.ABF_ProviderCode = "AAA";
			AssertHasError(beneficiary.ABF_ProviderCodeInfo, "Enter a valid selection.");
			beneficiary.ABF_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertNoErrors(beneficiary.ABF_ProviderCodeInfo);
		}

		public void TestCheckABF_RN_NKCountryCode()
		{
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_IsActive, 1));

			beneficiary.ABF_RN_NKCountryCode = country.RN_Code;
			AssertNoErrors("Should be no errors", beneficiary.ABF_RN_NKCountryCodeInfo);

			beneficiary.ABF_RN_NKCountryCode = "ZZ";
			AssertHasErrors("Should be errors", beneficiary.ABF_RN_NKCountryCodeInfo);

			beneficiary.ABF_RN_NKCountryCode = "";
			AssertNoErrors("Should be no errors", beneficiary.ABF_RN_NKCountryCodeInfo);
		}

		public void TestCheckABF_RX_NKAccountCurrency()
		{
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_IsActive, 1));

			beneficiary.ABF_RX_NKAccountCurrency = currency.RX_Code;
			AssertNoErrors("Should be no errors", beneficiary.ABF_RX_NKAccountCurrencyInfo);

			beneficiary.ABF_RX_NKAccountCurrency = "AAA";
			AssertHasErrors("Should be errors", beneficiary.ABF_RX_NKAccountCurrencyInfo);

			beneficiary.ABF_RX_NKAccountCurrency = "";
			AssertHasErrors("Should be errors", beneficiary.ABF_RX_NKAccountCurrencyInfo);
		}

		#region Implementation

		AccEPaymentBeneficiary beneficiary;

		protected override void SetUp()
		{
			base.SetUp();
			beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
		}

		#endregion
	}
}
