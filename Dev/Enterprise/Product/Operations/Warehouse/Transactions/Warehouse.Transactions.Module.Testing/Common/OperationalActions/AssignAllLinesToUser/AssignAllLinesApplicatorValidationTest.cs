using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class AssignAllLinesApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			AssertEquals(typeof(AssignAllLinesApplicatorValidation<DummyBusinessObject>), Applicator.Validation.AutoValidationType);
		}

		#endregion

		#region TestValidateSelectedUser

		public void TestValidateSelectedUser()
		{
			var user = Helper.CreateGlbStaff("T1", "Test1");
			var inactiveUser = Helper.CreateGlbStaff("IAU", "Test2", false);

			Applicator.SelectedUser = "";
			Applicator.Validation.ValidateSelectedUser();
			AssertHasError(Applicator.SelectedUserInfo, "Please enter a value.");

			Applicator.SelectedUser = "IC"; //Invalid code
			Applicator.Validation.ValidateSelectedUser();
			AssertHasError(Applicator.SelectedUserInfo, "Enter a valid selection.");

			Applicator.SelectedUser = user.GS_Code; //Valid code
			Applicator.Validation.ValidateSelectedUser();
			AssertNoErrors(Applicator.SelectedUserInfo);

			Applicator.SelectedUser = inactiveUser.GS_Code; // In active user
			Applicator.Validation.ValidateSelectedUser();
			AssertHasError(Applicator.SelectedUserInfo, "This selection is inactive - it may not be used.");
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var user = Helper.CreateGlbStaff("T1", "Test1");

			Applicator.SelectedUser = "";
			Applicator.Validation.ValidateAll();
			AssertHasErrors(Applicator.SelectedUserInfo);

			Applicator.SelectedUser = "IC"; //Invalid code
			Applicator.Validation.ValidateAll();
			AssertHasErrors(Applicator.SelectedUserInfo);

			Applicator.SelectedUser = user.GS_Code; //Valid code
			Applicator.Validation.ValidateAll();
			AssertNoErrors(Applicator.SelectedUserInfo);
		}

		#endregion

		#region Implementation

		DummyApplicatorActionMethodApplicator Applicator
		{
			get { return applicator ?? (applicator = new DummyApplicatorActionMethodApplicator(Factory)); }
		}
		DummyApplicatorActionMethodApplicator applicator;

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
