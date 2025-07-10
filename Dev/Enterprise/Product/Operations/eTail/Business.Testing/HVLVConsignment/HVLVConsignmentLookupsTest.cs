using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHVC_ReleaseStatus_List()
		{
			var list = HVLVReleaseStatus.GetAll();
			AssertContainsExactElementsInAnyOrder(new[] { "NON", "HLD", "CLR" }, list.GetAllCodes());
		}

		public void TestHVC_JS_ManifestedOnShipment_List()
		{
			var shipment1 = HVLVTestHelper.GetNewShipment(Factory, "S0100000", "STD");
			shipment1.JS_IsBooking = true;

			var shipment2 = HVLVTestHelper.GetNewShipment(Factory, "S0100001", "HVL");
			shipment2.JS_IsBooking = true;

			var shipment3 = HVLVTestHelper.GetNewShipment(Factory, "S0100002", "HVL");
			shipment3.JS_IsBooking = false;

			var shipment4 = HVLVTestHelper.GetNewShipment(Factory, "S0100003", "STD");
			shipment4.JS_IsBooking = true;

			var shipment5 = HVLVTestHelper.GetNewShipment(Factory, "S0100004", "HVL");
			shipment5.JS_IsBooking = false;

			var consignment = Factory.New<HVLVConsignment>();
			var lookups = consignment.Lookups.HVC_JS_ManifestedOnShipment_List;
			lookups.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "S0100002", "S0100004" }, lookups.Cast<ForwardingShipment>().Select(x => x.JS_UniqueConsignRef));
		}

		public void TestLastMileCarrierServiceLevel_ContainsCarrierOrgServiceLevels()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "ABC";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Expensive and Fast";

			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "XYZ";
			serviceLevel2.PL_CarrierServiceLevelDescription = "Slow and Affordable";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrier = carrier.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "STD", "ABC", "XYZ" }, consignment.Lookups.LastMileCarrierServiceLevels.Select(level => level.PL_Code));
		}

		public void TestLastMileCarrierServiceLevel_WhenCarrierOrgHasNoCustomisedServiceLevel_ContainsOnlyStandardServiceLevel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrier = carrier.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "STD" }, consignment.Lookups.LastMileCarrierServiceLevels.Select(level => level.PL_Code));
		}

		public void TestLastMileCarrierServiceLevel_WhenOH_IsShippingProviderIsFalse_IsEmpty()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = false;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrier = carrier.PK;

			Factory.Save();

			AssertEquals(0, consignment.Lookups.LastMileCarrierServiceLevels.Count);
		}

		public void TestLastMileCarrierServiceLevel_WhenNoLastMileCarrierOrg_IsEmpty()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			Factory.Save();

			AssertEquals(0, consignment.Lookups.LastMileCarrierServiceLevels.Count);
		}

		public void TestLastMileCarrierAccountNumber()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = false;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			AssertNull(consignment.Lookups.HVC_CarrierAccountNumber_List);

			consignment.HVC_OH_LastMileCarrier = carrier.PK;
			AssertNull(consignment.Lookups.HVC_CarrierAccountNumber_List);

			carrier.OH_IsShippingProvider = true;
			var acc1 = carrier.CarrierAccounts.AddNew();
			acc1.OAN_AccountNumber = "123456";
			var acc2 = carrier.CarrierAccounts.AddNew();
			acc2.OAN_AccountNumber = "654321";
			AssertEquals(2, consignment.Lookups.HVC_CarrierAccountNumber_List.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "123456", "654321" }, consignment.Lookups.HVC_CarrierAccountNumber_List.Select(level => level.OAN_AccountNumber));

			carrier.OH_IsShippingProvider = false;
			AssertNull(consignment.Lookups.HVC_CarrierAccountNumber_List);
		}

		public void TestOrganisationCollectionTypes()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			CombineAssertions("Organisation Lookup lists should have correct types for filter defaults", () =>
			{
				AssertType("Consignee Lookup", typeof(ConsigneeCollection), consignment.Lookups.ConsigneeOrganisation_List);
				AssertType("Shipper Lookup", typeof(ConsignorCollection), consignment.Lookups.ShipperOrganisation_List);
				AssertType("Destination Depot Lookup", typeof(UnpackDepotCollection), consignment.Lookups.DestinationDepotOrgCollection);

				AssertType("Carrier Lookup", typeof(CarrierCollection), consignment.Lookups.LastMileCarriers);
				AssertType("LMC Booking Agent Lookup", typeof(ControllingAgentCollection), consignment.Lookups.LastMileCarrierBookingAgents);
			});
		}

		public void TestHVC_ACASInterchangeStatus_List()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var list = consignment.Lookups.HVC_ACASInterchangeStatus_List;
			AssertContainsExactElementsInAnyOrder(new[] { "OIS", "OIJ", "KIS", "KIJ", "AIS", "AIJ" }, list.GetAllCodes());
		}

		public void TestHVC_DeniedPartyScreeningStatus_List()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var list = consignment.Lookups.HVC_DeniedPartyScreeningStatus_List;
			AssertContainsExactElementsInAnyOrder(new[] { "BLK", "CLR", "JCL", "MAT", "NDS", "NOT", "CLP", "REL", "REQ", "UNK", "CAN", "JCE", "JBE" }, list.GetAllCodes());
		}
	}
}
