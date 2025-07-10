using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManSlotOrgTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusSeaManSlotOrg);
	}
}
