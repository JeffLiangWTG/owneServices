using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business.Test
{
	class WorkItemHelperTest : TestCaseWithFactory
	{
		public void TestCreateWorkItems_WithDifferentSummaries_WithCheck()
		{
			var creator = new WorkItemHelper();

			creator.CreateWorkItem(Factory, true, "WKI", "", "", "", "", "summary", "WI1");
			creator.CreateWorkItem(Factory, true, "WKI", "", "", "", "", "different summary", "WI2");

			var workItems = Factory.Load<IWorkItem>(new ZQuery());
			AssertEquals(2, workItems.Length);
			AssertEquals("ASN", workItems[0].WKI_Status);
			AssertEquals("ASN", workItems[1].WKI_Status);
		}

		public void TestCreateWorkItems_WithSameSummaries_WithoutCheck()
		{
			var creator = new WorkItemHelper();
			creator.CreateWorkItem(Factory, false, "WKI", "", "", "", "", "summary", "WI1");
			creator.CreateWorkItem(Factory, false, "WKI", "", "", "", "", "summary", "WI2");

			var workItems = Factory.Load<IWorkItem>(new ZQuery());
			AssertEquals(2, workItems.Length);
			AssertEquals("ASN", workItems[0].WKI_Status);
			AssertEquals("ASN", workItems[1].WKI_Status);
		}

		public void TestCreateWorkItems_WithSameSummaries_WithCheck()
		{
			var creator = new WorkItemHelper();
			creator.CreateWorkItem(Factory, true, "WKI", "", "", "", "", "summary", "WI1");
			creator.CreateWorkItem(Factory, true, "WKI", "", "", "", "", "summary", "WI2");

			var workItems = Factory.Load<IWorkItem>(new ZQuery());
			AssertEquals(1, workItems.Length);
			AssertEquals("ASN", workItems[0].WKI_Status);
		}

		public void TestCreateWorkItems_WithSameSummaries_WithCheck_WithCancel()
		{
			var creator = new WorkItemHelper();
			creator.CreateWorkItem(Factory, true, "WKI", "", "", "", "", "summary", "WI1");

			var workItems = Factory.Load<IWorkItem>(new ZQuery());
			workItems[0].WKI_Status = "CAN";
			Factory.Save();

			creator.CreateWorkItem(Factory, true, "WKI", "", "", "", "", "summary", "WI2");

			workItems = Factory.Load<IWorkItem>(new ZQuery());
			AssertEquals(2, workItems.Length);
			AssertEquals("CAN", workItems.Where(wi => wi.WKI_Details.ToUTF8() == "WI1").First().WKI_Status);
			AssertEquals("ASN", workItems.Where(wi => wi.WKI_Details.ToUTF8() == "WI2").First().WKI_Status);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			var wkiSystem = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "Workflow");
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow);

			Factory.Save();
		}

		IBMTestHelper BMSTestHelper { get; } = ObjectFactory.Get<IBMTestHelper>();
	}
}
