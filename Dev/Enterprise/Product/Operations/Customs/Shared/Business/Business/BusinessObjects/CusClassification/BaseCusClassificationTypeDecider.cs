using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BaseCusClassificationTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetClassificationCountryCode(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.IClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Integration.Customs.MY.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.EU.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.ICusClassification>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusClassification>(); }),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusClassification>(); })
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusClassification>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseCusClassification);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusClassification>);
			}
		}

		ZString GetClassificationCountryCode(System.Data.DataRow row, BusinessObjectFactory factory) => (row != null) ? new ZString(row[BaseCusClassification.Schema.CC_RN_NKCountryCode]) : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
