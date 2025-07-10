using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerType = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerType;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class JobSupplierBookingPlannedContainerDataObjectCollectionReaderTest : OrganizationAddressTestHelper
	{
		void AssertPlannedContainer(JobSupplierBookingPlannedContainer plannedContainer, string containerTypeCode, int containerCount, ZGuid? containerPK = null)
		{
			AssertEquals(containerTypeCode, plannedContainer.Container.RC_Code);
			AssertEquals(containerCount, plannedContainer.J1_ContainerCount);

			if (containerPK != null)
			{
				AssertEquals(containerPK.Value, plannedContainer.PK);
			}
		}

		public void TestAdd_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20GP", 2);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "40GP", 3);
		}

		public void TestDelete_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var plannedContainer = supplierBooking.PlannedContainers.AddNew();
			plannedContainer.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			plannedContainer.J1_ContainerCount = 5;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			AssertEquals(1, supplierBooking.PlannedContainers.Count);
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20GP", 2);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "40GP", 3);
		}

		public void TestUpdateWhileExactlySame_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var plannedContainer1 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer1.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			plannedContainer1.J1_ContainerCount = 2;
			var plannedContainer2 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer2.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			plannedContainer2.J1_ContainerCount = 3;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Assert(!supplierBooking.PlannedContainers.HasChanges);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20GP", 2, plannedContainer1.PK);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "40GP", 3, plannedContainer2.PK);
		}

		public void TestUpdateWhilePartiallySame_ContentIsComplete()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var plannedContainer1 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer1.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			plannedContainer1.J1_ContainerCount = 5;
			var plannedContainer2 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer2.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			plannedContainer2.J1_ContainerCount = 3;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Assert(supplierBooking.PlannedContainers.HasChanges);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "40GP", 3, plannedContainer2.PK);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "20GP", 2);
			Assert(!supplierBooking.PlannedContainers[1].IsInDatabase);
			Assert(plannedContainer1.IsDeleted);
		}

		public void TestAdd_ContentIsPartial()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Content = CollectionContent.Partial;
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20GP", 2);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "40GP", 3);
		}

		public void TestDelete_ContentIsPartial()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var plannedContainer = supplierBooking.PlannedContainers.AddNew();
			plannedContainer.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			plannedContainer.J1_ContainerCount = 5;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			AssertEquals(1, supplierBooking.PlannedContainers.Count);
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Content = CollectionContent.Partial;
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(3, supplierBooking.PlannedContainers.Count);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20FR", 5, plannedContainer.PK);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "20GP", 2);
			AssertPlannedContainer(supplierBooking.PlannedContainers[2], "40GP", 3);
		}

		public void TestUpdateWhileExactlySame_ContentIsPartial()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var plannedContainer1 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer1.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			plannedContainer1.J1_ContainerCount = 2;
			var plannedContainer2 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer2.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			plannedContainer2.J1_ContainerCount = 3;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Content = CollectionContent.Partial;
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Assert(!supplierBooking.PlannedContainers.HasChanges);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20GP", 2, plannedContainer1.PK);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "40GP", 3, plannedContainer2.PK);
		}

		public void TestUpdateWhilePartiallySame_ContentIsPartial()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var plannedContainer1 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer1.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			plannedContainer1.J1_ContainerCount = 5;
			var plannedContainer2 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer2.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			plannedContainer2.J1_ContainerCount = 3;
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Factory.SaveForTesting();

			var plannedContainerObjects = new DataObjectList<UniversalContainer>();
			plannedContainerObjects.Content = CollectionContent.Partial;
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 });
			plannedContainerObjects.Add(new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 });
			new JobSupplierBookingPlannedContainerDataObjectCollectionReader(supplierBooking, plannedContainerObjects, Logger, new UniversalObjectFactory()).ReadIntoCollection();

			AssertEquals(3, supplierBooking.PlannedContainers.Count);
			AssertPlannedContainer(supplierBooking.PlannedContainers[0], "20GP", 5, plannedContainer1.PK);
			AssertPlannedContainer(supplierBooking.PlannedContainers[1], "40GP", 3, plannedContainer2.PK);
			AssertPlannedContainer(supplierBooking.PlannedContainers[2], "20GP", 2);
		}
	}
}
