using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectContactForEmail))]
	class ProjectContactForEmailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocManagerInfo()
		{
			Project project = Factory.New<Project>();
			ProjectContactForEmail contact = new ProjectContactForEmail(project);
			DocManagerInfo support = contact.DocManagerInfo;
			AssertEquals(project, support.BusinessEntity);
			AssertEquals(Core.Constants.DocManagerCodes.Project, support.DocManagerCode);
		}

		public void TestLogs()
		{
			Project project = Factory.New<Project>();
			ProjectContactForEmail contact = new ProjectContactForEmail(project);
			AssertEquals(project.Logs, ((ISendEmailSource)contact).Logs);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			Project project = Factory.New<Project>();
			return new ProjectContactForEmail(project);
		}

		#endregion
	}
}
