using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class InvoiceLineChargeTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseInvoiceLineCharge = bizO as BaseInvoiceLineCharge;
			if (baseInvoiceLineCharge != null)
			{
				baseInvoiceLineCharge.InvoiceLine.InvoiceHeader.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoice = declaration.Invoices.AddNew();
			var lines = invoice.JobComInvoiceLines.AddNew();
			var charge = lines.Charges.AddNew();
			return charge;
		}

		protected override Type BaseTypeDecidedType => typeof(BaseInvoiceLineCharge);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IInvoiceLineCharge>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceLineCharge>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IInvoiceLineCharge>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceLineCharge>() },
			};
		}
	}
}
