using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickLineValidation : AutoWhsPickLineValidation
	{
		public WhsPickLineValidation(AutoWhsPickLine parent)
			: base(parent)
		{
		}

		protected new WhsPickLine Parent
		{
			get { return (WhsPickLine)base.Parent; }
		}

		// persistent

		#region CheckWZ_OriginalReservedQty

		protected override void CheckWZ_OriginalReservedQty()
		{
			base.CheckWZ_OriginalReservedQty();

			MandatoryValidation.CheckNotNegative(Parent.WZ_OriginalReservedQtyInfo);

			if (Parent.IsReserveLine && !Parent.IsPickedFromPutawayLocation)
			{
				MandatoryValidation.CheckNotZero(Parent.WZ_OriginalReservedQtyInfo);
			}
		}

		#endregion

		#region CheckWZ_Units

		protected override void CheckWZ_Units()
		{
			base.CheckWZ_Units();

			if (Parent.WZ_Units < 0m)
			{
				Parent.WZ_UnitsInfo.AddError(Res.GetString("2beffce4-ab42-4cd3-b8ec-5eb3b9347e3a", "Pick line Units must be greater than or equal to zero."));
			}
			else if (Parent.WZ_Units > 0m)
			{
				var docketLine = Parent.DocketLine;
				if (docketLine != null && Parent.WZ_Units > docketLine.WE_TransactionQuantity)
				{
					Parent.WZ_UnitsInfo.AddError(Res.GetString("82838f78-d9a9-4b3a-ab59-ef5c526e5b9b", "Pick line Units must not exceed Quantity Ordered on the related Order Line record"));
				}
			}
		}

		#endregion

		#region CheckWZ_WE_TransactionLine

		protected override void CheckWZ_WE_TransactionLine()
		{
			base.CheckWZ_WE_TransactionLine();
			MandatoryValidation.CheckEntered(Parent.WZ_WE_TransactionLineInfo);
		}

		#endregion

		#region CheckWZ_WE_InventoryLine

		protected override void CheckWZ_WE_InventoryLine()
		{
			base.CheckWZ_WE_InventoryLine();
			MandatoryValidation.CheckEntered(Parent.WZ_WE_InventoryLineInfo);
		}

		#endregion

		// calculated

		#region ValidateReservedQuantity

		public void ValidateReservedQuantity()
		{
			ValidateCalculatedProperty(Parent.ReservedQuantityInfo);
		}

		// This is only protected so that architecture can reflect out this method from the subclasses.
		protected void CheckReservedQuantity()
		{
			var pickLine = Parent;
			if (pickLine.WZ_WE_TransactionLine.IsValid
				&& pickLine.WZ_WE_InventoryLine.IsValid)
			{
				var pickableDocketLine = (WhsPickableDocketLine)pickLine.DocketLine;
				if (pickableDocketLine != null && pickableDocketLine.IsDocketUnpicked) // Validation is only necessary when un-picked
				{
					var inventory = pickLine.Inventory;
					if (inventory != null)
					{
						CheckReservedQtyIsValid(pickableDocketLine, inventory);
					}
				}
			}
		}

		#region CheckReservedQtyIsValid

		void CheckReservedQtyIsValid(WhsPickableDocketLine pickableDocketLine, WhsInventoryView inventory)
		{
			var pickLine = Parent;
			var reservedQty = pickLine.ReservedQuantity;
			if (pickableDocketLine.WE_CrossDockQuantity > pickableDocketLine.WE_TransactionQuantity)
			{
				Parent.ReservedQuantityInfo.AddError(Res.GetString("a530dbaa-097c-4833-9e92-32fa3826e191", "You have allocated more than the ordered quantity"));
			}
			else if (inventory.WI_AvailableForCrossDockQuantity < 0m)
			{
				Parent.ReservedQuantityInfo.AddError(Res.GetString("39427d87-89bb-4b53-9b42-53da9dbcbf39", "This quantity is not available"));
			}
			else if (reservedQty < 0m)
			{
				Parent.ReservedQuantityInfo.AddError(Res.GetString("b694f5b6-b5ab-4157-8789-a98635a54b6a", "Please enter a quantity greater than zero"));
			}
			else if (reservedQty == 0m)
			{
				if (Parent.IsInDatabase)
				{
					Parent.ReservedQuantityInfo.AddWarning(Res.GetString("4e6b07ae-47db-4e0f-8101-59aeab0bd529", "Attempting to Cross Dock zero units."));
				}
				else
				{
					Parent.ReservedQuantityInfo.AddError(Res.GetString("185ee64c-6769-46cd-846c-8d627d06ba4c", "An unsaved Cross Dock allocation cannot have zero units."));
				}
			}
			else
			{
				CheckReservedQuantityCore();
			}
		}

		protected virtual void CheckReservedQuantityCore()
		{
		}

		#endregion

		#endregion

		//

		#region ValidateAll

		public override void ValidateAll()
		{
			// Tested in WhsOrderTest.TestFinaliseDocket_DBHits_WithPackageAudit.
			// This prevents FetchForLoad() on Receive Lines adding GenAddOnColumn Fetch Hints.
			// Since we are never going to load the Original Hold Reason, we don't need these extra loads.
			using (CustomsValuesBusinessObjectStrategyHelper.SuspendCustomsValuesFetchHint(Parent.Factory, typeof(WhsReceiveLine)))
			{
				base.ValidateAll();

				ValidateReservedQuantity();
				ValidateReservedPickLineStillMatchesOrderLine();
			}
		}

		void ValidateReservedPickLineStillMatchesOrderLine()
		{
			var errorMessage = Res.GetString("1C655693-EB66-4EA9-910A-77C1574A31DB", "Cross Docked Inventory is either damaged or has a mismatch on Warehouse, Client, Product, Part Attribute(s), Expiry Date, Packing Date or Ordered Pallet ID.");
			Parent.RemoveRowError(errorMessage);

			if (Parent.IsReserveLine)
			{
				var inventory = Parent.Inventory;
				if (inventory != null)
				{
					var docketLine = Parent.DocketLine as WhsOrderLine;
					if (docketLine != null && docketLine.IsDocketUnpicked)
					{
						if (!docketLine.MatchesInventoryForCrossDocking(inventory))
						{
							Parent.AddRowError(errorMessage);
						}
					}
				}
			}
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(
				WhsPickLineSchema.Constants.WZ_WE_InventoryLine,
				WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine,
				WhsPickLineSchema.Constants.WZ_WE_OriginalPickedInventoryLine,
				WhsPickLineSchema.Constants.WZ_WE_TransactionLine,
				WhsPickLineSchema.Constants.WZ_F3_NKAllocatedPackType,
				WhsPickLineSchema.Constants.WZ_P9_Task);

		#endregion
	}
}
