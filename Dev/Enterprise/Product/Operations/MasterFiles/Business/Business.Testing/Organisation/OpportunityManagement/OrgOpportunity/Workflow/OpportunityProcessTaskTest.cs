using System;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OpportunityProcessTasks))]
	sealed class OpportunityProcessTaskTest : ProcessTaskTest
	{
		public void TestLogging()
		{
			var tempValue = new EnableAddEditAndDeleteLogsItemCollection
			{
				new EnableAddEditAndDeleteLogsItem()
				{
					Table = ProcessTasksSchema.Constants.TableName,
					EnableADDLogs = true,
					EnableEDTLogs = true,
					EnableDELLogs = true,
				}
			};

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				OrgOpportunity opp = org.SalesOpportunities.AddNew();

				ProcessTask task = opp.WorkflowItems.AddNew();
				Factory.Save();
				AssertEquals("Autolog event reference description", "Task " + task.P9_TaskID + " for Opportunity " + opp.P8_OpportunityID + ".", task.Logs.AutoCreatedLog.SL_Reference);

				task.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today);
				Factory.Save();
				AssertEquals("Autolog event reference description", "Task " + task.P9_TaskID + " for Opportunity " + opp.P8_OpportunityID + ". Scheduled Date Set", task.Logs.AutoCreatedLog.SL_Reference);

				task.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Empty);
				Factory.Save();
				AssertEquals("Autolog event reference description", "Task " + task.P9_TaskID + " for Opportunity " + opp.P8_OpportunityID + ".", task.Logs.AutoCreatedLog.SL_Reference);
			}
		}

		public void TestSubclassOfCRMProcessTask()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(CRMProcessTask)));
		}

		public void TestReminderBodyTemplate_NullReferenceException()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test NRE";
			staff.GS_EmailAddress = "testnre@test.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			var task = opportunity.WorkflowItems.AddNew();
			task.P9_TaskID = "123";
			task.P9_Description = "NRE Description";
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Test Notes"));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_ScheduledDate = ZDateTime.Now;
			task.P9_EstDuration = ZDateTime.Now;
			Factory.Save();

			task.P9_OA_ZAddress.OrgPK = ZGuid.Empty;
			task.P9_ParentID = ZGuid.Invalid;
			AssertNoExceptionThrown(() => _ = task.TaskReminder);
		}

		public void TestCalendarReminderBody()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("AAA", (NoResString)"AAA Type", true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Staff 1";
			staff.GS_EmailAddress = "test@example.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bits & Bobs";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_City = "Bombay";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";
			org.MiscServ.OM_CMLastCallDate = new ZDateTime(2013, 4, 22);

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_Description = "Ring John <at home>";
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2005, 11, 26, 9, 0, 0)));
			ProcessTaskCoreTest.PrepareTaskForSendingReminder(task);
			Assert(task.P9_IsCalendarItem);

			OrgOpportunity opp = (OrgOpportunity)task.Parent;
			opp.P8_OpportunityType = "AAA";
			opp.P8_OpportunityNotes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Opp note 1"));
			Factory.Save();

			string taskIdLine = $"Task ID: {task.P9_TaskID}";
			string taskUrlLine = "Task ID: " + string.Format(@"<a href=""{0}"">{1}</a>",
					ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ProcessTasks, task.PK.ToGuid()),
					task.P9_TaskID);

			string parentIdLine = "Opportunity - " + opp.P8_OpportunityID;
			string parentUrlLine = "Opportunity - " + string.Format(@"<a href=""{0}"">{1}</a>",
					ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Opportunity, opp.PK.ToGuid()),
					opp.P8_OpportunityID);

			string expectedBodyWithNoContact = $@"Opportunity - 123 - Test Opportunity
Organization: Bits & Bobs ({org.OH_Code})

Client Address:
88 BOB ST
BOMBAY MH
INDIA
Office Phone: (02) 7777 8888

Last Communication Date: 22-Apr-13
Sales Type: AAA Type
Task Type: fdsfsdf
Task Description: Ring John <at home>
Task ID: {task.P9_TaskID}
Task Notes: Hello this is a test message.
Opportunity Notes: Opp note 1
";

			string expectedHtmlBody = string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>",
				WebUtility.HtmlEncode(expectedBodyWithNoContact)
				.Replace(taskIdLine, taskUrlLine)
				.Replace(parentIdLine, parentUrlLine));
			Reminder rem = task.TaskReminder;
			AssertMultilineASCIIEquals("Body", expectedBodyWithNoContact, rem.Body);
			AssertMultilineASCIIEquals("HtmlBody", expectedHtmlBody, rem.HtmlBody);

			org.MiscServ.OM_CMLastCallDate = ZDateTime.Empty;
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Leanne";
			contact1.OC_Phone = "(02) 8888 8888";
			contact1.OC_Title = "Governor";
			contact1.OC_Email = "leanne@test.com";
			contact1.OC_Mobile = "111 2222 3333";
			contact1.OC_JobCategory = "Ruler";
			task.P9_OC = contact1.PK;
			rem = task.TaskReminder;
			string expectedBodyWithContact = $@"Opportunity - 123 - Test Opportunity
Organization: Bits & Bobs ({org.OH_Code})

Contact: Leanne
Title: Governor
Job Category: Ruler
Email: leanne@test.com
Phone: (02) 8888 8888
Mobile: 111 2222 3333

Client Address:
88 BOB ST
BOMBAY MH
INDIA
Office Phone: (02) 7777 8888

Last Communication Date: 
Sales Type: AAA Type
Task Type: fdsfsdf
Task Description: Ring John <at home>
Task ID: {task.P9_TaskID}
Task Notes: Hello this is a test message.
Opportunity Notes: Opp note 1
";

			expectedHtmlBody = string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>",
				WebUtility.HtmlEncode(expectedBodyWithContact)
				.Replace(taskIdLine, taskUrlLine)
				.Replace(parentIdLine, parentUrlLine));

			AssertMultilineASCIIEquals("Body", expectedBodyWithContact, rem.Body);
			AssertMultilineASCIIEquals("HtmlBody", expectedHtmlBody, rem.HtmlBody);
		}

		public void TestCalendarReminderSubject()
		{
			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bits & Bobs";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John <at home>";
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;

			ProcessTaskCoreTest.PrepareTaskForSendingReminder(task);
			Assert(task.P9_IsCalendarItem);

			task.P9_ScheduledDate = new ZDateTime(2005, 11, 26, 9, 0, 0);
			task.P9_EstDuration = new ZDateTime(2007, 1, 1, 3, 23, 0);
			zubin.GS_EmailAddress = "test@example.com";

			Factory.Save();

			Reminder rem = task.TaskReminder;
			string expectedPrefix = !string.IsNullOrEmpty(ExpectedSubjectPrefix) ? ExpectedSubjectPrefix + " - " : "";
			AssertEquals("Correct Subject", expectedPrefix + "Bits & Bobs - (02) 7777 8888 (Task " + task.P9_TaskID + ")", rem.Subject);

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Leanne";
			contact1.OC_Phone = "(02) 8888 8888";
			task.P9_OC = contact1.PK;
			rem = task.TaskReminder;
			AssertEquals("Correct Subject", expectedPrefix + "Bits & Bobs - Leanne, (02) 8888 8888 (Task " + task.P9_TaskID + ")", rem.Subject);
		}

		string ExpectedSubjectPrefix => string.Format("Opportunity ({0}) - {1}", Opp.P8_OpportunityID, DescriptionPropertyAttribute.DescriptionFromBusinessObject(Opp));

		ProcessTask GetTaskForCalendarReminderTesting(OrgHeader org)
		{
			Opp = org.SalesOpportunities.AddNew();
			Opp.P8_OpportunityType = Opp.Lookups.Types[0].Code;
			Opp.P8_OpportunityID = "123";
			Opp.P8_OpportunityDescription = "Test Opportunity";

			return Opp.WorkflowItems.AddNew();
		}

		OrgOpportunity Opp;

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			return opp.WorkflowItems.AddNew();
		}

		#endregion
	}
}
