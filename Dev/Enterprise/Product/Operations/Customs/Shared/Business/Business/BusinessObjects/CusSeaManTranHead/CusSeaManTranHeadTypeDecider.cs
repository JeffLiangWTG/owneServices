using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManTranHeadTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.ICusSeaManTranHead>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusSeaManTranHead);
	}
}
