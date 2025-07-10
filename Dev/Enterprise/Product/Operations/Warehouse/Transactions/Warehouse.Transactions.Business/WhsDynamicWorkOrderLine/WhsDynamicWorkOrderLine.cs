using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsDynamicWorkOrder), nameof(WhsDynamicWorkOrder.Lines))]
	public class WhsDynamicWorkOrderLine : WhsComponentOrderLine
	{
		public WhsDynamicWorkOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region DocketType

		public override Type DocketType => typeof(WhsDynamicWorkOrder);

		protected override string DocketLineType => CodeLists.DocketType.Codes.DynamicWorkOrder;

		#endregion

		#region DynamicWorkOrder

		public WhsDynamicWorkOrder DynamicWorkOrder => (WhsDynamicWorkOrder)base.Docket;

		#endregion

		#region ParentLine

		public new WhsDynamicWorkOrderLine ParentLine => (WhsDynamicWorkOrderLine)base.ParentLine;

		#endregion

		#region ProductFieldType

		public ZString ProductFieldType => (ParentLine?.IsSecondaryInwardProcessedItem ?? false) ? nameof(FieldType.GuidDropEdit) : nameof(FieldType.Guid);

		#endregion

		#region ChildComponentLines

		protected override IReadOnlyCollection<WhsPickableDocketLine> ChildComponentLinesCore => ChildComponentLinesCollection.ToArray<WhsPickableDocketLine>();

		[ChildEditable(true)]
		public WhsDynamicWorkOrderComponentLineCollection ChildComponentLinesCollection
		{
			get
			{
				if (childComponentLines == null)
				{
					childComponentLines = new WhsDynamicWorkOrderComponentLineCollection(this);
					RegisterEditableChildObject(childComponentLines);
				}

				return childComponentLines;
			}
		}

		WhsDynamicWorkOrderComponentLineCollection childComponentLines;

		#endregion

		#region IsMainInwardProcessedItem

		[ReadOnlyMember(nameof(IsDisassemblyOrHasComponentLinesOrFinalizedOrCancelled))]
		[ResourceStringData("WhsDynamicWorkOrderLine|IsMainInwardProcessedItem", Caption = "Is Main Inward Processed Item", MediumCaption = "Is Main IP Item", ShortCaption = "Main")]
		public ZBool IsMainInwardProcessedItem
		{
			get
			{
				var customsData = CustomsData;
				return !customsData.IsDeleted && customsData.WB_IsMainInwardsProcessedItem;
			}
			set
			{
				CustomsData.WB_IsMainInwardsProcessedItem = value;
				((IBusinessObjectState)ChildComponentLinesCollection).RefreshBindingIncludingChildren();
			}
		}

		public ZPropertyInfo IsMainInwardProcessedItemInfo => GetWrappedZPropertyInfo(nameof(IsMainInwardProcessedItem), x => CustomsData.WB_IsMainInwardsProcessedItemInfo);

		bool IsDisassemblyOrHasComponentLinesOrFinalizedOrCancelled
			=> (!DynamicWorkOrder?.IsAssembly ?? false)
			|| IsFinalised
			|| WE_DocketLineStatus.EqualsIgnoringCase(DocketLineStatus.Codes.Cancelled)
			|| ChildComponentLinesCollection.Count > 0;

		#endregion

		#region IsSecondaryInwardProcessedItem

		[ReadOnlyMember(nameof(IsDisassemblyOrHasComponentLinesOrFinalizedOrCancelled))]
		[ResourceStringData("WhsDynamicWorkOrderLine|IsSecondaryInwardProcessedItem", Caption = "Is Secondary Inward Processed Item", MediumCaption = "Is Secondary IP Item", ShortCaption = "Secondary")]
		public ZBool IsSecondaryInwardProcessedItem
		{
			get
			{
				var customsData = CustomsData;
				return !customsData.IsDeleted && customsData.WB_IsSecondaryInwardsProcessedItem;
			}
			set
			{
				CustomsData.WB_IsSecondaryInwardsProcessedItem = value;
				((IBusinessObjectState)ChildComponentLinesCollection).RefreshBindingIncludingChildren();
			}
		}

		public ZPropertyInfo IsSecondaryInwardProcessedItemInfo => GetWrappedZPropertyInfo(nameof(IsSecondaryInwardProcessedItem), x => CustomsData.WB_IsSecondaryInwardsProcessedItemInfo);

		#endregion

		protected override void SetDefaultValueWhsBondedWarehouseAttribute(WhsBondedWarehouseAttribute bondedWarehouseAttribute)
		{
			base.SetDefaultValueWhsBondedWarehouseAttribute(bondedWarehouseAttribute);

			if (!DynamicWorkOrder?.IsAssembly ?? false)
			{
				bondedWarehouseAttribute.WB_IsMainInwardsProcessedItem = true;
				bondedWarehouseAttribute.WB_IsSecondaryInwardsProcessedItem = false;
			}
		}

		#region Properties

		public override ZGuid WE_OP
		{
			get => base.WE_OP;
			set
			{
				base.WE_OP = value;

				if (!IsValidationSuspended)
				{
					ParentLine?
						.ChildComponentLinesCollection
						.ForEach(l => l.Validation.ValidateWE_OP());
				}
			}
		}

		protected override bool CanOrderPalletID => false;

		#endregion

		#region Picking

		protected override ZDecimal SumOfUnitsMetCore
		{
			get
			{
				ZDecimal result;

				if (DynamicWorkOrder.IsAssembly && WE_WE_ParentDocketLine.IsEmpty)
				{
					var childComponentLines = ChildComponentLinesCollection;
					result = childComponentLines.Count > 0 && childComponentLines.All(c => c.QuantityNotMet == 0m) ? WE_TransactionQuantity : ZDecimal.Zero;
				}
				else
				{
					result = base.SumOfUnitsMetCore;
				}

				return result;
			}
		}

		protected override bool CanGenerateChildWorkOrderCore => false;

		protected override bool ReduceOverpickedStockAllowed => false;

		#endregion

		#region WE_WD

		public override ZGuid WE_WD
		{
			get => base.WE_WD;
			set
			{
				var valueChanged = base.WE_WD != value;

				base.WE_WD = value;
				if (valueChanged)
				{
					ChildComponentLinesCollection.ForEach(l => l.WE_WD = value);
				}
			}
		}

		#endregion

		#region Shortfall

		protected override decimal CalculateShortfallForPickedOrder() => 0m;

		protected override decimal CalculateShortfallForUnpickedOrderCore() => 0m;

		#endregion

		#region Lookups

		protected override WhsDocketLineLookups GetNewLookups() => new WhsDynamicWorkOrderLineLookups(this);

		#endregion

		#region AllowToDefaultPriceInfoFromProductCore

		protected override bool AllowToDefaultPriceInfoFromProductCore => false;

		#endregion

		#region SupportsHasProductUnitsOrAttribsChanged

		protected override bool SupportsHasProductUnitsOrAttribsChanged => false;

		#endregion

		#region ShouldUpdateWeightAndVolumeOfDocketFromDocketLine

		protected override bool ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(WhsDocket docket)
			=> docket != null
				&& (!((WhsDynamicWorkOrder)docket).IsAssembly || IsComponentForAMainInwardsProcessedItem);

		bool IsComponentForAMainInwardsProcessedItem => ParentLine?.IsMainInwardProcessedItem ?? false;

		#endregion

		#region Validation

		protected override WhsPickableDocketLineValidation GetNewPickableDocketLineValidation() => new WhsDynamicWorkOrderLineValidation(this);

		#endregion

		#region ReadOnly

		protected override bool CommonPartAttribReadOnly => base.CommonPartAttribReadOnly || (ParentLine?.IsSecondaryInwardProcessedItem ?? false);

		protected override bool BondedEntryKeyReadOnly => base.BondedEntryKeyReadOnly || WE_WE_ParentDocketLine.IsEmpty || !IsComponentForAMainInwardsProcessedItem;

		#endregion

		#region Delete

		public override void Delete()
		{
			foreach (var childComponentLine in ChildComponentLinesCollection.ToArray())
			{
				childComponentLine.WE_TransactionQuantity = 0; // need for recalculating weights					
				childComponentLine.Delete();
			}

			base.Delete();
		}

		#endregion
	}
}
