using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(LoadListForm))]
	public class LoadListFormTest : ZFormBasherTest
	{
		JobSailing Sailing;
		JobVoyage Voyage;
		VoyageDestination Destination;
		VoyageOrigin Origin;
		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonShipment Shipment3;
		CommonShipment Shipment4;
		CommonShipment Shipment5;
		protected override void SetUp()
		{
			base.SetUp();
			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			Origin = Factory.New<VoyageOrigin>();
			Origin.JA_JV = Voyage.PK;
			Origin.JA_E_DEP = new ZDateTime(2004, 1, 20);
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Destination = Factory.New<VoyageDestination>();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_E_ARV = new ZDateTime(2004, 2, 20);
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			Sailing = Factory.New<JobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;
			Sailing.JX_DepotReceivalCommences = new ZDateTime(2004, 1, 1);
			Sailing.JX_DepotCutOff = new ZDateTime(2004, 1, 18);
			Sailing.Origin.JA_ReceivalCommences = new ZDateTime(2004, 1, 1);
			Sailing.Origin.JA_CutOff = new ZDateTime(2004, 1, 19);
			Factory.Save();
			Shipment1 = Factory.New<CommonShipment>();
			Shipment1.JS_A_BKD = new ZDateTime(2004, 1, 3);
			Shipment1.JS_JX = Sailing.PK;
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_RL_NKDestination = "USLAX";
			Shipment1.JS_ActualVolume = 4m;
			Shipment1.JS_ActualWeight = 300m;
			Shipment1.JS_OuterPacks = 6;
			Shipment1.JS_GoodsDescription = "SDFSDFSDF";
			Sailing.Shipments.Add(Shipment1);
			Shipment2 = Factory.New<CommonShipment>();
			Shipment2.JS_A_BKD = new ZDateTime(2004, 1, 4);
			Shipment2.JS_JX = Sailing.PK;
			Shipment2.JS_RL_NKOrigin = "AUMEL";
			Shipment2.JS_RL_NKDestination = "USLAX";
			Shipment2.JS_ActualVolume = 4m;
			Shipment2.JS_ActualWeight = 300m;
			Shipment2.JS_OuterPacks = 6;
			Shipment2.JS_GoodsDescription = "SDFSDFSDF";
			Sailing.Shipments.Add(Shipment2);
			Shipment3 = Factory.New<CommonShipment>();
			Shipment3.JS_A_BKD = new ZDateTime(2004, 1, 6);
			Shipment3.JS_JX = Sailing.PK;
			Shipment3.JS_RL_NKOrigin = "AUBNE";
			Shipment3.JS_RL_NKDestination = "USLAX";
			Shipment3.JS_ActualVolume = 4m;
			Shipment3.JS_ActualWeight = 300m;
			Shipment3.JS_OuterPacks = 6;
			Shipment3.JS_GoodsDescription = "SDFSDFSDF";
			Sailing.Shipments.Add(Shipment3);
			Shipment4 = Factory.New<CommonShipment>();
			Shipment4.JS_A_BKD = new ZDateTime(2004, 1, 7);
			Shipment4.JS_JX = Sailing.PK;
			Shipment4.JS_RL_NKOrigin = "AUSYD";
			Shipment4.JS_RL_NKDestination = "USLAS";
			Shipment4.JS_ActualVolume = 4m;
			Shipment4.JS_ActualWeight = 300m;
			Shipment4.JS_OuterPacks = 6;
			Shipment4.JS_GoodsDescription = "SDFSDFSDF";
			Sailing.Shipments.Add(Shipment4);
			Shipment5 = Factory.New<CommonShipment>();
			Shipment5.JS_A_BKD = new ZDateTime(2004, 1, 9);
			Shipment5.JS_JX = Sailing.PK;
			Shipment5.JS_RL_NKOrigin = "AUBNE";
			Shipment5.JS_RL_NKDestination = "USLAS";
			Shipment5.JS_ActualVolume = 4m;
			Shipment5.JS_ActualWeight = 300m;
			Shipment5.JS_OuterPacks = 6;
			Shipment5.JS_GoodsDescription = "SDFSDFSDF";
			Sailing.Shipments.Add(Shipment5);
			Factory.Save();
		}

		protected override Form GetFormToBashCore()
		{
			return new LoadListForm(Sailing);
		}
	}
}
