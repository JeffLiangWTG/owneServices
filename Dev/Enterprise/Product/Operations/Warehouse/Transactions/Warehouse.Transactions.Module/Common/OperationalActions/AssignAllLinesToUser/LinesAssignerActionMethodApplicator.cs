using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class LinesAssignerActionMethodApplicator<T> : OperationalActionMethodApplicator
		where T : BusinessObject, IMasterStaffAssigner
	{
		protected LinesAssignerActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{ }

		#region SelectedUser

		[List("Lookups.UsersToSelect")]
		[MaxLength(3)]
		public ZString SelectedUser
		{
			get => selectedUser;
			set
			{
				CheckMaximumLength(SelectedUserInfo, value);
				SetNonPersistentPropertyValue(SelectedUserInfo, ref selectedUser, value);
				Validation.ValidateSelectedUser();
			}
		}
		ZString selectedUser;

		public ZPropertyInfo SelectedUserInfo => GetZPropertyInfo(nameof(SelectedUser));

		public GlbStaff VerifiedBy => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, SelectedUser);

		#endregion

		#region Lookups

		public AssignAllLinesApplicatorLookups Lookups => lookups ?? (lookups = new AssignAllLinesApplicatorLookups(this));
		AssignAllLinesApplicatorLookups lookups;

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Validation

		public abstract AssignAllLinesApplicatorValidation<T> Validation { get; }

		#endregion

		#region AssignTargetsToUsers

		protected abstract void AssignTargetsToUsers(IOperationalActionSectionLog log, IEnumerable<T> targets);

		#endregion

		protected abstract string GetErrorMessage(T target);
		protected abstract string MessageHeader { get; }
		protected abstract ControllerID ControllerID { get; }
		protected abstract string GetJobNo(T target);
	}
}
