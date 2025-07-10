using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolContractAndRouteAllocationNotificationProviderTest : TestCaseWithFactory
	{
		public void TestGetContractAllocationNotification_ReturnsNullIfNoError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GOBLIN";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today.AddDays(1).Date;

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "OCTAVIUS";
			contract.RCT_TransportMode = "SEA";
			contract.RCT_StartDate = ZDateTime.Today.Date;
			contract.RCT_EndDate = ZDateTime.Today.AddDays(5).Date;

			var notificationProvider = new ConsolContractAndRouteAllocationNotificationProvider(consol);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNull(result);
		}

		public void TestGetContractAllocationNotification_ReturnsNotificationIfError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GOBLIN";
			consol.JK_IsHazardous = true;
			consol.JK_TransportMode = "SEA";

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "OCTAVIUS";
			contract.RCT_AllowHazardousCommodities = false;
			contract.RCT_TransportMode = "SEA";

			var notificationProvider = new ConsolContractAndRouteAllocationNotificationProvider(consol);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		public void TestGetRouteAllocationNotification_ReturnsNullIfNoError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "YOUCANTDOTHISTOME";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transportLeg = consol.Transports[0];
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg.JW_ETD = ZDateTime.Today;

			var contract = Factory.New<RatingContract>();
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			var route = Factory.New<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = "OUTAMI";
			route.RCA_LoadLocation = "AUSYD";
			route.RCA_DischargeLocation = "NZAKL";
			route.RCA_StartDate = ZDate.Today.AddDays(-5);
			route.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			route.RCA_RCT_RatingContract = contract.PK;

			var notificationProvider = new ConsolContractAndRouteAllocationNotificationProvider(consol);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNull(result);
		}

		public void TestGetRouteAllocationNotification_ReturnsNotificationIfError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "YOUCANTDOTHISTOME";
			consol.Transports[0].JW_ServiceString = "DOYOUKNOWHOWMUCHISACRIFICED";

			var route = Factory.New<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = "OUTAMI";
			route.RCA_ServiceLoop = "ASDASD";

			var notificationProvider = new ConsolContractAndRouteAllocationNotificationProvider(consol);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}
	}
}
