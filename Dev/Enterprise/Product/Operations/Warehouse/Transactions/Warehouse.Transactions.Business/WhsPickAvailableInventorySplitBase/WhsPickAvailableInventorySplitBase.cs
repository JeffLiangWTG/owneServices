using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickAvailableInventorySplitBase : NonPersistentBusinessObject<WhsPickAvailableInventorySplitBaseValidation>
	{
		protected WhsPickAvailableInventorySplitBase(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string PickedDate = "PickedDate";
			public const string AssignedToPK = "AssignedToPK";
			public const string StockUnitQuantity = "StockUnitQuantity";
			public const string StockKeepingUnit = "StockKeepingUnit";
		}

		#endregion

		#region Related Entities

		#region PickLines

		/// <summary>
		/// This is only exposed for Validation to use and should not be public
		/// </summary>
		internal IEnumerable<WhsPickLine> PickLinesForPickingDetails => pickLinePairs?.Select(p => p.PickLineForPickingDetails) ?? Enumerable.Empty<WhsPickLine>();

		/// <summary>
		/// This is only exposed for Validation to use and should not be public
		/// </summary>
		internal IEnumerable<WhsPickLine> PickLinesOnOrder => pickLinePairs?.Select(p => p.PickLineOnOrder) ?? Enumerable.Empty<WhsPickLine>();

		#endregion

		#region Client

		public OrgHeader Client => availableInventory?.Client;

		#endregion

		#region OrderedInventory

		public WhsPickOrderedInventory OrderedInventory => availableInventory?.OrderedInventory;

		#endregion

		#region Product

		public WhsProduct Product => availableInventory?.WhsProduct;

		#endregion

		#endregion

		#region Properties

		// read only

		#region StockUnitQuantity

		[ResourceStringData("WhsPickAvailableInventorySplitBase|StockUnitQuantity", Caption = "Qty")]
		public ZDecimal StockUnitQuantity { get; private set; }

		public ZPropertyInfo StockUnitQuantityInfo => GetZPropertyInfo(Schema.StockUnitQuantity);

		#endregion

		#region StockKeepingUnit

		[ResourceStringData("WhsPickAvailableInventorySplitBase|StockKeepingUnit", Caption = "UQ")]
		public ZString StockKeepingUnit { get; private set; }

		public ZPropertyInfo StockKeepingUnitInfo => GetZPropertyInfo(Schema.StockKeepingUnit);

		#endregion

		// editable

		#region PickedDate

		[ReadOnlyMember(nameof(ReadOnlyForUnfinalizedNonAvailableStock))]
		[BusinessObjectTestExclude]
		[ResourceStringData("WhsPickAvailableInventorySplitBase|PickedDate", Caption = "Picked Date")]
		public ZDateTimeOffset PickedDate
		{
			get
			{
				var result = ZDateTimeOffset.Empty;
				if (pickLinePairs != null && pickLinePairs.Length > 0)
				{
					result = pickLinePairs[0].PickedDateTime;
				}
				return result;
			}
			set
			{
				if (pickLinePairs != null)
				{
					var pick = OrderedInventory?.Pick;
					using (pick?.SuspendPickPercentageRecalculation())
					{
						foreach (var pickLine in pickLinePairs)
						{
							pickLine.PickedDateTime = value;
						}
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePickedDate();
					Validation.ValidateAssignedToPK();
				}

				PickedDateInfo.RefreshBinding();
				AssignedToPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PickedDateInfo => GetZPropertyInfo(Schema.PickedDate);

		#endregion

		#region AssignedToPK

		[ReadOnlyMember(nameof(ReadOnlyForUnfinalizedNonAvailableStock))]
		[List("Lookups.AssignedTos")]
		[ResourceStringData("WhsPickAvailableInventorySplitBase|AssignedToPK", Caption = "Picker")]
		public ZGuid AssignedToPK
		{
			get
			{
				var result = ZGuid.Empty;
				if (pickLinePairs != null && pickLinePairs.Length > 0)
				{
					var assignedTo = pickLinePairs[0].AssignedTo;
					if (assignedTo != null)
					{
						result = assignedTo.PK;
					}
				}
				return result;
			}
			set
			{
				if (pickLinePairs != null)
				{
					var assignedTo = Factory.Load<GlbStaff>(value);
					foreach (var pickLine in pickLinePairs)
					{
						pickLine.AssignedToCode = assignedTo?.GS_Code ?? ZString.Empty;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAssignedToPK();
				}

				AssignedToPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AssignedToPKInfo => GetZPropertyInfo(Schema.AssignedToPK);

		#endregion

		#region ReadOnly

		protected bool ReadOnlyForUnfinalizedNonAvailableStock
		{
			get { return (availableInventory?.ReadOnlyForUnfinalizedNonAvailableStock ?? true) || IsPartiallyOrFullyPickedFromPutawayLocation || (OrderedInventory?.Pick?.Warehouse?.WW_GG_ReleaseGroup.IsValid ?? false); }
		}

		bool IsPartiallyOrFullyPickedFromPutawayLocation => PickLinesForPickingDetails.Any(l => l.IsPickedFromPutawayLocation);

		#endregion

		#endregion

		#region LinkPickLinesToTask

		public void LinkPickLinesToTask(decimal quantity, ZGuid taskPK)
		{
			if (quantity <= 0m)
			{
				throw new ArgumentException("Cannot link a Negative or Zero amount.");
			}
			else if (quantity > StockUnitQuantity)
			{
				throw new ArgumentException("Cannot link more than the available quantity.");
			}

			if (pickLinePairs != null)
			{
				foreach (var pickLine in pickLinePairs.Select(pl => pl.PickLineForPickingDetails).Where(pl => pl.WZ_P9_Task.IsEmpty && !pl.IsPickedFromPutawayLocation))
				{
					if (pickLine.WZ_Units > quantity)
					{
						var pickLineSplit = pickLine.Split(quantity);
						pickLineSplit.WZ_P9_Task = taskPK;
						pickLinePairs = pickLinePairs.Append(PickLinePair.New(pickLineSplit)).ToArray();
						quantity = 0;
					}
					else
					{
						pickLine.WZ_P9_Task = taskPK;
						quantity -= pickLine.WZ_Units;
					}

					if (quantity <= 0m)
					{
						break;
					}
				}
			}

			if (quantity > 0m)
			{
				throw new ArgumentException("Not enough Pick Lines to link the requested quantity.");
			}
		}

		#endregion

		#region SetData

		public void SetData(IPickLinePair[] whsPickLinePairs, WhsPickAvailableInventory whsPickAvailableInventory)
		{
			if (availableInventory != null)
			{
				throw new InvalidOperationException("SetData on WhsPickAvailableInventorySplitBase should not be called twice");
			}

			pickLinePairs = Argument.NotNull(whsPickLinePairs, nameof(whsPickLinePairs));
			availableInventory = Argument.NotNull(whsPickAvailableInventory, nameof(whsPickAvailableInventory));

			StockUnitQuantity = whsPickLinePairs.Sum(x => x.PickLineOnOrder.WZ_Units);
			StockKeepingUnit = availableInventory.SupplierPart.OP_StockKeepingUnit;
		}

		protected IPickLinePair[] pickLinePairs;
		protected WhsPickAvailableInventory availableInventory;

		#endregion

		#region Lookups

		public WhsPickAvailableInventorySplitBaseLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected abstract WhsPickAvailableInventorySplitBaseLookups GetNewLookups();

		WhsPickAvailableInventorySplitBaseLookups fLookups;

		#endregion
	}
}
