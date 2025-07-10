using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class BaseInvoiceLineApportionedChargeCountrySpecificTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseInvoiceLineApportionedCharge = bizO as BaseInvoiceLineApportionedCharge;
			if (baseInvoiceLineApportionedCharge != null)
			{
				baseInvoiceLineApportionedCharge.InvoiceLine.InvoiceHeader.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoice = declaration.Invoices.AddNew();
			var lines = invoice.JobComInvoiceLines.AddNew();
			var charge = lines.ApportionedCharges.AddNew();
			return charge;
		}

		protected override Type BaseTypeDecidedType => typeof(BaseInvoiceLineApportionedCharge);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IInvoiceLineApportionedCharge>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceLineApportionCharge>() },
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
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IInvoiceLineApportionedCharge>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IInvoiceLineApportionCharge>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceLineApportionCharge>() },
			};
		}
	}
}
