using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolInvoicingSupporter))]
	public class CFSLoadListConsolInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestActualAndEstimatedArrivalAtLoadPort()
		{
			var today = ZDateTime.Today;
			var eTD = today.AddDays(10);
			var eTA = today.AddDays(11);
			var sailing_E_ATL = today.AddDays(20);
			var sailing_A_ATL = today.AddDays(21);

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports.MostInterestingTransport.JW_ETD = eTD;
			consol.Transports.MostInterestingTransport.JW_ETA = eTA;

			var supporter = consol.InvoicingSupporter;
			AssertEquals("ATL", ZDateTime.Empty, supporter.ArrivalAtLoadPort);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "12";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First().RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			AssertNotNull("Sailing should exist", sailing);

			var departureTransport = consol.Transports.MostInterestingTransport;

			departureTransport.JW_IsLinked = false;
			AssertEquals("DepartureTransport is NOT Linked", false, departureTransport.JW_IsLinked);
			AssertEquals("DepartureTransport has NO Sailing", null, departureTransport.Sailing);
			AssertEquals("ATL: Departure Transport is NOT Linked - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Departure Transport is NOT Linked - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			departureTransport.JW_IsLinked = true;
			departureTransport.JW_JX = sailing.PK;
			AssertEquals("DepartureTransport is Linked", true, departureTransport.JW_IsLinked);
			AssertEquals("DepartureTransport has Sailing", sailing, departureTransport.Sailing);
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, departureTransport.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: Empty", ZDateTime.Empty, departureTransport.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			origin.JA_E_ARV = sailing_E_ATL;
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, departureTransport.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, departureTransport.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL is Empty - Sailing E_ATL", sailing_E_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing A_ATL is Empty - Sailing E_ATL", sailing_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			origin.JA_A_ARV = sailing_A_ATL;
			AssertEquals("Sailing A_ATL: sailing_A_ATL", sailing_A_ATL, departureTransport.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, departureTransport.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL", sailing_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing E_ATL", sailing_E_ATL, supporter.EstimatedArrivalAtLoadPort);
		}

		public void TestVoyageVesselOrFlightDate()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_VoyageFlight = "TEST54321";
			consol.Transports[0].JW_Vessel = "ENTERPRISE";

			var supporter = new CFSLoadListConsolInvoicingSupporter(consol);
			AssertEquals("TEST54321/ENTERPRISE", supporter.VoyageVesselOrFlightDate);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			CFSLoadListConsol cfsLoadListConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			return cfsLoadListConsol;
		}
	}
}
