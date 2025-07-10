using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentLastMileCarrierAndDepotDetailsCalculatorTest : TestCaseWithFactory
	{
		public void TestUpdateConsignmentsDestinationDetailsByClusterKeys()
		{
			var bookingHeader = CreateBookingHeaderConsignments();
			Factory.Save();

			foreach (HVLVConsignment consignment in bookingHeader.Consignments)
			{
				AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
				AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
				AssertEquals(ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
				AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
			}

			var lastMileCarrierAndDepotDetailsCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator();
			lastMileCarrierAndDepotDetailsCalculator.UpdateConsignmentsDestinationDetailsByClusterKeys(new int[] { bookingHeader.HVH_ClusterKey });
			bookingHeader.Consignments.Reload(true);

			foreach (HVLVConsignment consignment in bookingHeader.Consignments)
			{
				AssertEquals(depotAddress.PK, consignment.HVC_OA_DestinationDepot);
				AssertEquals(carrier.PK, consignment.HVC_OH_LastMileCarrier);
				AssertEquals("EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
				AssertEquals(agent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
			}

			PortHubSelectionTestDataCreator.ClearUpAllZoneItems(Factory);
			lastMileCarrierAndDepotDetailsCalculator.UpdateConsignmentsDestinationDetailsByClusterKeys(new int[] { bookingHeader.HVH_ClusterKey });
			bookingHeader.Consignments.Reload(true);

			foreach (HVLVConsignment consignment in bookingHeader.Consignments)
			{
				AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
				AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
				AssertEquals(ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
				AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
				AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot_ZAddress.OrgPK);
			}
		}

		public void TestPopulateLMCDepotDetails_WhenOnlyPopulateEmptyRegistryIsFalse_ThenOverwritePopulatedField()
		{
			var bookingHeader = CreateBookingHeaderConsignments();

			var initialBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var initialCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var initialDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();

			foreach (HVLVConsignment consignment in bookingHeader.Consignments)
			{
				consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
				consignment.HVC_OH_LastMileCarrierBookingAgent = initialBookingAgent.PK;
				consignment.HVC_OH_LastMileCarrier = initialCarrier.PK;
				consignment.HVC_OA_DestinationDepot = initialDestinationDepot.PK;
				consignment.HVC_CarrierAccountNumber = "0123456789";
				consignment.HVC_PL_NKLastMileCarrierServiceLevel = "D2D";
			}

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consignmentLMCCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator();
				consignmentLMCCalculator.UpdateConsignmentsDestinationDetails(bookingHeader.Consignments.Select(c => c.PK).ToList());

				Factory.Save();

				bookingHeader.Consignments.Reload(true);
				CombineAssertions("The registry should have these fields overwritten.", () =>
				{
					bookingHeader.Consignments.Cast<HVLVConsignment>().ForEach(consignment =>
					{
						var id = consignment.HVC_ConsignmentId;
						AssertEquals("Destination Depot | consignmentID: " + id, depotAddress.PK, consignment.HVC_OA_DestinationDepot);
						AssertEquals("Last Mile Carrier | consignmentID: " + id, carrier.PK, consignment.HVC_OH_LastMileCarrier);
						AssertEquals("Booking Agent | consignmentID: " + id, agent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
						AssertEquals("Carrier Account Number | consignmentID: " + id, "777888999", consignment.HVC_CarrierAccountNumber);
						AssertEquals("Service Level | consignmentID: " + id, "EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					});
				});
			}
		}

		public void TestPopulateLMCDepotDetails_WhenOnlyPopulateEmptyRegistryIsTrue_ThenDoNotOverwritePopulatedField()
		{
			var bookingHeader = CreateBookingHeaderConsignments();

			var initialBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var initialCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var initialDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();

			foreach (HVLVConsignment consignment in bookingHeader.Consignments)
			{
				consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
				consignment.HVC_OH_LastMileCarrierBookingAgent = initialBookingAgent.PK;
				consignment.HVC_OH_LastMileCarrier = initialCarrier.PK;
				consignment.HVC_OA_DestinationDepot = initialDestinationDepot.PK;
				consignment.HVC_CarrierAccountNumber = "0123456789";
				consignment.HVC_PL_NKLastMileCarrierServiceLevel = "D2D";
			}

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consignmentLMCCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator();
				consignmentLMCCalculator.UpdateConsignmentsDestinationDetails(bookingHeader.Consignments.Select(c => c.PK).ToList());

				Factory.Save();

				bookingHeader.Consignments.Reload(true);
				CombineAssertions("The registry should prevent populated fields from being overwritten.", () =>
				{
					bookingHeader.Consignments.Cast<HVLVConsignment>().ForEach(consignment =>
					{
						var id = consignment.HVC_ConsignmentId;
						AssertEquals("Destination Depot | consignmentID: " + id, initialDestinationDepot.PK, consignment.HVC_OA_DestinationDepot);
						AssertEquals("Last Mile Carrier | consignmentID: " + id, initialCarrier.PK, consignment.HVC_OH_LastMileCarrier);
						AssertEquals("Booking Agent | consignmentID: " + id, initialBookingAgent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
						AssertEquals("Carrier Account Number | consignmentID: " + id, "0123456789", consignment.HVC_CarrierAccountNumber);
						AssertEquals("Service Level | consignmentID: " + id, "D2D", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					});
				});
			}
		}

		public void TestPopulateLMCDepotDetails_WhenOnlyPopulateEmptyRegistryIsTrue_ThenEmptyLMCDepotFieldsArePopulated()
		{
			var bookingHeader = CreateBookingHeaderConsignments();
			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consignmentLMCCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator();
				consignmentLMCCalculator.UpdateConsignmentsDestinationDetails(bookingHeader.Consignments.GetPKs());

				Factory.Save();

				bookingHeader.Consignments.Reload(true);
				CombineAssertions("Empty field should be populated.", () =>
				{
					bookingHeader.Consignments.Cast<HVLVConsignment>().ForEach(consignment =>
					{
						var id = consignment.HVC_ConsignmentId;
						AssertEquals("Destination Depot | consignmentID: " + id, depotAddress.PK, consignment.HVC_OA_DestinationDepot);
						AssertEquals("Last Mile Carrier | consignmentID: " + id, carrier.PK, consignment.HVC_OH_LastMileCarrier);
						AssertEquals("Booking Agent | consignmentID: " + id, agent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
						AssertEquals("Carrier Account Number | consignmentID: " + id, "777888999", consignment.HVC_CarrierAccountNumber);
						AssertEquals("Service Level | consignmentID: " + id, "EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					});
				});
			}
		}

		public void TestGivenNoClusterKeys_WhenUpdatingConsignmentLMCByClusterKeys_ThenDoNotThrowExceptions()
		{
			var lastMileCarrierAndDepotDetailsCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator();
			AssertNoExceptionThrown("Expected no exceptions thrown when running this method with an empty array as parameter", () => { lastMileCarrierAndDepotDetailsCalculator.UpdateConsignmentsDestinationDetailsByClusterKeys(Array.Empty<int>()); });
		}

		#region Implementation

		HVLVBookingHeader CreateBookingHeaderConsignments()
		{
			var creator = new PortHubSelectionTestDataCreator(Factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = creator.GenerateAddress("AD2", "XY2", "XY 2");
			depotAddress = creator.GenerateAddress("AD3", "XY3", "XY 3");
			carrier = creator.GenerateOrganisation("XY4", "XY 4");
			agent = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress.PK, dispatchDepotAddress.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = Factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent.PK;

			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN100";
			consignment1.HVC_UndgClass = "6";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 11";
			consignment1.HVC_ConsigneeCity = "Melbourne Metro";
			consignment1.HVC_ConsigneeState = "VIC";
			consignment1.HVC_ConsigneePostcode = "3560";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN101";
			consignment2.HVC_UndgClass = "6";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 12";
			consignment2.HVC_ConsigneeCity = "Melbourne Metro";
			consignment2.HVC_ConsigneeState = "VIC";
			consignment2.HVC_ConsigneePostcode = "3561";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";

			return bookingHeader;
		}

		OrgAddress depotAddress;
		OrgHeader carrier;
		OrgHeader agent;

		#endregion
	}
}
