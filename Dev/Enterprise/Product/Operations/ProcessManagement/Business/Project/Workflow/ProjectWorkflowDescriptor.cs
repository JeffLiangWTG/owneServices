using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description / ControllerID

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.Project.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.Project.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Project; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(Project); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				// Must match Project.GetTemplateSelectionCriteria
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>();
				result.Add(GetSubType1Template(ProcessManagementRegistry.Instance.ProjectTypeLabel.Value));
				result.Add(GetSubType2Template(ProcessManagementRegistry.Instance.ProjectSubtypeLabel.Value));
				result.Add(GetSubType3Template(ProcessManagementRegistry.Instance.ProjectModuleLabel.Value));
				result.Add(GetSubType4Template(ProcessManagementRegistry.Instance.ProjectPriorityLabel.Value));
				return result.ToArray();
			}
		}

		protected ProcessTemplateSubType GetSubType1Template(string description)
		{
			return new ProcessTemplateSubType(description, CreateActiveTypes);
		}

		protected ProcessTemplateSubType GetSubType2Template(string description)
		{
			return new ProcessTemplateSubType(description, CreateActiveSubtypes);
		}

		protected ProcessTemplateSubType GetSubType3Template(string description)
		{
			return new ProcessTemplateSubType(description, CreateActiveModules);
		}

		protected ProcessTemplateSubType GetSubType4Template(string description)
		{
			return new ProcessTemplateSubType(description, CreateActivePriorities);
		}

		CodeDescriptionPairList CreateActiveTypes()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkProjectLookups.GetActiveTypes(LastProcessTaskTemplate.Factory);
			}
			else
			{
				return WorkProjectLookups.CreateTypeList(true);
			}
		}

		CodeDescriptionPairList CreateActiveSubtypes()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkProjectLookups.GetSubtypes(LastProcessTaskTemplate.Factory, LastProcessTaskTemplate.P0_SubType1, true);
			}
			else
			{
				return WorkProjectLookups.CreateSubtypeList(ZString.Empty, true);
			}
		}

		CodeDescriptionPairList CreateActiveModules()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkProjectLookups.GetModules(LastProcessTaskTemplate.Factory, LastProcessTaskTemplate.P0_SubType1, LastProcessTaskTemplate.P0_SubType2, true);
			}
			else
			{
				return WorkProjectLookups.CreateModuleList(ZString.Empty, ZString.Empty, true);
			}
		}

		CodeDescriptionPairList CreateActivePriorities()
		{
			if (LastProcessTaskTemplate != null)
			{
				return WorkProjectLookups.GetPriorities(LastProcessTaskTemplate.Factory, LastProcessTaskTemplate.P0_SubType1, LastProcessTaskTemplate.P0_SubType2, LastProcessTaskTemplate.P0_SubType3, true);
			}
			else
			{
				return WorkProjectLookups.CreatePriorityList(ZString.Empty, ZString.Empty, ZString.Empty, true);
			}
		}

		#endregion

		public override bool RequiresClient { get { return false; } }
		public override bool AreTasksCompanySpecific { get { return false; } }
		public override bool SupportsEventTracking { get { return true; } }

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new ProjectFormCustomisationSettingsProvider();
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Project }; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				var project = (Project)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(project.ClientOrganisation, ZString.Empty));
			}
			else
			{
				base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			}
		}
	}
}
