using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAllowedDepartmentsExcludeSelected()
		{
			var lookups = new GlbBranchLookups(GlbBranch.CurrentBranch);
			var allDepartments = new GlbDepartmentCollection(Factory);

			AssertEquals(allDepartments.Count, lookups.AllowedDepartmentsExcludeSelected.Count);
			AssertEquals(true, lookups.AllowedDepartmentsExcludeSelected.Any(department => department.PK == GlbDepartment.CurrentDepartment.PK));

			var allowedDepartment = GlbBranch.CurrentBranch.AllowedDepartments.AddNew();
			allowedDepartment.AAB_GE_Department = GlbDepartment.CurrentDepartment.PK;

			AssertEquals(allDepartments.Count - 1, lookups.AllowedDepartmentsExcludeSelected.Count);
			AssertEquals(false, lookups.AllowedDepartmentsExcludeSelected.Any(department => department.PK == GlbDepartment.CurrentDepartment.PK));

			GlbBranch.CurrentBranch.AllowedDepartments.DeleteAll();
			AssertEquals(allDepartments.Count, lookups.AllowedDepartmentsExcludeSelected.Count);
			AssertEquals(true, lookups.AllowedDepartmentsExcludeSelected.Any(department => department.PK == GlbDepartment.CurrentDepartment.PK));

			var errors = new StringCollectionX();
			lookups.AllowedDepartmentsExcludeSelected.AddNotificationWhenAdditionalFilterNotMetOverride(errors, GlbDepartment.CurrentDepartment);
			AssertEquals("This department has already been attached.", errors.ToString());
		}

		public void TestAccountingGroupCodes()
		{
			var newCodeCollection = new BranchManagementCodeDescriptionBoolCollection();
			var code1 = newCodeCollection.Add("BRA", null, true);
			var code2 = newCodeCollection.Add("BRB", null, false);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newCodeCollection);

			var lookups = new GlbBranchLookups(GlbBranch.CurrentBranch);
			var accountingGroupCodeList = lookups.AccountingGroupCodes;
			AssertCollectionContains("Should contain code1", code1, accountingGroupCodeList);
			AssertCollectionNotContains("Should not contain code2", code2, accountingGroupCodeList);
		}
	}
}
