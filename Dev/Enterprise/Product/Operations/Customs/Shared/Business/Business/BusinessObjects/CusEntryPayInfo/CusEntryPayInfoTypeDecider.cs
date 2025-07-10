using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusEntryPayInfoTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryPayInfo>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.Shared.ICusEntryPayInfo>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusEntryPayInfo>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.ICusEntryPayInfo>(); }),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEntryPayInfo);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type type = null;

			var countryCode = GetDeclarationCountryCode(row, factory);

			if (countryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				var decider = (CusEntryPayInfoTypeDecider)TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryPayInfo>());
				type = decider.GetTypeForCountryCode(countryCode);
				type = ((TypeDecider)ObjectFactory.Get<Integration.Customs.ZA.ICusEntryPayInfoTypeDecider>()).GetTypeForLoad(row, factory);
			}

			return type ?? base.GetTypeForLoad(row, factory);
		}

		protected ZString GetDeclarationCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var entryHeaderPK = (row != null) ? new ZGuid(row[CusEntryPayInfo.Schema.C9_CH]) : ZGuid.Invalid;
			var entryHeader = entryHeaderPK.IsValid ? factory.Load<CusEntryHeader>(entryHeaderPK) : null;
			return (entryHeader != null) ? entryHeader.Declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
