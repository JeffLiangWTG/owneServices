using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class BaseInvoiceChargeTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseInvoiceCharge = bizO as BaseInvoiceCharge;
			if (baseInvoiceCharge != null)
			{
				baseInvoiceCharge.Invoice.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader.Charges.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(BaseInvoiceCharge);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IInvoiceCharge>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceCharge>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IInvoiceCharge>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceCharge>() },
			};
		}
	}
}
