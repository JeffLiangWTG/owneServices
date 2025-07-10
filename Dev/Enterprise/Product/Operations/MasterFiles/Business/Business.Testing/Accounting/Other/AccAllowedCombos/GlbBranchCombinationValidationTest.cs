using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class GlbBranchCombinationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBranchDepartmentCombination()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { header.AH_GB = branch; header.AH_GE = department; }, header.AH_GEInfo);
		}

		public static void SetAllowedBranchDepartmentCombinations(GlbBranch branch, GlbDepartment[] departments)
		{
			branch.AllowedDepartments.DeleteAll();
			AssertEquals(branch.AllowedDepartments.Count, 0);

			foreach (var department in departments)
			{
				var allowedDepartment = branch.AllowedDepartments.AddNew();
				allowedDepartment.AAB_GE_Department = department.PK;
			}

			AssertEquals(branch.AllowedDepartments.Count, departments.Length);

			foreach (var department in departments)
			{
				AssertEquals(true, branch.IsDepartmentAllowed(department.PK));
			}
		}

		public static void ValidateBranchDepartmentCombinationsForBizObj(BusinessObjectFactory factory, Action<ZGuid, ZGuid> branchDepartmentSetter, ZPropertyInfo departmentInfo)
		{
			var aaaBranch = factory.NewWithValidTestData<GlbBranch>();
			var bbbBranch = factory.NewWithValidTestData<GlbBranch>();
			var aaaDepartment = factory.NewWithValidTestData<GlbDepartment>();
			var bbbDepartment = factory.NewWithValidTestData<GlbDepartment>();
			aaaBranch.GB_GC = Env.CurrentCompany.PK;
			bbbBranch.GB_GC = Env.CurrentCompany.PK;
			factory.Save();

			var setter = branchDepartmentSetter;

			//empty
			aaaBranch.AllowedDepartments.DeleteAll();
			bbbBranch.AllowedDepartments.DeleteAll();

			setter(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertNoErrors("shouldn't have error", departmentInfo);

			var currentBranch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);

			//configured department
			SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { GlbDepartment.CurrentDepartment });

			setter(Guid.Empty, Guid.Empty);
			setter(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertNoErrors("shouldn't have error", departmentInfo);

			//unconfigured department
			setter(Guid.Empty, Guid.Empty);
			setter(Env.CurrentBranch.PK, aaaDepartment.PK);
			AssertHasError(departmentInfo, string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, aaaDepartment.GE_Code, Env.CurrentBranch.Code));

			//multi-department
			SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { aaaDepartment, GlbDepartment.CurrentDepartment });

			setter(Guid.Empty, Guid.Empty);
			setter(Env.CurrentBranch.PK, aaaDepartment.PK);
			AssertNoErrors("shouldn't have error", departmentInfo);

			setter(Guid.Empty, Guid.Empty);
			setter(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertNoErrors("shouldn't have error", departmentInfo);

			setter(Guid.Empty, Guid.Empty);
			setter(Env.CurrentBranch.PK, bbbDepartment.PK);
			AssertHasError(departmentInfo, string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, bbbDepartment.GE_Code, Env.CurrentBranch.Code));

			//change branch to re-validate department
			SetAllowedBranchDepartmentCombinations(bbbBranch, new GlbDepartment[] { bbbDepartment });
			setter(Guid.Empty, Guid.Empty);
			setter(bbbBranch.PK, aaaDepartment.PK);
			AssertHasError(departmentInfo, string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, aaaDepartment.GE_Code, bbbBranch.GB_Code));

			SetAllowedBranchDepartmentCombinations(aaaBranch, new GlbDepartment[] { aaaDepartment });
			setter(aaaBranch.PK, aaaDepartment.PK); //change branch only
			AssertNoErrors("shouldn't have error", departmentInfo);
		}

		public static void ValidateBranchDepartmentCombinationsForBizObj(BusinessObjectFactory factory, Action<ZGuid> branchSetter, GlbDepartment fixedDepartment, ZPropertyInfo branchPropertyInfo)
		{
			var currentBranch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var aaaBranch = factory.NewWithValidTestData<GlbBranch>();
			var bbbBranch = factory.NewWithValidTestData<GlbBranch>();
			var aaaDepartment = factory.NewWithValidTestData<GlbDepartment>();
			var bbbDepartment = factory.NewWithValidTestData<GlbDepartment>();
			aaaBranch.GB_GC = Env.CurrentCompany.PK;
			bbbBranch.GB_GC = Env.CurrentCompany.PK;
			factory.Save();

			var setter = branchSetter;

			//empty 
			currentBranch.AllowedDepartments.DeleteAll();

			setter(Env.CurrentBranch.PK);
			AssertNoErrors("shouldn't have error", branchPropertyInfo);

			//configured department
			SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { fixedDepartment });

			setter(Guid.Empty);
			setter(Env.CurrentBranch.PK);
			AssertNoErrors("shouldn't have error", branchPropertyInfo);

			//unconfigured department
			SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { aaaDepartment });
			setter(Guid.Empty);
			setter(Env.CurrentBranch.PK);
			AssertHasError(branchPropertyInfo, string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, fixedDepartment.GE_Code, Env.CurrentBranch.Code));

			//ALL department
			currentBranch.AllowedDepartments.DeleteAll();

			setter(Guid.Empty);
			setter(Env.CurrentBranch.PK);
			AssertNoErrors("shouldn't have error", branchPropertyInfo);

			//multi-department
			SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { aaaDepartment, fixedDepartment });

			setter(Guid.Empty);
			setter(Env.CurrentBranch.PK);
			AssertNoErrors("shouldn't have error", branchPropertyInfo);
		}

		public static void ValidateBranchDepartmentCombinationsForBizObjInDatabase(BusinessObjectFactory factory, BusinessObject bizObj, Action validator, ZPropertyInfo departmentInfo)
		{
			ValidateBranchDepartmentCombinationsForBizObjInDatabase(factory, bizObj, validator, departmentInfo, CargoWise.EntityFramework.NotificationType.Error);
		}

		public static void ValidateBranchDepartmentCombinationsForBizObjInDatabase(BusinessObjectFactory factory, BusinessObject bizObj, Action validator, ZPropertyInfo departmentInfo, INotificationType notificationType)
		{
			var bbbDepartment = factory.NewWithValidTestData<GlbDepartment>();
			factory.Save();

			var isErrorType = notificationType == CargoWise.EntityFramework.NotificationType.Error;
			var currentBranch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);

			currentBranch.AllowedDepartments.DeleteAll();

			if (isErrorType)
			{
				AssertNoErrors("precondition", departmentInfo);
			}
			else
			{
				AssertNoWarnings("precondition", departmentInfo);
			}

			AssertEquals("precondition", bizObj.IsInDatabase, true);
			AssertEquals("precondition", departmentInfo.HasChanges, false);

			validator();
			if (isErrorType)
			{
				AssertNoErrors("shouldn't have error", departmentInfo);
			}
			else
			{
				AssertNoWarnings("shouldn't have warning", departmentInfo);
			}

			SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { bbbDepartment });
			validator();

			var expectedMsg = string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, Env.CurrentDepartment.Code, Env.CurrentBranch.Code);

			if (isErrorType)
			{
				AssertHasError(departmentInfo, expectedMsg);
			}
			else
			{
				AssertHasWarning(departmentInfo, expectedMsg);
			}

			currentBranch.AllowedDepartments.DeleteAll();
			validator();
			AssertEquals("precondition", bizObj.IsInDatabase, true);
			AssertEquals("precondition", departmentInfo.HasChanges, false);

			if (isErrorType)
			{
				AssertNoErrors("shouldn't have error", departmentInfo);
			}
			else
			{
				AssertNoWarnings("shouldn't have warning", departmentInfo);
			}
		}

		public static void ValidateBranchDepartmentCombinationsErrorMessage(BusinessObjectFactory factory, Func<string> validator)
		{
			var bbbDepartment = factory.NewWithValidTestData<GlbDepartment>();
			factory.Save();
			var currentBranch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);

			try
			{
				SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { bbbDepartment });
				var errorMsg = validator();

				var match = Regex.Match(errorMsg, @"The department \w\w\w cannot be used with the branch \w\w\w.", RegexOptions.IgnoreCase);
				Assert("Should have combination validation error", match.Success && match.Length > 0);
			}
			finally
			{
				currentBranch.AllowedDepartments.DeleteAll();
			}
		}
	}
}
