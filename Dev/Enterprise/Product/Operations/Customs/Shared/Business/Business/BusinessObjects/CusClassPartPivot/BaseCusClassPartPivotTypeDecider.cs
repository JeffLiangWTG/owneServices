using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseCusClassPartPivotTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetClassPartPivotCountryCode(row, factory));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.ICusClassPartPivot>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusClassPartPivot>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseCusClassPartPivot);

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusClassPartPivot>();

		ZString GetClassPartPivotCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var result = (row != null) ? new ZString(row[CusClassPartPivotSchema.CI_RN_NKCountry.Name]) : ZString.Empty;
			if (result.IsEmpty)
			{
				var classificationPK = (row != null) ? new ZGuid(row[CusClassPartPivotSchema.CI_CC.Name]) : ZGuid.Invalid;
				var classification = factory.Load<BaseCusClassification>(classificationPK);
				result = (classification != null) ? classification.CC_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			return result;
		}
	}
}
