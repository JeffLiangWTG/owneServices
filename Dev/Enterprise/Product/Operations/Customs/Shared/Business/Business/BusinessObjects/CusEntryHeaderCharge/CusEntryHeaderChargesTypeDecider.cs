using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderChargesTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetEntryHeaderChargesCountryCode(row, factory));
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusEntryHeaderCharges>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusEntryHeaderCharges>(); }),
				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryHeaderCharges>(); }),
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusEntryHeaderCharges>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEntryHeaderCharges);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeaderCharges>);
			}
		}

		ZString GetEntryHeaderChargesCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var entryHeaderPK = (row != null) ? new ZGuid(row[CusEntryHeaderCharges.Schema.C1_CH]) : ZGuid.Invalid;
			var entryHeader = (entryHeaderPK.IsValid) ? factory.Load<CusEntryHeader>(entryHeaderPK) : null;
			return (entryHeader != null) ? entryHeader.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
