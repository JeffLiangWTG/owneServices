using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ReconWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.ReconWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("2A23719E-0447-483F-BDCB-9D7D1FD599C9", "Reconciliation declaration job"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(JobDeclaration); }
		}

		public override bool SupportsBufferManagement => false;

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.Recon; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.ReconDeclaration }; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsUniversalTemplates => false;

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return false; }
		}

		public override bool RequiresPort2
		{
			get { return false; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				USAddInfoSchema.US_EstimatedEntryDate,
				USAddInfoSchema.US_IssueCode,
				USAddInfoSchema.US_SuretyCode
			};
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			var fieldName = fieldColumn.Name;
			var fieldColumnDescription = string.Empty;
			switch (fieldName)
			{
				case USAddInfoSchema.Constants.US_EstimatedEntryDate:
					fieldColumnDescription = Res.GetString("3BCF67A6-E846-4756-856B-48616EC4CDDB", "Estimated Recon. Date");
					break;
				case USAddInfoSchema.Constants.US_IssueCode:
					fieldColumnDescription = Res.GetString("16B96567-D5BF-4519-A168-949BE6F7DAF9", "Issue Code");
					break;
				case USAddInfoSchema.Constants.US_SuretyCode:
					fieldColumnDescription = Res.GetString("62766575-C70D-4787-AC67-7C8C13989EB2", "Surety Code");
					break;
				default:
					fieldColumnDescription = base.GetFieldColumnDescription(factory, fieldColumn);
					break;
			}
			return fieldColumnDescription;
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			return new string[] { WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging };
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendEntryDeclarationMessage);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			if (result != null)
			{
				return result;
			}
			var action = source.Action;
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage)
			{
				if (source.Job is Integration.Customs.IJobDeclarationAutoSendingMessageSupporter supporter)
				{
					return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
				}
			}
			return result;
		}

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return new ReconDeclaration(factory.New<JobDeclaration>());
		}

#endif
	}
}
