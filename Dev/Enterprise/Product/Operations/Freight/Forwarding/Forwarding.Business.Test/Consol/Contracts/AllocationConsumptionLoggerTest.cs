using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class AllocationConsumptionLoggerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CCAAllocationsFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CCAModules, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			ObjectFactory.Substitute(featureControlMock.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();

			ObjectFactory.DisposeSubstitutions();
		}

		public void TestAllocationAdded()
		{
			using (FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
				refContainer1.RC_TEU = 1;

				var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
				refContainer2.RC_TEU = 2;

				var container1 = AddContainer(consol, refContainer1, 3); // 3 TEU
				var container2 = AddContainer(consol, refContainer2, 1); // 1 CNT

				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var contract = Factory.NewWithValidTestData<RatingContract>();
				contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
				contract.RCT_OH = carrier.PK;

				var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute1.RCA_RCT_RatingContract = contract.PK;
				allocationRoute1.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.TwentyFootUnits;
				allocationRoute1.RCA_AllocatedQuantity = 100;

				var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute2.RCA_RCT_RatingContract = contract.PK;
				allocationRoute2.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.Containers;
				allocationRoute2.RCA_AllocatedQuantity = 100;

				container1.JC_RCA_AllocationLine = allocationRoute1.PK;
				container2.JC_RCA_AllocationLine = allocationRoute2.PK;

				Factory.Save();

				var allocatedLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code).ToArray();
				AssertEquals("2 allocated logs should have been created", allocatedLogs.Length, 2);

				var teuLog = allocatedLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == "TEU");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Status], "NEW");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute1.RCA_AllocationLineID);
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "3");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);

				var cntLog = allocatedLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == "CNT");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Status], "NEW");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute2.RCA_AllocationLineID);
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "1");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);
			}
		}

		public void TestAllocationRemoved()
		{
			using (FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
				refContainer1.RC_TEU = 1;

				var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
				refContainer2.RC_TEU = 2;

				var container1 = AddContainer(consol, refContainer1, 3); // 3 TEU
				var container2 = AddContainer(consol, refContainer2, 1); // 1 CNT

				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var contract = Factory.NewWithValidTestData<RatingContract>();
				contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
				contract.RCT_OH = carrier.PK;

				var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute1.RCA_RCT_RatingContract = contract.PK;
				allocationRoute1.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.TwentyFootUnits;
				allocationRoute1.RCA_AllocatedQuantity = 100;

				var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute2.RCA_RCT_RatingContract = contract.PK;
				allocationRoute2.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.Containers;
				allocationRoute2.RCA_AllocatedQuantity = 100;

				container1.JC_RCA_AllocationLine = allocationRoute1.PK;
				container2.JC_RCA_AllocationLine = allocationRoute2.PK;

				Factory.Save();

				container1.JC_RCA_AllocationLine = ZGuid.Empty;
				container2.JC_RCA_AllocationLine = ZGuid.Empty;

				Factory.Save();

				var deletedLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "DEL").ToArray();
				AssertEquals("2 deleted allocated logs should have been created", deletedLogs.Length, 2);

				var teuLog = deletedLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == "TEU");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Status], "DEL");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute1.RCA_AllocationLineID);
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "0");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);

				var cntLog = deletedLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == "CNT");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Status], "DEL");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute2.RCA_AllocationLineID);
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "0");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);
			}
		}

		public void TestAllocationConsumptionChanged()
		{
			using (FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
				refContainer1.RC_TEU = 1;

				var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
				refContainer2.RC_TEU = 2;

				var container1 = AddContainer(consol, refContainer1, 3); // 4 TEU
				var container2 = AddContainer(consol, refContainer2, 1); // 1 CNT

				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var contract = Factory.NewWithValidTestData<RatingContract>();
				contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
				contract.RCT_OH = carrier.PK;

				var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute1.RCA_RCT_RatingContract = contract.PK;
				allocationRoute1.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.TwentyFootUnits;
				allocationRoute1.RCA_AllocatedQuantity = 100;

				var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute2.RCA_RCT_RatingContract = contract.PK;
				allocationRoute2.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.Containers;
				allocationRoute2.RCA_AllocatedQuantity = 100;

				container1.JC_RCA_AllocationLine = allocationRoute1.PK;
				container2.JC_RCA_AllocationLine = allocationRoute2.PK;

				Factory.Save();

				container1.JC_ContainerCount = 4;
				container2.JC_ContainerCount = 2;

				Factory.Save();

				var amendedLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "AMD").ToArray();
				AssertEquals("2 amended allocated logs should have been created", amendedLogs.Length, 2);

				var teuLog = amendedLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == "TEU");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Status], "AMD");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute1.RCA_AllocationLineID);
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "4");
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(teuLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);

				var cntLog = amendedLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == "CNT");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Status], "AMD");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute2.RCA_AllocationLineID);
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "2");
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(cntLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);
			}
		}

		public void TestAllocationRouteChanged()
		{
			using (FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
				refContainer1.RC_TEU = 2;

				var container1 = AddContainer(consol, refContainer1, 3); // 6 TEU / 3 CNT

				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var contract = Factory.NewWithValidTestData<RatingContract>();
				contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
				contract.RCT_OH = carrier.PK;

				var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute1.RCA_RCT_RatingContract = contract.PK;
				allocationRoute1.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.TwentyFootUnits;
				allocationRoute1.RCA_AllocatedQuantity = 100;

				var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute2.RCA_RCT_RatingContract = contract.PK;
				allocationRoute2.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.Containers;
				allocationRoute2.RCA_AllocatedQuantity = 100;

				container1.JC_RCA_AllocationLine = allocationRoute1.PK;

				Factory.Save();

				var newLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "NEW").ToArray();
				AssertEquals("1 NEW log should have been created", newLogs.Length, 1);

				container1.JC_RCA_AllocationLine = allocationRoute2.PK;

				Factory.Save();

				newLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "NEW").ToArray();
				AssertEquals("2 NEW logs should have been created", newLogs.Length, 2);

				var deletedLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "DEL").ToArray();
				AssertEquals("1 DEL log should have been created", deletedLogs.Length, 1);
				var deletedLog = deletedLogs.First();

				var firstNewLog = newLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber] == allocationRoute1.RCA_AllocationLineID);
				var secondNewLog = newLogs.First(log => log.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber] == allocationRoute2.RCA_AllocationLineID);

				AssertEquals(firstNewLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "6");
				AssertEquals(firstNewLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(firstNewLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);

				AssertEquals(secondNewLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "3");
				AssertEquals(secondNewLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(secondNewLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);

				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute1.RCA_AllocationLineID);
				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "0");
				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);
			}
		}

		public void TestContainerRemoved()
		{
			using (FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
				refContainer1.RC_TEU = 2;

				var container1 = AddContainer(consol, refContainer1, 3); // 6 TEU / 3 CNT

				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var contract = Factory.NewWithValidTestData<RatingContract>();
				contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
				contract.RCT_OH = carrier.PK;

				var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute1.RCA_RCT_RatingContract = contract.PK;
				allocationRoute1.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.TwentyFootUnits;
				allocationRoute1.RCA_AllocatedQuantity = 100;

				var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute2.RCA_RCT_RatingContract = contract.PK;
				allocationRoute2.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.Containers;
				allocationRoute2.RCA_AllocatedQuantity = 100;

				container1.JC_RCA_AllocationLine = allocationRoute1.PK;

				Factory.Save();

				var newLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "NEW").ToArray();
				AssertEquals("1 NEW logs should have been created", newLogs.Length, 1);

				consol.Containers.RemoveAndDelete(container1);

				Factory.Save();

				var deletedLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "DEL").ToArray();
				AssertEquals("1 DEL log should have been created", deletedLogs.Length, 1);
				var deletedLog = deletedLogs.First();

				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute1.RCA_AllocationLineID);
				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "0");
				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(deletedLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);
			}
		}

		public void TestContainerAdded()
		{
			using (FreightConfigurationRegistry.Instance.EnableAllocatedEventGenerationOnConsolidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
				refContainer1.RC_TEU = 2;

				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var contract = Factory.NewWithValidTestData<RatingContract>();
				contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
				contract.RCT_OH = carrier.PK;

				var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				allocationRoute1.RCA_RCT_RatingContract = contract.PK;
				allocationRoute1.RCA_AllocatedUQ = Core.Constants.AllocationQuantityUnits.TwentyFootUnits;
				allocationRoute1.RCA_AllocatedQuantity = 100;

				consol.JK_RCA_AllocationLine = allocationRoute1.PK;

				Factory.Save();

				var container1 = AddContainer(consol, refContainer1, 3); // 6 TEU 

				AssertEquals("No allocated events yet", consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code).Count(), 0);

				Factory.Save();

				var newLogs = consol.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent == Events.Allocated.Code && log.Parameters[EventConstants.EventReferenceParameters.Codes.Status] == "NEW").ToArray();
				AssertEquals("1 NEW logs should have been created", newLogs.Length, 1);

				var newLog = newLogs.First();

				AssertEquals(newLog.Parameters[EventConstants.EventReferenceParameters.Codes.ReferenceNumber], allocationRoute1.RCA_AllocationLineID);
				AssertEquals(newLog.Parameters[EventConstants.EventReferenceParameters.Codes.Quantity], "6");
				AssertEquals(newLog.Parameters[EventConstants.EventReferenceParameters.Codes.ContractNumber], contract.RCT_ContractNumber);
				AssertEquals(newLog.Parameters[EventConstants.EventReferenceParameters.Codes.Organization], carrier.OH_Code);
			}
		}

		ForwardingContainer AddContainer(ForwardingConsol consol, RefContainer refContainer, short count)
		{
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.JC_ContainerCount = count;

			return container;
		}
	}
}
