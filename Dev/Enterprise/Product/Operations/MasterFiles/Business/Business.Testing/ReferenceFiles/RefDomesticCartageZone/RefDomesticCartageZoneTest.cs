using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefDomesticCartageZone))]
	sealed class RefDomesticCartageZoneTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetZone()
		{
			var zone1 = CreateZone("A", "CAXXX", "XXX", "12345", "city1");
			var zone2 = CreateZone("B", "USLAX", "LAX", "98765", "city2");
			var zone3 = CreateZone("B", "AUMEL", "MEL", "98765", "city3");

			var zone4 = CreateZone("C", "AUSYD", "SYD", "33333", "city4");
			var zone5 = CreateZone("D", "AUSYD", "SYD", "33333", "city5");
			var zone6 = CreateZone("E", "AUSYD", "SYD", "33333", "");

			AssertEquals(zone1, RefDomesticCartageZone.GetZone(Factory, "12345", "CAXXX"));
			AssertEquals(zone1, RefDomesticCartageZone.GetZone(Factory, "1234567", "CAXXX"));

			AssertEquals(null, RefDomesticCartageZone.GetZone(Factory, "1234567", "USLAX"));
			AssertEquals(null, RefDomesticCartageZone.GetZone(Factory, "1234", "AUSYD"));
			AssertEquals(zone2, RefDomesticCartageZone.GetZone(Factory, "98765", "USLAX"));
			AssertEquals(null, RefDomesticCartageZone.GetZone(Factory, "987654", "USLAX"));
			AssertEquals(zone3, RefDomesticCartageZone.GetZone(Factory, "98765", "AUMEL"));

			AssertEquals(zone4, RefDomesticCartageZone.GetZone(Factory, "33333", "AUSYD", "city4"));
			AssertEquals(zone5, RefDomesticCartageZone.GetZone(Factory, "33333", "AUSYD", "city5"));
			AssertEquals(zone4, RefDomesticCartageZone.GetZone(Factory, "33333", "AUSYD", ""));

			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "AUXXX";
			AssertEquals(null, RefDomesticCartageZone.GetZone(Factory, "33333", "AUXXX", "city5"));

			port.RL_IATA = "SYD";
			AssertEquals(zone5, RefDomesticCartageZone.GetZone(Factory, "33333", "AUXXX", "city5"));
		}

		public void TestHumanReadableNameCore()
		{
			var cartageZone = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			cartageZone.F1_AirportCity = string.Empty;
			cartageZone.F1_Zone = string.Empty;

			AssertEquals("Port Transport Zones (ACI)", cartageZone.HumanReadableName);

			cartageZone.F1_AirportCity = "SYD";

			AssertEquals("Port Transport Zones (ACI) - Airport: SYD", cartageZone.HumanReadableName);

			cartageZone.F1_Zone = "A";

			AssertEquals("Port Transport Zones (ACI) - Airport: SYD - Zone: A", cartageZone.HumanReadableName);
		}

		#region Implementation

		RefDomesticCartageZone CreateZone(ZString zoneName, ZString loco, ZString iATA, ZString postCode, ZString city)
		{
			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, loco));
			if (port == null)
			{
				port = Factory.NewWithValidTestData<RefUNLOCO>();
				port.RL_Code = loco;
				port.RL_IATA = iATA;
			}

			var zone = Factory.New<RefDomesticCartageZone>();
			zone.F1_Zone = zoneName;
			zone.F1_RL_NKLoco = loco;
			zone.F1_PortCode = iATA;
			zone.F1_CityTown = city;
			zone.F1_CityTownPostCode = postCode;

			return zone;
		}

		#endregion
	}
}
