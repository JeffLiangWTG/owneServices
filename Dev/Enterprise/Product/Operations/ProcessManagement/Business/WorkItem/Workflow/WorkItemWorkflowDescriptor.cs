using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemWorkflowDescriptor : WorkflowDescriptor
	{
		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WorkItem; }
		}

		#region ID / Description

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.WorkItem.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.WorkItem.MultilingualDescription; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(WorkItem); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				// Must match WorkItem.GetTemplateSelectionCriteria
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>();
				result.Add(new ProcessTemplateSubType(ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value, CreateActiveTypes));
				result.Add(new ProcessTemplateSubType(ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value, CreateActiveAreas));
				result.Add(new ProcessTemplateSubType(ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value, CreateActiveActivityTypes));
				result.Add(new ProcessTemplateSubType(ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value, CreateActiveActivitySubtypes));
				result.Add(new ProcessTemplateSubType(ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value, CreateActivePriorities));
				return result.ToArray();
			}
		}

		CodeDescriptionPairList CreateActiveTypes()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkItemActualLookups.GetActiveTypes(LastProcessTaskTemplate.Factory);
			}
			else
			{
				return WorkItemActualLookups.CreateWorkItemTypeList(true);
			}
		}

		CodeDescriptionPairList CreateActiveAreas()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkItemActualLookups.GetAreas(LastProcessTaskTemplate.Factory, LastProcessTaskTemplate.P0_SubType1, true);
			}
			else
			{
				return WorkItemActualLookups.CreateAreas(ZString.Empty, true);
			}
		}

		CodeDescriptionPairList CreateActiveActivityTypes()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkItemActualLookups.GetActivityTypeList(LastProcessTaskTemplate.Factory, LastProcessTaskTemplate.P0_SubType1, LastProcessTaskTemplate.P0_SubType2, true);
			}
			else
			{
				return WorkItemActualLookups.CreateActivityTypeList(ZString.Empty, ZString.Empty, true);
			}
		}

		CodeDescriptionPairList CreateActiveActivitySubtypes()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkItemActualLookups.GetActivitySubtypeList(LastProcessTaskTemplate.Factory,
					LastProcessTaskTemplate.P0_SubType1,
					LastProcessTaskTemplate.P0_SubType2,
					LastProcessTaskTemplate.P0_SubType3, true);
			}
			else
			{
				return WorkItemActualLookups.CreateActivitySubtypeList(ZString.Empty, ZString.Empty, ZString.Empty, true);
			}
		}

		CodeDescriptionPairList CreateActivePriorities()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkItemActualLookups.GetPriorities(LastProcessTaskTemplate.Factory, true,
					LastProcessTaskTemplate.P0_SubType1,
					LastProcessTaskTemplate.P0_SubType2,
					LastProcessTaskTemplate.P0_SubType3,
					LastProcessTaskTemplate.P0_SubType4);
			}
			else
			{
				return WorkItemActualLookups.CreatePriorities(true, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			}
		}

		#endregion

		public override bool RequiresDepartment { get { return true; } }
		public override bool RequiresPort1 { get { return true; } }
		public override ZString Port1Name { get { return WorkItemFormCustomisationSettingsProvider.PortOrCountryLabel; } }
		public override bool RequiresClient { get { return false; } }
		public override bool AreTasksCompanySpecific { get { return false; } }
		public override bool SupportsEventTracking { get { return true; } }

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new WorkItemFormCustomisationSettingsProvider();
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.WorkItem }; }
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null) => new SchemaColumn[]
		{
			WorkItemSchema.WKI_ActivitySubtype,
			WorkItemSchema.WKI_ActivityType,
			WorkItemSchema.WKI_GB_AssignedBranch,
			WorkItemSchema.WKI_GC_AssignedCompany,
			WorkItemSchema.WKI_GE_AssignedDepartment,
			WorkItemSchema.WKI_PortOrCountry,
			WorkItemSchema.WKI_Priority,
			WorkItemSchema.WKI_Status,
			WorkItemSchema.WKI_Summary,
			WorkItemSchema.WKI_WorkItemArea,
			WorkItemSchema.WKI_WorkItemType,
		};
	}
}
