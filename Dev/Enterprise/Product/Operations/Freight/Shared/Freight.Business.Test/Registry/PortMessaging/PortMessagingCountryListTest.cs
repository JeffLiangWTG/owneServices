using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PortMessagingCountryListTest : TestCase
	{
		public void TestFranceAndOverseasDepartments()
		{
			var list = new List<Guid>
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
			};
			AssertContainsExactElementsInAnyOrder(PortMessagingCountryList.FranceAndOverseasDepartments, list);
		}

		public void TestFrance()
		{
			var list = new List<Guid>
			{
				Core.Constants.CountryGuids.France
			};
			AssertContainsExactElementsInAnyOrder(PortMessagingCountryList.France, list);
		}
	}
}
