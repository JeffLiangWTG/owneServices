using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageItemDivotsWrapper))]
	class PkgPackageItemDivotsWrapperTest : PackingNonPersistentBusinessObjectTestCase<PkgPackageItemDivotsWrapper>
	{
		#region Related Entities

		#region TestKey

		public void TestKey()
		{
			Data.CreatePackingData();

			var itemDivot1 = Factory.New<PkgPackageItemDivot>();
			AssertEquals(EmptyKey.Instance, PkgPackageItemDivotsWrapper.New(itemDivot1, Data.DummyLine1).Key);

			var itemDivot2 = Factory.New<PkgPackageItemDivot>();
			itemDivot2.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			itemDivot2.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
			AssertEquals(new DummyGroupingKey(Data.DummyLine1), PkgPackageItemDivotsWrapper.New(itemDivot2, Data.DummyLine1).Key);
		}

		#endregion

		#region TestPackableItemParent

		public void TestPackableItemParent()
		{
			Data.CreatePackingData();

			var itemDivot = Factory.New<PkgPackageItemDivot>();
			var packedItem = PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine1);
			AssertEquals(Data.DummyLine1, packedItem.PackableItemParent);

			Data.DummyLine1.Delete();
			AssertNull(packedItem.PackableItemParent);
		}

		#endregion

		#region TestPackedItem

		public void TestPackedItem()
		{
			Data.CreatePackingData();

			var itemDivot1 = Factory.New<PkgPackageItemDivot>();
			itemDivot1.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			itemDivot1.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;

			var packedItemWrapper = PkgPackageItemDivotsWrapper.New(itemDivot1, Data.DummyLine1);
			AssertEquals(Data.DummyPackableItemOnLine1, packedItemWrapper.PackedItems.Single());

			var itemDivot2 = Factory.New<PkgPackageItemDivot>();
			AssertNull(PkgPackageItemDivotsWrapper.New(itemDivot2, Data.DummyLine1).PackedItems.Single());
		}

		#endregion

		#region TestParentPackage

		public void TestParentPackage()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var itemDivot = package.PackedItemDivots.AddNew();
			var packedItem = PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine1);
			AssertEquals(package, packedItem.ParentPackage);

			itemDivot.KI_KP_Package = ZGuid.Empty;
			AssertNull(packedItem.ParentPackage);
		}

		#endregion

		#region TestPackagePKAndParentPackageOnDeletedDivot

		public void TestPackagePKAndParentPackageOnDeletedDivot()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var itemDivot = package.PackedItemDivots.AddNew();
			itemDivot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			itemDivot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;

			var packedItem = PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine1);
			AssertEquals(Data.DummyPackableItemOnLine1, packedItem.PackedItems.Single());
			AssertEquals(package, packedItem.ParentPackage);
			AssertEquals(package.PK, packedItem.PackagePK);

			itemDivot.Delete();
			AssertNull(packedItem.ParentPackage);
			AssertEquals(ZGuid.Empty, packedItem.PackagePK);
			AssertEquals(true, packedItem.IsDeleted);
		}

		#endregion

		#endregion

		#region TestThrowExceptionIfDeleted

		public void TestThrowExceptionIfDeleted()
		{
			Data.CreatePackingData();

			var itemDivot = Factory.New<PkgPackageItemDivot>();
			var packedItem = PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine1);
			AssertNoExceptionThrown(() => packedItem.ThrowExceptionIfDeleted());

			packedItem.Delete();
			AssertExceptionThrown<InvalidOperationException>(() => packedItem.ThrowExceptionIfDeleted());
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			var wrappedDivot = package.PackedItemDivots.Single();
			AssertEquals("Precondition: Package Weight is updated.", 200m, package.KP_Weight);
			AssertEquals("By Default Packed Item has no Changes.", false, packedItem.HasChanges);

			packedItem.Delete();
			AssertEquals("Package Weight should have been reduced.", 0m, package.KP_Weight);
			AssertEquals("Packed Item should have HasChanges true.", true, packedItem.HasChanges);
			AssertEquals("Wrapped Divot should be deleted.", true, wrappedDivot.IsDeleted);
		}

		public void TestDelete_WhenWrappedDivotDeleting()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			var wrappedDivot = package.PackedItemDivots.Single();
			AssertEquals("Precondition: Package Weight is updated.", 200m, package.KP_Weight);
			AssertEquals("By Default Packed Item has no Changes.", false, packedItem.HasChanges);

			wrappedDivot.Delete();
			AssertEquals("Package Weight should have been reduced.", 0m, package.KP_Weight);
			AssertEquals("Packed Item should have HasChanges true.", true, packedItem.HasChanges);
			AssertEquals("Packed Item should be deleted.", true, packedItem.IsDeleted);
		}

		public void TestDelete_WhenDoneThroughDataRefresh()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packableItem1 = (DummyPackableItem)Data.DummyLine1.PackableItems.Single();
			var packableItem2 = Data.DummyLine1.AddNewPackableItem();
			packableItem1.Quantity = 30m;
			packableItem2.Quantity = 70m;
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			AssertEquals("Precondition: DescriptionWithoutSupplement is set.", "TV", packedItem.DescriptionWithoutSupplement);
			AssertEquals("Precondition: Description is set.", "TV - Size: 63in", packedItem.Description);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			packageInNewFactory.PackedItemDivots.DeleteAll();
			newFactory.Save(); // was blowing with RowIsDeleted exception prior to fix

			AssertEquals("DescriptionWithoutSupplement should be updated.", PackableItemParentWrapper.ItemNoLongerExistsText, packedItem.DescriptionWithoutSupplement);
			AssertEquals("Description should be updated.", PackableItemParentWrapper.ItemNoLongerExistsText, packedItem.Description);
			AssertEquals("Divot wrapper should be deleted.", true, packedItem.IsDeleted);
		}

		public void TestDelete_WhenDoneThroughDataRefresh_UpdatesPackageCollection()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			AssertEquals("Packed Items is Empty.", 0, package.PackedItems.Count);

			var packableItem1 = (DummyPackableItem)Data.DummyLine1.PackableItems.Single();
			var packableItem2 = Data.DummyLine1.AddNewPackableItem();
			packableItem1.Quantity = 30m;
			packableItem2.Quantity = 70m;
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			AssertEquals("Precondition: DescriptionWithoutSupplement is set.", "TV", packedItem.DescriptionWithoutSupplement);
			AssertEquals("Precondition: Description is set.", "TV - Size: 63in", packedItem.Description);
			AssertEquals("Precondition: Packed Items collection should have 1 element.", 1, package.PackedItems.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			packageInNewFactory.PackedItemDivots.DeleteAll();
			newFactory.Save();

			AssertEquals("DescriptionWithoutSupplement should be updated.", PackableItemParentWrapper.ItemNoLongerExistsText, packedItem.DescriptionWithoutSupplement);
			AssertEquals("Description should be updated.", PackableItemParentWrapper.ItemNoLongerExistsText, packedItem.Description);
			AssertEquals("Divot wrapper should be deleted.", true, packedItem.IsDeleted);
			AssertEquals("Packed Items collection should have no elements.", 0, package.PackedItems.Count);
		}

		#endregion

		#region TestDeleteMultiDivots

		public void TestDeleteMultiDivots_DeletedByWrapper()
		{
			Data.CreatePackingData();

			var packableItemOnLine1 = Data.DummyLine1.PackableItems.Single();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			var divot1 = CreateDivot(packableItemOnLine1, package.PK);
			var divot2 = CreateDivot(packableItemOnLine1, package.PK);

			var divotWrapper = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			divotWrapper.SetDivotPackedQty(divot1, 50m);
			divotWrapper.AddDivotToWrapper(divot2);
			divotWrapper.SetDivotPackedQty(divot2, 100m);
			AssertEquals("Package Weight should be 150 X 2", 300m, package.KP_Weight);

			divotWrapper.Delete();
			AssertEquals("Package Weight is reduced.", 0m, package.KP_Weight);
			AssertEquals("Wrapper should have HasChanges true.", true, divotWrapper.HasChanges);
			AssertEquals("divot1 should be deleted.", true, divot1.IsDeleted);
			AssertEquals("divot2 should be deleted.", true, divot2.IsDeleted);
		}

		public void TestDeleteMultiDivots_DeletedByDivot()
		{
			Data.CreatePackingData();

			var packableItemOnLine1 = Data.DummyLine1.PackableItems.Single();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			var divot1 = CreateDivot(packableItemOnLine1, package.PK);
			var divot2 = CreateDivot(packableItemOnLine1, package.PK);

			var divotWrapper = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			divotWrapper.SetDivotPackedQty(divot1, 50m);
			divotWrapper.AddDivotToWrapper(divot2);
			divotWrapper.SetDivotPackedQty(divot2, 100m);
			AssertEquals("Package Weight should be 150 X 2", 300m, package.KP_Weight);

			divot1.Delete();
			AssertEquals("Package Weight is reduced.", 200m, package.KP_Weight);
			AssertEquals("Wrapper should have HasChanges false.", false, divotWrapper.HasChanges);
			AssertEquals("divot1 should be deleted.", true, divot1.IsDeleted);
			AssertEquals("divot2 should not be deleted.", false, divot2.IsDeleted);
			AssertEquals("Wrapper should not be deleted.", false, divotWrapper.IsDeleted);

			divot2.Delete();
			AssertEquals("Package Weight is reduced.", 0m, package.KP_Weight);
			AssertEquals("Wrapper should have HasChanges true.", true, divotWrapper.HasChanges);
			AssertEquals("divot2 should be deleted.", true, divot2.IsDeleted);
			AssertEquals("Wrapper should be deleted.", true, divotWrapper.IsDeleted);
		}

		#endregion

		#region TestMovePackedItemMultiDivots

		public void TestMovePackedItemMultiDivots()
		{
			Data.CreatePackingData();

			var packableItem1 = Data.DummyLine1.PackableItems.Single();
			var packableItem2 = Data.DummyLine2.PackableItems.Single();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			var divot1 = CreateDivot(packableItem1, package.PK);
			var divot2 = CreateDivot(packableItem1, package.PK);
			var divot3 = CreateDivot(packableItem2, package.PK);

			var packedItem1 = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			packedItem1.SetDivotPackedQty(divot1, 40m);
			packedItem1.AddDivotToWrapper(divot2);
			packedItem1.SetDivotPackedQty(divot2, 60m);

			AssertEquals("Package Weight should be 100 X 2", 200m, package.KP_Weight);

			var packedItem2 = PkgPackageItemDivotsWrapper.New(divot3, Data.DummyLine2);
			packedItem2.SetDivotPackedQty(divot3, 10m);

			AssertEquals("Package Weight should be 110 X 2", 220m, package.KP_Weight);
			AssertEquals("There should be 3 Divot on first Package.", 3, package.PackedItemDivots.Count);
			AssertExceptionThrown<ArgumentNullException>(() => packedItem1.MovePackedItem(null));
			AssertExceptionThrown(typeof(InvalidOperationException), "You can only move a Divot to another Package on the same PackageJob.", () => packedItem1.MovePackedItem(Factory.New<PkgPackage>()));

			var otherPackage = Data.PackageJob.Packages.AddNew("CTN");
			var divot4 = CreateDivot(packableItem1, otherPackage.PK);
			var packedItem3 = PkgPackageItemDivotsWrapper.New(divot4, Data.DummyLine1);
			packedItem3.SetDivotPackedQty(divot4, 10m);

			packedItem1.MovePackedItem(otherPackage);
			AssertEquals("Package Weight is reduced", 20m, package.KP_Weight);
			AssertEquals("There should be 1 Divot on first Package.", 1, package.PackedItemDivots.Count);

			AssertEquals("Other Package Weight is increased.", 220m, otherPackage.KP_Weight);
			AssertEquals("Divots are on new Package.", 3, otherPackage.PackedItemDivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2, divot4 }, otherPackage.PackedItemDivots);
		}

		#endregion

		#region TestSetDivotPackedQty

		public void TestSetDivotPackedQty()
		{
			Data.CreatePackingData();

			var packableItemOnLine1 = Data.DummyLine1.PackableItems.Single();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			var divot1 = CreateDivot(packableItemOnLine1, package.PK);

			var divotWrapper = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			divotWrapper.SetDivotPackedQty(divot1, 5m);
			AssertEquals("Divot KI_PackedQty should be 5", 5m, divot1.KI_PackedQty);
			AssertEquals("DivotWrapper PackedQty should be 5", 5m, divotWrapper.PackedQty);
			AssertEquals("Package Weight should be 5 X 2", 10m, package.KP_Weight);

			divotWrapper.SetDivotPackedQty(divot1, 10m);
			AssertEquals("Divot KI_PackedQty should be 10", 10m, divot1.KI_PackedQty);
			AssertEquals("DivotWrapper PackedQty should be 10", 10m, divotWrapper.PackedQty);
			AssertEquals("Package Weight should be 10 X 2", 20m, package.KP_Weight);

			var divot2 = CreateDivot(packableItemOnLine1, package.PK);
			divotWrapper.AddDivotToWrapper(divot2);
			divotWrapper.SetDivotPackedQty(divot2, 15m);
			AssertEquals("Divot KI_PackedQty should be 15", 15m, divot2.KI_PackedQty);
			AssertEquals("DivotWrapper PackedQty should be 25", 25m, divotWrapper.PackedQty);
			AssertEquals("Package Weight should be 25 X 2", 50m, package.KP_Weight);
		}

		#endregion

		#region TestReducePackedQty

		public void TestReducePackedQty_ArgumentException()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 10m);
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentException), "Quantity to Unpack should be greater than zero.\r\nParameter name: qtyToUnpack", () => packedItem1.ReducePackedQty(-1));
			AssertExceptionThrown(typeof(ArgumentException), "Quantity to Unpack should be greater than zero.\r\nParameter name: qtyToUnpack", () => packedItem1.ReducePackedQty(0));
#else
			AssertExceptionThrown(typeof(ArgumentException), "Quantity to Unpack should be greater than zero. (Parameter 'qtyToUnpack')", () => packedItem1.ReducePackedQty(-1));
			AssertExceptionThrown(typeof(ArgumentException), "Quantity to Unpack should be greater than zero. (Parameter 'qtyToUnpack')", () => packedItem1.ReducePackedQty(0));
#endif
		}

		public void TestReducePackedQtyWhenPackableItemIsInvalid_ExpectNoExceptions()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew("PL1");
			var packedItem = pallet.Pack_ForTesting(Data.DummyLine1, 10m);
			var divots = Factory.Load<PkgPackageItemDivot>(new ZQuery(PkgPackageItemDivotSchema.KI_KP_Package, pallet.PK));
			divots.ForEach(d => ((BusinessObject)d.PackedItem).Delete());
			AssertNoExceptionThrown(() => packedItem.ReducePackedQty(1m));
		}

		public void TestReducePackedQty()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			var packedItem1 = package.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = package.Pack_ForTesting(Data.DummyLine1, 5m);
			AssertEquals("Should return same wrapper.", packedItem1, packedItem2);
			AssertEquals("PackedQty should be 15", 15m, packedItem1.PackedQty);
			AssertEquals("Package Weight should be 15 X 2", 30m, package.KP_Weight);

			AssertDivotsCountAndWeight(packedItem1, 3m, 12m, 88m, 2m);
			AssertDivotsCountAndWeight(packedItem1, 2m, 10m, 90m, 0m);
		}

		void AssertDivotsCountAndWeight(PkgPackageItemDivotsWrapper divotWrapper, ZDecimal reduceQuantity, ZDecimal packedQty, ZDecimal remergedPackedQty, ZDecimal splitQuantity)
		{
			var packageWeight = packedQty * 2;
			divotWrapper.ReducePackedQty(reduceQuantity);
			AssertEquals("PackedQty should be " + packedQty, packedQty, divotWrapper.PackedQty);
			AssertEquals("Package Weight should be " + packageWeight, packageWeight, divotWrapper.ParentPackage.KP_Weight);

			var packedItemAfterRemerged = divotWrapper.PackableItemParent.PackableItems.Single(p => p.Quantity == remergedPackedQty);
			AssertNotNull(packedItemAfterRemerged);

			if (splitQuantity > 0m)
			{
				var packedITemAfterSplit = divotWrapper.PackableItemParent.PackableItems.Single(p => p.Quantity == splitQuantity);
				AssertNotNull(packedITemAfterSplit);
			}
		}

		#region TestReducePackedQty_PackableItemDeletedDuringSplit

		public void TestReducePackedQty_PackableItemDeletedDuringSplit()    //	e.g WhsReleaseCaptureAttributes
		{
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItem_DeleteSelfDuringSplit);

			var dummy = Factory.New<DummyWithPacking>();
			var dummyLine1 = dummy.Lines.AddNew();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			var package = packageJob.Packages.AddNew();

			var packedItemDivotWrapper = package.Pack_ForTesting(dummyLine1, 10m);
			var packedItem = (DummyPackableItem)packedItemDivotWrapper.PackedItems.Single();
			AssertEquals(10m, packedItem.Quantity);

			AssertEquals("Pre-condition: PackedQty should be 10", 10m, packedItemDivotWrapper.PackedQty);
			packedItemDivotWrapper.ReducePackedQty(3m);

			AssertEquals(7m, packedItemDivotWrapper.PackedQty);
			AssertNotNull(packedItemDivotWrapper.PackedItems.First());

			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		#endregion

#endregion

		#region TestAddDivotToWrapper

		public void TestAddDivotToWrapper()
		{
			Data.CreatePackingData();
			var packableItemOnLine1 = Data.DummyLine1.PackableItems.Single();
			var packableItemOnLine2 = Data.DummyLine2.PackableItems.Single();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			var divot1 = CreateDivot(packableItemOnLine1, package.PK);
			var divot2 = CreateDivot(packableItemOnLine2, package.PK);
			var divot3 = CreateDivot(packableItemOnLine1, package.PK);

			var divotWrapper = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			divotWrapper.SetDivotPackedQty(divot1, 5m);
			AssertExceptionThrown<ArgumentException>(() => divotWrapper.AddDivotToWrapper(divot2));
			AssertEquals(5m, divotWrapper.PackedQty);

			divotWrapper.AddDivotToWrapper(divot3);
			divotWrapper.SetDivotPackedQty(divot3, 10m);
			AssertEquals(15m, divotWrapper.PackedQty);
		}

		public void TestAddDivotToWrapper_CannotPassInNull()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			AssertExceptionThrown<ArgumentNullException>(() => packedItem.AddDivotToWrapper(null));
		}

		#endregion

		#region CreateDivots

		PkgPackageItemDivot CreateDivot(IPackableItem packableItem, ZGuid packagePK)
		{
			var itemDivot = Factory.New<PkgPackageItemDivot>();
			itemDivot.KI_KP_Package = packagePK;
			itemDivot.KI_ParentID = packableItem.PK;
			itemDivot.KI_ParentTableCode = packableItem.TablePrefix;

			return itemDivot;
		}

		#endregion

		#region Properties

		#region TestCode

		public void TestCode()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var packedItemForLine1 = package.Pack_ForTesting(Data.DummyLine1, 50m);
			var packedItemForLine2 = package.Pack_ForTesting(Data.DummyLine2, 20m);

			AssertEquals("P1", packedItemForLine1.Code);
			AssertEquals("P2", packedItemForLine2.Code);
		}

		#endregion

		#region TestDescriptionWithoutSupplement

		public void TestDescriptionWithoutSupplement()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var packedItemForLine1 = package.Pack_ForTesting(Data.DummyLine1, 50m);
			var packedItemForLine2 = package.Pack_ForTesting(Data.DummyLine2, 20m);

			AssertEquals("TV", packedItemForLine1.DescriptionWithoutSupplement);
			AssertEquals("Amp", packedItemForLine2.DescriptionWithoutSupplement);
			AssertEquals(PackableItemParentWrapper.ItemNoLongerExistsText, PkgPackageItemDivotsWrapper.New(Factory.New<PkgPackageItemDivot>(), Data.DummyLine1).DescriptionWithoutSupplement);

			int descriptionChangedHitCount = 0;
			packedItemForLine1.DescriptionChanged += (sender, e) => descriptionChangedHitCount++;
			((BusinessObject)packedItemForLine1.PackedItems.Single()).Delete();
			AssertEquals("On Packed Item Delete should update Description.", PackableItemParentWrapper.ItemNoLongerExistsText, packedItemForLine1.DescriptionWithoutSupplement);
			AssertEquals("On Packed Item Delete should update Description.", 1, descriptionChangedHitCount);

			var packedItem = packedItemForLine2.PackedItems.Single();
			packedItemForLine2.DescriptionChanged += (sender, e) => descriptionChangedHitCount++;
			packedItemForLine2.Delete();
			AssertEquals("Delete should unhook Description update.", "Amp", packedItemForLine2.DescriptionWithoutSupplement);
			AssertEquals("Delete should unhook Description update.", 1, descriptionChangedHitCount);

			((BusinessObject)packedItem).Delete();
			AssertEquals("Delete should unhook Description update.", "Amp", packedItemForLine2.DescriptionWithoutSupplement);
			AssertEquals("Delete should unhook Description update.", 1, descriptionChangedHitCount);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var packedItemForLine1 = package.Pack_ForTesting(Data.DummyLine1, 50m);
			var packedItemForLine2 = package.Pack_ForTesting(Data.DummyLine2, 20m);

			AssertEquals("TV", packedItemForLine1.DescriptionWithoutSupplement);
			AssertEquals("TV - Size: 63in", packedItemForLine1.Description);
			AssertEquals("Amp", packedItemForLine2.DescriptionWithoutSupplement);
			AssertEquals("Amp", packedItemForLine2.Description);
			AssertEquals(PackableItemParentWrapper.ItemNoLongerExistsText, PkgPackageItemDivotsWrapper.New(Factory.New<PkgPackageItemDivot>(), Data.DummyLine1).Description);

			int descriptionChangedHitCount = 0;
			packedItemForLine1.DescriptionChanged += (sender, e) => descriptionChangedHitCount++;
			((BusinessObject)packedItemForLine1.PackedItems.Single()).Delete();
			AssertEquals("On Packed Item Delete should update Description.", PackableItemParentWrapper.ItemNoLongerExistsText, packedItemForLine1.Description);
			AssertEquals("On Packed Item Delete should update Description.", 1, descriptionChangedHitCount);

			var packedItem = packedItemForLine2.PackedItems.Single();
			packedItemForLine2.DescriptionChanged += (sender, e) => descriptionChangedHitCount++;
			packedItemForLine2.Delete();
			AssertEquals("Delete should unhook Description update.", "Amp", packedItemForLine2.Description);
			AssertEquals("Delete should unhook Description update.", 1, descriptionChangedHitCount);

			((BusinessObject)packedItem).Delete();
			AssertEquals("Delete should unhook Description update.", "Amp", packedItemForLine2.Description);
			AssertEquals("Delete should unhook Description update.", 1, descriptionChangedHitCount);
		}

		#endregion

		#region TestCodeWithDescription

		public void TestCodeWithDescription()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var packedItemForLine1 = package.Pack_ForTesting(Data.DummyLine1, 50m);
			var packedItemForLine2 = package.Pack_ForTesting(Data.DummyLine2, 20m);

			AssertEquals("P1 - TV - Size: 63in", packedItemForLine1.CodeWithDescription);
			AssertEquals("P2 - Amp", packedItemForLine2.CodeWithDescription);
		}

		public void TestCodeWithDescription_CodeIsEmpty()
		{
			Data.CreatePackingData();
			var dummyLine1 = Data.DummyLine1;
			var dummyLine2 = Data.DummyLine2;
			dummyLine1.Code = "";
			dummyLine2.Code = "";

			var package = Factory.New<PkgPackage>();
			var packedItemForLine1 = package.Pack_ForTesting(Data.DummyLine1, 50m);
			var packedItemForLine2 = package.Pack_ForTesting(Data.DummyLine2, 20m);

			AssertEquals("TV - Size: 63in", packedItemForLine1.CodeWithDescription);
			AssertEquals("Amp", packedItemForLine2.CodeWithDescription);
		}

		#endregion

		#region TestPackagePK

		public void TestPackagePK()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var itemDivot = package.PackedItemDivots.AddNew();
			var packedItem = PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine1);
			AssertEquals(package.PK, packedItem.PackagePK);

			itemDivot.KI_KP_Package = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, packedItem.PackagePK);
		}

		#endregion

		#region TestChangingItemOrQtyAddsToPackageWeight

		public void TestChangingItemOrQtyAddsToPackageWeight()
		{
			Data.CreatePackingData();

			const decimal PalletWeight = 20m;
			const decimal ContainerWeight = 100m;
			const decimal BoxWeight = 10m;

			var container = Data.PackageJob.Packages.AddNew();
			var pallet = container.Packages.AddNew();
			var box = pallet.Packages.AddNew();

			box.KP_Weight = BoxWeight;
			pallet.KP_Weight = PalletWeight;
			container.KP_Weight = ContainerWeight;

			var divot1 = Factory.New<PkgPackageItemDivot>();
			divot1.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot1.KI_ParentTableCode = DummyDependentBizoSchema.Constants.Prefix;
			divot1.KI_KP_Package = Data.PackageJob.Packages.AddNew().PK;

			var packedItem1 = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			packedItem1.SetDivotPackedQty(divot1, 10m); // 10units x 2kg ea = 20kg

			// test adding items
			packedItem1.MovePackedItem(box);
			AssertEquals("Adding an item should add the item's weight to the package's weight.", BoxWeight + 20m, box.KP_Weight);
			AssertEquals("Increasing a package's weight should increase the parent package's weight.", PalletWeight + 20m, pallet.KP_Weight);
			AssertEquals("Increasing a package's weight should increase the parent package's weight.", ContainerWeight + 20m, container.KP_Weight);

			// test removing items

			// hack the box down to 15kg. removing 20kg of items should not reduce the weight below 0. This will leave an extra 5kg in every parent package.

			((INeedRow)box).Row["KP_Weight"] = 15m;
			packedItem1.Delete();
			AssertEquals("Removing an item should remove the item's weight from the package's weight, but never go below 0.", 0m, box.KP_Weight);
			AssertEquals("Reducing a package's weight should reduce the parent package's weight.", PalletWeight + 5m, pallet.KP_Weight);
			AssertEquals("Reducing a package's weight should reduce the parent package's weight.", ContainerWeight + 5m, container.KP_Weight);

			// test that assigning KI_KP_Package directly updates the package weights
			var divot2 = Factory.New<PkgPackageItemDivot>();
			divot2.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot2.KI_ParentTableCode = DummyDependentBizoSchema.Constants.Prefix;
			divot2.KI_KP_Package = Data.PackageJob.Packages.AddNew().PK;

			var packedItem2 = PkgPackageItemDivotsWrapper.New(divot2, Data.DummyLine1);
			packedItem2.SetDivotPackedQty(divot2, 10m); // 10units x 2kg ea = 20kg
			packedItem2.MovePackedItem(pallet);
			AssertEquals("Box weight should not change because no item was added below it.", 0m, box.KP_Weight);
			AssertEquals("Adding an item to a package should add the item's weight to all parent package's weight.", PalletWeight + 5m + 20m, pallet.KP_Weight);
			AssertEquals("Adding an item to a package should add the item's weight to all parent package's weight.", ContainerWeight + 5m + 20m, container.KP_Weight);

			// test that modifying PackedQty updates the package weights

			packedItem2.SetDivotPackedQty(divot2, packedItem2.PackedQty + 5); // 5units x 2kg ea = 10kg
			AssertEquals("Box weight should not change because no item was added below it.", 0m, box.KP_Weight);
			AssertEquals("Adding an item to a package should add the item's weight to all parent package's weight.", PalletWeight + 5m + 20m + 10m, pallet.KP_Weight);
			AssertEquals("Adding an item to a package should add the item's weight to all parent package's weight.", ContainerWeight + 5m + 20m + 10m, container.KP_Weight);

			// test that deleting the item updates the package weights
			packedItem2.Delete();
			AssertEquals("Removing an item from a package should remove the item's weight from the package's weight.", 0m, box.KP_Weight);
			AssertEquals("Removing an item from a package should remove the item's weight from the package's weight.", PalletWeight + 5m, pallet.KP_Weight);
			AssertEquals("Removing an item from a package should remove the item's weight from the package's weight.", ContainerWeight + 5m, container.KP_Weight);
		}

		#endregion

		#region TestAddWeightToParentPackage_InvalidWeightUQ

		public void TestAddWeightToParentPackage_InvalidWeightUQ()
		{
			Data.CreatePackingData();
			var parentPackage = Data.PackageJob.Packages.AddNew();
			parentPackage.KP_WeightUQ = "XX"; // Setup invalid UQ
			AssertHasErrors("Precondition", parentPackage.KP_WeightUQInfo);

			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot.KI_ParentTableCode = DummyDependentBizoSchema.Constants.Prefix;
			divot.KI_KP_Package = parentPackage.PK;

			var packedItem = PkgPackageItemDivotsWrapper.New(divot, Data.DummyLine1);
			AssertNoExceptionThrown(() => packedItem.SetDivotPackedQty(divot, 10m));
			AssertNoExceptionThrown(() => packedItem.SetDivotPackedQty(divot, 5m));

			parentPackage.KP_WeightUQ = Constants.Weight.Kilograms;
			Data.DummyLine1.WeightUQ = "XX"; // Setup invalid UQ
			AssertNoExceptionThrown(() => packedItem.SetDivotPackedQty(divot, 10m));
		}

		#endregion

		#region TestPackedQty_WhenPackableItemParentDeleted

		public void TestPackedQty_WhenPackableItemParentDeleted()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			AssertEquals("Precondition: Package Weight is updated.", 200m, package.KP_Weight);

			var divot = package.PackedItemDivots.Single(d => d.PackedItem.Key == packedItem.Key);
			packedItem.SetDivotPackedQty(divot, 90m);
			AssertEquals("Package Weight is reduced when PackedQty is reduced.", 180m, package.KP_Weight);

			Data.DummyLine1.Delete();
			packedItem.SetDivotPackedQty(divot, 80m);
			AssertEquals("Package Weight is not reduced when PackableItem is deleted.", 180m, package.KP_Weight);
		}

		#endregion

		#region TestQuantityChanged

		public void TestQuantityChanged()
		{
			Data.CreatePackingData();

			var itemDivot = Factory.New<PkgPackageItemDivot>();
			var packedItem = PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine1);

			int qtyChangedHitCount = 0;
			packedItem.QuantityChanged += (sender, e) => qtyChangedHitCount++;

			itemDivot.KI_PackedQty = 1m;
			AssertEquals("QuantityChanged should have fired when Wrapper's PackedQty changed.", 1, qtyChangedHitCount);
		}

		#endregion

		#region TestAfterMoving

		public void TestAfterMoving()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = package.Pack(Data.DummyLine1, 10m).Single();

			var otherPackage = Data.PackageJob.Packages.AddNew("CTN");
			var packedItem2 = otherPackage.Pack(Data.DummyLine1, 20m).Single();

			int afterMovingHitCount = 0;
			packedItem2.QuantityChanged += (sender, e) => afterMovingHitCount++;

			packedItem1.MovePackedItem(otherPackage);
			AssertEquals("AfterMoving should have fired when Wrapped Divot had moved to another package.", 1, afterMovingHitCount);
		}

		#endregion

		#region TestUQ

		public void TestUQ()
		{
			Data.CreatePackingData();
			Data.DummyLine1.TotalQtyUQ = "UNT";
			Data.DummyLine2.TotalQtyUQ = "BOX";

			var package = Factory.New<PkgPackage>();
			var packedItemForLine1 = package.Pack_ForTesting(Data.DummyLine1, 50m);
			var packedItemForLine2 = package.Pack_ForTesting(Data.DummyLine2, 20m);

			AssertEquals("UNT", packedItemForLine1.UQ);
			AssertEquals("BOX", packedItemForLine2.UQ);

			Data.DummyLine1.Delete();
			AssertEquals(true, PkgPackageItemDivotsWrapper.New(Factory.New<PkgPackageItemDivot>(), Data.DummyLine1).UQ.IsEmpty);
		}

		#endregion

		#region TestICustomPropertyContainerMembers

		public void TestICustomPropertyContainerMembers()
		{
			Data.CreatePackingData();
			Data.DummyLine1.ZD1_Code = "abc";

			var divotWrapper = PkgPackageItemDivotsWrapper.New(Factory.New<PkgPackageItemDivot>(), Data.DummyLine1);
			var textCustomProperty = ((ICustomPropertyContainer)divotWrapper).CustomProperties.First(property => property.Identifier == "ZD1 Code");
			AssertEquals("Should have proxied GetValue delegate from Wrapper to PackableItem.", "abc", textCustomProperty.GetValue(divotWrapper));

			var badDivot = Factory.New<PkgPackageItemDivot>();
			var badWrapper = PkgPackageItemDivotsWrapper.New(badDivot, Data.DummyLine1);
			AssertNoExceptionThrown(() => { var poke = ((ICustomPropertyContainer)badWrapper).CustomProperties; });
		}

		#endregion

		#region TestPackedItems

		public void TestPackedItems()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packableItemOnLine1 = Data.DummyLine1.PackableItems.Single();
			var divot1 = CreateDivot(packableItemOnLine1, package.PK);
			var divot2 = CreateDivot(packableItemOnLine1, package.PK);

			var divotWrapper = PkgPackageItemDivotsWrapper.New(divot1, Data.DummyLine1);
			divotWrapper.SetDivotPackedQty(divot1, 5m);
			AssertEquals("Threse should be 1 PackedItems", 1, divotWrapper.PackedItems.Count());

			divotWrapper.AddDivotToWrapper(divot2);
			AssertEquals("Threse should be 2 PackedItems", 2, divotWrapper.PackedItems.Count());
		}

		#endregion

		#endregion

		#region TestMovePackedItem

		public void TestMovePackedItem_MoveToDestination()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			var wrappedDivot = package.PackedItemDivots.Single();
			AssertEquals("Precondition: Package Weight is updated.", 200m, package.KP_Weight);
			AssertExceptionThrown<ArgumentNullException>(() => packedItem.MovePackedItem(null));
			AssertExceptionThrown(typeof(InvalidOperationException), "You can only move a Divot to another Package on the same PackageJob.", () => packedItem.MovePackedItem(Factory.New<PkgPackage>()));

			var otherPackage = Data.PackageJob.Packages.AddNew("CTN");
			packedItem.MovePackedItem(otherPackage);
			AssertEquals("Package Weight is reduced.", 0m, package.KP_Weight);
			AssertEquals("Item Divot is no longer on first Package.", 0, package.PackedItemDivots.Count);
			AssertEquals("Other Package Weight is increased.", 200m, otherPackage.KP_Weight);
			AssertContainsExactElementsInAnyOrder(new[] { wrappedDivot }, otherPackage.PackedItemDivots);
		}

		public void TestMovePackedItem_MergeToDestination()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = package.Pack(Data.DummyLine1, 10m).Single();
			AssertEquals("Package Weight is updated.", 20m, package.KP_Weight);
			AssertEquals("Package divots list is updated.", 1, package.PackedItemDivots.Count);

			var otherPackage = Data.PackageJob.Packages.AddNew("CTN");
			var packedItem2 = otherPackage.Pack(Data.DummyLine1, 20m).Single();
			AssertEquals("otherPackage Weight is updated.", 40m, otherPackage.KP_Weight);
			AssertEquals("otherPackage divots list is updated.", 1, package.PackedItemDivots.Count);

			packedItem1.MovePackedItem(otherPackage);
			AssertEquals("Package Weight is updated.", 0m, package.KP_Weight);
			AssertEquals("Package divots list is updated.", 0, package.PackedItemDivots.Count);
			AssertEquals("otherPackage Weight is updated.", 60m, otherPackage.KP_Weight);
			AssertEquals("otherPackage divots list is updated.", 2, otherPackage.PackedItemDivots.Count);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var package = Factory.New<PkgPackage>();
			var divot = package.PackedItemDivots.AddNew();
			var packableItemParent = Factory.New<DummyPackableItemParent>();
			return PkgPackageItemDivotsWrapper.New(divot, packableItemParent);
		}

		#endregion
	}
}
