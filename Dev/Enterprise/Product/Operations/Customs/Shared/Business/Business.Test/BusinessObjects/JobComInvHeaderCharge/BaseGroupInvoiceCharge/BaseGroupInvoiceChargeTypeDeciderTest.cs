using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class BaseGroupInvoiceChargeTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseGroupInvoiceCharge = bizO as BaseGroupInvoiceCharge;
			if (baseGroupInvoiceCharge != null)
			{
				baseGroupInvoiceCharge.GroupInvoice.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			return invoiceGroupHeader.Charges.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(BaseGroupInvoiceCharge);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IGroupInvoiceCharge>() },
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
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IGroupInvoiceCharge>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IGroupInvoiceCharge>() },
			};
		}
	}
}
