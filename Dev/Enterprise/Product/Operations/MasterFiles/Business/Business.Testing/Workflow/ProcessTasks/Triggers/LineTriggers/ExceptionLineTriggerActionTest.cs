using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ExceptionLineTriggerActionTest : TemplateApplicationTestCase
	{
		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");
		}

		#endregion

		#region NotificationEmail Trigger

		public void TestLineTriggerUDFDataContext()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ALX";

			Factory.Save();

			var trigger = staff.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Line Trigger";
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<GS_Code>\"==\"ALX\"";
			trigger.P9_RespondToCascadedEvents = true;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			AssertEquals(0, trigger.GetWTELogs().Length);

			var exception = staff.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "I am Exception";
			exception.Logs.AddNew(Events.CustomisableEvent00);

			AssertEquals(1, trigger.GetWTELogs().Length);
		}

		public void TestLineTriggerDataSource()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "A dark calamity come";
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailText = "Type:(*P9_Type*)";
			notification.PQ_EmailAddr = "bigbadcheese@kodo.com";
			var exception = dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "Better hide those acorns";
			exception.Logs.AddNew(Events.CustomisableEvent00);

			Assert(trigger.P9_ActualDateForBinding.IsValid);

			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var email = Environment.Env.OutgoingMailManager.EmailsCreated.Single();
			AssertContains("Type:EXC", email.Body);
		}

		public void TestLineTriggerDataSource_Group()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "JIMMY@jammy.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "A dark calamity come";
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AssignedGroupMembers;
			notification.PQ_EmailText = "Type:(*P9_Type*)";
			notification.PQ_EmailAddr = "bigbadcheese@kodo.com";
			var exception = dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "Better hide those acorns";
			exception.P9_GG_AssignedGroup = group.PK;
			exception.Logs.AddNew(Events.CustomisableEvent00);

			Assert(trigger.P9_ActualDateForBinding.IsValid);

			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var email = Environment.Env.OutgoingMailManager.EmailsCreated.Single();
			AssertContains("Type:EXC", email.Body);
		}

		public void TestEstimateDoesNotFireLineTrigger()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "A dark calamity come";
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AssignedGroupMembers;
			notification.PQ_EmailText = "Type:(*P9_Type*)";
			notification.PQ_EmailAddr = "bigbadcheese@kodo.com";

			var exception = dummy.WorkflowItems.Exceptions.AddNew();
			exception.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now, isEstimate: true);
			AssertEquals(false, trigger.P9_ActualDateForBinding.IsValid);
		}

		#endregion

		#region TriggerCountdown

		public void TestTaskLineTriggerCountdownDoesNotGoBelowZero()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var exception = dummy.WorkflowItems.Exceptions.AddNew();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 1;

			Factory.Save();

			exception.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals((short)0, trigger.P9_TriggerFiredCountdown);

			Factory.Save();

			exception.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals((short)0, trigger.P9_TriggerFiredCountdown);
		}

		#endregion

		#region XUE

		static ProcessTask FireXUELineTrigger(IWorkflowProvider provider)
		{
			var trigger = provider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "SomeTriggerDescription";
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			var exception = provider.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "SomeTaskDescription";
			exception.Logs.AddNew(Events.CustomisableEvent00);
			return exception;
		}

		public void TestXUE_Integration()
		{
			using (var com = WithOrgProxyCommunicationsMode())
			{
				com.Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				com.Mode.EK_Module = "SHP";
				Factory.Save();
				var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)dummy).FillWithValidTestData();
				FireXUELineTrigger(dummy);
				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var message = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(message.Length, 1);
			}
		}

		public void TestXUE_ContextCollection()
		{
			using (var com = WithOrgProxyCommunicationsMode())
			{
				com.Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				com.Mode.EK_Module = "SHP";
				Factory.Save();
				var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)dummy).FillWithValidTestData();
				var line = FireXUELineTrigger(dummy);
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Code = "BOO";
				line.P9_GG_AssignedGroup = group.PK;
				line.P9_NotesAsString = "Fishcake";
				line.P9_Sequence = 11;
				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery());
				var text = Regex.Replace(message.EM_MessageText, "\\s", "");

				CombineAssertions(() =>
				{
					AssertContains("<Type>TaskType</Type><Value>EXC</Value>", text);
					AssertContains("<Type>TaskSequence</Type><Value>11</Value>", text);
					AssertContains("<Type>TaskDescription</Type><Value>SomeTaskDescription</Value>", text);
					AssertContains("<Type>TaskStatus</Type><Value>OPN</Value>", text);
					AssertContains("<Type>AssignedGroup</Type><Value>BOO</Value>", text);
					AssertContains("<Type>AssignedStaff</Type><Value></Value>", text);
					AssertContains("<Type>AssignedCapability</Type><Value></Value>", text);
					AssertContains("<Type>TaskNotes</Type><Value>Fishcake</Value>", text);
				});
			}
		}

		public void TestXUE_AdditionalContextCollection()
		{
			using (var com = WithOrgProxyCommunicationsMode())
			{
				com.Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				Factory.Save();
				var dummy = Factory.New<DummyWithWorkflow>();
				var line = FireXUELineTrigger(dummy);
				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery());
				var text = Regex.Replace(message.EM_MessageText, "\\s", "");
				AssertContains("<AdditionalContext><DataContext><DataSourceCollection><DataSource><Type>DummyBusinessObject</Type>", text);
				AssertContains("CubbyHouseBill", text);
			}
		}

		public void TestXUE_ContextCollection_P9Notes_Rtf()
		{
			using (var com = WithOrgProxyCommunicationsMode())
			{
				com.Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				com.Mode.EK_Module = "SHP";
				Factory.Save();
				var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)dummy).FillWithValidTestData();
				var line = FireXUELineTrigger(dummy);
				line.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("I love chicken sandwich"));
				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery());
				var text = message.EM_MessageText;
				AssertContains("<Value>I love chicken sandwich</Value>", text);
			}
		}

		public void TestXUE_Recipients()
		{
			var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)dummy).FillWithValidTestData();
			var lineTrigger = dummy.WorkflowItems.Triggers.AddNew();
			lineTrigger.P9_Description = "A dark calamity come";
			lineTrigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			lineTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var lineNotification = lineTrigger.ProcessTaskNotifications.AddNew();
			lineNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			var codes2 = lineNotification.Lookups.MessagingTriggerPartiesList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(new[] { "ORP", "OTH" }, codes2);
		}

		#endregion

		#region XUS

		static ProcessTask FireXUSLineTrigger(IWorkflowProvider provider)
		{
			var trigger = provider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "SomeTriggerDescription";
			trigger.P9_LineTriggerType = ProcessTasksLookups.ExceptionLineTriggerCode;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			var exception = provider.WorkflowItems.Exceptions.AddNew();
			exception.Logs.AddNew(Events.CustomisableEvent00);
			return exception;
		}

		public void TestXUS_Integration()
		{
			var categories = new CodeDescriptionPairList();
			categories.AddPair("CAT", "Category");
			WorkflowDataRegistry.Instance.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(categories));

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);
			exceptionType.WET_Category = "CAT";

			var cause = exceptionType.Causes.AddNew();
			cause.WEC_Code = "CAU";
			cause.WEC_Description = "cause";
			var resolution = exceptionType.Resolutions.AddNew();
			resolution.WER_Code = "RES";
			resolution.WER_Description = "resolution";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			group.GG_Desc = "Group";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "STF";
			staff.GS_FullName = "Staff";

			using (var com = WithOrgProxyCommunicationsMode())
			{
				com.Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				com.Mode.EK_Module = "SHP";
				Factory.Save();
				var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)dummy).FillWithValidTestData();
				var exception = FireXUSLineTrigger(dummy);

				exception.P9_Description = "exception";
				exception.IsExceptionActioned = true;
				exception.P9_ActualDateForBinding = ZDateTimeOffset.UtcNow;
				exception.ExceptionTypeCode = exceptionType.WET_Code;
				exception.ExceptionCausePK = cause.PK;
				exception.ExceptionResolutionPK = resolution.PK;
				exception.P9_GG_AssignedGroup = group.PK;
				exception.P9_GS_NKAssignedStaffMember = staff.GS_Code;

				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(messages.Length, 1);

				var text = Regex.Replace(messages.Single().EM_MessageText, "\\s", "");

				CombineAssertions(() =>
				{
					AssertContains("<ExceptionCollection>", text);
					AssertContains("<Description>exception</Description>", text);
					AssertContains($"<ExceptionID>{exception.P9_TaskID}</ExceptionID>", text);
					AssertContains("<Actioned>true</Actioned>", text);
					AssertContains("<Category>CAT</Category>", text);
					AssertContains("<Cause>CAU</Cause>", text);
					AssertContains("<Resolution>RES</Resolution>", text);
					AssertContains("<Group><Code>GRP</Code><Name>Group</Name></Group>", text);
					AssertContains("<Staff><Code>STF</Code><Name>Staff</Name></Staff>", text);
					AssertContains("<Type>TYP</Type>", text);
				});
			}
		}

		#endregion
	}
}
