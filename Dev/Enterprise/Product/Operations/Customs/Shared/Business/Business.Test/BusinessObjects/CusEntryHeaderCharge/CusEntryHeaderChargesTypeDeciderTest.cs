using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusEntryHeaderChargesTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusEntryHeaderCharges = bizO as CusEntryHeaderCharges;
			if (cusEntryHeaderCharges != null)
			{
				cusEntryHeaderCharges.EntryHeader.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryHeaderCharges = entryHeader.Charges.AddNew();

			entryHeaderCharges.C1_ChargeType = "AAA";
			entryHeaderCharges.C1_ChargeAmount = 1000m;

			return entryHeaderCharges;
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryHeaderCharges);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryHeaderCharges>() },
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
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryHeaderCharges>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryHeaderCharges>() },
			};
		}
	}
}
