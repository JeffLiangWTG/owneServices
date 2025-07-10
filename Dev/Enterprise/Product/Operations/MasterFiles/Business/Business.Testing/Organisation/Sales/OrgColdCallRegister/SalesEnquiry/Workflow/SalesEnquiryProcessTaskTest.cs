using System;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	// Not deriving from ProcessTaskTest since it has Opportunity specific code that causes errors.
	[TestedType(typeof(SalesEnquiryProcessTask))]
	class SalesEnquiryProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReminder()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "111";
			staff1.GS_EmailAddress = "1@test.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bits & Bobs";
			org.MainAddress.OA_Address1 = "1st Unit";
			org.MainAddress.OA_Address1 = "2nd Street";
			org.MainAddress.OA_City = "Auckland";
			org.MainAddress.OA_State = "Auckland";
			org.MainAddress.OA_PostCode = "1234";
			org.MainAddress.OA_Phone = "95551111";
			org.MainAddress.OA_Email = "org@test.com";
			org.MiscServ.OM_CMLastCallDate = new DateTime(2013, 4, 22, 11, 15, 0);
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Bob Bit";
			SalesEnquiry enquiryWithOrg = Factory.New<SalesEnquiry>();
			enquiryWithOrg.O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
			enquiryWithOrg.O1_LeadUniqueReference = "111";
			enquiryWithOrg.OrgPk = org.PK;
			enquiryWithOrg.O1_ContactName = contact.OC_ContactName;
			enquiryWithOrg.EnquiryNotesContent = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("This enquiry note."));
			SalesEnquiryProcessTask taskWithOrg = (SalesEnquiryProcessTask)enquiryWithOrg.WorkflowItems.AddNew();
			taskWithOrg.P9_Description = "Ring John <at home>";
			taskWithOrg.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			taskWithOrg.P9_OA = org.MainAddress.PK;
			taskWithOrg.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			SalesEnquiry enquiryWithoutOrg = Factory.New<SalesEnquiry>();
			enquiryWithoutOrg.O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
			enquiryWithoutOrg.O1_LeadUniqueReference = "222";
			enquiryWithoutOrg.O1_CompanyName = "Company 2";
			enquiryWithoutOrg.O1_Address1 = "Address 2a";
			enquiryWithoutOrg.O1_City = "Brisbane";
			enquiryWithoutOrg.O1_State = "QLD";
			enquiryWithoutOrg.O1_PortOrCountry = "AU";
			enquiryWithoutOrg.O1_ContactName = "Contact 2";
			enquiryWithoutOrg.O1_Email = "contact2@test.com";
			enquiryWithoutOrg.O1_JobCategory = "System Tester";

			SalesEnquiryProcessTask taskWithoutOrg = (SalesEnquiryProcessTask)enquiryWithoutOrg.WorkflowItems.AddNew();
			taskWithoutOrg.P9_Description = "Task <2>";
			taskWithoutOrg.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Task 2 note."));
			taskWithoutOrg.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			ProcessTaskCoreTest.PrepareTaskForSendingReminder(taskWithOrg, taskWithoutOrg);
			taskWithoutOrg.P9_Type = taskWithOrg.P9_Type;

			Factory.Save();

			#region Expected

			string expectedBodyWithOrg = $@"Inquiry (111)
Organization: Bits & Bobs ({org.OH_Code})

Contact: Bob Bit
Job Category: Employee (Undefined)

Client Address:
2ND STREET
AUCKLAND AUCKLAND 1234
Office Email: org@test.com
Office Phone: 95551111

Last Communication Date: 22-Apr-13
Inquiry Type: General Inquiry
Task Type: fdsfsdf
Task Description: Ring John <at home>
Task ID: {taskWithOrg.P9_TaskID}
Task Notes: Hello this is a test message.
Inquiry Notes: This enquiry note.";

			var urlCreator = ObjectFactory.Get<IShowEditFormUrlCreator>();

			string expectedHTMLBodyWithOrg = @"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>"
				+ WebUtility.HtmlEncode(expectedBodyWithOrg)
					.Replace("Task ID: " + taskWithOrg.P9_TaskID, @"Task ID: <a href=""" + urlCreator.Create(ControllerIDs.ProcessTasks, taskWithOrg.PK.ToGuid()) + $@""">{taskWithOrg.P9_TaskID}</a>")
					.Replace("Inquiry (111)", @"Inquiry (<a href=""" + urlCreator.Create(ControllerIDs.SalesEnquiry, enquiryWithOrg.PK.ToGuid()) + @""">111</a>)")
				+ @"</BODY></HTML>";

			string expectedBodyWithoutOrg = $@"Inquiry (222)
Organization: Company 2

Contact: Contact 2
Job Category: System Tester
Email: contact2@test.com

Client Address:
ADDRESS 2A
BRISBANE QLD

Inquiry Type: General Inquiry
Task Type: fdsfsdf
Task Description: Task <2>
Task ID: {taskWithoutOrg.P9_TaskID}
Task Notes: Task 2 note.
Inquiry Notes: ";

			string expectedHTMLBodyWithoutOrg = @"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>"
				+ WebUtility.HtmlEncode(expectedBodyWithoutOrg)
					.Replace("Task ID: " + taskWithoutOrg.P9_TaskID, @"Task ID: <a href=""" + urlCreator.Create(ControllerIDs.ProcessTasks, taskWithoutOrg.PK.ToGuid()) + $@""">{taskWithoutOrg.P9_TaskID}</a>")
					.Replace("Inquiry (222)", @"Inquiry (<a href=""" + urlCreator.Create(ControllerIDs.SalesEnquiry, enquiryWithoutOrg.PK.ToGuid()) + @""">222</a>)")
				+ @"</BODY></HTML>";

			#endregion

			Reminder rem = taskWithOrg.TaskReminder;
			AssertEquals($"Inquiry (111) - Bits & Bobs - Bob Bit (Task {taskWithOrg.P9_TaskID})", rem.Subject);
			AssertMultilineASCIIEquals("Body", expectedBodyWithOrg, rem.Body);
			AssertMultilineASCIIEquals("HtmlBody", expectedHTMLBodyWithOrg, rem.HtmlBody);

			rem = taskWithoutOrg.TaskReminder;
			AssertEquals($"Inquiry (222) - Company 2 - Contact 2 (Task {taskWithoutOrg.P9_TaskID})", rem.Subject);
			AssertMultilineASCIIEquals("Body", expectedBodyWithoutOrg, rem.Body);
			AssertMultilineASCIIEquals("HtmlBody", expectedHTMLBodyWithoutOrg, rem.HtmlBody);
		}

		public void TestGetTypeForLoad()
		{
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_ParentTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			AssertEquals(typeof(SalesEnquiryProcessTask), ProcessTask.TypeDecider.GetTypeForLoad(((INeedRow)processTask).Row, Factory));
		}

		public void TestGetQueryForLoad()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask dummyTask = dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask dummyTrigger = dummy.WorkflowItems.Triggers.AddNew();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			ProcessTask task = enquiry.WorkflowItems.Tasks.AddNew();
			ProcessTask milestone = enquiry.WorkflowItems.Milestones.AddNew();
			Factory.Save();

			WorkflowDescriptors descriptors = WorkflowDescriptors.Instance;
			ProcessTaskTypeDecider typeDecider = ProcessTask.TypeDecider;
			WorkflowDescriptor descriptor;
			descriptors.TryGetValue(SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, out descriptor);
			ProcessTask[] processTasks = Factory.Load<ProcessTask>(typeDecider.GetQueryForLoad(descriptor));
			AssertEquals(2, processTasks.Length);
			AssertCollectionContains(task, processTasks);
			AssertCollectionContains(milestone, processTasks);
		}

		public void TestSubclassOfCRMProcessTask()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(CRMProcessTask)));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			return enquiry.WorkflowItems.AddNew();
		}

		#endregion
	}
}
