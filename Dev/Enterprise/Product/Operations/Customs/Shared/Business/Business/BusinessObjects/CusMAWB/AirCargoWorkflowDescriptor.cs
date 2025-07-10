using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class AirCargoWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.CusMAWB.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.CusMAWB.MultilingualDescription; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("C53694C3-5A8B-46C1-8258-3112189BFD61", "Airline"), delegate
				{ return AirlineList; }));
				return list.ToArray();
			}
		}

		public CodeDescriptionPairList AirlineList
		{
			get
			{
				if (fAirlineList == null)
				{
					fAirlineList = new CodeDescriptionPairList();

					RefAirlineCollection airlines = new RefAirlineCollection(new BusinessObjectFactory(), new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.NotEqual, ""));
					airlines.ApplySort(RefAirlineSchema.RM_TwoCharacterCode.Name, ListSortDirection.Descending);

					foreach (RefAirline airline in airlines)
					{
						fAirlineList.AddPair(airline.RM_TwoCharacterCode, airline.RM_AirlineName1);
					}
				}
				return fAirlineList;
			}
		}

		CodeDescriptionPairList fAirlineList;

		#endregion

		#region Requires Client / Ports

		public override bool RequiresPort1
		{
			get { return false; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return true; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		#endregion

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CusMAWB, BusinessContext.CusHAWB }; }
		}

		public override Type WorkflowProviderType => TypeDecider.GetTypeForCountry(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		CusMAWBTypeDecider TypeDecider => typeDecider ?? (typeDecider = new CusMAWBTypeDecider());
		CusMAWBTypeDecider typeDecider;

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return trigger == null || !trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)bizo, Core.Constants.CountryCodes.UnitedKingdom);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			var result = MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)business, Core.Constants.CountryCodes.Australia))
			{
				result |= MessageRecipientPartyType.DeConsolidator |
							MessageRecipientPartyType.AirCargoResponsibleParty;
			}
			return result | MessageRecipientPartyType.ArrivalTransitWarehouse;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsBufferManagement => true;

		protected override bool SupportsTaskLineTriggersCore => false;

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			var bizo = (BusinessObject)parent;
			if (trigger.IsForCountryOrTemplateDischargePortCountry(bizo, Core.Constants.CountryCodes.Australia))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
				yield return WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn;
			}
			else if (trigger.IsForCountryOrTemplateDischargePortCountry(bizo, Core.Constants.CountryCodes.NewZealand))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

		protected override IEnumerable<string> GetScheduleDeferredMessageSendTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)parent, Core.Constants.CountryCodes.Australia))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage;
			}
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			var mawb = bizObj as CusMAWB;
			if (mawb != null)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.DeConsolidator:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb.DeConsolidatorOrgHeader as OrgHeader, ZString.Empty));
						break;
					case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb.EffectiveResponsiblePartyOrgHeader as OrgHeader, ZString.Empty));
						break;
					case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb.UnpackDepotAddress?.Header, ZString.Empty));
						break;
				}
			}
		}

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.SupportedTriggerPartyServicesCore(recipient);
			}
		}

		protected override void CheckWorkflowTriggerRecipientPartyCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
			base.CheckWorkflowTriggerRecipientPartyCore(trigger, parent, recipientPartyInfo);

			var mawb = parent as CusMAWB;
			switch (recipientPartyInfo.Value.ToString())
			{
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					if (mawb != null && mawb.EffectiveResponsiblePartyOrgHeader == null)
					{
						recipientPartyInfo.AddWarning(mawb.ReasonEffectiveResponsiblePartyIsUnavailable);
					}
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					if (mawb != null && mawb.UnpackDepotAddress == null)
					{
						recipientPartyInfo.AddWarning(mawb.ReasonCFSIsUnavailable);
					}

					break;
			}
		}

		public override ControllerID ControllerID
		{
			get
			{
				ControllerID result;
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.Australia:
						result = ControllerIDs.Customs.AU.AirCargo;
						break;
					case Core.Constants.CountryCodes.NewZealand:
						result = ControllerIDs.Customs.NZ.ExpressECI;
						break;
					case Core.Constants.CountryCodes.UnitedKingdom:
						result = ControllerIDs.Customs.GB.CcsukAirInventory;
						break;
					default:
						result = ControllerIDs.Customs.BaseAirCargo;
						break;
				}
				return result;
			}
		}
	}

	public class DeferredMessageSchedulingProcessor : IProcessor, Integration.Customs.Shared.IDeferredMessageSchedulingProcessor
	{
		public DeferredMessageSchedulingProcessor(BusinessObject job, ProcessTaskNotification triggerAction, ZString queuedUserNK)
		{
			this.job = job;
			this.triggerAction = triggerAction;
			this.queuedUserNK = queuedUserNK;
		}
		readonly BusinessObject job;
		readonly ProcessTaskNotification triggerAction;
		readonly ZString queuedUserNK;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var supporter = GetTriggerActionMessagingSupporter();
			if (supporter == null)
			{
				notifications.AddWarning(Res.GetString("9d798e4c-45ba-4d7c-9007-800b482b570e", "Could not find the parent Cargo Report for Trigger Action with PK:{0}", triggerAction.PK.ToString()));
				return;
			}

			supporter.SendMessage(notifications, queuedUserNK, triggerAction.PQ_TriggerType.ToString());
		}

		ITriggerActionMessagingSupporter GetTriggerActionMessagingSupporter()
		{
			var entity = job as ITriggerActionMessagingSupporterProvider;
			return entity?.GetSupporter(triggerAction.PQ_TriggerType.ToString());
		}
	}
}
