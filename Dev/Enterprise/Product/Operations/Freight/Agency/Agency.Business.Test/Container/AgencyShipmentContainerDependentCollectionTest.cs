using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentContainerDependentCollection))]
	internal class AgencyShipmentContainerDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultingContainerModeForTopLevelPackMaster()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				var collection = new AgencyShipmentContainerDependentCollection(shipment, "FOO");
				string defaultContainerMode = Factory.New<AgencyShipmentContainer>().JC_ContainerMode;
				AssertEquals(defaultContainerMode, collection.AddNew().JC_ContainerMode);
				shipment.JS_PackingMode = mode;
				AssertEquals(mode, collection.AddNew().JC_ContainerMode);
				shipment.JS_PackingMode = "XXX";
				AssertEquals("Invalid container type should not be defaulted from invalid shipment", ZString.Empty, collection.AddNew().JC_ContainerMode);
				shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
				AssertEquals(Constants.ContainerModes.RollOnRollOff, collection.AddNew().JC_ContainerMode);
			}
		}

		public void TestDefaultingPackTypeForTopLevelPackMaster()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				var collection = new AgencyShipmentContainerDependentCollection(shipment, "FOO");
				shipment.JS_PackingMode = mode;
				shipment.JS_F3_NKPackType = "BOX";
				AssertEquals(shipment.IsRollOnRollOff ? "" : "BOX", collection.AddNew().JC_F3_NKPackType);
				shipment.JS_PackingMode = "XXX";
				AssertEquals("", collection.AddNew().JC_F3_NKPackType);
			}
		}

		public void TestSetDefaultsForNewChild_TotalUnitOfMeasureIsDefaultedFromRegistryForNonFCLBookings()
		{
			var booking = Factory.New<AgencyBooking>();
			var collection = new AgencyShipmentContainerDependentCollection(booking, "FOO");
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				booking.JS_PackingMode = mode;
				AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
				AssertEquals(Constants.Length.Inches, collection.AddNew().JC_TotalUnitOfMeasure);
				AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Miles);
				AssertEquals(Constants.Length.Miles, collection.AddNew().JC_TotalUnitOfMeasure);
			}

			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
			AssertEquals(Constants.Length.Feet, collection.AddNew().JC_TotalUnitOfMeasure);
		}

		public void TestSetDefaultsForNewChild_TotalUnitOfMeasureIsDefaultedFromRegistryForNonFCLBOLs()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var collection = new AgencyShipmentContainerDependentCollection(billOfLading, "FOO");
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				billOfLading.JS_PackingMode = mode;
				AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
				AssertEquals(Constants.Length.Inches, collection.AddNew().JC_TotalUnitOfMeasure);
				AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Miles);
				AssertEquals(Constants.Length.Miles, collection.AddNew().JC_TotalUnitOfMeasure);
			}

			billOfLading.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
			AssertEquals(Constants.Length.Feet, collection.AddNew().JC_TotalUnitOfMeasure);
		}

		public void TestDefaultingVolumeUnitForTopLevelPackMaster()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				var collection = new AgencyShipmentContainerDependentCollection(shipment, "FOO");
				shipment.JS_PackingMode = mode;
				AssertEquals(shipment.RegistryVolumeUnit, collection.AddNew().JC_GrossVolumeUQ);
			}
		}

		public void TestShipmentIsLoggedWhenContainerRemoved()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container1 = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			container1.JC_ContainerNum = "KIKI2233221";
			shipment.BookedContainers.Add(container1);
			Factory.Save();
			Func<string, ZQuery> getQuery = x =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, shipment.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EDT");
				query.AddToFilter(StmALogSchema.SL_Reference, string.Format("{0} DELETED", x));
				return query;
			};
			AssertNull(Factory.LoadTop1<StmALog>(getQuery("KIKI2233221")));
			shipment.BookedContainers.RemoveAndDeleteAll();
			AssertNotNull(Factory.LoadTop1<StmALog>(getQuery("KIKI2233221")));
			var container2 = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			container2.JC_ContainerNum = "KIKI3333331";
			shipment.BookedContainers.Add(container2);
			AssertNull(Factory.LoadTop1<StmALog>(getQuery("KIKI3333331")));
		}

		public void TestPurposefulRelationship()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainerDependentCollection p1Collection = new AgencyShipmentContainerDependentCollection(shipment, "P1");
			AgencyShipmentContainerDependentCollection p1Collection2 = new AgencyShipmentContainerDependentCollection(shipment, "P1");
			AgencyShipmentContainerDependentCollection p2Collection = new AgencyShipmentContainerDependentCollection(shipment, "P2");
			AgencyShipmentContainer p1Container = p1Collection.AddNew();
			AgencyShipmentContainer p1Container2 = p1Collection2.AddNew();
			AgencyShipmentContainer p2Container = p2Collection.AddNew();
			AssertEquals("p1Container", "P1", p1Container.JC_Purpose);
			AssertEquals("p1Container2", "P1", p1Container2.JC_Purpose);
			AssertEquals("p2Container", "P2", p2Container.JC_Purpose);
			p1Collection.Load();
			p1Collection2.Load();
			p2Collection.Load();
			AssertContainsExactElementsInAnyOrder("p1Collection", new AgencyShipmentContainer[] { p1Container, p1Container2 }, p1Collection.ToArray<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder("p1Collection2", new AgencyShipmentContainer[] { p1Container, p1Container2 }, p1Collection2.ToArray<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder("p2Collection", new AgencyShipmentContainer[] { p2Container }, p2Collection.ToArray<AgencyShipmentContainer>());
		}

		public void TestTestingCorrectCollection()
		{
			AssertType(typeof(AgencyShipmentContainerDependentCollection), GetCollectionToTest());
		}

		#region Updating Elements
		public void TestNotifyWeightChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyWeightChanged(0m, 1500m);
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("weight updated", 1500m, collection[0].JC_GrossWeight);
			collection.AddNew();
			collection.NotifyWeightChanged(1500m, 2000m);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("weight not updated", 1500m, collection[0].JC_GrossWeight);
			AssertEquals("weight not updated", 0m, collection[1].JC_GrossWeight);
		}

		public void TestNotifyWeightUQChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyWeightUQChanged(Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds);
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("unit of weight updated", Core.Constants.Weight.Pounds, collection[0].JC_GrossWeightUQ);
			collection.AddNew().JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			collection.NotifyWeightUQChanged(Core.Constants.Weight.Pounds, Core.Constants.Weight.Ounces);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("unit of weight updated", Core.Constants.Weight.Ounces, collection[0].JC_GrossWeightUQ);
			AssertEquals("unit of weight not updated", Core.Constants.Weight.Kilograms, collection[1].JC_GrossWeightUQ);
			collection[1].JC_GrossWeightUQ = Core.Constants.Weight.Ounces;
			collection.NotifyWeightUQChanged(Core.Constants.Weight.Ounces, Core.Constants.Weight.MetricCarat);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("unit of weight updated", Core.Constants.Weight.MetricCarat, collection[0].JC_GrossWeightUQ);
			AssertEquals("unit of weight updated", Core.Constants.Weight.MetricCarat, collection[0].JC_GrossWeightUQ);
		}

		public void TestNotifyVolumeChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyVolumeChanged(0m, 1500m);
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("volume updated", 1500m, collection[0].JC_GrossVolume);
			collection.AddNew();
			collection.NotifyVolumeChanged(1500m, 2000m);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("volume not updated", 1500m, collection[0].JC_GrossVolume);
			AssertEquals("volume not updated", 0m, collection[1].JC_GrossVolume);
		}

		public void TestNotifyVolumeUQChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyVolumeUQChanged(Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicFeet);
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("unit of volume updated", Core.Constants.Volume.CubicFeet, collection[0].JC_GrossVolumeUQ);
			collection.AddNew().JC_GrossVolumeUQ = Core.Constants.Volume.CubicInches;
			collection.NotifyVolumeUQChanged(Core.Constants.Volume.CubicFeet, Core.Constants.Volume.CubicYards);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("unit of volume updated", Core.Constants.Volume.CubicYards, collection[0].JC_GrossVolumeUQ);
			AssertEquals("unit of volume not updated", Core.Constants.Volume.CubicInches, collection[1].JC_GrossVolumeUQ);
			collection[1].JC_GrossVolumeUQ = Core.Constants.Volume.CubicYards;
			collection.NotifyVolumeUQChanged(Core.Constants.Volume.CubicYards, Core.Constants.Volume.CubicFeet);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("unit of volume updated", Core.Constants.Volume.CubicFeet, collection[0].JC_GrossVolumeUQ);
			AssertEquals("unit of volume updated", Core.Constants.Volume.CubicFeet, collection[0].JC_GrossVolumeUQ);
		}

		public void TestNotifyDescriptionChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyDescriptionChanged(string.Empty, "bbb");
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("description updated", "bbb", collection[0].JC_Description);
			collection.AddNew().JC_Description = "ccc";
			collection.NotifyDescriptionChanged("bbb", "ddd");
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("description updated", "ddd", collection[0].JC_Description);
			AssertEquals("description not updated", "ccc", collection[1].JC_Description);
			collection[1].JC_Description = "ddd";
			collection.NotifyDescriptionChanged("ddd", "eee");
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("description updated", "eee", collection[0].JC_Description);
			AssertEquals("description updated", "eee", collection[0].JC_Description);
		}

		public void TestNotifyPackageCountChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyPackageCountChanged(0, 3);
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("packs count updated", (short)3, collection[0].JC_ContainerCount);
			collection.AddNew();
			collection.NotifyPackageCountChanged(3, 5);
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("packs count not updated", (short)3, collection[0].JC_ContainerCount);
			AssertEquals("packs count not updated", (short)1, collection[1].JC_ContainerCount);
		}

		public void TestNotifyPackageTypeChanged()
		{
			var collection = (AgencyShipmentContainerDependentCollection)GetCollectionToTest();
			AssertEquals("collection has no elements", 0, collection.Count);
			collection.NotifyPackageTypeChanged("", "BOX");
			AssertEquals("element added", 1, collection.Count);
			AssertEquals("packs type updated", "BOX", collection[0].JC_F3_NKPackType);
			collection.AddNew();
			collection.NotifyPackageTypeChanged("BOX", "PLT");
			AssertEquals("no new elements added", 2, collection.Count);
			AssertEquals("packs type not updated", "BOX", collection[0].JC_F3_NKPackType);
			AssertEquals("packs type not updated", "", collection[1].JC_F3_NKPackType);
		}

		#endregion
		#region Container Change Tracking
		public void TestContainerChangeTracking_AddedUpdatedRemovedElements()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Real);
			var container = collection.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { container }, collection.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { container }, collection.ChangedElements);
			container.Delete();
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.ChangedElements);
			container = collection.AddNew();
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.ChangedElements);
			container.JC_ContainerNum = "ZZZ";
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.AddedElements);
			AssertContainsExactElementsInAnyOrder(new[] { container }, collection.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { container }, collection.ChangedElements);
			container.Delete();
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(new[] { container }, collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { container }, collection.ChangedElements);
		}

		public void TestContainerChangeTracking_MultipleAddAndRemovalsOfElement()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Real);
			for (int i = 0; i < 10; i++)
			{
				var container = Factory.New<AgencyShipmentContainer>();
				collection.Add(container);
				collection.Remove(container);
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.ChangedElements);
			Factory.Save();
			AssertNull("should have not log container changes", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded));
			AssertNull("should have not log container changes", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved));
		}

		public void TestContainerChangeTracking_UpdatedByDataRefreshBuss()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.BookedContainers.AddNew();
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var shipmentOnNewFactory = otherFactory.Load<AgencyShipment>(shipment.PK);
			AssertEquals("prerequisite", 1, shipmentOnNewFactory.BookedContainers.Count);
			shipmentOnNewFactory.BookedContainers.AddNew();
			otherFactory.Save();
			AssertEquals("prerequisite - collection refreshed", 2, shipment.BookedContainers.Count);
			var container = shipment.BookedContainers.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.BookedContainers.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), shipment.BookedContainers.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), shipment.BookedContainers.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.BookedContainers.ChangedElements);
		}

		public void TestDangerousGoodsChangeTracking_AddedUpdatedRemovedElements()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Real);
			ICollectionChangeTrackable<UNDGDataItem> dangerousGoodsTracking = collection;
			var container = collection.AddNew();
			var undg = container.UNDGs.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.ChangedElements);
			undg.Delete();
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.ChangedElements);
			undg = container.UNDGs.AddNew();
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.ChangedElements);
			undg.DI_TechnicalName = "ZEBRA FART";
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.ChangedElements);
			undg.Delete();
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.ChangedElements);
		}

		public void TestDangerousGoodsChangeTracking_AddedUpdatedRemovedElementsForMultipleContainers()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Real);
			ICollectionChangeTrackable<UNDGDataItem> dangerousGoodsTracking = collection;
			var container1 = collection.AddNew();
			var undg1 = container1.UNDGs.AddNew();
			var container2 = collection.AddNew();
			var undg2 = container2.UNDGs.AddNew();
			var undg3 = container2.UNDGs.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { undg1, undg2, undg3 }, dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg1, undg2, undg3 }, dangerousGoodsTracking.ChangedElements);
			undg2.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { undg1, undg3 }, dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg1, undg3 }, dangerousGoodsTracking.ChangedElements);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), collection.RemovedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.ChangedElements);
			undg3.DI_TechnicalName = "ZEBRA FART";
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg3 }, dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg3 }, dangerousGoodsTracking.ChangedElements);
			Factory.Save();
			undg1.Delete();
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg1 }, dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg1 }, dangerousGoodsTracking.ChangedElements);
		}

		public void TestResetChangeTrackingOnLoad()
		{
			var booking = Factory.New<AgencyBooking>();
			var collection = new AgencyShipmentContainerDependentCollection(booking, ContainerBookedStatus.Codes.Booked);
			var container = collection.AddNew();
			var undg = container.UNDGs.AddNew();
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			booking = otherFactory.Load<AgencyBooking>(booking.PK);
			collection = new AgencyShipmentContainerDependentCollection(booking, ContainerBookedStatus.Codes.Booked);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), collection.ChangedElements);
			AssertContainsExactElementsInAnyOrder(Array.Empty<UNDGDataItem>(), ((ICollectionChangeTrackable<UNDGDataItem>)collection).ChangedElements);
		}

		#endregion
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<AgencyShipment>().BookedContainers;
		}
		#endregion
	}
}
