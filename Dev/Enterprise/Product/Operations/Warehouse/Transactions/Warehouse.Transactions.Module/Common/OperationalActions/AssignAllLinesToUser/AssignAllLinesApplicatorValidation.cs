using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AssignAllLinesApplicatorValidation<T> : ZValidation where T : BusinessObject, IMasterStaffAssigner
	{
		public AssignAllLinesApplicatorValidation(BusinessObject parent)
			: base(parent)
		{
			this.ZValidationInternals = this;
		}

		#region AutoValidationType

		public override Type AutoValidationType => typeof(AssignAllLinesApplicatorValidation<T>);

		#endregion

		#region CheckSelectedUser

		protected virtual void CheckSelectedUser()
		{
			MandatoryValidation.CheckEntered(Parent.SelectedUserInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SelectedUserInfo, Parent.Lookups.UsersToSelect);
			ListValidation.ErrorIfCancelledAndEditable(Parent.SelectedUserInfo);
		}

		#endregion

		#region ValidateSelectedUser

		public void ValidateSelectedUser()
			=> ZValidationInternals.Validate(Parent.SelectedUserInfo, new RunValidationInvoker(() => CheckSelectedUser()));

		#endregion

		#region Validate All

		public override void ValidateAll() => ValidateSelectedUser();

		#endregion

		protected LinesAssignerActionMethodApplicator<T> Parent => (LinesAssignerActionMethodApplicator<T>)base.ParentFilter;

		readonly IValidationInternals ZValidationInternals;
	}
}
