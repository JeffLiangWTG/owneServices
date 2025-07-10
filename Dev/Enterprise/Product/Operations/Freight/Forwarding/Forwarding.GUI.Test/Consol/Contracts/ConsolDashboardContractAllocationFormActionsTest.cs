using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class ConsolDashboardContractAllocationFormActionsTest : TestCaseWithFactory
	{
		public void TestIsEnabledAllocationToContract()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var formActions = new ConsolDashboardContractAllocationFormActions(consol) as IRatingContractSimulationFormActions;
			AssertEquals("Allocation to Contract Enabled", true, formActions.IsEnabledAllocationToContract);
		}

		public void TestIsEnabledAllocationToRoute()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var formActions = new ConsolDashboardContractAllocationFormActions(consol) as IRatingContractSimulationFormActions;
			AssertEquals("Allocation to Contract Enabled", true, formActions.IsEnabledAllocationToRoute);
		}

		public void TestAllocateToContract()
		{
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_IsShippingLine = true;
			serviceProvider.OH_IsShippingProvider = true;
			serviceProvider.OH_FullName = "Carrier";
			serviceProvider.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			serviceProvider.OH_Code = "MAELI_WW";

			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "CONTRACT";
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract.RCT_StartDate = ZDate.Today;
			contract.RCT_OH = serviceProvider.PK;
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			contract.RCT_GS_NKContractOwner = "USR";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETD = ZDateTime.Today;
			transport.CarrierPK = serviceProvider.PK;

			var serviceProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "SHREK1ONVHS";

			var carrierContract2 = Factory.New<RatingContract>();
			carrierContract2.RCT_ContractNumber = "SHREK1234";
			carrierContract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			carrierContract2.RCT_StartDate = ZDate.Today.Add(new TimeSpan(-5, 12, 0, 0)).Date;
			carrierContract2.RCT_EndDate = ZDate.Today.Add(new TimeSpan(5, 12, 0, 0)).Date;
			carrierContract2.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract2.RCT_OH = serviceProvider2.PK;
			carrierContract2.RCT_GS_NKContractOwner = "SHR";
			consol.JK_OA_ShippingLineAddress = carrierContract2.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;

			Factory.Save();

			var formActions = new ConsolDashboardContractAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var canAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should terminate if Consol's Carrier is not empty and user selects 'No'.", !canAllocate);
			AssertNull("Consol should not have been allocated to Contract.", consol.CarrierContract);

			var expectedMessage = $"Service Provider MAELI_WW of Contract CONTRACT to be allocated is different from the Carrier SHREK1ONVHS on Consol.\r\n\r\nDo you want to override the Carrier on the Consol with MAELI_WW?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			canAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should proceed if Consol's Carrier is not empty and user selects 'Yes'.", canAllocate);
			AssertEquals("Consol should have been allocated to Contract.", contract.RCT_OH, consol.CarrierContract?.RCT_OH ?? ZGuid.Empty);

			AssertContains("Consol is allocated to Route and Saved", $"{consol.HumanReadableName} has been successfully allocated to {contract.HumanReadableName} and saved.", UnitTestUserNotification.Instance.LastMessage.ToString());

			var otherFactory = new BusinessObjectFactory();
			var loadedConsol = otherFactory.Load<ForwardingConsol>(consol.PK);
			AssertEquals("Consolidation has been allocated and saved to DB", contract.RCT_ContractNumber, loadedConsol.JK_CarrierContractNumber);
		}

		public void TestAllocateToAllocationRoute()
		{
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_IsShippingLine = true;
			serviceProvider.OH_IsShippingProvider = true;
			serviceProvider.OH_FullName = "Carrier";
			serviceProvider.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			serviceProvider.OH_Code = "MAELI_WW";

			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "CONTRACT";
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract.RCT_StartDate = ZDate.Today;
			contract.RCT_OH = serviceProvider.PK;
			contract.RCT_GS_NKContractOwner = "USR";

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_AllocatedQuantity = 100;
			allocationRoute.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.Containers;
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_AllocationLineID = "0001";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = serviceProvider.MainAddress.PK;
			consol.JK_MasterBillNum = "0007";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETD = ZDateTime.Today;
			transport.CarrierPK = serviceProvider.PK;

			var serviceProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "SHREK1ONVHS";

			var carrierContract2 = Factory.New<RatingContract>();
			carrierContract2.RCT_ContractNumber = "SHREK1234";
			carrierContract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			carrierContract2.RCT_StartDate = ZDate.Today.Add(new TimeSpan(-5, 12, 0, 0)).Date;
			carrierContract2.RCT_EndDate = ZDate.Today.Add(new TimeSpan(5, 12, 0, 0)).Date;
			carrierContract2.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract2.RCT_OH = serviceProvider2.PK;
			carrierContract2.RCT_GS_NKContractOwner = "SHR";
			consol.JK_OA_ShippingLineAddress = carrierContract2.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;

			Factory.Save();

			var formActions = new ConsolDashboardContractAllocationFormActions(consol) as IRatingContractSimulationFormActions;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var canAllocate = formActions.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation should terminate if Consol's Carrier is not empty and user selects 'No'.", !canAllocate);
			AssertNull("Consol should not have been allocated to Contract.", consol.CarrierContract);
			AssertNotEquals("Consol should not have been allocated to Route.", allocationRoute.PK, consol.JK_RCA_AllocationLine);

			var expectedMessage = $"Service Provider MAELI_WW of Contract CONTRACT and Allocation Route 0001 is different from the Carrier SHREK1ONVHS on Consol.\r\n\r\nDo you want to override the Carrier on the Consol with MAELI_WW?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			canAllocate = formActions.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation should proceed if Consol's Carrier is not empty and user selects 'Yes'.", canAllocate);
			AssertEquals("Consol should have been allocated to Contract.", contract.RCT_OH, consol.CarrierContract?.RCT_OH ?? ZGuid.Empty);
			AssertEquals("Consol should have been allocated to Route.", allocationRoute.PK, consol.JK_RCA_AllocationLine);

			AssertContains("Consol is allocated to Route and Saved", $"{consol.HumanReadableName} has been successfully allocated to {allocationRoute.HumanReadableName} and saved.", UnitTestUserNotification.Instance.LastMessage.ToString());

			var otherFactory = new BusinessObjectFactory();
			var loadedConsol = otherFactory.Load<ForwardingConsol>(consol.PK);
			AssertEquals("Consolidation has been allocated and saved to DB", allocationRoute.PK, loadedConsol.JK_RCA_AllocationLine);
		}
	}
}
