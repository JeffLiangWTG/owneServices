using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusEntryInstructionTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var entryInstruction = bizO as CusEntryInstruction;
			if (entryInstruction != null)
			{
				entryInstruction.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testProvider = new EntryInstructionProvider(declaration);

			return testProvider.CustomsEntryInstructions.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryInstruction);

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryInstruction>() },
				{ Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryInstruction>() },
				{ Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryInstruction>() },
				{ Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryInstruction>() },
				{ Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Sweden, ObjectFactory.GetType<Integration.Customs.SE.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryInstruction>() },
				{ Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryInstruction>() },
				{ Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryInstruction>() },
				{ Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryInstruction>() },
			};
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryInstruction>() },
				{ Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryInstruction>() },
				{ Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryInstruction>() },
				{ Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryInstruction>() },
				{ Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Sweden, ObjectFactory.GetType<Integration.Customs.SE.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryInstruction>() },
				{ Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryInstruction>() },
				{ Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryInstruction>() },
				{ Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryInstruction>() },
			};
		}
	}
}
