using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceLineTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.IJobComInvoiceLine>();

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseJobComInvoiceLine = bizO as BaseJobComInvoiceLine;
			if (baseJobComInvoiceLine != null)
			{
				baseJobComInvoiceLine.InvoiceHeader.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoice = declaration.Invoices.AddNew();
			return invoice.JobComInvoiceLines.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(BaseJobComInvoiceLine);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Denmark, ObjectFactory.GetType<Integration.Customs.DK.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Finland, ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids._EUTemplateCountryName_, ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobComInvoiceLine>() },
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
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Denmark, ObjectFactory.GetType<Integration.Customs.DK.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Finland, ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes._EUTemplateCountryName_, ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobComInvoiceLine>() },
			};
		}
	}
}
