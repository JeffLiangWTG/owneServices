using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusEntryPayInfoTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var payInfo = bizO as CusEntryPayInfo;
			if (payInfo != null)
			{
				payInfo.EntryHeader.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			return header.EntryPayInfos.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryPayInfo);

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryPayInfo>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryPayInfo>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryPayInfo>() },
			};
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryPayInfo>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryPayInfo>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryPayInfo>() },
			};
		}
	}
}
