using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAllowedBranchDepartmentCombo))]
	sealed class TestAccAllowedBranchDepartmentCombo : EnterpriseBusinessObjectTestCase
	{
		public void TestLogging()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "ASD";
			department.GE_Desc = "Asdf";
			Factory.Save();

			var link0 = Factory.New<AccAllowedBranchDepartmentCombo>();
			link0.AAB_GB_Branch = branch.PK;
			link0.AAB_GE_Department = department.PK;
			link0.Delete();
			Factory.Save();
			AssertEquals("Attached", false, branch.Logs.Find(log => log.SL_Reference == string.Format("Attached - ({0}) {1}", department.GE_Code, department.GE_Desc)).Any());
			AssertEquals("Detached", false, branch.Logs.Find(log => log.SL_Reference == string.Format("Detached - ({0}) {1}", department.GE_Code, department.GE_Desc)).Any());

			var link1 = Factory.New<AccAllowedBranchDepartmentCombo>();
			link1.AAB_GB_Branch = branch.PK;
			link1.AAB_GE_Department = department.PK;
			Factory.Save();
			AssertEquals("Attached", true, branch.Logs.Find(log => log.SL_Reference == string.Format("Attached - ({0}) {1}", department.GE_Code, department.GE_Desc)).Any());
			AssertEquals("Detached", false, branch.Logs.Find(log => log.SL_Reference == string.Format("Detached - ({0}) {1}", department.GE_Code, department.GE_Desc)).Any());

			link1.Delete();
			Factory.Save();
			AssertEquals("Attached", true, branch.Logs.Find(log => log.SL_Reference == string.Format("Attached - ({0}) {1}", department.GE_Code, department.GE_Desc)).Any());
			AssertEquals("Detached", true, branch.Logs.Find(log => log.SL_Reference == string.Format("Detached - ({0}) {1}", department.GE_Code, department.GE_Desc)).Any());

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "QWE";
			department2.GE_Desc = "Qwer";
			Factory.Save();

			branch2.GB_City = "Sydney";
			var link2 = Factory.New<AccAllowedBranchDepartmentCombo>();
			link2.AAB_GB_Branch = branch2.PK;
			link2.AAB_GE_Department = department2.PK;

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedBranch2 = anotherFactory.Load<GlbBranch>(branch2.PK);
			loadedBranch2.GB_City = "Melbourne";
			anotherFactory.Save();

			try
			{
				Factory.Save();
			}
			catch { }

			AssertEquals("Save should fail because of concurrency error and link is not in database", false, link2.IsInDatabase);
			AssertEquals("Attached", false, branch2.Logs.Find(log => log.SL_Reference == string.Format("Attached - ({0}) {1}", department2.GE_Code, department2.GE_Desc)).Any());
			AssertEquals("Detached", false, branch2.Logs.Find(log => log.SL_Reference == string.Format("Detached - ({0}) {1}", department2.GE_Code, department2.GE_Desc)).Any());

			branch2.Reload();
			Factory.Save();
			AssertEquals("Attached", true, branch2.Logs.Find(log => log.SL_Reference == string.Format("Attached - ({0}) {1}", department2.GE_Code, department2.GE_Desc)).Any());
			AssertEquals("Detached", false, branch2.Logs.Find(log => log.SL_Reference == string.Format("Detached - ({0}) {1}", department2.GE_Code, department2.GE_Desc)).Any());
		}
	}
}
