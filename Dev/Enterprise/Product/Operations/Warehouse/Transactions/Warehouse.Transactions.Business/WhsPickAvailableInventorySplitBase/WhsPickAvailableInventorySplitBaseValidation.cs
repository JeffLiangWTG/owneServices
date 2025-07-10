using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickAvailableInventorySplitBaseValidation : ZValidation
	{
		protected WhsPickAvailableInventorySplitBaseValidation(WhsPickAvailableInventorySplitBase parent)
			: base(parent)
		{
		}

		WhsPickAvailableInventorySplitBase Parent => (WhsPickAvailableInventorySplitBase)ParentFilter;

		public override void ValidateAll()
		{
			ValidatePickedDate();
			ValidateAssignedToPK();
		}

		#region ValidatePickedDate

		public void ValidatePickedDate()
		{
			ValidateCalculatedProperty(Parent.PickedDateInfo);
		}

		protected void CheckPickedDate()
		{
			ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(Parent.PickedDateInfo, Parent.OrderedInventory, Parent.Product, Parent.Client, Parent.PickLinesOnOrder);
			if (!Parent.PickedDate.IsEmpty)
			{
				bool isCustomsHoldEventApplied = Parent.PickLinesOnOrder.Select(pl => pl.DocketLine.Docket).All(o => o.IsFinaliseAllowed);
				if (!isCustomsHoldEventApplied)
				{
					Parent.PickedDateInfo.AddError(Res.GetString("19e00d71-93b1-4fdc-a3ab-6e9b405ed4ad", "Cannot pick Customs orders on Hold, awaiting Customs response."));
				}
			}
		}

		#endregion

		#region ValidateAssignedToPK

		public void ValidateAssignedToPK()
		{
			ValidateCalculatedProperty(Parent.AssignedToPKInfo);
		}

		protected void CheckAssignedToPK()
		{
			if (!Parent.PickedDate.IsEmpty && !Parent.AssignedToPK.IsValid)
			{
				Parent.AssignedToPKInfo.AddError(Res.GetString("ee92166d-2786-4439-9dd3-1c91f5d6c9a7", "Lines that have been picked must have a Picker."));
			}

			var pickLinesWithPickingDetails = Parent.PickLinesForPickingDetails.ToArray();
			if (pickLinesWithPickingDetails.Any(p => p.WZ_IsPicking
				&& p.WZ_GS_NKAssignedTo != p.WZ_GS_NKAssignedToInfo.OriginalValue.ToString()))
			{
				Parent.AssignedToPKInfo.AddError(Res.GetString("bfb9c9d8-0def-4023-951b-de955577232f", "Lines being Picked cannot be reassigned."));
			}
		}

		#endregion
	}
}
