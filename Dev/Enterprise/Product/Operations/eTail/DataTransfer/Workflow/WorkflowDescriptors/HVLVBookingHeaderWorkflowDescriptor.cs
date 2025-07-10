using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVBookingHeaderWorkflowDescriptor : WorkflowDescriptor, IEventSubscriptionAgent
	{
		#region ID / Description

		public override string Code => WorkflowDescriptors.HVLVBookingHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("eTail|HVLVBookingHeaderWorkflowDescriptor|Description", "HVLV Booking Header");

		#endregion

		#region Requirements

		public override bool RequiresClient => true;

		public override ZString ClientName => Res.GetString("fabe55cd-625a-4ed9-a136-f8ae8ae5aba5", "eTailer");

		public override bool RequiresPort1 => true;

		public override ZString Port1Name => Res.GetString("dee91ac3-222b-49cd-9a1b-dd2b322b2d71", "Dispatch UNLOCO");

		public override bool RequiresPort2 => false;

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => false;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(GetServiceLevelSubType());

				return list.ToArray();
			}
		}

		internal static ProcessTemplateSubType GetServiceLevelSubType() => new ProcessTemplateSubType(Res.GetString("b7597d14-fe3d-48e8-85cf-3ffe5c7ddaee", "Service Level"), GetServiceLevels());

		static CodeDescriptionPairList GetServiceLevels()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("", Res.GetString("2d677ec4-455e-4d69-a437-3c022dc9cb4a", "All"));

			var serviceLevels = new RefServiceLevelCollection(new BusinessObjectFactory());

			foreach (var serviceLevel in serviceLevels)
			{
				result.AddPair(serviceLevel.RS_Code, serviceLevel.RS_DescriptionMultilingual);
			}

			return result;
		}

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsUniversalTemplates => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsCreateTransportBooking => true;

		#endregion

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			var result = MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;

			return result;
		}

		public override Type WorkflowProviderType => typeof(HVLVBookingHeader);

		public override ControllerID ControllerID => ControllerIDs.HVLVBookingHeader;

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			var bookingHeader = (HVLVBookingHeader)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.BillToParty:
					if (bookingHeader != null)
					{
						messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(bookingHeader.BillToParty));
					}

					break;
			}
		}

		#region Pre-Screening

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			var bookingHeader = (HVLVBookingHeader)source.Job;

			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening)
			{
				return ObjectFactory.Get<IProcessor>("HVLVPreScreeningProcessor", bookingHeader);
			}
			else
			{
				return base.GetWorkflowTriggerActionCore(source);
			}
		}

		public override bool SupportsHVLVPreScreening => true;

		#endregion

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
		}

		#region IEventSubscriptionAgent

		IEnumerable<IStmALogParent> IEventSubscriptionAgent.GetLogParentsToFireWorkflowForEvent(IStmEventSubscription subscription, IStmALog evnt)
		{
			Argument.NotNull(subscription, nameof(subscription));
			Argument.NotNull(evnt, nameof(evnt));

			if (subscription.SES_PublisherDescriptor == WorkflowDescriptors.CustomsHouseAirCargoCode)
			{
				var cusHAWBSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusHAWB>(), CusHAWBSchema.CS_HAWB);
				cusHAWBSubQuery.AddToFilter(CusHAWBSchema.PK, evnt.SL_Parent);

				var consignmentQuery = new ZDBOnlyQuery(typeof(HVLVConsignment));
				consignmentQuery.AddToFilter(HVLVConsignmentSchema.HVC_HVH_BookingHeader, subscription.SES_AgentParentId);
				consignmentQuery.AddSubQuery(HVLVConsignmentSchema.HVC_ConsignmentId, cusHAWBSubQuery, JoinCondition.And);

				return evnt.Factory.Load<HVLVConsignment>(consignmentQuery);
			}
			else if (subscription.SES_PublisherDescriptor == WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode)
			{
				var cusSCAPivotSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAPivot>(), CusSCAPivotSchema.CV_CA);
				cusSCAPivotSubQuery.AddToFilter(CusSCAPivotSchema.PK, evnt.SL_Parent);

				var cusSCAHouseSubQueryByPivot = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAHouse>(), CusSCAHouseSchema.CA_HouseBill);
				cusSCAHouseSubQueryByPivot.AddSubQuery(cusSCAPivotSubQuery, JoinCondition.And);

				var cusSCAHouseSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusSCAHouse>(), CusSCAHouseSchema.CA_HouseBill);
				cusSCAHouseSubQuery.AddToFilter(CusSCAHouseSchema.PK, evnt.SL_Parent);
				cusSCAHouseSubQuery.AddAsUnionQuery(cusSCAHouseSubQueryByPivot);

				var consignmentQuery = new ZDBOnlyQuery(typeof(HVLVConsignment));
				consignmentQuery.AddToFilter(HVLVConsignmentSchema.HVC_HVH_BookingHeader, subscription.SES_AgentParentId);
				consignmentQuery.AddSubQuery(HVLVConsignmentSchema.HVC_ConsignmentId, cusSCAHouseSubQuery, JoinCondition.And);

				return evnt.Factory.Load<HVLVConsignment>(consignmentQuery);
			}

			return Enumerable.Empty<HVLVConsignment>();
		}

		#endregion
	}
}
