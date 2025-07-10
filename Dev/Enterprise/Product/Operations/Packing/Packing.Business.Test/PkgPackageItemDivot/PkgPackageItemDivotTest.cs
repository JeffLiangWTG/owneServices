using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageItemDivot))]
	public class PkgPackageItemDivotTest : PackingBusinessObjectTestCase
	{
		#region Related Entities

		#region TestPackedItem

		public void TestPackedItem()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var packedItem = package.Pack_ForTesting(Data.DummyLine1, 100m);
			AssertEquals(Data.DummyLine1, packedItem.PackableItemParent);

			var itemDivot = package.PackedItemDivots[0];
			AssertEquals(Data.DummyPackableItemOnLine1, itemDivot.PackedItem);
			AssertNull(Factory.New<PkgPackageItemDivot>().PackedItem);
		}

		#endregion

		#region TestParentPackage

		public void TestParentPackage()
		{
			Data.CreatePackingData();

			var package = Factory.New<PkgPackage>();
			var packedItem = package.Pack_ForTesting(Data.DummyLine1, 50m);
			AssertEquals(package, packedItem.ParentPackage);

			var itemDivot = package.PackedItemDivots[0];
			AssertEquals(package, itemDivot.ParentPackage);
			AssertNull(Factory.New<PkgPackageItemDivot>().ParentPackage);
		}

		#endregion

		public void TestPkgNetWeight()
		{
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.PkgNetWeight = 1m;

			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, packageItemDivot.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, "PkgNW");
			query.AddToFilter(GenAddOnColumnSchema.XA_Type, "DEC");
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageItemDivotSchema.Constants.Prefix);
			var genAddOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			AssertEquals("1", genAddOnColumn.XA_Data);
			AssertEquals(1m, packageItemDivot.PkgNetWeight);
		}

		public void TestPkgNetWeightUQ()
		{
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.PkgNetWeightUQ = "KG";

			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, packageItemDivot.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, "PkgNWUQ");
			query.AddToFilter(GenAddOnColumnSchema.XA_Type, "STR");
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, PkgPackageItemDivotSchema.Constants.Prefix);
			var genAddOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			AssertEquals("KG", genAddOnColumn.XA_Data);
			AssertEquals("KG", packageItemDivot.PkgNetWeightUQ);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			AssertNoExceptionThrown(() => Factory.New<PkgPackageItemDivot>().Delete());

			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			var packedItem = package.Pack_ForTesting(Data.DummyLine1, 80m);
			var itemDivot = package.PackedItemDivots[0];
			var packingItem1 = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 80m);
			var packingItem2 = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 20m);

			int beforeDeleteHitCount = 0;
			itemDivot.BeforeDelete += (sender, e) =>
			{
				AssertEquals("Should not have merged Packing Item yet when BeforeDelete is Fired.", 80m, packingItem1.Quantity);
				beforeDeleteHitCount++;
			};

			int validationHitCount = 0;
			itemDivot.KI_KP_PackageInfo.AdditionalValidation += () => validationHitCount++;
			itemDivot.Delete();
			AssertEquals("Before Delete event should have fired, when deleting Divot.", 1, beforeDeleteHitCount);
			AssertEquals("Validation should not be run on Delete.", 0, validationHitCount);
			AssertEquals("Should have merged Packing Item.", true, ((DummyPackableItem)packingItem1).IsDeleted);
			AssertEquals("Should have merged Packing Item.", 100m, packingItem2.Quantity);
		}

		#endregion

		#region TestUniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandler_FK_UX__KI_KP_Package_KI_ParentID()
		{
			var packingParent = Helper.CreatePackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);

			var packableItemParent = Helper.CreatePackableItemParent();
			var packableItem = Helper.CreatePackableItem(packableItemParent, 5m);
			var divot1 = Helper.CreatePackageDivot(package, packableItem);
			var divot2 = Helper.CreatePackageDivot(package, packableItem);

			/* Set the Parent Table Code to CUI so that the only index that can fail is FK_UX__KI_KP_Package_KI_ParentID,
			as there is a filtered unique index on KI_ParentID which is not relevant for this test */
			divot1.KI_ParentTableCode = "CUI";
			divot2.KI_ParentTableCode = "CUI";

			try
			{
				Factory.Save();
				Fail("Expected a unique index violation exception");
			}
			catch (ZSaveException ex)
			{
				var uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;
				AssertEquals("1 bizo should be involved with the unique constraint violation (one should save fine)", 1, ex.BusinessObjects.Length);
				IBusinessObjectInternals divotInError = (PkgPackageItemDivot)ex.BusinessObjects[0];

				AssertEquals("Correct unique constraint should be violated", PkgPackageItemDivotSchema.Constants.Indexes.FK_UX__KI_KP_Package_KI_ParentID, uniqueIndexName);
				var notifier = new MockNotificationHandler();
				divotInError.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notifier, uniqueIndexName);
				AssertEquals("User should be notified of the situation", "Error", notifier.LastErrorCaption);
				AssertEquals("User should be notified of the situation", "Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.", notifier.LastErrorMessage);
			}
		}

		public void TestUniqueIndexFailureHandler_NR_UX__KI_ParentID()
		{
			var packingParent = Helper.CreatePackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package1 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);
			var package2 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);

			var packableItemParent = Helper.CreatePackableItemParent();
			var packableItem = Helper.CreatePackableItem(packableItemParent, 5m);
			var divot1 = Helper.CreatePackageDivot(package1, packableItem);
			var divot2 = Helper.CreatePackageDivot(package2, packableItem);

			try
			{
				Factory.Save();
				Fail("Expected a unique index violation exception");
			}
			catch (ZSaveException ex)
			{
				var uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;
				AssertEquals("1 bizo should be involved with the unique constraint violation (one should save fine)", 1, ex.BusinessObjects.Length);
				IBusinessObjectInternals divotInError = (PkgPackageItemDivot)ex.BusinessObjects[0];

				AssertEquals("Correct unique constraint should be violated", PkgPackageItemDivotSchema.Constants.Indexes.NR_UX__KI_ParentID, uniqueIndexName);
				var notifier = new MockNotificationHandler();
				divotInError.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notifier, uniqueIndexName);
				AssertEquals("User should be notified of the situation", "Error", notifier.LastErrorCaption);
				AssertEquals("User should be notified of the situation", "Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.", notifier.LastErrorMessage);
			}
		}

		class MockNotificationHandler : INotificationHandler
		{
			public string LastErrorMessage;
			public string LastErrorCaption;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				LastErrorMessage = message;
				LastErrorCaption = caption;
			}

			public void ReportInformation(string message, string caption)
			{
				throw new NotSupportedException();
			}
		}

		#endregion

		#region TestUpdateIPackableItem

		public void TestUpdateIPackableItem()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(Data.DummyLine1, 100m).Single();
			var itemDivot = package.PackedItemDivots[0];

			var otherPackableItemOnLine2 = Data.DummyLine2.PackableItems.Single();
			AssertNoExceptionThrown(
				() =>
				{
					itemDivot.UpdateIPackableItem(otherPackableItemOnLine2);
				});

			AssertEquals(otherPackableItemOnLine2.PK, packedItem.PackedItems.Single().PK);
		}

		#endregion

		#region TestIPackingHasChanges

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnNewItemPacked()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;

			var item = Data.DummyPackableItemOnLine1;
			package.Pack(item, Data.DummyLine1);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnItemUnPacked()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			package.Pack(Data.DummyLine1, 100m);
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;

			package.Unpack(package.PackedItems[0], 100m);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnPackItemQuantityModified()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			package.Pack(Data.DummyLine1, 100m);
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;

			package.PackedItemDivots.First().KI_PackedQty = 12;

			AssertNoExceptionThrown(() => Factory.Save());

			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		#endregion

		#region TestParentTableCodeConstraint

		public void TestParentTableCodeConstraint_Allow_WZ() => AssertTestParentTableCodeConstraint("WZ", true);
		public void TestParentTableCodeConstraint_Allow_CUI() => AssertTestParentTableCodeConstraint("CUI", true);
		public void TestParentTableCodeConstraint_OtherNotAllowed_VV() => AssertTestParentTableCodeConstraint("VV", false);
		public void TestParentTableCodeConstraint_OtherNotAllowed_ZZZ() => AssertTestParentTableCodeConstraint("ZZZ", false);

		public void AssertTestParentTableCodeConstraint(string parentTableCode, bool expectedAllowed)
		{
			var package = Factory.NewWithValidTestData<PkgPackage>();

			var pivot = Factory.New<PkgPackageItemDivot>();
			pivot.KI_ParentTableCode = parentTableCode;
			pivot.KI_ParentID = ZGuid.NewZGuid();
			pivot.KI_KP_Package = package.PK;
			pivot.KI_PackedQty = 1;

			if (expectedAllowed)
			{
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				var expectedErrorMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_KI_ParentTableCode_NoCheck\".";
				AssertInnermostException("SqlException should throw", typeof(SqlException), expectedErrorMsg, Factory.Save, true);
			}
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[PkgPackageItemDivot.Schema.KI_ParentID] = ZGuid.Invalid;

				return result;
			}
		}

		#endregion
	}
}
