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
	public class CusMiscRequestHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetCountryCode(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		static string GetCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var branch = factory.Load<GlbBranch>(new ZGuid(row[CusMiscRequestHeader.Schema.CMR_GB]));
			var company = branch?.Company ?? GlbCompany.CurrentCompany;
			return company.GC_RN_NKCountryCode;
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusMiscRequestHeader>(); }),
			};
		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusMiscRequestHeader);
	}
}
