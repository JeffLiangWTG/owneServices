using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(ItemsBusinessObjectForTest))]
	public class ItemsBusinessObjectTest : ItemsBusinessObjectTestCase<ItemsBusinessObjectTest.ItemsBusinessObjectForTest>
	{
		#region Related Entities

		#region TestPackableItemParentsForBinding

		public void TestPackableItemParentsForBinding()
		{
			var bizO = GetNewItemsBusinessObject();

			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }, bizO.PackableItemParentsForBinding);
			AssertEquals("Collection should be registered editable.", true, bizO.IsRegisteredEditableChildObject(bizO.PackableItemParentsForBinding));
			AssertEquals("Collection should be cached.", bizO.PackableItemParentsForBinding, bizO.PackableItemParentsForBinding);
		}

		#endregion

		#region TestPackableItemParentsForBinding_Refresh

		public void TestPackableItemParentsForBinding_Refresh()
		{
			var bizO = GetNewItemsBusinessObject();
			bizO.RefreshPackableItemParentsForBinding(); // should work when collection is null
			var wrappers = bizO.PackableItemParentsForBinding;
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }, wrappers);

			bizO.NewPackableItemParentsForBinding = new[] { Data.DummyLine3Wrapper };
			bizO.RefreshPackableItemParentsForBinding();
			AssertEquals("Refreshing the collection should rebuild it and not change the instance (which would bugger binding).", wrappers, bizO.PackableItemParentsForBinding);
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine3Wrapper }, wrappers);
		}

		#endregion

		#endregion

		#region Properties

		#region TestPackageQtyToCreate

		public void TestPackageQtyToCreate()
		{
			var bizO = GetNewItemsBusinessObject();
			AssertEquals("Precondition", false, bizO.OnPackageQtyToCreateChangedInvoked);
			AssertEquals("Default Package Qty should be 1.", 1, bizO.PackageQtyToCreate);

			bizO.PackageQtyToCreate = 2;
			AssertEquals(true, bizO.OnPackageQtyToCreateChangedInvoked);
			AssertEquals(2, bizO.PackageQtyToCreate);
		}

		#endregion

		#region TestPackageTypeToCreate

		public void TestPackageTypeToCreate()
		{
			var bizO = GetNewItemsBusinessObject();

			AssertEquals(3, bizO.PackageTypeToCreateInfo.MaxLength);
			AssertEquals("Default Package Type should be defaulted from DefaultPackageTypeToCreate.", "BOX", bizO.PackageTypeToCreate);

			bizO.PackageTypeToCreate = "";
			AssertEquals("", bizO.PackageTypeToCreate);

			bizO.PackageTypeToCreate = "PLT";
			AssertEquals("PLT", bizO.PackageTypeToCreate);
		}

		#endregion

		#endregion

		#region Flags

		#region TestCanAutoApplyChanges

		public void TestCanAutoApplyChanges()
		{
			var bizO = GetNewItemsBusinessObject();
			AssertEquals(false, bizO.CanAutoApplyChanges);
		}

		#endregion

		#region TestCanScanPackQuanity

		public void TestCanScanPackQuanity()
		{
			var bizO = GetNewItemsBusinessObject();
			AssertEquals(false, bizO.CanScanPackQuanity);
		}

		#endregion

		#region TestHasItemsWithQtyGreatherThanZero

		public void TestHasItemsWithQtyGreatherThanZero()
		{
			AssertItemsWithQtyGreatherThanZero(b => b.HasItemsWithQtyGreatherThanZero);
		}

		#endregion

		#region TestIsAnythingSelectedToPackOrUnpack

		public void TestIsAnythingSelectedToPackOrUnpack()
		{
			AssertItemsWithQtyGreatherThanZero(b => b.IsAnythingSelectedToPackOrUnpack);
		}

		#endregion

		void AssertItemsWithQtyGreatherThanZero(Func<ItemsBusinessObject, bool> propertyToInvoke)
		{
			var bizO = GetNewItemsBusinessObject();
			AssertEquals(false, propertyToInvoke(bizO));

			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 1;
			AssertEquals(true, propertyToInvoke(bizO));
		}

		#endregion

		#region Lookups

		public void TestPackageTypes()
		{
			var bizO = GetNewItemsBusinessObject();

			AssertEquals(true, bizO.PackageTypes.Any());
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Factory, excludeCNT: false), bizO.PackageTypes);
			AssertEquals(true, bizO.PackageTypes.ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}

		public void TestPackageTypes_WithPackTypesToExclude()
		{
			var bizO = GetNewItemsBusinessObject();
			// package with Custom Pack Types Parent Job should exclude specified Pack Types
			Data.Dummy.PackTypesToExclude = new ZString[] { "BOX", "PAI" };
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Factory, excludeCNT: false).Where(p => p.F3_Code != "BOX" && p.F3_Code != "PAI"),
				bizO.PackageTypes);
		}

		#endregion

		#region Validation

		public void TestRunPreSaveValidation()
		{
			var bizO = GetNewItemsBusinessObject();

			AssertNoRowErrors("Precondition", bizO);
			bizO.RunPreSaveValidation();
			AssertHasRowError("RunPreSaveValidation should invoke ValidateAll().", bizO, "Nothing is selected to Test Desc.");
		}

		#endregion

		#region TestMode

		public void TestMode()
		{
			var bizO = GetNewItemsBusinessObject();
			AssertEquals(PackOrUnpackMode.SingleItemNoQty, bizO.Mode);

			AssertEquals("Precondition.", 1, bizO.GetModeHitCount);
			var poke = bizO.Mode;
			AssertEquals("Mode should be cached.", 1, bizO.GetModeHitCount);
		}

		#endregion

		#region TestRunValidationAndApplyChanges

		public void TestRunValidationAndApplyChanges()
		{
			var bizO = GetNewItemsBusinessObject();

			AssertEquals(false, bizO.RunValidationAndApplyChanges());
			AssertEquals("BizO had errors, should *not* have called ApplyChanges.", false, bizO.IsApplyChangesInvoked);

			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 1m;
			AssertEquals(true, bizO.RunValidationAndApplyChanges());
			AssertEquals("BizO had no error, should have called ApplyChanges.", true, bizO.IsApplyChangesInvoked);
		}

		#endregion

		#region TestSetPackOrRemoveQuantity

		public override void TestSetPackOrRemoveQuantity()
		{
			var bizO = GetNewItemsBusinessObject();
			var wrapper = bizO.PackableItemParentsForBinding[0];
			bizO.SetPackOrRemoveQuantity(wrapper, 10m);
			AssertEquals(10m, wrapper.ProposedPackQty);
		}

		#endregion

		#region Implementation

		protected override ItemsBusinessObjectForTest GetNewItemsBusinessObject()
		{
			Data.CreatePackingData();
			return new ItemsBusinessObjectForTest(Data);
		}

		protected override ZString ProposedColumnName
		{
			get { return PackableItemParentWrapper.Schema.ProposedPackQty; }
		}

		protected override ZString ProposedDescription
		{
			get { return "Test Desc"; }
		}

		protected override Type ValidationType
		{
			get { return typeof(ItemsBusinessObjectValidation); }
		}

		#region ItemsBusinessObjectForTest

		public class ItemsBusinessObjectForTest : ItemsBusinessObject, IScanningItemsBusinessObject
		{
			public ItemsBusinessObjectForTest(TestDataForPacking data)
				: base(data.PackageJob)
			{
				Data = data;
			}

			public readonly TestDataForPacking Data;

			protected override IEnumerable<PackableItemParentWrapper> GetNewPackableItemParentsForBinding()
			{
				GetNewPackableItemParentsForBindingInvokeCount++;
				return NewPackableItemParentsForBinding;
			}

			public IEnumerable<PackableItemParentWrapper> NewPackableItemParentsForBinding
			{
				set { newPackableItemParentsForBinding = value; }
				get { return newPackableItemParentsForBinding ?? new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }; }
			}

			IEnumerable<PackableItemParentWrapper> newPackableItemParentsForBinding;

			protected override void OnPackageQtyToCreateChanged()
			{
				base.OnPackageQtyToCreateChanged();
				OnPackageQtyToCreateChangedInvoked = true;
			}

			protected override ZString DefaultPackageTypeToCreate
			{
				get { return "BOX"; }
			}

			protected override ZString ProposedPackUnpackColumnNameCore
			{
				get { return PackableItemParentWrapper.Schema.ProposedPackQty; }
			}

			protected override void ApplyChanges()
			{
				IsApplyChangesInvoked = true;
			}

			protected override ZString ProposedPackUnpackDescriptionCore
			{
				get { return "Test Desc"; }
			}

			public bool OnPackageQtyToCreateChangedInvoked { get; private set; }
			public int GetNewPackableItemParentsForBindingInvokeCount { get; private set; }
			public bool IsApplyChangesInvoked;

			#region Mode

			public void SetModeForTest(PackOrUnpackMode mode)
			{
				var field = typeof(ItemsBusinessObject).GetField("modeCore", BindingFlags.Instance | BindingFlags.NonPublic);
				field.SetValue(this, mode);
			}

			public int GetModeHitCount
			{
				get;
				private set;
			}

			protected override PackOrUnpackMode GetMode()
			{
				GetModeHitCount++;
				return base.GetMode();
			}

			#endregion

			#region IScanningItemsBusinessObject

			void IScanningItemsBusinessObject.SelectPackableItemParent(PackableItemParentWrapper wrapper)
			{
				throw new NotImplementedException();
			}

			public bool IsUserEnteringQty
			{
				get;
				set;
			}

			public bool IsTUN
			{
				get;
				set;
			}

			ZString IScanningItemsBusinessObject.TUNCode
			{
				get { throw new NotImplementedException(); }
			}

			bool IScanningItemsBusinessObject.AddFilterIfValidAttribute(ZString value)
			{
				throw new NotImplementedException();
			}

			IEnumerable<PackableItemParentWrapper> IScanningItemsBusinessObject.GetAllWrappers()
			{
				return GetNewPackableItemParentsForBinding();
			}

			#endregion

			#region SetPackOrRemoveQuantity

			public override void SetPackOrRemoveQuantity(PackableItemParentWrapper wrapper, decimal quantity)
			{
				wrapper.ProposedPackQty = quantity;
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
