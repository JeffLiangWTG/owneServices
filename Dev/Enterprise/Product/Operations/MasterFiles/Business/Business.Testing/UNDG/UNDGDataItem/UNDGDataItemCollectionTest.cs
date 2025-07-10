using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGDataItemCollection))]
	sealed class UNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGDataItemCollection>
	{
		public void TestRelationShip()
		{
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			var collection = new UNDGDataItemCollection(dummy);
			var item = Factory.New<UNDGDataItem>();
			collection.Add(item);

			AssertEquals(DummyBaseBusinessObject.Schema.TablePrefix, item.DI_ParentTableCode);
		}

		public void TestBindingItem()
		{
			DummyBizoWithUNDGs dummy = Factory.New<DummyBizoWithUNDGs>();
			AssertEquals(0, dummy.UNDGs.Count);

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "0014A";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			dummy.UNDGs.FirstItemForBinding[0].DI_DG = subs.PK;
			dummy.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
			AssertEquals(1, dummy.UNDGs.Count);
			AssertEquals(false, dummy.UNDGs[0].IsAutoAddedItem);

			AssertEquals(1, dummy.UNDGs.FirstItemForBinding.Count);
			AssertEquals(dummy.UNDGs[0], dummy.UNDGs.FirstItemForBinding[0]);

			dummy.UNDGs.DeleteAll();
			AssertEquals(0, dummy.UNDGs.Count);
			AssertEquals(1, dummy.UNDGs.FirstItemForBinding.Count);
			AssertEquals(true, dummy.UNDGs.FirstItemForBinding[0].IsAutoAddedItem);
			AssertEquals(false, dummy.UNDGs.FirstItemForBinding[0].IsSavedByFactory);

			UNDGDataItem item1 = dummy.UNDGs.AddNew();
			UNDGDataItem item2 = dummy.UNDGs.AddNew();

			AssertEquals(2, dummy.UNDGs.Count);
			AssertEquals(1, dummy.UNDGs.FirstItemForBinding.Count);
			AssertEquals(false, dummy.UNDGs.FirstItemForBinding[0].IsAutoAddedItem);

			AssertEquals(false, item2.IsDeleted);
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = "XXXX";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			item2.LinkDefault(subs2);
			AssertEquals(false, item2.IsDeleted);
		}

		public void TestBindingItems_ShouldNotAddNewItemTwice()
		{
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			dummy.UNDGs.DeleteAll();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals(0, dummy.UNDGs.Count);
				AssertEquals(1, dummy.UNDGs.FirstItemForBinding.Count);
				Assert("IsAutoAddedItem", dummy.UNDGs.FirstItemForBinding[0].IsAutoAddedItem);
				Assert("IsSavedByFactory", !dummy.UNDGs.FirstItemForBinding[0].IsSavedByFactory);
			});

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "0014A";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			dummy.UNDGs.FirstItemForBinding[0].DI_DG = subs.PK;
			dummy.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
			AssertEquals("UNDGs.Count", 1, dummy.UNDGs.Count);
			AssertEquals("UNDGs.FirstItemForBinding.Count", 1, dummy.UNDGs.FirstItemForBinding.Count);
			AssertEquals(dummy.UNDGs[0], dummy.UNDGs.FirstItemForBinding[0]);
			Assert("IsAutoAddedItem", !dummy.UNDGs.FirstItemForBinding[0].IsAutoAddedItem);
			Assert("IsSavedByFactory", dummy.UNDGs.FirstItemForBinding[0].IsSavedByFactory);
		}

		public void TestBindingItemReadonly()
		{
			DummyBizoWithUNDGs dummy = Factory.New<DummyBizoWithUNDGs>();

			dummy.UNDGs.SetReadOnlyIncludingChildren(true);

			AssertEquals(0, dummy.UNDGs.Count);
			AssertReadOnly(dummy.UNDGs, true);

			dummy.UNDGs.SetReadOnlyIncludingChildren(false);

			AssertEquals(0, dummy.UNDGs.Count);
			AssertReadOnly(dummy.UNDGs, false);

			dummy.UNDGs.SetReadOnlyIncludingChildren(true);

			AssertEquals(0, dummy.UNDGs.Count);
			AssertReadOnly(dummy.UNDGs, true);

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "0014A";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			dummy.UNDGs.FirstItemForBinding[0].DI_DG = subs.PK;
			dummy.UNDGs.FirstItemForBinding[0].LinkDefault(subs);

			AssertEquals(1, dummy.UNDGs.Count);
			AssertReadOnly(dummy.UNDGs, true);

			dummy.UNDGs.SetReadOnlyIncludingChildren(false);

			AssertEquals(1, dummy.UNDGs.Count);
			AssertReadOnly(dummy.UNDGs, false);

			dummy.UNDGs.SetReadOnlyIncludingChildren(true);

			AssertEquals(1, dummy.UNDGs.Count);
			AssertReadOnly(dummy.UNDGs, true);
		}

		void AssertReadOnly(UNDGDataItemCollection undgs, bool readOnly)
		{
			AssertEquals(readOnly, undgs.FirstItemForBinding.ReadOnly);
			AssertEquals(readOnly, undgs.FirstItemForBinding[0].DI_DG_NKSubsInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGSubstanceManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGSubstanceManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGIsCombustibleManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGIsCombustibleManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGFlashPointManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGFlashPointManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGContactManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGContactManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGDGManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGDGManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGClassManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGClassManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGMarinePollutantManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGMarinePollutantManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGTechnicalNameManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGTechnicalNameManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGVolumeManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGVolumeManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGVolumeUnitManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGVolumeUnitManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGWeightManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGWeightManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGWeightUnitManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGWeightUnitManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGHasOverpackManager.ReadOnly);
			AssertEquals(readOnly, undgs.UNDGHasOverpackManager.ValueInfo.ReadOnly);
		}

		public void TestItemDeletedWhenClassAndSubstanceAreBlanked()
		{
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			AssertEquals(0, dummy.UNDGs.Count);

			dummy.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals(1, dummy.UNDGs.Count);
			AssertEquals(false, dummy.UNDGs[0].IsAutoAddedItem);
			AssertEquals(1, dummy.UNDGs.FirstItemForBinding.Count);

			dummy.UNDGs.FirstItemForBinding[0].DI_DG = ZGuid.Empty;
			dummy.UNDGs.FirstItemForBinding[0].DI_IMOClass = "";
			AssertEquals(0, dummy.UNDGs.Count);
			AssertEquals(true, dummy.UNDGs.FirstItemForBinding[0].IsAutoAddedItem);
			AssertEquals(1, dummy.UNDGs.FirstItemForBinding.Count);
		}

		public void TestDoesNotCreateEmptyParentCodeAndIdWhenInvalidCodeIsEnteredFirst()
		{
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			AssertEquals(0, dummy.UNDGs.Count);

			dummy.UNDGs.FirstItemForBinding[0].DI_DG = ZGuid.Invalid;
			dummy.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var addedItemsFactory = Factory.GetChanges().GetAddedObjects();
			AssertNotNull(addedItemsFactory);
			var hasEmptyParentId = addedItemsFactory.Where(x => x is UNDGDataItem).Any(x => ((UNDGDataItem)x).DI_ParentID.IsEmpty && x.IsSavedByFactory);
			var hasEmptyParentTableCode = addedItemsFactory.Where(x => x is UNDGDataItem).Any(x => ((UNDGDataItem)x).DI_ParentTableCode.IsEmpty && x.IsSavedByFactory);
			AssertEquals("Should not have added empty ParentId item to insert", false, hasEmptyParentId);
			AssertEquals("Should not have added empty ParentTableCode item to insert", false, hasEmptyParentTableCode);
		}

		public void TestOverpack()
		{
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			var undg = dummy.UNDGs.FirstItemForBinding[0];
			undg.DI_HasOverpack = true;
			Assert("Overpack ID is not read-only", !undg.DI_OverpackIDInfo.ReadOnly);

			undg.DI_OverpackID = "12345";
			AssertEquals("12345", undg.DI_OverpackID);

			undg.DI_HasOverpack = false;
			Assert("Overpack ID is read-only", undg.DI_OverpackIDInfo.ReadOnly);
			AssertEquals("", undg.DI_OverpackID);
		}

		public void TestNonEmptyInvalidItemMakesCollectionInvalid()
		{
			var msg = "Enter a valid DG Substance.";
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			dummy.UNDGs.FirstItemForBinding[0].DI_DG = ZGuid.Invalid;
			AssertEquals(1, dummy.UNDGs.Count);
			AssertHasErrorContaining(dummy.UNDGs[0].DI_DGInfo, msg);

			dummy.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals(1, dummy.UNDGs.Count);
			AssertNoErrorContaining(dummy.UNDGs[0].DI_DGInfo, msg);
		}

		public void TestDefaultValues()
		{
			DummyBizoWithUNDGs dummy = Factory.New<DummyBizoWithUNDGs>();
			UNDGDataItem item = dummy.UNDGs.AddNew();
			AssertEquals(DummyBizoSchema.Constants.Prefix, item.DI_ParentTableCode);
			AssertEquals(dummy.PK, item.DI_ParentID);
		}

		public void TestTryGetOrCreate()
		{
			DummyBizoWithUNDGs dummy = Factory.New<DummyBizoWithUNDGs>();

			AssertEquals(null, dummy.UNDGs.TryGetOrCreate("1234a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));

			var subs1 = Factory.New<UNDGSubstance>();
			subs1.DG_Code = "1234a";
			subs1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = "1235b";
			subs2.DG_Standard = "IAT";

			var item = dummy.UNDGs.AddNew();
			item.LinkDefault(subs2);
			AssertEquals(item, dummy.UNDGs.TryGetOrCreate("1235b", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA));

			var newItem = dummy.UNDGs.TryGetOrCreate("1234a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			AssertEquals(subs1, newItem.Substance);
			AssertEquals(newItem, dummy.UNDGs.TryGetOrCreate("1234a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
			AssertEquals(2, dummy.UNDGs.Count);
		}

		public void TestDoNotSaveNonPersistedData()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var invoice = Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine = Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			Factory.Save();

			invoiceLine.JI_Description = "MB1";
			((ILightValidationInternals)invoiceLine).IsValid = true;
			((BusinessObject)invoiceLine).HasChanges = false;

			UNDGDataItem item = null;
			((IBindingList)invoiceLine).ListChanged += (s, e) =>
			{
				item = ((IUNDGDataItemProvider)invoiceLine).UNDGs.FirstItemForBinding[0];
			};

			Factory.Save();

			AssertNull("item is null due to bizObj.SuspendListChanged()", item);
		}

		public void TestDataItemCollectionSubstanceUpdateHandlerCount()
		{
			var dummy = Factory.NewWithValidTestData<DummyBizoWithUNDGs>();
			var collection = new UNDGDataItemCollection(dummy);
			var dgItem = Factory.New<UNDGDataItem>();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = $"123a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_LQMaxAmt = 250;
			subs.DG_LQMaxAmtUQ = "G";

			collection.Add(dgItem);
			var firstItem = collection.FirstItemForBinding;

			dgItem.DI_DG = subs.PK;
			dgItem.DI_DG = ZGuid.Empty;

			AssertEquals("SubstanceUpdated event should be added once", 1, firstItem[0].SubstanceUpdatedCounter);
		}

		public void TestAsString()
		{
			var dummy = Factory.NewWithValidTestData<DummyBizoWithUNDGs>();
			var collection = new UNDGDataItemCollection(dummy);

			var dgItem = Factory.New<UNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "123a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			dgItem.DI_DG = subs.PK;
			collection.Add(dgItem);

			AssertEquals("123a", collection.AsString);

			var dgItem2 = Factory.New<UNDGDataItem>();
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = "123b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			dgItem2.DI_DG = subs2.PK;
			collection.Add(dgItem2);

			AssertEquals("123a,123b", collection.AsString);
		}

		protected override UNDGDataItemCollection GetCollectionToTest()
		{
			var dummy = Factory.NewWithValidTestData<OrgSupplierPart>();
			var collection = new UNDGDataItemCollection(dummy);
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "a", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "0014a";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			collection.AddNew().LinkDefault(subs);
			Factory.Save();
			return collection;
		}
	}
}
