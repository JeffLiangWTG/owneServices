using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Test
{
	public class MasterBookingJobCostSupporterTest : DtbBookingTestCaseWithFactory
	{
		public void TestSubParentShipments_ReturnsCorrectParentsOfSubBookings()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var parent1 = Factory.New<IForwardingShipment>();
			var parent2 = Factory.New<IBaseJobDeclaration>();
			var parent3 = Factory.New<IForwardingConsol>();
			var parent4 = Factory.New<IForwardingShipment>();
			var parent5 = Factory.New<IForwardingShipment>();
			IJobInvoicingPlugIn[] parents = { (IJobInvoicingPlugIn)parent1, (IJobInvoicingPlugIn)parent2, (IJobInvoicingPlugIn)parent3, (IJobInvoicingPlugIn)parent4, (IJobInvoicingPlugIn)parent5 };

			SetupSubBookingsForBookingParentsAndAttachThemToMaster(parents, masterBooking);

			var expectedParentPKs = new List<ZGuid> { parent1.PK, parent2.PK, parent3.PK, parent4.PK, parent5.PK };

			var supporter = new MasterBookingJobCostSupporter(masterBooking);
			AssertEquals("Supporter should have correct number of shipments", ((IGenericJobCostSupporter)supporter).ShipmentsListPKs.Length, 5);
			AssertContainsExactElementsInAnyOrder("Returns correct parents of sub bookings", expectedParentPKs, ((IGenericJobCostSupporter)supporter).ShipmentsListPKs);
		}
		public void SetupSubBookingsForBookingParentsAndAttachThemToMaster(IJobInvoicingPlugIn[] parents, DtbBooking master)
		{
			for (int i = 0; i < parents.Length; i++)
			{
				var subConsolidation = Helper.CreateConsolidation((IDtbBookingParent)parents[i]);
				var sub = Helper.CreateBooking(subConsolidation);
				master.SubBookings.Add(sub);
			}
		}

		public void TestIGenericJobCostSupporter()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			var costSupporter = (IGenericJobCostSupporter)new MasterBookingJobCostSupporter(masterBooking);

			AssertEquals("ConsolMode", "", costSupporter.ConsolMode);
			AssertEquals("DocumentSupporter", ((IDocumentSupportable)masterBooking).DocumentSupporter, costSupporter.DocumentSupporter);
			AssertEquals("HasChanges", masterBooking.HasChanges, costSupporter.HasChanges);
			AssertEquals("IsBuyersConsol", false, costSupporter.IsBuyersConsol);
			AssertEquals("Direction", Directions.Unknown, costSupporter.Direction);
			AssertEquals("IsInDatabase", masterBooking.IsInDatabase, costSupporter.IsInDatabase);
			AssertEquals("MasterBillNum", "", costSupporter.MasterBillNum);
			AssertEquals("PK", masterBooking.PK, costSupporter.PK);
			AssertEquals("PortOfDischarge", "", costSupporter.PortOfDischarge);
			AssertEquals("PortOfLoading", "", costSupporter.PortOfLoading);
			AssertEquals("TotalChargeableUnit", "KG", costSupporter.TotalChargeableUnit);
			AssertEquals("TransportMode", "", costSupporter.TransportMode);
			AssertEquals("Type", masterBooking.TablePrefix, costSupporter.Type);

			AssertNull("ReceivingForwarder", costSupporter.ReceivingForwarder);
			AssertNull("SendingForwarder", costSupporter.SendingForwarder);
			AssertNull("Shipments", costSupporter.Shipments);

			AssertEquals("ExcludedApportionmentMethods", 1, costSupporter.ExcludedApportionmentMethods.Count());
			AssertEquals("IsApportionmentFilterEnabled", true, costSupporter.IsApportionmentFilterEnabled);
			AssertEquals("JobConsolCostingCheckPoint", Env.Security.None, costSupporter.JobConsolCostingCheckPoint);
		}
	}
}
