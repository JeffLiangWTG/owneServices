using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusRulingCombinedTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCountryCodeFromRow(row));

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusRuling>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(ZZRefCusRulingCombined);

		ZString GetCountryCodeFromRow(System.Data.DataRow row) => row != null ? new ZString(row[ZZRefCusRulingCombinedSchema.ZZX_RN_NKCountryCode.Name]) : ZString.Empty;
	}
}
