using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusEquipmentTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEquipment>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEquipment);

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusEquipment>();
	}
}
