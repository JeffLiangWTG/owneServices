using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AllocationRouteContainerWeightLimitHelperTest : TestCaseWithFactory
	{
		#region Validation Logic Tests

		public void TestContainerWeightLimitValidationLogic()
		{
			var testCases = new List<(string Name, decimal Limit, string Unit, string LimitType, int[] Weights, string ExpectedMessage)>
			{
				(
					Name: "TestSomeContainerWeightExceedsAbsoluteLimitNoConversion",
					Limit: 200m,
					Unit: "KG",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 210 },
					ExpectedMessage: "Over-limit. Per-TEU gross weight exceeds the 200.00KG limit on allocation route WARATAH."),
				(
					Name: "TestSomeContainerWeightEqualsAbsoluteLimitNoConversion",
					Limit: 200m,
					Unit: "KG",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 200 },
					ExpectedMessage: ""),
				(
					Name: "TestNoContainerWeightReachesAbsoluteLimitNoConversion",
					Limit: 200m,
					Unit: "KG",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 190 },
					ExpectedMessage: ""),
				(
					Name: "TestContainerWeightExceedsAverageLimitNoConversion",
					Limit: 200m,
					Unit: "KG",
					LimitType: ContainerWeightLimitType.AveragePerTEU,
					Weights: new[] { 300, 170, 160, 210 },
					ExpectedMessage: "Over-limit. Avg per-TEU weight exceeds the 200.00KG limit on allocation route WARATAH."),
				(
					Name: "TestContainerWeightEqualsAverageLimitNoConversion",
					Limit: 200m,
					Unit: "KG",
					LimitType: "AVT",
					Weights: new[] { 262, 138, 176, 224 },
					ExpectedMessage: ""),
				(
					Name: "TestContainerWeightDoesNotReachAverageLimitNoConversion",
					Limit: 200m,
					Unit: "KG",
					LimitType: "AVT",
					Weights: new[] { 180, 5, 12, 190 },
					ExpectedMessage: ""),
				(
					Name: "TestSomeContainerWeightExceedsAbsoluteLimitConvertTL",
					Limit: 0.19684130529m,
					Unit: "TL",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 210 },
					ExpectedMessage: "Over-limit. Per-TEU gross weight exceeds the 0.20TL limit on allocation route WARATAH."),
				(
					Name: "TestSomeContainerWeightEqualsAbsoluteLimitConvertTL",
					Limit: 0.19684130529m,
					Unit: "TL",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 200 },
					ExpectedMessage: ""),
				(
					Name: "TestNoContainerWeightReachesAbsoluteLimitConvertTL",
					Limit: 0.19684130529m,
					Unit: "TL",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 190 },
					ExpectedMessage: ""),
				(
					Name: "TestContainerWeightExceedsAverageLimitConvertTL",
					Limit: 0.19684130529m,
					Unit: "TL",
					LimitType: "AVT",
					Weights: new[] { 300, 170, 160, 210 },
					ExpectedMessage: "Over-limit. Avg per-TEU weight exceeds the 0.20TL limit on allocation route WARATAH."),
				(
					Name: "TestContainerWeightEqualsAverageLimitConvertTL",
					Limit: 0.19684130529m,
					Unit: "TL",
					LimitType: "AVT",
					Weights: new[] { 262, 138, 176, 224 },
					ExpectedMessage: ""),
				(
					Name: "TestContainerWeightDoesNotReachAverageLimitConvertTL",
					Limit: 0.19684130529m,
					Unit: "TL",
					LimitType: "AVT",
					Weights: new[] { 180, 5, 12, 190 },
					ExpectedMessage: ""),
				(
					Name: "TestSomeContainerWeightExceedsAbsoluteLimitConvertTonnes",
					Limit: 0.2m,
					Unit: "T",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 210 },
					ExpectedMessage: "Over-limit. Per-TEU gross weight exceeds the 0.20T limit on allocation route WARATAH."),
				(
					Name: "TestSomeContainerWeightEqualsAbsoluteLimitConvertTonnes",
					Limit: 0.2m,
					Unit: "T",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 200 },
					ExpectedMessage: ""),
				(
					Name: "TestNoContainerWeightReachesAbsoluteLimitConvertTonnes",
					Limit: 0.2m,
					Unit: "T",
					LimitType: ContainerWeightLimitType.AbsolutePerTEU,
					Weights: new[] { 180, 5, 12, 190 },
					ExpectedMessage: ""),
				(
					Name: "TestContainerWeightExceedsAverageLimitConvertTonnes",
					Limit: 0.2m,
					Unit: "T",
					LimitType: "AVT",
					Weights: new[] { 300, 170, 160, 210 },
					ExpectedMessage: "Over-limit. Avg per-TEU weight exceeds the 0.20T limit on allocation route WARATAH."),
				(
					Name: "TestContainerWeightEqualsAverageLimitConvertTonnes",
					Limit: 0.2m,
					Unit: "T",
					LimitType: "AVT",
					Weights: new[] { 262, 138, 176, 224 },
					ExpectedMessage: ""),
				(
					Name: "TestContainerWeightDoesNotReachAverageLimitConvertTonnes",
					Limit: 0.2m,
					Unit: "T",
					LimitType: "AVT",
					Weights: new[] { 180, 5, 12, 190 },
					ExpectedMessage: ""),
			};

			foreach (var testCase in testCases)
			{
				RunSingleContainerWeightValidationTest(testCase.Name, testCase.Limit, testCase.Unit, testCase.LimitType, testCase.Weights, testCase.ExpectedMessage);
			}
		}

		void RunSingleContainerWeightValidationTest(
			string testName,
			decimal limit,
			string unit,
			string limitType,
			int[] grossWeights,
			string expectedMessage)
		{
			ConfigureContextMock(userPassedEscalationDialog: false, userPassedConfirmationDialog: false, hasSecurityRole: false);
			allocationRoute.RCA_ContainerWeightLimit = limit;
			allocationRoute.RCA_ContainerWeightLimitUQ = unit;
			allocationRoute.RCA_ContainerWeightLimitType = limitType;

			for (int i = 0; i < 4; i++)
			{
				containers[i].JC_GrossWeight = grossWeights[i];
				containers[i].JC_GrossWeightUQ = "KG";
				containers[i].JC_RCA_AllocationLine = allocationRoute.PK;
			}

			var weightIsValid = allocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute);
			var message = CCAValidationMessageProvider.GrossContainerWeightExceedsAllocationLimit(allocationRoute);
			Assert(testName, weightIsValid == string.IsNullOrEmpty(expectedMessage));

			if (!weightIsValid)
			{
				Assert(testName + " message", message == expectedMessage);
			}
		}

		#endregion

		#region Escalation Dialog Tests

		public void TestErrorWithoutGUIContext()
		{
			SetupEscalationFlowTests(userPassedEscalationDialog: null, userPassedConfirmationDialog: null, hasSecurityRole: true);

			var result = allocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute);
			Assert("Error occurs with no GUI context and valid security role", !result);
		}

		public void TestProceedWithSecurityRole()
		{
			SetupEscalationFlowTests(userPassedEscalationDialog: true, userPassedConfirmationDialog: true, hasSecurityRole: true);

			var result = allocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute);
			Assert("Proceed with security role allows override", result);
		}

		public void TestCancelWithSecurityRole()
		{
			SetupEscalationFlowTests(userPassedEscalationDialog: true, userPassedConfirmationDialog: false, hasSecurityRole: true);

			var result = allocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute);
			Assert("Cancel with security role creates error", !result);
		}

		public void TestCancelWithoutSecurityRole()
		{
			SetupEscalationFlowTests(userPassedEscalationDialog: false, userPassedConfirmationDialog: false, hasSecurityRole: false);

			var result = allocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute);
			Assert("Cancel without security role creates error", !result);
		}

		public void TestProceedWithoutSecurityRole()
		{
			SetupEscalationFlowTests(userPassedEscalationDialog: true, userPassedConfirmationDialog: true, hasSecurityRole: false);

			var result = allocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute);
			Assert("Proceed without security role allows override through escalation", result);
		}

		#endregion

		#region Event Tests

		public void TestEventCreatedAfterOverrideConsol()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			var logs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Consol has ACA log", logs.Count() == 1);
		}

		public void TestEventCreatedAfterOverrideContainer()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();

			var logs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Container has ACA log", logs.Count() == 1);
		}

		public void TestEventIsNotCreatedAfterUnsuccessfulOverrideConsol()
		{
			SetupOverweightScenario(confirmOverride: false, allowOverride: false);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			var logs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Consol does not have ACA log", !logs.Any());
		}

		public void TestEventIsNotCreatedAfterUnsuccessfulOverrideContainer()
		{
			SetupOverweightScenario(confirmOverride: false, allowOverride: false);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;

			var logs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Container does not have ACA log", !logs.Any());
		}

		public void TestEventIsCancelledAfterDeallocateConsol()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();

			var initialLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Consol had ACA log", initialLogs.Any());

			consol.JK_RCA_AllocationLine = ZGuid.Empty;
			Factory.Save();

			var finalLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode).ToList();
			Assert("ACA log cancelled and no new log created", finalLogs.Count == 1 && finalLogs[0].SL_IsCancelled);
		}

		public void TestEventIsCancelledAfterDeallocateContainer()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();

			var initialLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Container had ACA log", initialLogs.Any());

			containers[0].JC_RCA_AllocationLine = ZGuid.Empty;
			Factory.Save();

			var finalLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode).ToList();
			Assert("ACA log cancelled and no new log created", finalLogs.Count == 1 && finalLogs[0].SL_IsCancelled);
		}

		public void TestEventIsCancelledAfterChangeAllocationConsol()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();

			var initialLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Consol had ACA log", initialLogs.Any());

			consol.JK_RCA_AllocationLine = secondaryAllocationRoute.PK;
			Factory.Save();

			var finalLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode).ToList();
			Assert("ACA log cancelled and no new log created", finalLogs.Count == 1 && finalLogs[0].SL_IsCancelled);
		}

		public void TestEventIsCancelledAfterChangeAllocationContainer()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();

			var initialLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode);
			Assert("Container had ACA log", initialLogs.Any());

			containers[0].JC_RCA_AllocationLine = secondaryAllocationRoute.PK;
			Factory.Save();

			var finalLogs = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.ApprovedAllocationWithContainerWeightLimitExceededCode).ToList();
			Assert("ACA log cancelled and no new log created", finalLogs.Count == 1 && finalLogs[0].SL_IsCancelled);
		}

		public void TestValidationSkippedIfEventCreatedConsol()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			consol.Validation.ValidateJK_RCA_AllocationLine();
			Assert("Consol validation skipped, so there are no errors", !consol.JK_RCA_AllocationLineInfo.HasErrors());
		}

		public void TestValidationSkippedIfEventCreatedContainer()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;

			containers[0].Validation.ValidateJC_RCA_AllocationLine();
			Assert("Container validation skipped, so there are no errors", !containers[0].JC_RCA_AllocationLineInfo.HasErrors());
		}

		public void TestValidationRunAfterEventCancellationConsol()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();
			consol.JK_RCA_AllocationLine = secondaryAllocationRoute.PK;
			Factory.Save();

			SetupOverweightScenario(confirmOverride: false, allowOverride: false);
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			Assert("Consol validation event cancelled, so there are errors", consol.JK_RCA_AllocationLineInfo.HasErrors());
		}

		public void TestValidationRunAfterEventCancellationContainer()
		{
			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();
			containers[0].JC_RCA_AllocationLine = secondaryAllocationRoute.PK;
			Factory.Save();

			SetupOverweightScenario(confirmOverride: false, allowOverride: false);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;

			Assert("Container validation event cancelled, so there are errors", containers[0].JC_RCA_AllocationLineInfo.HasErrors());
		}

		public void TestEventCancelledAfterDeallocateContainerFromConsol()
		{
			var logQuery = new ZQuery(StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, false);
			logQuery.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.ApprovedAllocationWithContainerWeightLimitExceededCode));

			SetupOverweightScenario(confirmOverride: true, allowOverride: true);
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;
			Factory.Save();

			Assert("Consol override logs created", consol.Logs.Find(logQuery).Length == 1);

			consol.Containers.Remove(containers[0]);
			Factory.Save();

			Assert("Consol override logs cleared", consol.Logs.Find(logQuery).Length == 0);
		}

		#endregion

		#region Helper Functions

		void SetupOverweightScenario(bool confirmOverride, bool allowOverride)
		{
			ConfigureContextMock(allowOverride, confirmOverride, hasSecurityRole: true);

			allocationRoute.RCA_ContainerWeightLimit = 5;
			allocationRoute.RCA_ContainerWeightLimitType = ContainerWeightLimitType.AbsolutePerTEU;
			allocationRoute.RCA_ContainerWeightLimitUQ = "KG";

			containers[0].JC_GrossWeight = 6;
			containers[0].JC_GrossWeightUQ = "KG";
		}

		void SetupEscalationFlowTests(bool? userPassedEscalationDialog, bool? userPassedConfirmationDialog, bool hasSecurityRole)
		{
			ConfigureContextMock(userPassedEscalationDialog, userPassedConfirmationDialog, hasSecurityRole);

			allocationRoute.RCA_ContainerWeightLimit = 2;
			allocationRoute.RCA_ContainerWeightLimitUQ = "KG";
			allocationRoute.RCA_ContainerWeightLimitType = ContainerWeightLimitType.AbsolutePerTEU;

			containers[0].JC_GrossWeightUQ = "KG";
			containers[0].JC_GrossWeight = 5;
			containers[0].JC_RCA_AllocationLine = allocationRoute.PK;
		}

		void ConfigureContextMock(bool? userPassedEscalationDialog, bool? userPassedConfirmationDialog, bool hasSecurityRole)
		{
			Env.Security.ApproveAllocationExceedContainerWeightLimit.IsAllowed = hasSecurityRole;

			if (userPassedEscalationDialog != null)
			{
				attachmentMock
					.Setup(mock => mock.AllowContainerWeightLimitOverrideBySecurityRole(It.IsAny<string>()))
					.Returns(userPassedEscalationDialog.Value);
			}

			if (userPassedConfirmationDialog != null)
			{
				attachmentMock
					.Setup(mock => mock.ConfirmContainerWeightLimitOverride(It.IsAny<string>()))
					.Returns(userPassedConfirmationDialog.Value);
			}

			if (userPassedEscalationDialog == null || userPassedConfirmationDialog == null)
			{
				consol.Factory.SetValue<IAllocationContainerWeightLimitDialogsProvider>(() => null);
			}
		}

		ForwardingContainer CreateForwardingContainer(string containerId, RefContainer refContainer)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerJobID = containerId;
			container.JC_RC = refContainer.PK;
			container.JC_ContainerCount = 1;
			container.Container.RC_TEU = 1;
			return container;
		}

		static RatingContractAllocationLine CreateAllocationRoute(RatingContract contract, string lineId, ZDate start, ZDate expiry, string load, string discharge)
		{
			var allocation = contract.Allocations.AddNew();
			allocation.RCA_AllocationLineID = lineId;
			allocation.RCA_AllocatedQuantity = 100;
			allocation.RCA_AllocatedUQ = "CN";
			allocation.RCA_StartDate = start;
			allocation.RCA_ExpiryDate = expiry;
			allocation.RCA_AllowRelatedPorts = false;
			allocation.RCA_LoadLocation = load;
			allocation.RCA_DischargeLocation = discharge;
			return allocation;
		}

		#endregion

		#region Setup & Teardown

		protected override void SetUp()
		{
			disposables =
			[
				FreightConfigurationRegistry.Instance
					.EnableContainerWeightLimitSupportOnAllocationRoutes
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),

				FreightConfigurationRegistry.Instance
					.EnableCarrierContractAllocations
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),

				FreightDataRegistry.Instance
					.EnableCarrierAndClientContractModules
					.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
			];

			base.SetUp();

			var today = ZDate.Today;
			var startDate = today.AddDays(-5);
			var expiryDate = today.AddDays(5);
			const string loadPort = "AUSYD";
			const string dischargePort = "NZAKL";
			const string contractNumber = "MILLENIUM";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = contractNumber;
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;

			allocationRoute =
				CreateAllocationRoute(contract, "WARATAH", startDate, expiryDate, loadPort, dischargePort);
			secondaryAllocationRoute =
				CreateAllocationRoute(contract, "RSET", startDate, expiryDate, loadPort, dischargePort);

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_CarrierContractNumber = contractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var watRefContainer = Factory.NewWithValidTestData<RefContainer>();
			watRefContainer.RC_Code = "WAT";
			var whyRefContainer = Factory.NewWithValidTestData<RefContainer>();
			whyRefContainer.RC_Code = "WHY";

			containers =
			[
				CreateForwardingContainer("VSET", watRefContainer),
				CreateForwardingContainer("KSET", watRefContainer),
				CreateForwardingContainer("MARIYUNG", whyRefContainer),
				CreateForwardingContainer("TANGARA", whyRefContainer)
			];

			attachmentMock = new Mock<IAllocationContainerWeightLimitDialogsProvider>();
			consol.Factory.SetValue(() => attachmentMock.Object);

			allocationRouteContainerWeightLimitHelper = new AllocationRouteContainerWeightLimitHelper(consol);
		}

		protected override void TearDown()
		{
			base.TearDown();
			foreach (var disposable in disposables)
			{
				disposable.Dispose();
			}
		}

		RatingContractAllocationLine allocationRoute;
		RatingContractAllocationLine secondaryAllocationRoute;
		ForwardingConsol consol;
		Mock<IAllocationContainerWeightLimitDialogsProvider> attachmentMock;
		List<ForwardingContainer> containers;
		IDisposable[] disposables;
		AllocationRouteContainerWeightLimitHelper allocationRouteContainerWeightLimitHelper;

		#endregion
	}
}
