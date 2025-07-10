using System;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ImpAddInfoTypeDecider
	{
		public Type GetCountrySpecificTypeForNewOrgAddInfo(OrgCountryData countryData)
		{
			var clientCountryRelation = countryData.OV_RN_NKClientCountryRelation;
			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(clientCountryRelation))
			{
				case Core.Constants.CountryCodes.Australia:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Canada:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.China:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.France:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Germany:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.KoreaSouth:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Netherlands:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Spain:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Taiwan:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Turkey:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.UnitedKingdom:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.GB.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.UnitedStates:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Brazil:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgImpAddInfo>();

				case Core.Constants.CountryCodes.India:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgImpAddInfo>();

				default:
					throw new NotSupportedException("You should have a country-specific Org AddInfo for " + clientCountryRelation);
			}
		}

		public Type GetRegionSpecificTypeForNewOrgAddInfo(OrgCountryData countryData) => GetRegionSpecificTypeForNewOrgAddInfo(countryData.OV_RN_NKClientCountryRelation);

		public Type GetRegionSpecificTypeForNewOrgAddInfo(ZString clientCountryRelation)
		{
			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(clientCountryRelation))
			{
				case Core.Constants.CountryCodes.France:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IRegionOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Ireland:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IE.IRegionOrgImpAddInfo>();

				case Core.Constants.CountryCodes.Netherlands:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IRegionOrgImpAddInfo>();

				default:
					if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(clientCountryRelation))
					{
						return ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgImpAddInfo>();
					}
					throw new NotSupportedException("You should have a region-specific Org AddInfo for " + clientCountryRelation);
			}
		}
	}
}
