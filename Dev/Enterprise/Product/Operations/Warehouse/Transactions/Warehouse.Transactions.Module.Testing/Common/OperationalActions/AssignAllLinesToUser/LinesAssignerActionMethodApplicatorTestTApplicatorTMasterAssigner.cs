using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class LinesAssignerActionMethodApplicatorTest<TApplicator, TMasterAssigner> : OperationalActionMethodApplicatorTest
		where TMasterAssigner : BusinessObject, IMasterStaffAssigner
		where TApplicator : LinesAssignerActionMethodApplicator<TMasterAssigner>
	{
		#region TestSelectedUser

		public void TestSelectedUser()
		{
			var user = Helper.CreateGlbStaff("T2", "Test1");
			Applicator.SelectedUser = user.GS_Code;
			AssertEquals("T2", Applicator.SelectedUser);
			AssertEquals(user, Applicator.VerifiedBy);
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			AssertEquals(typeof(AssignAllLinesApplicatorLookups), Applicator.Lookups.GetType());
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			AssertEquals(ExpectedApplicatorValidationType, Applicator.Validation.GetType());
		}

		protected abstract Type ExpectedApplicatorValidationType { get; }

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		protected GlbStaff SelectedUser => selectedUser ?? (selectedUser = Helper.CreateGlbStaff("T1", "T1"));
		GlbStaff selectedUser;

		protected override BusinessObject GetNewBusinessObject()
		{
			var applicator = GetNewApplicator();
			applicator.SelectedUser = SelectedUser.GS_Code;
			return applicator;
		}

		#endregion

		protected abstract TApplicator GetNewApplicator();

		new TApplicator Applicator => (TApplicator)base.Applicator;
	}
}
