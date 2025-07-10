using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Freight.Business
{
	public static class PortMessagingCountryList
	{
		public static IEnumerable<Guid> FranceAndOverseasDepartments
		{
			get
			{
				return franceAndOverseasDepartments ?? (franceAndOverseasDepartments = new[]
				{
					Core.Constants.CountryGuids.France,
					Core.Constants.CountryGuids.FrenchGuiana,
					Core.Constants.CountryGuids.FrenchPolynesia,
					Core.Constants.CountryGuids.Guadeloupe,
					Core.Constants.CountryGuids.Martinique,
					Core.Constants.CountryGuids.Mayotte,
					Core.Constants.CountryGuids.NewCaledonia,
					Core.Constants.CountryGuids.Reunion,
					Core.Constants.CountryGuids.SaintBarthelemy,
					Core.Constants.CountryGuids.SaintMartin,
					Core.Constants.CountryGuids.SaintPierreandMiquelon,
					Core.Constants.CountryGuids.WallisandFutuna,
					Core.Constants.CountryGuids.FrenchSouthernTerritories
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IEnumerable<Guid> franceAndOverseasDepartments;

		public static IEnumerable<Guid> France
		{
			get
			{
				return france ?? (france = new[]
				{
					Core.Constants.CountryGuids.France
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IEnumerable<Guid> france;
	}
}
