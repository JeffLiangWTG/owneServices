using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class ConsolContractAndRouteAllocationFormActionsTest : TestCaseWithFactory
	{
		public void TestIsEnabledProperties()
		{
			var container = Factory.New<ForwardingContainer>();
			var formActions = new ConsolContractAndRouteAllocationFormActions(container) as IRatingContractSimulationFormActions;
			AssertEquals("If Consol is null, allocation should be disallowed.", false, formActions.IsEnabledAllocationToContract);
			AssertEquals("If Consol is null, allocation should be disallowed.", false, formActions.IsEnabledAllocationToRoute);

			container.JC_JK = Factory.New<ForwardingConsol>().PK;
			formActions = new ConsolContractAndRouteAllocationFormActions(container);
			AssertEquals("If Consol is not null, allocation should be allowed.", true, formActions.IsEnabledAllocationToContract);
			AssertEquals("If Consol is not null, allocation should be allowed.", true, formActions.IsEnabledAllocationToRoute);
		}

		public void TestAllocateToContract()
		{
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_Code = "MAELI_WW";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = serviceProvider.PK;

			var serviceProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "SHREK1ONVHS";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "FIONAAA";
			consol.JK_RCA_AllocationLine = ZGuid.NewZGuid();
			consol.JK_OA_ShippingLineAddress = serviceProvider2.MainAddress?.PK ?? ZGuid.Empty;

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = ZGuid.NewZGuid();

			var formActions = new ConsolContractAndRouteAllocationFormActions(container) as IRatingContractSimulationFormActions;

			var canAllocate = formActions.TryAllocateToContract(null);
			Assert("Allocation should terminate if contract is null.", !canAllocate);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			canAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should terminate if Consol Carrier is not empty and user selects 'No'.", !canAllocate);

			var expectedMessage = $"Service Provider MAELI_WW of Contract SHREK123 to be allocated is different from the Carrier SHREK1ONVHS on Consol.\r\n\r\nDo you want to override the Carrier on the Consol with MAELI_WW?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			canAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should proceed if Consol Carrier is not empty and user selects 'Yes'.", canAllocate);
			AssertEquals("Consol should have been allocated to Contract.", contract.RCT_OH, consol.CarrierContract.RCT_OH);

			canAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should proceed if Consol Carrier is the same as Contract Carrier.", canAllocate);
			AssertEquals("Consol should have had its allocation route cleared.", ZGuid.Empty, consol.JK_RCA_AllocationLine);
			AssertEquals("Container should have had its allocation route cleared.", ZGuid.Empty, container.JC_RCA_AllocationLine);
		}

		public void TestAllocateToAllocationRoute()
		{
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_Code = "MAELI_WW";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = serviceProvider.PK;

			var serviceProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "SHREK1ONVHS";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "FIONAAA";
			consol.JK_RCA_AllocationLine = ZGuid.NewZGuid();
			consol.JK_OA_ShippingLineAddress = serviceProvider2.MainAddress?.PK ?? ZGuid.Empty;

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_AllocationLineID = "00000001";

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation should terminate if Consol's Carrier is not empty and user selects 'No'.", !canAllocate);
			AssertNull("Consol should not have been allocated to Contract.", consol.CarrierContract);
			AssertNotEquals("Consol should not have been allocated to Route.", allocationRoute.PK, consol.JK_RCA_AllocationLine);

			var expectedMessage = $"Service Provider MAELI_WW of Contract SHREK123 and Allocation Route 00000001 is different from the Carrier SHREK1ONVHS on Consol.\r\n\r\nDo you want to override the Carrier on the Consol with MAELI_WW?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation should proceed if Consol's Carrier is not empty and user selects 'Yes'.", canAllocate);
			AssertEquals("Consol should have been allocated to Contract.", contract.PK, consol.CarrierContract.PK);
			AssertEquals("Consol should have been allocated to Route.", allocationRoute.PK, consol.JK_RCA_AllocationLine);

			var container = consol.Containers.AddNew();
			formDataProvider = new ConsolContractAndRouteAllocationFormActions(container);
			formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			AssertEquals("Container should have been allocated to Route.", allocationRoute.PK, container.JC_RCA_AllocationLine);
		}

		public void TestAllocateToAllocationRoute_LinkedRoute()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();
			var leg = consol.Transports[0];
			leg.JW_Vessel = "DifferenBoat";

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation not allowed as Schedule linking denied", !canAllocate);
			AssertEquals("Allocation not done", consol.JK_RCA_AllocationLine, ZGuid.Empty);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation allowed, as Schedule linking accepted", canAllocate);
			AssertEquals("Allocation complete", consol.JK_RCA_AllocationLine, allocationRoute.PK);

			var linkedLeg = consol.Transports.OfType<Transport>().Where(leg => leg.JW_JX == schedule.PK);
			AssertNotNull("Linked Leg Created", linkedLeg);
		}

		public void TestAllocateToAllocationRoute_LinkedRouteExists()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();
			var leg = consol.Transports[0];
			leg.JW_IsLinked = true;
			leg.JW_JX = schedule.PK;

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation allowed, no dialog options", canAllocate);
			AssertEquals("Allocation done", consol.JK_RCA_AllocationLine, allocationRoute.PK);
		}

		public void TestAllocateToAllocationRoute_LinkedRouteDoesNotMatch()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();
			schedule.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			schedule.Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			schedule.Origin.JA_E_DEP = new ZDateTime(2024, 4, 20);
			schedule.Voyage.JV_RV_NKVessel = "ShreksBoat";
			schedule.Voyage.JV_VoyageFlight = "SHRK1234";
			schedule.JX_ServiceString = "FI0000NA";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "SHRK9696";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "ShreksConsol";
			var leg = consol.Transports[0];
			leg.JW_Vessel = "DifferenBoat";

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert(!canAllocate);

			var expectedMessage = "Linked Schedule: AUSYD, NZAKL, 20-Apr-24, ShreksBoat, SHRK1234, FI0000NA of Allocation Route (SHRK9696) should match one of the Routing "
				+ "Legs of Consol (ShreksConsol).\r\nWould you like to use the Linked Schedule to create a new Routing Leg to complete the Allocation?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAllocateToAllocationRoute_MessageIfConsolNotSaved()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();
			schedule.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			schedule.Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			schedule.Origin.JA_E_DEP = new ZDateTime(2024, 4, 20);
			schedule.Voyage.JV_RV_NKVessel = "ShreksBoat";
			schedule.Voyage.JV_VoyageFlight = "SHRK1234";
			schedule.JX_ServiceString = "FI0000NA";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "SHRK9696";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert(!canAllocate);

			var expectedMessage = "Linked Schedule: AUSYD, NZAKL, 20-Apr-24, ShreksBoat, SHRK1234, FI0000NA of Allocation Route (SHRK9696) should match one of the Routing "
				+ "Legs of the Consol.\r\nWould you like to use the Linked Schedule to create a new Routing Leg to complete the Allocation?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAllocateToAllocationRoute_LinkedRoutePartiallyMatch()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();
			schedule.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			schedule.Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			schedule.Origin.JA_E_DEP = new ZDateTime(2024, 4, 20);
			schedule.Voyage.JV_RV_NKVessel = "ShreksBoat";
			schedule.Voyage.JV_VoyageFlight = "SHRK1234";
			schedule.JX_ServiceString = "FI0000NA";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "SHRK9696";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "ShreksConsol";

			var leg = consol.Transports[0];
			leg.JW_Vessel = "ShreksBoat";
			leg.JW_RL_NKLoadPort = "AUSYD";

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation allowed, as default leg partial match to Schedule", canAllocate);
			AssertEquals("Allocation complete", consol.JK_RCA_AllocationLine, allocationRoute.PK);

			var linkedLeg = consol.Transports.OfType<Transport>().Where(leg => leg.JW_JX == schedule.PK);
			AssertNotNull("Linked Leg Created", linkedLeg);
			AssertEquals("Allocation replaced single leg", consol.Transports.Count, 1);
		}

		public void TestAllocateToAllocationRoute_MultipleConsolLegsWithLinkedRoutePartiallyMatch()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();
			schedule.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			schedule.Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			schedule.Origin.JA_E_DEP = new ZDateTime(2024, 4, 20);
			schedule.Voyage.JV_RV_NKVessel = "ShreksBoat";
			schedule.Voyage.JV_VoyageFlight = "SHRK1234";
			schedule.JX_ServiceString = "FI0000NA";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "SHRK9696";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "ShreksConsol";

			var legPartiallyMatch = consol.Transports[0];
			legPartiallyMatch.JW_Vessel = "ShreksBoat";
			legPartiallyMatch.JW_RL_NKLoadPort = "AUSYD";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_Vessel = "ShreksBoat";
			leg2.JW_RL_NKLoadPort = "AUSYD";

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			AssertEquals("Allocation complete", consol.JK_RCA_AllocationLine, allocationRoute.PK);
			var linkedLeg = consol.Transports.OfType<Transport>().Where(leg => leg.JW_JX == schedule.PK);
			AssertNotNull("Linked Leg Created", linkedLeg);
			AssertEquals("Allocation creates a third leg", consol.Transports.Count, 3);
		}

		public void TestAllocateToAllocationRoute_LinkedRoutePartiallyDoesNotMatch()
		{
			var schedule = Factory.NewWithValidTestData<JobSailing>();
			schedule.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			schedule.Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			schedule.Origin.JA_E_DEP = new ZDateTime(2024, 4, 20);
			schedule.Voyage.JV_RV_NKVessel = "ShreksBoat";
			schedule.Voyage.JV_VoyageFlight = "SHRK1234";
			schedule.JX_ServiceString = "FI0000NA";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "SHRK9696";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "ShreksConsol";

			var leg = consol.Transports[0];
			leg.JW_Vessel = "DifferentBoat";
			leg.JW_RL_NKLoadPort = "AUSYD";

			var formDataProvider = new ConsolContractAndRouteAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			var canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation not allowed, different vessel information", !canAllocate);

			var expectedMessage = "Linked Schedule: AUSYD, NZAKL, 20-Apr-24, ShreksBoat, SHRK1234, FI0000NA of Allocation Route (SHRK9696) should match one of the Routing "
				+ "Legs of Consol (ShreksConsol).\r\nWould you like to use the Linked Schedule to create a new Routing Leg to complete the Allocation?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg.JW_Vessel = "ShreksBoat";
			leg.JW_RL_NKLoadPort = "AUMEL";
			canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation not allowed, different Loading Port information", !canAllocate);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg.JW_RL_NKDiscPort = "AUMEL";
			canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation not allowed, different Discharge Port information", !canAllocate);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg.JW_VoyageFlight = "DIFF1234";
			canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation not allowed, different Loading Voyage information", !canAllocate);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg.JW_ETD = new ZDateTime(2024, 7, 1);
			canAllocate = formDataProvider.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation not allowed, different Departure Date information", !canAllocate);
		}
	}
}
