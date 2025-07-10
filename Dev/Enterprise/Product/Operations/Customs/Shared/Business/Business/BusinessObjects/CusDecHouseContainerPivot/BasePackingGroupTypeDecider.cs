using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BasePackingGroupTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetBasePackingGroupCountryCode(row, factory));

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.IPackingGroup>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.IPackingGroup>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.IPackingGroup>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.IPackingGroup>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.IPackingGroup>(); }),
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IPackingGroup>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BasePackingGroup);

		ZString GetBasePackingGroupCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var housebillPK = (row != null) ? new ZGuid(row[BasePackingGroup.Schema.CR_CU_HouseBill]) : ZGuid.Invalid;
			var housebill = (housebillPK.IsValid) ? factory.Load<Bill>(housebillPK) : null;
			return housebill?.Declaration is BaseJobDeclaration declaration ? declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
