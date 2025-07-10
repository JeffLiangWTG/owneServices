using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ContainerContractAndRouteAllocationNotificationProviderTest : TestCaseWithFactory
	{
		public void TestGetContractAllocationNotification_ReturnsNullIfNoError()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "GOBLINJR";

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "OCTAVIUS";

			var notificationProvider = new ConsolContainerAllocationNotificationProvider(container);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNull(result);
		}

		public void TestGetContractAllocationNotification_ReturnsNotificationIfError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GOBLIN";
			consol.JK_IsHazardous = true;

			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "GOBLINJR";
			container.JC_JK = consol.PK;

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "OCTAVIUS";
			contract.RCT_AllowHazardousCommodities = false;

			var notificationProvider = new ConsolContainerAllocationNotificationProvider(container);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		public void TestGetRouteAllocationNotification_ReturnsNullIfNoError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GOBLIN";
			var leg = consol.Transports[0];
			leg.JW_TransportMode = Core.Constants.TransportModes.Sea;

			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "GOBLINJR";
			container.JC_JK = consol.PK;

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "OCTAVIUS";

			var route = Factory.New<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = "OUTAMI";
			route.RCA_RCT_RatingContract = contract.PK;

			var notificationProvider = new ConsolContainerAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNull(result);
		}

		public void TestGetRouteAllocationNotification_ReturnsNotificationIfError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GOBLIN";

			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "GOBLINJR";
			container.JC_RC = Guid.NewGuid();
			container.JC_JK = consol.PK;

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "OCTAVIUS";

			var route = Factory.New<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = "OUTAMI";
			route.RCA_ServiceLoop = "ASDASD";
			route.RCA_RC_ContainerType = Guid.NewGuid();
			route.RCA_RCT_RatingContract = contract.PK;

			var notificationProvider = new ConsolContainerAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}
	}
}
