using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class HouseAirCargoWorkflowDescriptor : WorkflowDescriptor, IEventPublisher
	{
		#region ID / Description / ControllerID

		public override string Code
		{
			get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("56fbd5c3-784d-48a1-abb1-42851f3b42e3", "House Air Cargo"); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("C53694C3-5A8B-46C1-8258-3112189BFD61", "Airline"), () => { return AirlineList; }));
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

		#region Port Names

		public override ZString Port1Name
		{
			get { return Res.GetString("120119C0-B80F-4703-89D1-0D29F8408BA0", "Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("E781C14B-3F07-4177-9F44-35390A2F1215", "Destination"); }
		}

		#endregion

		#region Requires Client / Ports

		public override bool RequiresPort1
		{
			get { return true; }
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

		#region SupportedTriggerPartyServicesCore

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

		#endregion

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CusHAWB }; }
		}

		public override Type WorkflowProviderType
		{
			get
			{
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.Australia:
						return ObjectFactory.GetType<Integration.Customs.AU.ICusHAWB>();
					case Core.Constants.CountryCodes.NewZealand:
						return ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>();
					case Core.Constants.CountryCodes.UnitedKingdom:
						return ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusHAWB>();
					default:
						return typeof(CusHAWB);
				}
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
						result = ControllerIDs.Customs.AU.HouseAirCargo;
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

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			var result = MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
			var bizo = (BusinessObject)business;
			if (trigger.IsForCountryOrTemplateDischargePortCountry(bizo, Core.Constants.CountryCodes.Australia))
			{
				result |= MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.DeConsolidator |
					MessageRecipientPartyType.AirCargoResponsibleParty;
			}
			else if (trigger.IsForCountryOrTemplateDischargePortCountry(bizo, Core.Constants.CountryCodes.UnitedKingdom))
			{
				result |= MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
			}

			return result | MessageRecipientPartyType.ArrivalTransitWarehouse;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			var bizo = (BusinessObject)parent;
			if (trigger.IsForCountryOrTemplateDischargePortCountry(bizo, Core.Constants.CountryCodes.Australia) || trigger.IsForCountryOrTemplateDischargePortCountry(bizo, Core.Constants.CountryCodes.NewZealand))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return true;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			var hawb = bizObj as CusHAWB;
			var mawb = hawb == null ? null : hawb.MAWB;
			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb != null ? mawb.DeConsolidatorOrgHeader as OrgHeader : null, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb != null ? mawb.EffectiveResponsiblePartyOrgHeader as OrgHeader : null, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(mawb.UnpackDepotAddress?.Header, ZString.Empty));
					break;
			}
		}

		protected override void CheckWorkflowTriggerRecipientPartyCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo recipientPartyInfo)
		{
			base.CheckWorkflowTriggerRecipientPartyCore(trigger, parent, recipientPartyInfo);
			switch (recipientPartyInfo.Value.ToString())
			{
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					var hawb = parent as CusHAWB;
					var mawb = hawb == null ? null : hawb.MAWB;
					if (mawb != null && mawb.EffectiveResponsiblePartyOrgHeader == null)
					{
						recipientPartyInfo.AddWarning(mawb.ReasonEffectiveResponsiblePartyIsUnavailable);
					}
					break;
			}
		}

		public override bool SupportsBufferManagement => true;

		protected override bool SupportsTaskLineTriggersCore => false;

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			if (trigger.IsForCountryOrTemplateDischargePortCountry((BusinessObject)parent, Core.Constants.CountryCodes.Australia))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage,
					WorkflowTriggerActionTypeConstants.Descriptions.SendWithdrawalMessage);
			}
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var action = source.Action;
			var trigger = source.Trigger;
			if (trigger.IsForCountryOrTemplateDischargePortCountry(source.Job, Core.Constants.CountryCodes.Australia)
				&& action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage
				&& queuedLog != null)
			{
				var hawb = (CusHAWB)source.Job;
				return ObjectFactory.New<Integration.Customs.AU.ISendWithdrawalMessageProcessor>(hawb) as IProcessor;
			}

			return base.GetWorkflowTriggerActionCore(source, queuedLog);
		}

		#region IEventsPublisher

		public string GetSubscriptionsQuery()
		{
			// any change to this code - please re-evaluate performance with functional testing
			var query = @"
				SELECT
					SES_PK, CS_PK
				FROM 
					dbo.CusHAWB
				JOIN 
					dbo.StmEventSubscription ON SES_RegistrarParentId = CS_CM
				WHERE 
					SES_PublisherDescriptor = '@DescriptorCode'";

			query = query
				.Replace("@DescriptorCode", Code);

			return query;
		}

		public IEnumerable<string> GetPublisherTableNames()
		{
			yield return CusHAWBSchema.Constants.TableName;
		}

		#endregion
	}
}
