using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class StatementWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code => StatementProcessTask.StatementWorkflow.Code;

		public override IMultilingualString Description => StatementProcessTask.StatementWorkflow.Description;

		public override ZString MilestoneTemplateHintCaption => string.Empty;

		#endregion

		#region Criteria Requirements

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => true;

		public override bool RequiresPort1 => false;

		public override bool RequiresPort2 => false;

		#endregion

		#region Workflow Trigger

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null) => new SchemaColumn[]
		{
			CusStatementHeaderSchema.B2_ProcessDate,
			CusStatementHeaderSchema.B2_ProcessPort,
			CusStatementHeaderSchema.B2_PaymentType,
			CusStatementHeaderSchema.B2_PrintDate,
			CusStatementHeaderSchema.B2_DueDate,
			CusStatementHeaderSchema.B2_PaymentAuthorizationDate,
			CusStatementHeaderSchema.B2_Status
		};

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.OrgProxy;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			var statement = (BaseCusStatementHeader)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(statement.Importer, ZString.Empty));
			}
		}

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.CustomsStatementHdr };

		#endregion

		public override Type WorkflowProviderType => typeof(BaseCusStatementHeader);

		public override bool SupportsEventTracking => true;

		public override ControllerID ControllerID => ControllerIDs.Customs.CustomsStatement;
	}
}
