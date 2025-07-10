using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Packing.Business
{
	public sealed class PkgPackageItemDivotsWrapper : NonPersistentBusinessObject, IPackageParent, ICustomPropertyContainer, IPackableItemParentWrapper
	{
		PkgPackageItemDivotsWrapper(BusinessObjectFactory factory, GroupingKey key, IPackableItemParent packableItemParent)
			: base(factory)
		{
			Key = Argument.NotNull(key, nameof(key));
			PackableItemParentWrapped = Argument.NotNull(packableItemParent, nameof(packableItemParent));
			Divots = new HashSet<PkgPackageItemDivot>();
		}

		readonly IPackableItemParent PackableItemParentWrapped;
		readonly HashSet<PkgPackageItemDivot> Divots;

		#region New

		public static PkgPackageItemDivotsWrapper New(PkgPackageItemDivot divot, IPackableItemParent packableItemParent)
		{
			Argument.NotNull(divot, nameof(divot));

			var key = divot.PackedItem?.Key ?? EmptyKey.Instance;
			var result = new PkgPackageItemDivotsWrapper(divot.Factory, key, packableItemParent);
			result.AddDivotToWrapper(divot, key);

			return result;
		}

		#endregion

		#region Events

		public event EventHandler QuantityChanged;
		public event EventHandler DescriptionChanged;

		#region Hook

		void HookDivotEvents(PkgPackageItemDivot divot)
		{
			divot.BeforeDelete += Divot_BeforeDelete;
			divot.DeletedByDataRefresh += Divot_DeletedByDataRefresh;
			divot.KI_PackedQtyInfo.ValueChanged += PackedQty_QuantityChanged;
			// HookPackableItemEvents occurs later (for performance)
		}

		void HookPackableItemEvents(IPackableItem packableItem)
		{
			ThrowExceptionIfDeleted();

			var packableItemBO = (BusinessObject)packableItem;
			if (packableItemBO != null)
			{
				packableItemBO.HasChangesChanged += RefreshDescriptionIfNecessary;
			}
		}

		#endregion

		#region Unhook

		void UnhookDivotEvents(PkgPackageItemDivot divot)
		{
			divot.BeforeDelete -= Divot_BeforeDelete;
			divot.DeletedByDataRefresh -= Divot_DeletedByDataRefresh;
			divot.KI_PackedQtyInfo.ValueChanged -= PackedQty_QuantityChanged;
			UnhookPackableItemEvents(divot.PackedItem);
		}

		void UnhookPackableItemEvents(IPackableItem packableItem)
		{
			var packableItemBO = (BusinessObject)packableItem;
			if (packableItemBO != null)
			{
				packableItemBO.HasChangesChanged -= RefreshDescriptionIfNecessary;
			}
		}

		#endregion

		bool IsDivotsEmpty => Divots.Count == 0 || Divots.All(d => d.IsDeleted);

		#region Divot_BeforeDelete

		void Divot_BeforeDelete(object sender, EventArgs e)
		{
			var deletingDivot = (PkgPackageItemDivot)sender;
			if (deletingDivot != null && !deletingDivot.IsDeleted)
			{
				DeleteCore(deletingDivot);
			}
		}

		#endregion

		#region Divot_DeletedByDataRefresh

		void Divot_DeletedByDataRefresh(object sender, EventArgs e)
		{
			if (!IsDeleted && IsDivotsEmpty)
			{
				Delete(); // I have no more divots, therefore my existence is futile. Goodbye cruel world.
			}
		}

		#endregion

		#region QuantityChanged

		void PackedQty_QuantityChanged(object sender, EventArgs e)
		{
			QuantityChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region RefreshDescriptionIfNecessary

		void RefreshDescriptionIfNecessary(object sender, EventArgs e)
		{
			var packedItem = (IPackableItem)sender;
			if (packedItem.IsDeleted)
			{
				UnhookPackableItemEvents(packedItem);

				if (GetNonDeletedDivots().Select(d => d.PackedItem).All(p => p == null || p.IsDeleted))
				{
					description = PackableItemParentWrapper.ItemNoLongerExistsText;
					descriptionWithoutSupplement = PackableItemParentWrapper.ItemNoLongerExistsText;
					DescriptionChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		#endregion

		#endregion

		#region Related Entities

		public GroupingKey Key { get; }
		public IPackableItemParent PackableItemParent => PackableItemParentWrapped.IsDeleted ? null : PackableItemParentWrapped;
		public IEnumerable<IPackableItem> PackedItems => GetNonDeletedDivots().Select(d => d.PackedItem);
		public PkgPackage ParentPackage => FirstDivot?.ParentPackage;

		PkgPackageItemDivot FirstDivot => GetNonDeletedDivots().FirstOrDefault();

		#endregion

		#region Properties

		#region Code

		[ResourceStringData("PkgPackageItemDivot|Code", Caption = "Code")]
		public ZString Code => IsValidPackedItem ? PackableItemParent.Code : ZString.Empty;

		#endregion

		#region Description

		[ResourceStringData("PkgPackageItemDivot|DescriptionWithoutSupplement", Caption = "Description")]
		public ZString DescriptionWithoutSupplement
		{
			get
			{
				if (!descriptionWithoutSupplement.HasValue)
				{
					(description, descriptionWithoutSupplement) = GetDescription();
				}
				return descriptionWithoutSupplement.Value;
			}
		}

		ZString? descriptionWithoutSupplement;

		[ResourceStringData("PkgPackageItemDivot|Description", Caption = "Description")]
		public ZString Description
		{
			get
			{
				if (!description.HasValue)
				{
					(description, descriptionWithoutSupplement) = GetDescription();
				}
				return description.Value;
			}
		}

		ZString? description;

		public ZString CodeWithDescription
		{
			get
			{
				ZString codeWithDescription;

				var code = Code;
				if (code.IsEmpty)
				{
					codeWithDescription = Description;
				}
				else
				{
					codeWithDescription = $"{code} - {Description}";
				}

				return codeWithDescription;
			}
		}

		(ZString, ZString) GetDescription()
		{
			var descriptionBuilder = new ZStringBuilder();
			var descriptionWithoutSupplementBuilder = new ZStringBuilder();

			if (IsValidPackedItem)
			{
				GetNonDeletedDivots().ForEach(d => HookPackableItemEvents(d.PackedItem));

				var description = PackableItemParent.Description;

				descriptionBuilder.AppendIfNotEmpty(description);
				descriptionWithoutSupplementBuilder.AppendIfNotEmpty(description);

				var supplement = PackableItemParent.DescriptionSupplement;
				if (!supplement.IsEmpty)
				{
					descriptionBuilder.AppendIfNotEmpty(PackableItemParent.DescriptionSupplementSeparator);
					descriptionBuilder.AppendIfNotEmpty(supplement);
				}
			}
			else
			{
				descriptionBuilder.Append(PackableItemParentWrapper.ItemNoLongerExistsText);
				descriptionWithoutSupplementBuilder.Append(PackableItemParentWrapper.ItemNoLongerExistsText);
			}

			return (descriptionBuilder.ToStringWithDelimiterBetweenAppends(" "), descriptionWithoutSupplementBuilder.ToStringWithDelimiterBetweenAppends(" "));
		}

		#endregion

		#region PackagePK

		public ZGuid PackagePK => FirstDivot?.KI_KP_Package ?? ZGuid.Empty;

		#endregion

		#region PackedQty

		[ResourceStringData("PkgPackageItemDivotsWrapper|PackedQty", Caption = "Packed Quantity", MediumCaption = "Quantity", ShortCaption = "Qty.")]
		public ZDecimal PackedQty
		{
			get { return GetPackedQty(GetNonDeletedDivots()); }
		}

		ZDecimal GetPackedQty(IEnumerable<PkgPackageItemDivot> nonDeletedDivots) => nonDeletedDivots.Sum(d => d.KI_PackedQty);

		#endregion

		#region UQ

		[ResourceStringData("PkgPackageItemDivot|UQ", Caption = "UQ")]
		public ZString UQ => PackableItemParent?.TotalQtyUQ ?? ZString.Empty;

		#endregion

		#endregion

		#region IsValidPackedItem

		/// <summary>
		/// If the underlying item that a Divot is pointing to is gone, then the item
		/// no longer exists, we also check if the PackableItem is not deleted as a safety check.
		/// </summary>
		bool IsValidPackedItem => PackableItemParent != null && GetNonDeletedDivots().All(d => d.PackedItem != null);

		#endregion

		// functions

		#region AddDivotToWrapper

		internal void AddDivotToWrapper(PkgPackageItemDivot divot)
		{
			Argument.NotNull(divot, nameof(divot));
			AddDivotToWrapper(divot, divot.PackedItem?.Key ?? EmptyKey.Instance);
		}

		void AddDivotToWrapper(PkgPackageItemDivot divot, GroupingKey divotKey)
		{
			ThrowExceptionIfDeleted();

			if (Key != divotKey)
			{
				throw new ArgumentException(ZString.Format("Divot Key {0} differs from current Wrapper Key {1}.", divotKey, Key));
			}

			if (Divots.Count > 0 && PackagePK != divot.KI_KP_Package)
			{
				throw new ArgumentException(ZString.Format("Divot Package PK {0} differs from current Package PK {1}.", divot.KI_KP_Package, PackagePK));
			}

			HookDivotEvents(divot);
			Divots.Add(divot);
		}

		#endregion

		#region ReducePackedQty

		internal void ReducePackedQty(ZDecimal qtyToUnpack)
		{
			ThrowExceptionIfDeleted();
			if (qtyToUnpack <= 0m)
			{
				throw new ArgumentException("Quantity to Unpack should be greater than zero.", nameof(qtyToUnpack));
			}

			var nonDeletedDivots = GetNonDeletedDivots().ToArray();
			if (GetPackedQty(nonDeletedDivots) > qtyToUnpack)
			{
				var parentPackage = ParentPackage;
				using (parentPackage.SuspendPackedItemDivotsCountChanged())
				{
					foreach (var divot in nonDeletedDivots.OrderBy(d => d.KI_PackedQty))
					{
						var divotQty = divot.KI_PackedQty;
						if (divotQty <= qtyToUnpack)
						{
							divot.Delete();
							Divots.Remove(divot);

							if (divotQty == qtyToUnpack)
							{
								PackedQty_QuantityChanged(this, EventArgs.Empty); // will refresh ui
							}

							qtyToUnpack -= divotQty;
						}
						else
						{
							UpdatePackedQtyOnDivot(divot, -qtyToUnpack);
							SplitPackableItem(qtyToUnpack, divot);

							qtyToUnpack = 0;
						}

						if (qtyToUnpack == 0m)
						{
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// When attempting to split the packable item, if null, don’t attempt to split as there’s no point in doing so. 
		/// That way, when the user closes and reopens the form, they will see whatever remains packed 
		/// (assuming they didn’t unpack the entire amount) as “3x <item does not exist>”
		/// </summary>
		void SplitPackableItem(ZDecimal qtyToUnpack, PkgPackageItemDivot divot)
		{
			var originalPackedItem = divot.PackedItem;
			if (originalPackedItem != null)
			{
				UnhookPackableItemEvents(originalPackedItem); // since the packable item may change, we need to unhook the original one and later hook again after splitting

				var splitItem = SplitPackedItem(originalPackedItem, qtyToUnpack);

				if (divot.PackedItem == null) // this means the existing packable item was deleted as part of the split
				{
					divot.UpdateIPackableItem(splitItem); // re-point the divot to the new split item
				}

				HookPackableItemEvents(divot.PackedItem);
			}
		}

		static IPackableItem SplitPackedItem(IPackableItem packedItem, ZDecimal qtyToUnPack)
		{
			var splitPackableItem = PackableItemSplitHelper.SplitItem(packedItem, qtyToUnPack);
			splitPackableItem.ReMerge();

			return splitPackableItem;
		}

		void UpdatePackedQtyOnDivot(PkgPackageItemDivot divot, ZDecimal difference)
		{
			divot.KI_PackedQty += difference;
			AddWeightToParentPackage(difference);
		}

		#endregion

		#region SetDivotPackedQty

		internal void SetDivotPackedQty(PkgPackageItemDivot divot, ZDecimal quantity)
		{
			Argument.NotNull(divot, nameof(divot));

			if (!Divots.Contains(divot))
			{
				throw new ArgumentException(ZString.Format("Divot PK {0} doesn't exist on Wrapper.", divot.PK));
			}

			if (quantity != divot.KI_PackedQty)
			{
				UpdatePackedQtyOnDivot(divot, quantity - divot.KI_PackedQty);
			}
		}

		#endregion

		#region AddWeightToParentPackage

		void AddWeightToParentPackage(ZDecimal qty)
		{
			var package = ParentPackage;
			if (package != null)
			{
				var weightUQ = PackableItemParent?.WeightUQ ?? ZString.Empty;
				if (!weightUQ.IsEmpty)
				{
					var packageWeightUQ = (string)package.KP_WeightUQ;
					if (packageWeightUQ.Length == 0)
					{
						packageWeightUQ = PackingRegistry.Instance.WeightUnit.Value;
						package.KP_WeightUQ = packageWeightUQ;
					}

					if (Constants.Weight.ContainsCode(weightUQ) && Constants.Weight.ContainsCode(packageWeightUQ))
					{
						var itemWeight = Constants.Weight.Convert(qty * PackableItemParent.WeightPerUnit, weightUQ, packageWeightUQ);
						package.KP_Weight = Math.Max(0, package.KP_Weight + itemWeight);
					}
				}
			}
		}

		#endregion

		#region MovePackedItem

		public void MovePackedItem(PkgPackage newParent)
		{
			ThrowExceptionIfDeleted();

			Argument.NotNull(newParent, nameof(newParent));

			var packagePK = PackagePK;
			if (packagePK != newParent.PK)
			{
				var parentPackage = Factory.Load<PkgPackage>(packagePK);
				var currentPackageJob = parentPackage?.KP_KJ_ParentPackageJob ?? ZGuid.Empty;
				if (!currentPackageJob.IsEmpty && newParent.KP_KJ_ParentPackageJob != currentPackageJob)
				{
					throw new InvalidOperationException("You can only move a Divot to another Package on the same PackageJob.");
				}

				using (parentPackage.SuspendPackedItemDivotsCountChanged()) // manually moving the packed item
				using (newParent.SuspendPackedItemDivotsCountChanged())
				{
					MoveDivots(newParent);
				}
			}
		}

		void MoveDivots(PkgPackage dstParentPackage)
		{
			ThrowExceptionIfDeleted();

			var dstPackedItemToMergeWith = dstParentPackage.PackedItems.Typed.FirstOrDefault(d => d.Key == Key);
			if (dstPackedItemToMergeWith != null)
			{
				MergeWrapperToDestination(dstParentPackage, dstPackedItemToMergeWith);
			}
			else
			{
				MoveWrapperToDestination(dstParentPackage);
			}
		}

		void MergeWrapperToDestination(PkgPackage dstParentPackage, PkgPackageItemDivotsWrapper dstPackedItemToMergeWith)
		{
			ThrowExceptionIfDeleted();

			var srcPackedItem = this;
			var nonDeletedDivots = srcPackedItem.GetNonDeletedDivots().ToArray();
			var movedQty = GetPackedQty(nonDeletedDivots);
			if (movedQty > 0) // reducing moved quantity only
			{
				AddWeightToParentPackage(-movedQty);
			}

			srcPackedItem.ParentPackage.PackedItems.RemoveFromRelationship(srcPackedItem);
			SetPackageToDivots(dstParentPackage.PK, nonDeletedDivots);

			foreach (var divot in nonDeletedDivots)
			{
				UnhookDivotEvents(divot);
				dstPackedItemToMergeWith.AddDivotToWrapper(divot, Key);
			}

			if (movedQty > 0) // after moved adding total quantity
			{
				AddWeightToParentPackage(movedQty);
			}

			dstPackedItemToMergeWith.PackedQty_QuantityChanged(this, EventArgs.Empty); // re-paint node if necessary

			srcPackedItem.Divots.Clear(); // do this instead of delete, as delete would kill divots that were moved to another wrapper (tree node)
			srcPackedItem.Delete();
		}

		void MoveWrapperToDestination(PkgPackage dstParentPackage)
		{
			ThrowExceptionIfDeleted();

			var srcPackedItem = this;
			var nonDeletedDivots = GetNonDeletedDivots().ToArray();
			var movedQty = GetPackedQty(nonDeletedDivots);

			// remove item and weight from source package
			if (movedQty > 0)
			{
				AddWeightToParentPackage(-movedQty);
			}

			srcPackedItem.ParentPackage.PackedItems.RemoveFromRelationship(srcPackedItem);

			// change divots to destination
			SetPackageToDivots(dstParentPackage.PK, nonDeletedDivots);

			// add item + weight to existing
			dstParentPackage.PackedItems.Add(srcPackedItem);
			if (movedQty > 0) // after moving to new Package, re-add quantities
			{
				AddWeightToParentPackage(movedQty);
			}
		}

		#endregion

		#region SetPackageToDivots

		static void SetPackageToDivots(ZGuid packagePK, PkgPackageItemDivot[] nonDeletedDivots)
		{
			foreach (var divot in nonDeletedDivots)
			{
				using (divot.GetValidationSuspender())
				{
					divot.KI_KP_Package = packagePK;
				}
			}
		}

		#endregion

		#region SetDivotPackageEmpty

		void SetDivotPackageEmpty(PkgPackageItemDivot packageDivot)
		{
			var packedQty = packageDivot.KI_PackedQty;
			if (packedQty > 0)
			{
				AddWeightToParentPackage(-packedQty);
			}

			using (packageDivot.GetValidationSuspender())
			{
				packageDivot.KI_KP_Package = ZGuid.Empty;
			}
		}

		#endregion

		#region SetPackagePKToEmpty

		void SetPackagePKToEmpty(PkgPackageItemDivot[] nonDeletedDivots)
		{
			var packedQty = GetPackedQty(nonDeletedDivots);
			if (packedQty > 0)
			{
				AddWeightToParentPackage(-packedQty);
			}

			SetPackageToDivots(ZGuid.Empty, nonDeletedDivots);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			try
			{
				IsDeletingWrapper = true;
				DeleteCore(null);
			}
			finally
			{
				IsDeletingWrapper = false;
			}
		}

		void DeleteCore(PkgPackageItemDivot packageDivot)
		{
			if (packageDivot == null)
			{
				DeleteWrapper(); // delete wrapper and all divots
			}
			else
			{
				DeleteDivot(packageDivot);
			}

			if (IsDivotsEmpty) // if we are deleting one divot at a time and we have no divots left this wrapper should be deleted
			{
				Divots.Clear();

				base.Delete();
				HasChanges = true; // Fire HasChanges so that the Tree knows to Delete the Divot Node.
			}
		}

		bool IsDeletingWrapper;

		void DeleteDivot(PkgPackageItemDivot packageDivot)
		{
			UnhookDivotEvents(packageDivot);

			SetDivotPackageEmpty(packageDivot);
			Divots.Remove(packageDivot);
		}

		void DeleteWrapper()
		{
			if (IsDivotsEmpty)
			{
				// if the Wrapper has no Divots at the time of Deleting, we need to manually remove the Wrapper from its Parent Collection
				ParentCollections.OfType<IPackedItemsCollection>().SingleOrDefault()?.RemoveFromRelationship(this);
			}
			else
			{
				var nonDeletedDivots = GetNonDeletedDivots().ToArray();
				nonDeletedDivots.ForEach(d => UnhookDivotEvents(d));

				SetPackagePKToEmpty(nonDeletedDivots);
				nonDeletedDivots.ForEach(d => d.Delete());
			}
		}

		public void Delete_ThrowAwayWrapperButDontDeleteDivots()
		{
			if (!IsDeletingWrapper)
			{
				GetNonDeletedDivots().ForEach(d => UnhookDivotEvents(d));
				Divots.Clear();

				base.Delete();
			}
		}

		public void ThrowExceptionIfDeleted()
		{
			if (IsDeleted)
			{
				throw new InvalidOperationException("This PkgPackageItemDivotsWrapper is deleted.");
			}
		}

		#endregion

		IEnumerable<PkgPackageItemDivot> GetNonDeletedDivots() => Divots.Where(d => !d.IsDeleted);

		#region Interfaces

		#region ICustomPropertyContainer members

		IEnumerable<ICustomProperty> ICustomPropertyContainer.CustomProperties
		{
			get { return customProperties ?? (customProperties = PkgPackageItemDivotCustomPropertiesHelper.GetCustomProperties<PkgPackageItemDivotsWrapper>(PackableItemParent)); }
		}

		IEnumerable<ICustomProperty> customProperties;

		#endregion

		#endregion
	}
}
