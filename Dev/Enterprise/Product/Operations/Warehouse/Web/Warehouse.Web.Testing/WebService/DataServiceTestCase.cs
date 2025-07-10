using System;
using CargoWise.Data;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class DataServiceTestCase : BaseServiceTestCase<DataService>
	{
		#region Test Cases

		#region TestGetAvailableDepartments

		public void TestGetAvailableDepartments()
		{
			var webService = GetNewWebService();
			var departments = webService.GetAvailableDepartments();
			var expectedDepartments = LoadAvailableDepartments();
			AssertEquals(departments.Count, expectedDepartments.Count);
			foreach (var department in departments)
			{
				var departmentWasLoaded = false;
				foreach (var expectedDepartment in expectedDepartments)
				{
					if (DepartmentsMatch(expectedDepartment, department))
					{
						departmentWasLoaded = true;
					}
				}
				Assert("Department is missing", departmentWasLoaded);
			}
		}

		#endregion

		#region TestGetAvailableBranches

		public void TestGetAvailableBranches()
		{
			var webService = GetNewWebService();
			var branches = webService.GetAvailableBranches();
			var expectedBranches = LoadAvailableBranches();
			AssertEquals(branches.Count, expectedBranches.Count);
			foreach (var branch in branches)
			{
				var branchWasLoaded = false;
				foreach (var expectedBranch in expectedBranches)
				{
					if (BranchesMatch(expectedBranch, branch))
					{
						branchWasLoaded = true;
					}
				}
				Assert("Branch is missing", branchWasLoaded);
			}
		}

		#endregion

		#region TestCW1Version

		public void TestCW1Version()
		{
			var webService = GetNewWebService();
			AssertEquals("CW1 Version number shoud match with the ReleaseInfo Version number", ReleaseInfo.Instance.VersionNumber.ToString(), webService.CW1Version());
		}

		#endregion

		#endregion

		#region Implementation

		#region DepartmentsMatch

		bool DepartmentsMatch(DepartmentInfo department1, DepartmentInfo department2)
		{
			if (department1.Code != department2.Code)
			{
				return false;
			}
			if (department1.Description != department2.Description)
			{
				return false;
			}

			return department1.PK == department2.PK;
		}

		#endregion

		#region BranchesMatch

		bool BranchesMatch(BranchInfo branch1, BranchInfo branch2)
		{
			if (branch1.Code != branch2.Code)
			{
				return false;
			}
			if (branch1.Name != branch2.Name)
			{
				return false;
			}

			return branch1.PK == branch2.PK;
		}

		#endregion

		#region LoadAvailableDepartments

		DepartmentInfoCollection LoadAvailableDepartments()
		{
			var result = new DepartmentInfoCollection();
			using (var reader = Db.Connection.Command("select GE_PK, GE_Code, GE_Desc from dbo.GlbDepartment where GE_IsActive = 1 order by GE_Desc").ExecuteReader())    // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				while (reader.Read())
				{
					var deparment = new DepartmentInfo();
					deparment.Code = (string)reader[GlbDepartmentSchema.GE_Code.Name];
					deparment.Description = (string)reader[GlbDepartmentSchema.GE_Desc.Name];
					deparment.PK = (Guid)reader[GlbDepartmentSchema.PK.Name];
					result.Add(deparment);
				}
			}

			return result;
		}

		#endregion

		#region LoadAvailableBranches

		BranchInfoCollection LoadAvailableBranches()
		{
			var result = new BranchInfoCollection();
			using (var reader = Db.Connection.Command("select GB_PK, GB_Code, GB_BranchName, GC_Name from dbo.GlbBranch join dbo.GlbCompany on GB_GC = GC_PK where GB_IsActive = 1 order by GB_BranchName").ExecuteReader())    // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				while (reader.Read())
				{
					var branch = new BranchInfo();
					branch.Code = (string)reader[GlbBranchSchema.GB_Code.Name];
					branch.Name = (string)reader[GlbBranchSchema.GB_BranchName.Name];
					branch.PK = (Guid)reader[GlbBranchSchema.PK.Name];
					result.Add(branch);
				}
			}

			return result;
		}

		#endregion

		#region GetNewWebService

		protected override DataService GetNewWebService()
		{
			return new DataService();
		}

		#endregion

		#endregion
	}
}
