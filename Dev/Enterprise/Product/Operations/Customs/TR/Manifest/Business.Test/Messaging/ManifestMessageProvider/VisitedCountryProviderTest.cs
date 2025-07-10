using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class VisitedCountryProviderTest : TestCaseWithFactory
	{
		public void TestVehicleVisitedCountryMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				IVehicleVisitedCountry vehicleVisitedCountry = sumDec.VehicleVisitedCountry.FirstOrDefault();
				CombineAssertions("Vehicle Visited Countries (DENIHR)", () =>
				{
					AssertEquals("PortLocationName", "TRIST", vehicleVisitedCountry.PortLocationName);
					AssertEquals("CountryCode", "052", vehicleVisitedCountry.CountryCode);
					AssertEquals("MovementDateTime", ZDateTime.Today, vehicleVisitedCountry.MovementDateTime);
				});
				header.AMA_TransportMode = "SEA";
				header.AMA_ManifestType = "CIKONC";
				sumDec = new ManifestMessageProvider(header);
				vehicleVisitedCountry = sumDec.VehicleVisitedCountry.FirstOrDefault();
				AssertEquals("MovementDateTime - CIKONC / SEA", ZDateTime.Empty, vehicleVisitedCountry.MovementDateTime);
				header.AMA_TransportMode = "AIR";
				header.AMA_ManifestType = "CIKONC";
				sumDec = new ManifestMessageProvider(header);
				vehicleVisitedCountry = sumDec.VehicleVisitedCountry.FirstOrDefault();
				AssertEquals("MovementDateTime - CIKONC / AIR", ZDateTime.Empty, vehicleVisitedCountry.MovementDateTime);
				header.AMA_TransportMode = "ROA";
				header.AMA_ManifestType = "CIKONC";
				sumDec = new ManifestMessageProvider(header);
				vehicleVisitedCountry = sumDec.VehicleVisitedCountry.FirstOrDefault();
				AssertEquals("MovementDateTime - CIKONC / ROA", ZDateTime.Today, vehicleVisitedCountry.MovementDateTime);
				header.AMA_TransportMode = "AIR";
				header.VisitedPorts[0].CY_Code = "";
				header.VisitedPorts[0].CY_Data = "TRIST";
				var billVisitedPorts = header.Bills[0].VisitedPorts.AddNew();
				billVisitedPorts.CY_Data = "TRIST";
				sumDec = new ManifestMessageProvider(header);
				vehicleVisitedCountry = sumDec.VehicleVisitedCountry.FirstOrDefault();
				IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
				IBillVisitedCountry billVisitedCountry = billofLading.BillVisitedCountry.FirstOrDefault();
				CombineAssertions("Bill Visited Countries", () =>
				{
					AssertEquals("Vehicle PortLocationName", "IST", vehicleVisitedCountry.PortLocationName);
					AssertEquals("Vehicle CountryCode", "052", vehicleVisitedCountry.CountryCode);
					AssertEquals("Vehicle MovementDateTime", ZDateTime.Today, vehicleVisitedCountry.MovementDateTime);
					AssertEquals("Bill PortLocationName", "IST", billVisitedCountry.PortLocationName);
					AssertEquals("Bill CountryCode", "052", billVisitedCountry.CountryCode);
				});
			}
		}
	}
}
