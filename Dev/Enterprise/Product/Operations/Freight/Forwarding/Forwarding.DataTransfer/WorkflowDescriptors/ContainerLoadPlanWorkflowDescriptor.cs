using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ContainerLoadPlanWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Forwarding|ContainerLoadPlanWorkflowDescriptorCode|Description", "Container Load Plan");

		public override ControllerID ControllerID => ControllerIDs.ContainerLoadPlan;

		public override Type WorkflowProviderType => typeof(CFSContainerLoadList);

		protected override bool SupportsReleaseGroupRulesCore => false;

		public override bool SupportsBufferManagement => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return false; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.ContainerLoadPlan }; }
		}

		#region Requires Port

		public override bool RequiresPort1 => true;

		public override bool RequiresPort2 => true;

		#endregion

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback
		{
			get { return false; }
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("0282C502-601F-47FA-8647-CE244EB3A87D", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("A9F90C6E-7753-474F-91A1-F9873DD475F1", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.Email;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var entity = bizObj as CFSContainerLoadList;

			if (entity != null)
			{
				if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(entity.LoadListParty, ZString.Empty));
				}
			}
			else
			{
				base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			}
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SetField, WorkflowTriggerActionTypeConstants.Descriptions.SetField);

			return result;
		}

		protected override ValidationToolSettings GetValidationToolSettings() => new ContainerLoadPlanValidationToolSettings(this);

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => false;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsScreenLayout => false;		
	}
}
