using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class UnassignAllLinesApplicatorValidation<T> : AssignAllLinesApplicatorValidation<T>
		where T : BusinessObject, IMasterStaffAssigner
	{
		public UnassignAllLinesApplicatorValidation(BusinessObject parent)
			: base(parent)
		{ }

		#region CheckSelectedUser

		protected override void CheckSelectedUser()
		{
			ListValidation.ErrorIfInvalidCode(Parent.SelectedUserInfo, Parent.Lookups.UsersToSelect);
			ListValidation.ErrorIfCancelledAndEditable(Parent.SelectedUserInfo);
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType => typeof(UnassignAllLinesApplicatorValidation<T>);

		#endregion
	}
}
