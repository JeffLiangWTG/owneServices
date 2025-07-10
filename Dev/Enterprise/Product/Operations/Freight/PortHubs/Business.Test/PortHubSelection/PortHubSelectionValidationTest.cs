using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.PortHubs.Testing
{
	internal class PortHubSelectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTY_F3_NKPackType()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();

			portDepotSelection.TY_F3_NKPackType = "XXX";
			AssertHasErrors(portDepotSelection.TY_F3_NKPackTypeInfo);

			portDepotSelection.TY_F3_NKPackType = "PLT";
			AssertNoErrors(portDepotSelection.TY_F3_NKPackTypeInfo);
		}

		public void TestCheckTY_Direction()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();

			portDepotSelection.TY_Direction = "XXX";
			AssertHasErrors(portDepotSelection.TY_DirectionInfo);

			portDepotSelection.TY_Direction = PortHubSelectionDirectionList.Codes.Pickup;
			AssertNoErrors(portDepotSelection.TY_DirectionInfo);

			portDepotSelection.TY_Direction = ZString.Empty;
			AssertHasErrors(portDepotSelection.TY_DirectionInfo);
		}

		public void TestCheckTY_UndgClass()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();

			portDepotSelection.TY_UndgClass = "XXX";
			AssertHasErrors(portDepotSelection.TY_UndgClassInfo);

			portDepotSelection.TY_UndgClass = "1.1F";
			AssertNoErrors(portDepotSelection.TY_UndgClassInfo);

			portDepotSelection.TY_UndgClass = PortHubSelection.All;
			AssertNoErrors(portDepotSelection.TY_UndgClassInfo);
		}

		public void TestCheckTY_RS_NKServiceLevel()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();

			portDepotSelection.TY_RS_NKServiceLevel = "XXX";
			AssertHasErrors(portDepotSelection.TY_RS_NKServiceLevelInfo);

			portDepotSelection.TY_RS_NKServiceLevel = "STD";
			AssertNoErrors(portDepotSelection.TY_RS_NKServiceLevelInfo);
		}

		public void TestCheckTY_RatingFreightMode()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();

			portDepotSelection.TY_RatingFreightMode = "XXX";
			AssertHasErrors(portDepotSelection.TY_RatingFreightModeInfo);

			portDepotSelection.TY_RatingFreightMode = Constants.TransportModes.Sea;
			AssertNoErrors(portDepotSelection.TY_RatingFreightModeInfo);

			portDepotSelection.TY_RatingFreightMode = ZString.Empty;
			AssertHasErrors(portDepotSelection.TY_RatingFreightModeInfo);

			portDepotSelection.TY_RatingFreightMode = Constants.TransportModes.All;
			AssertNoErrors(portDepotSelection.TY_RatingFreightModeInfo);
		}

		public void TestCheckTY_OA_DepotAddress()
		{
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_FullName = "Depot";

			var selection = Factory.New<PortHubSelection>();
			selection.TY_OA_DepotAddress = depot.MainAddress.PK;
			AssertHasErrors("The organization you select is not a valid Depot.", selection.TY_OA_DepotAddressInfo);

			var depotValid = Factory.NewWithValidTestData<OrgHeader>();
			depotValid.OH_IsMiscFreightServices = true;
			depotValid.OH_IsPackDepot = true;
			selection.TY_OA_DepotAddress = depotValid.MainAddress.PK;
			AssertNoErrors("The Depot is a valid organization", selection.TY_OA_DepotAddressInfo);

			selection.TY_OA_DepotAddress = ZGuid.Empty;
			AssertHasErrors("The Depot is mandatory", selection.TY_OA_DepotAddressInfo);
		}

		public void TestCheckTY_OA_DispatchDepotAddress()
		{
			var dispatchDepot = Factory.NewWithValidTestData<OrgHeader>();
			dispatchDepot.OH_FullName = "DispatchDepot";

			var selection = Factory.New<PortHubSelection>();
			selection.TY_OA_DispatchDepotAddress = dispatchDepot.MainAddress.PK;
			AssertHasErrors("The organization you select is not a valid Depot.", selection.TY_OA_DispatchDepotAddressInfo);

			var dispatchDepotValid = Factory.NewWithValidTestData<OrgHeader>();
			dispatchDepotValid.OH_IsMiscFreightServices = true;
			dispatchDepotValid.OH_IsPackDepot = true;
			selection.TY_OA_DispatchDepotAddress = dispatchDepotValid.MainAddress.PK;
			AssertNoErrors("The DispatchDepot is a valid organization", selection.TY_OA_DispatchDepotAddressInfo);

			selection.TY_OA_DispatchDepotAddress = ZGuid.Empty;
			AssertNoErrors("The DispatchDepot is not mandatory", selection.TY_OA_DispatchDepotAddressInfo);
		}

		public void TestCheckDepotPK()
		{
			var selection = Factory.NewWithValidTestData<PortHubSelection>();

			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_FullName = "Depot";
			depot.OH_IsMiscFreightServices = true;
			depot.OH_IsPackDepot = true;

			selection.DepotPK = depot.PK;
			selection.TY_OA_DepotAddress = depot.MainAddress.PK;

			AssertNoErrors("The Depot is a valid organization", selection.DepotPKInfo);

			selection.DepotPK = ZGuid.Invalid;

			AssertHasErrors("The Depot is not a valid organization", selection.DepotPKInfo);
		}

		public void TestCheckDispatchDepotPK()
		{
			var selection = Factory.NewWithValidTestData<PortHubSelection>();

			var dispatchDepot = Factory.NewWithValidTestData<OrgHeader>();
			dispatchDepot.OH_FullName = "DispatchDepot";
			dispatchDepot.OH_IsMiscFreightServices = true;
			dispatchDepot.OH_IsPackDepot = true;

			selection.DispatchDepotPK = dispatchDepot.PK;
			selection.TY_OA_DispatchDepotAddress = dispatchDepot.MainAddress.PK;

			AssertNoErrors("The DispatchDepot is a valid organization", selection.DispatchDepotPKInfo);

			selection.DispatchDepotPK = ZGuid.Invalid;

			AssertHasErrors("The DispatchDepot is not a valid organization", selection.DispatchDepotPKInfo);
		}

		public void TestCheckTY_OH_CarrierBookingAgent()
		{
			var selection = Factory.NewWithValidTestData<PortHubSelection>();
			var error = "Please enter a Destination Depot or Carrier Booking Agent.";

			var depot = Factory.NewWithValidTestData<OrgHeader>();
			var carrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();

			depot.OH_FullName = "Depot";
			depot.OH_IsMiscFreightServices = true;
			depot.OH_IsPackDepot = true;

			selection.DepotPK = depot.PK;
			selection.TY_OH_CarrierBookingAgent = ZGuid.Empty;
			selection.TY_OA_DepotAddress = depot.MainAddress.PK;

			AssertNoErrors("The Depot is a valid organization, no depot errors", selection.DepotPKInfo);
			AssertNoErrors("The Depot is a valid organization, no CBA errors", selection.TY_OH_CarrierBookingAgentInfo);

			selection.TY_OH_CarrierBookingAgent = carrierBookingAgent.PK;
			selection.DepotPK = ZGuid.Empty;

			AssertNoErrors("The CBA is a valid organization, no depot errors", selection.DepotPKInfo);
			AssertNoErrors("The CBA is a valid organization, no CBA errors", selection.TY_OH_CarrierBookingAgentInfo);

			selection.TY_OH_CarrierBookingAgent = ZGuid.Empty;
			selection.DepotPK = ZGuid.Empty;

			AssertHasError("The CBA and Depot are not valid organizations, has depot errors", selection.DepotPKInfo, error);
			AssertHasError("The CBA and Depot are not valid organizations, has CBA errors", selection.TY_OH_CarrierBookingAgentInfo, error);
		}

		public void TestCheckTY_PackMode()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();
			var error = "Enter a valid Pack Mode.";

			portDepotSelection.TY_PackMode = "XXX";
			AssertHasError(portDepotSelection.TY_PackModeInfo, error);

			portDepotSelection.TY_PackMode = "PAR";
			AssertHasError(portDepotSelection.TY_PackModeInfo, error);

			portDepotSelection.TY_PackMode = "LTL";
			AssertHasError(portDepotSelection.TY_PackModeInfo, error);

			portDepotSelection.TY_PackMode = "";
			AssertNoErrors(portDepotSelection.TY_PackModeInfo);

			portDepotSelection.TY_PackMode = "CNT";
			AssertNoErrors(portDepotSelection.TY_PackModeInfo);

			portDepotSelection.TY_PackMode = "LSE";
			AssertNoErrors(portDepotSelection.TY_PackModeInfo);
		}
	}
}
