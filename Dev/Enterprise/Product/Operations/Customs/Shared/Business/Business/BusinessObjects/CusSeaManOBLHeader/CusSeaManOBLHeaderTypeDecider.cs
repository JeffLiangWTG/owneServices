using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.ICusSeaManOBLHeader>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusSeaManOBLHeader);
	}
}
