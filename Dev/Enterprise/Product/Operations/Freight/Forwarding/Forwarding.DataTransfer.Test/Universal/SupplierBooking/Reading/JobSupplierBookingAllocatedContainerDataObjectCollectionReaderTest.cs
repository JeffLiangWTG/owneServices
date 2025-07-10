using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class JobSupplierBookingAllocatedContainerDataObjectCollectionReaderTest : OrganizationAddressTestHelper
	{
		ForwardingConsol consol1;
		ForwardingConsol consol2;
		ForwardingContainer container1;
		ForwardingContainer container2;
		ForwardingContainer container3;
		ForwardingContainer container4;
		SupplierBookingAllocatedContainerDataObjectWriter containerWriter;

		protected override void SetUp()
		{
			base.SetUp();

			consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "JK001";
			consol2.JK_UniqueConsignRef = "JK002";
			container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CON001";
			container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "CON002";
			container2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container3 = consol2.Containers.AddNew();
			container3.JC_ContainerNum = "CON003";
			container3.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container4 = consol2.Containers.AddNew();
			container4.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container4.JC_ContainerCount = 3;

			containerWriter = new SupplierBookingAllocatedContainerDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
		}

		public void TestAdd_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };

			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();

			AssertEquals(2, supplierBooking.Containers.Count);
			AssertEquals(container1.PK, supplierBooking.Containers[0].PK);
			AssertEquals(container4.PK, supplierBooking.Containers[1].PK);
		}

		public void TestDelete_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.Containers.Add(container2);
			Factory.SaveForTesting();
			AssertEquals(1, supplierBooking.Containers.Count);
			AssertEquals(container2.PK, supplierBooking.Containers[0].PK);

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };

			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();

			AssertEquals(2, supplierBooking.Containers.Count);
			AssertEquals(container1.PK, supplierBooking.Containers[0].PK);
			AssertEquals(container4.PK, supplierBooking.Containers[1].PK);
		}

		public void TestUpdateWhileExactlySame_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.Containers.Add(container1);
			supplierBooking.Containers.Add(container4);
			Factory.SaveForTesting();
			AssertEquals(2, supplierBooking.Containers.Count);
			AssertEquals(container1.PK, supplierBooking.Containers[0].PK);
			AssertEquals(container4.PK, supplierBooking.Containers[1].PK);

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };

			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();

			AssertEquals(2, supplierBooking.Containers.Count);
			Assert(!supplierBooking.HasChanges);
			AssertEquals(container1.PK, supplierBooking.Containers[0].PK);
			AssertEquals(container4.PK, supplierBooking.Containers[1].PK);
		}

		public void TestUpdateWhilePartiallySame_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.Containers.Add(container2);
			supplierBooking.Containers.Add(container4);
			Factory.SaveForTesting();
			AssertEquals(2, supplierBooking.Containers.Count);
			AssertEquals(container2.PK, supplierBooking.Containers[0].PK);
			AssertEquals(container4.PK, supplierBooking.Containers[1].PK);

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };

			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();

			AssertEquals(2, supplierBooking.Containers.Count);
			Assert(supplierBooking.HasChanges);
			AssertEquals(container4.PK, supplierBooking.Containers[0].PK);
			AssertEquals(container1.PK, supplierBooking.Containers[1].PK);
		}

		public void TestCommonCheck()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].DataContext.ClearDataSourceCollection();
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Consol Key is required.", Logger.GetWarnings());

			containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].DataContext.DataSourceCollection.Single().Key = ZString.Empty;
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Consol Key is required.", Logger.GetWarnings());

			containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].ContainerCollection.Add(new UniversalDataBuss.DataObjects.Universal.Container());
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Only one container should be provided JK001.", Logger.GetWarnings());

			containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].ContainerCollection.Clear();
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Only one container should be provided JK001.", Logger.GetWarnings());

			containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].DataContext.DataSourceCollection.Single().Key = "xxxx";
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Could not match the Consol xxxx.", Logger.GetWarnings());

			containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].ContainerCollection[0].ContainerNumber = "xxxx";
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Could not match the Container JK001 xxxx.", Logger.GetWarnings());

			containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			containerObjects[0].ContainerCollection[0].ContainerNumber = ZString.Empty;
			containerObjects[0].ContainerCollection[0].ContainerType.Code = "20FR";
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Could not match the Container JK001 20FR 1.", Logger.GetWarnings());
		}

		public void TestCheckWhileContainerIsNotAttachedToAny()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Successfully allocated  JK001 container CON001 to this supplier booking.", Logger.Logs);
			AssertContains("Successfully allocated  JK002 container 40GP 3 to this supplier booking.", Logger.Logs);
		}

		public void TestCheckWhileContainerIsAttachedToApprovedSupplierBooking()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var supplierBooking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking2.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;
			container1.JC_JSB_SupplierBooking = supplierBooking2.PK;
			container4.JC_JSB_SupplierBooking = supplierBooking2.PK;
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("JK001 container CON001 is already allocated to another supplier booking or a container load plan. Element is skipped.", Logger.GetWarnings());
			AssertContains("JK002 container 40GP 3 is already allocated to another supplier booking or a container load plan. Element is skipped.", Logger.GetWarnings());
		}

		public void TestCheckWhileContainerIsAttachedToCancelledSupplierBooking()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var supplierBooking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking2.JSB_Status = Core.Constants.SupplierBookingStatus.Cancelled;
			container1.JC_JSB_SupplierBooking = supplierBooking2.PK;
			container4.JC_JSB_SupplierBooking = supplierBooking2.PK;
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Successfully allocated  JK001 container CON001 to this supplier booking.", Logger.Logs);
			AssertContains("Successfully allocated  JK002 container 40GP 3 to this supplier booking.", Logger.Logs);
		}

		public void TestCheckWhileContainerIsAttachedToApprovedContainerLoadPlan()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Approved;
			container1.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			container4.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("JK001 container CON001 is already allocated to another supplier booking or a container load plan. Element is skipped.", Logger.GetWarnings());
			AssertContains("JK002 container 40GP 3 is already allocated to another supplier booking or a container load plan. Element is skipped.", Logger.GetWarnings());
		}

		public void TestCheckWhileContainerIsAttachedToCancelledContainerLoadPlan()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Cancelled;
			container1.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			container4.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(container1), containerWriter.GetDataObject(container4) };
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Successfully allocated  JK001 container CON001 to this supplier booking.", Logger.Logs);
			AssertContains("Successfully allocated  JK002 container 40GP 3 to this supplier booking.", Logger.Logs);
		}

		public void TestMatchPriority()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Approved;
			container4.JC_CLH_LoadListPlan = containerLoadPlan.PK;

			var containe5 = consol2.Containers.AddNew();
			containe5.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			containe5.JC_ContainerCount = 3;
			var supplierBooking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking2.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;
			containe5.JC_JSB_SupplierBooking = supplierBooking2.PK;

			var containe6 = consol2.Containers.AddNew();
			containe6.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			containe6.JC_ContainerCount = 3;

			Factory.SaveForTesting();

			var containerObjects = new DataObjectList<UniversalShipment> { containerWriter.GetDataObject(containe6) };
			Logger.ClearLogs();
			new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(supplierBooking, containerObjects, Logger).ReadIntoCollection();
			AssertContains("Successfully allocated  JK002 container 40GP 3 to this supplier booking.", Logger.Logs);
		}
	}
}
