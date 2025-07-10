using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentCollectionTest : BaseFreightTest
	{
		#region TestAdd

		public void TestAdd()
		{
			JobSailing sailing1 = SetUpSailing("APL EMERALD", "12", "AUSYD", "GBLON", ZDateTime.Today, ZDateTime.Today.AddDays(34));
			JobSailing sailing2 = SetUpSailing("APL IVORY", "80", "AUSYD", "GBLON", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(38));

			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_JX = sailing2.PK;
			shipment.JS_IsBooking = true;

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = sailing1.PK;

			consol.Shipments.Add(shipment);

			Factory.Save();

			AssertEquals("Expecting Shipment to return consol's sailing", consol.Schedule.PK, shipment.Sailing.PK);

			consol.Shipments.Remove(shipment);

			Assert("Not expecting shipment to be deleted.", !shipment.IsDeleted);

			Assert("Expecting Shipment to have an empty sailing", shipment.JS_JX.IsEmpty);
		}

		#region TestAttachedShipmentClientRefDefaultToConsolValue
		public void TestAttachedShipmentClientRefDefaultToConsolValue()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_AgentsReference = "ABC";
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_ConsolReference = "12345";
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(shipment1);
			Factory.Save();
			AssertEquals("Shipment client ref is set to Consol Client Ref", consol.JK_AgentsReference, shipment.JS_ConsolReference);
			AssertEquals("Shipment1 client ref is not set to Consol Client Ref", "12345", shipment1.JS_ConsolReference);

			CFSShipment shipment2 = consol.Shipments.AddNew();
			AssertEquals("Shipment client ref is set to Consol Client ref", consol.JK_AgentsReference, shipment2.JS_ConsolReference);
		}
		#endregion

		#endregion

		#region Implementation

		JobSailing SetUpSailing(ZString vessel, ZString voyageNo, ZString load, ZString discharge, ZDateTime eTD, ZDateTime eTA)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageNo;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = eTD;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = eTA;

			voyage.GenerateSailings();

			Factory.Save();
			return voyage.Sailings[0];
		}

		#endregion
	}
}
