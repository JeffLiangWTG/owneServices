using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseCusClassPartPivotTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.ICusClassPartPivot>();

		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var partPivot = bizO as BaseCusClassPartPivot;
			if (partPivot != null)
			{
				var classification = Factory.Load<BaseCusClassification>(partPivot.CI_CC);
				((IBusinessObjectInternals)partPivot).Row[BaseCusClassPartPivot.Schema.CI_RN_NKCountry] = countryCode;
				partPivot.HasChanges = true;
				classification.CC_RN_NKCountryCode = countryCode;
				if (countryCode == Core.Constants.CountryCodes.Australia)
				{
					((IBusinessObjectInternals)partPivot).Row[BaseCusClassPartPivot.Schema.CI_RN_NKCountry] = ZString.Empty;
				}
				else if (countryCode == Core.Constants.CountryCodes.NewZealand)
				{
					classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			var partPivot = (BaseCusClassPartPivot)Factory.New(BaseTypeDecidedType);
			partPivot.CI_OP = part.PK;
			partPivot.CI_CC = classification.PK;
			return partPivot;
		}

		protected override Type BaseTypeDecidedType => typeof(BaseCusClassPartPivot);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusClassPartPivot>() },
				{ Core.Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusClassPartPivot>() },
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
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusClassPartPivot>() },
				{ Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusClassPartPivot>() },
			};
		}

		#endregion
	}
}
