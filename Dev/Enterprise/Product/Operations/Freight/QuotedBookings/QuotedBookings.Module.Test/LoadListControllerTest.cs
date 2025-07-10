using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(LoadListController))]
	public class LoadListControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LoadList;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobSailing sailing;
			JobVoyage voyage;
			VoyageDestination destination;
			VoyageOrigin origin;
			CommonShipment shipment1;
			CommonShipment shipment2;
			CommonShipment shipment3;
			CommonShipment shipment4;
			CommonShipment shipment5;
			voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "234";

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_E_DEP = new ZDateTime(2004, 1, 20);
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = new ZDateTime(2004, 2, 20);
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing.JX_DepotCutOff = new ZDateTime(2004, 1, 18);
			sailing.Origin.JA_ReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing.Origin.JA_CutOff = new ZDateTime(2004, 1, 19);
			Factory.Save();
			shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_A_BKD = new ZDateTime(2004, 1, 3);
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_ActualVolume = 4m;
			shipment1.JS_ActualWeight = 300m;
			shipment1.JS_OuterPacks = 6;
			shipment1.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment1);
			shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_A_BKD = new ZDateTime(2004, 1, 4);
			shipment2.JS_RL_NKOrigin = "AUMEL";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_ActualVolume = 4m;
			shipment2.JS_ActualWeight = 300m;
			shipment2.JS_OuterPacks = 6;
			shipment2.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment2);
			shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_A_BKD = new ZDateTime(2004, 1, 6);
			shipment3.JS_RL_NKOrigin = "AUBNE";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_ActualVolume = 4m;
			shipment3.JS_ActualWeight = 300m;
			shipment3.JS_OuterPacks = 6;
			shipment3.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment3);
			shipment4 = Factory.New<CommonShipment>();
			shipment4.JS_A_BKD = new ZDateTime(2004, 1, 7);
			shipment4.JS_RL_NKOrigin = "AUSYD";
			shipment4.JS_RL_NKDestination = "USLAS";
			shipment4.JS_ActualVolume = 4m;
			shipment4.JS_ActualWeight = 300m;
			shipment4.JS_OuterPacks = 6;
			shipment4.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment4);
			shipment5 = Factory.New<CommonShipment>();
			shipment5.JS_A_BKD = new ZDateTime(2004, 1, 9);
			shipment5.JS_RL_NKOrigin = "AUBNE";
			shipment5.JS_RL_NKDestination = "USLAS";
			shipment5.JS_ActualVolume = 4m;
			shipment5.JS_ActualWeight = 300m;
			shipment5.JS_OuterPacks = 6;
			shipment5.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment5);
			Factory.Save();
			return sailing;
		}
	}
}
