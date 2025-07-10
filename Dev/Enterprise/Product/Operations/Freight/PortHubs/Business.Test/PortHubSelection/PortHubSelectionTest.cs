using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Business.Testing
{
	[TestedType(typeof(PortHubSelection))]
	public class PortHubSelectionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTY_OA_DispatchDepotAddress_Readonlyness_ShipmentPickup()
		{
			var selection = Factory.New<PortHubSelection>();
			var dispatchDepot = Factory.New<OrgHeader>();

			selection.TY_OA_DispatchDepotAddress = dispatchDepot.MainAddress.PK;
			selection.TY_ProcessType = PortHubSelectionProcessTypeList.Codes.Shipment;
			selection.TY_Direction = PortHubSelectionDirectionList.Codes.Pickup;
			AssertEquals("precondition: changing processType to Shipment and direction to pickup empties out DispatchDepotPK", ZGuid.Empty, selection.DispatchDepotPK);
			AssertEquals("Depot Address is readOnly", true, selection.TY_OA_DispatchDepotAddressInfo.ReadOnly);
		}

		public void TestTY_OA_DispatchDepotAddress_Readonlyness_ShipmentDeliver()
		{
			var selection = Factory.New<PortHubSelection>();
			var dispatchDepot = Factory.New<OrgHeader>();

			selection.TY_OA_DispatchDepotAddress = dispatchDepot.MainAddress.PK;
			selection.TY_ProcessType = PortHubSelectionProcessTypeList.Codes.Shipment;
			selection.TY_Direction = PortHubSelectionDirectionList.Codes.Delivery;
			AssertEquals("precondition: changing processType to Shipment and direction to delivery does not empty out DispatchDepotPK", dispatchDepot.PK, selection.DispatchDepotPK);
			AssertEquals("Depot Address is not readOnly", false, selection.TY_OA_DispatchDepotAddressInfo.ReadOnly);
		}

		public void TestTY_OA_DispatchDepotAddress_Readonlyness_NonShipment()
		{
			var selection = Factory.New<PortHubSelection>();
			var dispatchDepot = Factory.New<OrgHeader>();

			selection.TY_OA_DispatchDepotAddress = dispatchDepot.MainAddress.PK;
			selection.TY_ProcessType = PortHubSelectionProcessTypeList.Codes.HVLVBookingHeader;
			AssertEquals("precondition: changing processType to HVH does not empty out DispatchDepotPK", dispatchDepot.PK, selection.DispatchDepotPK);
			AssertEquals("Depot Address is not readOnly", false, selection.TY_OA_DispatchDepotAddressInfo.ReadOnly);

			selection.TY_ProcessType = PortHubSelectionProcessTypeList.Codes.HVLVOriginLoadList;
			AssertEquals("precondition: changing processType to HVL does not empty out DispatchDepotPK", dispatchDepot.PK, selection.DispatchDepotPK);
			AssertEquals("Depot Address is not readOnly", false, selection.TY_OA_DispatchDepotAddressInfo.ReadOnly);
		}

		public void TestDispatchDepotPK()
		{
			var dispatchDepot = Factory.NewWithValidTestData<OrgHeader>();
			dispatchDepot.OH_Code = "DEPOT1";
			var dispatchDepotAddress = dispatchDepot.Addresses.AddNew();
			dispatchDepotAddress.OA_Address1 = "14 Blah St";

			var selection = Factory.New<PortHubSelection>();
			AssertEquals(ZGuid.Empty, selection.DispatchDepotPK);
			AssertNull(selection.Depot);

			selection.TY_OA_DispatchDepotAddress = dispatchDepotAddress.PK;
			AssertEquals(dispatchDepot.PK, selection.DispatchDepotPK);

			selection.DispatchDepotPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, selection.TY_OA_DispatchDepotAddress);

			selection.DispatchDepotPK = dispatchDepot.PK;
			AssertEquals(dispatchDepot.MainAddress.PK, selection.TY_OA_DispatchDepotAddress);
		}

		public void TestDispatchDepotPort()
		{
			var dispatchDepot = Factory.NewWithValidTestData<OrgHeader>();
			dispatchDepot.OH_RL_NKClosestPort = "USLAX";
			var dispatchDepotAddress = dispatchDepot.Addresses.AddNew();

			var selection = Factory.New<PortHubSelection>();
			selection.TY_OA_DispatchDepotAddress = dispatchDepotAddress.PK;
			AssertEquals("USLAX", selection.DispatchDepotPortCode);

			dispatchDepotAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("AUSYD", selection.DispatchDepotPortCode);

			selection.DispatchDepotPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, selection.DispatchDepotPortCode);
		}

		public void TestZonesValidation()
		{
			var selection = Factory.New<PortHubSelection>();
			selection.RunPreSaveValidation();
			AssertHasRowError(selection, "This selection has no zones attached.");

			selection.PortHubZonePivots.AddNew();
			selection.RunPreSaveValidation();
			AssertNoRowError(selection, "This selection has no zones attached.");
		}

		public void TestDefaultValues()
		{
			var selection = Factory.New<PortHubSelection>();
			AssertEquals(Constants.TransportModes.All, selection.TY_RatingFreightMode);
		}

		public void TestCBADefault()
		{
			var portHubSelection = Factory.New<PortHubSelection>();
			AssertEquals(ZGuid.Empty, portHubSelection.TY_OH_CarrierBookingAgent);
		}

		public void TestPackModeDefault()
		{
			var portHubSelection = Factory.New<PortHubSelection>();
			AssertEquals(string.Empty, portHubSelection.TY_PackMode);
		}

		public void TestDepotPK()
		{
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "DEPOT1";
			var depotAddress = depot.Addresses.AddNew();
			depotAddress.OA_Address1 = "Some address";

			var selection = Factory.New<PortHubSelection>();
			AssertEquals(ZGuid.Empty, selection.DepotPK);
			AssertNull("Depot", selection.Depot);

			selection.TY_OA_DepotAddress = depotAddress.PK;
			AssertEquals(depot.PK, selection.DepotPK);
			AssertEquals(depot, selection.Depot);

			selection.DepotPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, selection.TY_OA_DepotAddress);

			selection.DepotPK = depot.PK;
			AssertEquals(depot.MainAddress.PK, selection.TY_OA_DepotAddress);
		}

		public void TestDepotPort()
		{
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_RL_NKClosestPort = "DEBER";
			var depotAddress = depot.Addresses.AddNew();

			var selection = Factory.New<PortHubSelection>();
			selection.TY_OA_DepotAddress = depotAddress.PK;
			AssertEquals("DEBER", selection.DepotPortCode);

			depotAddress.OA_RL_NKRelatedPortCode = "NLAMS";
			AssertEquals("NLAMS", selection.DepotPortCode);

			selection.DepotPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, selection.DepotPortCode);
		}

		public void TestGivenOriginDepot_WhenOverrideOriginDepotDefaultPort_ThenDisplayOverrideUNLOCO()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTGSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.DispatchDepotPK = orgHeader.PK;
			portHubSelection.TY_OA_DispatchDepotAddress = orgHeader.MainAddress.PK;

			AssertEquals("Expected AUSYD because it is the default UNLOCO for orgHeader for this portHubSelection", "AUSYD", portHubSelection.DispatchDepotPortCode);

			portHubSelection.TY_RL_NKOriginPort = "AUBNE";
			AssertEquals("Expected AUBNE to override the default UNLOCO for orgHeaer WTGSYD", "AUBNE", portHubSelection.DispatchDepotPortCode);
		}

		public void TestGivenOverrideOriginDepot_WhenRemovingOverrideUNLOCO_ThenDisplayDefaultUNLOCO()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTGSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.DispatchDepotPK = orgHeader.PK;
			portHubSelection.TY_OA_DispatchDepotAddress = orgHeader.MainAddress.PK;

			portHubSelection.TY_RL_NKOriginPort = "AUBNE";
			AssertEquals("Expected AUBNE to override the default UNLOCO for orgHeaer WTGSYD", "AUBNE", portHubSelection.DispatchDepotPortCode);

			portHubSelection.TY_RL_NKOriginPort = null;
			AssertEquals("Expected AUSYD because it is the default UNLOCO for orgHeader for this portHubSelection", "AUSYD", portHubSelection.DispatchDepotPortCode);
		}

		public void TestGivenDestinationDepot_WhenOverrideDestinationDepotDefaultPort_ThenDisplayOverrideUNLOCO()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTGSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.DepotPK = orgHeader.PK;
			portHubSelection.TY_OA_DepotAddress = orgHeader.MainAddress.PK;

			AssertEquals("Expected AUSYD because it is the default UNLOCO for orgHeader for this portHubSelection", "AUSYD", portHubSelection.DepotPortCode);

			portHubSelection.TY_RL_NKDestinationPort = "AUBNE";
			AssertEquals("Expected AUBNE to override the default UNLOCO for orgHeaer WTGSYD", "AUBNE", portHubSelection.DepotPortCode);
		}

		public void TestGivenOverrideDestinationDepot_WhenRemovingOverrideUNLOCO_ThenDisplayDefaultUNLOCO()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTGSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.DepotPK = orgHeader.PK;
			portHubSelection.TY_OA_DepotAddress = orgHeader.MainAddress.PK;

			portHubSelection.TY_RL_NKDestinationPort = "AUBNE";
			AssertEquals("Expected AUBNE to override the default UNLOCO for orgHeaer WTGSYD", "AUBNE", portHubSelection.DepotPortCode);

			portHubSelection.TY_RL_NKDestinationPort = null;
			AssertEquals("Expected AUSYD because it is the default UNLOCO for orgHeader for this portHubSelection", "AUSYD", portHubSelection.DepotPortCode);
		}

		public void TestGivenOriginDepot_WhenOverrideOriginDepotNotValid_ThenSelectionHasError()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTGSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.DispatchDepotPK = orgHeader.PK;
			portHubSelection.TY_OA_DispatchDepotAddress = orgHeader.MainAddress.PK;

			AssertEquals("Expected AUSYD because it is the default UNLOCO for orgHeader for this portHubSelection", "AUSYD", portHubSelection.DispatchDepotPortCode);

			portHubSelection.TY_RL_NKOriginPort = "XXXXX";
			portHubSelection.RunPreSaveValidation();
			AssertHasError("Origin Port Info should have an error because the UNLOCO does not exist.", portHubSelection.TY_RL_NKOriginPortInfo, "Enter a valid selection.");

			portHubSelection.TY_RL_NKOriginPort = "XXXX";
			portHubSelection.RunPreSaveValidation();
			AssertHasError("Origin Port Info should have an error because the UNLOCO is too short.", portHubSelection.TY_RL_NKOriginPortInfo, "Enter a valid selection.");

			portHubSelection.TY_RL_NKOriginPort = "AUSYD";
			portHubSelection.RunPreSaveValidation();
			AssertNoError("Origin Port Info should not have an error because the UNLOCO is valid.", portHubSelection.TY_RL_NKOriginPortInfo, "Enter a valid selection.");
		}

		public void TestGivenDestinationDepot_WhenOverrideDestinationDepotNotValid_ThenSelectionHasError()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WTGSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.DepotPK = orgHeader.PK;
			portHubSelection.TY_OA_DepotAddress = orgHeader.MainAddress.PK;

			AssertEquals("Expected AUSYD because it is the default UNLOCO for orgHeader for this portHubSelection", "AUSYD", portHubSelection.DepotPortCode);

			portHubSelection.TY_RL_NKDestinationPort = "XXXXX";
			portHubSelection.RunPreSaveValidation();
			AssertHasError("Destination Port Info should have an error because the UNLOCO does not exist.", portHubSelection.TY_RL_NKDestinationPortInfo, "Enter a valid selection.");

			portHubSelection.TY_RL_NKDestinationPort = "XXXX";
			portHubSelection.RunPreSaveValidation();
			AssertHasError("Destination Port Info should have an error because the UNLOCO is too short.", portHubSelection.TY_RL_NKDestinationPortInfo, "Enter a valid selection.");

			portHubSelection.TY_RL_NKDestinationPort = "AUSYD";
			portHubSelection.RunPreSaveValidation();
			AssertNoError("Destination Port Info should not have an error because the UNLOCO is valid.", portHubSelection.TY_RL_NKDestinationPortInfo, "Enter a valid selection.");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			PortHubSelection result = (PortHubSelection)base.GetNewBusinessObjectForDeleteTest(factory);
			var depotAddress = factory.NewWithValidTestData<OrgAddress>();
			result.TY_OA_DepotAddress = depotAddress.PK;
			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			PortHubSelection result = (PortHubSelection)base.GetBusinessObjectForFetchForLoad();
			var depotAddress = Factory.NewWithValidTestData<OrgAddress>();
			result.TY_OA_DepotAddress = depotAddress.PK;
			return result;
		}
	}
}
