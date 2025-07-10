using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CCAContractConsolAssignmentValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, true);
			AssertEquals(notification, null);
		}

		public void TestCheckContract_StartDate()
		{
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			contract.RCT_StartDate = new ZDate(2022, 4, 2);
			contract.RCT_EndDate = new ZDate(2022, 4, 5);

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract Start Date is later than the Consol's ETD.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date (02-Apr-22) and the Expiry Date (05-Apr-22) of Contract R2D2.", notification.Message);
		}

		public void TestCheckContract_EndDate()
		{
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			contract.RCT_StartDate = new ZDate(2020, 1, 2);
			contract.RCT_EndDate = new ZDate(2020, 2, 12);

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract Expiry Date is earlier than the Consol's ETD.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date (02-Jan-20) and the Expiry Date (12-Feb-20) of Contract R2D2.", notification.Message);
		}

		public void TestCheckAllShipmentsHaveMatchingOrg()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "EvilInc01";

			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);
			contract.RCT_OH = carrier.PK;

			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when attached shipment don't have matching Named Account.", CargoWise.ComponentModel.NotificationType.Warning, notification.Type);
			AssertEquals("All of the Consol's Shipments should have at least one client that matches a Named Account of Carrier Contract R2D2 (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers for a Consol Shipment).", notification.Message);
		}

		public void TestCheckContainerType()
		{
			var container = Factory.New<ForwardingContainer>();
			var refCont = Factory.New<RefContainer>();
			refCont.RC_ContainerType = "LAA";
			contract.RCT_ContainerType = "BOO";
			container.JC_RC = refCont.PK;
			consol.Containers.Add(container);

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Ref Container Type and Contract Container Type is different.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("NOT all Containers on Consol EXPELLIARMUS share the same Container Type (BOO) of the selected Carrier Contract R2D2. Consol Container(s) and selected Carrier Contract should share the same Container Type.", notification.Message);
		}

		public void TestCheckAllowHazardousPreAlloc()
		{
			contract.RCT_AllowHazardousCommodities = false;
			consol.JK_IsHazardous = true;

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract does not allow hazardous commodities when Consol has hazardous Pre-Allocation.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Hazardous Commodities are not allowed for Carrier Contract R2D2 but Consol EXPELLIARMUS has Pre-Allocation > Is Hazardous checked. Only Consol(s) with 'Is Hazardous' not checked can be allocated.", notification.Message);

			consol.JK_IsHazardous = false;

			result = helper.IsAllowedToAllocateToContract(consol, contract, out notification);

			AssertEquals(result, true);
			AssertEquals("Shows no error when Consol is not hazardous and Contract doesn't allow hazardous commodities", null, notification);
		}

		public void TestCheckAllowHazardousCommodities()
		{
			contract.RCT_AllowHazardousCommodities = false;
			consol.JK_IsHazardous = false;

			var refCommodityCode = Factory.New<RefCommodityCode>();
			refCommodityCode.RH_IsHazardous = true;
			refCommodityCode.RH_Code = "NOX";

			var container = Factory.New<ForwardingContainer>();
			container.JC_RH_NKContainerCommodityCode = refCommodityCode.RH_Code;
			container.JC_ContainerNum = "IMPERIO";
			consol.Containers.Add(container);

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Consol and Contract is not hazardous but has hazardous container.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Hazardous Commodities are not allowed for Carrier Contract R2D2 but Container IMPERIO has Commodity NOX with 'Is this Commodity Hazardous' checked. Only Consol(s) and Container(s) without Hazardous Commodities can be allocated.", notification.Message);
		}

		public void TestCheckTransportMode()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_ETD = ZDateTime.Today;

			var result = helper.IsAllowedToAllocateToContract(consol, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract's Transport Mode does not match the Consol's.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Transport Mode 'AIR' of Consol is NOT the same as the Transport Mode 'SEA' of Carrier Contract for allocation.", notification.Message);

			consol.JK_TransportMode = "SEA";

			result = helper.IsAllowedToAllocateToContract(consol, contract, out notification);

			AssertEquals(result, true);
			AssertEquals("Shows no error when Consol's Transport Mode matches the Contract's.", null, notification);
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new CCAContractConsolAssignmentValidator();
			contract = Factory.NewWithValidTestData<CarrierContractForUtilizationSimulation>();
			contract.RCT_ContractNumber = "R2D2";
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "EXPELLIARMUS";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
		}

		CCAContractConsolAssignmentValidator helper;
		CarrierContractForUtilizationSimulation contract;
		ForwardingConsol consol;
	}
}
