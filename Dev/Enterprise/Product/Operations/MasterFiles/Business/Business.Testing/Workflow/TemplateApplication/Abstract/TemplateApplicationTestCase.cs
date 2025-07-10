using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class TemplateApplicationTestCase : TestCaseWithFactory
	{
		#region Builders

		protected ProcessTaskNotification MakeNotification(ProcessTask task)
		{
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			return notification;
		}

		protected ProcessTaskTemplate MakeTemplate(bool isGlobal = true)
		{
			return MakeTemplate(Factory, isGlobal);
		}

		protected ProcessTaskTemplate MakeTemplate(BusinessObjectFactory factory, bool isGlobal = true)
		{
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.GlobalTemplate = isGlobal;
			return template;
		}

		protected void SetAllFallback(ProcessTaskTemplate template, string code)
		{
			template.P0_CustomFieldFallback = FallbackTypeList.Codes.EmptyFallback == code ? FallbackTypeList.Codes.NeverFallback : code;
			template.P0_MilestoneFallbackMethod = code;
			template.P0_TriggerFallbackMethod = code;
			template.P0_TaskFallbackMethod = code;
		}

		protected ProcessTask MakeTask(IWorkflowProvider provider, bool isShared = true, string udfCondition = null, string description = null)
		{
			var task = provider.WorkflowItems.Tasks.AddNew();
			task.P9_Description = description ?? "T" + task.P9_Sequence;
			task.P9_ShareTasksForAllCompanies = isShared;
			if (udfCondition != null)
			{
				task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				task.TemplateConditions.TemplateCondition2Value = udfCondition;
			}
			return task;
		}

		protected ProcessTask MakeMilestone(IWorkflowProvider template, string eventCode = null, string udfCondition = null, string description = null)
		{
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = description ?? ("M" + milestone.P9_Sequence);
			milestone.TriggerConditions.TriggerEventCode = eventCode ?? Events.CustomisableEvent00Code;
			if (udfCondition != null)
			{
				milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				milestone.TemplateConditions.TemplateCondition2Value = udfCondition;
			}
			return milestone;
		}

		protected ProcessTask MakeTrigger(IWorkflowProvider template, string eventCode = null, string udfCondition = null)
		{
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "R" + trigger.P9_Sequence;
			trigger.TriggerConditions.TriggerEventCode = eventCode ?? Events.CustomisableEvent00Code;
			if (udfCondition != null)
			{
				trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				trigger.TemplateConditions.TemplateCondition2Value = udfCondition;
			}
			return trigger;
		}

		protected IProcessCompanyLinkRule MakeProcessCompanyLinkRule(ZGuid? company = null, string type = null, string macro = "\"1\"==\"1\"")
		{
			var rule = Factory.New<IProcessCompanyLinkRule>();
			rule.PCR_GC_Company = company ?? GlbCompany.CurrentCompany.PK;
			rule.PCR_Macro = macro;
			rule.PCR_Type = type ?? DummyWorkflowDescriptor.Instance.Code;
			return rule;
		}

		protected OrgHeader MakeOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			return org;
		}

		protected (GlbCompany company, GlbBranch branch) MakeCompanyWithBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			return (company, branch);
		}

		protected EDICommunicationsMode MakeCommunicationsMode(OrgHeader org, WorkflowDescriptor descriptor = null, string fileFormat = null)
		{
			var mode = org.EDICommunicationsModes.AddNew();
			mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			mode.EK_Destination = "DummyDestination";
			mode.EK_FileFormat = fileFormat ?? EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			mode.EK_Module = descriptor?.Code ?? DummyWorkflowDescriptor.Instance.Code;
			return mode;
		}

		protected WithCommunicationsMode WithOrgProxyCommunicationsMode()
		{
			var org = MakeOrg();
			var mode = MakeCommunicationsMode(org);
			var (company, branch) = MakeCompanyWithBranch();
			company.GC_OH_OrgProxy = org.PK;
			Factory.Save();
			return new WithCommunicationsMode(mode, branch);
		}

		public sealed class WithCommunicationsMode : IDisposable
		{
			public EDICommunicationsMode Mode { get; }
			readonly IDisposable disposable;

			public WithCommunicationsMode(EDICommunicationsMode mode, GlbBranch branch)
			{
				this.Mode = mode;
				disposable = Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK);
			}

			public void Dispose()
			{
				disposable.Dispose();
			}
		}

		#endregion

		#region Assertions

		static StmALog[] GetLogs(Event @event, IBusiness provider)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, provider.Identifier);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.ReLoadExistingRows = true;
			return provider.Factory.Load<StmALog>(query);
		}

		public static void AssertLogCount(string message, int expectedAmount, Event @event, IBusiness provider)
		{
			var logs = GetLogs(@event, provider);
			AssertEquals(message ?? FormattableString.Invariant($"Expecting {provider} to have fired a certain number of times"), expectedAmount, logs.Length);
		}

		public static void AssertLogCount(int expectedAmount, Event @event, IBusiness provider) => AssertLogCount(null, expectedAmount, @event, provider);

		public static void AssertMilestoneFireCount(string message, int expectedAmount, ProcessTask task)
		{
			AssertLogCount(message, expectedAmount, Events.WorkflowTriggerEvent, task);
		}

		public static void AssertMilestoneFireCount(int expectedAmount, ProcessTask task) => AssertMilestoneFireCount(null, expectedAmount, task);

		#endregion
	}
}
