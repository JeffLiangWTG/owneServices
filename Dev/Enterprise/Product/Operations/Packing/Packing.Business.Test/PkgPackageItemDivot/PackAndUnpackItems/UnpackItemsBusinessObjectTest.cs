using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(UnpackItemsBusinessObject))]
	public class UnpackItemsBusinessObjectTest : ItemsBusinessObjectTestCase<UnpackItemsBusinessObject>
	{
		#region Related Entities

		#region TestPackableItemParents

		public void TestPackableItemParents()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1 = container.Packages.AddNew("PL1");
			var pallet2 = container.Packages.AddNew("PL2");
			var box1 = pallet1.Packages.AddNew("BX1");
			var box2 = pallet2.Packages.AddNew("BX2");
			var ctn2 = box2.Packages.AddNew("CTN");

			// pack pallet 1
			var pallet1_PackedItem1 = pallet1.Pack_ForTesting(Data.DummyLine1, 10m);
			var pallet1_PackedItem2 = pallet1.Pack_ForTesting(Data.DummyLine2, 10m);

			// pack pallet 2
			var pallet2_PackedItem1 = pallet2.Pack_ForTesting(Data.DummyLine1, 2m); // will not be removed.
			var pallet2_PackedItem2 = pallet2.Pack_ForTesting(Data.DummyLine2, 3m);

			// pack box 1
			var box1_PackedItem1 = box1.Pack_ForTesting(Data.DummyLine1, 5m);

			// pack box 2
			var box2_PackedItem1 = box2.Pack_ForTesting(Data.DummyLine1, 2m);
			var box2_PackedItem2 = box2.Pack_ForTesting(Data.DummyLine2, 1m);

			// pack the carton
			var ctn2_PackedItem2 = ctn2.Pack_ForTesting(Data.DummyLine2, 1m);

			// select some packs and items to remove. make sure we try to remove a child whose parent is also flagged for remove.
			var packagesToRemove = new[] { pallet1, box1, box2 };
			var packedItemsToRemove = WrapPackedItemsWithBarcodeYes(new[] { box1_PackedItem1, pallet2_PackedItem2 });
			var bizO = new UnpackItemsBusinessObject(Data.PackageJob, packedItemsToRemove, packagesToRemove);

			AssertContainsExactElementsInAnyOrder
			(
				new[]
				{
					pallet1_PackedItem1,
					pallet1_PackedItem2,
					pallet2_PackedItem2,
					box1_PackedItem1,
					box2_PackedItem1,
					box2_PackedItem2,
					ctn2_PackedItem2
				},
				bizO.PackableItemParentsForBinding.SelectMany<PackableItemParentWrapper, PkgPackageItemDivotsWrapper>(w => w.GroupedPackedItems)
			);

			foreach (var wrapper in bizO.PackableItemParentsForBinding)
			{
				AssertEquals("The Remove Qty should default to the Packed Qty.", wrapper.PackedQtyFromWrapper, wrapper.ProposedRemoveQty);
			}
		}

		public void TestCanAutoApplyChanges_PackingParentWithoutPackableItems()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyParentJob);
			var package = Factory.New<DummyParentJob>();
			var packageJob = package.Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = package.PK;
			packageJob.KJ_ParentTableCode = package.TablePrefix;
			packageJob.KJ_IsFinalized = package.IsParentJobFinalised;
			package.RegisterEditableChildObject(packageJob);

			var pallet1 = packageJob.Packages.AddNew("PLT");
			var packagesToRemove = new[] { pallet1 };
			var bizO = new UnpackItemsBusinessObject(packageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), packagesToRemove);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(true, bizO.CanAutoApplyChanges);
			});
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
		}

		class DummyParentJob : DummyBusinessObject, IPackingParent
		{
			public DummyParentJob(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString JobDescription
			{
				get { return ZString.Empty; }
			}

			public ZString JobNo
			{
				get { return ZString.Empty; }
			}

			public ZString ConnoteNo
			{
				get { return ZString.Empty; }
			}
			public ZString GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
			{
				throw new NotImplementedException();
			}

			public bool IsParentJobFinalised { get; set; }

			public bool IsAutoPrintAllowed
			{
				get { return false; }
			}

			public bool IsLoosePackageIDsSupported
			{
				get { return false; }
			}

			public bool ShouldPackTrackedPackagesViaDivot
			{
				get { return false; }
			}

			public bool IsReadOnly { get; set; }

			public bool IsPackingJobReadOnly { get; set; }

			public bool IsScanEventsVisible { get; set; }

			public ControllerID ControllerID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
			{
				return new PackageActionStrategy(package);
			}

			public void OnContainerIDChanged(PkgPackage container)
			{
				throw new NotImplementedException();
			}

			public void OnPackageJobReleased()
			{
				throw new NotImplementedException();
			}

			public void OnPackageDelete(PkgPackage package)
			{
				throw new NotImplementedException();
			}

			public void OnPackageJobCreatedOrLoaded(PkgPackageJob packageJob)
			{
				throw new NotImplementedException();
			}

			public DocumentOptions DocumentOptions { get; set; }

			public ZGuid LogsParentPK
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public string LogsParentTableName
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public Logs Logs
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public BusinessObjectFactory LogsFactory
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public BusinessObject[] BusinessObjectsWithRelatedEvents
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool DeferFiringWorkflow
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZString CarrierServiceLevelCode(PkgPackage package)
			{
				throw new NotImplementedException();
			}

			public OrgHeader CarrierBookingAgent => throw new NotImplementedException();

			public OrgHeader GetCarrier(PkgPackage package)
			{
				throw new NotImplementedException();
			}

			public void BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
			{
				throw new NotImplementedException();
			}

			void IStmALogParent.ProcessLog(IStmALog log)
			{
				throw new NotImplementedException();
			}

			bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

			IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

			ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

			PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Standard;

			public ZString TransportReference { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

			ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

			void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
			{
			}

			NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;
		}
		#endregion

		#endregion

		#region Properties

		#region DefaultPackageTypeToCreate

		public void TestDefaultPackageTypeToCreate()
		{
			AssertEquals("", GetNewItemsBusinessObject().PackageTypeToCreate);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsAnythingSelectedToPackOrUnpack

		public void TestIsAnythingSelectedToPackOrUnpack()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew();
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 5m);

			var bizO = new UnpackItemsBusinessObject(Data.PackageJob, new[] { new PkgPackageItemDivotsWrapperAndBarcode(packedItem, BarcodeMatch.Yes) }, Array.Empty<PkgPackage>());
			AssertEquals(true, bizO.IsAnythingSelectedToPackOrUnpack);

			bizO.PackableItemParentsForBinding[0].ProposedRemoveQty = 0;
			AssertEquals(false, bizO.IsAnythingSelectedToPackOrUnpack);

			var emptyBox = Data.PackageJob.Packages.AddNew();
			AssertEquals("Should be able to unpack an empty package.", true, new UnpackItemsBusinessObject(Data.PackageJob, Enumerable.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), new[] { emptyBox }).IsAnythingSelectedToPackOrUnpack);
		}

		#endregion

		#region TestCanAutoApplyChanges

		public void TestCanAutoApplyChanges()
		{
			Data.CreatePackingData();

			var pallet1 = Data.PackageJob.Packages.AddNew();
			var pallet2 = Data.PackageJob.Packages.AddNew();
			var box2 = pallet2.Packages.AddNew();
			var box2_PackedItem = box2.Pack_ForTesting(Data.DummyLine1, 5m); // pack goods into Box2

			var bizO1 = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), Array.Empty<PkgPackage>());
			AssertEquals(true, bizO1.CanAutoApplyChanges);

			var bizO2 = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), new[] { pallet1 });
			AssertEquals("Selected an empty package (Pallet1), expect can auto apply changes.", true, bizO2.CanAutoApplyChanges);

			var bizO3 = new UnpackItemsBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(box2_PackedItem), new[] { pallet1 });
			AssertEquals("Selected a Packed Item, expect cannot auto apply changes.", false, bizO3.CanAutoApplyChanges);

			var bizO4 = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), new[] { pallet2 });
			AssertEquals("Selected Pallet2 whose child (Box2) has items, expect can auto apply changes.", false, bizO4.CanAutoApplyChanges);

			var bizO5 = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), new[] { box2 });
			AssertEquals("Selected Box2 which has items, expect can auto apply changes.", false, bizO5.CanAutoApplyChanges);
		}

		#endregion

		#region TestCanAutoApplyChanges_CallsBeforeUnpackingPackages

		public void TestCanAutoApplyChanges_CallsBeforeUnpackingPackages()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew();
			var package2 = Data.PackageJob.Packages.AddNew();
			var package3 = Data.PackageJob.Packages.AddNew();
			var unpack = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), new[] { package1, package2 });
			unpack.RunValidationAndApplyChanges();
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, Data.Dummy.BeforeUnpackingPackagesWasCalledOnThisPackages);
		}

		#endregion

		#endregion

		#region RunValidationAndApplyChanges

		#region TestRunValidationAndApplyChanges

		public void TestRunValidationAndApplyChanges()
		{
			Data.CreatePackingData();

			var pallet1 = Data.PackageJob.Packages.AddNew("PL1");
			var pallet2 = Data.PackageJob.Packages.AddNew("PL2");
			var box1 = pallet1.Packages.AddNew("BX1");
			var box2 = pallet2.Packages.AddNew("BX2");
			var ctn2 = box2.Packages.AddNew("CN2");

			// pack a bunch of items.
			var pallet1_PackedItem1 = pallet1.Pack_ForTesting(Data.DummyLine1, 10m);
			var pallet1_PackedItem2 = pallet1.Pack_ForTesting(Data.DummyLine2, 10m);
			var pallet2_PackedItem1 = pallet2.Pack_ForTesting(Data.DummyLine1, 10m);
			var pallet2_PackedItem2 = pallet2.Pack_ForTesting(Data.DummyLine2, 10m);
			var box1_PackedItem1 = box1.Pack_ForTesting(Data.DummyLine1, 10m);
			var box2_PackedItem1 = box2.Pack_ForTesting(Data.DummyLine1, 10m);
			var box2_PackedItem2 = box2.Pack_ForTesting(Data.DummyLine2, 10m);
			AssertEquals("Precondition - Item Weights should be added to all parent Packages.", 60m, pallet1.KP_Weight);
			AssertEquals("Precondition - Item Weights should be added to all parent Packages.", 80m, pallet2.KP_Weight);

			// specify some packages + some items to be removed.
			var packagesToRemove = new[] { pallet1, box2 };
			var itemsToRemove = new[] { pallet1_PackedItem2 };
			var bizO = new UnpackItemsBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(itemsToRemove), packagesToRemove);

			// for all packed items due for remove, specify amounts to remove (the user would do this via a grid)
			bizO.PackableItemParentsForBinding.FindByPackedItem(pallet1_PackedItem2).ProposedRemoveQty = 9; // leave 1 remaining to test that Pallet1 is not removed.
			bizO.RunValidationAndApplyChanges();

			// ensure the remaining packages were not deleted
			AssertContainsExactElementsInAnyOrder("No Pallets should be deleted as at least one item or child item is packed.", new[] { pallet1, pallet2 }, Data.PackageJob.Packages);
			AssertEquals(false, pallet1.IsDeleted);
			AssertEquals(false, pallet2.IsDeleted);
			AssertEquals("Remove should reduce parent package weight.", 2m, pallet1.KP_Weight);
			AssertEquals("Remove should reduce parent package weight.", 40m, pallet2.KP_Weight);

			// ensure the remaining packed items were not deleted and have a correct qty
			AssertEquals("Packed Item was not fully removed, should not be deleted.", false, pallet1_PackedItem2.IsDeleted);
			AssertEquals("Packed Item was not selected for remove, should not be deleted.", false, pallet2_PackedItem1.IsDeleted);
			AssertEquals("Packed Item was not selected for remove, should not be deleted.", false, pallet2_PackedItem2.IsDeleted);

			AssertEquals("10 packed then 9 removed, should be 1 left.", 1m, pallet1_PackedItem2.PackedQty);
			AssertEquals("Packed Item was not selected for remove, Qty should not have been reduced.", 10m, pallet2_PackedItem1.PackedQty);
			AssertEquals("Packed Item was not selected for remove, Qty should not have been reduced.", 10m, pallet2_PackedItem2.PackedQty);

			// ensure packages whose divots were deleted are also deleted
			AssertEquals(true, box1.IsDeleted);
			AssertEquals(true, box2.IsDeleted);
			AssertEquals(true, ctn2.IsDeleted);

			// ensure fully removed divots were deleted
			AssertEquals(true, pallet1_PackedItem1.IsDeleted);
			AssertEquals(true, box1_PackedItem1.IsDeleted);
			AssertEquals(true, box2_PackedItem1.IsDeleted);
			AssertEquals(true, box2_PackedItem2.IsDeleted);

			// ensure factory.save doesn't die in the arse
			Factory.Save();
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithNoData

		[ExpectNoExceptions]
		public void TestRunValidationAndApplyChanges_WithNoData()
		{
			Data.CreatePackingData();
			var bizO = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), Array.Empty<PkgPackage>());
			bizO.RunValidationAndApplyChanges();
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WhenParentIsNotIPackingParentWithPackableItems

		public void TestRunValidationAndApplyChanges_WhenParentIsNotIPackingParentWithPackableItems()
		{
			var parent = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = parent.PK;
			packageJob.KJ_ParentTableCode = parent.TablePrefix;

			var box = packageJob.Packages.AddNew("BOX");
			AssertEquals("Precondition", true, parent is IPackingParent);
			AssertEquals("Precondition - Parent is not an IPackingParentWithPackableItems.", false, parent is IPackingParentWithPackableItems);
			AssertNotNull("Precondition", packageJob.ParentJob);

			var bizO = new UnpackItemsBusinessObject(packageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), new PkgPackage[] { box });
			AssertNoExceptionThrown(() => bizO.RunValidationAndApplyChanges());
		}

		#endregion

		#region TestRunValidationAndApplyChanges_DeletesEmptyPackages_WhenSelected

		public void TestRunValidationAndApplyChanges_DeletesEmptyPackages_WhenSelected()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var pallet1 = packageJob.Packages.AddNew("PLT");
			var pallet2 = packageJob.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var box1Empty = pallet1.Packages.AddNew("BOX");

			// pack a bunch of items.
			var pallet1_PackedItem1 = pallet1.Pack_ForTesting(Data.DummyLine1, 10m);
			var pallet2_PackedItem = pallet2.Pack_ForTesting(Data.DummyLine1, 10m);
			var box1_PackedItem1 = box1.Pack_ForTesting(Data.DummyLine1, 10m);

			// specify some items to be removed.
			var packagesToRemove = new[] { pallet1, pallet2, box1 };
			var itemsToRemove = new[] { pallet1_PackedItem1 };
			var bizO = new UnpackItemsBusinessObject(packageJob, WrapPackedItemsWithBarcodeYes(itemsToRemove), packagesToRemove);
			bizO.PackableItemParentsForBinding.FindByPackedItem(pallet2_PackedItem).ProposedRemoveQty = 5m; // only remove half.

			bizO.RunValidationAndApplyChanges();
			AssertEquals(true, pallet1.IsDeleted);
			AssertEquals(true, box1.IsDeleted);
			AssertEquals(true, box1Empty.IsDeleted);
			AssertEquals("Pallet 2 was not fully removed and should not be deleted.", false, pallet2.IsDeleted);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_InitiatesMassPackageProcess

		public void TestRunValidationAndApplyChanges_InitiatesMassPackageProcess()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var pallet1 = packageJob.Packages.AddNew("PLT");
			var pallet2 = packageJob.Packages.AddNew("PLT");

			// pack a bunch of items.
			var pallet1_PackedItem = pallet1.Pack_ForTesting(Data.DummyLine1, 10m);
			var pallet2_PackedItem = pallet2.Pack_ForTesting(Data.DummyLine1, 10m);

			var packageCountChangedInvoked = false;
			var divotCountChanged1Invoked = false;
			var divotCountChanged2Invoked = false;
			Data.PackageJob.Packages.CollectionCountChange += (sender, e) =>
			{
				packageCountChangedInvoked = true;
				AssertEquals("Mass Package Process should be running when Unpacking.", true, Data.PackageJob.IsMassPackageProcessRunning);
			};

			pallet1.PackedItemDivots.CountChanged += (sender, e) =>
			{
				divotCountChanged1Invoked = true;
				AssertEquals("Mass Package Process should be running when Unpacking.", true, Data.PackageJob.IsMassPackageProcessRunning);
			};

			pallet2.PackedItemDivots.CountChanged += (sender, e) =>
			{
				divotCountChanged2Invoked = true;
				AssertEquals("Mass Package Process should be running when Unpacking.", true, Data.PackageJob.IsMassPackageProcessRunning);
			};

			// specify some items to be removed.
			var packagesToRemove = new[] { pallet1, pallet2 };
			var itemsToRemove = new[] { pallet1_PackedItem, pallet2_PackedItem };
			var bizO = new UnpackItemsBusinessObject(packageJob, WrapPackedItemsWithBarcodeYes(itemsToRemove), packagesToRemove);
			AssertEquals("Mass Package Process should *not* be running before Unpacking.", false, Data.PackageJob.IsMassPackageProcessRunning);

			bizO.RunValidationAndApplyChanges();
			AssertEquals("Mass Package Process should *not* be running after Unpacking.", false, Data.PackageJob.IsMassPackageProcessRunning);
			AssertEquals("Mass Package Process assertions should have run.", true, packageCountChangedInvoked);
			AssertEquals("Mass Package Process assertions should have run.", true, divotCountChanged1Invoked);
			AssertEquals("Mass Package Process assertions should have run.", true, divotCountChanged2Invoked);
		}

		#endregion

		#endregion

		#region TestSetPackOrRemoveQuantity

		public override void TestSetPackOrRemoveQuantity()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var pallet1 = packageJob.Packages.AddNew("PLT");

			// pack a item.
			var pallet1_PackedItem1 = pallet1.Pack_ForTesting(Data.DummyLine1, 10m);

			// specify the item to be removed.
			var packagesToRemove = new[] { pallet1 };
			var itemsToRemove = new[] { pallet1_PackedItem1 };
			var bizO = new UnpackItemsBusinessObject(packageJob, WrapPackedItemsWithBarcodeYes(itemsToRemove), packagesToRemove);
			var wrapper = bizO.PackableItemParentsForBinding.Single<PackableItemParentWrapper>();
			AssertEquals("Precondition", 0m, wrapper.ProposedPackQty);

			bizO.SetPackOrRemoveQuantity(wrapper, 10m);
			AssertEquals(10m, wrapper.ProposedRemoveQty);
		}

		#endregion

		#region Implementation

		protected override UnpackItemsBusinessObject GetNewItemsBusinessObject()
		{
			Data.CreatePackingData();
			var packedItem = Data.PackageJob.Packages.AddNew().Pack_ForTesting(Data.DummyLine1, 50m);
			return new UnpackItemsBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(packedItem), Array.Empty<PkgPackage>());
		}

		protected override ZString ProposedColumnName
		{
			get { return PackableItemParentWrapper.Schema.ProposedRemoveQty; }
		}

		protected override ZString ProposedDescription
		{
			get { return "Unpack"; }
		}

		protected override Type ValidationType
		{
			get { return typeof(UnpackItemsBusinessObjectValidation); }
		}

		#endregion
	}
}
