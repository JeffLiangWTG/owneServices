using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackage))]
	public class PkgPackageTest : PackingBusinessObjectTestCase
	{
		public void TestPackagesDeleteWhenFetchHintsCountMoreThanMaximumParameterSupportsCount()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			for (int i = 0; i < 2101; i++)
			{
				package.Packages.AddNew();
			}
			Factory.Save();

			DataRegistry.Instance.ConcatenateMultipleFetchHintTypes = true;

			AssertNoExceptionThrown(() => package.Delete());
		}

		#region IsDataVersionsAutoLogged

		public void TestIsDataVersionsAutoLogged()
		{
			// PkgPackage is a shared/generic entity and some modules create millions of them.
			// Therefore do not create data version logs for this entity.
			var package = Factory.New<PkgPackage>();
			AssertEquals(false, package is IDataVersionLoggingSupported);
		}

		#endregion

		#region TestGetNewValidation

		public virtual void TestGetNewValidation()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(typeof(PkgPackageValidationForUnfinalisedPackageJob), package.Validation.GetType());

			Data.PackageJob.KJ_IsFinalized = true;
			AssertEquals(typeof(PkgPackageValidation), package.Validation.GetType());
		}

		#endregion

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicMetres);
			PackingRegistry.Instance.SetDimensionUnitForTest(Constants.Length.Metres);

			var outer1 = packageJob.Packages.AddNew();
			AssertEquals(1, outer1.KP_PackageQty);
			AssertEquals(Constants.Weight.Kilograms, outer1.KP_WeightUQ);
			AssertEquals(Constants.Volume.CubicMetres, outer1.KP_VolumeUQ);
			AssertEquals(Constants.Length.Metres, outer1.KP_DimensionUQ);

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Pounds);
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicFeet);
			PackingRegistry.Instance.SetDimensionUnitForTest(Constants.Length.Feet);

			var outer2 = packageJob.Packages.AddNew();
			AssertEquals(1, outer2.KP_PackageQty);
			AssertEquals(Constants.Weight.Pounds, outer2.KP_WeightUQ);
			AssertEquals(Constants.Volume.CubicFeet, outer2.KP_VolumeUQ);
			AssertEquals(Constants.Length.Feet, outer2.KP_DimensionUQ);

			var inner = outer2.Packages.AddNew();
			AssertEquals(1, inner.KP_PackageQty);
			AssertEquals(Constants.Weight.Pounds, inner.KP_WeightUQ);
			AssertEquals(Constants.Volume.CubicFeet, inner.KP_VolumeUQ);
			AssertEquals(Constants.Length.Feet, inner.KP_DimensionUQ);
		}

		#endregion

		#region MeasureUnit

		#region TestAllContainerProxyProperties

		public void TestAllContainerProxyProperties()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("CNT");
			var container = package.Container;
			package.KP_TareWeight = 1m;
			container.GoodsWeight = 2m;
			package.KP_DunnageWeight = 4m;

			AssertEquals("ContainerTareWeight should be 1", 1m, package.ContainerTareWeight);
			AssertEquals("ContainerGoodsWeight should be 2", 2m, package.ContainerGoodsWeight);
			AssertEquals("ContainerDunnageWeight should be 4", 4m, package.ContainerDunnageWeight);
		}

		#endregion

		#region TestGoodsWeight

		public void TestGoodsWeight()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_TareWeight = 1m;
			package.GoodsWeight = 2m;

			CombineAssertions(() =>
			{
				AssertEquals("KP_Weight should be 3", 3m, package.KP_Weight);
				AssertEquals("GoodsWeight should be 2", 2m, package.GoodsWeight);
			});
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestContainer

		public void TestContainer()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT");
			AssertNull(package.Container);

			package.KP_F3_NKPackType = "CNT";
			AssertNotNull(package.Container);
			AssertEquals(package.Container, package.Container);
			AssertEquals(true, package.IsRegisteredEditableChildObject(package.Container));

			// we need to FK to a refContainer to save
			package.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var otherPackage = otherFactory.Load<PkgPackage>(package.PK);
			AssertEquals("Should have loaded the existing Container.", package.Container.PK, otherPackage.Container.PK);
		}

		#endregion

		#region TestOuterPackage

		public void TestOuterPackage()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew();
			AssertEquals(container, container.OuterPackage);

			var pallet = container.Packages.AddNew();
			AssertEquals(container, pallet.OuterPackage);

			var box = pallet.Packages.AddNew();
			AssertEquals(container, box.OuterPackage);
		}

		#endregion

		#region TestPackageJob

		public void TestPackageJob()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew();
			AssertEquals(Data.PackageJob, pallet.PackageJob);

			var box = pallet.Packages.AddNew();
			AssertEquals(Data.PackageJob, box.PackageJob);

			AssertNull(Factory.New<PkgPackage>().PackageJob);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(package.Packages.Count, 0);
			AssertEquals(true, package.IsRegisteredEditableChildObject(package.Packages));

			var childPackage1 = package.Packages.AddNew();
			var childPackage2 = package.Packages.AddNew();
			var childChildPackage1 = childPackage1.Packages.AddNew(); // this should not be found in the collection
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { childPackage1, childPackage2 }, package.Packages);
		}

		public void TestGetAllPackages()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1a = container1.Packages.AddNew("PLT");
			var pallet1b = container1.Packages.AddNew("PLT");
			var box1a = pallet1a.Packages.AddNew("BOX");
			var ctn1a = box1a.Packages.AddNew("CTN");

			var container2 = Data.PackageJob.Packages.AddNew("CNT");

			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { pallet1a, pallet1b, box1a, ctn1a }, container1.GetAllPackages());
		}

		#endregion

		#region TestPackedItemDivots

		public void TestPackedItemDivots()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(package.PackedItemDivots.Count, 0);
			AssertEquals(true, package.IsRegisteredEditableChildObject(package.PackedItemDivots));

			package.Pack_ForTesting(Data.DummyLine1, 10m);
			package.Pack_ForTesting(Data.DummyLine2, 20m);

			var divot1 = package.PackedItemDivots.Single(d => d.GetPackableItemParent() == Data.DummyLine1);
			var divot2 = package.PackedItemDivots.Single(d => d.GetPackableItemParent() == Data.DummyLine2);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, package.PackedItemDivots);
		}

		#endregion

		#region TestPokePackageDataChangedEventShouldTriggerPackageDataChangedEvent

		public void TestPokePackageDataChangedEventShouldTriggerPackageDataChangedEvent()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();

			var eventPoked = false;
			Data.PackageJob.PackageDataChanged += (s, e) => eventPoked = true;

			package.PokePackageDataChangedEvent();

			AssertEquals("PokePackageDataChangedEvent should trigger the update for Package Data", true, eventPoked);
		}

		#endregion

		#region TestPackedItemDivots_UpdatedFromExternalSource

		public void TestPackedItemDivots_UpdatedFromExternalSource()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: Nothing is Packed.", 0, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Nothing is Packed.", 0, package.PackedItems.Count); // need to poke PackedItems collection to hook event handler for updating divot wrappers

			int packedItemDivotsCountChangedHitCount = 0;
			package.PackedItemDivotsCountChanged += (sender, e) => packedItemDivotsCountChangedHitCount++;
			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_PackedQty = 10m;
			divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
			divot.KI_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new[] { divot }, package.PackedItemDivots);
			AssertEquals("Should have fired the Packed Item Divots Count Changed for consumers to Hook onto.", 1, packedItemDivotsCountChangedHitCount);
		}

		#endregion

		#region TestPackedItemDivots_WhenPackingSameItemInTwoSeparateFactories

		public void TestPackedItemDivots_WhenPackingSameItemInTwoSeparateFactories()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			Factory.Save();

			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			AssertEquals("Precondition: Description is set.", "TV - Size: 63in", packedItem.Description);

			var newFactory = new BusinessObjectFactory();
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			var dummyLineInOtherFactory = newFactory.Load<DummyPackableItemParent>(((BusinessObject)Data.DummyLine1).PK);
			dummyLineInOtherFactory.Description = "TV";
			dummyLineInOtherFactory.DescriptionSupplement = "Size: 63in";
			dummyLineInOtherFactory.ZD1_Code = "attr1";
			dummyLineInOtherFactory.ZD1_Number = 1;

			var packedItemInOtherFactory = packageInNewFactory.Pack(dummyLineInOtherFactory, 100m).Single();
			AssertEquals("Precondition: Description is set.", "TV - Size: 63in", packedItemInOtherFactory.Description);
			AssertEquals("Should have only 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have only 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);

			Factory.Save(); // save first factory
			AssertEquals("Should still have only 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should still have only 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		#endregion

		#region TestPackedItemDivots_WhenPackingSameItemInTwoSeparateFactories_ThenUnpacking

		public void TestPackedItemDivots_WhenPackingSameItemInTwoSeparateFactories_ThenUnpacking()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			Factory.Save();

			Data.DummyLine1.WeightPerUnit = 0; // make sure package weight is not updated
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			AssertEquals("Precondition: Description is set.", "TV - Size: 63in", packedItem.Description);

			var newFactory = new BusinessObjectFactory();
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			var dummyLineInOtherFactory = newFactory.Load<DummyPackableItemParent>(((BusinessObject)Data.DummyLine1).PK);
			dummyLineInOtherFactory.Description = "TV";
			dummyLineInOtherFactory.DescriptionSupplement = "Size: 63in";
			dummyLineInOtherFactory.WeightPerUnit = 0; // make sure package weight is not updated
			dummyLineInOtherFactory.ZD1_Code = "attr1";
			dummyLineInOtherFactory.ZD1_Number = 1;

			var packedItemInOtherFactory = packageInNewFactory.Pack(dummyLineInOtherFactory, 100m).Single();
			AssertEquals("Precondition: Description is set.", "TV - Size: 63in", packedItemInOtherFactory.Description);
			AssertEquals("Should have only 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have only 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);

			Factory.Save(); // save first factory
			AssertEquals("Should still have only 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should still have only 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);
			AssertEquals("Has Changes should be true for the Package.", true, packageInNewFactory.HasChanges);

			packageInNewFactory.Unpack(packageInNewFactory.PackedItems[0], 100m);
			AssertEquals("Has Changes should still be true for the Package.", true, packageInNewFactory.HasChanges);
			AssertNoExceptionThrown(() => newFactory.Save());
			AssertEquals("First factory should now have no Divots Packed.", 0, package.PackedItemDivots.Count);
		}

		#endregion

		#region TestPkgPackage_TopHandlingUnitPackage

		public void TestPkgPackage_TopHandlingUnitPackage()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "3PL";
			var handlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			handlingUnitJob.KJ_ParentID = handlingUnit.PK;
			handlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var handlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			handlingUnitPackage.KP_KJ_ParentPackageJob = handlingUnitJob.PK;
			handlingUnitPackage.KP_PackageID = "HU1";
			Helper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			AssertEquals(handlingUnitPackage, package.TopHandlingUnitPackage);
		}

		public void TestPkgPackage_TopHandlingUnitPackage_HandlingUnitPackedInAnotherHandlingUnit()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "3PL";
			var handlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			handlingUnitJob.KJ_ParentID = handlingUnit.PK;
			handlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var handlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			handlingUnitPackage.KP_KJ_ParentPackageJob = handlingUnitJob.PK;
			handlingUnitPackage.KP_PackageID = "HU1";

			var topHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			topHandlingUnit.KPU_JobContext = "3PL";
			var topHandlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			topHandlingUnitJob.KJ_ParentID = topHandlingUnit.PK;
			topHandlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var topHandlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			topHandlingUnitPackage.KP_KJ_ParentPackageJob = topHandlingUnitJob.PK;
			topHandlingUnitPackage.KP_PackageID = "TopHU1";

			Helper.PackHandlingUnit(handlingUnitPackage, package, topHandlingUnitPackage);

			AssertEquals(topHandlingUnitPackage, package.TopHandlingUnitPackage);
		}

		#endregion

		#region TestPackedItemDivots_WhenUnpacking_PackedItemDeletedInAnotherFactory

		public void TestPackedItemDivots_WhenUnpackingInAnotherFactory_PackedItemDeleted()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var item1 = Data.DummyPackableItemOnLine1;
			var item2 = Data.DummyLine2.PackableItems.Single();
			package.Pack(item1, Data.DummyLine1);
			package.Pack(item2, Data.DummyLine2);
			AssertEquals("Should have 2 Divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			packageInNewFactory.KP_PackageID = "NEWID";
			var packedItemInNewFactory = packageInNewFactory.PackedItems[0];
			AssertEquals("Should have 2 Divots.", 2, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, packageInNewFactory.PackedItems.Count);

			((DummyPackableItem)item1).Delete();
			((DummyPackableItem)item2).Delete();
			Factory.Save(); // save first factory after delete

			package.Unpack(package.PackedItems[0], 100);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should have 1 Divot.", 1, package.PackedItemDivots.Count);
			AssertEquals("Should have 1 Divot Wrapper.", 1, package.PackedItems.Count);
			Assert("Package saved successfully.", !package.HasChanges);
			AssertEquals("Should have 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);

			AssertNoExceptionThrown(() => newFactory.Save());
			AssertEquals("Should have 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);
			Assert("Package in new factory saved successfully.", !packageInNewFactory.HasChanges);
		}

		public void TestPackedItemDivots_WhenUnpackingInSameFactory_PackedItemDeleted()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var item1 = Data.DummyPackableItemOnLine1;
			var item2 = Data.DummyLine2.PackableItems.Single();
			package.Pack(item1, Data.DummyLine1);
			package.Pack(item2, Data.DummyLine2);
			AssertEquals("Should have 2 Divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			var packedItemInNewFactory = packageInNewFactory.PackedItems[0];
			AssertEquals("Should have 2 Divots.", 2, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, packageInNewFactory.PackedItems.Count);

			((DummyPackableItem)item1).Delete();
			((DummyPackableItem)item2).Delete();
			Factory.Save(); // save first factory after delete

			packageInNewFactory.Unpack(packageInNewFactory.PackedItems[0], 100);
			AssertNoExceptionThrown(() => newFactory.Save());

			AssertEquals("Should have 1 Divot.", 1, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 1 Divot Wrapper.", 1, packageInNewFactory.PackedItems.Count);
			Assert("Package in new factory saved successfully.", !packageInNewFactory.HasChanges);
		}

		public void TestPackedItemDivots_WhenUnpackingInAnotherFactory_PackageJobDeleted()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packagePK = package.PK;
			var item1 = Data.DummyPackableItemOnLine1;
			var item2 = Data.DummyLine2.PackableItems.Single();
			package.Pack(item1, Data.DummyLine1);
			package.Pack(item2, Data.DummyLine2);
			AssertEquals("Should have 2 Divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			packageInNewFactory.KP_PackageID = "NEWID";
			var packageJobInNewFactory = packageInNewFactory.PackageJob;
			var packedItemInNewFactory = packageInNewFactory.PackedItems[0];
			AssertEquals("Should have 2 Divots.", 2, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, packageInNewFactory.PackedItems.Count);

			((DummyPackableItem)item1).Delete();
			Factory.Save(); // save first factory after delete

			package.Unpack(package.PackedItems[1], 100);
			var packageJobDeleteCalled = false;
			packageInNewFactory.PackedItems.ListChanged += (s, e) =>
			{
				if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset && !packageJobDeleteCalled)
				{
					packageJobDeleteCalled = true;
					packageJobInNewFactory.Delete(); // simulate job/package getting deleted while packing/unpacking
				}
			};

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNoExceptionThrown(() => newFactory.Save());

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = true };
			AssertNull("Package has been deleted.", newFactory2.Load<PkgPackage>(packagePK));
		}

		public void TestPackedItemDivots_WhenPackingInAnotherFactory_PackageJobDeleted()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packagePK = package.PK;
			var item1 = Data.DummyPackableItemOnLine1;
			var item2 = Data.DummyLine2.PackableItems.Single();
			package.Pack(item1, Data.DummyLine1);
			package.Pack(item2, Data.DummyLine2);
			AssertEquals("Should have 2 Divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			packageInNewFactory.KP_PackageID = "NEWID";
			var packageJobInNewFactory = packageInNewFactory.PackageJob;
			var packedItemInNewFactory = packageInNewFactory.PackedItems[0];
			AssertEquals("Should have 2 Divots.", 2, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, packageInNewFactory.PackedItems.Count);

			((DummyPackableItem)item1).Delete();
			Factory.Save(); // save first factory after delete

			var item3 = Data.DummyLine3.PackableItems.Single();
			package.Pack(item3, Data.DummyLine3);
			var packageJobDeleteCalled = false;
			packageInNewFactory.PackedItems.ListChanged += (s, e) =>
			{
				if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset && !packageJobDeleteCalled)
				{
					packageJobDeleteCalled = true;
					packageJobInNewFactory.Delete(); // simulate job/package getting deleted while packing/unpacking
				}
			};

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNoExceptionThrown(() => newFactory.Save());

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = true };
			AssertNull("Package has been deleted.", newFactory2.Load<PkgPackage>(packagePK));
		}

		public void TestPackedItemDivots_PackInDifferentFactory()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var item1 = Data.DummyPackableItemOnLine1;
			var item2 = Data.DummyLine2.PackableItems.Single();
			package.Pack(item1, Data.DummyLine1);
			AssertEquals("Should have 1 Divots.", 1, package.PackedItemDivots.Count);
			AssertEquals("Should have 1 Divot Wrappers.", 1, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			var dummyLine2InOtherFactory = newFactory.Load<DummyPackableItemParent>(((BusinessObject)Data.DummyLine2).PK);
			var item2InOtherFactory = dummyLine2InOtherFactory.PackableItems.Single();
			packageInNewFactory.Pack(item2InOtherFactory, dummyLine2InOtherFactory);
			AssertNoExceptionThrown(() =>
			{
				var items = packageInNewFactory.PackedItems;
			});

			AssertEquals("Should have 2 Divots.", 2, packageInNewFactory.PackedItemDivots.Count);
			AssertEquals("Should have 2 Divot Wrappers.", 2, packageInNewFactory.PackedItems.Count);
		}

		#endregion

		#region TestPackedItems

		public void TestPackedItems()
		{
			AssertEquals("Package with no PackageJob should not blow up.", 0, Factory.New<PkgPackage>().PackedItems.Count);

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Package should have nothing Packed.", 0, package.PackedItems.Count);

			var packedItems = package.Pack(Data.DummyLine1, 100m);
			AssertContainsExactElementsInAnyOrder(packedItems, package.PackedItems);

			// test Typed Enumeration
			foreach (var typedElement in package.PackedItems.Typed)
			{
				AssertCollectionContains(typedElement, packedItems);
			}

			var previousCount = package.PackedItems.Count;
			var itemDivot = package.PackedItemDivots.AddNew();
			AssertEquals("New Item Divot should have created a new Non-Persistent Divot.", previousCount + 1, package.PackedItems.Count);

			var emptyDivotWrapper = package.PackedItems.Typed.Single(d => d.PackedItems.All(p => p == null));
			AssertNull(emptyDivotWrapper.PackableItemParent);
			AssertEquals("Should be wrapping Correct Divot.", package, emptyDivotWrapper.ParentPackage);
			AssertEquals("Should be wrapping Correct Divot.", EmptyKey.Instance, emptyDivotWrapper.Key);
			AssertEquals("Should be wrapping Correct Divot.", PackableItemParentWrapper.ItemNoLongerExistsText, emptyDivotWrapper.Description);
		}

		public void TestPackedItems_BuiltAfterPacking()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Package should have nothing Packed.", 0, package.PackedItemDivots.Count);

			var packedItems = package.Pack(Data.DummyLine1, 100m);
			AssertEquals("Package should have One Item Packed.", 1, package.PackedItemDivots.Count);
			AssertContainsExactElementsInAnyOrder("Even if PackedItems collection is built after PackedItemDivots, it should be built using the divots created when Packing.", packedItems, package.PackedItems);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var packageInOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			AssertEquals("Package should have One Item Packed.", 1, packageInOtherFactory.PackedItems.Count);

			var packedItemInOtherFactory = packageInOtherFactory.PackedItems.Typed.Single();
			AssertEquals("Divot should have Correct PackingItem.", Data.DummyPackableItemOnLine1.PK, packedItemInOtherFactory.PackedItems.Single().PK);
			AssertEquals("Divot should have Correct PackingItem.", Data.DummyPackableItemOnLine1.Key, packedItemInOtherFactory.Key);
			AssertNotNull(packedItemInOtherFactory.PackableItemParent);
			AssertEquals("Divot should have Correct PackableItem.", ((BusinessObject)Data.DummyLine1).PK, ((BusinessObject)packedItemInOtherFactory.PackableItemParent).PK);
		}

		public void TestPackedItems_PackInDifferentFactory()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Package should have nothing Packed.", 0, package.PackedItemDivots.Count);

			var packedItem1 = package.Pack(Data.DummyLine1, 50m);
			AssertEquals("Package should have One Item Packed.", 1, package.PackedItemDivots.Count);
			AssertContainsExactElementsInAnyOrder(packedItem1, package.PackedItems);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var packageInOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			var dummyLine1InOtherFactory = otherFactory.Load<DummyPackableItemParent>(((BusinessObject)Data.DummyLine1).PK);
			var packedItem2 = packageInOtherFactory.Pack(dummyLine1InOtherFactory, 50m);

			AssertEquals("Package should have Two Item Packed.", 2, packageInOtherFactory.PackedItemDivots.Count);
			AssertNoExceptionThrown(() =>
			{
				var items = packageInOtherFactory.PackedItems;
			});
		}

		public void TestPackedItems_NoCorrespondingPackableItemParent()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			package.Pack(Data.DummyLine1, 100m);
			AssertEquals("Precondition: Package should have One Item Packed.", 1, package.PackedItems.Count);
			AssertEquals("Precondition: Package Qty is correct.", 100m, package.PackedItems[0].PackedQty);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var divotInOtherFactory = otherFactory.Load<PkgPackageItemDivot>(package.PackedItemDivots.Single().PK);
			var dummyItem = divotInOtherFactory.PackedItem;
			((DummyPackableItem)dummyItem).Key = new DummyGroupingKey(Data.DummyLine1);
			otherFactory.Load<DummyPackableItemParent>(((BusinessObject)Data.DummyLine1).PK).Delete();

			var packageInOtherFactory = divotInOtherFactory.ParentPackage;
			AssertEquals("Package should have One Item Packed.", 1, packageInOtherFactory.PackedItems.Count);
			AssertEquals("Packed Qty should not change.", 100m, packageInOtherFactory.PackedItems[0].PackedQty);
			AssertEquals("Description should show Item does not exist as no PackableItemParent can be found.", PackableItemParentWrapper.ItemNoLongerExistsText, packageInOtherFactory.PackedItems[0].Description);
			AssertNull("PackableItemParent should be null as no PackableItemParent can be found.", packageInOtherFactory.PackedItems[0].PackableItemParent);
			AssertEquals("Packed Item should not change.", dummyItem, packageInOtherFactory.PackedItems[0].PackedItems.Single());
		}

		public void TestPackedItems_PackageJobDeletedInAnotherFactory()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var item1 = Data.DummyPackableItemOnLine1;
			var item2 = Data.DummyLine2.PackableItems.Single();
			package.Pack(item1, Data.DummyLine1);
			package.Pack(item2, Data.DummyLine2);
			AssertEquals("Should have 2 Divot Wrappers.", 2, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);

			package.PackageJob.Delete();
			Factory.Save();

			Assert(packageInNewFactory.IsDeleted);
			AssertEquals(0, packageInNewFactory.PackedItems.Count);
		}

		#endregion

		#region TestPackageHandlingUnitDivots

		public void TestPackageHandlingUnitDivots()
		{
			Data.CreatePackingData();

			var innerPackage1 = Data.PackageJob.Packages.AddNew();
			var innerPackage2 = Data.PackageJob.Packages.AddNew();
			var handlingUnitPackage = Data.PackageJob.Packages.AddNew();
			AssertEquals(handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots.Count, 0);
			AssertEquals(false, handlingUnitPackage.IsRegisteredEditableChildObject(handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots));

			Helper.CreatePackageHandlingUnitDivot(handlingUnitPackage, innerPackage1);
			Helper.CreatePackageHandlingUnitDivot(handlingUnitPackage, innerPackage2);

			var divot1 = handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots.Single(d => d.KPD_KP_Package == innerPackage1.PK);
			var divot2 = handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots.Single(d => d.KPD_KP_Package == innerPackage2.PK);

			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots);
		}

		#endregion

		#region TestHandlingUnitPackedPackages

		public void TestHandlingUnitPackedPackages()
		{
			Data.CreatePackingData();

			var innerPackage1 = Data.PackageJob.Packages.AddNew();
			var innerPackage2 = Data.PackageJob.Packages.AddNew();
			var innerPackage3 = Data.PackageJob.Packages.AddNew();
			var handlingUnitPackage = Data.PackageJob.Packages.AddNew();
			AssertEquals(handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots.Count, 0);
			AssertEquals(false, handlingUnitPackage.IsRegisteredEditableChildObject(handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots));

			Helper.CreatePackageHandlingUnitDivot(handlingUnitPackage, innerPackage1);
			Helper.CreatePackageHandlingUnitDivot(handlingUnitPackage, innerPackage2);

			var unpackedDivot = Helper.CreatePackageHandlingUnitDivot(handlingUnitPackage, innerPackage3);
			unpackedDivot.KPD_UnpackedTime = ZDateTimeOffset.Now;
			unpackedDivot.KPD_GS_NKUnpackedUser = "BOB";

			AssertContainsExactElementsInAnyOrder("Should only return the packed Packages.", new[] { innerPackage1, innerPackage2 }, handlingUnitPackage.HandlingUnitPackedPackages);
		}

		#endregion

		#region TestScanEventsForBindingOnly

		public void TestScanEventsForBindingOnly()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT");
			AssertEquals(0, package.ScanEventsForBindingOnly.Count);

			var editEvent = package.Logs.AddNew(Events.Delivered);
			AssertEquals(editEvent, package.ScanEventsForBindingOnly.Single());
		}

		#endregion

		#region TestPackageSeals

		public void TestPackageSeals()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(package.PackageSeals.Count, 0);
			AssertEquals(false, package.IsRegisteredEditableChildObject(package.PackageSeals));

			Helper.CreatePackageSeal(package, "Test Seal 1");
			Helper.CreatePackageSeal(package, "Test Seal 2", false);

			var seal1 = package.PackageSeals.Single(d => d.KPE_IsSealOK);
			var seal2 = package.PackageSeals.Single(d => !d.KPE_IsSealOK);

			AssertContainsExactElementsInAnyOrder(new[] { seal1, seal2 }, package.PackageSeals);
		}

		#endregion

		#region TestJobServiceLinks

		public void TestJobServiceLinks()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(package.JobServiceLinks.Count, 0);
			var jobServiceLink1 = Factory.NewWithValidTestData<JobServiceLink>();
			jobServiceLink1.ESL_ParentID = package.PK;
			jobServiceLink1.ESL_Quantity = 1;
			jobServiceLink1.ESL_ParentTableCode = "KP";
			var jobServiceLink2 = Factory.NewWithValidTestData<JobServiceLink>();
			jobServiceLink2.ESL_ParentID = package.PK;
			jobServiceLink2.ESL_Quantity = 2;
			jobServiceLink2.ESL_ParentTableCode = "KP";

			var links = package.JobServiceLinks;
			AssertNotNull(links);
			AssertEquals("Should have 2 links", 2, links.Count);

			AssertEquals("Method should match", 1, links.Single(s => s.PK == jobServiceLink1.PK).ESL_Quantity);
			AssertEquals("Method should match", 2, links.Single(s => s.PK == jobServiceLink2.PK).ESL_Quantity);
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region TestKP_WeightUQ

		public void TestKP_WeightUQ()
		{
			Data.CreatePackingData();

			var parentPackage = Data.PackageJob.Packages.AddNew();
			var childPackage = parentPackage.Packages.AddNew();
			parentPackage.KP_WeightUQ = "XX"; // Setup invalid Weight Unit
			AssertNoExceptionThrown(() => childPackage.KP_Weight = 10m);

			parentPackage.KP_WeightUQ = Constants.Weight.Kilograms;
			childPackage.KP_Weight = 10m;
			AssertNoExceptionThrown(() => childPackage.KP_WeightUQ = "XX");
		}

		public void TestKP_WeightUQ_CannotSaveWithInvalidEntry()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_WeightUQ = "KG";
			AssertNoExceptionThrown(() => Factory.Save());

			package.KP_WeightUQ = "AA";
			AssertExceptionThrown<ZSaveException>("Exception should be thrown when trying to save package with invalid WeightUQ", () => Factory.Save());
		}

		#region TestKP_WeightUQ_ConstraintSynchronisedWithLookup

		public void TestKP_WeightUQ_ConstraintSynchronisedWithLookup() => TestConstraintSynchronisedWithLookup(PkgPackageSchema.Constants.KP_WeightUQ, pkgl => pkgl.WeightUQs);

		void TestConstraintSynchronisedWithLookup(string columnName, Func<PkgPackageLookups, CodeDescriptionPairList> getLookupList)
		{
			var codesOnLookup = getLookupList(Factory.New<PkgPackage>().Lookups).GetAllCodes();
			var codesOnConstraint = TestDbObjectHelper.GetInFiltersFromConstraint(columnName);

			AssertContainsExactElementsInAnyOrder(codesOnLookup, codesOnConstraint);
		}

		#endregion

		#endregion

		#region TestKP_VolumeUQ_CannotSaveWithInvalidEntry

		public void TestKP_VolumeUQ_CannotSaveWithInvalidEntry()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_VolumeUQ = "M3";
			AssertNoExceptionThrown(() => Factory.Save());

			package.KP_VolumeUQ = "AA";
			AssertExceptionThrown<ZSaveException>("Exception should be thrown when trying to save package with invalid VolumeUQ", () => Factory.Save());
		}

		#endregion

		#region TestKP_VolumeUQ_ConstraintSynchronisedWithLookup

		public void TestKP_VolumeUQ_ConstraintSynchronisedWithLookup() => TestConstraintSynchronisedWithLookup(PkgPackageSchema.Constants.KP_VolumeUQ, pkgl => pkgl.VolumeUQs);

		#endregion

		#region TestKP_DimensionUQ

		public void TestKP_DimensionUQ_CannotSaveWithInvalidEntry()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_DimensionUQ = "MM";
			AssertNoExceptionThrown(() => Factory.Save());

			package.KP_DimensionUQ = "AA";
			AssertExceptionThrown<ZSaveException>("Exception should be thrown when trying to save package with invalid DimensionUQ", () => Factory.Save());
		}

		#endregion

		#region TestKP_ClosedTimeUtc

		public void TestKP_ClosedTimeUtc()
		{
			Data.CreatePackingData();

			var outer = Data.PackageJob.Packages.AddNew();
			var inner = outer.Packages.AddNew();
			var innerInner = inner.Packages.AddNew();
			AssertEquals("Precondition", false, outer.IsClosed);
			AssertEquals("Precondition", true, outer.KP_GS_NKClosedBy.IsEmpty);
			AssertEquals("Precondition", false, inner.IsClosed);
			AssertEquals("Precondition", true, inner.KP_GS_NKClosedBy.IsEmpty);
			AssertEquals("Precondition", false, innerInner.IsClosed);
			AssertEquals("Precondition", true, innerInner.KP_GS_NKClosedBy.IsEmpty);

			innerInner.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Closing an inner should *not* close its parents.", false, outer.IsClosed);
			AssertEquals("Closing an inner should *not* close its parents.", false, inner.IsClosed);
			AssertEquals(true, innerInner.IsClosed);
			AssertEquals(false, innerInner.KP_GS_NKClosedBy.IsEmpty);

			outer.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			inner.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Precondition", true, outer.IsClosed);
			AssertEquals("Precondition", false, outer.KP_GS_NKClosedBy.IsEmpty);
			AssertEquals("Precondition", true, inner.IsClosed);
			AssertEquals("Precondition", false, inner.KP_GS_NKClosedBy.IsEmpty);
			AssertEquals("Precondition", true, innerInner.IsClosed);
			AssertEquals("Precondition", false, innerInner.KP_GS_NKClosedBy.IsEmpty);

			innerInner.KP_ClosedTimeUtc = ZDateTime.Empty;
			AssertEquals("Opening an inner should open all parents.", false, outer.IsClosed);
			AssertEquals("Opening an inner should open all parents.", true, outer.KP_GS_NKClosedBy.IsEmpty);
			AssertEquals("Opening an inner should open all parents.", false, inner.IsClosed);
			AssertEquals("Opening an inner should open all parents.", true, inner.KP_GS_NKClosedBy.IsEmpty);
			AssertEquals(false, innerInner.IsClosed);
			AssertEquals(true, innerInner.KP_GS_NKClosedBy.IsEmpty);
		}

		public void TestKP_ClosedTimeUtcCreatesPackingCompleteEvent()
		{
			Data.CreatePackingData();

			var outerNonContainerPackage = Data.PackageJob.Packages.AddNew();
			AssertEquals("There is no PKC (Packing Complete) event on package.", false, outerNonContainerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			var innerPackage = outerNonContainerPackage.Packages.AddNew();
			AssertEquals("There is no PKC (Packing Complete) event on package.", false, innerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			var outerContainerPackage = Data.PackageJob.Packages.AddNew("CNT");
			AssertEquals("There is no PKC (Packing Complete) event on package.", false, outerContainerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			outerNonContainerPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("PKC (Packing Complete) event exists on package.", true, outerNonContainerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			innerPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Still NO PKC (Packing Complete) event exists on package.", false, innerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			outerContainerPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Still NO PKC (Packing Complete) event exists on package.", false, outerContainerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());
		}

		public void TestKP_ClosedTimeUtcCreatesPackingCompleteEvent_HasPrinterName()
		{
			Data.CreatePackingData();

			var outerPackage1 = Data.PackageJob.Packages.AddNew();
			outerPackage1.PrinterUsedToPrintLabel = "TestPrinter";
			outerPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			var pkcEvent1 = outerPackage1.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).First();
			AssertEquals("Event SL_Ref is correct.", "Packing Completed|EQN=TestPrinter", pkcEvent1.SL_Reference);

			var outerPackage2 = Data.PackageJob.Packages.AddNew();
			AssertEquals("Pre-condition: PrinterUsedToPrintLabel is empty.", true, string.IsNullOrEmpty(outerPackage2.PrinterUsedToPrintLabel));
			outerPackage2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			var pkcEvent2 = outerPackage2.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).First();
			AssertEquals("Event SL_Ref is correct.", "Packing Completed", pkcEvent2.SL_Reference);
		}

		public void TestKP_ClosedAuditColumnsReadOnly()
		{
			var package = Factory.New<PkgPackage>();
			Assert("Closed Audit Columns should be readonly.", package.KP_ClosedTimeUtcInfo.ReadOnly);
			Assert("Closed Audit Columns should be readonly.", package.KP_GS_NKClosedByInfo.ReadOnly);
			Assert("Closed Audit Columns should be readonly.", package.KP_IsClosedInfo.ReadOnly);
		}

		#endregion

		#region TestKP_IsHeld

		public void TestKP_IsHeld()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			AssertEquals("It should start without hold", false, package.KP_IsHeld);
			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			package.KP_IsHeld = true;
			Assert("It should be held now", package.KP_IsHeld);
			Assert("The package is unreleased when it's hold", !package.IsReleased);

			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			package.KP_IsHeld = false;
			Assert("It should be unheld now", !package.KP_IsHeld);
			Assert("The package should not been unreleased", package.IsReleased);
		}

		#endregion

		#region TestKP_IsHeldCreatesLogWhenClearingHold

		public void TestKP_IsHeldCreatesLogWhenClearingHold()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var parentJob = Data.PackageJob.ParentJob as IStmALogParent;

			package.KP_IsHeld = true;
			package.KP_PackageID = "ID001";
			Assert("It should be held now", package.KP_IsHeld);
			Assert("There should not be ClearedHold events before changing the property", !parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ClearedHold.Code).Any());
			package.KP_IsHeld = false;
			AssertEquals("Package is no longer held|RFN=ID001", parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ClearedHold.Code).First().SL_Reference);
		}

		#endregion

		#region TestKP_ReleasedTimeUtc

		public void TestKP_ReleasedTimeUtc()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");

			// Setting to true
			AssertEquals("Precondition", false, package.KP_IsReleasedViaJob);
			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Package should be released.", true, package.IsReleased);
			AssertEquals("Package should be released.", false, package.KP_GS_NKReleasedBy.IsEmpty);
			AssertEquals("Setting package to released should not update KP_IsReleasedViaJob", false, package.KP_IsReleasedViaJob);

			// Setting to false
			package.KP_IsReleasedViaJob = true;
			package.KP_ReleasedTimeUtc = ZDateTime.Empty;
			AssertEquals("Package should not be released", false, package.IsReleased);
			AssertEquals("Package should not be released", true, package.KP_GS_NKReleasedBy.IsEmpty);
			AssertEquals("Setting package to un-released should also set KP_IsReleasedViaJob to false", false, package.KP_IsReleasedViaJob);
		}

		public void TestKP_ReleasedAuditColumnsReadOnly()
		{
			var package = Factory.New<PkgPackage>();
			Assert("Closed Audit Columns should be readonly.", package.KP_ReleasedTimeUtcInfo.ReadOnly);
			Assert("Closed Audit Columns should be readonly.", package.KP_GS_NKReleasedByInfo.ReadOnly);
			Assert("Closed Audit Columns should be readonly.", package.KP_IsReleasedInfo.ReadOnly);
		}

		#endregion

		#region TestKP_F3_NKPackType

		#region TestKP_F3_NKPackType_UpdatesDIMs

		public void TestKP_F3_NKPackType_UpdatesDIMs()
		{
			var package = (PkgPackage)GetNewBusinessObject();
			var standardPackTypes = new RefPackTypeCollection(Factory, false);

			var somePackTypeCode = IsContainerTypeValid ? "CNT" : "KEG";
			var somePackType = standardPackTypes.Single(pack => pack.F3_Code == somePackTypeCode);
			somePackType.F3_Length = 1m;
			somePackType.F3_Width = 2m;
			somePackType.F3_Height = 3m;
			somePackType.F3_Weight = 4m;
			somePackType.F3_UnitOfDimension = "M";
			somePackType.F3_UnitOfWeight = "KG";

			var pLT_PackType = standardPackTypes.Single(pack => pack.F3_Code == "PLT");
			pLT_PackType.F3_Length = 5m;
			pLT_PackType.F3_Width = 6m;
			pLT_PackType.F3_Height = 7m;
			pLT_PackType.F3_Weight = 8m;
			pLT_PackType.F3_UnitOfDimension = "cm";
			pLT_PackType.F3_UnitOfWeight = "gm";

			var bOX_PackType = standardPackTypes.Single(pack => pack.F3_Code == "BOX");
			bOX_PackType.F3_UnitOfDimension = "";
			bOX_PackType.F3_UnitOfWeight = "";

			package.KP_F3_NKPackType = somePackTypeCode;
			AssertEquals(1m, package.KP_Length);
			AssertEquals(2m, package.KP_Width);
			AssertEquals(3m, package.KP_Height);
			AssertEquals(4m, package.KP_Weight);
			AssertEquals(IsTareWeightDefaulted ? 4m : 0m, package.KP_TareWeight);
			AssertEquals("M", package.KP_DimensionUQ);
			AssertEquals("KG", package.KP_WeightUQ);

			ResetPackageIfNeeded(package);

			package.KP_F3_NKPackType = "PLT";
			AssertEquals(5m, package.KP_Length);
			AssertEquals(6m, package.KP_Width);
			AssertEquals(7m, package.KP_Height);
			AssertEquals(8m, package.KP_Weight);
			AssertEquals(IsTareWeightDefaulted ? 8m : 0m, package.KP_TareWeight);
			AssertEquals("cm", package.KP_DimensionUQ);
			AssertEquals("gm", package.KP_WeightUQ);

			package.KP_F3_NKPackType = "xXx";
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", 5m, package.KP_Length);
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", 6m, package.KP_Width);
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", 7m, package.KP_Height);
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", 8m, package.KP_Weight);
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", IsTareWeightDefaulted ? 8m : 0m, package.KP_TareWeight);
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", "cm", package.KP_DimensionUQ);
			AssertEquals("Values should not be recalculated when setting an invalid PackType.", "gm", package.KP_WeightUQ);

			ResetPackageIfNeeded(package);

			package.KP_F3_NKPackType = "BOX";
			AssertEquals(0m, package.KP_Length);
			AssertEquals(0m, package.KP_Width);
			AssertEquals(0m, package.KP_Height);
			AssertEquals(0m, package.KP_Weight);
			AssertEquals(0m, package.KP_TareWeight);
			AssertEquals("DimensionUQ should not be defaulted if the RefPackType has no value.", "cm", package.KP_DimensionUQ);
			AssertEquals("WeightUQ should not be defaulted if the RefPackType has no value.", "gm", package.KP_WeightUQ);
		}

		protected virtual void ResetPackageIfNeeded(PkgPackage package)
		{
		}

		protected virtual bool IsContainerTypeValid => true;
		protected virtual bool IsTareWeightDefaulted => true;

		#endregion

		#region TestKP_F3_NKPackType_SetLastUsedPackTypes

		public void TestKP_F3_NKPackType_SetLastUsedPackTypes()
		{
			PackingRegistry.Instance.SetOuterPackageUnitForTest(Constants.PkgUnit.Pallet);
			PackingRegistry.Instance.SetInnerPackageUnitForTest(Constants.PkgUnit.Box);

			var packageJob = Factory.New<PkgPackageJob>();
			var outer = packageJob.Packages.AddNew();
			var inner = outer.Packages.AddNew();

			AssertEquals("Precondition", Constants.PkgUnit.Pallet, packageJob.LastUsedOuterPackType);
			AssertEquals("Precondition", Constants.PkgUnit.Box, packageJob.LastUsedInnerPackType);

			// outer
			outer.KP_F3_NKPackType = Constants.PkgUnit.Container;
			AssertEquals(Constants.PkgUnit.Container, packageJob.LastUsedOuterPackType);
			AssertEquals("BOX", packageJob.LastUsedInnerPackType);

			// inner
			inner.KP_F3_NKPackType = "KEG";
			AssertEquals(Constants.PkgUnit.Container, packageJob.LastUsedOuterPackType);
			AssertEquals(Constants.PkgUnit.Keg, packageJob.LastUsedInnerPackType);

			// invalid pack type
			outer.KP_F3_NKPackType = "xXx";
			inner.KP_F3_NKPackType = "xXx";
			AssertEquals("Should not set an invalid Pack Type as the Last Used Pack Type.", Constants.PkgUnit.Container, packageJob.LastUsedOuterPackType);
			AssertEquals("Should not set an invalid Pack Type as the Last Used Pack Type.", Constants.PkgUnit.Keg, packageJob.LastUsedInnerPackType);

			// emptypack type
			outer.KP_F3_NKPackType = "";
			inner.KP_F3_NKPackType = "";
			AssertEquals("Should not set an empty Pack Type as the Last Used Pack Type.", Constants.PkgUnit.Container, packageJob.LastUsedOuterPackType);
			AssertEquals("Should not set an empty Pack Type as the Last Used Pack Type.", Constants.PkgUnit.Keg, packageJob.LastUsedInnerPackType);
		}

		#endregion

		#region TestKP_F3_NKPackType_DeletesUnusedContainer

		public void TestKP_F3_NKPackType_DeletesUnusedContainer()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";
			var container = package.Container;

			package.KP_F3_NKPackType = "PLT";
			AssertNull(package.Container);
			AssertEquals(true, container.IsDeleted);
		}

		#endregion

		#region TestKP_F3_NKPackType_ChangeCanBeCanceled

		public void TestKP_F3_NKPackType_ChangeCanBeCanceled()
		{
			int changeFromContainerCancelledCount = 0;
			int packTypeChangingCount = 0;
			bool continuePackTypeChange = false;

			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";

			package.PackTypeChangingFromContainerCancelled += (sender, e) =>
			{
				changeFromContainerCancelledCount++;
			};

			package.PackTypeChangingFromContainerWithData += (sender, e) =>
			{
				packTypeChangingCount++;
				e.Continue = continuePackTypeChange;
			};

			// test when container has changes

			package.Container.K0_ContainerMode = "123";
			package.KP_F3_NKPackType = "PLT";
			AssertEquals("Event should have fired.", 1, packTypeChangingCount);
			AssertEquals("Change from Container was cancelled, cancel event should fire.", 1, changeFromContainerCancelledCount);
			AssertEquals("The PackType should not have changed.", "CNT", package.KP_F3_NKPackType);
			AssertEquals("The Container should not have changed.", "123", package.Container.K0_ContainerMode);

			// test when container has no changes

			package.Container.K0_ContainerMode = "";
			package.KP_F3_NKPackType = "PLT";
			AssertEquals("The Container had all default values, event should not have fired.", 1, packTypeChangingCount);
			AssertEquals("Change from Container was not cancelled, cancel event should not fire.", 1, changeFromContainerCancelledCount);
			AssertEquals("The Container had all default values, the PackType should have changed.", "PLT", package.KP_F3_NKPackType);

			package.KP_F3_NKPackType = "CNT";
			package.Container.K0_ContainerMode = "123";
			continuePackTypeChange = true;
			package.KP_F3_NKPackType = "PLT";
			AssertEquals("Event should have fired.", 2, packTypeChangingCount);
			AssertEquals("Change from Container was not cancelled, cancel event should not fire.", 1, changeFromContainerCancelledCount);
		}

		#endregion

		#region TestKP_F3_NKPackType_ValidatesPackageID

		public void TestKP_F3_NKPackType_ValidatesPackageID()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "1234567890123";
			AssertNoErrors("Precondition", package.KP_F3_NKPackTypeInfo);

			package.KP_F3_NKPackType = "CNT";  // containers only allow 12 chars
			AssertHasErrors(package.KP_PackageIDInfo);
		}

		#endregion

		#region TestKP_F3_NKPackType_DoesNoWorkDuringClone

		public void TestKP_F3_NKPackType_DoesNoWorkDuringClone()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";

			bool workDoneDuringClone = false;
			package.PackTypeChangingFromContainer += delegate
			{ workDoneDuringClone = true; };
			package.PackTypeChangingFromContainerCancelled += delegate
			{ workDoneDuringClone = true; };
			package.PackTypeChangedToContainer += delegate
			{ workDoneDuringClone = true; };

			package.KP_F3_NKPackType = "PLT";
			AssertEquals("Precondition", true, workDoneDuringClone);
			package.KP_F3_NKPackType = "CNT"; //
			workDoneDuringClone = false;      // cleanup

			((IBusinessObjectInternals)package).IsCopying = true;
			package.KP_F3_NKPackType = "PLT";
			AssertEquals(false, workDoneDuringClone);
		}

		#endregion

		#region TestPackTypeChangingFromContainer

		public void TestPackTypeChangingFromContainer()
		{
			int changeFromContainerEventFiredCount = 0;

			var package = Factory.New<PkgPackage>();
			package.PackTypeChangingFromContainer += delegate
			{
				changeFromContainerEventFiredCount++;
			};

			package.KP_F3_NKPackType = "PLT";
			AssertEquals(0, changeFromContainerEventFiredCount);

			package.KP_F3_NKPackType = "CNT";
			AssertEquals(0, changeFromContainerEventFiredCount);

			package.KP_F3_NKPackType = "BOX";
			AssertEquals(1, changeFromContainerEventFiredCount);
		}

		#endregion

		#region TestPackTypeChangingToContainer

		public void TestPackTypeChangingToContainer()
		{
			int changeToContainerEventFiredCount = 0;

			var package = Factory.New<PkgPackage>();
			package.PackTypeChangedToContainer += delegate
			{
				changeToContainerEventFiredCount++;
			};

			package.KP_F3_NKPackType = "PLT";
			AssertEquals(0, changeToContainerEventFiredCount);

			package.KP_F3_NKPackType = "CNT";
			AssertEquals(1, changeToContainerEventFiredCount);

			package.KP_F3_NKPackType = "BOX";
			AssertEquals(1, changeToContainerEventFiredCount);
		}

		#endregion

		#endregion

		#region TestKP_ItemRemovedEvent

		public void TestKP_ItemRemovedEvent_SingleOpenPkg_NoLog()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var parentJob = Data.PackageJob.ParentJob as IStmALogParent;
			Factory.Save();

			AssertEquals("Precondition: Pkg is outer pkg", true, package.IsOuter);
			AssertEquals("Precondition: Pkg is not closed", false, package.IsClosed);

			package.Delete();
			AssertEquals("There should not be any Item Removed events before closing the pkg", false, parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ItemRemoved.Code).Any());
		}

		public void TestKP_ItemRemovedEvent_SingleClosedPkg_OneLog()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var parentJob = Data.PackageJob.ParentJob as IStmALogParent;
			package.KP_PackageID = "ID001";
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition: Pkg is outer pkg", true, package.IsOuter);
			AssertEquals("Precondition: Pkg is a Pallet", "PLT", package.KP_F3_NKPackType);
			AssertEquals("Precondition: Pkg is closed", true, package.IsClosed);

			package.KP_ClosedTimeUtc = ZDateTime.Empty; //value changed in factory still closed in db.
			package.Delete();
			var logEvent = parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ItemRemoved.Code).First();
			CombineAssertions(() =>
			{
				AssertEquals("Event SL_Ref is correct.", "|RFN=ID001|TYP=Pallet", logEvent.SL_Reference);
				AssertEquals("Event Display is correct.", "Pallet Item Removed Ref No: ID001", logEvent.DisplayEventReference.Trim());
			});
		}

		public void TestKP_ItemRemovedEvent_RemovingInnerPkg_NoLog()
		{
			Data.CreatePackingData();
			var outerPkg = Data.PackageJob.Packages.AddNew();
			var innerPkg = outerPkg.Packages.AddNew();
			var parentJob = Data.PackageJob.ParentJob as IStmALogParent;
			innerPkg.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition: innerPkg is NOT outer pkg", false, innerPkg.IsOuter);
			AssertEquals("Precondition: innerPkg is closed", true, innerPkg.IsClosed);

			innerPkg.KP_ClosedTimeUtc = ZDateTime.Empty; //value changed in factory still closed in db
			innerPkg.Delete();
			AssertEquals("There should not be any Item Removed events from removing an inner pkg.", false, parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ItemRemoved.Code).Any());
		}

		public void TestKP_ItemRemovedEvent_RemovingSecondLevelInnerPkg_NoLog()
		{
			Data.CreatePackingData();
			var outerPkg = Data.PackageJob.Packages.AddNew();
			var innerPkg = outerPkg.Packages.AddNew();
			var secondLvlInnerPkg = innerPkg.Packages.AddNew();
			var parentJob = Data.PackageJob.ParentJob as IStmALogParent;
			secondLvlInnerPkg.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition: innerInnerPkg is NOT outer pkg", false, secondLvlInnerPkg.IsOuter);
			AssertEquals("Precondition: innerInnerPkg is closed", true, secondLvlInnerPkg.IsClosed);

			secondLvlInnerPkg.KP_ClosedTimeUtc = ZDateTime.Empty; //value changed in factory still closed in db.
			secondLvlInnerPkg.Delete();
			AssertEquals("There should not be any Item Removed events from removing an inner pkg.", false, parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ItemRemoved.Code).Any());
		}

		public void TestKP_ItemRemovedEvent_RemovingPkgWithMultipleInnerPkgs_OneLog()
		{
			Data.CreatePackingData();
			var outerPkg = Data.PackageJob.Packages.AddNew();
			outerPkg.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			outerPkg.KP_PackageID = "ID001";
			outerPkg.Packages.AddNew();

			var innerPkg = outerPkg.Packages.AddNew();
			innerPkg.KP_PackageID = "INNER";
			innerPkg.Packages.AddNew();

			var parentJob = Data.PackageJob.ParentJob as IStmALogParent;
			Factory.Save();

			AssertEquals("Precondition: outerPkg is outer pkg", true, outerPkg.IsOuter);
			AssertEquals("Precondition: outerPkg is closed", true, outerPkg.IsClosed);

			outerPkg.KP_ClosedTimeUtc = ZDateTime.Empty; //value changed in factory still closed in db.
			outerPkg.Delete();
			var logEvents = parentJob.Logs.Find(l => l.SL_SE_NKEvent == Events.ItemRemoved.Code);
			AssertEquals("There should be only ONE Item Removed events from removing an outer pkg with inner pkgs.", 1, logEvents.Count());
			AssertEquals("Event SL_Ref is correct.", "|RFN=ID001|TYP=Pallet", logEvents.First().SL_Reference);
		}

		#endregion

		#region TestKP_KJ_ParentPackageJob_ReassignPackageBetweenPackageJobsClearsKP_KP

		public void TestKP_KJ_ParentPackageJob_ReassignPackageBetweenPackageJobsClearsKP_KP()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			var pallet = packageJob1.Packages.AddNew();

			var packageJob2 = Factory.New<PkgPackageJob>();
			var pallet2 = packageJob2.Packages.AddNew();
			var box = pallet2.Packages.AddNew(); // must be a child for this test otherwise box.KP_KP_ParentPackage is already empty

			// put the box from PJ2 onto PJ1
			box.KP_KJ_ParentPackageJob = packageJob1.PK;
			AssertEquals("A package has been assigned to ANOTHER PackageJob. That Package's KP_KP should be cleared.", true, box.KP_KP_ParentPackage.IsEmpty);
		}

		#endregion

		#region TestKP_KJ_ParentPackageJob_FiresOuterPackageAddedOnPackageJob

		public void TestKP_KJ_ParentPackageJob_FiresOuterPackageAddedOnPackageJob()
		{
			int outerPackageAddedHitCount = 0;
			PackageEventArgs args = null;
			Data.CreatePackingData();
			Data.PackageJob.OuterPackageAdded += (sender, e) =>
			{
				outerPackageAddedHitCount++;
				args = e;
			};

			var outer1 = Data.PackageJob.Packages.AddNew();
			AssertEquals(1, outerPackageAddedHitCount);
			AssertEquals(outer1, args.Package);

			outer1.KP_KJ_ParentPackageJob = Data.PackageJob.PK;
			AssertEquals("Event should not fire again.", 1, outerPackageAddedHitCount);

			var inner = outer1.Packages.AddNew();
			AssertEquals("Event should not fire for inners.", 1, outerPackageAddedHitCount);

			var outer2 = Data.PackageJob.Packages.AddNew();
			AssertEquals("Event should fire on any top level package collection add.", 2, outerPackageAddedHitCount);
			AssertEquals(outer2, args.Package);
		}

		#endregion

		#region TestKP_KJ_ParentPackageJob_Sequence

		public void TestKP_KJ_ParentPackageJob_Sequence()
		{
			Data.CreatePackingData();

			var package1 = Factory.New<PkgPackage>();
			package1.KP_PackageID = "P1";
			AssertEquals("Package Sequence should be 0 if it is not in a package job", (ZShort)0, package1.KP_Sequence);

			package1.KP_KJ_ParentPackageJob = Data.PackageJob.PK;
			AssertEquals("Package Sequence should be populated if it is assigned to a package job", (ZShort)1, package1.KP_Sequence);

			package1.KP_KJ_ParentPackageJob = Data.PackageJob.PK;
			AssertEquals("Package Sequence should be changed if its package job is not changed", (ZShort)1, package1.KP_Sequence);

			var dummy2 = Factory.New<DummyWithPacking>();
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummy2);
			var package21 = packageJob2.Packages.AddNew("Box", "P21");
			var package22 = packageJob2.Packages.AddNew("Box", "P22");
			AssertEquals((ZShort)1, package21.KP_Sequence);
			AssertEquals((ZShort)2, package22.KP_Sequence);

			package1.KP_KJ_ParentPackageJob = packageJob2.PK;
			AssertEquals("Package Sequence should be popuplated if its package job is changed", (ZShort)3, package1.KP_Sequence);

			package1.KP_KJ_ParentPackageJob = ZGuid.Empty;
			AssertEquals("Package Sequence should be reset to 0 if it is not in a package job", (ZShort)0, package1.KP_Sequence);
		}

		#endregion

		#region TestKP_KP_ParentPackage

		public void TestKP_KP_ParentPackage_ReassignPackageBetweenPackageJobsUpdatesKP_KJ()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			var pallet = packageJob1.Packages.AddNew();

			var packageJob2 = Factory.New<PkgPackageJob>();
			var box = packageJob2.Packages.AddNew();

			// put the box from PJ2 onto the pallet from PJ1
			box.KP_KP_ParentPackage = pallet.PK;
			AssertEquals("A package has been assigned to a package on ANOTHER PackageJob. That Package's KP_KJ should also point to the new PackageJob.", packageJob1.PK, box.KP_KJ_ParentPackageJob);
		}

		public void TestKP_KP_ParentPackage_EmptySequence()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("Box", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("Box", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("Box", "P3");

			AssertEquals((ZShort)2, package2.KP_Sequence);

			package2.KP_KP_ParentPackage = package1.PK;
			AssertEquals("Outer package has package sequence.", (ZShort)1, package1.KP_Sequence);
			AssertEquals("Inner package does not have package sequence.", (ZShort)0, package2.KP_Sequence);
			AssertEquals("Third package resequenced.", (ZShort)2, package3.KP_Sequence);
		}

		public void TestKP_KP_ParentPackage_ReSequenceAfterClearParentPackage()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("Box", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("Box", "P2");

			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);

			package1.KP_KP_ParentPackage = package2.PK;
			AssertEquals("Inner package does not have package sequence.", (ZShort)0, package1.KP_Sequence);
			AssertEquals("Outer package has package sequence.", (ZShort)1, package2.KP_Sequence);

			package1.KP_KP_ParentPackage = ZGuid.Empty;
			AssertEquals("Outer package has package sequence.", (ZShort)2, package1.KP_Sequence);
			AssertEquals("Outer package has package sequence.", (ZShort)1, package2.KP_Sequence);
		}

		public void TestKP_KP_ParentPackage_ValidatesPackTypeIfContainer()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			var box = Data.PackageJob.Packages.AddNew("BOX");
			AssertNoErrors("Precondition", container.KP_F3_NKPackTypeInfo);

			box.Packages.Add(container);
			AssertHasErrors(container.KP_F3_NKPackTypeInfo);
		}

		public void TestKP_KP_ParentPackage_ValidatesReleasedFlags()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var box = pallet.Packages.AddNew("BOX");
			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			box.KP_IsReleasedViaJob = true;
			AssertHasErrors("Precondition", box.KP_ReleasedTimeUtcInfo);
			AssertHasErrors("Precondition", box.KP_IsReleasedViaJobInfo);

			Data.PackageJob.Packages.Add(box);
			AssertNoErrors(box.KP_ReleasedTimeUtcInfo);
			AssertNoErrors(box.KP_IsReleasedViaJobInfo);
		}

		public void TestKP_KP_ParentPackage_FiresAppropriateEventOnPackageJob()
		{
			Data.CreatePackingData();

			var outer1 = Data.PackageJob.Packages.AddNew();
			var outer2 = Data.PackageJob.Packages.AddNew();
			var inner1 = outer2.Packages.AddNew();
			var inner2 = outer2.Packages.AddNew();

			int outerPackageAddedHitCount = 0;
			int outerParentPackageChangedHitCount = 0;
			PackageEventArgs outerPackageAddedArgs = null;
			PackageEventArgs outerParentPackageChangedArgs = null;
			Data.PackageJob.OuterPackageAdded += (sender, e) =>
			{
				outerPackageAddedHitCount++;
				outerPackageAddedArgs = e;
			};
			Data.PackageJob.OuterParentPackageChanged += (sender, e) =>
			{
				outerParentPackageChangedHitCount++;
				outerParentPackageChangedArgs = e;
			};

			inner2.KP_KP_ParentPackage = inner1.PK;
			AssertEquals("Assigning an inner to a package should not fire any event.", 0, outerPackageAddedHitCount);
			AssertEquals("Assigning an inner to a package should not fire any event.", 0, outerParentPackageChangedHitCount);

			inner2.KP_KP_ParentPackage = ZGuid.Empty;
			AssertEquals("Making an inner an outer is equivalent to an outer being added.", 1, outerPackageAddedHitCount);
			AssertEquals(0, outerParentPackageChangedHitCount);
			AssertEquals(inner2, outerPackageAddedArgs.Package);

			outer2.KP_KP_ParentPackage = outer1.PK;
			AssertEquals("Outer package added should not have been fired.", 1, outerPackageAddedHitCount);
			AssertEquals("When outer becomes an inner, should fire Parent Package Changed on Outer.", 1, outerParentPackageChangedHitCount);
			AssertEquals(outer2, outerParentPackageChangedArgs.Package);

			inner1.Delete();
			AssertEquals("Deleting inners clears Parent Package, this should not fire Outer Package Added.", 1, outerPackageAddedHitCount);
		}

		public void TestKP_KP_ParentPackage_ShouldPackTrackedPackagesViaDivotIsTrue_DivotsCreated()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var hu = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU1");
			var huInner = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU1Inner");

			Assert("Precondition - Top handling unit package has not been set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Parent package has not been set.", hu.KP_KP_ParentPackage.IsEmpty);
			Assert("Precondition - Top handling unit package has not been set.", huInner.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Parent package has not been set.", huInner.KP_KP_ParentPackage.IsEmpty);
			AssertEquals("Precondition - No Divots have been created yet.", 0, Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);

			hu.Packages.Add(huInner);

			Assert("Top handling unit must not have top handling unit package.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Top handling unit must not have parent package set.", hu.KP_KP_ParentPackage.IsEmpty);
			AssertEquals("Parent package has not been cleared.", hu.PK, huInner.KP_KP_ParentPackage);
			AssertEquals("Top handling unit package has been set.", hu.PK, huInner.KP_KP_TopHandlingUnitPackage);

			var divot = Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Single();
			AssertEquals(hu.PK, divot.KPD_KP_HandlingUnit);
			AssertEquals(huInner.PK, divot.KPD_KP_Package);
			AssertEquals(Env.CurrentUser.Initials, divot.KPD_GS_NKPackedUser);
			AssertEquals(false, divot.KPD_PackedTime.IsEmpty);
		}

		public void TestKP_KP_ParentPackage_ShouldPackTrackedPackagesViaDivotIsFalse_DivotsNotCreated()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParent);
			var parentJob = Factory.New<DummyPackingParent>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var hu = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU1");
			var huInner = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU1Inner");

			Assert("Precondition - Top HU must not have top handling unit package set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Top HU must not have parent package set.", hu.KP_KP_ParentPackage.IsEmpty);
			Assert("Precondition - Top handling unit package has not been set.", huInner.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Parent package has not been set.", huInner.KP_KP_ParentPackage.IsEmpty);
			AssertEquals("Precondition - No Divots have been created yet.", 0, Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);

			hu.Packages.Add(huInner);

			Assert("Top HU must not have top handling unit package set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Top HU must not have parent package set.", hu.KP_KP_ParentPackage.IsEmpty);
			Assert("Top handling unit package has not been set.", huInner.KP_KP_TopHandlingUnitPackage.IsEmpty);
			AssertEquals("Parent package has not been cleared.", hu.PK, huInner.KP_KP_ParentPackage);

			var divots = Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("0 Divot should have been created when ShouldPackTrackedPackagesViaDivot is set to false.", 0, divots.Length);
		}

		#endregion

		#region TestKP_PackageQty

		#region TestKP_PackageQty_ValidatesPackageID

		public void TestKP_PackageQty_ValidatesPackageID()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "123";
			AssertNoErrors("Precondition", package.KP_F3_NKPackTypeInfo);

			package.KP_PackageQty = 2; // Package ID is only valid for qty of 1.
			AssertHasErrors(package.KP_PackageIDInfo);
		}

		#endregion

		#region TestCalculateKP_WeightWhenKP_TareWeightChanged

		public void TestCalculateKP_WeightWhenKP_TareWeightChanged()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_TareWeight = 0;
			package.KP_Weight = 10;
			AssertEquals(10m, package.GoodsWeight);

			package.KP_TareWeight = 2;
			CombineAssertions(() =>
			{
				AssertEquals(10m, package.GoodsWeight);
				AssertEquals(12m, package.KP_Weight);
			});
		}

		#endregion

		#region TestKP_PackageQty_CalculatesGrossTareDunnageGoodsAndVolume

		public void TestKP_PackageQty_CalculatesGrossTareDunnageGoodsAndVolume()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 10m;
			package.KP_Length = 1m;
			package.KP_Width = 1m;
			package.KP_Height = 1m;
			package.KP_VolumeUQ = "M3";
			package.KP_DimensionUQ = "M";

			package.KP_PackageQty = 10;
			AssertEquals("package.KP_Weight", 100m, package.KP_Weight);
			AssertEquals("package.KP_Volume", 10m, package.KP_Volume);

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Precondition", 2280m, container.KP_TareWeight);
			AssertEquals("Precondition", 38.508m, container.KP_Volume);

			container.KP_DunnageWeight = 10m;
			container.Container.GoodsWeight = 20m;
			AssertEquals("container.KP_Weight", 2300m, container.KP_Weight);

			container.KP_PackageQty = 10;
			AssertEquals("container.KP_TareWeight", 22800m, container.KP_TareWeight);
			AssertEquals("container.KP_DunnageWeight", 100m, container.KP_DunnageWeight);
			AssertEquals("container.Container.GoodsWeight", 200m, container.Container.GoodsWeight);
			AssertEquals("container.KP_Volume", 385.08m, container.KP_Volume);
			AssertEquals("container.KP_Weight", 23000m, container.KP_Weight);
		}

		public void TestKP_PackageQty_CalculatesGrossTareDunnageGoodsAndVolume_SequentialInvocations()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 10m;
			package.KP_Length = 1m;
			package.KP_Width = 1m;
			package.KP_Height = 1m;
			package.KP_VolumeUQ = "M3";

			package.KP_DimensionUQ = "M";

			package.KP_PackageQty = 10;
			AssertEquals("package.KP_Weight", 100m, package.KP_Weight);
			AssertEquals("package.KP_Volume", 10m, package.KP_Volume);

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Precondition", 2280m, container.KP_TareWeight);
			AssertEquals("Precondition", 38.508m, container.KP_Volume);

			container.KP_DunnageWeight = 10m;
			container.Container.GoodsWeight = 20m;
			AssertEquals("container.KP_Weight", 2300m, container.KP_Weight);

			var sequentialCallInvoked = false;
			container.KP_PackageQtyInfo.ValueChanged += KP_PackageQtyInfo_ValueChanged;
			container.KP_PackageQty = 10;

			Assert("container.KP_PackageQty setter invoked twice.", sequentialCallInvoked);
			AssertEquals("container.KP_TareWeight", 22800m, container.KP_TareWeight);
			AssertEquals("container.KP_DunnageWeight", 100m, container.KP_DunnageWeight);
			AssertEquals("container.Container.GoodsWeight", 200m, container.Container.GoodsWeight);
			AssertEquals("container.KP_Volume", 385.08m, container.KP_Volume);
			AssertEquals("container.KP_Weight", 23000m, container.KP_Weight);

			void KP_PackageQtyInfo_ValueChanged(object sender, EventArgs e)
			{
				container.KP_PackageQty = 10;
				sequentialCallInvoked = true;
			}
		}

		public void TestKP_PackageQty_DoesNotCalculatesGrossTareDunnageGoods_ButDoesCalculateVolume_IfQtyPreviouslyZero()
		{
			Data.CreatePackingData();

			var packageSetup = Data.PackageJob.Packages.AddNew("BOX");
			packageSetup.KP_PackageQty = 0;
			packageSetup.KP_IsUnknownQty = true;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var package = otherFactory.LoadTop1<PkgPackage>(new ZQuery(PkgPackageSchema.PK, packageSetup.PK));

			package.KP_Weight = 23000m;
			package.KP_Length = 1m;
			package.KP_Width = 1m;
			package.KP_Height = 1m;
			package.KP_Volume = 0m;
			package.KP_DimensionUQ = "M";
			package.KP_VolumeUQ = "M3";

			package.KP_TareWeight = 22800m;
			package.KP_DunnageWeight = 100m;

			var originalTareWeight = package.KP_TareWeight;
			var originalDunnageWeight = package.KP_DunnageWeight;
			var originalGoodsWeight = package.GoodsWeight;
			AssertEquals("Precondition: Original package KP_PackageQty should be zero", 0, package.KP_PackageQty);

			AssertNoExceptionThrown(() =>
			{
				package.KP_PackageQty = 10;
			});

			CombineAssertions("Should have not recalculated weights, but should have calculated volumes", () =>
			{
				AssertEquals("Package Tare Weight should not be recalculated", originalTareWeight, package.KP_TareWeight);
				AssertEquals("Package Tare Weight should not be recalculated", originalDunnageWeight, package.KP_DunnageWeight);
				AssertEquals("Package Goods Weight should not be recalculated", originalGoodsWeight, package.GoodsWeight);
				AssertEquals("Package Volume should have been recalculated", 10m, package.KP_Volume);
			});
		}

		#endregion

		#region TestKP_PackageQty_CachesPreviousPackageQtyToAllowRecalculationOfPackageWeights

		public void TestKP_PackageQty_CachesPreviousPackageQtyToAllowRecalculationOfPackageWeights()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 10m;

			package.KP_PackageQty = 10;
			AssertEquals("package.KP_Weight", 100m, package.KP_Weight);

			package.KP_PackageQty = 0;
			AssertEquals("package.KP_Weight", 100m, package.KP_Weight);

			package.KP_PackageQty = 1;
			AssertEquals("package.KP_Weight", 10m, package.KP_Weight);
		}

		#endregion

		#region TestKP_PackageQty_CachesPreviousPackageQtyToAllowRecalculationOfContainerWeights

		public void TestKP_PackageQty_CachesPreviousPackageQtyToAllowRecalculationOfContainerWeights()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Precondition", 2280m, container.KP_TareWeight);
			AssertEquals("Precondition", 38.508m, container.KP_Volume);

			container.KP_DunnageWeight = 10m;
			container.Container.GoodsWeight = 20m;
			AssertEquals("container.KP_Weight", 2300m, container.KP_Weight);

			container.KP_PackageQty = 10;
			AssertEquals("container.KP_TareWeight", 22800m, container.KP_TareWeight);
			AssertEquals("container.KP_DunnageWeight", 100m, container.KP_DunnageWeight);
			AssertEquals("container.Container.GoodsWeight", 200m, container.Container.GoodsWeight);
			AssertEquals("container.KP_Volume", 385.08m, container.KP_Volume);
			AssertEquals("container.KP_Weight", 23000m, container.KP_Weight);

			container.KP_PackageQty = 0;
			AssertEquals("container.KP_TareWeight", 22800m, container.KP_TareWeight);
			AssertEquals("container.KP_DunnageWeight", 100m, container.KP_DunnageWeight);
			AssertEquals("container.Container.GoodsWeight", 200m, container.Container.GoodsWeight);
			AssertEquals("container.KP_Volume", 385.08m, container.KP_Volume);
			AssertEquals("container.KP_Weight", 23000m, container.KP_Weight);

			container.KP_PackageQty = 1;
			AssertEquals("container.KP_TareWeight", 2280m, container.KP_TareWeight);
			AssertEquals("container.KP_DunnageWeight", 10m, container.KP_DunnageWeight);
			AssertEquals("container.Container.GoodsWeight", 20m, container.Container.GoodsWeight);
			AssertEquals("container.KP_Volume", 38.508m, container.KP_Volume);
			AssertEquals("container.KP_Weight", 2300m, container.KP_Weight);
		}

		#endregion

		#region TestKP_PackageQty_DoesNotCalculateVolumeIfAnyDimsIsZero

		public void TestKP_PackageQty_DoesNotCalculateVolumeIfAnyDimsIsZero()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_DimensionUQ = "M";
			package.KP_Volume = 10m;
			package.KP_VolumeUQ = "M3";

			package.KP_Length = 0m;
			package.KP_Width = 1m;
			package.KP_Height = 1m;
			package.KP_PackageQty = 10;
			AssertEquals("package.KP_Volume", 10m, package.KP_Volume);

			package.KP_Width = 0m;
			package.KP_Length = 1m;
			package.KP_Height = 1m;
			package.KP_PackageQty = 5;
			AssertEquals("package.KP_Volume", 10m, package.KP_Volume);

			package.KP_Height = 0m;
			package.KP_Length = 1m;
			package.KP_Width = 1m;
			package.KP_PackageQty = 10;
			AssertEquals("package.KP_Volume", 10m, package.KP_Volume);
		}

		#endregion

		#region TestKP_PackageQty_DoesNotCalculateVolumeIfPackageQtyIsZero

		public void TestKP_PackageQty_DoesNotCalculateVolumeIfPackageQtyIsZero()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 10m;
			package.KP_Length = 1m;
			package.KP_Width = 1m;
			package.KP_Height = 1m;
			package.KP_VolumeUQ = "M3";
			package.KP_DimensionUQ = "M";

			package.KP_PackageQty = 0;
			AssertEquals("package.KP_Volume", 1m, package.KP_Volume);
		}

		#endregion

		#region TestKP_PackageQty_DoesNotCalculateVolumeIfImportingData

		public void TestKP_PackageQty_DoesNotCalculateVolumeIfImportingData()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			((ISupportDataImporting)package).IsImportingData = true;
			package.KP_DimensionUQ = "M";
			package.KP_Volume = 10m;
			package.KP_VolumeUQ = "M3";

			package.KP_Length = 100m;
			package.KP_Width = 100m;
			package.KP_Height = 100m;
			package.KP_PackageQty = 10;
			AssertEquals("Volume should not be calculated when IsImportingData is set to true", 10m, package.KP_Volume);

			((ISupportDataImporting)package).IsImportingData = false;
			package.KP_Width = 100m; // poke
			AssertEquals("Volume should be calculated when IsImportingData is not set to true", 10000000m, package.KP_Volume);
		}

		#endregion

		#region TestKP_PackageQty_DoesNoWorkDuringClone

		public void TestKP_PackageQty_DoesNoWorkDuringClone()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 10m;
			package.KP_Length = 1m;
			package.KP_Width = 1m;
			package.KP_Height = 1m;
			package.KP_VolumeUQ = "M3";
			package.KP_DimensionUQ = "M";

			package.KP_PackageQty = 10;
			AssertEquals("Precondition", 100m, package.KP_Weight);
			AssertEquals("Precondition", 10m, package.KP_Volume);

			((IBusinessObjectInternals)package).IsCopying = true;
			package.KP_PackageQty = 20;
			AssertEquals("Weight should *not* be recalculated during clone.", 100m, package.KP_Weight);
			AssertEquals("Volume should *not* be recalculated during clone.", 10m, package.KP_Volume);
		}

		#endregion

		#endregion

		#region TestKP_RequiredTemperatureMinimumMaxValidateEachOther

		public void TestKP_RequiredTemperatureMinimumMaxValidateEachOther()
		{
			var package = Factory.New<PkgPackage>();
			AssertNoErrors("Precondition", package.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors("Precondition", package.KP_RequiredTemperatureMaximumInfo);

			package.KP_RequiredTemperatureMinimum = 7m;
			AssertHasErrors(package.KP_RequiredTemperatureMinimumInfo);
			AssertHasErrors("Entering an invalid Mininum Temperature should also validate the Maximum Temperature.", package.KP_RequiredTemperatureMaximumInfo);

			package.KP_RequiredTemperatureMinimum = 0m;
			AssertNoErrors("Precondition", package.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors("Precondition", package.KP_RequiredTemperatureMaximumInfo);

			package.KP_RequiredTemperatureMaximum = -7m;
			AssertHasErrors(package.KP_RequiredTemperatureMaximumInfo);
			AssertHasErrors("Entering an invalid Maximum Temperature should also validate the Mininum Temperature.", package.KP_RequiredTemperatureMinimumInfo);
		}

		#endregion

		#region TestChildWeightAffectsParentWeight

		#region TestChildWeightAffectsParentWeight

		public void TestChildWeightAffectsParentWeight()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.KP_Weight = 100m;
			AssertEquals(100m, container.KP_Weight);

			// add a child package

			var pallet = Factory.New<PkgPackage>();
			pallet.KP_Weight = 20m;
			container.Packages.Add(pallet);
			AssertEquals("Adding a Pallet to the Container should have added the Pallet's Weight to the Container.", 120m, container.KP_Weight);

			// modify the child package's weight

			pallet.KP_Weight = 30m;
			AssertEquals("Increasing the Pallet weight should have increased the parent Container's Weight.", 130m, container.KP_Weight);
			pallet.KP_Weight = 25m;
			AssertEquals("Decreasing the Pallet weight should have decreased the parent Container's Weight.", 125m, container.KP_Weight);

			// reduce the parent package weight, then reduce the child's weight and ensure the parent weight does not become negative

			container.KP_Weight = 10m;
			pallet.KP_Weight = 5m; // was 25
			AssertEquals("Decreasing the Pallet weight should have decreased the parent Container's Weight, but not below 0.", 0m, container.KP_Weight);

			// add a child below the first child (CNT -> PLT -> BOX)

			var box = Factory.New<PkgPackage>();
			box.KP_Weight = 2m;
			pallet.KP_Weight = 25m;
			container.KP_Weight = 125m;

			pallet.Packages.Add(box);
			AssertEquals("Adding a Box to the Pallet should have added the Box's weight to all parents (both Pallet and Container).", 27m, pallet.KP_Weight);
			AssertEquals("Adding a Box to the Pallet should have added the Box's weight to all parents (both Pallet and Container).", 127m, container.KP_Weight);

			// modify the lowest child package's weight

			box.KP_Weight = 3m;
			AssertEquals("Increasing the Box weight should have increased the weight of all parents (both Pallet and Container).", 28m, pallet.KP_Weight);
			AssertEquals("Increasing the Box weight should have increased the weight of all parents (both Pallet and Container).", 128m, container.KP_Weight);
			box.KP_Weight = 1m;
			AssertEquals("Decreasing the Box weight should have decreased the weight of all parents (both Pallet and Container).", 26m, pallet.KP_Weight);
			AssertEquals("Decreasing the Box weight should have decreased the weight of all parents (both Pallet and Container).", 126m, container.KP_Weight);

			// remove the child package (will also delete it's child)

			pallet.Delete();
			AssertEquals("Removing the Pallet should have decreased the parent Container's weight by the Pallet weight.", 100m, container.KP_Weight);
		}

		#endregion

		#region TestChildWeightAffectsParentWeight_WithMixedUQs

		public void TestChildWeightAffectsParentWeight_WithMixedUQs()
		{
			Data.CreatePackingData();

			// create a half tonne pallet

			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			pallet.KP_WeightUQ = Constants.Weight.Tonnes;
			pallet.KP_Weight = 0.5m;
			AssertEquals(0.5m, pallet.KP_Weight);

			// add a 100kg box

			var box = Factory.New<PkgPackage>();
			box.KP_WeightUQ = Constants.Weight.Kilograms;
			box.KP_Weight = 100m;
			pallet.Packages.Add(box);
			AssertEquals("Adding a Box to the Pallet should have added the Box's weight to the Pallet.", 0.6m, pallet.KP_Weight);

			// change the box's UQ to pounds

			box.KP_WeightUQ = Constants.Weight.Pounds;
			AssertEquals("Changing the Box's UQ should have updated the parent Pallet's weight.", 0.545m, pallet.KP_Weight);
		}

		#endregion

		#region TestChildWeightAffectsParentWeight_Suspended

		public void TestChildWeightAffectsParentWeight_Suspended()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.KP_Weight = 100m;

			var pallet = Factory.New<PkgPackage>();
			pallet.KP_F3_NKPackType = "PLT";
			pallet.KP_Weight = 20m;

			var box = Factory.New<PkgPackage>();
			box.KP_F3_NKPackType = "BOX";
			box.KP_Weight = 3m;

			AssertEquals("Precondition", 100m, container.KP_Weight);
			AssertEquals("Precondition", 20m, pallet.KP_Weight);
			AssertEquals("Precondition", 3m, box.KP_Weight);

			using (new SemaphoreManager(pallet.SuspendAddWeightToParentPackageSemaphore))
			{
				// add a child package
				container.Packages.Add(pallet);
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have increased.", 100m, container.KP_Weight);

				// modify the child package's weight
				pallet.KP_Weight = 25m;
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have increased.", 100m, container.KP_Weight);
				pallet.KP_Weight = 20m;
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have decreased.", 100m, container.KP_Weight);

				// add a child below the first child (CNT -> PLT -> BOX)
				pallet.Packages.Add(box);
				AssertEquals("Box.ParentPackageWeightUpdate is not suspended, Pallet Weight should have increased.", 23m, pallet.KP_Weight);
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have increased.", 100m, container.KP_Weight);

				// modify the lowest child package's weight
				box.KP_Weight = 5m;
				AssertEquals("Box.ParentPackageWeightUpdate is not suspended, Pallet Weight should have increased.", 25m, pallet.KP_Weight);
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have increased.", 100m, container.KP_Weight);
				box.KP_Weight = 1m;
				AssertEquals("Box.ParentPackageWeightUpdate is not suspended, Pallet Weight should have increased.", 21m, pallet.KP_Weight);
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have increased.", 100m, container.KP_Weight);

				// remove the child package (will also delete it's child)
				pallet.Delete();
				AssertEquals("Pallet.ParentPackageWeightUpdate is suspended, Container Weight should not have decreased.", 100m, container.KP_Weight);
			}
		}

		#endregion

		#endregion

		#region TestVolumeIsAutoCalculated

		public void TestVolumeIsAutoCalculated()
		{
			var package = Factory.New<PkgPackage>();

			package.KP_Length = 1;
			package.KP_Width = 2;
			package.KP_Height = 3;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals(6m, package.KP_Volume);

			package.KP_Length = 0;
			AssertEquals("Volume should not be reset if any value is 0.", 6m, package.KP_Volume);

			package.KP_Length = 2;
			AssertEquals(12m, package.KP_Volume);

			package.KP_Width = 3;
			AssertEquals(18m, package.KP_Volume);

			package.KP_Height = 4;
			AssertEquals(24m, package.KP_Volume);

			package.KP_DimensionUQ = Constants.Length.Feet;
			AssertEquals(0.68m, package.KP_Volume);

			package.KP_VolumeUQ = Constants.Volume.CubicFeet;
			AssertEquals("Volume should not be recalculated when changing the UQ (otherwise the the user cannot enter volume followed by UQ).", 0.68m, package.KP_Volume);

			package.KP_DimensionUQ = "XX";
			AssertEquals("Volume should not be recalculated if a requisite value is invalid.", 0.68m, package.KP_Volume);
		}

		#endregion

		#region TestTemperatureDetailsReadOnly

		public void TestTemperatureDetailsReadOnly()
		{
			var package = Factory.New<PkgPackage>();

			var temperatureProperties = new[]
			{
				package.KP_RequiredTemperatureMinimumInfo,
				package.KP_RequiredTemperatureMaximumInfo,
				package.KP_RequiredTemperatureUnitInfo,
			};

			package.KP_RequiresTemperatureControl = false;
			Array.ForEach(temperatureProperties, info => AssertEquals(true, info.ReadOnly));

			package.KP_RequiresTemperatureControl = true;
			Array.ForEach(temperatureProperties, info => AssertEquals(false, info.ReadOnly));
		}

		#endregion

		#region TestKP_IsUnknownQty

		PropertyInfo GetKP_IsUnknownQtyProperty()
		{
			return typeof(PkgPackage).GetProperty("KP_IsUnknownQty");
		}

		public void TestKP_IsUnknownQty_FieldTypeIsHidden()
		{
			var property = GetKP_IsUnknownQtyProperty();
			AssertEquals(ActionFieldType.Hidden, ((ActionFieldAttribute)Attribute.GetCustomAttribute(property, typeof(ActionFieldAttribute))).FieldType);
		}

		public void TestKP_IsUnknownQty_IsReadOnly()
		{
			var property = GetKP_IsUnknownQtyProperty();
			AssertEquals(true, ((ActionFieldAttribute)Attribute.GetCustomAttribute(property, typeof(ActionFieldAttribute))).ReadOnly);
		}

		public void TestKP_IsUnknownQty_IsExcludedFromMap()
		{
			var property = GetKP_IsUnknownQtyProperty();
			Assert(Attribute.IsDefined(property, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestKP_IsUnknownQty_IsSetAsReadOnlyInWorkflow()
		{
			var property = GetKP_IsUnknownQtyProperty();
			Assert(Attribute.IsDefined(property, typeof(WorkflowSetFieldReadonly), false));
		}

		public void TestKP_IsUnknownQty_IsReadOnlyInPackageInstance()
		{
			var package = Factory.New<PkgPackage>();
			var kpIsUnknownQtyInfo = package.KP_IsUnknownQtyInfo;
			Assert(kpIsUnknownQtyInfo.ReadOnly);
		}

		public void TestKP_IsUnknownQty_SetFromPackageQtyIfWasUnknown()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_IsUnknownQty = true;

			package.KP_PackageQty = 1;
			AssertEquals("Setting KP_PackageQty to 1 should set KP_IsUnknownQty to false", false, package.KP_IsUnknownQty);

			package.KP_PackageQty = 0;
			AssertEquals("Setting KP_PackageQty to 0 should leave KP_IsUnknownQty as false", false, package.KP_IsUnknownQty);
		}

		public void TestKP_IsUnknownQty_NotUpdatedToUnknown()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals("KP_IsUnknownQty starts as false", false, package.KP_IsUnknownQty);

			package.KP_PackageQty = 1;
			AssertEquals("Setting KP_PackageQty to 1 should leave KP_IsUnknownQty as false", false, package.KP_IsUnknownQty);

			package.KP_PackageQty = 0;
			AssertEquals("Setting KP_PackageQty to 0 should leave KP_IsUnknownQty as false when already false", false, package.KP_IsUnknownQty);
		}

		#endregion

		#region TestKP_PackageID

		public void TestKP_PackageID()
		{
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();

			AssertNull(package1.PackageID);
			AssertNull(package2.PackageID);

			var packageJob = Factory.New<PkgPackageJob>();
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			package1.KP_PackageID = "ABC";
			AssertEquals("ABC", package1.KP_PackageID);
			AssertEquals("", package2.KP_PackageID);

			package2.KP_PackageID = "DEF";
			AssertEquals("ABC", package1.KP_PackageID);
			AssertEquals("DEF", package2.KP_PackageID);

			package1.KP_PackageID = "DEF";
			AssertEquals("DEF", package1.KP_PackageID);
			AssertEquals("DEF", package2.KP_PackageID);

			package1.KP_PackageID = "";
			AssertEquals("", package1.KP_PackageID);
			AssertEquals("DEF", package2.KP_PackageID);
		}

		public void TestKP_PackageID_ReadOnlyWhenIsSentToRTUS()
		{
			var package = Factory.New<PkgPackage>();

			AssertNull(package.PackageID);
			AssertEquals(false, package.IsSentToRTUS);

			package.KP_PackageID = "ABC";
			AssertEquals("ABC", package.KP_PackageID);
			AssertEquals(false, package.IsSentToRTUS);
			AssertEquals(false, package.KP_PackageIDInfo.ReadOnly);

			package.IsSentToRTUS = true;
			AssertEquals(true, package.KP_PackageIDInfo.ReadOnly);
		}

		public void TestGetPackageHeader()
		{
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();

			AssertNull(package1.GetPackageHeader());
			AssertNull(package2.GetPackageHeader());

			var packageJob = Factory.New<PkgPackageJob>();
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			package1.KP_PackageID = "ABC";
			package2.KP_PackageID = "DEF";

			var packageHeader1 = package1.GetPackageHeader();
			var packageHeader2 = package2.GetPackageHeader();
			AssertEquals("ABC", packageHeader1.KPH_PackageID);
			AssertEquals("DEF", packageHeader2.KPH_PackageID);

			package1.KP_PackageID = "DEF";
			AssertEquals("DEF", packageHeader1.KPH_PackageID);
			AssertEquals("DEF", packageHeader2.KPH_PackageID);

			package1.KP_PackageID = "";
			AssertEquals("", package1.KP_PackageID);
			AssertEquals("DEF", packageHeader2.KPH_PackageID);
		}

		public void TestKP_PackageID_PkgPackageHeader()
		{
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();

			AssertNull(package1.PackageID);
			AssertNull(package2.PackageID);
			AssertEquals("", package1.KP_PackageID);
			AssertEquals("", package2.KP_PackageID);

			var packageJob = Factory.New<PkgPackageJob>();
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			package1.KP_PackageID = "ABC";
			AssertEquals("ABC", package1.KP_PackageID);
			AssertEquals("", package2.KP_PackageID);
			var packageID1 = package1.PackageID;
			var packageID2 = package2.PackageID;
			AssertEquals("ABC", packageID1.KPH_PackageID);
			AssertNull(packageID2);

			package2.KP_PackageID = "DEF";
			AssertEquals("ABC", package1.KP_PackageID);
			AssertEquals("DEF", package2.KP_PackageID);
			packageID1 = package1.PackageID;
			packageID2 = package2.PackageID;
			AssertEquals("ABC", packageID1.KPH_PackageID);
			AssertEquals("DEF", packageID2.KPH_PackageID);

			package1.KP_PackageID = "DEF";
			AssertEquals("DEF", package1.KP_PackageID);
			AssertEquals("DEF", package2.KP_PackageID);
			AssertEquals("Should have just set the id, should be the same id object for now.", packageID1, package1.PackageID);
			packageID1 = package1.PackageID;
			packageID2 = package2.PackageID;
			AssertEquals("DEF", packageID1.KPH_PackageID);
			AssertEquals("DEF", packageID2.KPH_PackageID);

			package1.KP_PackageID = "";
			AssertEquals("", package1.KP_PackageID);
			AssertEquals("DEF", package2.KP_PackageID);
			packageID1 = package1.PackageID;
			packageID2 = package2.PackageID;
			AssertNull(packageID1);
			AssertEquals("DEF", packageID2.KPH_PackageID);
		}

		#region TestKP_KPH_PackageHeader

		public void TestKP_KPH_PackageHeader_ShouldPackTrackedPackagesViaDivotIsTrue_HUAndInnerHaveID_DivotsCreated()
		{
			TestKP_KPH_PackageHeader_PackageHandlingUnitDivotsCore(shouldUseDivots: true, handlingUnitHasId: true, innerHasId: true, divotsCreated: true);
		}

		public void TestKP_KPH_PackageHeader_ShouldPackTrackedPackagesViaDivotIsTrue_HUHasNoID_DivotsNotCreated()
		{
			TestKP_KPH_PackageHeader_PackageHandlingUnitDivotsCore(shouldUseDivots: true, handlingUnitHasId: false, innerHasId: true, divotsCreated: false);
		}

		public void TestKP_KPH_PackageHeader_ShouldPackTrackedPackagesViaDivotIsTrue_InnerHasNoID_DivotsNotCreated()
		{
			TestKP_KPH_PackageHeader_PackageHandlingUnitDivotsCore(shouldUseDivots: true, handlingUnitHasId: true, innerHasId: false, divotsCreated: false);
		}

		public void TestKP_KPH_PackageHeader_ShouldPackTrackedPackagesViaDivotIsFalse_DivotsNotCreated()
		{
			TestKP_KPH_PackageHeader_PackageHandlingUnitDivotsCore(shouldUseDivots: false, handlingUnitHasId: true, innerHasId: true, divotsCreated: false);
		}

		public void TestKP_KPH_PackageHeader_PackageHandlingUnitDivotsCore(bool shouldUseDivots, bool handlingUnitHasId, bool innerHasId, bool divotsCreated)
		{
			IPackingParent packingParent = null;
			if (shouldUseDivots)
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
				packingParent = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			}
			else
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParent);
				packingParent = Factory.New<DummyPackingParent>();
			}

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			var hu = packageJob.Packages.AddNew(PackType.Freight.PLT);
			var huInner = packageJob.Packages.AddNew(PackType.Freight.PKG);
			hu.Packages.Add(huInner);

			Assert("Precondition - Top HU must not have handling unit package set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Top HU must not have Parent package set.", hu.KP_KP_ParentPackage.IsEmpty);
			Assert("Precondition - Top handling unit package has not been set.", huInner.KP_KP_TopHandlingUnitPackage.IsEmpty);
			AssertEquals("Precondition - Parent package has been set.", hu.PK, huInner.KP_KP_ParentPackage);
			AssertEquals("Precondition - No Divots have been created yet.", 0, Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);

			if (handlingUnitHasId)
			{
				hu.KP_PackageID = "HU";
			}

			if (innerHasId)
			{
				huInner.KP_PackageID = "HUInner";
			}

			if (divotsCreated)
			{
				Assert("Top handling unit package has not been set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
				Assert("Parent package has not been set.", hu.KP_KP_ParentPackage.IsEmpty);
				AssertEquals("Parent package has not been cleared.", hu.PK, huInner.KP_KP_ParentPackage);
				AssertEquals("Top handling unit package has been set.", hu.PK, huInner.KP_KP_TopHandlingUnitPackage);

				var divot = Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Single();
				AssertEquals(divot.KPD_KP_HandlingUnit, hu.PK);
				AssertEquals(divot.KPD_KP_Package, huInner.PK);
				AssertEquals(divot.KPD_GS_NKPackedUser, Env.CurrentUser.Initials);
				AssertEquals(false, divot.KPD_PackedTime.IsEmpty);
			}
			else
			{
				Assert("Top handling unit package has not been set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
				Assert("Parent package has not been set.", hu.KP_KP_ParentPackage.IsEmpty);
				Assert("Top handling unit package has not been set.", huInner.KP_KP_TopHandlingUnitPackage.IsEmpty);
				AssertEquals("Parent package has not been cleared.", hu.PK, huInner.KP_KP_ParentPackage);

				var divots = Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
				AssertEquals("0 Divot should have been created when ShouldPackTrackedPackagesViaDivot is set to false.", 0, divots.Length);
			}
		}

		#endregion

		#region TestKP_KPH_PackageHeader_Sequence

		public void TestKP_KPH_PackageHeader_Sequence_PackageSequenceTypeIsStandard()
		{
			TestKP_KPH_PackageHeader_SequenceCore(PackageSequenceType.Standard);
		}

		public void TestKP_KPH_PackageHeader_Sequence_PackageSequenceTypeIsOuter()
		{
			TestKP_KPH_PackageHeader_SequenceCore(PackageSequenceType.Outer);
		}

		public void TestKP_KPH_PackageHeader_Sequence_PackageSequenceTypeIsOuterAndLooseID()
		{
			TestKP_KPH_PackageHeader_SequenceCore(PackageSequenceType.OuterWithLooseID);
		}

		void TestKP_KPH_PackageHeader_SequenceCore(PackageSequenceType packageSequenceType)
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = packageSequenceType;

			var packageHeader1 = Factory.New<PkgPackageHeader>();
			packageHeader1.KPH_PackageID = "ABC";
			var packageHeader2 = Factory.New<PkgPackageHeader>();
			packageHeader2.KPH_PackageID = "DEF";

			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			package1.KP_KJ_ParentPackageJob = Data.PackageJob.PK;
			package2.KP_KJ_ParentPackageJob = Data.PackageJob.PK;

			package1.KP_KPH_PackageHeader = packageHeader1.PK;
			AssertEquals("ABC", package1.KP_PackageID);
			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals("", package2.KP_PackageID);
			AssertEquals(packageSequenceType != PackageSequenceType.Standard ? (ZShort)0 : (ZShort)2, package2.KP_Sequence);

			package2.KP_KPH_PackageHeader = packageHeader2.PK;
			AssertEquals((ZShort)2, package2.KP_Sequence);

			// Reset sequence to 0
			package2.KP_Sequence = 0;

			package2.KP_KPH_PackageHeader = packageHeader2.PK;
			AssertEquals("Should not populate sequence if Package Header not be changed", (ZShort)0, package2.KP_Sequence);

			package1.KP_KPH_PackageHeader = packageHeader1.PK;
			AssertEquals("Sequence not be changed if Package Header not be changed", (ZShort)1, package1.KP_Sequence);

			package1.KP_KPH_PackageHeader = ZGuid.Empty;
			AssertEquals(packageSequenceType != PackageSequenceType.Standard ? (ZShort)0 : (ZShort)1, package1.KP_Sequence);
		}

		#endregion

		#endregion

		#region TestPackageIDHasChanges

		public void TestPackageIDHasChanges()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PKG");
			Factory.Save();

			AssertEquals("Precondition", false, package.PackageIDHasChanges);
			package.KP_PackageID = "123";
			AssertEquals("When PackageID changed from empty to 123, a new PackageID object gets created.", true, package.PackageIDHasChanges);
			Factory.Save();
			AssertEquals(false, package.PackageIDHasChanges);

			package.KP_PackageID = "345";
			AssertEquals("When PkgPackageHeader.KPH_PackageID value has changed - PackageIDHasChanges must become true.", true, package.PackageIDHasChanges);
			package.KP_PackageID = "123";
			AssertEquals(false, package.PackageIDHasChanges);

			package.KP_PackageID = "";
			AssertEquals("When PackageID gets deleted - PackageIDHasChanges must become true.", true, package.PackageIDHasChanges);
		}

		#endregion

		#region TestOriginalPackageID

		public void TestOriginalPackageID()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var package1 = Factory.New<PkgPackage>();
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package1.KP_F3_NKPackType = "PKG";
			AssertEquals("", package1.OriginalPackageID);

			var package2 = Factory.New<PkgPackage>();
			package2.KP_KJ_ParentPackageJob = packageJob.PK;
			package2.KP_PackageID = "hello";
			package2.KP_F3_NKPackType = "PKG";
			AssertEquals("", package2.OriginalPackageID);

			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var package1_2 = factory2.Load<PkgPackage>(package1.PK);
			AssertEquals("", package1_2.OriginalPackageID);

			var package2_2 = factory2.Load<PkgPackage>(package2.PK);
			AssertEquals("hello", package2_2.OriginalPackageID);

			var factory3 = new BusinessObjectFactory();
			var package2_3 = factory3.Load<PkgPackage>(package2.PK);
			package2_3.KP_PackageID = "What";
			AssertEquals("hello", package2_3.OriginalPackageID);

			package2_3.KP_PackageID = "";
			package2_3.KP_PackageID = "How";
			AssertEquals("hello", package2_3.OriginalPackageID);
		}

		#endregion

		#region TestCartonGroupAndSize

		public void TestCartonGroupAndSize()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("PKG");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");
			AssertEquals("Precondition", "", package1.CartonGroupAndSize);
			AssertEquals("Precondition", "", package2.CartonGroupAndSize);
			AssertEquals("Should be read only as this field is only supposed to be set during Cartonisation.", true, package1.CartonGroupAndSizeInfo.ReadOnly);
			AssertEquals("Should be read only as this field is only supposed to be set during Cartonisation.", true, package2.CartonGroupAndSizeInfo.ReadOnly);
			AssertEquals("MaxLength should be less than or equal to GenAddOnColumn Data MaxLength.", true, package2.CartonGroupAndSizeInfo.MaxLength <= GenAddOnColumnSchema.XA_Data.MaxLength);
			AssertEquals("Should not have created any GenAddOnColumn rows.", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageSchema.Constants.Prefix)).Length);

			package1.CartonGroupAndSize = "TEST1";
			AssertEquals("Should be set.", "TEST1", package1.CartonGroupAndSize);
			AssertEquals("Should not be set.", "", package2.CartonGroupAndSize);

			package2.CartonGroupAndSize = "TEST2";
			AssertEquals("Should *not* change.", "TEST1", package1.CartonGroupAndSize);
			AssertEquals("Should be set.", "TEST2", package2.CartonGroupAndSize);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var package1_InOtherFactory = otherFactory.Load<PkgPackage>(package1.PK);
			var package2_InOtherFactory = otherFactory.Load<PkgPackage>(package2.PK);
			AssertEquals("Should persist.", "TEST1", package1_InOtherFactory.CartonGroupAndSize);
			AssertEquals("Should persist.", "TEST2", package2_InOtherFactory.CartonGroupAndSize);
			AssertEquals("Should be read only as this field is only supposed to be set during Cartonisation.", true, package1_InOtherFactory.CartonGroupAndSizeInfo.ReadOnly);
			AssertEquals("Should be read only as this field is only supposed to be set during Cartonisation.", true, package2_InOtherFactory.CartonGroupAndSizeInfo.ReadOnly);
		}

		#endregion

		#region TestIsSentToRTUS

		public void TestIsSentToRTUS()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package1 = packageJob.Packages.AddNew("PKG");
			var package2 = packageJob.Packages.AddNew("PKG");
			AssertEquals("Precondition", false, package1.IsSentToRTUS);
			AssertEquals("Precondition", false, package2.IsSentToRTUS);
			AssertEquals("Should be read only as this field is only supposed to be set during Carrier Label Booking.", true, package1.IsSentToRTUSInfo.ReadOnly);
			AssertEquals("Should be read only as this field is only supposed to be set during Carrier Label Booking.", true, package2.IsSentToRTUSInfo.ReadOnly);
			AssertEquals("Should not have created any GenAddOnColumn rows.", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageSchema.Constants.Prefix)).Length);

			package1.IsSentToRTUS = true;
			AssertEquals("Should be set.", true, package1.IsSentToRTUS);
			AssertEquals("Should not be set.", false, package2.IsSentToRTUS);

			package2.IsSentToRTUS = false;
			AssertEquals("Should *not* change.", true, package1.IsSentToRTUS);
			AssertEquals("Should be set.", false, package2.IsSentToRTUS);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var package1_InOtherFactory = otherFactory.Load<PkgPackage>(package1.PK);
			var package2_InOtherFactory = otherFactory.Load<PkgPackage>(package2.PK);
			AssertEquals("Should persist.", true, package1_InOtherFactory.IsSentToRTUS);
			AssertEquals("Should persist.", false, package2_InOtherFactory.IsSentToRTUS);
		}

		#endregion

		#region TestRTUSBookedType

		public void TestRTUSBookedType()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = packageJob.Packages.AddNew("PKG");
			AssertEquals("Precondition", "", package.RTUSBookedType);
			AssertEquals("RTUSBookedType should be read only as this field is only supposed to be set during Carrier Label Booking.", true, package.RTUSBookedTypeInfo.ReadOnly);
			AssertEquals("RTUSBookedType should not have created any GenAddOnColumn rows.", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageSchema.Constants.Prefix)).Length);

			package.RTUSBookedType = "BLK";
			AssertEquals("RTUSBookedType should be set to BLK.", "BLK", package.RTUSBookedType);
			AssertEquals("RTUSBookedType should have created one GenAddOnColumn row.", 1, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageSchema.Constants.Prefix)).Length);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var package_InOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			AssertEquals("RTUSBookedType should persist.", "BLK", package_InOtherFactory.RTUSBookedType);
		}

		#endregion

		#region TestRTUSLabelPrinterPK

		public void TestRTUSLabelPrinterPK()
		{
			var printer1 = Factory.New<IStmPrintQueue>();
			printer1.QueueName = "PRINTER1";
			printer1.SQ_AllowPrinting = true;

			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			AssertEquals("RTUS Label Printer should be empty if there is no default printer created.", ZGuid.Empty, package.RTUSLabelPrinterPK);

			package.IsSentToRTUS = true;
			package.RTUSLabelPrinterPK = printer1.PK;
			AssertEquals("RTUS Label Printer should return printer1 PK.", printer1.PK, package.RTUSLabelPrinterPK);

			var printer2 = Factory.New<IStmPrintQueue>();
			printer2.QueueName = "PRINTER2";
			printer2.SQ_AllowPrinting = true;

			package.RTUSLabelPrinterPK = printer2.PK;
			AssertEquals("RTUS Label Printer should return printer2 PK.", printer2.PK, package.RTUSLabelPrinterPK);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			AssertEquals("Printer PK should persist.", printer2.PK, packageInNewFactory.RTUSLabelPrinterPK);
		}

		#endregion

		// calculated

		#region TestPackageSequenceString

		public void TestPackageSequenceString()
		{
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			AssertEquals("0001", package1.PackageSequenceString);

			var package2 = Data.PackageJob.Packages.AddNew("BOX", "P2");
			AssertEquals("0002", package2.PackageSequenceString);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			package.KP_F3_NKPackType = "";
			AssertEquals("???", package.Description);

			package.KP_F3_NKPackType = "BOX";
			AssertEquals("Box", package.Description);

			package.KP_PackageQty = 2;
			AssertEquals("Boxes", package.Description);

			package.KP_F3_NKPackType = "xXx";
			AssertEquals("xXx", package.Description);
		}

		public void TestDescription_Chinese()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			var packageBox = Data.PackageJob.Packages.AddNew(Constants.PkgUnit.Box);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("Preconditon: Single Pallet in Chinese.", "托盘", package.Description);
				AssertEquals("Preconditon: Single Box in Chinese.", "盒", packageBox.Description);

				package.KP_PackageQty = 2;
				AssertEquals("Plural Pallets in Chinese.", "托盘", package.Description);

				packageBox.KP_PackageQty = 2;
				AssertEquals("Plural Boxes in Chinese.", "盒", packageBox.Description);
			}
		}

		#endregion

		#region TestPackageIdDescription

		public void TestPackageIdDescription()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals("Package ID:", package.PackageIdDescription);

			package.KP_F3_NKPackType = "CNT";
			AssertEquals("Container ID:", package.PackageIdDescription);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsContainer

		public void TestIsContainer()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals(false, package.IsContainer);

			package.KP_F3_NKPackType = "CNT";
			AssertEquals(true, package.IsContainer);

			package.KP_F3_NKPackType = "PLT";
			AssertEquals(false, package.IsContainer);

			package.KP_F3_NKPackType = "cNt";
			AssertEquals(true, package.IsContainer);
		}

		#endregion

		#region TestIsBookedViaCarrier

		public void TestIsBookedViaCarrier()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals(false, package.IsBookedViaCarrier);

			package.KP_F3_NKPackType = "CNT";
			AssertEquals(false, package.IsBookedViaCarrier);

			package.IsSentToRTUS = true;
			AssertEquals(true, package.IsBookedViaCarrier);

			package.IsSentToRTUS = false;
			AssertEquals(false, package.IsBookedViaCarrier);
		}

		#endregion

		#region TestIsPackageIdValid

		public void TestIsPackageIdValid()
		{
			Data.CreatePackingData();

			var packageWithUniqueID = Data.PackageJob.Packages.AddNew();
			var packageWithNoID = Data.PackageJob.Packages.AddNew();
			var packageWithDuplicateID1 = packageWithUniqueID.Packages.AddNew();
			var packageWithDuplicateID2 = packageWithNoID.Packages.AddNew();

			packageWithUniqueID.KP_PackageID = "CNT-1";
			packageWithNoID.KP_PackageID = "";
			packageWithDuplicateID1.KP_PackageID = "PLT-1";
			packageWithDuplicateID2.KP_PackageID = "plt-1";

			AssertEquals("Unique Package ID should be valid.", true, packageWithUniqueID.IsPackageIdValid);
			AssertEquals("Empty Package ID should be valid.", true, packageWithNoID.IsPackageIdValid);
			AssertEquals("Duplicate Package ID should be invalid.", false, packageWithDuplicateID1.IsPackageIdValid);
			AssertEquals("Duplicate Package ID should be invalid.", false, packageWithDuplicateID2.IsPackageIdValid);
		}

		#endregion

		#region TestIsPackageIdValidSSCCBarCode

		public void TestIsPackageIdValidSSCCBarCode()
		{
			Data.CreatePackingData();

			var packageWithNoID = Data.PackageJob.Packages.AddNew();
			var packageWithID = Data.PackageJob.Packages.AddNew();
			var packageWithSSCCBarCode = Data.PackageJob.Packages.AddNew();

			packageWithID.KP_PackageID = "016765167700000010"; // invalid check-digit
			packageWithSSCCBarCode.KP_PackageID = "016765167700000011";

			AssertEquals(false, packageWithNoID.IsPackageIdValidSSCCBarCode);
			AssertEquals(false, packageWithID.IsPackageIdValidSSCCBarCode);
			AssertEquals(true, packageWithSSCCBarCode.IsPackageIdValidSSCCBarCode);
		}

		#endregion

		#region TestIsOuter

		public void TestIsOuter()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			AssertEquals(true, pallet.IsOuter);

			var box = pallet.Packages.AddNew("BOX");
			AssertEquals(false, box.IsOuter);
		}

		#endregion

		#region TestIsClosedOrReleased

		public void TestIsClosedOrReleased()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			AssertEquals(false, package.IsClosedOrReleased);

			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(true, package.IsClosedOrReleased);
			package.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			package.KP_IsReleasedViaJob = true;
			AssertEquals(true, package.IsClosedOrReleased);
			package.KP_IsReleasedViaJob = false; // cleanup

			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(true, package.IsClosedOrReleased);
			package.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup
		}

		#endregion

		#region TestIsValidCandidateForInner

		public void TestIsValidCandidateForInner()
		{
			ZString errorMessage;

			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew("BOX");
			AssertEquals(true, package.IsValidCandidateForInner(out errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);

			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, package.IsValidCandidateForInner(out errorMessage));
			AssertEquals("The selected Box is already Released and cannot be packed into another Package.", errorMessage);
			package.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			package.KP_IsReleasedViaJob = true;
			AssertEquals(false, package.IsValidCandidateForInner(out errorMessage));
			AssertEquals("The selected Box is already Released and cannot be packed into another Package.", errorMessage);
			package.KP_IsReleasedViaJob = false; // cleanup

			package.KP_F3_NKPackType = "CNT";
			AssertEquals(false, package.IsValidCandidateForInner(out errorMessage));
			AssertEquals("Cannot Pack a Container into another Package.", errorMessage);
		}

		public void TestIsValidCandidateForInner_MultiplePackages()
		{
			ZString errorMessage;

			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew("BOX");
			AssertEquals(true, package.IsValidCandidateForInner(out errorMessage, useMessageAppropriateForMultiplePackages: true));
			AssertEquals(true, errorMessage.IsEmpty);

			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, package.IsValidCandidateForInner(out errorMessage, useMessageAppropriateForMultiplePackages: true));
			AssertEquals("One or more Selected Packages are already Released and cannot be packed into another Package.", errorMessage);
			package.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			package.KP_IsReleasedViaJob = true;
			AssertEquals(false, package.IsValidCandidateForInner(out errorMessage, useMessageAppropriateForMultiplePackages: true));
			AssertEquals("One or more Selected Packages are already Released and cannot be packed into another Package.", errorMessage);
			package.KP_IsReleasedViaJob = false; // cleanup

			package.KP_F3_NKPackType = "CNT";
			AssertEquals(false, package.IsValidCandidateForInner(out errorMessage, useMessageAppropriateForMultiplePackages: true));
			AssertEquals("Cannot Pack a Container into another Package.", errorMessage);
		}

		#endregion

		// temperature

		#region TestIsNotRequiringTemperatureControl

		public void TestIsNotRequiringTemperatureControl()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals("Default should be no temperature control.", true, package.IsNotRequiringTemperatureControl);

			// turn on temperature control via the KP property
			package.KP_RequiresTemperatureControl = true;
			package.KP_RequiredTemperatureMinimum = 7m;
			package.KP_RequiredTemperatureMaximum = 14m;
			package.KP_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			AssertEquals("Precondition", 7m, package.KP_RequiredTemperatureMinimum);
			AssertEquals("Precondition", 14m, package.KP_RequiredTemperatureMaximum);
			AssertEquals("Precondition", Constants.Temperature.Centigrade, package.KP_RequiredTemperatureUnit);
			AssertEquals(false, package.IsNotRequiringTemperatureControl);

			// turn off temperature control via the flag
			package.IsNotRequiringTemperatureControl = true;
			AssertEquals(false, package.KP_RequiresTemperatureControl);
			AssertEquals(0m, package.KP_RequiredTemperatureMinimum);
			AssertEquals(0m, package.KP_RequiredTemperatureMaximum);
			AssertEquals("", package.KP_RequiredTemperatureUnit);
		}

		#endregion

		#region TestIsChillerRequired

		public void TestIsChillerRequired()
		{
			ITemperatureSettingsExtensionsTest.AssertIsChillerAndSetIsChiller(Factory.New<PkgPackage>());
		}

		#endregion

		#region TestIsFreezerRequired

		public void TestIsFreezerRequired()
		{
			ITemperatureSettingsExtensionsTest.AssertIsFreezerAndSetIsFreezer(Factory.New<PkgPackage>());
		}

		#endregion

		#region TestIsLabelPrinted

		public void TestIsLabelPrinted()
		{
			var package = Factory.New<PkgPackage>();

			AssertEquals(false, package.IsLabelPrinted);

			package.IsLabelPrinted = true;
			AssertEquals(true, package.IsLabelPrinted);

			package.IsLabelPrinted = false;
			AssertEquals(false, package.IsLabelPrinted);
		}

		#endregion

		#endregion

		//

		#region TestDataRefresh

		public void TestDataRefresh()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			Factory.Save();

			int packageJobChangedHitCount = 0;
			package.KP_KJ_ParentPackageJobInfo.ValueChanged += delegate
			{ packageJobChangedHitCount++; };

			var otherFactory = new BusinessObjectFactory();
			var packageInOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			packageInOtherFactory.KP_PackageID = "123";
			otherFactory.Save();
			AssertEquals(0, packageJobChangedHitCount);

			var packageJob = otherFactory.NewWithValidTestData<PkgPackageJob>();
			packageJob.HasChanges = true;
			packageInOtherFactory.KP_KJ_ParentPackageJob = packageJob.PK;
			AssertEquals(0, packageJobChangedHitCount);

			otherFactory.Save();
			AssertEquals(1, packageJobChangedHitCount);
		}

		#endregion

		#region TestFetchStrategy

		public void TestFetchStrategy()
		{
			AssertEquals(true, Factory.New<PkgPackage>().FetchStrategy is PkgPackageFetchStrategy);
		}

		#endregion

		#region TestAddNewPackage

		public void TestAddNewPackage()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();

			var package_PLT1 = package.Packages.AddNew();
			var package_PLT2 = package.Packages.AddNew();

			var package_PLT1_BOX1 = package_PLT1.Packages.AddNew();
			var package_PLT1_BOX2 = package_PLT1.Packages.AddNew();

			var package_PLT2_BOX1 = package_PLT2.Packages.AddNew();
			var package_PLT2_BOX2 = package_PLT2.Packages.AddNew();

			// top level
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package_PLT1, package_PLT2 }, package.Packages);

			// mid level
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package_PLT1_BOX1, package_PLT1_BOX2 }, package_PLT1.Packages);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package_PLT2_BOX1, package_PLT2_BOX2 }, package_PLT2.Packages);

			// bottom level
			AssertEquals(0, package_PLT1_BOX1.Packages.Count);
			AssertEquals(0, package_PLT1_BOX2.Packages.Count);
			AssertEquals(0, package_PLT2_BOX1.Packages.Count);
			AssertEquals(0, package_PLT2_BOX2.Packages.Count);
		}

		#endregion

		#region Pack / Unpack

		#region TestIsAvailableForPacking / TestIsAvailableForUnpacking

		public void TestIsAvailableForPacking()
		{
			AssertIsAvailable(isPacking: true);
		}

		public void TestIsAvailableForUnpacking()
		{
			AssertIsAvailable(isPacking: false);
		}

		void AssertIsAvailable(bool isPacking)
		{
			var verb = isPacking ? "Pack into" : "Unpack";

			Data.CreatePackingData();
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var box = pallet.Packages.AddNew("BOX");
			var keg = box.Packages.AddNew("KEG");

			ZString errorMessage;

			AssertEquals(true, isPacking ? box.IsAvailableForPacking(out errorMessage) : box.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);

			box.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, isPacking ? box.IsAvailableForPacking(out errorMessage) : box.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(string.Format("Cannot {0} the selected BOX because it is already Closed.", verb), errorMessage);
			box.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			box.KP_IsReleasedViaJob = true;
			AssertEquals(false, isPacking ? box.IsAvailableForPacking(out errorMessage) : box.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(string.Format("Cannot {0} the selected BOX because it is already Released via Job.", verb), errorMessage);
			box.KP_IsReleasedViaJob = false; // cleanup

			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, isPacking ? box.IsAvailableForPacking(out errorMessage) : box.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(string.Format("Cannot {0} the selected BOX because it is already Released.", verb), errorMessage);
			box.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			pallet.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, isPacking ? box.IsAvailableForPacking(out errorMessage) : box.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(string.Format("Cannot {0} the selected BOX because it is packed onto a PLT that is already Closed.", verb), errorMessage);

			// also test another level down to ensure we get the message correct
			AssertEquals(false, isPacking ? keg.IsAvailableForPacking(out errorMessage) : keg.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(string.Format("Cannot {0} the selected KEG because it is packed onto a PLT that is already Closed.", verb), errorMessage);
		}

		#endregion

		#region TestGetPackedQty

		public void TestGetPackedQty()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.Pack(Data.DummyLine1, 10m);
			package.Pack(Data.DummyLine1, 1.5m);
			package.Pack(Data.DummyLine2, 7m); // different line

			AssertEquals(11.5m, package.GetPackedQty(Data.DummyLine1));
			AssertEquals(7m, package.GetPackedQty(Data.DummyLine2));
			AssertEquals(0m, package.GetPackedQty(Data.DummyLine3));
		}

		#endregion

		#region TestIsPacked

		public void TestIsPacked()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			package.Pack(Data.DummyLine1, 10m);
			package.Pack(Data.DummyLine1, 1.5m);
			package.Pack(Data.DummyLine2, 7m); // different line

			AssertEquals(true, package.IsPacked(Data.DummyLine1));
			AssertEquals(true, package.IsPacked(Data.DummyLine2));
			AssertEquals(false, package.IsPacked(Data.DummyLine3));
		}

		#endregion

		#region TestIsPacked_PackableItem

		public void TestIsPacked_PackableItem()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var otherPackage = Data.PackageJob.Packages.AddNew("PLT");
			package.Pack(Data.DummyLine1, 10m);
			package.Pack(Data.DummyLine2, 7m);

			var packedItem1 = Data.DummyLine1.PackableItems.Single(p => p.Quantity == 10m);
			var packedItem2 = Data.DummyLine2.PackableItems.Single(p => p.Quantity == 7m);
			var unPackedItem = Data.DummyLine3.PackableItems.Single();

			AssertEquals(true, package.IsPacked(packedItem1));
			AssertEquals(true, package.IsPacked(packedItem2));
			AssertEquals(false, package.IsPacked(unPackedItem));
			AssertEquals(false, otherPackage.IsPacked(packedItem1));
			AssertEquals(false, otherPackage.IsPacked(packedItem2));
			AssertEquals(false, otherPackage.IsPacked(unPackedItem));
			AssertExceptionThrown<ArgumentNullException>(() => package.IsPacked((IPackableItem)null));
		}

		#endregion

		#region TestPack

		#region TestPack

		public void TestPack()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertExceptionThrown<ArgumentNullException>(() => package.Pack(null, 10m));

			// pack a line
			package.Pack(Data.DummyLine1, 12m);
			AssertEquals(1, package.PackedItemDivots.Count);
			AssertEquals(package.PackedItemDivots[0].KI_PackedQty, 12m);

			// pack more of the same line into the same package
			package.Pack(Data.DummyLine1, 8m);
			AssertEquals("Packing the line multiple times will not group the items.", 2, package.PackedItemDivots.Count);
			AssertEquals("Packing the line multiple times will not group the items.", 1, package.PackedItemDivots.Count(p => p.KI_PackedQty == 12m));
			AssertEquals("Packing the line multiple times will not group the items.", 1, package.PackedItemDivots.Count(p => p.KI_PackedQty == 8m));

			// pack a different line
			package.Pack(Data.DummyLine2, 25m);
			var packDivotForDummyLine2 = package.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2);
			AssertEquals(25m, packDivotForDummyLine2.KI_PackedQty);

			// pack the line into a child package
			var childPackage = package.Packages.AddNew();
			childPackage.Pack(Data.DummyLine1, 5m);
			AssertEquals(1, childPackage.PackedItemDivots.Count);
			AssertEquals(childPackage.PackedItemDivots[0].KI_PackedQty, 5m);
		}

		#endregion

		#region TestPack_WithMultiDivots

		public void TestPack_WithMultiDivots()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// pack a line
			var packedItem1 = package.Pack(Data.DummyLine1, 10m).Single();
			AssertEquals(1, package.PackedItemDivots.Count);
			AssertEquals(1, package.PackedItems.Count);
			var divot1 = package.PackedItemDivots.Single();
			AssertEquals(divot1.KI_PackedQty, 10m);
			AssertEquals("Precondition: Package weight should be 20 - 1 unit is 2KG", 20m, package.KP_Weight);
			AssertEquals("Precondition: Package Quantity on Wrapper should be 10", 10m, packedItem1.PackedQty);

			// pack more of the same line into the same package
			var packedItem2 = package.Pack(Data.DummyLine1, 5m).Single();
			AssertEquals("Precondition: As same parent, should return same wrapper for both.", packedItem1, packedItem2);
			AssertEquals(2, package.PackedItemDivots.Count);
			AssertEquals("Divots should be grouped into one wrapper.", 1, package.PackedItems.Count);
			AssertEquals("PackedQty should be 5.", package.PackedItemDivots.Single(d => d != divot1).KI_PackedQty, 5m);
			AssertEquals("Precondition: Package weight should be 30.", 20m + 10m, package.KP_Weight);
			AssertEquals("Precondition: Package Quantity on Wrapper should be 15", 15m, packedItem1.PackedQty);

			// pack a different line
			var packedItem3 = package.Pack(Data.DummyLine2, 5m).SingleOrDefault();
			AssertEquals("Precondition: Package weight is increased.", 20m + 10m + 10m, package.KP_Weight);
			AssertEquals(3, package.PackedItemDivots.Count);
			AssertEquals(2, package.PackedItems.Count);
			AssertNotEquals("Should not be the same wrapper.", packedItem3, packedItem2);

			// pack the line into a child package
			var childPackage = package.Packages.AddNew();
			childPackage.Pack(Data.DummyLine1, 5m);
			AssertEquals(childPackage.PackedItemDivots.Single().KI_PackedQty, 5m);
		}

		#endregion

		#region TestPack_CreatesPackedItems

		public void TestPack_CreatesPackedItems()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// pack a line
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			var itemDivot = package.PackedItemDivots.Single();
			AssertEquals(nameof(itemDivot.KI_PackedQty), 100m, itemDivot.KI_PackedQty);
			AssertEquals(nameof(itemDivot.KI_KP_Package), package.PK, itemDivot.KI_KP_Package);
			AssertEquals(nameof(itemDivot.KI_ParentID), Data.DummyPackableItemOnLine1.PK, itemDivot.KI_ParentID);
			AssertEquals(nameof(itemDivot.KI_ParentTableCode), Data.DummyPackableItemOnLine1.TablePrefix, itemDivot.KI_ParentTableCode);
			AssertEquals(nameof(packedItem.Key), Data.DummyPackableItemOnLine1.Key, packedItem.Key);
			AssertEquals(nameof(packedItem.PackableItemParent), Data.DummyLine1, packedItem.PackableItemParent);
			AssertEquals(nameof(packedItem.PackedQty), 100m, packedItem.PackedQty);
			AssertEquals("PackedItem", Data.DummyPackableItemOnLine1, packedItem.PackedItems.Single());
			AssertEquals(nameof(packedItem.ParentPackage), package, packedItem.ParentPackage);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem }, package.PackedItems);

			var otherPackedItem = package.Pack(Data.DummyLine2, 100m).Single();
			AssertContainsExactElementsInAnyOrder(new[] { packedItem, otherPackedItem }, package.PackedItems);

			var packableItem1 = (DummyPackableItem)Data.DummyLine3.PackableItems.Single();
			var packableItem2 = Data.DummyLine3.AddNewPackableItem();
			packableItem1.Quantity = 30m;
			packableItem2.Quantity = 70m;
			var packedItem1 = package.Pack(Data.DummyLine3, 80m).Single();

			var packableItem3 = Data.DummyLine3.PackableItems.Single(i => i.Quantity == 50m);
			var packedItem1Divot1 = package.PackedItemDivots.Single(d => d.PackedItem == packableItem1);
			var packedItem1Divot2 = package.PackedItemDivots.Single(d => d.PackedItem == packableItem3);
			AssertEquals("Items that can be fully Packed are Packed first.", Data.DummyLine3, packedItem1.PackableItemParent);
			AssertEquals("Items that can be fully Packed are Packed first.", 30m, packedItem1Divot1.KI_PackedQty);
			AssertEquals("Items that have more Qty than to Pack are split.", Data.DummyLine3, packedItem1.PackableItemParent);
			AssertEquals("Items that have more Qty than to Pack are split.", 50m, packedItem1Divot2.KI_PackedQty);
			AssertEquals("Items that have more Qty than to Pack are split.", false, package.IsPacked(packableItem2));
			AssertContainsExactElementsInAnyOrder(new[] { packedItem, otherPackedItem, packedItem1 }, package.PackedItems);
		}

		#endregion

		#region TestPack_PackableItem

		public void TestPack_PackableItem()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertExceptionThrown<ArgumentNullException>(() => package.Pack(null, Data.DummyLine1));
			AssertExceptionThrown<ArgumentNullException>(() => package.Pack(Data.DummyPackableItemOnLine1, null));
			AssertEquals("Precondition: Package has no weight.", 0m, package.KP_Weight);

			// pack a line
			package.Pack(Data.DummyPackableItemOnLine1, Data.DummyLine1);
			var packedItem = package.PackedItems.Typed.Single();
			var itemDivot = package.PackedItemDivots.Single();
			AssertEquals(nameof(itemDivot.KI_PackedQty), 100m, itemDivot.KI_PackedQty);
			AssertEquals(nameof(itemDivot.KI_KP_Package), package.PK, itemDivot.KI_KP_Package);
			AssertEquals(nameof(itemDivot.KI_ParentID), Data.DummyPackableItemOnLine1.PK, itemDivot.KI_ParentID);
			AssertEquals(nameof(itemDivot.KI_ParentTableCode), Data.DummyPackableItemOnLine1.TablePrefix, itemDivot.KI_ParentTableCode);
			AssertEquals(nameof(packedItem.Key), Data.DummyPackableItemOnLine1.Key, packedItem.Key);
			AssertEquals(nameof(packedItem.PackableItemParent), Data.DummyLine1, packedItem.PackableItemParent);
			AssertEquals(nameof(packedItem.PackedQty), 100m, packedItem.PackedQty);
			AssertEquals("PackedItem", Data.DummyPackableItemOnLine1, packedItem.PackedItems.Single());
			AssertEquals(nameof(packedItem.ParentPackage), package, packedItem.ParentPackage);
			AssertEquals("Package Weight should have increased.", 200m, package.KP_Weight);

			var otherPackedItem = package.Pack(Data.DummyLine2, 100m).Single();
			AssertContainsExactElementsInAnyOrder(new[] { packedItem, otherPackedItem }, package.PackedItems);
		}

		#endregion

		#region TestPack_AlreadyPackedItem

		public void TestPack_AlreadyPackedItem()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var item = Data.DummyPackableItemOnLine1;
			var parent = Data.DummyLine1;

			AssertEquals("Precondition: Item should not be packed.", false, package.IsPacked(item));
			package.Pack(item, parent);
			AssertEquals("Precondition: Item should be packed.", true, package.IsPacked(item));

			AssertExceptionThrown<InvalidOperationException>("Attempting to pack IPackableItem that is already packed", () => package.Pack(item, parent));
		}

		#endregion

		#region TestPack_DBHits

		public void TestPack_DBHits()
		{
			Data.CreatePackingData();

			for (int lineCounter = 0; lineCounter < 10; lineCounter++)
			{
				Data.PackageJob.Packages.AddNew();
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var dummyInOtherFactory = otherFactory.Load<DummyWithPacking>(Data.Dummy.PK);
			var packageJobInOtherFactory = otherFactory.Load<PkgPackageJob>(Data.PackageJob.PK);

			foreach (var package in packageJobInOtherFactory.Packages)
			{
				foreach (var line in dummyInOtherFactory.Lines)
				{
					foreach (var item in line.PackableItems)
					{
						package.Pack(item, line);
					}
				}
			}
			var expetedDbHits = new Dictionary<string, int>
			{
				{ "DummyBizo", 1 },
				{ "DummyDependentBizo", 4 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expetedDbHits, otherFactory);
		}

		#endregion

		#region TestPack_AddsWeightToPackage

		public void TestPack_AddsWeightToPackage()
		{
			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);

			Data.CreatePackingData();
			Data.DummyLine1.WeightUQ = Constants.Weight.Kilograms;
			Data.DummyLine2.WeightUQ = Constants.Weight.Pounds;
			Data.DummyLine3.WeightUQ = "";

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_WeightUQ = "";
			AssertEquals("Precondition", 0m, package.KP_Weight);
			AssertEquals("Precondition", "", package.KP_WeightUQ);

			// pack a line
			package.Pack(Data.DummyLine1, 10m);
			AssertEquals(20m, package.KP_Weight);
			AssertEquals("When packing items where the package has no Weight UQ, the default Weight UQ should be set.", Constants.Weight.Kilograms, package.KP_WeightUQ);

			// pack more of the same line
			package.Pack(Data.DummyLine1, 5m);
			AssertEquals(30m, package.KP_Weight);
			AssertEquals(Constants.Weight.Kilograms, package.KP_WeightUQ);

			// pack a line whose unit differs from the package unit
			package.Pack(Data.DummyLine2, 10m);
			AssertEquals("Packing should have converted the unit weight (LB) to the package weight (KG).", 39.072m, package.KP_Weight);
			AssertEquals(Constants.Weight.Kilograms, package.KP_WeightUQ);

			// pack a line with no weight UQ defined
			package.Pack(Data.DummyLine3, 10m);
			AssertEquals("Packing should not add any weight if the item has no Weight UQ defined.", 39.072m, package.KP_Weight);
		}

		#endregion

		#region TestPack_ConcurrentPackingOfSameItem

		public void TestPack_ConcurrentPackingOfSameItem()
		{
			// create package + 2 packableItems with same packableItemParent.
			var packingParent = Helper.CreatePackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			var packableItemParent = Helper.CreatePackableItemParent();
			var packableItem1 = Helper.CreatePackableItem(packableItemParent, 5m);
			var packableItem2 = Helper.CreatePackableItem(packableItemParent, 10m);
			Factory.Save();

			// pack same item in 2 different factories at same time
			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);
			package.Pack(packableItemParent, 5m);

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var packageJobInOtherFactory = otherFactory.Load<PkgPackageJob>(packageJob.PK);
			var packageInOtherFactory = Helper.CreatePackage(packageJobInOtherFactory, 1, Constants.PkgUnit.Box);
			var packableItemParentInOtherFactory = otherFactory.Load<DummyPackableItemParent>(packableItemParent.PK);
			packageInOtherFactory.Pack(packableItemParentInOtherFactory, 5m);
			otherFactory.Save();

			// continue packing in first factory after otherFactory is already saved
			package.Pack(packableItemParent, 10m);
			AssertExceptionThrown("The packableItem1 already packed and saved by another factory, should prevent saving of duplicate.", typeof(ZSaveException), () => Factory.Save());
		}

		#endregion

		#endregion

		#region TestUnpack

		public void TestUnpack()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// pack a few lines
			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = package.Pack_ForTesting(Data.DummyLine2, 5m);
			AssertEquals("Precondition: Package weight is increased.", 20m + 10m, package.KP_Weight);
			AssertEquals("Precondition: Has two Packable Items.", 2, Data.DummyLine1.PackableItems.Count());
			AssertEquals("Precondition: Has two Packable Items.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 10m));
			AssertEquals("Precondition: Has two Packable Items.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 90m));

			// don't allow a negative unpack to increase the qty packed
			package.Unpack(packedItem1, -5m);
			AssertEquals(10m, package.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty);
			AssertEquals(10m, packedItem1.PackedQty);

			var packableItem1 = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 10m);

			// partially unpack
			package.Unpack(packedItem1, 3m); // 7 left
			package.Unpack(packedItem2, 4m); // 1 left
			AssertEquals(7m, package.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty);
			AssertEquals(1m, package.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty);
			AssertEquals(7m, packedItem1.PackedQty);
			AssertEquals(1m, packedItem2.PackedQty);
			AssertEquals("Package weight is decreased.", 14m + 2m, package.KP_Weight);
			AssertEquals("Should have split off 3 Units on Packable Item.", 7m, packableItem1.Quantity);
			AssertEquals("Has two Packable Items.", 2, Data.DummyLine1.PackableItems.Count());
			AssertEquals("Should have merged Unpacked Packable Item.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 7m));
			AssertEquals("Should have merged Unpacked Packable Item.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 93m));

			// attempt to unpack more than is packed
			package.Unpack(packedItem1, 17m); // only 7 left, should not blow up
			AssertEquals(true, packedItem1.IsDeleted);
			AssertEquals(1, package.PackedItemDivots.Count);
			AssertEquals("Package weight is decreased.", 2m, package.KP_Weight);
		}

		public void TestPartiallyUnpackWhenPackableItemParentHasBeenDeleted_ExpectNoExceptions()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 10m);
			AssertEquals(10m, package.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty);
			AssertEquals(10m, packedItem1.PackedQty);

			Data.DummyLine1.Delete();

			AssertNoExceptionThrown(() => package.Unpack(packedItem1, 3m)); // 7 left
		}

		public void TestUnpackWithMultiDivots()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = package.Pack_ForTesting(Data.DummyLine1, 5m);
			var packedItem3 = package.Pack_ForTesting(Data.DummyLine2, 5m);

			AssertEquals("Precondition: As same parent, should return same wrapper for both.", packedItem1, packedItem2);
			AssertEquals("Precondition: PackedQty should be 15 in packedItems1", 15m, packedItem1.PackedQty);
			AssertEquals("Precondition: Package weight is increased.", 20m + 10m + 10m, package.KP_Weight);
			AssertEquals("Precondition: Has three Packable Items.", 3, Data.DummyLine1.PackableItems.Count());
			AssertEquals("Precondition: Has three Packable Items.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 10m));
			AssertEquals("Precondition: Has three Packable Items.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 5m));
			AssertEquals("Precondition: Has three Packable Items.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 85m));

			// don't allow a negative unpack to increase the qty packed
			package.Unpack(packedItem1, -5m);
			AssertEquals(true, package.PackedItemDivots.Any(d => d.KI_PackedQty == 10m));
			AssertEquals(true, package.PackedItemDivots.Any(d => d.KI_PackedQty == 5m));
			AssertEquals(15m, packedItem1.PackedQty);

			var packableItemToSplit = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 10m);

			// partially unpack
			package.Unpack(packedItem1, 3m); // Was 15 before and 10 + 2 items left, as contains 2 divots - reduce 3 from divot with qty 5
			package.Unpack(packedItem2, 4m); // Was 12 before and now 8 items left, with 1 divot. wrapper recreated during split/remerge
			package.Unpack(packedItem3, 3m); // Was 5 before and now 2 items left

			AssertEquals(2, package.PackedItemDivots.Count);
			AssertNotNull(package.PackedItemDivots.Single(d => d.KI_PackedQty == 8m));
			AssertNotNull(package.PackedItemDivots.Single(d => d.KI_PackedQty == 2m));
			AssertEquals(8m, packedItem1.PackedQty);
			AssertEquals(2m, packedItem3.PackedQty);
			AssertEquals("Package weight is decreased.", 16m + 4m, package.KP_Weight);
			AssertEquals("Should have split off on Packable Item.", 8m, packableItemToSplit.Quantity);
			AssertEquals("Has two Packable Items.", 2, Data.DummyLine1.PackableItems.Count());
			AssertEquals("Should have merged Unpacked Packable Item.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 8m));
			AssertEquals("Should have merged Unpacked Packable Item.", 1, Data.DummyLine1.PackableItems.Count(i => i.Quantity == 92m));

			// attempt to unpack more than is packed
			var newWrapper = package.PackedItems.Cast<PkgPackageItemDivotsWrapper>().Single(d => d.PackedQty == 8); // loading as wrapper re-created
			package.Unpack(newWrapper, 17m); // only 8 left, should not blow up
			AssertEquals(true, newWrapper.IsDeleted);
			AssertEquals(2m, package.PackedItemDivots.Single().KI_PackedQty);
			AssertEquals("Package weight is decreased.", 4m, package.KP_Weight);
		}

		public void TestUnpack_PartiallyUnpackFullyPackedItem()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// fully pack DummyPackableItemOnLine1
			var packableItem1 = Data.DummyPackableItemOnLine1;
			package.Pack(packableItem1, Data.DummyLine1);
			AssertEquals("Precondition: Item is packed.", true, package.IsPacked(packableItem1));

			// partially unpack
			var packedItem = package.PackedItems.Typed.Single();
			package.Unpack(packedItem, 40m);
			AssertEquals("Should reduced Packed Qty.", 60m, packedItem.PackedQty);
			AssertEquals("Should reduced Packed Qty.", packableItem1, packedItem.PackedItems.Single());
			AssertEquals("Should reduced Packed Qty.", 60m, packableItem1.Quantity);

			var splitPackableItem = Data.DummyLine1.PackableItems.Single(i => i != packableItem1);
			AssertEquals("Should have split off Unpacked Qty.", 40m, splitPackableItem.Quantity);
		}

		public void TestUnpack_DeletesParentWithNoID()
		{
			Data.CreatePackingData();
			var pallet1 = Data.PackageJob.Packages.AddNew("PLT");
			var pallet2 = Data.PackageJob.Packages.AddNew("PLT");
			var pallet3 = Data.PackageJob.Packages.AddNew("PLT");
			var pallet4 = Data.PackageJob.Packages.AddNew("PLT");

			// pack a few lines
			var packedItem1 = pallet1.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = pallet2.Pack_ForTesting(Data.DummyLine2, 10m);
			var packedItem3 = pallet3.Pack_ForTesting(Data.DummyLine3, 10m);
			var packedItem4 = pallet4.Pack_ForTesting(Data.DummyLine3, 10m);

			// add an ID to pallet2, add and a BOX to pallet3, pack another item into pallet4
			pallet2.KP_PackageID = "abc";
			pallet3.Packages.AddNew("BOX");
			pallet4.Pack(Data.DummyLine1, 10m);

			// unpack all items
			pallet1.Unpack(packedItem1, 10m);
			AssertEquals("Pallet 1 was empty and should have been auto-deleted.", true, pallet1.IsDeleted);

			pallet2.Unpack(packedItem2, 10m);
			AssertEquals("Pallet 2 was empty but had an ID, therefore it should not have been auto-deleted.", false, pallet2.IsDeleted);

			pallet3.Unpack(packedItem3, 10m);
			AssertEquals("Pallet 3 contained a BOX and therefore it should not have been auto-deleted.", false, pallet3.IsDeleted);

			pallet4.Unpack(packedItem4, 10m);
			AssertEquals("Pallet 4 contained another item and therefore it should not have been auto-deleted.", false, pallet4.IsDeleted);
		}

		public void TestUnpack_DeletesParentWithID_WTF()
		{
			Data.CreatePackingData();
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			pallet.KP_PackageID = "abc";

			// pack then unpack an item
			var packedItem1 = pallet.Pack_ForTesting(Data.DummyLine1, 10m);
			pallet.Unpack(packedItem1, 10m);
			AssertEquals("Empty Pallet has an ID and should *not* have been deleted.", false, pallet.IsDeleted);

			// pack then unpack an item but instruct for deletion of Package
			var packedItem2 = pallet.Pack_ForTesting(Data.DummyLine1, 10m);
			pallet.Unpack(packedItem2, 10m, removeEmptyPackageEvenWithID: true);
			AssertEquals("Empty Pallet has an ID but removeEmptyPackageEvenWithID override should have allowed delete.", true, pallet.IsDeleted);
		}

		public void TestUnpack_PackedItemsWithEmptyKey()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			((DummyPackableItem)Data.DummyLine1).Key = EmptyKey.Instance;
			((DummyPackableItem)Data.DummyLine2).Key = EmptyKey.Instance;
			((DummyPackableItem)Data.DummyLine3).Key = EmptyKey.Instance;
			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = package.Pack_ForTesting(Data.DummyLine2, 5m);
			var packedItem3 = package.Pack_ForTesting(Data.DummyLine3, 15m);

			package.Unpack(packedItem1, 10m);
			AssertEquals("2 packed items", 2, package.PackedItems.Count);
			AssertContainsExactElementsInAnyOrder("2 packed items", new[] { packedItem2, packedItem3 }, package.PackedItems);

			package.Unpack(packedItem3, 8m);
			AssertEquals("2 packed items", 2, package.PackedItems.Count);
			AssertContainsExactElementsInAnyOrder("2 packed items", new[] { packedItem2, packedItem3 }, package.PackedItems);

			package.Unpack(packedItem3, 15m);
			AssertEquals("1 packed items", 1, package.PackedItems.Count);
			AssertEquals("1 packed items", packedItem2, package.PackedItems[0]);
		}

		#endregion

		#region TestUnpackAndRepack

		public void TestUnpackAndRepack()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			var packedItem1 = package.Pack(Data.DummyLine1, 10m).Single();
			var packedItem2 = package.Pack(Data.DummyLine1, 20m).Single();
			AssertEquals("Precondition: As same parent, should return same wrapper for both.", packedItem1, packedItem2);

			var packedItem3 = package.Pack(Data.DummyLine2, 5m).Single();

			package.Unpack(packedItem1, 10m);
			package.Unpack(packedItem1, 20m);
			package.Unpack(packedItem3, 5m);
			AssertEquals("Should be no Packed Items.", 0, package.PackedItems.Count);

			var newPackedItem1 = package.Pack(Data.DummyLine1, 10m).Single();
			var newPackedItem2 = package.Pack(Data.DummyLine1, 20m).Single();
			var newPackedItem3 = package.Pack(Data.DummyLine2, 5m).Single();

			AssertEquals("Precondition: As same parent, should return same wrapper for both.", newPackedItem1, newPackedItem2);
			AssertContainsExactElementsInAnyOrder(new[] { newPackedItem1, newPackedItem3 }, package.PackedItems);
		}

		#endregion

		#region TestUnpackAndRepack_LoadPackedItemsFirst

		public void TestUnpackAndRepack_LoadPackedItemsFirst()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			var packedItem1 = package.Pack(Data.DummyLine1, 10m).Single();
			var packedItem2 = package.Pack(Data.DummyLine1, 20m).Single();
			AssertEquals("Precondition: As same parent, should return same wrapper for both.", packedItem1, packedItem2);
			AssertEquals("Precondition: Wrapper's PackedQty should be 30", 30m, packedItem1.PackedQty);
			var packedItem3 = package.Pack(Data.DummyLine2, 5m).Single();
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1, packedItem3 }, package.PackedItems);

			package.Unpack(packedItem1, 10m);
			package.Unpack(packedItem1, 20m);
			package.Unpack(packedItem3, 5m);
			AssertEquals("Should be no Packed Items.", 0, package.PackedItems.Count);

			var newPackedItem1 = package.Pack(Data.DummyLine1, 10m).Single();
			var newPackedItem2 = package.Pack(Data.DummyLine1, 20m).Single();
			var newPackedItem3 = package.Pack(Data.DummyLine2, 5m).Single();

			AssertEquals("Precondition: As same parent, should return same wrapper for both.", newPackedItem1, newPackedItem2);
			AssertContainsExactElementsInAnyOrder(new[] { newPackedItem1, newPackedItem3 }, package.PackedItems);
		}

		#endregion

		#region TestUnpackReducesWeightFromPackage

		public void TestUnpackReducesWeightFromPackage()
		{
			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);

			Data.CreatePackingData();
			Data.DummyLine1.WeightUQ = Constants.Weight.Kilograms;
			Data.DummyLine2.WeightUQ = Constants.Weight.Pounds;
			Data.DummyLine3.WeightUQ = "";

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_WeightUQ = "";
			AssertEquals("Precondition", 0m, package.KP_Weight);
			AssertEquals("Precondition", "", package.KP_WeightUQ);

			// pack 3 lines with Weight UQs of KG + LB + "".
			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 15m);
			var packedItem2 = package.Pack_ForTesting(Data.DummyLine2, 10m);
			var packedItem3 = package.Pack_ForTesting(Data.DummyLine3, 10m);
			AssertEquals("Precondition", 39.072m, package.KP_Weight);
			AssertEquals("Precondition", Constants.Weight.Kilograms, package.KP_WeightUQ);

			// partially unpack line 1 (KG)
			package.Unpack(packedItem1, 5m);
			AssertEquals(29.072m, package.KP_Weight);

			// partially unpack line 2 (LB)
			package.Unpack(packedItem2, 5m);
			AssertEquals(24.536m, package.KP_Weight);

			// partially unpack line 2 (no UQ);
			package.Unpack(packedItem3, 5m);
			AssertEquals(24.536m, package.KP_Weight);

			// fully unpack line 1 (KG), attempting to over-unpack
			package.Unpack(packedItem1, 100m);
			AssertEquals(4.536m, package.KP_Weight);
		}

		#endregion

		#endregion

		#region Booked Dimensions

		public void TestBookedDimensions()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			Factory.Save();

			AssertNull("Saving a package without touching the booked dimensions should not create a new booked dimension record.", Factory.LoadTop1<PkgPackageBookedDetail>(new ZQuery()));

			package.BookedDimensions.KPB_DimensionUQ = Constants.Length.Metres;
			package.BookedDimensions.KPB_WeightUQ = Constants.Weight.Kilograms;
			package.BookedDimensions.KPB_VolumeUQ = Constants.Volume.CubicMetres;
			package.BookedDimensions.KPB_PackageQty = 5;
			package.BookedDimensions.KPB_Length = 2.5m;
			package.BookedDimensions.KPB_Width = 1.5m;
			package.BookedDimensions.KPB_Height = 0.5m;
			package.BookedDimensions.KPB_Weight = 1.875m;
			package.BookedDimensions.KPB_Volume = 1.875m;

			Factory.Save();

			AssertPackageBookedDimensions(
				package,
				length: 2.5m,
				width: 1.5m,
				height: 0.5m,
				weight: 1.875m,
				dimUQ: Constants.Length.Metres,
				weightUQ: Constants.Weight.Kilograms,
				volume: 1.875m,
				volumeUQ: Constants.Volume.CubicMetres
			);

			var anotherFactory = Factory.CreateNewFactory();
			var packageInAnotherFactory = anotherFactory.Load<PkgPackage>(package.PK);

			AssertPackageBookedDimensions(
				packageInAnotherFactory,
				length: 2.5m,
				width: 1.5m,
				height: 0.5m,
				weight: 1.875m,
				dimUQ: Constants.Length.Metres,
				weightUQ: Constants.Weight.Kilograms,
				volume: 1.875m,
				volumeUQ: Constants.Volume.CubicMetres
			);
		}

		void AssertPackageBookedDimensions(PkgPackage package, decimal length, decimal width, decimal height, decimal weight, string dimUQ, string weightUQ, decimal volume, string volumeUQ)
		{
			CombineAssertions("PkgPackageBookedDetail (PkgPackage Booked Dimensions)", () =>
			{
				AssertEquals("KPB_KP_Package", package.PK, package.BookedDimensions.KPB_KP_Package);

				AssertEquals("KPB_Length", length, package.BookedDimensions.KPB_Length);
				AssertEquals("KPB_Width", width, package.BookedDimensions.KPB_Width);
				AssertEquals("KPB_Height", height, package.BookedDimensions.KPB_Height);
				AssertEquals("KPB_Weight", weight, package.BookedDimensions.KPB_Weight);

				AssertEquals("KPB_DimensionUQ", dimUQ, package.BookedDimensions.KPB_DimensionUQ);
				AssertEquals("KPB_WeightUQ", weightUQ, package.BookedDimensions.KPB_WeightUQ);

				AssertEquals("KPB_Volume", volume, package.BookedDimensions.KPB_Volume);
				AssertEquals("KPB_VolumeUQ", volumeUQ, package.BookedDimensions.KPB_VolumeUQ);
			});
		}

		#endregion

		#region Screenings

		public void TestScreenings()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(false, package.IsRegisteredEditableChildObject(package.Screenings));
			AssertEquals("package should have 0 screenings", 0, package.Screenings.Count);

			var screening = Helper.CreatePackageScreening(package, "ABC");
			screening.KPS_GS_NKScreenedBy = "BOB";
			AssertEquals("package should have 1 screening", 1, package.Screenings.Count);

			var packageScreening = package.Screenings.Single();
			AssertEquals("Screened By should be BOB", "BOB", packageScreening.KPS_GS_NKScreenedBy);
			AssertEquals("Screening Method should be ABC", "ABC", packageScreening.KPS_Method);
			AssertEquals("Screening Result should be failed", false, packageScreening.KPS_Passed);
		}

		public void TestLatestScreening()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: package should have zero screenings", 0, package.Screenings.Count);

			var screening1 = Helper.CreatePackageScreening(package, "1st");
			var screening2 = Helper.CreatePackageScreening(package, "2nd");
			var screening3 = Helper.CreatePackageScreening(package, "3rd");
			screening1.KPS_Time = DateTime.Today;
			screening2.KPS_Time = DateTime.Today.AddDays(-1);
			screening3.KPS_Time = DateTime.Today.AddDays(-2);

			AssertEquals("Precondition: package should have three screenings", 3, package.Screenings.Count);
			AssertEquals("Latest screening should return the latest screening", screening1, package.LatestScreening);
		}

		public void TestLatestScreeningMethod()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: package should have zero screenings", 0, package.Screenings.Count);

			var screening1 = Helper.CreatePackageScreening(package, ScreeningMethods.Codes.CargoMetalDetection);
			var screening2 = Helper.CreatePackageScreening(package, ScreeningMethods.Codes.ExplosiveDetectionSystem);
			var screening3 = Helper.CreatePackageScreening(package, ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment);
			screening1.KPS_Time = DateTime.Today;
			screening2.KPS_Time = DateTime.Today.AddDays(-1);
			screening3.KPS_Time = DateTime.Today.AddDays(-2);

			AssertEquals("Precondition: package should have three screenings", 3, package.Screenings.Count);
			AssertEquals("Should return the latest screening method", ScreeningMethods.Descriptions.CargoMetalDetection, package.LatestScreeningMethod);
		}

		public void TestLatestScreeningResult()
		{
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: package should have zero screenings", 0, package1.Screenings.Count);

			var package1_screening1 = Helper.CreatePackageScreening(package1, ScreeningMethods.Codes.CargoMetalDetection, false);
			var package1_screening2 = Helper.CreatePackageScreening(package1, ScreeningMethods.Codes.CargoMetalDetection, true);
			var package1_screening3 = Helper.CreatePackageScreening(package1, ScreeningMethods.Codes.CargoMetalDetection, true);
			package1_screening1.KPS_Time = DateTime.Today;
			package1_screening2.KPS_Time = DateTime.Today.AddDays(-1);
			package1_screening3.KPS_Time = DateTime.Today.AddDays(-2);

			var package2 = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: package should have zero screenings", 0, package2.Screenings.Count);

			var package2_screening1 = Helper.CreatePackageScreening(package2, ScreeningMethods.Codes.CargoMetalDetection, true);
			var package2_screening2 = Helper.CreatePackageScreening(package2, ScreeningMethods.Codes.CargoMetalDetection, false);
			package2_screening1.KPS_Time = DateTime.Today;
			package2_screening2.KPS_Time = DateTime.Today.AddDays(-1);

			var package3 = Data.PackageJob.Packages.AddNew();

			AssertEquals("Precondition: package should have three screenings", 3, package1.Screenings.Count);
			AssertEquals("Precondition: package should have three screenings", 2, package2.Screenings.Count);
			AssertEquals("Should return the latest screening method", PkgPackageScreeningResults.Descriptions.FAI, package1.LatestScreeningResult);
			AssertEquals("Should return the latest screening method", PkgPackageScreeningResults.Descriptions.PAS, package2.LatestScreeningResult);
			AssertEquals("Should return the latest screening method", PkgPackageScreeningResults.Descriptions.NOT, package3.LatestScreeningResult);
		}

		public virtual void TestScreeningMethod()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(ZString.Empty, package.ScreeningMethod);
		}

		public virtual void TestIsHighRisk()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(ZBool.False, package.IsHighRisk);
		}

		public virtual void TestAdditionalScreeningMethod()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(ZString.Empty, package.AdditionalScreeningMethod);
		}

		#endregion

		#region TestSetValuesFromTemplate

		public void TestSetValuesFromTemplate()
		{
			var stubWithoutVolumeValueAndUQ = SetupPackageTemplateMock(1m, 3m, 5m, 7m, Constants.Length.Metres, Constants.Weight.Kilograms, null, "");
			var package1 = (PkgPackage)GetNewBusinessObject();
			package1.SetValuesFromTemplate(stubWithoutVolumeValueAndUQ);
			AssertPackage(package1, 1m, 3m, 5m, 7m, Constants.Length.Metres, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);

			var stubWithoutVolumeValue = SetupPackageTemplateMock(2m, 4m, 8m, 16m, Constants.Length.Centimetres, Constants.Weight.Pounds, null, Constants.Volume.CubicFeet);
			var package2 = (PkgPackage)GetNewBusinessObject();
			package2.KP_Volume = 5m;
			package2.KP_VolumeUQ = Constants.Volume.CubicDecimetres;

			package2.SetValuesFromTemplate(stubWithoutVolumeValue);
			// the volume will be calculated from template dimensions and converted to CubicDecimetres (the package volume UQ)
			var expectedVolume = 2 * 4 * 8 / 1000m;
			AssertPackage(package2, 2m, 4m, 8m, 16m, Constants.Length.Centimetres, Constants.Weight.Pounds, expectedVolume, Constants.Volume.CubicDecimetres);

			var stubWithVolumeAndUQ = SetupPackageTemplateMock(2m, 4m, 8m, 16m, Constants.Length.Feet, Constants.Weight.Pounds, 37m, Constants.Volume.CubicFeet);
			var package3 = (PkgPackage)GetNewBusinessObject();
			package3.KP_Volume = 5m;
			package3.KP_VolumeUQ = Constants.Volume.CubicDecimetres;

			package3.SetValuesFromTemplate(stubWithVolumeAndUQ);
			AssertPackage(package3, 2m, 4m, 8m, 16m, Constants.Length.Feet, Constants.Weight.Pounds, 37m, Constants.Volume.CubicFeet);
		}

		public void TestSetValuesFromTemplate_WithDifferentWeightUnit()
		{
			var templateWithKGWeight = SetupPackageTemplateMock(1m, 3m, 5m, 7m, Constants.Length.Metres, Constants.Weight.Kilograms, null, "");
			var package = (PkgPackage)GetNewBusinessObject();
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Pounds;
			package.SetValuesFromTemplate(templateWithKGWeight);
			AssertEquals(IsTareWeightDefaulted ? 11.536m : 10m, package.KP_Weight);
			AssertEquals(IsTareWeightDefaulted ? 7m : 0m, package.KP_TareWeight);
			AssertEquals(Constants.Weight.Kilograms, package.KP_WeightUQ);
		}

		public void TestSetValuesFromTemplate_InvalidWeightUnit()
		{
			var templateWithKGWeight = SetupPackageTemplateMock(1m, 3m, 5m, 7m, Constants.Length.Metres, Constants.Weight.Kilograms, null, "");
			var package = Factory.New<PkgPackage>();
			package.KP_Weight = 10m;
			package.KP_WeightUQ = "XX";
			package.SetValuesFromTemplate(templateWithKGWeight);
			AssertEquals(17m, package.KP_Weight);
			AssertEquals(7m, package.KP_TareWeight);
			AssertEquals(Constants.Weight.Kilograms, package.KP_WeightUQ);
		}

		public void TestSetValuesFromTemplate_InvalidTemplateUnit()
		{
			var templateWithKGWeight = SetupPackageTemplateMock(1m, 3m, 5m, 7m, Constants.Length.Metres, "XX", null, "");
			var package = Factory.New<PkgPackage>();
			package.KP_Weight = 10m;
			package.KP_WeightUQ = "KG";
			package.SetValuesFromTemplate(templateWithKGWeight);
			AssertEquals(17m, package.KP_Weight);
			AssertEquals(7m, package.KP_TareWeight);
			AssertEquals("XX", package.KP_WeightUQ);
		}

		IPackageTemplate SetupPackageTemplateMock(decimal length, decimal width, decimal height, decimal tareWeight, string dimUQ, string weightUQ, decimal? volume, string volumeUQ)
		{
			var mock = new Mock<IPackageTemplate>();
			mock.Setup(x => x.Length).Returns(length);
			mock.Setup(x => x.Width).Returns(width);
			mock.Setup(x => x.Height).Returns(height);
			mock.Setup(x => x.TareWeight).Returns(tareWeight);
			mock.Setup(x => x.DimensionUQ).Returns(dimUQ);
			mock.Setup(x => x.WeightUQ).Returns(weightUQ);
			mock.Setup(x => x.Volume).Returns(volume);
			mock.Setup(x => x.VolumeUQ).Returns(volumeUQ);
			return mock.Object;
		}

		void AssertPackage(PkgPackage package, decimal length, decimal width, decimal height, decimal tareWeight, string dimUQ, string weightUQ, decimal volume, string volumeUQ)
		{
			AssertEquals(length, package.KP_Length);
			AssertEquals(width, package.KP_Width);
			AssertEquals(height, package.KP_Height);
			AssertEquals(tareWeight, package.KP_Weight);
			AssertEquals(IsTareWeightDefaulted ? tareWeight : 0m, package.KP_TareWeight);

			AssertEquals(dimUQ, package.KP_DimensionUQ);
			AssertEquals(weightUQ, package.KP_WeightUQ);

			AssertEquals(volume, package.KP_Volume);
			AssertEquals(volumeUQ, package.KP_VolumeUQ);
		}

		#endregion

		#region TestBreakDownIntoIndividualPackages

		public void TestBreakDownIntoIndividualPackages()
		{
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);

			var individualBoxes = tenBoxes.BreakDownIntoIndividualPackages();
			var countOfBoxesWithCorrectDetails =
			(
				from b in individualBoxes
				where b.KP_KP_ParentPackage == ZGuid.Empty
				where b.KP_KJ_ParentPackageJob == tenBoxes.KP_KJ_ParentPackageJob
				where b.KP_PackageQty == 1
				select b
			)
			.Count();

			AssertEquals(10, individualBoxes.Count());
			AssertCollectionContains("The original '10x BOX' should be included as one of the split packages.", tenBoxes, individualBoxes);
			AssertEquals(10, countOfBoxesWithCorrectDetails);
		}

		public void TestBreakDownIntoIndividualPackages_UpdatesBinding()
		{
			var refreshBindingHitCount = 0;
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);
			tenBoxes.KP_PackageQtyInfo.ValueChanged += (sender, e) => refreshBindingHitCount++;

			tenBoxes.BreakDownIntoIndividualPackages();

			AssertEquals("Doesn't update binding upon breaking down into individual packages", 1, refreshBindingHitCount);
		}

		public void TestBreakDownIntoIndividualPackages_WithContainers()
		{
			Data.CreatePackingData();

			var tenContainers = Data.PackageJob.Packages.AddNew("CNT", 10);
			tenContainers.Container.K0_RC_ContainerType = Data.Container20GP.PK; // necessary to create the PkgPackageContainer object

			var splitContainers = tenContainers.BreakDownIntoIndividualPackages();
			AssertEquals(10, splitContainers.Count());
		}

		public void TestBreakDownIntoIndividualPackages_SplitsGrossWeightAndVolumeOnPackages()
		{
			Data.CreatePackingData();

			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);
			tenBoxes.KP_Weight = 20m;
			tenBoxes.KP_Volume = 10m;

			var splitBoxes = tenBoxes.BreakDownIntoIndividualPackages();
			AssertEquals(10, splitBoxes.Count());

			foreach (var box in splitBoxes)
			{
				AssertEquals(2m, box.KP_Weight);
				AssertEquals(1m, box.KP_Volume);
			}
		}

		public void TestBreakDownIntoIndividualPackages_SplitsDunnageTareAndGoodsWeight()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_DunnageWeight = 20m;
			package.KP_TareWeight = 30m;
			package.GoodsWeight = 100m;
			package.KP_Volume = 36m;
			CombineAssertions(() =>
			{
				AssertEquals("Precondition", 130m, package.KP_Weight);
				AssertEquals("Precondition", 36m, package.KP_Volume);
				AssertEquals("Precondition", 30m, package.KP_TareWeight);
			});

			package.KP_PackageQty = 10;
			CombineAssertions(() =>
			{
				AssertEquals("package.KP_TareWeight", 300m, package.KP_TareWeight);
				AssertEquals("package.KP_DunnageWeight", 200m, package.KP_DunnageWeight);
				AssertEquals("package.GoodsWeight", 1000m, package.GoodsWeight);
				AssertEquals("package.KP_Volume", 36m, package.KP_Volume);
				AssertEquals("package.KP_Weight", 1300m, package.KP_Weight);
			});

			var splitPackages = package.BreakDownIntoIndividualPackages();
			AssertEquals(10, splitPackages.Count());

			foreach (var splitPackage in splitPackages)
			{
				CombineAssertions(() =>
				{
					AssertEquals(130m, splitPackage.KP_Weight);
					AssertEquals(3.6m, splitPackage.KP_Volume);
					AssertEquals(20m, splitPackage.KP_DunnageWeight);
					AssertEquals(100m, splitPackage.GoodsWeight);
					AssertEquals(30m, splitPackage.KP_TareWeight);
				});
			}
		}

		public void TestBreakDownIntoIndividualPackages_WithQtyGreaterThan1()
		{
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);

			var splitBoxes = tenBoxes.BreakDownIntoIndividualPackages(2);
			var countOfBoxesWithCorrectDetails =
			(
				from b in splitBoxes
				where b.KP_KP_ParentPackage == ZGuid.Empty
				where b.KP_KJ_ParentPackageJob == tenBoxes.KP_KJ_ParentPackageJob
				where b.KP_PackageQty == 2
				select b
			)
			.Count();

			CombineAssertions(delegate
			{
				AssertEquals("splitBoxes.Count()", 5, splitBoxes.Count());
				AssertCollectionContains("The original '10x BOX' should be included as one of the split packages.", tenBoxes, splitBoxes);
				AssertEquals("countOfBoxesWithCorrectDetails", 5, countOfBoxesWithCorrectDetails);
			});
		}

		public void TestBreakDownIntoIndividualPackages_WithNonDivisibleQty()
		{
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);

			var splitBoxes = tenBoxes.BreakDownIntoIndividualPackages(3);
			var countOfEvenlySplitBoxes =
			(
				from b in splitBoxes
				where b.KP_KP_ParentPackage == ZGuid.Empty
				where b.KP_KJ_ParentPackageJob == tenBoxes.KP_KJ_ParentPackageJob
				where b.KP_PackageQty == 3
				select b
			)
			.Count();

			var remainder =
			(
				from b in splitBoxes
				where b.KP_KP_ParentPackage == ZGuid.Empty
				where b.KP_KJ_ParentPackageJob == tenBoxes.KP_KJ_ParentPackageJob
				where b.KP_PackageQty == 1
				select b
			)
			.Count();

			CombineAssertions(delegate
			{
				AssertEquals("splitBoxes.Count()", 4, splitBoxes.Count());
				AssertCollectionContains("The original '10x BOX' should be included as one of the split packages.", tenBoxes, splitBoxes);
				AssertEquals("countOfEvenlySplitBoxes", 3, countOfEvenlySplitBoxes);
				AssertEquals("Splitting boxes should have 1 remainder.", 1, remainder);
			});
		}

		public void TestBreakDownIntoIndividualPackages_WithQtyLessThan1()
		{
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);
			var splitBoxes = tenBoxes.BreakDownIntoIndividualPackages(0);

			AssertEquals("splitBoxes.Count()", 0, splitBoxes.Count());
		}

		public void TestBreakDownIntoIndividualPackages_WithQtyEqualToNumberOfPackages()
		{
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);
			var splitBoxes = tenBoxes.BreakDownIntoIndividualPackages(10);

			AssertEquals("splitBoxes.Count()", 0, splitBoxes.Count());
		}

		public void TestBreakDownIntoIndividualPackages_WithQtyGreaterThanNumberOfPackages()
		{
			Data.CreatePackingData();
			var tenBoxes = Data.PackageJob.Packages.AddNew("BOX", 10);
			var splitBoxes = tenBoxes.BreakDownIntoIndividualPackages(11);

			AssertEquals("splitBoxes.Count()", 0, splitBoxes.Count());
		}

		public void TestBreakDownIntoIndividualPackages_OnlySplitsEmptyPackages()
		{
			Data.CreatePackingData();
			var tenBoxesWithGoods = Data.PackageJob.Packages.AddNew("BOX", 10);
			var tenBoxesWithCartons = Data.PackageJob.Packages.AddNew("BOX", 10);
			tenBoxesWithGoods.Pack(Data.DummyLine1, 5m);
			tenBoxesWithCartons.Packages.AddNew("CTN");

			var individualBoxesWithGoods = tenBoxesWithGoods.BreakDownIntoIndividualPackages();
			AssertEquals(0, individualBoxesWithGoods.Count());
			AssertEquals("Package was already packed and should not have been split.", 10, tenBoxesWithGoods.KP_PackageQty);

			var individualBoxesWithCartons = tenBoxesWithCartons.BreakDownIntoIndividualPackages();
			AssertEquals(0, individualBoxesWithCartons.Count());
			AssertEquals("Package has Child Packages and should not have been split.", 10, tenBoxesWithCartons.KP_PackageQty);
		}

		public void TestBreakDownIntoIndividualPackages_WithAutoPackageBreakdownParent()
		{
			var originalTypeOverride = DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride;
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithAutoPackageBreakdown);

			try
			{
				var parent = Factory.New<DummyWithAutoPackageBreakdown>();
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parent);
				var tenCartons = packageJob.Packages.AddNew("CTN", 10);
				var tenBoxes = packageJob.Packages.AddNew("BOX", 10);

				AssertEquals("Precondition", false, parent.AutoBreakdownSemaphore.IsSuspended);
				AssertNull("Precondition", parent.PackagesPassedIntoRunAfterAllAutoCreatedPackagesAreAdded);

				var semaphoreSuspendedHitCount = 0;
				packageJob.Packages.CollectionCountChange += (sender, e) =>
				{
					if (parent.AutoBreakdownSemaphore.IsSuspended)
					{
						semaphoreSuspendedHitCount++;
					}
				};
				tenBoxes.BreakDownIntoIndividualPackages();
				AssertEquals("During Adding Packages, Semaphore should have been suspended. 10 counts for each Package being added.", 10, semaphoreSuspendedHitCount);
				AssertEquals("No suspension should exist after process is finished.", false, parent.AutoBreakdownSemaphore.IsSuspended);
				AssertContainsExactElementsInAnyOrder(packageJob.Packages.Where(p => p != tenCartons),
					parent.PackagesPassedIntoRunAfterAllAutoCreatedPackagesAreAdded);
			}
			finally
			{
				DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = originalTypeOverride;
			}
		}

		#endregion

		#region Printing

		public void TestPrintLabel()
		{
			ZString autoPrintFailureMessage = "";
			PkgPackage autoPrintingPackage = null;
			var autoPrintingFiredCount = 0;
			var autoPrintedFiredCount = 0;

			Data.CreatePackingData();
			Data.PackageJob.AutoPrintFailed += (sender, args) => { autoPrintFailureMessage = args.Message; };
			Data.PackageJob.AutoPrinted += (sender, e) => { autoPrintedFiredCount++; };
			Data.PackageJob.AutoPrinting += (sender, e) =>
			{
				autoPrintingFiredCount++;
				autoPrintingPackage = e.Package;
			};

			var package = Data.PackageJob.Packages.AddNew();

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			// should not print without a PackageID
			package.PrintLabel();
			AssertEquals(true, autoPrintFailureMessage.IsEmpty);
			AssertEquals(0, autoPrintingFiredCount);
			AssertEquals(0, autoPrintedFiredCount);

			// test successful print
			package.KP_PackageID = "abc";
			package.PrintLabel();
			AssertEquals(true, autoPrintFailureMessage.IsEmpty);
			AssertEquals(1, autoPrintingFiredCount);
			AssertEquals(1, autoPrintedFiredCount);
			AssertEquals(package, autoPrintingPackage);

			// test print failure
			var document = Factory.Load<StmMenuItem>(PackingRegistry.DefaultTargetLabelDocument);
			document.SU_PreventAutoDelivery = true;
			Factory.Save();

			// the AutoPrinter caches documents for performance so kill it :P
			var autoPrinter = typeof(PkgPackageJob).GetField("autoPrinter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Data.PackageJob);
			typeof(PackageLabelAutoPrinter).GetField("DocumentToPrint", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(autoPrinter, null);

			var package2 = Data.PackageJob.Packages.AddNew("BOX", "abc");
			package2.PrintLabel();
			AssertEquals("The default document to print has 'Prevent Auto Delivery' checked.", autoPrintFailureMessage);
			AssertEquals(1, autoPrintingFiredCount);
			AssertEquals(1, autoPrintedFiredCount);
		}

		public void TestPrintLabel_WithPrinterDetails()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// setup the printer
			var printer = Factory.New<IStmPrintQueue>();
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, printer.PK, 1);
			Factory.Save();

			// should not print without a PackageID
			package.PrintLabel(printer.PK.ToGuid(), 1);
			AssertNull("Nothing should be selected if package has no ID.", Data.PackageJob.Selected.SelectedPackage);

			var printJobQuery = new ZQuery();
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_SQ, printer.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, Data.PackageJob.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_JobType, "PRN");
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_DocumentName, "Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling);
			AssertNull("No print Job should have been created.", Factory.LoadTop1<IStmPrintJob>(printJobQuery));

			// test successful print
			package.KP_PackageID = "abc";
			package.PrintLabel(printer.PK.ToGuid(), numberOfLabelsToPrint: 1);
			AssertEquals("Package that is Printing should be selected.", package, Data.PackageJob.Selected.SelectedPackage);

			var printJob1 = (BusinessObject)Factory.LoadTop1<IStmPrintJob>(printJobQuery);
			AssertNotNull("Print Job should have been created if Package has ID.", printJob1);
			AssertEquals("Number of Copies should be the same as passed into PrintLabel().", (short)1, printJob1[StmPrintJobSchema.SP_Copies]);

			printJob1.Delete(); // clean-up
			Factory.Save();

			package.PrintLabel(printer.PK.ToGuid(), numberOfLabelsToPrint: 4);
			var printJob2 = (BusinessObject)Factory.LoadTop1<IStmPrintJob>(printJobQuery);
			AssertNotNull("Print Job should have been created if Package has ID.", printJob2);
			AssertEquals("Number of Copies should be the same as passed into PrintLabel().", (short)4, printJob2[StmPrintJobSchema.SP_Copies]);
		}

		public void TestPrintDocument_WithPrinterDetails()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// setup the printer
			var printer = Factory.New<IStmPrintQueue>();
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, printer.PK, 1);
			Factory.Save();

			// load a document
			var labelQuery = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Package, "Package Manifest");
			var manifestDocument = Factory.LoadTop1<StmMenuItem>(labelQuery);

			Factory.Save();

			// should not print without a PackageID
			package.PrintDocument(printer.PK.ToGuid(), 2, manifestDocument);
			AssertNull("Nothing should be selected if package has no ID.", Data.PackageJob.Selected.SelectedPackage);

			var printJobQuery = new ZQuery();
			AssertNull("No print Job should have been created.", Factory.LoadTop1<IStmPrintJob>(printJobQuery));

			// test successful print
			package.KP_PackageID = "abc";
			package.PrintDocument(printer.PK.ToGuid(), 2, manifestDocument);
			AssertEquals("Package that is Printing should be selected.", package, Data.PackageJob.Selected.SelectedPackage);

			var printJob = (BusinessObject)Factory.LoadTop1<IStmPrintJob>(printJobQuery);
			AssertNotNull("Print Job should have been created if Package has ID.", printJob);
			AssertEquals("Number of Copies should be 2.", (short)2, printJob[StmPrintJobSchema.SP_Copies]);

			var printedDocument = (ZString)printJob[StmPrintJobSchema.SP_DocumentName];
			Assert("Correct Document was printed.", printedDocument.StartsWith("Package Manifest"));
		}

		#region PrintCarrierLabel

		public void TestPrintCarrierLabel()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.RTUSLabelPrinterPK = printer.PK;
			package.KP_PackageID = "ABC-1";
			Factory.Save();

			AssertEquals("Precondition: Package ID.", "ABC-1", package.KP_PackageID);
			AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING"; // simulate setting Tracking Number
					p.IsSentToRTUS = true;
				})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is successful.", true, result.Success);
				AssertEquals("Print is successful.", true, string.IsNullOrEmpty(result.Message));
			}

			var newFactory = new BusinessObjectFactory();
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			AssertEquals("Package is sent to RTUS.", true, packageInNewFactory.IsSentToRTUS);
			AssertEquals("Package ID is updated.", "TRACKING", packageInNewFactory.KP_PackageID);
		}

		public void TestPrintCarrierLabel_Integration()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var rtusPrinterMock = new Mock<IRTUSPrinter>();
			rtusPrinterMock.Setup(pr => pr.Print(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "PRINTER", "PRINTSERVER")).Returns(true);

			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(new byte[] { (byte)'A', (byte)'B' });
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(), It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is successful.", true, result.Success);
				AssertEquals("Print is successful.", true, string.IsNullOrEmpty(result.Message));
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "NUMBER", package.KP_PackageID);

				var newFactory = new BusinessObjectFactory();
				var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
				AssertEquals("Package is sent to RTUS.", true, packageInNewFactory.IsSentToRTUS);
			}
		}

		IDisposable SetupRegistryForTest(string remotePrintServer, string remotePrintUserName, string remotePrintPassword)
		{
			return new DisposableList(new[]
			{
					TransportRegistry.Instance.RemotePrintServerURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintServer),
					WebDataRegistry.Instance.WebServiceUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintUserName),
					WebDataRegistry.Instance.WebServicePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintPassword),
				});
		}

		static bool IsStreamGeneratedFromCorrectWriter(Stream originalStream)
		{
			using (var streamToRead = new MemoryStream())
			{
				originalStream.CopyTo(streamToRead);
				originalStream.Position = 0;
				streamToRead.Position = 0;

				using (var reader = new StreamReader(streamToRead))
				{
					var uxml = reader.ReadToEnd();
					return uxml.Contains("<ReferenceNumber>ABC-1</ReferenceNumber>");
				}
			}
		}

		public void TestPrintCarrierLabel_WithPrinterPkAndPrinterProviderManager()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) => p.KP_PackageID = "TRACKING") // simulate setting Tracking Number
				.Returns(new ReturnResult { Success = true });

			var carrierLabelPrintingProviderManagerMock = new Mock<ICarrierLabelPrintingProviderManager>();
			carrierLabelPrintingProviderManagerMock.Setup(c => c.IsParentJobValid(It.Is<IPackingParent>(p => p.PK == package.PackageJob.ParentJob.PK)))
				.Returns(true);

			var stringBuilder = new ZStringBuilder();
			carrierLabelPrintingProviderManagerMock.Setup(c => c.GetCarrierLabelPrintingProvider(It.IsAny<Action<Exception>>()))
					.Returns(carrierLabelProvider.Object);

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			using (var carrierLabelPrintingProviderManager = carrierLabelPrintingProviderManagerMock.Object)
			{
				var result = package.PrintCarrierLabel(printer.PK, carrierLabelPrintingProviderManager);

				AssertEquals("Print is successful.", true, result.Success);
				AssertEquals("Print is successful.", true, string.IsNullOrEmpty(result.Message));
			}
		}

		public void TestPrintCarrierLabel_JobIsNotSaved()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.KP_PackageQty = 10;

			AssertEquals("Precondition", true, package.HasChanges);
			var result = package.PrintCarrierLabel();
			AssertEquals("Print is not successful.", false, result.Success);
			AssertEquals("Print is not successful.", "Save your changes first before printing carrier label.", result.Message);
		}

		public void TestPrintCarrierLabel_ShowsErrorWhenPackageHasNoPrinterSelected()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			AssertEquals("Precondition", true, package.RTUSLabelPrinterPK.IsEmpty);
			var result = package.PrintCarrierLabel();
			AssertEquals("Print is not successful.", false, result.Success);
			AssertEquals("Print is not successful.", "Please select a Carrier Label Printer to print carrier label.", result.Message);
		}

		public void TestPrintCarrierLabel_PrintFailure()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) => p.KP_PackageID = "TRACKING") // simulate setting Tracking Number
				.Returns(new ReturnResult { Success = false, Message = "Some Error Message" });

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Print is not successful.", "Some Error Message", result.Message);
			}
		}

		public void TestPrintCarrierLabel_PrintFailure_PrintErrorIsException()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			printer.SQ_DisplayName = "PRINTER";

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			Action<Exception> onError = null;

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.SetupGet(c => c.IsRemotePrintingConnectionDetailsProvided).Returns(true);
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.IsNotNull<PkgPackage>(), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.IsNotNull<IStmPrintQueue>()))
				.Returns(new ReturnResult { Message = "", Success = false }).Callback(() => onError(new Exception("Error 407 - Proxy Authentication Required")));

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(MockFactory))
			{
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Failed print, should have Exception Error Message.",
					"Failed to send Print Request to 'PRINTER'.\r\nDetails below:\r\nError 407 - Proxy Authentication Required", result.Message);
			}

			// store exception handler passed in so we can invoke it later to simulate an exception occuring
			ICarrierLabelPrintingProvider MockFactory(object[] args)
			{
				onError = (Action<Exception>)args[0];
				return carrierLabelProvider.Object;
			}
		}

		public void TestPrintCarrierLabel_ParentJobTypeIsNotConfigured()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Print is not successful.", "Packing parent job is not configured to send its packages to RTUS.", result.Message);
			}
		}

		public void TestPrintCarrierLabel_JobWithNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.JobNoForPackingParent = "DUMMY1";
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Print is not successful.", "Dummy 'DUMMY1' has no Carrier Booking Agent.", result.Message);
			}
		}

		public void TestPrintCarrierLabel_RegistryMissingRTUS()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			smartFreight.OH_Code = "SMART";
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) => p.KP_PackageID = "TRACKING") // simulate setting Tracking Number
				.Returns(new ReturnResult { Success = true });

			var result = package.PrintCarrierLabel();
			AssertEquals("Print is not successful.", false, result.Success);
			AssertEquals("Print is not successful.", "No RTUS URL provided for Organization 'SMART'.", result.Message);
		}

		const string SmartfreightUrl = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";

		#endregion

		public void TestPrintLabel_SetsIsLabelPrintedToTrue()
		{
			var autoPrintFailureMessage = ZString.Empty;
			PkgPackage autoPrintingPackage = null;
			var autoPrintingFiredCount = 0;
			var autoPrintedFiredCount = 0;

			Data.CreatePackingData();
			Data.PackageJob.AutoPrintFailed += (sender, args) => { autoPrintFailureMessage = args.Message; };
			Data.PackageJob.AutoPrinted += (sender, e) => { autoPrintedFiredCount++; };
			Data.PackageJob.AutoPrinting += (sender, e) =>
			{
				autoPrintingFiredCount++;
				autoPrintingPackage = e.Package;
			};

			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition", false, package.IsLabelPrinted);

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			// test successful print
			package.KP_PackageID = "abc";
			package.PrintLabel();
			AssertEquals("Print is successful.", true, autoPrintFailureMessage.IsEmpty);
			AssertEquals("Print is successful.", 1, autoPrintingFiredCount);
			AssertEquals("Print is successful.", 1, autoPrintedFiredCount);
			AssertEquals("Print is successful.", package, autoPrintingPackage);
			AssertEquals("IsLabelPrinted is updated to true.", true, package.IsLabelPrinted);
		}

		public void TestPrintLabel_PrintFailureDoesNotSetIsLabelPrintedToTrue()
		{
			var targetLabelMenu = Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocument);
			targetLabelMenu.SU_PreventAutoDelivery = true;

			var autoPrintFailureMessage = ZString.Empty;
			Data.CreatePackingData();
			Data.PackageJob.AutoPrintFailed += (sender, args) => { autoPrintFailureMessage = args.Message; };

			var package = Data.PackageJob.Packages.AddNew();
			package.KP_PackageID = "abc";
			AssertEquals("Precondition", false, package.IsLabelPrinted);

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			// test failed print
			package.PrintLabel();
			AssertEquals("Print is not successful.", false, autoPrintFailureMessage.IsEmpty);
			AssertEquals("Print is not successful.", "The default document to print has 'Prevent Auto Delivery' checked.", autoPrintFailureMessage);
			AssertEquals("IsLabelPrinted is not updated to true.", false, package.IsLabelPrinted);
		}

		public void TestPrintCarrierLabel_PrintFailure_SuccessfulRTUSResponsePrintFailed()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			printer.SQ_DisplayName = "PRINTER";

			var rtusPrinterMock = new Mock<IRTUSPrinter>();
			rtusPrinterMock.Setup(pr => pr.Print(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "PRINTER", "PRINTSERVER")).Returns(false);

			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(new byte[] { (byte)'A', (byte)'B' });
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.RTUSLabelPrinterPK = printer.PK;
			Factory.Save();

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(), It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				var result = package.PrintCarrierLabel();

				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Print is not successful.", "Failed to send Print Request to 'PRINTER'.", result.Message);

				var newFactory = new BusinessObjectFactory();
				var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
				AssertEquals("Package is sent to RTUS.", true, packageInNewFactory.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", "NUMBER", packageInNewFactory.KP_PackageID);
			}
		}

		#endregion

		#region TestIPackingParentOnPackageDelete

		public void TestIPackingParentOnPackageDelete()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");

			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.Delete, "Cannot delete.") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete);
			package.Delete();
			AssertEquals("Delete was canceled, package should not have been deleted.", false, package.IsDeleted);
			AssertEquals("Delete was canceled, Data.Dummy.OnPackageDeleteFiredCount should not fired.", 0, Data.Dummy.OnPackageDeleteFiredCount);

			Data.Dummy.SetPackageActionResponses(actionStrategyNone);
			package.Delete();
			AssertEquals(true, package.IsDeleted);
			AssertEquals("Delete was canceled, Data.Dummy.OnPackageDeleteFiredCount should have fired.", 1, Data.Dummy.OnPackageDeleteFiredCount);
		}

		#endregion

		#region PackageActionStrategy

		#region TestPackageActionStrategy_RestrictDelete

		public void TestPackageActionStrategy_RestrictDelete_PackageIsDeleted()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");

			AssertEquals("Package has a allow delete.", true, package.CanDelete);
			package.Delete();
			AssertEquals("Package IsDeleted.", true, package.IsDeleted);
			AssertNoExceptionThrown("Package doesn't throw exception when deleted when IsDeleted.", () => package.Delete());
		}

		public void TestPackageActionStrategy_RestrictDelete_Simple()
		{
			int packageActionCanceledFiredCount = 0;

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");

			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.Delete, "Cannot delete.") }
			};

			Data.PackageJob.PackageDeleteCanceled += delegate(object sender, PackageCanceledActionEventArgs args)
			{
				packageActionCanceledFiredCount++;

				AssertEquals(package, args.Package);
				AssertEquals("Cannot delete.", args.ReasonForNotAllowingAction);
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete);
			package.Delete();
			AssertEquals("Delete was canceled, package should not have been deleted.", false, package.IsDeleted);
			AssertEquals("Delete was canceled, packageJob.PackageDeleteCanceled should have fired.", 1, packageActionCanceledFiredCount);

			Data.Dummy.SetPackageActionResponses(actionStrategyNone);
			package.Delete();
			AssertEquals(true, package.IsDeleted);
			AssertEquals("Delete was not canceled, packageJob.PackageDeleteCanceled should not have fired.", 1, packageActionCanceledFiredCount);
		}

		public void TestPackageActionStrategy_RestrictDelete_ChildrenHaveDifferentStrategies()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var packageChild = package.Packages.AddNew();

			var actionStrategiesByPackage = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) },
				{ packageChild, new PackageActionStrategy(package) },
			};

			Data.Dummy.SetPackageActionResponses(actionStrategiesByPackage, false);
			AssertEquals("Package has a allow delete strategy, package can be deleted.", true, package.CanDelete);

			actionStrategiesByPackage = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) },
				{ packageChild, new PackageActionStrategy(packageChild, PackageAction.Delete, "Cannot delete.") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategiesByPackage);
			AssertEquals("Package has a allow delete strategy, but child cannot, package can NOT be deleted.", false, package.CanDelete);
		}

		#endregion

		#region TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild

		public void TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var packageChild = package.Packages.AddNew();

			var actionStrategiesByPackage = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) },
				{ packageChild, new PackageActionStrategy(packageChild, PackageAction.Delete, "I am a good child and I do not want to be deleted!") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategiesByPackage);
			package.PackageJob.PackageDeleteCanceled += (object sender, PackageCanceledActionEventArgs e) =>
			{
				AssertEquals("Should show error of the child in parent if cannot delete", "I am a good child and I do not want to be deleted!", e.ReasonForNotAllowingAction);
			};
			AssertEquals("Package has a allow delete but child cannot, package can NOT be deleted.", false, package.CanDelete);
		}

		#endregion

		#region TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_MultiChildError

		public void TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_MultiChildError()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var packageChild = package.Packages.AddNew();
			var packageGrandchildWithError = packageChild.Packages.AddNew();
			var packageGrandchildNoError = packageChild.Packages.AddNew();

			var actionStrategiesByPackage = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) },
				{ packageChild, new PackageActionStrategy(packageChild) },
				{ packageGrandchildNoError, new PackageActionStrategy(packageChild) },
				{ packageGrandchildWithError, new PackageActionStrategy(packageChild, PackageAction.Delete, "I am a good grandchild and I do not want to be deleted!") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategiesByPackage);
			package.PackageJob.PackageDeleteCanceled += (object sender, PackageCanceledActionEventArgs e) =>
			{
				AssertEquals("Should show error of the child in parent if cannot delete", "I am a good grandchild and I do not want to be deleted!", e.ReasonForNotAllowingAction);
			};
			AssertEquals("Package has a allow delete but child cannot, package can NOT be deleted.", false, package.CanDelete);
		}

		#endregion

		#region TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_NoChild

		public void TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_NoChild_CanDelete()
		{
			TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_NoChild_Core(canDelete: true);
		}

		public void TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_NoChild_CannotDelete()
		{
			TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_NoChild_Core(canDelete: false);
		}

		public void TestPackageActionStrategy_RestrictDelete_ShowReasonIfCannotDeleteChild_NoChild_Core(bool canDelete)
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");

			var packageActionStrategy = canDelete ? new PackageActionStrategy(package) : new PackageActionStrategy(package, PackageAction.Delete, "Cannot Delete!");
			var actionStrategy = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, packageActionStrategy },
			};

			Data.Dummy.SetPackageActionResponses(actionStrategy);
			var eventIsCalled = false;
			package.PackageJob.PackageDeleteCanceled += (object sender, PackageCanceledActionEventArgs e) =>
			{
				eventIsCalled = true;
				AssertEquals("Should show error of the child in parent if cannot delete", "Cannot Delete!", e.ReasonForNotAllowingAction);
			};

			AssertEquals($"Should {(canDelete ? "** not **" : "")} be able to delete package!", canDelete, package.CanDelete);
			AssertNotEquals($"Should {(canDelete ? "** not **" : "")} call event!", eventIsCalled, canDelete);
		}

		#endregion

		#region TestPackageActionStrategy AddInnerPackage \ Pack \ UnPack

		public void TestTestPackageActionStrategy_RestrictEdit_IsAvailableForPacking()
		{
			AssertPackageActionStrategy_IsAvailable(isPacking: true); //AddInnerPackage & Pack
		}

		public void TestTestPackageActionStrategy_RestrictEdit_IsAvailableForUnpacking()
		{
			AssertPackageActionStrategy_IsAvailable(isPacking: false);
		}

		void AssertPackageActionStrategy_IsAvailable(bool isPacking)
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoPackUnpack = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.PackUnpack, "Cannot Add Inner Package.") }
			};

			ZString errorMessage;
			AssertEquals(true, isPacking ? package.IsAvailableForPacking(out errorMessage) : package.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);

			Data.Dummy.SetPackageActionResponses(actionStrategyNoPackUnpack);
			AssertEquals(false, isPacking ? package.IsAvailableForPacking(out errorMessage) : package.IsAvailableForUnpacking(out errorMessage));
			AssertEquals("Cannot Add Inner Package.", errorMessage);

			Data.Dummy.SetPackageActionResponses(actionStrategyNone);
			AssertEquals(true, isPacking ? package.IsAvailableForPacking(out errorMessage) : package.IsAvailableForUnpacking(out errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);
		}

		#endregion

		#region TestPackageActionStrategy_RestrictEdit_BreakDownIntoIndividualPackages

		public void TestPackageActionStrategy_RestrictEdit_BreakDownIntoIndividualPackages()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoPackUnpack = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.PackUnpack, "Cannot Break Down Package.") }
			};

			ZString errorMessage;

			AssertEquals(true, package.IsBreakDownAllowed(out errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);

			Data.Dummy.SetPackageActionResponses(actionStrategyNoPackUnpack);
			AssertEquals(false, package.IsBreakDownAllowed(out errorMessage));
			AssertEquals("Cannot Break Down Package.", errorMessage);

			Data.Dummy.SetPackageActionResponses(actionStrategyNone);
			AssertEquals(true, package.IsBreakDownAllowed(out errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);
		}

		#endregion

		#region TestPackageActionStrategy_ClearStrategyIncludingChildren

		public void TestPackageActionStrategy_ClearStrategyIncludingChildren()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var packageChild = package.Packages.AddNew();
			var packageChildOfChild = packageChild.Packages.AddNew();
			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.Delete, "Cannot delete.") },
				{ packageChild, new PackageActionStrategy(packageChild, PackageAction.Delete, "Cannot delete.") },
				{ packageChildOfChild, new PackageActionStrategy(packageChildOfChild, PackageAction.Delete, "Cannot delete.") }
			};

			AssertEquals("Package has a allow delete strategy, package can be deleted.", true, package.CanDelete);
			AssertEquals("packageChild has a allow delete strategy, packageChild can be deleted.", true, packageChild.CanDelete);
			AssertEquals("packageChildOfChild has a allow delete strategy, packageChildOfChild can be deleted.", true, packageChildOfChild.CanDelete);

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete);
			AssertEquals("package has a no delete strategy, package cannot be deleted.", false, package.CanDelete);
			AssertEquals("package has a no delete strategy, packageChild cannot be deleted.", false, packageChild.CanDelete);
			AssertEquals("package has a no delete strategy, packageChildOfChild can be deleted.", false, packageChildOfChild.CanDelete);
		}

		#endregion

		#region TestPackageActionStrategy_ClearStrategyOnKP_KP_ParentPackageChange

		public void TestPackageActionStrategy_ClearStrategyOnKP_KP_ParentPackageChange()
		{
			Data.CreatePackingData();
			var containerReadOnly = Data.PackageJob.Packages.AddNew("CNT");
			var containerEditable = Data.PackageJob.Packages.AddNew("CNT");
			var pallet = containerEditable.Packages.AddNew("PLT");
			var actionStrategyReadOnly = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ pallet, new PackageActionStrategy(pallet) }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyReadOnly);
			AssertEquals("Precondition: pallet should be editable.", false, pallet.ReadOnly);

			actionStrategyReadOnly = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ pallet, new PackageActionStrategy(pallet, PackageAction.Edit, "Cannot edit.") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyReadOnly, false);
			AssertEquals("Pallet should be editable as action strategy has not been cleared yet.", false, pallet.ReadOnly);
			pallet.KP_KP_ParentPackage = containerReadOnly.PK; // this should clear action strategies
			AssertEquals("Pallet should now be ReadOnly", true, pallet.ReadOnly);
		}

		#endregion

		#endregion

		#region Delete

		#region TestDelete

		public void TestDelete()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = "CNT";

			var container = package.Container;
			package.CartonGroupAndSize = "SOME VALUE";

			var bookedDimensions = package.BookedDimensions;
			bookedDimensions.KPB_Length = 5m;
			bookedDimensions.KPB_DimensionUQ = "M";

			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, package.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, PkgPackage.Schema.CartonGroupAndSize);
			var addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			AssertNotNull("Precondition", addOnColumn);

			package.Delete();
			AssertEquals(true, container.IsDeleted);
			AssertEquals(true, addOnColumn.IsDeleted);
			AssertEquals(true, bookedDimensions.IsDeleted);
			AssertNull(Factory.LoadTop1<GenAddOnColumn>(query));
		}

		#region TestDelete_JobServiceLink

		public void TestDelete_JobServiceLink()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = "CNT";

			var jobServiceLink = Factory.New<JobServiceLink>();
			jobServiceLink.ESL_ParentID = package.PK;
			jobServiceLink.ESL_ParentTableCode = "KP";

			package.Delete();
			AssertEquals(true, jobServiceLink.IsDeleted);
			AssertNull(Factory.LoadTop1<JobServiceLink>(new ZQuery()));
		}

		#endregion

		#region TestDelete_Sequence

		public void TestDelete_Sequence_PackageSequenceTypeIsStandard()
		{
			TestDelete_SequenceCore(PackageSequenceType.Standard);
		}

		public void TestDelete_Sequence_PackageSequenceTypeIsOuter()
		{
			TestDelete_SequenceCore(PackageSequenceType.Outer);
		}

		public void TestDelete_Sequence_PackageSequenceTypeIsOuterAndLooseID()
		{
			TestDelete_SequenceCore(PackageSequenceType.OuterWithLooseID);
		}

		void TestDelete_SequenceCore(PackageSequenceType packageSequenceType)
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = packageSequenceType;

			var package1 = Data.PackageJob.Packages.AddNew("Box", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("Box", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("Box", "P3");
			var innerPackage = package2.Packages.AddNew("BOX", "IP1");

			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);
			AssertEquals((ZShort)0, innerPackage.KP_Sequence);

			package1.Delete();
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)1, package3.KP_Sequence);
			AssertEquals((ZShort)0, innerPackage.KP_Sequence);

			innerPackage.Delete();
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)1, package3.KP_Sequence);

			var package4 = Data.PackageJob.Packages.AddNew("Box", "P4");
			AssertEquals((ZShort)3, package4.KP_Sequence);

			// Delete the last package should not break the Sequence Calculation
			package4.Delete();
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)1, package3.KP_Sequence);

			var package5 = Data.PackageJob.Packages.AddNew("Box", "P5");
			AssertEquals((ZShort)3, package5.KP_Sequence);
		}

		#endregion

		#endregion

		#region TestDelete_WhenPackageIsBookedWithCarrier

		public void TestDelete_WhenPackageIsBookedWithCarrier()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				cancellationMock.Setup(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(pkg => pkg.PK == package.PK)))
					.Returns(new ReturnResult { Success = true });

				package.Delete();
				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(p => p.PK == package.PK)), Times.Once);

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);

				cancellationMock.Setup(c => c.CancelPackages(It.Is<IEnumerable<PkgPackage>>(i => i.Select(p => p.PK).ContainsSameElementsInAnyOrder(new[] { package.PK }))))
					.Returns(new[] { new CancellationResponse(Data.PackageJob.PK, Data.Dummy, responseMock.Object, "ABC-1") });
				Factory.Save();

				cancellationMock.VerifyAll();

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Success Log.", 1, notes.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-1'.", notes[0].ST_NoteText);
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_PackageJobDeleted()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				cancellationMock.Setup(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(pkg => pkg.PK == package.PK)))
					.Returns(new ReturnResult { Success = true });

				Data.PackageJob.Delete();
				Assert(package.IsDeleted);
				Assert(Data.PackageJob.IsDeleted);
				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(p => p.PK == package.PK)), Times.Once);

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);

				cancellationMock.Setup(c => c.CancelPackages(It.Is<IEnumerable<PkgPackage>>(i => i.Select(p => p.PK).ContainsSameElementsInAnyOrder(new[] { package.PK }))))
					.Returns(new[] { new CancellationResponse(Data.PackageJob.PK, Data.Dummy, responseMock.Object, "ABC-1") });
				AssertNoExceptionThrown(Factory.Save);

				cancellationMock.VerifyAll();

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Success Log.", 1, notes.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-1'.", notes[0].ST_NoteText);
			}
		}

		public void TestDelete_MultiplePackages_DbHits()
		{
			Data.CreatePackingData();

			const int NumberOfPackagesToCreate = 10;
			for (int index = 0; index < NumberOfPackagesToCreate; index++)
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-" + (index + 1));
				package.IsSentToRTUS = true;
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				cancellationMock.Setup(c => c.SubscribePackageForCancellation(It.IsAny<PkgPackage>()))
					.Returns(new ReturnResult { Success = true });

				var packageJobInNewFactory = otherFactory.Load<PkgPackageJob>(Data.PackageJob.PK);
				otherFactory.Load<DummyWithPacking>(Data.Dummy.PK).ParentJobType = ParentJobType.Dummy;
				packageJobInNewFactory.Packages.DeleteAll();
				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.IsAny<PkgPackage>()), Times.Exactly(NumberOfPackagesToCreate));

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);

				cancellationMock.Setup(c => c.CancelPackages(It.Is<IEnumerable<PkgPackage>>(i => i.Count() == NumberOfPackagesToCreate)))
					.Returns(new[] { new CancellationResponse(Data.PackageJob.PK, Data.Dummy, responseMock.Object, "Irrelevant") });
				otherFactory.Save();
				cancellationMock.VerifyAll();

				// None of these DB Hits were added by Carrier Label Cancellation code.
				// Perhaps some of these hits should be investigated.
				var expectedHits = new Dictionary<string, int>
				{
					{ DummyBizoSchema.Constants.TableName, 1 },
					{ GenAddOnColumnSchema.Constants.TableName, 1 },
					{ JobDocumentDeliverySchema.Constants.TableName, 10 },
					{ JobDocumentExclusionSchema.Constants.TableName, 10 },
					{ PkgPackageSchema.Constants.TableName, 2 },
					{ PkgPackageBookedDetailSchema.Constants.TableName, 10 },
					{ PkgPackageHeaderSchema.Constants.TableName, 1 },
					{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 10 },
					{ PkgPackageScreeningSchema.Constants.TableName, 1 },
					{ StmDocDataOverrideSchema.Constants.TableName, 40 },
					{ StmNoteSchema.Constants.TableName, 10 },
					{ StmUniversalCopySchema.Constants.TableName, 21 },
					{ UNDGDataItemSchema.Constants.TableName, 1 },
					{ StmDefaultPrinterSchema.Constants.TableName, 1 },
					{ JobServiceLinkSchema.Constants.TableName, 1 }
				};

				AssertDbHits(expectedHits, otherFactory);
			}
		}

		public void TestShouldPackTrackedPackagesViaDivot_DBHits()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var topHandlingUnit = packageJob.Packages.AddNew("PLT", "TopHU");
			var subHandlingUnit = packageJob.Packages.AddNew("PLT", "SubHU");
			Helper.PackHandlingUnit(topHandlingUnit, subHandlingUnit, topHandlingUnit);

			const int NumberOfPackedPackagesToCreate = 10;
			for (int index = 0; index < NumberOfPackedPackagesToCreate; index++)
			{
				var package = packageJob.Packages.AddNew("BOX", "Inner-" + (index + 1));
				Helper.PackHandlingUnit(subHandlingUnit, package, topHandlingUnit);
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var packageJobInNewFactory = otherFactory.Load<PkgPackageJob>(packageJob.PK);
			packageJobInNewFactory.Packages.DeleteAll();

			var expectedHits = new Dictionary<string, int>
				{
					{ DummyBizoSchema.Constants.TableName, 1 },
					{ GenAddOnColumnSchema.Constants.TableName, 1 },
					{ JobDocumentDeliverySchema.Constants.TableName, 12 },
					{ JobDocumentExclusionSchema.Constants.TableName, 12 },
					{ PkgPackageSchema.Constants.TableName, 2 },
					{ PkgPackageBookedDetailSchema.Constants.TableName, 12 },
					{ PkgPackageHandlingUnitDivotSchema.Constants.TableName, 2 },
					{ PkgPackageHeaderSchema.Constants.TableName, 1 },
					{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 12 },
					{ PkgPackageScreeningSchema.Constants.TableName, 1 },
					{ StmDocDataOverrideSchema.Constants.TableName, 46 },
					{ StmNoteSchema.Constants.TableName, 23 },
					{ StmUniversalCopySchema.Constants.TableName, 24 },
					{ UNDGDataItemSchema.Constants.TableName, 1 },
					{ StmDefaultPrinterSchema.Constants.TableName, 1 },
					{ JobServiceLinkSchema.Constants.TableName, 1 }
				};

			AssertDbHits(expectedHits, otherFactory);
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_NoCancellationOccursWhenParentHasNoParentJobType()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				package.Delete();
				Factory.Save();

				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.IsAny<PkgPackage>()), Times.Never);
				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated any Notes.", 0, notes.Length);
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_NoCancellationOccursWhenPackageIsNotSentToRTUS()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = false;
			Factory.Save();

			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				package.Delete();
				Factory.Save();

				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.IsAny<PkgPackage>()), Times.Never);
				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated any Notes.", 0, notes.Length);
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_NoCancellationOccursWhenIsSentToRTUSNotSet()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			Factory.Save();

			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				package.Delete();
				Factory.Save();

				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.IsAny<PkgPackage>()), Times.Never);
				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated any Notes.", 0, notes.Length);
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_Integration()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				var package2 = Data.PackageJob.Packages.AddNew("PLT", "ABC-2");
				package1.IsSentToRTUS = true;
				package2.IsSentToRTUS = true;
				package1.Delete();
				Factory.Save();

				// we are only going to cancel on saved packages
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), It.IsAny<RTUSCBA>(), It.IsNotNull<Uri>()), Times.Never);

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");

				Func<ITopLevelDataObject, bool> funcWrapper = IsUniversalShipmentMatchingPackage;
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(t => funcWrapper(t)), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				package2.Delete();
				Factory.Save();
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(t => funcWrapper(t)), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)));
				responseMock.VerifyGet(r => r.IsSuccessful);

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Success Log.", 1, notes.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-2'.", notes[0].ST_NoteText);
			}

			bool IsUniversalShipmentMatchingPackage(ITopLevelDataObject packageUniversalShipment)
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(packageUniversalShipment, stream);

					stream.Position = 0;

					using (var reader = new StreamReader(stream))
					{
						var xml = reader.ReadToEnd();
						return
							xml.Contains("<BookingConfirmationReference>TESTREFERENCE</BookingConfirmationReference>") &&
							xml.Contains("<Key>ABC-2</Key>") &&
							!xml.Contains("<PackingLine>");
					}
				}
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_PackageJobDeleted_Integration()
		{
			Data.CreatePackingData();
			Factory.Save();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package.IsSentToRTUS = true;
				Factory.Save();

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				Data.PackageJob.Delete();
				Assert(package.IsDeleted);
				Assert(Data.PackageJob.IsDeleted);
				AssertNoExceptionThrown(Factory.Save);

				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)));

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Success Log.", 1, notes.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-1'.", notes[0].ST_NoteText);
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_ErrorWithCancellationRequest_Integration()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package.IsSentToRTUS = true;
				Factory.Save();

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(false);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("Some Error");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				package.Delete();
				Factory.Save();
				managerMock.VerifyAll();

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Failure Log.", 1, notes.Length);
				AssertEquals("Should have generated a Failure Log.", "Some Error", notes[0].ST_NoteText);
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_MultiplePackageCancellations_Integration()
		{
			var dummyParent1 = Factory.New<DummyWithPacking>();
			var dummyParent2 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent2);

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			dummyParent1.CarrierBookingAgent = smartFreight;
			dummyParent2.CarrierBookingAgent = smartFreight;

			var package1 = packageJob1.Packages.AddNew("PLT", "ABC-1");
			var package2 = packageJob2.Packages.AddNew("PLT", "XYZ-2");
			package1.IsSentToRTUS = true;
			package2.IsSentToRTUS = true;
			Factory.Save();

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			Func<PkgPackage, string> getUniversalXml = p =>
			{
				return p.PK == package1.PK
					? GetUniversalShipmentXML("SUCCESS")
					: GetUniversalShipmentXML("FAIL");
			};

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(new[] { dummyParent1, dummyParent2 }, getUniversalXml))
			{
				var successResponse = new Mock<IRTUSResponse>();
				successResponse.SetupGet(r => r.IsSuccessful).Returns(true);

				Func<ITopLevelDataObject, string, bool> funcWrapper = UniversalShipmentHasReference;
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(d => funcWrapper(d, "SUCCESS")), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(successResponse.Object);

				var failResponse = new Mock<IRTUSResponse>();
				failResponse.SetupGet(r => r.IsSuccessful).Returns(false);
				failResponse.SetupGet(r => r.ErrorMessageForFailure).Returns("ERROR");
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(d => funcWrapper(d, "FAIL")), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(failResponse.Object);

				package1.Delete();
				package2.Delete();
				Factory.Save();
				managerMock.VerifyAll();

				var note1 = dummyParent1.Notes.FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description).Single();
				AssertEquals("Should have generated Success Log on the Correct Parent.", "Successfully canceled Package 'ABC-1'.", note1.ST_NoteText);

				var note2 = dummyParent2.Notes.FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description).Single();
				AssertEquals("Should have generated Failure Log on the Correct Parent.", "ERROR", note2.ST_NoteText);
			}

			bool UniversalShipmentHasReference(ITopLevelDataObject universalShipment, string reference)
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(universalShipment, stream);

					stream.Position = 0;

					using (var reader = new StreamReader(stream))
					{
						var xml = reader.ReadToEnd();
						return xml.Contains($"<BookingConfirmationReference>{reference}</BookingConfirmationReference>");
					}
				}
			}
		}

		public void TestDelete_WhenPackageIsBookedWithCarrier_WhenUniversalShipmentGenerationFails_Integration()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();
			var exceptionToThrow = new InvalidOperationException("Test!");

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationFailure(Data.Dummy, exceptionToThrow))
			{
				var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				var package2 = Data.PackageJob.Packages.AddNew("PLT", "ABC-2");
				package1.IsSentToRTUS = true;
				package2.IsSentToRTUS = true;
				Factory.Save();

				package1.Delete();
				package2.Delete();

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated Failure Log.", 2, notes.Length);
				AssertEquals("Should have generated Failure Log.", true,
					notes.Any(note => note.ST_NoteText == "Error occurred during Cancellation Process of Carrier Label(s). Packages for label cancellation must be canceled manually. Error: Failed to generate Universal Shipment from Package 'ABC-1'.\r\nDetails below:\r\nTest!"));
				AssertEquals("Should have generated Failure Log.", true,
					notes.Any(note => note.ST_NoteText == "Error occurred during Cancellation Process of Carrier Label(s). Packages for label cancellation must be canceled manually. Error: Failed to generate Universal Shipment from Package 'ABC-2'."));
			}
		}

		const string SmartFreightUrl = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";

		static string GetUniversalShipmentXML(string bookingReference)
		{
			return $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <BookingConfirmationReference>{bookingReference}</BookingConfirmationReference>
  </Shipment>
</UniversalShipment>";
		}

		#endregion

		#region TestDelete_WhenUserCancels

		public void TestDelete_WhenUserCancels()
		{
			int packageDeleteCanceledFiredCount = 0;

			// create a package with packed items and a child package
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var childPackage = package.Packages.AddNew("PLT");
			var packedItem = package.Pack(Data.DummyLine1, 10m).Single();
			var divot = package.PackedItemDivots.Single();

			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.Delete, "moo") }
			};

			Data.PackageJob.PackageDeleteCanceled += delegate(object sender, PackageCanceledActionEventArgs args)
			{
				packageDeleteCanceledFiredCount++;

				AssertEquals(package, args.Package);
				AssertEquals("moo", args.ReasonForNotAllowingAction);
			};

			// cancel the delete
			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete);
			package.Delete();
			AssertEquals("Delete was canceled, package should not have been deleted.", false, package.IsDeleted);
			AssertEquals("Delete was canceled, package.Packages should not have been deleted.", false, childPackage.IsDeleted);
			AssertEquals("Delete was canceled, package.PackedItemDivots should not have been deleted.", false, divot.IsDeleted);
			AssertEquals("Delete was canceled, package.PackedItems should not have been deleted.", false, packedItem.IsDeleted);
			AssertEquals("Delete was canceled, packageJob.PackageDeleteCanceled should have fired.", 1, packageDeleteCanceledFiredCount);

			// allow the delete
			Data.Dummy.SetPackageActionResponses(actionStrategyNone);
			package.Delete();
			AssertEquals(true, package.IsDeleted);
			AssertEquals(true, childPackage.IsDeleted);
			AssertEquals(true, divot.IsDeleted);
			AssertEquals(true, packedItem.IsDeleted);
			AssertEquals("Delete was not canceled, packageJob.PackageDeleteCanceled should not have fired.", 1, packageDeleteCanceledFiredCount);
		}

		#endregion

		#region TestDeleteEmptyPackagesIncludingChildren

		public void TestDeleteEmptyPackagesIncludingChildren()
		{
			int packageDeleteCanceledFireCount = 0;
			PkgPackage lastCanceledPackage = null;

			Data.CreatePackingData();
			Data.PackageJob.PackageDeleteCanceled += delegate(object sender, PackageCanceledActionEventArgs args)
			{
				packageDeleteCanceledFireCount++;
				lastCanceledPackage = args.Package;
			};

			// delete an empty package
			var emptyPackage = Data.PackageJob.Packages.AddNew();
			emptyPackage.DeleteEmptyPackagesIncludingChildren();
			AssertEquals(true, emptyPackage.IsDeleted);
			AssertEquals(0, packageDeleteCanceledFireCount);

			// attempt to delete a package when the user prevents the delete
			var package = Data.PackageJob.Packages.AddNew("PLT");
			var box1 = package.Packages.AddNew("BX1");
			var box2 = package.Packages.AddNew("BX2");
			var ctn2 = box2.Packages.AddNew("CN2");

			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.Delete, "moo") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete);
			package.DeleteEmptyPackagesIncludingChildren();
			AssertEquals(false, package.IsDeleted);
			AssertEquals(false, box1.IsDeleted);
			AssertEquals(false, box2.IsDeleted);
			AssertEquals(false, ctn2.IsDeleted);
			AssertEquals(1, packageDeleteCanceledFireCount);
			AssertEquals(package, lastCanceledPackage);
			Data.Dummy.SetPackageActionResponses(actionStrategyNone); // cleanup

			// attempt to delete a package that is packed with items
			ctn2.Pack(Data.DummyLine1, 1m);
			package.DeleteEmptyPackagesIncludingChildren();
			AssertEquals(false, package.IsDeleted);
			AssertEquals(true, box1.IsDeleted);
			AssertEquals(false, box2.IsDeleted);
			AssertEquals(false, ctn2.IsDeleted);
			AssertEquals(1, packageDeleteCanceledFireCount);
			AssertEquals(package, lastCanceledPackage);
		}

		#endregion

		#region TestDeletePkgHandlingUnitDivots

		public void TestDeletePackageDivots_DeleteTopHU()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Package tree
			//	TopHU
			//		SubHU
			//			Inner1
			//		Inner2
			var topHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "TopHU");
			var subHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "SubHU");
			var inner1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner1");
			var inner2 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner2");

			// old divots
			var topHUSubHU_UnackedDivot = Helper.PackHandlingUnitAndUnpack(topHU, subHU);
			var subHUInner1_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(subHU, inner1);
			var topHUInner2_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(topHU, inner2);

			// new divots
			var topHUSubHU_PackedDivot = Helper.PackHandlingUnit(topHU, subHU, topHU);
			var subHUInner1_PackedDivot = Helper.PackHandlingUnit(subHU, inner1, topHU);
			var topHUInner2_PackedDivot = Helper.PackHandlingUnit(topHU, inner2, topHU);

			// attempt to delete package
			topHU.Delete();

			AssertEquals("Package should be deleted.", true, topHU.IsDeleted);
			AssertEquals("Package should not be deleted.", false, subHU.IsDeleted);
			AssertEquals("Package should not be deleted.", false, inner1.IsDeleted);
			AssertEquals("Package should not be deleted.", false, inner2.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUSubHU_UnackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUSubHU_PackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUInner2_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUInner2_PackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, subHUInner1_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, subHUInner1_PackedDivot.IsDeleted);

			AssertEquals("Top handling unit package should be updated.", ZGuid.Empty, subHU.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", subHU.PK, inner1.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", ZGuid.Empty, inner2.KP_KP_TopHandlingUnitPackage);
		}

		public void TestDeletePackageDivots_DeleteSubHU()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Package tree
			//	TopHU
			//		SubHU
			//			Inner1
			//			BottomHU
			//				Inner2
			//		Inner3
			var topHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "TopHU");
			var subHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "SubHU");
			var bottomHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "BottomHU");
			var inner1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner1");
			var inner2 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner2");
			var inner3 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner3");

			// old divots
			var topHUSubHU_UnackedDivot = Helper.PackHandlingUnitAndUnpack(topHU, subHU);
			var subHUInner1_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(subHU, inner1);
			var subHUBottomHU_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(subHU, bottomHU);
			var bottomHUInner2_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(bottomHU, inner2);
			var topHUInner3_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(topHU, inner3);

			// new divots
			var topHUSubHU_PackedDivot = Helper.PackHandlingUnit(topHU, subHU, topHU);
			var subHUInner1_PackedDivot = Helper.PackHandlingUnit(subHU, inner1, topHU);
			var subHUBottomHU_PackedDivot = Helper.PackHandlingUnit(subHU, bottomHU, topHU);
			var bottomHUInner2_PackedDivot = Helper.PackHandlingUnit(bottomHU, inner2, topHU);
			var topHUInner3_PackedDivot = Helper.PackHandlingUnit(topHU, inner3, topHU);

			// attempt to delete package
			subHU.Delete();

			AssertEquals("Package should be deleted.", true, subHU.IsDeleted);
			AssertEquals("Package should not be deleted.", false, topHU.IsDeleted);
			AssertEquals("Package should not be deleted.", false, inner1.IsDeleted);
			AssertEquals("Package should not be deleted.", false, inner2.IsDeleted);
			AssertEquals("Package should not be deleted.", false, inner3.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUSubHU_UnackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUSubHU_PackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, subHUInner1_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, subHUInner1_PackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, subHUBottomHU_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, subHUBottomHU_PackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, bottomHUInner2_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, bottomHUInner2_PackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, topHUInner3_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, topHUInner3_PackedDivot.IsDeleted);

			AssertEquals("Top handling unit package should be updated.", ZGuid.Empty, topHU.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", ZGuid.Empty, bottomHU.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", ZGuid.Empty, inner1.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", bottomHU.PK, inner2.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", topHU.PK, inner3.KP_KP_TopHandlingUnitPackage);
		}

		public void TestDeletePackageDivots_DeleteInners()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Package tree
			//	TopHU
			//		SubHU
			//			Inner1
			//		Inner2
			var topHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "TopHU");
			var subHU = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "SubHU");
			var inner1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner1");
			var inner2 = packageJob.Packages.AddNew(Constants.PkgUnit.Box, "Inner2");

			// old divots
			var topHUSubHU_UnackedDivot = Helper.PackHandlingUnitAndUnpack(topHU, subHU);
			var subHUInner1_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(subHU, inner1);
			var topHUInner2_UnpackedDivot = Helper.PackHandlingUnitAndUnpack(topHU, inner2);

			// new divots
			var topHUSubHU_PackedDivot = Helper.PackHandlingUnit(topHU, subHU, topHU);
			var subHUInner1_PackedDivot = Helper.PackHandlingUnit(subHU, inner1, topHU);
			var topHUInner2_PackedDivot = Helper.PackHandlingUnit(topHU, inner2, topHU);

			// attempt to delete package
			inner1.Delete();
			inner2.Delete();

			AssertEquals("Package should be deleted.", true, inner1.IsDeleted);
			AssertEquals("Package should be deleted.", true, inner2.IsDeleted);
			AssertEquals("Package should not be deleted.", false, topHU.IsDeleted);
			AssertEquals("Package should not be deleted.", false, subHU.IsDeleted);
			AssertEquals("Divot should be deleted.", true, subHUInner1_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, subHUInner1_PackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUInner2_UnpackedDivot.IsDeleted);
			AssertEquals("Divot should be deleted.", true, topHUInner2_PackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, topHUSubHU_UnackedDivot.IsDeleted);
			AssertEquals("Divot should not be deleted.", false, topHUSubHU_PackedDivot.IsDeleted);

			AssertEquals("Top handling unit package should be updated.", ZGuid.Empty, topHU.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package should be updated.", topHU.PK, subHU.KP_KP_TopHandlingUnitPackage);
		}

		#endregion

		#endregion

		#region TestPackageActionStrategy_RestrictEdit_ReadOnly

		public void TestPackageActionStrategy_RestrictEdit_ReadOnly()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");

			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package) }
			};

			var actionStrategyNoPackUnpack = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.PackUnpack, "moo") }
			};

			var actionStrategyReadOnly = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package, new PackageActionStrategy(package, PackageAction.Edit, "moo") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNone, false);
			AssertEquals(false, package.ReadOnly);

			Data.Dummy.SetPackageActionResponses(actionStrategyReadOnly);
			AssertEquals(true, package.ReadOnly);

			Data.Dummy.SetPackageActionResponses(actionStrategyNone);
			AssertEquals(false, package.ReadOnly);

			Data.Dummy.SetPackageActionResponses(actionStrategyNoPackUnpack);
			AssertEquals(false, package.ReadOnly);
		}

		#endregion

		#region TestClone

		public void TestClone()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew();
			var box = pallet.Packages.AddNew("BOX");
			box.KP_PackageID = "ABC";
			box.UNDGs.AddNew();

			// test clone
			var clone = box.Clone();
			AssertEquals("BOX", clone.KP_F3_NKPackType);
			AssertEquals("Should not clone PackageID.", "", clone.KP_PackageID);
			AssertEquals("Should not clone FK to Parent Package.", true, clone.KP_KP_ParentPackage.IsEmpty);
			AssertEquals("Should not clone FK to PackageJob.", true, clone.KP_KJ_ParentPackageJob.IsEmpty);

			// test clone UNDGs
			AssertEquals(1, clone.UNDGs.Count);
		}

		public void TestClone_Container()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_Seal1 = "Seal";

			// test clone
			var clone = container.Clone();
			AssertEquals("CNT", clone.KP_F3_NKPackType);
			AssertEquals("Should not clone FK to PackageJob.", true, clone.KP_KJ_ParentPackageJob.IsEmpty);

			// test clone container
			AssertEquals("Seal", clone.Container.K0_Seal1);
		}

		#endregion

		#region TestNotes

		public void TestNotes()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			AssertEquals(false, packageJob.Packages.AddNew().SupportsNotes);
		}

		public void TestNoStmNoteDbHitsWhenDeletingPackages()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var palletGroup = packageJob.Packages.AddNew("PLT", 100);
			palletGroup.BreakDownIntoIndividualPackages(1); // creates 100 separate pallets
			Factory.Save();

			Array.ForEach(packageJob.Packages.ToArray(), p => p.Delete());
			Factory.Save();
			AssertEquals(
				"Deleting packages should not hit the StmNote table as Packages have no Notes.\r\n" +
				"\r\n" +
				"NOTE:\r\n" +
				"Multiple packages can be deleted via the GUI, resulting in a call to package.Delete() per package.\r\n" +
				"If support for Notes is needed, this would need to be batched in order to take advantage of the\r\n" +
				"architecture's StmNote fetch hint, resulting in *1* DB hit to StmNote for *all* deleted packages.\r\n\r\n",
				0, Factory.GetTableHitCount(StmNoteSchema.Constants.TableName));
		}

		#endregion

		#region TestIsTransitPackage

		public void TestIsTransitPackage()
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);

			var packageJob = Data.PackageJob;

			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
			AssertEquals("IsTransitPackage Should be false when there is no transit package", false, package.IsTransitPackage);

			whsHelper.CreateWhsItemPackageState("FIN", location, rtu, package);
			AssertEquals("IsTransitPackage Should be true when there is transit package with package", true, package.IsTransitPackage);
		}

		#endregion

		#region TestIsTransitOverpackPackage

		public void TestIsTransitOverpackPackage()
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);

			var packageJob = Data.PackageJob;

			var package1 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
			whsHelper.CreateWhsItemPackageState("ARV", location, rtu, package1, unitType: "HU");
			AssertEquals("IsTransitOverpackPackage Should be false when WPS_UnitType is not OVP", false, package1.IsTransitOverpackPackage);

			var package2 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
			whsHelper.CreateWhsItemPackageState("ARV", location, rtu, package2, unitType: "OVP");
			AssertEquals("IsTransitOverpackPackage Should be true when WPS_UnitType is OVP", true, package2.IsTransitOverpackPackage);
		}

		public void TestIsTransitOverpackPackage_AllowLoadingOverpackChildrenUndgs() => TestIsTransitOverpackPackage_AllowLoadingOverpackChildrenUndgsCore(true);

		public void TestIsTransitOverpackPackage_DisallowLoadingOverpackChildrenUndgs() => TestIsTransitOverpackPackage_AllowLoadingOverpackChildrenUndgsCore(false);

		public void TestIsTransitOverpackPackage_AllowLoadingOverpackChildrenUndgsCore(bool allowLoadingOverpackChildrenUndgs)
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);
			var rcn = whsHelper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var packageJob = Data.PackageJob;

			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
			var packageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, package);
			packageState[WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment] = rcn.PK;

			var ovpPackage = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
			var ovpPackageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, ovpPackage, unitType: "OVP");
			ovpPackageState[WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment] = rcn.PK;

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_ExceptedQuantityCode = "E1";
			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			package.UNDGs.Add(undgDataItem);

			Helper.PackHandlingUnit(ovpPackage, package, ovpPackage);

			Factory.Save();

			AssertEquals("IsTransitOverpackPackage Should be true when WPS_UnitType is OVP", true, ovpPackage.IsTransitOverpackPackage);

			ovpPackage.AllowLoadingOverpackChildrenUndgs = allowLoadingOverpackChildrenUndgs;
			AssertEquals("Overpack UNDGs should have childrens undgs if flag is set", ovpPackage.UNDGs.Count, allowLoadingOverpackChildrenUndgs ? 1 : 0);
		}

		#endregion

		#region TestToStringPackageSummary

		public void TestToStringPackageSummary()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			package.KP_F3_NKPackType = "";
			AssertEquals("1x ???", package.ToStringPackageSummary());

			package.KP_F3_NKPackType = "CNT";
			AssertEquals("1x Container", package.ToStringPackageSummary());

			package.KP_PackageQty = 7;
			AssertEquals("7x Containers", package.ToStringPackageSummary());
		}

		#endregion

		#region TestToStringDimensionSummary

		public void TestToStringDimensionSummary()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals("0.0 x 0.0 x 0.0 M", package.ToStringDimensionSummary());

			package.KP_Length = 5;
			package.KP_Width = 7.51;
			package.KP_Height = 10.2;
			package.KP_DimensionUQ = Constants.Length.Inches;
			AssertEquals("5.0 x 7.51 x 10.2 IN", package.ToStringDimensionSummary());
		}

		#endregion

		#region TestNotifyParentOfContainerIDChangeIfRequired

		public void TestNotifyParentOfContainerIDChangeIfRequired_PackageJobNotSaved()
		{
			Data.CreatePackingData();
			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Precondition.", 0, Data.Dummy.ContainersWithIDChange.Count);

			container.KP_PackageID = "CONT123";
			AssertEquals("Parent Package Job not in DB, so expect no changes.", 0, Data.Dummy.ContainersWithIDChange.Count);
		}

		public void TestNotifyParentOfContainerIDChangeIfRequired_PackageJobSaved()
		{
			Data.CreatePackingData();
			Factory.Save();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Precondition.", 0, Data.Dummy.ContainersWithIDChange.Count);

			container.KP_PackageID = "CONT123";
			AssertContainsExactElementsInAnyOrder("Parent Package Job in in DB, so expect a change.", new[] { container }, Data.Dummy.ContainersWithIDChange);
		}

		public void TestNotifyParentOfContainerIDChangeIfRequired_NotYetAContainer()
		{
			Data.CreatePackingData();
			Factory.Save();

			var container = Data.PackageJob.Packages.AddNew("PLT");
			AssertEquals("Precondition.", 0, Data.Dummy.ContainersWithIDChange.Count);

			container.KP_PackageID = "CONT123";
			AssertEquals("Container is a Pallet, expect no changes.", 0, Data.Dummy.ContainersWithIDChange.Count);

			container.KP_F3_NKPackType = "CNT";
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertContainsExactElementsInAnyOrder("Package is now a Container, so expect a change.", new[] { container }, Data.Dummy.ContainersWithIDChange);
		}

		public void TestNotifyParentOfContainerIDChangeIfRequired_ContainerWasSaved()
		{
			Data.CreatePackingData();
			Factory.Save();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			container.KP_PackageID = "CONT123";
			Factory.Save();
			Data.Dummy.ContainersWithIDChange.Clear();

			container.KP_PackageID = "CONT456";
			AssertContainsExactElementsInAnyOrder("Parent Package Job is in DB, so expect a change.", new[] { container }, Data.Dummy.ContainersWithIDChange);
		}

		public void TestNotifyParentOfContainerIDChangeIfRequired_TwoContainers()
		{
			Data.CreatePackingData();
			Factory.Save();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var container2 = Data.PackageJob.Packages.AddNew("CNT");
			container1.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			container2.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			container1.KP_PackageID = "CONT123";
			container2.KP_PackageID = "CONT456";
			AssertContainsExactElementsInAnyOrder("Parent Package Job is in DB, so expect a change.", new[] { container1, container2 }, Data.Dummy.ContainersWithIDChange);
		}

		#endregion

		#region TestDataChangeEvent

		public void TestDataChangeEvent()
		{
			Data.CreatePackingData();
			AssertChangeFiresDataChangeEvent(p => p.KP_PackageQty = 2, p => p.KP_F3_NKPackType = "DRM", PackageDataChangeType.PackageContent);
			AssertChangeFiresDataChangeEvent(p => p.KP_Volume = 3, p => p.KP_VolumeUQ = "CF", PackageDataChangeType.Volume);
			AssertChangeFiresDataChangeEvent(p => p.KP_Weight = 10, p => p.KP_WeightUQ = "G", PackageDataChangeType.Weight);
		}

		void AssertChangeFiresDataChangeEvent(Action<PkgPackage> setValue, Action<PkgPackage> setUnit, PackageDataChangeType expectedDataChangeType)
		{
			var outerPackage = Data.PackageJob.Packages.AddNew();

			PackageDataChangeType? actualDataChangeType = null;
			int eventFired = 0;
			Data.PackageJob.PackageDataChanged += (s, e) =>
			{
				eventFired++;
				actualDataChangeType = e.DataChangeType;
			};

			setValue(outerPackage);
			AssertEquals("Should fire when there has been a change.", 1, eventFired);
			AssertEquals("Should fire with correct data type.", expectedDataChangeType, actualDataChangeType);

			setValue(outerPackage);
			AssertEquals("Should NOT fire when there has been NO change.", 1, eventFired);

			setUnit(outerPackage);
			AssertEquals("Should fire again cause there has been another change.", 2, eventFired);
			AssertEquals("Should fire with correct data type.", expectedDataChangeType, actualDataChangeType);

			setUnit(outerPackage);
			AssertEquals("Should NOT fire again cause there has been NO change.", 2, eventFired);
		}

		#endregion

		// interface members

		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var package = Factory.New<PkgPackage>();
			var documentSupportable = (IDocumentSupportable)package;

			AssertEquals(package, documentSupportable.DocumentSupporter.BusinessObject);
			AssertEquals(typeof(PkgPackageDocumentSupporter), documentSupportable.DocumentSupporter.GetType());
		}

		#endregion

		#region TestIPackageSummary

		public void TestIPackageSummary()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var packageSummary = (IPackageSummary)package;

			package.KP_F3_NKPackType = "PLT";
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package.KP_Length = 2.5;
			package.KP_Width = 1.5;
			package.KP_Height = 0.5;
			package.KP_Weight = 1021;
			package.KP_Volume = 2010.5126;
			package.KP_RH_NKCommodityCode = "GEN";
			package.KP_IsHeld = true;

			AssertEquals("Dims", packageSummary.DimensionsCaption);
			AssertEquals("ID", packageSummary.PackageIDCaption);
			AssertEquals("Wgt", packageSummary.WeightCaption);
			AssertEquals("Vol", packageSummary.VolumeCaption);
			AssertEquals("Comm", packageSummary.CommodityCaption);
			AssertEquals("Held", packageSummary.IsHeldCaption);
			AssertEquals("2.5 x 1.5 x 0.5 M", packageSummary.Dimensions);
			AssertEquals("---", packageSummary.PackageID);
			AssertEquals("2,010.513 M3", packageSummary.Volume);
			AssertEquals("1,021 KG", packageSummary.Weight);
			AssertEquals("GEN", packageSummary.Commodity);
			AssertEquals("Yes", packageSummary.IsHeld);
			package.KP_IsHeld = false;
			AssertEquals("No", packageSummary.IsHeld);
			AssertEquals("", packageSummary.Contents);

			package.KP_PackageID = "SSCC123";
			AssertEquals("SSCC123", packageSummary.PackageID);

			package.Packages.AddNew("BOX");
			package.Packages.AddNew("BOX");
			AssertEquals("2x BOX", packageSummary.Contents);

			package.KP_RH_NKCommodityCode = "";
			AssertEquals("", packageSummary.CommodityCaption);
		}

		public void TestIPackageSummary_Status()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var packageSummary = (IPackageSummary)package;
			AssertEquals("Status significance is not used by PkgPackage at this time.", false, packageSummary.IsStatusSignificant);
			AssertEquals("", packageSummary.Status);

			// with no scan event

			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(PackageStatuses.Closed, packageSummary.Status);

			package.KP_IsReleasedViaJob = true;
			AssertEquals(PackageStatuses.ReleasedViaJob, packageSummary.Status);

			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(PackageStatuses.Released, packageSummary.Status);

			// with a scan event
			Thread.Sleep(2); // avoid intermittent failures if execution of adding the events is within 1ms
			package.ScanEventsForBindingOnly.AddNew(Events.CommercialInvoiceReceived);
			AssertEquals(string.Format("{0}, {1}", PackageStatuses.Released, Events.CommercialInvoiceReceived.Description), packageSummary.Status);

			Thread.Sleep(2); // avoid intermittent failures if execution of adding the events is within 1ms
			package.ScanEventsForBindingOnly.AddNew(Events.Delivered);
			AssertEquals("Should show the most recent event.", string.Format("{0}, {1}", PackageStatuses.Released, Events.Delivered.Description), packageSummary.Status);

			package.KP_ReleasedTimeUtc = ZDateTime.Empty;
			package.KP_IsReleasedViaJob = false;
			package.KP_ClosedTimeUtc = ZDateTime.Empty;
			AssertEquals(string.Format("{0}", Events.Delivered.Description), packageSummary.Status);
		}

		public void TestIPackageSummary_HandlingUnit_InnerPackage()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var packageSummary = (IPackageSummary)package;
			var innerPackage = package.Packages.AddNew();
			var innerPackageSummary = (IPackageSummary)innerPackage;

			AssertEquals("", packageSummary.HandlingUnitCaption);
			AssertEquals("", packageSummary.HandlingUnit);
			AssertEquals("", innerPackageSummary.HandlingUnitCaption);
			AssertEquals("", innerPackageSummary.HandlingUnit);

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "3PL";
			var handlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			handlingUnitJob.KJ_ParentID = handlingUnit.PK;
			handlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var handlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			handlingUnitPackage.KP_KJ_ParentPackageJob = handlingUnitJob.PK;
			handlingUnitPackage.KP_PackageID = "HU00000001";
			Helper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			AssertEquals("HU", packageSummary.HandlingUnitCaption);
			AssertEquals("HU00000001", packageSummary.HandlingUnit);
			AssertEquals("", innerPackageSummary.HandlingUnitCaption);
			AssertEquals("", innerPackageSummary.HandlingUnit);
		}

		public void TestIPackageSummary_HandlingUnit_OuterPackage()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var packageSummary = (IPackageSummary)package;

			AssertEquals("", packageSummary.HandlingUnitCaption);
			AssertEquals("", packageSummary.HandlingUnit);

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "3PL";
			var handlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			handlingUnitJob.KJ_ParentID = handlingUnit.PK;
			handlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var handlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			handlingUnitPackage.KP_KJ_ParentPackageJob = handlingUnitJob.PK;
			handlingUnitPackage.KP_PackageID = "HU1";
			Helper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			AssertEquals("HU", packageSummary.HandlingUnitCaption);
			AssertEquals("HU1", packageSummary.HandlingUnit);
		}

		public void TestIPackageSummary_HandlingUnitPackedInAnotherHandlingUnit()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var packageSummary = (IPackageSummary)package;

			AssertEquals("", packageSummary.HandlingUnitCaption);
			AssertEquals("", packageSummary.HandlingUnit);

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "3PL";
			var handlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			handlingUnitJob.KJ_ParentID = handlingUnit.PK;
			handlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var handlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			handlingUnitPackage.KP_KJ_ParentPackageJob = handlingUnitJob.PK;
			handlingUnitPackage.KP_PackageID = "HU1";

			var topHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			topHandlingUnit.KPU_JobContext = "3PL";
			var topHandlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			topHandlingUnitJob.KJ_ParentID = topHandlingUnit.PK;
			topHandlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			var topHandlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			topHandlingUnitPackage.KP_KJ_ParentPackageJob = topHandlingUnitJob.PK;
			topHandlingUnitPackage.KP_PackageID = "TopHU1";

			Helper.PackHandlingUnit(handlingUnitPackage, package, topHandlingUnitPackage);

			AssertEquals("HU", packageSummary.HandlingUnitCaption);
			AssertEquals("TopHU1", packageSummary.HandlingUnit);
		}

		public void TestStatus_Closed_ExcludesPackingCompleteEevent()
		{
			Data.CreatePackingData();

			var outerNonContainerPackage = Data.PackageJob.Packages.AddNew();
			outerNonContainerPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("PKC (Packing Complete) event exists on package.", true, outerNonContainerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			AssertEquals("Closed", outerNonContainerPackage.Status.Trim());
			AssertNotContains("Packing Completed", outerNonContainerPackage.Status);
		}

		public void TestStatus_ExcludesDataExportEvent()
		{
			Data.CreatePackingData();

			var outerNonContainerPackage = Data.PackageJob.Packages.AddNew();
			outerNonContainerPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("PKC (Packing Complete) event exists on package.", true, outerNonContainerPackage.Logs.Find(l => l.SL_SE_NKEvent == Events.PackingCompleted.Code).Any());

			AssertEquals("Closed", outerNonContainerPackage.Status.Trim());
			AssertNotContains("Packing Completed", outerNonContainerPackage.Status);

			outerNonContainerPackage.Logs.AddNew(Events.DataExport);
			AssertEquals("Closed", outerNonContainerPackage.Status.Trim());
			AssertNotContains("Data Export", outerNonContainerPackage.Status);
		}

		public void TestIPackageSummary_Temperature()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var packageSummary = (IPackageSummary)package;
			AssertEquals(false, packageSummary.IsTemperatureControlled);
			AssertEquals("", packageSummary.TemperatureCaption);

			package.KP_RequiresTemperatureControl = true;
			AssertEquals(true, packageSummary.IsTemperatureControlled);
			AssertEquals("Temp.", packageSummary.TemperatureCaption);

			package.KP_RequiredTemperatureMinimum = -10m;
			package.KP_RequiredTemperatureMaximum = -4m;
			package.KP_RequiredTemperatureUnit = "C";
			AssertEquals("-10 to -4 °C", packageSummary.Temperature);

			package.KP_RequiredTemperatureMinimum = -4m;
			package.KP_RequiredTemperatureMaximum = -4m;
			AssertEquals("-4 °C", packageSummary.Temperature);

			package.KP_F3_NKPackType = "CNT";
			package.Container.K0_IsControlledAtmosphere = false;
			AssertEquals(false, packageSummary.IsTemperatureControlled);

			package.Container.K0_IsControlledAtmosphere = true;
			AssertEquals(true, packageSummary.IsTemperatureControlled);

			package.Container.K0_SetPointTemp = 40;
			package.Container.K0_SetPointTempUnit = "F";
			AssertEquals("40 °F", packageSummary.Temperature);
		}

		#endregion

		#region TestIProcessHandlingInfoProvider

		public void TestIProcessHandlingInfoProvider()
		{
			var package = (IProcessHandlingInfoProvider)Factory.New<PkgPackage>();
			AssertEquals(typeof(PkgPackageProcessHandlingInfo), package.ProcessHandlingInfo.GetType());

			// actual propagation is tested in PkgPackageProcessHandlingInfo
		}

		#endregion

		#region TestITemperatureSettings

		public void TestITemperatureSettings()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var packageTemperatureSettings = (ITemperatureSettings)package;

			packageTemperatureSettings.IsTemperatureControlled = true;
			AssertEquals(true, package.KP_RequiresTemperatureControl);
			package.KP_RequiresTemperatureControl = false;
			AssertEquals(false, packageTemperatureSettings.IsTemperatureControlled);

			packageTemperatureSettings.TemperatureMin = 7m;
			AssertEquals(7m, package.KP_RequiredTemperatureMinimum);
			package.KP_RequiredTemperatureMinimum = 14m;
			AssertEquals(14m, packageTemperatureSettings.TemperatureMin);

			packageTemperatureSettings.TemperatureMax = 6m;
			AssertEquals(6m, package.KP_RequiredTemperatureMaximum);
			package.KP_RequiredTemperatureMaximum = 5m;
			AssertEquals(5m, packageTemperatureSettings.TemperatureMax);

			packageTemperatureSettings.TemperatureUnit = Constants.Temperature.Centigrade;
			AssertEquals(Constants.Temperature.Centigrade, package.KP_RequiredTemperatureUnit);
			package.KP_RequiredTemperatureUnit = Constants.Temperature.Fahrenheit;
			AssertEquals(Constants.Temperature.Fahrenheit, packageTemperatureSettings.TemperatureUnit);
		}

		#endregion

		#region TestIUNDGDataItemProvider

		public void TestIUNDGDataItemProvider()
		{
			var package = Factory.New<PkgPackage>();

			AssertNotNull(package.UNDGs);
			AssertEquals(true, package.IsRegisteredEditableChildObject(package.UNDGs));
			AssertEquals("UNDG should be cached.", package.UNDGs, package.UNDGs);
		}

		#endregion

		#region TestIJobNumberMembers

		public void TestIJobNumberMembers()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var packageWithID = packageJob.Packages.AddNew("PLT");
			packageWithID.KP_PackageID = "ID-123";
			AssertEquals("ID-123", ((IJobNumber)packageWithID).JobNumber);
		}

		#endregion

		#region TestIEDocsProvider

		public void TestGetEDocsProviderSupporter()
		{
			var packageProvider = (IEDocsProvider)Factory.New<PkgPackage>();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(EDocsProviderSupporter), packageProvider.GetEDocsProviderSupporter().GetType());
			AssertEquals("DocManagerInfo should be of type PKG", "PKG", packageProvider.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestISupportPackageIDGeneration

		public void TestISupportPackageIDGeneration()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("PLT");
			package.KP_PackageID = "ABC";
			package.KP_PackageQty = 4;
			var iPackage = (ISupportPackageIDGeneration)package;
			AssertEquals(false, iPackage.IsContainer);
			AssertEquals("ABC", iPackage.KP_PackageID);
			AssertEquals(4, iPackage.KP_PackageQty);
			AssertEquals(package.PackageJob, iPackage.PackageJob);
			AssertEquals(package.Packages, iPackage.Packages);
			AssertEquals(package.Factory, iPackage.Factory);
			AssertEquals(false, iPackage.ShouldGenerateIDOnSaving);
			AssertEquals(Data.PackageJob.PK, iPackage.PackageJobPK);

			package.KP_F3_NKPackType = "CNT";
			iPackage.KP_PackageID = "DEF";
			iPackage.ShouldGenerateIDOnSaving = true;
			AssertEquals(true, iPackage.IsContainer);
			AssertEquals("DEF", package.KP_PackageID);
			AssertEquals(true, iPackage.ShouldGenerateIDOnSaving);
		}

		public void TestCallAfterIDGenerated()
		{
			Data.CreatePackingData();

			ISupportPackageIDGenerationInternals package = Data.PackageJob.Packages.AddNew("PLT");
			AssertNoExceptionThrown(package.CallAfterIDGenerated);

			var count = 0;
			EventHandler afterIdEvent = (s, e) => count++;
			package.AfterIDGenerated += afterIdEvent;
			AssertEquals("Precondition.", 0, count);

			package.CallAfterIDGenerated();
			AssertEquals("Should have fired AfterIDGenerated.", 1, count);

			package.CallAfterIDGenerated();
			AssertEquals("Should have fired AfterIDGenerated.", 2, count);

			package.AfterIDGenerated -= afterIdEvent;
			package.CallAfterIDGenerated();
			AssertEquals("Should *not* have fired AfterIDGenerated.", 2, count);
		}

		#endregion

		#region TestIPackageIDSequence

		public void TestIPackageIDSequence()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("CNT", "P1");
			var packageIDSequence = (IPackageSequence)package;

			AssertEquals((ZShort)1, packageIDSequence.Sequence);
			AssertEquals(package.KP_KPH_PackageHeader, packageIDSequence.PackageHeaderFK);
			AssertEquals(package.KP_KJ_ParentPackageJob, packageIDSequence.PackageJobFK);
			AssertEquals(true, packageIDSequence.RequiresSequencing);

			var package2 = Data.PackageJob.Packages.AddNew("CNT", "P2");
			package.KP_KP_ParentPackage = package2.PK;
			AssertEquals(false, packageIDSequence.RequiresSequencing);

			packageIDSequence.Sequence = 2;
			AssertEquals((ZShort)2, packageIDSequence.Sequence);
		}

		#endregion

		#region TestCodeProperty

		public void TestCodeProperty()
		{
			const string packageID = "TESTPACKID";
			var package = Factory.New<PkgPackage>();
			var packageHeader = Factory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = packageID;
			package.KP_KPH_PackageHeader = packageHeader.PK;

			var code = ((ICodeDescription)package).Code;
			AssertNotNull("Package code should not be null", code);
			AssertEquals("Package code should equal package ID", packageID, code);
		}

		#endregion

		#region TestPackageLineTriggers

		public void TestPackageLineTriggers()
		{
			Data.CreatePackingData();
			var packingParent = Data.Dummy;
			var package1 = Data.PackageJob.Packages.AddNew("PLT");
			var package2 = Data.PackageJob.Packages.AddNew("PLT");
			var trigger = packingParent.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());

			AssertNotNull(trigger.WorkflowDescriptor);

			package1.GetLogs().AddNew(Events.CustomisableEvent00);
			package2.GetLogs().AddNew(Events.CustomisableEvent00);
			AssertEquals(2, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
		}

		static void SetupEmailNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "bung@bung.bung";
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var package1 = Factory.New<PkgPackage>();
			AssertEquals(1, package1.KP_PackageQty);
			AssertEquals(PackingRegistry.Instance.DimensionUnit.Value, package1.KP_DimensionUQ);
			AssertEquals(PackingRegistry.Instance.VolumeUnit.Value, package1.KP_VolumeUQ);
			AssertEquals(PackingRegistry.Instance.WeightUnit.Value, package1.KP_WeightUQ);

			PackingRegistry.Instance.SetDimensionUnitForTest(Constants.Length.Yards);
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.TeaChest);
			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.MetricCarat);

			var package2 = Factory.New<PkgPackage>();
			AssertEquals(PackingRegistry.Instance.DimensionUnit.Value, package2.KP_DimensionUQ);
			AssertEquals(PackingRegistry.Instance.VolumeUnit.Value, package2.KP_VolumeUQ);
			AssertEquals(PackingRegistry.Instance.WeightUnit.Value, package2.KP_WeightUQ);
			AssertEquals(Constants.Length.Yards, package2.KP_DimensionUQ);
			AssertEquals(Constants.Volume.TeaChest, package2.KP_VolumeUQ);
			AssertEquals(Constants.Weight.MetricCarat, package2.KP_WeightUQ);
		}

		#endregion

		#region TestSetDefaultValue_CheckMaximumLength

		public void TestSetDefaultValue_CheckMaximumLength_WeightUnit()
		{
			var package1 = Factory.New<PkgPackage>();
			AssertEquals(PackingRegistry.Instance.WeightUnit.Value, package1.KP_WeightUQ);

			using (PackingRegistry.Instance.WeightUnit.DataType.SuspendValidation())
			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC"))
			{
				AssertExceptionThrown(typeof(ApplicationException), () => Factory.New<PkgPackage>()); // not ArgumentException: Cannot set column 'KP_WeightUQ'. The value violates the MaxLength limit of this column.
				AssertEquals("The maximum length of 'KP_WeightUQ' has been exceeded.\n The maximum length of this property is 2 characters, but 3 were entered. New value: ABC. Old value: ", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSetDefaultValue_CheckMaximumLength_VolumeUnit()
		{
			var package1 = Factory.New<PkgPackage>();
			AssertEquals(PackingRegistry.Instance.VolumeUnit.Value, package1.KP_VolumeUQ);

			using (PackingRegistry.Instance.VolumeUnit.DataType.SuspendValidation())
			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC"))
			{
				AssertExceptionThrown(typeof(ApplicationException), () => Factory.New<PkgPackage>()); // not ArgumentException: Cannot set column 'KP_VolumeUQ'. The value violates the MaxLength limit of this column.
				AssertEquals("The maximum length of 'KP_VolumeUQ' has been exceeded.\n The maximum length of this property is 2 characters, but 3 were entered. New value: ABC. Old value: ", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSetDefaultValue_CheckMaximumLength_DimensionUnit()
		{
			var package1 = Factory.New<PkgPackage>();
			AssertEquals(PackingRegistry.Instance.DimensionUnit.Value, package1.KP_DimensionUQ);

			using (PackingRegistry.Instance.DimensionUnit.DataType.SuspendValidation())
			using (PackingRegistry.Instance.DimensionUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC"))
			{
				AssertExceptionThrown(typeof(ApplicationException), () => Factory.New<PkgPackage>()); // not ArgumentException: Cannot set column 'KP_DimensionUQ'. The value violates the MaxLength limit of this column.
				AssertEquals("The maximum length of 'KP_DimensionUQ' has been exceeded.\n The maximum length of this property is 2 characters, but 3 were entered. New value: ABC. Old value: ", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			AssertEquals("Human readable name for package without package id is correct.", "Package", package.HumanReadableName);

			package.KP_PackageID = "ABC";
			AssertEquals("Human readable name for package with package id is correct.", "Package ABC", package.HumanReadableName);
		}

		#endregion

		#region TestCanPrintCarrierLabel

		public void TestCanPrintCarrierLabel()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			AssertEquals(true, package.CanPrintCarrierLabel);
		}

		public void TestCanPrintCarrierLabel_PackageJobTypeIsNotConfigured()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			AssertEquals(false, package.CanPrintCarrierLabel);
		}

		public void TestCanPrintCarrierLabel_PackageWithNoPackageID()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT");
			Factory.Save();

			AssertEquals(false, package.CanPrintCarrierLabel);
		}

		public void TestCanPrintCarrierLabel_PackageWithNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Factory.Save();

			AssertEquals(false, package.CanPrintCarrierLabel);
		}

		#endregion

		#region TestCancelPackageLabel

		public void TestCancelPackageLabel()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Precondition: Package can cancel carrier label.", true, package.CanCancelPackageLabel);
			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);

				cancellationMock.Setup(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(pkg => pkg.PK == package.PK)))
					.Returns(new ReturnResult { Success = true });

				cancellationMock.Setup(c => c.CancelPackages(It.Is<IEnumerable<PkgPackage>>(i => i.Select(p => p.PK).ContainsSameElementsInAnyOrder(new[] { package.PK }))))
					.Returns(new[] { new CancellationResponse(Data.PackageJob.PK, Data.Dummy, responseMock.Object, "ABC-1") });

				var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });
				AssertEquals(true, result.Success);
				Factory.Save();
				cancellationMock.VerifyAll();

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Success Log.", 1, notes.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-1'.", notes[0].ST_NoteText);

				var newFactory = new BusinessObjectFactory();
				var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
				AssertEquals(false, packageInNewFactory.IsSentToRTUS);
			}
		}

		public void TestCancelPackageLabel_SubscriptionFails()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();
			var exceptionToThrow = new InvalidOperationException("Error!");

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationFailure(Data.Dummy, exceptionToThrow))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package.IsSentToRTUS = true;
				Factory.Save();

				var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });
				AssertEquals(true, result.Success);
				Factory.Save();

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Failure Log.", 1, notes.Length);
				AssertEquals("Should have generated a Failure Log.",
					"Error occurred during Cancellation Process of Carrier Label(s). Packages for label cancellation must be canceled manually. Error: Failed to generate Universal Shipment from Package 'ABC-1'.\r\nDetails below:\r\nError!", notes[0].ST_NoteText);
			}
		}

		public void TestCancelPackageLabel_Integration()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package.IsSentToRTUS = true;
				Factory.Save();

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });
				Factory.Save();
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)));
				responseMock.VerifyGet(r => r.IsSuccessful);

				AssertEquals("IsSentToRTUS is reverted to false.", false, package.IsSentToRTUS);
				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated Success Logs.", 1, notes.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-1'.", notes[0].ST_NoteText);
			}
		}

		public void TestCancelPackageLabel_FactorySaveFails()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Precondition: Package can cancel carrier label.", true, package.CanCancelPackageLabel);
			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			{
				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);

				cancellationMock.Setup(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(pkg => pkg.PK == package.PK)))
					.Returns(new ReturnResult { Success = true });

				cancellationMock.Setup(c => c.CancelPackages(It.Is<IEnumerable<PkgPackage>>(i => i.Select(p => p.PK).ContainsSameElementsInAnyOrder(new[] { package.PK }))))
					.Returns(new[] { new CancellationResponse(Data.PackageJob.PK, Data.Dummy, responseMock.Object, "ABC-1") });

				var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });
				var preventGenerationService = new PackageIDGenerationHelperTest.PreventGenerationService();
				Factory.ServiceContainer.AddAfterOnSavingService(preventGenerationService);
				AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());

				AssertEquals("After save, package label is not cancelled.", true, package.IsSentToRTUS);
				var notes1 = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated Logs.", 0, notes1.Length);

				Factory.ServiceContainer.RemoveAfterOnSavingService<PackageIDGenerationHelperTest.PreventGenerationService>();
				Factory.Save();

				cancellationMock.Verify(c => c.SubscribePackageForCancellation(It.Is<PkgPackage>(p => p.PK == package.PK)), Times.Once);
				cancellationMock.VerifyAll();

				AssertEquals("After save, package label is cancelled.", false, package.IsSentToRTUS);
				var notes2 = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated succesful Logs.", 1, notes2.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-1'.", notes2[0].ST_NoteText);
			}
		}

		public void TestCancelPackageLabel_FactorySaveFails_ClearsPackagesToCancelHashSet()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			var managerMock = new Mock<ICarrierLabelManager>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package.IsSentToRTUS = true;
				Factory.Save();

				var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });
				var preventGenerationService = new PackageIDGenerationHelperTest.PreventGenerationService();
				Factory.ServiceContainer.AddAfterOnSavingService(preventGenerationService);
				AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());

				AssertEquals("After save, package label is not cancelled.", true, package.IsSentToRTUS);
				var notes1 = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated Logs.", 0, notes1.Length);

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				Factory.ServiceContainer.RemoveAfterOnSavingService<PackageIDGenerationHelperTest.PreventGenerationService>();
				package.ShouldCancelCarrierLabelOnSaving = false; // set flag to false so that the package does not get added to hash set on factory saving
				Factory.Save();

				AssertEquals("After save, package label is not cancelled.", true, package.IsSentToRTUS);
				var notes2 = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated succesful Logs.", 0, notes2.Length);
			}
		}

		public void TestCancelPackageLabel_FactorySaveFails_ClearsPackagesToCancelHashSet_DeletedPackageGetsCancelled()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			var managerMock = new Mock<ICarrierLabelManager>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package1.IsSentToRTUS = true;

				var package2 = Data.PackageJob.Packages.AddNew("PLT", "ABC-2");
				package2.IsSentToRTUS = true;
				Factory.Save();

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				var preventGenerationService = new PackageIDGenerationHelperTest.PreventGenerationService();
				Factory.ServiceContainer.AddAfterOnSavingService(preventGenerationService);
				var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package1 });
				package2.Delete();

				try
				{
					Factory.Save();
				}
				catch (ZCannotSaveException ex)
				{
					AssertEquals("Exception caught should be expected one.", "Cannot save for test", ex.Message);
				}

				AssertEquals("After save, package label is not cancelled.", true, package1.IsSentToRTUS);
				AssertEquals("After save, deleted package has label cancelled.", false, package2.IsSentToRTUS);
				var notes1 = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated succesful Logs.", 0, notes1.Length);

				Factory.ServiceContainer.RemoveAfterOnSavingService<PackageIDGenerationHelperTest.PreventGenerationService>();
				package1.ShouldCancelCarrierLabelOnSaving = false; // set flag to false so that the package does not get added to hash set on factory saving
				Factory.Save();

				AssertEquals("After save, package label is not cancelled.", true, package1.IsSentToRTUS);
				var notes2 = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated succesful Log for the deleted package.", 1, notes2.Length);
				AssertEquals("Should have generated a Success Log.", "Successfully canceled Package 'ABC-2'.", notes2[0].ST_NoteText);
			}
		}

		public void TestCancelPackageLabel_MultiplePackageCancellations_Integration()
		{
			var dummyParent = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummyParent);

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			dummyParent.CarrierBookingAgent = smartFreight;

			var package1 = packageJob.Packages.AddNew("PLT", "ABC-1");
			var package2 = packageJob.Packages.AddNew("PLT", "ABC-2");
			package1.IsSentToRTUS = true;
			package2.IsSentToRTUS = true;
			Factory.Save();

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			Func<PkgPackage, string> getUniversalXml = p => GetUniversalShipmentXML("SUCCESS");
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(new[] { dummyParent }, getUniversalXml))
			{
				var successResponse = new Mock<IRTUSResponse>();
				successResponse.SetupGet(r => r.IsSuccessful).Returns(true);

				Func<ITopLevelDataObject, string, bool> funcWrapper = UniversalShipmentHasReference;
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(d => funcWrapper(d, "SUCCESS")), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(successResponse.Object);

				packageJob.CancelPackageCarrierLabel(new[] { package1, package2 });
				Factory.Save();
				managerMock.VerifyAll();

				AssertEquals("IsSentToRTUS is reverted to false.", false, package1.IsSentToRTUS);
				AssertEquals("IsSentToRTUS is reverted to false.", false, package2.IsSentToRTUS);

				var notes = dummyParent.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated Success Logs.", 2, notes.Length);
				AssertEquals("Should have generated a Success Log.", true, notes.Any(note => note.ST_NoteText == "Successfully canceled Package 'ABC-1'."));
				AssertEquals("Should have generated a Success Log.", true, notes.Any(note => note.ST_NoteText == "Successfully canceled Package 'ABC-2'."));
			}

			bool UniversalShipmentHasReference(ITopLevelDataObject universalShipment, string reference)
			{
				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(universalShipment, stream);

					stream.Position = 0;

					using (var reader = new StreamReader(stream))
					{
						var xml = reader.ReadToEnd();
						return xml.Contains($"<BookingConfirmationReference>{reference}</BookingConfirmationReference>");
					}
				}
			}
		}

		public void TestCancelPackageLabel_MultiplePackageCancellations_OneFails_Integration()
		{
			var dummyParent1 = Factory.New<DummyWithPacking>();
			var dummyParent2 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent2);

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			dummyParent1.CarrierBookingAgent = smartFreight;
			dummyParent2.CarrierBookingAgent = smartFreight;

			var package1 = packageJob1.Packages.AddNew("PLT", "ABC-1");
			var package2 = packageJob2.Packages.AddNew("PLT", "ABC-2");
			package1.IsSentToRTUS = true;
			package2.IsSentToRTUS = true;
			Factory.Save();

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			Func<PkgPackage, string> getUniversalXml = p =>
			{
				return p.PK == package1.PK
					? GetUniversalShipmentXML("SUCCESS")
					: GetUniversalShipmentXML("FAIL");
			};

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(new[] { dummyParent1, dummyParent2 }, getUniversalXml))
			{
				var successResponse = new Mock<IRTUSResponse>();
				successResponse.SetupGet(r => r.IsSuccessful).Returns(true);

				Func<ITopLevelDataObject, string, bool> funcWrapper = UniversalShipmentHasReference;
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(d => funcWrapper(d, "SUCCESS")), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(successResponse.Object);

				var failResponse = new Mock<IRTUSResponse>();
				failResponse.SetupGet(r => r.IsSuccessful).Returns(false);
				failResponse.SetupGet(r => r.ErrorMessageForFailure).Returns("ERROR");
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<ITopLevelDataObject>(d => funcWrapper(d, "FAIL")), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(failResponse.Object);

				packageJob1.CancelPackageCarrierLabel(new[] { package1 });
				packageJob2.CancelPackageCarrierLabel(new[] { package2 });
				Factory.Save();
				managerMock.VerifyAll();

				AssertEquals(false, package1.IsSentToRTUS);
				var note1 = dummyParent1.Notes.FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description).Single();
				AssertEquals("Should have success log.", "Successfully canceled Package 'ABC-1'.", note1.ST_NoteText);

				AssertEquals(true, package2.IsSentToRTUS);
				var note2 = dummyParent2.Notes.FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description).Single();
				AssertEquals("Should have failure log.", "ERROR", note2.ST_NoteText);
			}

			bool UniversalShipmentHasReference(ITopLevelDataObject universalShipment, string reference)
			{
				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(universalShipment, stream);

					stream.Position = 0;

					using (var reader = new StreamReader(stream))
					{
						var xml = reader.ReadToEnd();
						return xml.Contains($"<BookingConfirmationReference>{reference}</BookingConfirmationReference>");
					}
				}
			}
		}

		public void TestCancelPackageLabel_ErrorWithCancellationRequest_Integration()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var parentUniversalShipment = GetUniversalShipmentXML("TESTREFERENCE");
			var dataTransferTestHelper = ObjectFactory.New<IPackingDataTransferTestHelper>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (dataTransferTestHelper.MockUniversalShipmentGenerationForPackage(Data.Dummy, parentUniversalShipment))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
				package.IsSentToRTUS = true;
				Factory.Save();

				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(false);
				responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("Some Error");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				Data.PackageJob.CancelPackageCarrierLabel(new[] { package });
				Factory.Save();
				managerMock.VerifyAll();

				AssertEquals("IsSentToRTUS is not reverted to false.", true, package.IsSentToRTUS);

				var notes = Data.Dummy.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a failure Log.", 1, notes.Length);
				AssertEquals("Should have generated a failure Log.", "Some Error", notes[0].ST_NoteText);
			}
		}

		#endregion

		#region TestKPH_PackageIDMaxLengthIsMatchWithMaxTrackingNumberLength

		public void TestKPH_PackageIDMaxLengthIsMatchWithMaxTrackingNumberLength()
		{
			AssertEquals("Please check if KPH_PackageID length has changed in the database and update MaxTrackingNumberLength accordingly.", RTUSConstants.MaxTrackingNumberLength, PkgPackageHeaderSchema.KPH_PackageID.MaxLength);
		}

		#endregion

		#region TestCanCancelPackageLabel

		public void TestCanCancelPackageLabel()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals(true, package.CanCancelPackageLabel);
		}

		public void TestTestCancelPackageLabel_NotSentToRTUS()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.IsSentToRTUS = false;
			Factory.Save();

			AssertEquals(false, package.CanCancelPackageLabel);
		}

		public void TestTestCancelPackageLabel_PackageJobTypeIsNotConfigured()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals(false, package.CanCancelPackageLabel);
		}

		public void TestTestCancelPackageLabel_PackageWithNoPackageID()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals(false, package.CanCancelPackageLabel);
		}

		public void TestTestCancelPackageLabel_PackageWithNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals(false, package.CanCancelPackageLabel);
		}

		#endregion

		#region TestValidUnitsObeyDbConstraint

		public void TestValidUnitsObeyDbConstraint_Weight()
		{
			AssertNoExceptionThrown("New unit of weight must be added to PkgPackage database constraint.", () =>
			{
				Data.CreatePackingData();
				foreach (var unit in Constants.Weight.Codes)
				{
					var package = Data.PackageJob.Packages.AddNew();
					package.KP_WeightUQ = unit;
					Factory.Save();
				}
			});
		}

		public void TestValidUnitsObeyDbConstraint_Volume()
		{
			AssertNoExceptionThrown("New unit of volume must be added to PkgPackage database constraint.", () =>
			{
				Data.CreatePackingData();
				foreach (var unit in Constants.Volume.Codes)
				{
					var package = Data.PackageJob.Packages.AddNew();
					package.KP_VolumeUQ = unit;
					Factory.Save();
				}
			});
		}

		public void TestValidUnitsObeyDbConstraint_Dimension()
		{
			AssertNoExceptionThrown("New unit of dimension must be added to PkgPackage database constraint.", () =>
			{
				Data.CreatePackingData();
				foreach (var unit in Constants.Length.Codes)
				{
					var package = Data.PackageJob.Packages.AddNew();
					package.KP_DimensionUQ = unit;
					Factory.Save();
				}
			});
		}

		#endregion

		#region TestIPackingHasChanges

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnNewPackageAdded()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			job.Packages.AddNew("PLT");
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}
		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnPackageWeightModified()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			Factory.Save();
			// Change CriticalChangesVersionID to empty after Save.
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;
			package.KP_Weight = 2;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}
		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnPackageDeleted()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package1 = job.Packages.AddNew("PLT");
			var package2 = job.Packages.AddNew("BOX");
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;
			package2.Delete();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		#endregion

		#region Test_KP_SegregationCheckResult

		public void Test_KP_SegregationCheckResult_AcceptsValidValues_And_RejectsInvalidValues()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			AssertEquals("Segregation check result is empty by default", ZString.Empty, package.KP_DGSegregationCheckResult);

			package.KP_DGSegregationCheckResult = "PASS";
			AssertNoExceptionThrown("PASS is a valid value for KP_DGSegregationCheckResult. It means that the segregation check passed.", () => Factory.Save());

			package.KP_DGSegregationCheckResult = "FAIL";
			AssertNoExceptionThrown("FAIL is a valid value for KP_DGSegregationCheckResult. It means that the segregation check failed.", () => Factory.Save());

			package.KP_DGSegregationCheckResult = "WARN";
			AssertNoExceptionThrown("WARN is a valid value for KP_DGSegregationCheckResult. It means that the segregation check passed with warnings.", () => Factory.Save());

			package.KP_DGSegregationCheckResult = "PNDG";
			AssertNoExceptionThrown("PNDG is a valid value for KP_DGSegregationCheckResult. It means that the segregation check must run before saving the package.", () => Factory.Save());

			package.KP_DGSegregationCheckResult = "ABCD";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		#endregion

		#region Test_KP_PackagingLayerType

		public void Test_KP_PackagingLayerType_AcceptsValidValues_And_RejectsInvalidValues()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();

			AssertEquals("Packaging layer type is empty by default. This is because sparse columns must be nullable and cannot have default values.", ZString.Empty, package.KP_PackagingLayerType);

			package.KP_PackagingLayerType = "OVP";
			AssertNoExceptionThrown("Overpack is a valid type of packaging layer", () => Factory.Save());

			package.KP_PackagingLayerType = "OUT";
			AssertNoExceptionThrown("Outer is a valid type of packaging layer", () => Factory.Save());

			package.KP_PackagingLayerType = "INR";
			AssertNoExceptionThrown("Inner is a valid type of packaging layer", () => Factory.Save());

			package.KP_PackagingLayerType = "ABC";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			return packageJob.Packages.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		#endregion
	}
}
