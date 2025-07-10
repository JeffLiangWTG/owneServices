using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class UpdateInventoryHeldCodeValidation : ZValidation
	{
		public UpdateInventoryHeldCodeValidation(BusinessObject parent)
			: base(parent)
		{
			this.ZValidationInternals = this;
		}
		readonly IValidationInternals ZValidationInternals;

		#region AutoValidationType

		public override Type AutoValidationType => typeof(UpdateInventoryHeldCodeValidation);

		#endregion

		#region ValidateSelectedInventoryHeldCode

		public void ValidateSelectedInventoryHeldCode()
		{
			ZValidationInternals.Validate(Applicator.SelectedInventoryHeldCodeInfo, new RunValidationInvoker(() => CheckSelectedInventoryHeldCode()));
		}

		void CheckSelectedInventoryHeldCode()
		{
			var parent = Applicator;

			ListValidation.ErrorIfInvalidCode(parent.SelectedInventoryHeldCodeInfo, parent.Lookups.InventoryHeldCodeCollection);
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateSelectedInventoryHeldCode();
		}

		#endregion

		#region Parent

		UpdateInventoryHeldCodeActionMethodApplicator Applicator => (UpdateInventoryHeldCodeActionMethodApplicator)base.ParentFilter;

		#endregion
	}
}
