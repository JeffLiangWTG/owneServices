using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusSCAOceanBillWorkflowDescriptor : WorkflowDescriptor, IEventPublisher
	{
		public static class Constants
		{
			public const string Code = WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode;
			public static IMultilingualString Description { get { return ResString.GetMultilingualString("af59f9da-892f-4ef7-b6f7-c039e85825d7", "Sea Cargo"); } }
		}

		public override Func<ProcessTaskTemplate, OrgHeaderCollection> ClientListProvider
		{
			get { return (template) => new ShippingProviderCollection(template.Factory); }
		}

		public override ZString ClientName
		{
			get { return Res.GetString("d87c5c12-34fb-45f5-94e8-24be40085b3c", "Shipping Line"); }
		}

		public override string Code
		{
			get { return Constants.Code; }
		}

		public override IMultilingualString Description
		{
			get { return Constants.Description; }
		}

		public override ControllerID ControllerID
		{
			get { return null; } // TODO: Determine the controller ID - Same process task type used for different process tasks (one for each country)
		}

		public override ZString Port1Name
		{
			get { return Res.GetString("0e4cc709-e68e-4774-ac93-29888d639ae7", "Loading Port"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("0e7cf482-6e55-4ef2-9e0e-c6dc0d632a49", "Discharge Port"); }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			var result = MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)business, Core.Constants.CountryCodes.Australia))
			{
				result |= MessageRecipientPartyType.SeaCargoResponsibleParty;
			}
			return result;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return true;
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness business)
		{
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)business, Core.Constants.CountryCodes.Australia))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

		protected override IEnumerable<string> GetScheduleDeferredMessageSendTriggerActionsCore(IBaseTrigger trigger, IBusiness business)
		{
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)business, Core.Constants.CountryCodes.Australia))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
			}
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			var ocean = bizObj as BaseCusSCAOceanBill;
			if (ocean != null)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.SeaCargoResponsibleParty:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ocean.EffectiveResponsiblePartyOrgHeader as OrgHeader, ZString.Empty));
						break;
				}
			}
		}

		protected override void CheckWorkflowTriggerRecipientPartyCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
			base.CheckWorkflowTriggerRecipientPartyCore(trigger, parent, recipientPartyInfo);
			switch (recipientPartyInfo.Value.ToString())
			{
				case MessageRecipientPartyTypeList.Codes.SeaCargoResponsibleParty:
					var ocean = parent as BaseCusSCAOceanBill;
					if (ocean != null && ocean.EffectiveResponsiblePartyOrgHeader == null)
					{
						recipientPartyInfo.AddWarning(ocean.ReasonEffectiveResponsiblePartyIsUnavailable);
					}
					break;
			}
		}

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(BaseCusSCAOceanBill); }
		}

		#region Requirements

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		#endregion // Requirements

		#region IEventPublisher

		public string GetSubscriptionsQuery()
		{
			// any change to this code - please re-evaluate performance with functional testing
			return string.Format(CultureInfo.InvariantCulture,
				@"select SES_PK, CA_PK
	FROM dbo.CusSCAHouse
	LEFT JOIN dbo.CusSCAPivot ON CV_CA = CA_PK
	JOIN dbo.StmEventSubscription ON SES_RegistrarParentId = CA_CB
	WHERE SES_PublisherDescriptor = '{0}'
UNION ALL
	SELECT SES_PK, CV_PK
	FROM dbo.CusSCAHouse
	LEFT JOIN dbo.CusSCAPivot ON CV_CA = CA_PK
	JOIN dbo.StmEventSubscription ON SES_RegistrarParentId = CA_CB
	WHERE SES_PublisherDescriptor = '{0}'", Code);
		}

		public IEnumerable<string> GetPublisherTableNames()
		{
			yield return CusSCAHouseSchema.Constants.TableName;
			yield return CusSCAPivotSchema.Constants.TableName;
		}

		#endregion
	}
}
