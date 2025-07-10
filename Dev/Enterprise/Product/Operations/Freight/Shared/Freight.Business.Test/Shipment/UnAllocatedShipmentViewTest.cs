using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class UnAllocatedShipmentViewTest : TestCaseWithFactory
	{
		public void TestIsThisElementPartOfTheCollection()
		{
			UnAllocatedShipmentView view = new UnAllocatedShipmentView(Shipments, Sailing);

			AssertEquals("Expecting one item in the view.", 1, view.Count);
			Assert("Shipment1 should show in the view.", view.Contains(Shipment1.PK));
			Assert("Shipment2 should not show in the view.", !view.Contains(Shipment2.PK));
			Assert("Shipment3 should not show in the view.", !view.Contains(Shipment3.PK));

			view.ShowOnlyReceived = false;
			AssertEquals("Expecting 2 items in the view.", 2, view.Count);
			Assert("Shipment1 should show in the view.", view.Contains(Shipment1.PK));
			Assert("Shipment2 should show in the view.", view.Contains(Shipment2.PK));
			Assert("Shipment3 should not show in the view.", !view.Contains(Shipment3.PK));

			view.ShowOnlyReceived = true;
			view.ShowOnlyThisSailing = false;
			AssertEquals("Expecting 2 items in the view.", 2, view.Count);
			Assert("Shipment1 should show in the view.", view.Contains(Shipment1.PK));
			Assert("Shipment2 should not show in the view.", !view.Contains(Shipment2.PK));
			Assert("Shipment3 should show in the view.", view.Contains(Shipment3.PK));
		}

		public void TestReceivedShipments()
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_JX = Sailing.PK;
			shipment1.JS_InterimReceipt = "2322";
			shipment1.JS_IsBooking = true;
			shipment1.JS_IsForwardRegistered = false;
			shipment1.JS_IsCancelled = false;
			shipment1.JS_A_RCV = ZDateTime.Today;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_JX = ZGuid.Empty;
			shipment2.JS_IsBooking = true;
			shipment2.JS_IsForwardRegistered = false;
			shipment2.JS_IsCancelled = false;
			shipment2.JS_A_RCV = ZDateTime.Empty;
			shipment2.JS_InterimReceipt = ZString.Empty;

			var shipmentsCollection = new ShipmentCollection(Factory);
			shipmentsCollection.Add(shipment1);
			shipmentsCollection.Add(shipment2);

			var view = new UnAllocatedShipmentView(shipmentsCollection, Sailing);
			view.ShowOnlyReceived = true;

			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, view);
		}

		public void TestShowOnlyThisSailing()
		{
			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_JX = Sailing.PK;

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_JX = ZGuid.Empty;

			var shipmentsCollection = new ShipmentCollection(Factory);
			shipmentsCollection.Add(shipment1);
			shipmentsCollection.Add(shipment2);

			AssertNotNull("prerequisite", shipment1.Sailing);
			AssertNull("prerequisite", shipment2.Sailing);

			var view = new UnAllocatedShipmentView(shipmentsCollection, Sailing);
			view.ShowOnlyReceived = false;
			view.ShowOnlyThisSailing = true;

			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, view);
		}

		#region Implementation

		JobSailing Sailing;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetUp")]
		JobSailing Sailing2;
		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonShipment Shipment3;
		ShipmentCollection Shipments;

		protected override void SetUp()
		{
			base.SetUp();
			Sailing = new SailingsForTestClasses(Factory).SydLaxSailing;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = Sailing.Vessel.RV_FK;
			voyage.JV_VoyageFlight = "2134123";

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(3);
			voyage.Origins.Add(origin);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(39);
			voyage.Destinations.Add(destination);

			Sailing2 = voyage.Sailings[0];

			Shipment1 = Factory.New<CommonShipment>();
			Shipment1.JS_JX = Sailing.PK;
			Shipment1.JS_InterimReceipt = "2352";
			Shipment1.JS_IsBooking = true;
			Shipment1.JS_IsForwardRegistered = false;

			Shipment2 = Factory.New<CommonShipment>();
			Shipment2.JS_JX = Sailing.PK;
			Shipment2.JS_IsBooking = true;
			Shipment2.JS_IsForwardRegistered = false;
			Shipment2.JS_InterimReceipt = ZString.Empty;
			Shipment2.JS_A_RCV = ZDateTime.Empty;

			Shipment3 = Factory.New<CommonShipment>();
			Shipment3.JS_JX = ZGuid.Empty;
			Shipment3.JS_InterimReceipt = "2155";
			Shipment3.JS_IsBooking = true;
			Shipment3.JS_IsForwardRegistered = false;

			Shipments = new ShipmentCollection(Factory);
			Shipments.Add(Shipment1);
			Shipments.Add(Shipment2);
			Shipments.Add(Shipment3);
		}

		#endregion
	}
}
