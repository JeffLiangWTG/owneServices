using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RegistryItemProcessTaskExtensionsTest : TestCaseWithFactory
	{
		public void TestGetValue()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "CCC";

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "BBB";

			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Code = "EEE";
			Factory.Save();

			RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "System");
			RegistryItem.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), "System-Department");
			RegistryItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "Company");
			RegistryItem.SetValue(company.PK.ToGuid(), Guid.Empty, department.PK.ToGuid(), "Company-Department");
			RegistryItem.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "Branch");
			RegistryItem.SetValue(Guid.Empty, branch.PK.ToGuid(), department.PK.ToGuid(), "Branch-Department");

			Template.P0_GC = ZGuid.Empty;
			AssertEquals("System", RegistryItem.GetValue(Task));

			Template.P0_GC = company.PK;
			AssertEquals("Company", RegistryItem.GetValue(Task));

			Template.P0_GB = branch.PK;
			AssertEquals("Branch", RegistryItem.GetValue(Task));

			Template.P0_GE = department.PK;
			AssertEquals("Branch-Department", RegistryItem.GetValue(Task));

			Template.P0_GB = ZGuid.Empty;
			AssertEquals("Company-Department", RegistryItem.GetValue(Task));

			Template.P0_GC = ZGuid.Empty;
			AssertEquals("System-Department", RegistryItem.GetValue(Task));
		}

		public void TestFallbackLevels()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "CCC";

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "BBB";

			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Code = "EEE";
			Factory.Save();

			Template.P0_GC = company.PK;
			Template.P0_GB = branch.PK;
			Template.P0_GE = department.PK;

			StringRegistryItem regItemSystem = createRegistryItem(RegistryStorageFlags.System);
			regItemSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "System");
			AssertEquals("System", regItemSystem.GetValue(Task));
			((IRegistryItemInternals)regItemSystem).DeleteRecord(Guid.Empty, Guid.Empty, Guid.Empty);

			StringRegistryItem regItemSystemDepartment = createRegistryItem(RegistryStorageFlags.SystemDepartment);
			regItemSystemDepartment.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), "System Department");
			AssertEquals("System Department", regItemSystemDepartment.GetValue(Task));
			((IRegistryItemInternals)regItemSystemDepartment).DeleteRecord(Guid.Empty, Guid.Empty, department.PK.ToGuid());

			StringRegistryItem regItemCompany = createRegistryItem(RegistryStorageFlags.Company);
			regItemCompany.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "Company");
			AssertEquals("Company", regItemCompany.GetValue(Task));
			((IRegistryItemInternals)regItemCompany).DeleteRecord(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			StringRegistryItem regItemCompDept = createRegistryItem(RegistryStorageFlags.CompanyDepartment);
			regItemCompDept.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), "Company Department");
			AssertEquals("Company Department", regItemCompDept.GetValue(Task));
			((IRegistryItemInternals)regItemCompDept).DeleteRecord(Guid.Empty, Guid.Empty, department.PK.ToGuid());

			StringRegistryItem regItemBranch = createRegistryItem(RegistryStorageFlags.Branch);
			regItemBranch.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "Branch");
			AssertEquals("Branch", regItemBranch.GetValue(Task));
			((IRegistryItemInternals)regItemBranch).DeleteRecord(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);

			StringRegistryItem regItemBranchDepartment = createRegistryItem(RegistryStorageFlags.BranchDepartment);
			regItemBranchDepartment.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), "Branch Department");
			AssertEquals("Branch Department", regItemBranchDepartment.GetValue(Task));
			((IRegistryItemInternals)regItemBranchDepartment).DeleteRecord(Guid.Empty, Guid.Empty, department.PK.ToGuid());

			StringRegistryItem regItemAllDepartments = createRegistryItem(RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.BranchDepartment);
			regItemAllDepartments.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), "All Departments");
			AssertEquals("All Departments", regItemAllDepartments.GetValue(Task));
			((IRegistryItemInternals)regItemAllDepartments).DeleteRecord(Guid.Empty, Guid.Empty, department.PK.ToGuid());
		}

		public void TestGetValue_NoTemplate()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "CCC";

			Factory.Save();

			RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "System");
			RegistryItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "Company");

			Task.P9_ParentID = ZGuid.Empty;

			Template.P0_GC = ZGuid.Empty;
			AssertEquals("System", RegistryItem.GetValue(Task));

			Task.P9_GC = company.PK;
			AssertEquals("Company", RegistryItem.GetValue(Task));
		}

		public void TestInvalid()
		{
			Template.P0_GC = ZGuid.Invalid; // should not be possible, but check anyway.
			Template.P0_GB = ZGuid.Invalid;
			Template.P0_GE = ZGuid.Invalid;

			AssertNoExceptionThrown(delegate
			{ RegistryItem.GetValue(Task); });
		}

		#region Implementation

		ProcessTaskTemplate Template
		{
			get { return template ?? (template = Factory.New<ProcessTaskTemplate>()); }
		}

		ProcessTask Task
		{
			get { return task ?? (task = Template.WorkflowItems.AddNew()); }
		}

		StringRegistryItem RegistryItem
		{
			get
			{
				if (registryItem == null)
				{
					registryItem = createRegistryItem(RegistryStorageFlags.All);
				}

				return registryItem;
			}
		}

		static StringRegistryItem createRegistryItem(RegistryStorageFlags regStorageFlags)
		{
			return new StringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", regStorageFlags, RegistryOptions.Default, "Default");
		}

		ProcessTaskTemplate template;
		ProcessTask task;
		StringRegistryItem registryItem;

		#endregion
	}
}
