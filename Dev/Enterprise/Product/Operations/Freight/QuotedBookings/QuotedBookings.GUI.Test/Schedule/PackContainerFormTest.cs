using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	#region Form Bashing

	abstract class BashFormForTransportMode : ZFormBasherTest
	{
		protected abstract ZString TransportType { get; }

		#region GetSailing

		protected JobSailing GetSailing()
		{
			base.SetUp();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "234";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = TransportType;

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_E_DEP = new ZDateTime(2004, 1, 20);
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = new ZDateTime(2004, 2, 20);
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing.JX_DepotCutOff = new ZDateTime(2004, 1, 18);
			sailing.Origin.JA_ReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing.Origin.JA_CutOff = new ZDateTime(2004, 1, 19);

			//Factory.Save();

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "112";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			voyage2.JV_AirSeaRoad = TransportType;

			VoyageOrigin origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_JV = voyage2.PK;
			origin2.JA_E_DEP = new ZDateTime(2004, 1, 26);
			origin2.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination2 = Factory.New<VoyageDestination>();
			destination2.JB_JV = voyage2.PK;
			destination2.JB_E_ARV = new ZDateTime(2004, 2, 26);
			destination2.JB_RL_NKPortOfDischarge = "USLAX";

			JobSailing sailing2 = Factory.New<JobSailing>();
			sailing2.JX_JA = origin2.PK;
			sailing2.JX_JB = destination2.PK;
			sailing2.JX_DepotCutOff = new ZDateTime(2004, 1, 24);
			sailing2.JX_DepotReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing2.Origin.JA_ReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing2.Origin.JA_CutOff = new ZDateTime(2004, 1, 25);

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_A_BKD = new ZDateTime(2004, 1, 3);
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_ActualVolume = 4m;
			shipment1.JS_ActualWeight = 300m;
			shipment1.JS_OuterPacks = 6;
			shipment1.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment1);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_A_BKD = new ZDateTime(2004, 1, 4);
			shipment2.JS_RL_NKOrigin = "AUMEL";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_ActualVolume = 4m;
			shipment2.JS_ActualWeight = 300m;
			shipment2.JS_OuterPacks = 6;
			shipment2.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment2);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_A_BKD = new ZDateTime(2004, 1, 21);
			shipment3.JS_RL_NKOrigin = "AUBNE";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_ActualVolume = 4m;
			shipment3.JS_ActualWeight = 300m;
			shipment3.JS_OuterPacks = 6;
			shipment3.JS_GoodsDescription = "SDFSDFSDF";
			sailing2.Shipments.Add(shipment3);

			CommonShipment shipment4 = Factory.New<CommonShipment>();
			//Shipment4.JS_JX = Sailing.PK;
			shipment4.JS_A_BKD = new ZDateTime(2004, 1, 7);
			shipment4.JS_RL_NKOrigin = "AUSYD";
			shipment4.JS_RL_NKDestination = "USLAX";
			shipment4.JS_ActualVolume = 4m;
			shipment4.JS_ActualWeight = 300m;
			shipment4.JS_OuterPacks = 6;
			shipment4.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment4);

			CommonShipment shipment5 = Factory.New<CommonShipment>();
			//Shipment5.JS_JX = Sailing.PK;
			shipment5.JS_A_BKD = new ZDateTime(2004, 1, 23);
			shipment5.JS_RL_NKOrigin = "AUBNE";
			shipment5.JS_RL_NKDestination = "USLAX";
			shipment5.JS_ActualVolume = 4m;
			shipment5.JS_ActualWeight = 300m;
			shipment5.JS_OuterPacks = 6;
			shipment5.JS_GoodsDescription = "SDFSDFSDF";
			sailing2.Shipments.Add(shipment5);

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
			shipment3.JS_A_BKD = new ZDateTime(2004, 1, 21);
			shipment3.JS_RL_NKOrigin = "AUBNE";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_ActualVolume = 4m;
			shipment3.JS_ActualWeight = 300m;
			shipment3.JS_OuterPacks = 6;
			shipment3.JS_GoodsDescription = "SDFSDFSDF";
			sailing2.Shipments.Add(shipment3);

			shipment4 = Factory.New<CommonShipment>();
			shipment4.JS_JX = sailing.PK;
			shipment4.JS_A_BKD = new ZDateTime(2004, 1, 7);
			shipment4.JS_RL_NKOrigin = "AUSYD";
			shipment4.JS_RL_NKDestination = "USLAS";
			shipment4.JS_ActualVolume = 4m;
			shipment4.JS_ActualWeight = 300m;
			shipment4.JS_OuterPacks = 6;
			shipment4.JS_GoodsDescription = "SDFSDFSDF";
			sailing.Shipments.Add(shipment4);

			shipment5 = Factory.New<CommonShipment>();
			shipment5.JS_JX = sailing.PK;
			shipment5.JS_A_BKD = new ZDateTime(2004, 1, 23);
			shipment5.JS_RL_NKOrigin = "AUBNE";
			shipment5.JS_RL_NKDestination = "USLAS";
			shipment5.JS_ActualVolume = 4m;
			shipment5.JS_ActualWeight = 300m;
			shipment5.JS_OuterPacks = 6;
			shipment5.JS_GoodsDescription = "SDFSDFSDF";
			sailing2.Shipments.Add(shipment5);

			Factory.Save();

			return sailing;
		}

		#endregion

		protected override Form GetFormToBashCore()
		{
			PackContainerHelper sailingHelper = new PackContainerHelper(GetSailing());
			return new PackContainerForm(sailingHelper);
		}
	}

	[TestedType(typeof(PackContainerForm))]
	class BashFormForSEA : BashFormForTransportMode
	{
		protected override ZString TransportType
		{
			get { return Core.Constants.TransportModes.Sea; }
		}
	}

	[TestedType(typeof(PackContainerForm))]
	class BashFormForRAI : BashFormForTransportMode
	{
		protected override ZString TransportType
		{
			get { return Core.Constants.TransportModes.Rail; }
		}
	}

	[TestedType(typeof(PackContainerForm))]
	class BashFormForROA : BashFormForTransportMode
	{
		protected override ZString TransportType
		{
			get { return Core.Constants.TransportModes.Road; }
		}
	}

	[TestedType(typeof(PackContainerForm))]
	class BashFormForAIR : BashFormForTransportMode
	{
		protected override ZString TransportType
		{
			get { return Core.Constants.TransportModes.Air; }
		}
	}

	#endregion

	#region PackContainerFormTest

	public class PackContainerFormTest : BaseFreightTest
	{
		JobSailing GetSailing
		{
			get
			{
				var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.JV_VoyageFlight = "234";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				VoyageOrigin origin = Factory.New<VoyageOrigin>();
				origin.JA_JV = voyage.PK;
				origin.JA_E_DEP = new ZDateTime(2004, 1, 20);
				origin.JA_RL_NKPortOfLoading = "AUSYD";

				VoyageDestination destination = Factory.New<VoyageDestination>();
				destination.JB_JV = voyage.PK;
				destination.JB_E_ARV = new ZDateTime(2004, 2, 20);
				destination.JB_RL_NKPortOfDischarge = "USLAX";

				JobSailing sailing = Factory.New<JobSailing>();
				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;
				sailing.JX_DepotCutOff = new ZDateTime(2004, 1, 18);
				sailing.JX_DepotReceivalCommences = new ZDateTime(2004, 1, 1);
				sailing.Origin.JA_ReceivalCommences = new ZDateTime(2004, 1, 1);
				sailing.Origin.JA_CutOff = new ZDateTime(2004, 1, 19);
				Factory.Save();

				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_A_BKD = new ZDateTime(2004, 1, 3);
				shipment1.JS_RL_NKOrigin = "AUSYD";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_ActualVolume = 4m;
				shipment1.JS_ActualWeight = 300m;
				shipment1.JS_OuterPacks = 6;
				shipment1.JS_GoodsDescription = "SDFSDFSDF";
				shipment1.JS_A_RCV = ZDateTime.Today; //IsReceived

				var packline1 = shipment1.OuterPackLines.AddNew();
				packline1.JL_PackageCount = 1;
				packline1.JL_ActualVolume = 1m;
				packline1.JL_ActualWeight = 1m;
				var packline2 = shipment1.OuterPackLines.AddNew();
				packline2.JL_PackageCount = 2;
				packline2.JL_ActualVolume = 2m;
				packline2.JL_ActualWeight = 2m;
				var packline3 = shipment1.OuterPackLines.AddNew();
				packline3.JL_PackageCount = 3;
				packline3.JL_ActualVolume = 3m;
				packline3.JL_ActualWeight = 3m;
				var packline4 = shipment1.OuterPackLines.AddNew();
				packline4.JL_PackageCount = 4;
				packline4.JL_ActualVolume = 4m;
				packline4.JL_ActualWeight = 4m;

				sailing.Shipments.Add(shipment1);

				Factory.Save();
				return sailing;
			}
		}

		public void TestDeleteContainer()
		{
			AssertEquals("Should have 0 containers", 0, SailingHelper.Containers.Count);
			AssertEquals("Should have 5 unallocated packlines", 5, Sailing.UnAllocatedPackLines.Count);

			using (PackContainerForm packContainerForm = new PackContainerForm(SailingHelper))
			{
				packContainerForm.Show();
				AssertEquals("Should have 1 container", 1, SailingHelper.Containers.Count);

				ForwardingContainer container = SailingHelper.Containers[0];
				container.AddPackLines(Sailing.UnAllocatedPackLines.ToArray());

				AssertEquals("Should have 5 packlines", 5, container.PackLines.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //has packlines
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //rejoin pack2
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //rejoin pack3
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //rejoin pack4
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //rejoin pack5
				packContainerForm.RemoveContainer(container);

				AssertEquals("Should have 0 containers", 0, SailingHelper.Containers.Count);
				AssertEquals("Should have 5 unallocated packlines", 1, Sailing.UnAllocatedPackLines.Count);
			}
		}

		public void TestPackAllException()
		{
			AssertEquals("Should have 0 containers", 0, SailingHelper.Containers.Count);
			AssertEquals("Should have 5 unallocated packlines", 5, Sailing.UnAllocatedPackLines.Count);

			using (PackContainerFormForTest packContainerForm = new PackContainerFormForTest(SailingHelper))
			{
				bool packAllFound = false;
				packContainerForm.Show();
				foreach (MenuItem item in packContainerForm.ExposedActionsMenuItem.MenuItems)
				{
					if (item.Text == "Pack All")
					{
						item.PerformClick();
						packAllFound = true;
						break;
					}
				}

				Assert(packAllFound);
			}
		}

		JobSailing Sailing;
		PackContainerHelper SailingHelper;
		protected override void SetUp()
		{
			base.SetUp();
			Sailing = GetSailing;
			SailingHelper = new PackContainerHelper(Sailing);
		}

		class PackContainerFormForTest : PackContainerForm
		{
			public PackContainerFormForTest(PackContainerHelper sailingHelper)
				: base(sailingHelper)
			{
			}

			public MenuItem ExposedActionsMenuItem
			{
				get { return ActionsMenuItem; }
			}
		}
	}
	#endregion
}
