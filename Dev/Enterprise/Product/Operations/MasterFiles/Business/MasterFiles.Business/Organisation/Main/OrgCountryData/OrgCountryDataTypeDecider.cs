using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(row[OrgCountryDataSchema.OV_RN_NKClientCountryRelation.Name].ToString());
		}

		public override Type GetTypeForNew()
		{
			return GetTypeForCountry(GlbCompany.CurrentCompany.Country);
		}

		public override Type GetTypeForBinding()
		{
			return GetTypeForCountry(GlbCompany.CurrentCompany.Country);
		}

		Type GetTypeForCountry(RefCountry country)
		{
			return GetTypeForCountryCode(country.Code);
		}

		Type GetTypeForCountryCode(string countryCode)
		{
			return GetTypeForCountryOrEUCode(Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(countryCode) ? Constants.CountryCodes.EuropeanUnion : countryCode);
		}

		Type GetTypeForCountryOrEUCode(string countryOrEUCode)
		{
			Type result = null;

			switch (countryOrEUCode)
			{
				case Constants.CountryCodes.Australia:
					result = typeof(OrgCountryDataAU);
					break;

				case Constants.CountryCodes.EuropeanUnion:
					result = typeof(OrgCountryDataEU);
					break;

				case Constants.CountryCodes.Japan:
					result = typeof(OrgCountryDataJP);
					break;

				case Constants.CountryCodes.HongKong:
					result = typeof(OrgCountryDataHK);
					break;

				default:
					result = typeof(OrgCountryData);
					break;
			}

			return result;
		}
	}
}
