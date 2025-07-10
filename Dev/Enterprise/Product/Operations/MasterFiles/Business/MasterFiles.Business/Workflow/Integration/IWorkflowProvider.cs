using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowProviderCollection : IWorkflowProvider, IBusinessObjectCollection, IProcessTaskInitialiser
	{
		event EventHandler OnRebuild;
		IEnumerable<ProcessTask> Items { get; }
		IComparer GetDefaultOrderComparer();
		void Sort(IComparer comparer);
	}

	public interface ISometimesWorkflowProvider : IWorkflowProvider
	{
		bool ShouldSupportWorkflowTemplateApplication { get; }
	}

	public interface IWorkflowTemplateParameterProvider : IWorkflowProvider
	{
		TemplateApplicationParameters TemplateApplicationParameters { get; }
	}

	public interface IWorkflowProvider : IWorkflowProviderCore, IIdentified, IStmALogProvider
	{
		IProcessHeaderCollection Workflows { get; }
		ProcessTaskCollection WorkflowItems { get; }
		IWorkflowInformationProvider GetWorkflowInformationProvider();
	}

	public interface IWorkflowProviderIncludingRelated : IWorkflowProvider
	{
		IEnumerable<IWorkflowProvider> RelatedIWorkflowProviders { get; }
	}

	public interface IWorkflowProviderEvent : IWorkflowProvider
	{
		OrgHeader[] RecipientOrganisations { get; }
	}

	public interface IWorkflowProviderTemplateCriteria : IWorkflowProvider
	{
		ZGuid CompanyPK { get; }
	}

	public static class WorkflowProviderExtension
	{
		public static void CreateProcessTaskFromTemplate(this IWorkflowProvider provider, BusinessObjectFactory factory)
		{
			new ProcessTask.Loader(factory).CreateTasksAndMilestonesFromTemplateIfRequired(provider, TemplateApplicationParameters.ApplyIgnoreHasChanges());
		}

		public static void DisableProcessTasks(this IWorkflowProvider provider)
		{
			foreach (ProcessTask processTask in provider.WorkflowItems)
			{
				switch (processTask.P9_Type)
				{
					case Core.Constants.Workflow.MilestoneType:
						processTask.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
						processTask.TriggerConditions.TriggerConditionValue = (NoResString)"false";
						if (processTask.P9_MilestoneExceptionAdded.IsEmpty)
						{
							processTask.P9_MilestoneExceptionAdded = ZDateTime.UtcNow;
						}
						break;

					case Core.Constants.Workflow.WorkflowTriggerType:
						processTask.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
						processTask.TriggerConditions.TriggerConditionValue = (NoResString)"false";
						break;

					case Core.Constants.Workflow.ExceptionType:
						processTask.IsExceptionActioned = true;
						break;

					default:
						if (processTask.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned || processTask.P9_Status == ProcessTaskStatusCodeList.Codes.Open)
						{
							processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
						}
						else if (processTask.P9_Status == ProcessTaskStatusCodeList.Codes.Working || processTask.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
						{
							processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
						}
						break;
				}
			}
		}
	}

	public interface IWorkflowInformationProvider
	{
		ZString Destination { get; }
		ZString Origin { get; }
		TrackingConstants.BusinessContext BusinessContext { get; }
		IEnumerable<ZGuid> Companies { get; }
	}

	public class WorkflowInformationProvider : IWorkflowInformationProvider
	{
		public WorkflowInformationProvider(ZGuid[] companies)
		{
			Companies = companies;
		}

		public ZString Destination { get; set; }
		public ZString Origin { get; set; }
		public TrackingConstants.BusinessContext BusinessContext { get; set; }
		public IEnumerable<ZGuid> Companies { get; private set; }
	}

	public static class IJobInvoicingPlugInWorkflowExtensions
	{
		public static ColumnValueRanker GetJobRelatedTemplateSelectionCriteria(this IJobInvoicingPlugIn workflowProvider)
		{
			ColumnValueRanker result = new ColumnValueRanker();
			JobHeader job = new JobHeader.Loader(workflowProvider).Load(true, false);

			// Branch and Department
			result.Add(ProcessTaskTemplateSchema.P0_GB, (job != null) ? job.JH_GB : ZGuid.Empty, null);
			result.Add(ProcessTaskTemplateSchema.P0_GE, (job != null) ? job.JH_GE : ZGuid.Empty, null);

			// Client
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder(job, workflowProvider));

			// Ports
			if (workflowProvider.InvoicingSupporter.Origin != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, workflowProvider.InvoicingSupporter.Origin.Code, workflowProvider.InvoicingSupporter.Origin.Country?.Code ?? ZString.Empty, ZString.Empty);
			}
			else
			{
				result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, ZString.Empty);
			}
			if (workflowProvider.InvoicingSupporter.Destination != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, workflowProvider.InvoicingSupporter.Destination.Code, workflowProvider.InvoicingSupporter.Destination.Country?.Code ?? ZString.Empty, ZString.Empty);
			}
			else
			{
				result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, ZString.Empty);
			}

			return result;
		}

		static IZType[] GetClientsInTemplateSelectionOrder(JobHeader job, IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			List<IZType> result = new List<IZType>();

			if (jobInvoicingPlugIn.InvoicingSupporter.IsImport)
			{
				if (jobInvoicingPlugIn.InvoicingSupporter.Consignee != null)
				{
					result.Add(jobInvoicingPlugIn.InvoicingSupporter.Consignee.PK);
				}

				if (jobInvoicingPlugIn.InvoicingSupporter.Consignor != null)
				{
					result.Add(jobInvoicingPlugIn.InvoicingSupporter.Consignor.PK);
				}
			}
			else
			{
				if (jobInvoicingPlugIn.InvoicingSupporter.Consignor != null)
				{
					result.Add(jobInvoicingPlugIn.InvoicingSupporter.Consignor.PK);
				}

				if (jobInvoicingPlugIn.InvoicingSupporter.Consignee != null)
				{
					result.Add(jobInvoicingPlugIn.InvoicingSupporter.Consignee.PK);
				}
			}

			if (job != null)
			{
				result.Add(job.LocalChargesPK);
			}
			result.Add(ZGuid.Empty);

			return result.ToArray();
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TestExcludeWorkflowProviderHasTestCaseAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class WorkflowSetFieldReadonlyCheckBypass : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class WorkflowSetFieldReadonly : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DisableWorkflowSettingPropertiesAfterOnSavingAttribute : Attribute
	{
	}
}
