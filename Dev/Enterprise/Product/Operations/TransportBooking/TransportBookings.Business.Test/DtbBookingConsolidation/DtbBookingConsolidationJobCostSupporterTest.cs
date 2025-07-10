using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Test
{
	public class DtbBookingConsolidationJobCostSupporterTest : DtbBookingTestCaseWithFactory
	{
		public void TestShipments()
		{
			// Create 3 TB parents.
			var dummy1 = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			var dummy3 = Factory.New<DummyWithDtbBooking>();

			// create 1 single job consolidation for each parent
			var dummy1Consolidation = Helper.CreateConsolidation(dummy1);
			var dummy2Consolidation = Helper.CreateConsolidation(dummy2);
			var dummy3Consolidation = Helper.CreateConsolidation(dummy3);

			// create 2 TBs for each single job consolidation
			var dummy1Booking1 = Helper.CreateBooking(dummy1Consolidation);
			var dummy1Booking2 = Helper.CreateBooking(dummy1Consolidation);
			var dummy2Booking1 = Helper.CreateBooking(dummy2Consolidation);
			var dummy2Booking2 = Helper.CreateBooking(dummy2Consolidation);
			var dummy3Booking1 = Helper.CreateBooking(dummy3Consolidation);
			var dummy3Booking2 = Helper.CreateBooking(dummy3Consolidation);

			// create multiJobConsolidation with 2 bookings from dummy1, 1 booking from dummy2 and no bookings from dummy3
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(dummy1Booking1);
			multiJobConsolidation.Bookings.Add(dummy1Booking2);
			multiJobConsolidation.Bookings.Add(dummy2Booking1);

			// IJobCostSupporter.ShipmentsListPKs
			var costSupporter = ((IJobCostingPlugIn)multiJobConsolidation).CostSupporter;
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { dummy1.PK, dummy2.PK }, costSupporter.ShipmentsListPKs);

			// IJobCostSupporter.ShipmentsList
			AssertEquals("Only 2 shipments expected to be found (dummy1 and dummy2).", 2, costSupporter.ShipmentsList.Length);
			AssertContainsSupporterWithBookings(costSupporter.ShipmentsList, dummy1, dummy1Booking1, dummy1Booking2);
			AssertContainsSupporterWithBookings(costSupporter.ShipmentsList, dummy2, dummy2Booking1);
		}

		public void TestDtbBookingConsolidationJobCostSupporter_ForAttachedTBWithConsolParent()
		{
			var (forwardingConsolParent, shipment1, shipment2) = CreateForwardingConsolWithTwoShipments();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolParent);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingConsolidationJobCostSupporter(bookingConsolidation);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingConsolidationJobCostSupporter for a booking consolidation with an attached TB parent being a Forwarding Consol should have correct property values for properties that have unique values for a Forwarding Consol", () =>
			{
				AssertEquals("Supporter PK should match PK of booking consolidation", bookingConsolidation.PK, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KB' (DtbBookingConsolidation)", DtbBookingConsolidationSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertContainsExactElementsInAnyOrder("Supporter ShipmentsListPKs should match consol shipments", new[] { shipment1.PK, shipment2.PK }, supporterAsIGenericCostSupporter.ShipmentsListPKs);
			});
		}

		public void TestIGenericJobCostSupporter()
		{
			var consolidation = Helper.CreateConsolidationMultiJob();
			IGenericJobCostSupporter costSupporter = new DtbBookingConsolidationJobCostSupporter(consolidation);
			PackingRegistry.Instance.SetWeightUnitForTest(Core.Constants.Weight.Kilograms);

			AssertEquals("ConsolMode", "", costSupporter.ConsolMode);
			AssertEquals("DocumentSupporter", ((IDocumentSupportable)consolidation).DocumentSupporter, costSupporter.DocumentSupporter);
			AssertEquals("HasChanges", consolidation.HasChanges, costSupporter.HasChanges);
			AssertEquals("IsBuyersConsol", false, costSupporter.IsBuyersConsol);
			AssertEquals("Direction", Enterprise.MasterFiles.Business.Directions.Unknown, costSupporter.Direction);
			AssertEquals("IsInDatabase", consolidation.IsInDatabase, costSupporter.IsInDatabase);
			AssertEquals("MasterBillNum", "", costSupporter.MasterBillNum);
			AssertEquals("PK", consolidation.PK, costSupporter.PK);
			AssertEquals("PortOfDischarge", "", costSupporter.PortOfDischarge);
			AssertEquals("PortOfLoading", "", costSupporter.PortOfLoading);
			AssertEquals("TotalChargeableUnit", "KG", costSupporter.TotalChargeableUnit);
			AssertEquals("TransportMode", "", costSupporter.TransportMode);
			AssertEquals("Type", consolidation.TablePrefix, costSupporter.Type);

			AssertNull("ReceivingForwarder", costSupporter.ReceivingForwarder);
			AssertNull("SendingForwarder", costSupporter.SendingForwarder);
			AssertNull("Shipments", costSupporter.Shipments);

			AssertEquals("ExcludedApportionmentMethods", 1, costSupporter.ExcludedApportionmentMethods.Count());
			AssertEquals("IsApportionmentFilterEnabled", true, costSupporter.IsApportionmentFilterEnabled);
			AssertEquals("JobConsolCostingCheckPoint", Env.Security.None, costSupporter.JobConsolCostingCheckPoint);
		}

		(BusinessObject consol, BusinessObject shipment1, BusinessObject shipment2) CreateForwardingConsolWithTwoShipments()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "Consol1";
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";
			consol[JobConsolSchema.JK_IsCancelled] = false;
			consol[JobConsolSchema.JK_IsForwarding] = true;

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment1[JobShipmentSchema.JS_UniqueConsignRef] = "Shipment1";
			shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment1[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol);

			var shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment2[JobShipmentSchema.JS_UniqueConsignRef] = "Shipment2";
			shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment2[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol);

			return (consol, shipment1, shipment2);
		}

		void AssertContainsSupporterWithBookings(IJobInvoicingPlugIn[] parentShipments, DummyWithDtbBooking dummy, params DtbBooking[] bookings)
		{
			foreach (var shipment in parentShipments)
			{
				if (shipment.PK == dummy.PK)
				{
					AssertContainsExactElementsInAnyOrder(bookings, ((DtbBookingConsolidationJobInvoicingPlugIn)shipment).Bookings);
					return;
				}
			}
			Fail(string.Format("Parent shipment {0} is not in a list of parentShipments.", dummy.JobNumber));
		}
	}
}
