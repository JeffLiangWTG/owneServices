using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffManagementTreeNodeTest : TestCaseWithFactory
	{
		[TestDate(2019, 05, 03)]
		public void TestManagerRole()
		{
			SetupManagerTestData();

			AssertEquals(2, treeModel.RootNodes.Count);

			var payrollManager2Node = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Payroll Manager");
			AssertNotNull("Payroll Manager", payrollManager2Node);

			var funManagerNode = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Fun Manager");
			AssertNotNull("Fun Manager", funManagerNode);
		}

		[TestDate(2019, 05, 03)]
		public void TestManagerChildren()
		{
			SetupManagerTestData();

			AssertEquals(2, treeModel.RootNodes.Count);

			var payrollManagerNode = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Payroll Manager");
			AssertNotNull("Payroll Manager", payrollManagerNode);
			AssertEquals(1, payrollManagerNode.ChildNodes.Count());

			var payrollManager2Node = payrollManagerNode.ChildNodes.FirstOrDefault();
			var payrollManager2BizObj = (GlbStaffManagementManagerWrapper)payrollManager2Node.BizObj;
			AssertEquals("Role", $"{PRM2FullName} ({payrollManager2.GS_Code})", payrollManager2BizObj.Role);
			AssertEquals("EffectiveDate", "03-May-19", payrollManager2BizObj.EffectiveDate);
			AssertEquals("JobTitle", PRM2JobTitle, payrollManager2BizObj.JobTitle);
			AssertEquals("Branch", BranchCode + " (" + BranchCountry + ")", payrollManager2BizObj.Branch);

			var funManagerNode = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Fun Manager");
			AssertNotNull("Fun Manager", funManagerNode);
			AssertEquals(2, funManagerNode.ChildNodes.Count());

			var funManager1Node = funManagerNode.ChildNodes.Single(x =>
				x.BizObj is GlbStaffManagementManagerWrapper &&
				((GlbStaffManagementManagerWrapper)x.BizObj).Role == $"{FNMFullName1} ({funManager1.GS_Code})");
			var funManager1BizObj = (GlbStaffManagementManagerWrapper)funManager1Node.BizObj;
			AssertEquals("EffectiveDate", "03-May-19", funManager1BizObj.EffectiveDate);
			AssertEquals("Branch", BranchCode + " (" + BranchCountry + ")", funManager1BizObj.Branch);

			var funManager2Node = funManagerNode.ChildNodes.Single(x =>
				x.BizObj is GlbStaffManagementManagerWrapper &&
				((GlbStaffManagementManagerWrapper)x.BizObj).Role == $"{FNMFullName2} ({funManager2.GS_Code})");
			var funManager2BizObj = (GlbStaffManagementManagerWrapper)funManager2Node.BizObj;
			AssertEquals("EffectiveDate", "03-May-19", funManager2BizObj.EffectiveDate);
			AssertEquals("Branch", BranchCode + " (" + BranchCountry + ")", funManager2BizObj.Branch);
		}

		[TestDate(2019, 05, 03)]
		public void TestDirectReportRole()
		{
			SetupDirectReportTestData();

			AssertEquals(2, treeModel.RootNodes.Count);

			var payrollManager2Node = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Payroll Manager");
			AssertNotNull("Payroll Manager", payrollManager2Node);

			var funManagerNode = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Fun Manager");
			AssertNotNull("Fun Manager", funManagerNode);
		}

		[TestDate(2019, 05, 03)]
		public void TestDirectReportChildren()
		{
			SetupDirectReportTestData();

			AssertEquals(2, treeModel.RootNodes.Count);

			var payrollManagerNode = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Payroll Manager");
			AssertNotNull("Payroll Manager", payrollManagerNode);
			AssertEquals(1, payrollManagerNode.ChildNodes.Count());

			var payrollManager2Node = payrollManagerNode.ChildNodes.FirstOrDefault();
			var payrollManager2BizObj = (GlbStaffManagementManagerWrapper)payrollManager2Node.BizObj;
			AssertEquals("Role - Money Man 2", $"{PRM2FullName} ({payrollDirectReport2.GS_Code})", payrollManager2BizObj.Role);

			var funManagerNode = (GlbStaffManagementTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is GlbStaffManagementRoleWrapper &&
				((GlbStaffManagementRoleWrapper)x.BizObj).Role == "Fun Manager");
			AssertNotNull("Fun Manager", funManagerNode);
			AssertEquals(2, funManagerNode.ChildNodes.Count());

			var funManager1Node = funManagerNode.ChildNodes.Single(x =>
				x.BizObj is GlbStaffManagementManagerWrapper &&
				((GlbStaffManagementManagerWrapper)x.BizObj).Role == $"{FNMFullName1} ({funDirectReport1.GS_Code})");
			AssertNotNull("Role - Fun Man 1", funManager1Node);

			var funManager2Node = funManagerNode.ChildNodes.Single(x =>
				x.BizObj is GlbStaffManagementManagerWrapper &&
				((GlbStaffManagementManagerWrapper)x.BizObj).Role == $"{FNMFullName2} ({funDirectReport2.GS_Code})");
			AssertNotNull("Role - Fun Man 2", funManager2Node);
		}

		#region Implementation

		const string FullName = "John Smith";
		const string PRM1FullName = "Money Man 1";
		const string PRM2FullName = "Money Man 2";
		const string PRM2JobTitle = "Banker";
		const string FNMFullName1 = "Fun Man 1";
		const string FNMFullName2 = "Fun Man 2";
		const string BranchCountry = "AU";
		const string BranchCode = "SYD";

		GlbStaff staff;
		GlbStaff payrollManager1;
		GlbStaff payrollManager2;
		GlbStaff funManager1;
		GlbStaff funManager2;
		GlbStaffManagementTreeModel treeModel;

		void SetupManagerTestData()
		{
			var roleCollection = new StaffReportingRoleCollection();
			roleCollection.Add("PRM", (NoResString)"Payroll Manager", true, false, true);
			roleCollection.Add("FNM", (NoResString)"Fun Manager", false, false, true);
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roleCollection);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = BranchCountry;
			branch.GB_Code = BranchCode;

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = FullName;

			payrollManager1 = Factory.NewWithValidTestData<GlbStaff>();
			payrollManager1.GS_FullName = PRM1FullName;
			payrollManager1.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, payrollManager1, "PRM", new ZDateTime(2019, 01, 20), ZDateTime.Now.AddDays(-1));

			payrollManager2 = Factory.NewWithValidTestData<GlbStaff>();
			payrollManager2.GS_FullName = PRM2FullName;
			payrollManager2.GS_Title = PRM2JobTitle;
			payrollManager2.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, payrollManager2, "PRM");

			funManager1 = Factory.NewWithValidTestData<GlbStaff>();
			funManager1.GS_FullName = FNMFullName1;
			funManager1.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, funManager1, "FNM", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			funManager2 = Factory.NewWithValidTestData<GlbStaff>();
			funManager2.GS_FullName = FNMFullName2;
			funManager2.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, funManager2, "FNM");

			treeModel = new GlbStaffManagementTreeModel(staff);
			treeModel.BuildTree();
		}

		GlbStaff payrollDirectReport1;
		GlbStaff payrollDirectReport2;
		GlbStaff funDirectReport1;
		GlbStaff funDirectReport2;

		void SetupDirectReportTestData()
		{
			var roleCollection = new StaffReportingRoleCollection();
			roleCollection.Add("PRM", (NoResString)"Payroll Manager", true, false, true);
			roleCollection.Add("FNM", (NoResString)"Fun Manager", false, false, true);
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roleCollection);

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = FullName;

			payrollDirectReport1 = Factory.NewWithValidTestData<GlbStaff>();
			payrollDirectReport1.GS_FullName = PRM1FullName;
			StaffManagerTestHelper.AddManager(payrollDirectReport1, staff, "PRM", new ZDateTime(2019, 01, 20), ZDateTime.Now.AddDays(-1));

			payrollDirectReport2 = Factory.NewWithValidTestData<GlbStaff>();
			payrollDirectReport2.GS_FullName = PRM2FullName;
			payrollDirectReport2.GS_Title = PRM2JobTitle;
			StaffManagerTestHelper.AddManager(payrollDirectReport2, staff, "PRM");

			funDirectReport1 = Factory.NewWithValidTestData<GlbStaff>();
			funDirectReport1.GS_FullName = FNMFullName1;
			StaffManagerTestHelper.AddManager(funDirectReport1, staff, "FNM", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			funDirectReport2 = Factory.NewWithValidTestData<GlbStaff>();
			funDirectReport2.GS_FullName = FNMFullName2;
			StaffManagerTestHelper.AddManager(funDirectReport2, staff, "FNM");

			treeModel = new GlbStaffManagementTreeModel(staff, isManager: true);
			treeModel.BuildTree();
		}

		#endregion
	}
}
