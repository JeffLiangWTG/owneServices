using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class UpdateInventoryHeldCodeActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public UpdateInventoryHeldCodeActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("ee62134b-4233-457c-8fab-7d41e2b18c21", "Update Inventory Hold Code"), factory)
		{
		}

		#region HoldReason

		[MaxLength(WhsDocketLine.Schema.WE_CurrentHoldReasonMaxLength)]
		[ResourceStringData("UpdateInventoryHeldCodeActionMethodApplicator|HoldReason", Caption = "Hold Reason")]
		public ZString HoldReason
		{
			get => holdReason;
			set
			{
				SetNonPersistentPropertyValue(HoldReasonInfo, ref holdReason, value);
			}
		}

		ZString holdReason;

		public ZPropertyInfo HoldReasonInfo => GetZPropertyInfo(nameof(HoldReason));

		#endregion

		#region SelectedInventoryHeldCode

		[MaxLength(WhsDocketLine.Schema.WE_WHC_NKCurrentInventoryHeldCodeMaxLength)]
		[List("Lookups.InventoryHeldCodeCollection")]
		[RelatedBusinessObject("HeldCode")]
		public ZString SelectedInventoryHeldCode
		{
			get { return selectedInventoryHeldCode; }
			set
			{
				CheckMaximumLength(SelectedInventoryHeldCodeInfo, value);
				SetNonPersistentPropertyValue(SelectedInventoryHeldCodeInfo, ref selectedInventoryHeldCode, value, setValueOnlyIfDifferentToGetter: false);
				Validation.ValidateSelectedInventoryHeldCode();
			}
		}

		public ZPropertyInfo SelectedInventoryHeldCodeInfo => this.GetZPropertyInfo(nameof(SelectedInventoryHeldCode));

		public WhsInventoryHeldCode HeldCode => Factory.LoadFromNaturalKey<WhsInventoryHeldCode>(WhsInventoryHeldCodeSchema.WHC_Code, SelectedInventoryHeldCode);

		ZString selectedInventoryHeldCode;

		#endregion

		#region Lookups

		public UpdateInventoryHeldCodesApplicatorLookups Lookups => lookups ?? (lookups = new UpdateInventoryHeldCodesApplicatorLookups(this));
		UpdateInventoryHeldCodesApplicatorLookups lookups;

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Validation

		public UpdateInventoryHeldCodeValidation Validation => new UpdateInventoryHeldCodeValidation(this);

		#endregion

		#region Apply

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			AddFetchHints(targets.Cast<WhsInventoryView>());

			int totalNumberOfInventoriesUpdated = 0;
			int totalNumberOfInventoriesNotUpdated = 0;
			foreach (WhsInventoryView inventory in targets)
			{
				var quantityToChangeStatus = inventory.WI_AvailableToTransferQuantity;
				var docketLine = inventory.InDocketLine;
				docketLine.HeldCodeChangeQuantity = quantityToChangeStatus;
				docketLine.HeldCodeToChangeTo = SelectedInventoryHeldCode;
				docketLine.HoldReasonToChangeTo = HoldReason;

				if (inventory.IsDocketLineFinalised && IsValidHeldCode(inventory) && docketLine.ChangeInventoryHeldCode(true))
				{
					totalNumberOfInventoriesUpdated++;
				}
				else
				{
					totalNumberOfInventoriesNotUpdated++;
				}
			}

			if (totalNumberOfInventoriesNotUpdated > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("9472afa3-4116-40eb-917f-fec53c0abb85",
@"{0} Inventory was not updated with the new Hold Code.
This could be because the Receipt is not finalized, there is no available stock to be updated or new Hold Code is not valid for the client.", totalNumberOfInventoriesNotUpdated));
			}

			if (totalNumberOfInventoriesUpdated > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					Res.GetString("a4d07fb3 -1ae1-4d5a-a7c5-8de19bd13489", "{0} Inventory was updated with the new Hold Code.", totalNumberOfInventoriesUpdated));
			}
		}

		bool IsValidHeldCode(WhsInventoryView inventory)
		{
			var isEmpty = string.IsNullOrEmpty(SelectedInventoryHeldCode);
			var heldCode = isEmpty ? null : HeldCode;

			return isEmpty
				|| (heldCode != null && (heldCode.WHC_OH_Client.IsEmpty || heldCode.WHC_OH_Client == inventory.WI_OH_Client));
		}

		void AddFetchHints(IEnumerable<WhsInventoryView> inventories)
		{
			foreach (var inventory in inventories)
			{
				// inventory.LoadFetchHintForPickLines() triggers a factory load on DocketLine
				inventory.Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.WI_WE_InDocketLine);
			}

			foreach (var inventory in inventories)
			{
				inventory.LoadFetchHintsForPickLines();
				inventory.Factory.AddFetchHint(WhsDocketLineSchema.PK, inventory.WI_WE_InDocketLine);
				inventory.Factory.AddFetchHint(WhsDocketSchema.PK, inventory.WI_WD);
				inventory.Factory.AddFetchHint(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, inventory.WI_WE_InDocketLine);

				inventory.Factory.AddFetchHint(StmALogSchema.SL_Parent, inventory.WI_WD);
				inventory.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, inventory.WI_WD);
				inventory.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, inventory.WI_WE_InDocketLine);
			}
		}

		#endregion
	}
}
