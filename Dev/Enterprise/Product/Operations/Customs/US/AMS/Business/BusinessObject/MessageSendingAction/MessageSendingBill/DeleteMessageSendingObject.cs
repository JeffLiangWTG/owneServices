using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.AMS.Business
{
	public class DeleteMessageSendingObject : IACEBillManifestMessageAttachee, IACEBillOfLading, IPort
	{
		public DeleteMessageSendingObject(MessageSendingObject message)
		{
			this.message = Argument.NotNull(message, "message");

			InitDeleteMessageArguments();
		}

		readonly MessageSendingObject message;
		INPM01 inpm01;
		INPP01 inpp01;
		INPB04 inpb04;

		void InitDeleteMessageArguments()
		{
			var messages = MessageAttachee.Messages;
			CBPEDIMessage lastOutgoingMessage = null;
			var feedbackMessages = messages.GetMatchingMessages(EDIMessage.ApplicationCodes.AMS,
				new ZString[] { AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse },
				EDIMessage.Direction.Receive);

			if (feedbackMessages != null)
			{
				foreach (AMSEDIMessage msg in feedbackMessages)
				{
					if (!msg.MessageBlock.MessageBlocks.OfType<US.Messaging.Business.MessageBuildingBlocks.AMS.Common.TARW01>().Any())
					{
						var inpm02Block = msg.MessageBlock.MessageBlocks.OfType<IINPM02>().FirstOrDefault();
						if (inpm02Block != null && CarrierAssignedBatchNumberCreator.GetBillOfLading(inpm02Block) == MessageAttachee.BillOfLadingDetails.BillOfLadingSequenceNumber)
						{
							lastOutgoingMessage = msg.OriginalMessage;
							break;
						}
					}
				}
				if (lastOutgoingMessage != null)
				{
					inpm01 = lastOutgoingMessage.MessageBlock.MessageBlocks.OfType<INPM01>().FirstOrDefault();
					inpp01 = lastOutgoingMessage.MessageBlock.MessageBlocks.OfType<INPP01>().FirstOrDefault();
					inpb04 = lastOutgoingMessage.MessageBlock.MessageBlocks.OfType<INPB04>().FirstOrDefault();
				}
			}
		}

		IACEBillManifestMessageAttachee MessageAttachee
		{
			get { return message; }
		}

		IACEBillOfLading MessageBill
		{
			get { return message; }
		}

		IPort MessagePort
		{
			get { return message; }
		}

		#region DATA From Last Message

		ZString IManifestMessageAttachee.CarrierCode
		{
			get { return inpm01 == null ? MessageAttachee.CarrierCode : inpm01.CarrierCode; }
		}

		ZString IManifestMessageAttachee.ModeOfTransportationCode
		{
			get { return inpm01 == null ? MessageAttachee.ModeOfTransportationCode : inpm01.ModeOfTransportationCode; }
		}

		ZString IManifestMessageAttachee.ConveyanceCountryCode
		{
			get { return inpm01 == null ? MessageAttachee.ConveyanceCountryCode : inpm01.VesselCountryCode; }
		}

		ZString IManifestMessageAttachee.ConveyanceName
		{
			get
			{
				var result = MessageAttachee.ConveyanceName;
				if (inpm01 != null && !message.MB_VesselOverride)
				{
					result = inpm01.VesselName;
				}
				return result;
			}
		}

		ZString IManifestMessageAttachee.VoyageNumber
		{
			get { return inpm01 == null ? MessageAttachee.VoyageNumber : inpm01.VoyageNumber; }
		}

		ZString IManifestMessageAttachee.ManifestSequenceNumber
		{
			get { return inpm01 == null ? MessageAttachee.ManifestSequenceNumber : inpm01.ManifestSequenceNumber; }
			set { (this as IManifestMessageAttachee).ManifestSequenceNumber = value; }
		}

		ZString IManifestMessageAttachee.ConveyanceCode
		{
			get { return inpm01 == null ? MessageAttachee.ConveyanceCode : inpm01.VesselCode; }
		}

		ZString IPort.DistrictPortOfUnladingCode
		{
			get
			{
				ZString portOfUnlading;

				if (!message.MB_PortOfUnladingOverride.IsEmpty)
				{
					portOfUnlading = message.MB_PortOfUnladingOverride;
				}
				else if (inpp01 != null)
				{
					portOfUnlading = inpp01.PortOfUnladingCode;
				}
				else
				{
					portOfUnlading = MessagePort.DistrictPortOfUnladingCode;
				}

				return portOfUnlading;
			}
		}

		ZDate IPort.OriginalEstimatedDate
		{
			get { return inpp01 == null ? MessagePort.OriginalEstimatedDate : inpp01.OriginalEstimatedDate; }
		}

		ZString IManifestMessageAttachee.UniqueVoyageIdentifier
		{
			get
			{
				if (inpm01 == null)                 //inpb04 is not a mandatory block, but inpm01 is mandatory.
				{
					return MessageAttachee.UniqueVoyageIdentifier;
				}
				else if (inpb04 != null)
				{
					return inpb04.ReferenceIdentifier;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region IACEBillManifestMessageAttachee Members

		IACEBillOfLading IACEBillManifestMessageAttachee.BillOfLadingDetails
		{
			get { return this; }
		}

		ZDateTime IACEBillManifestMessageAttachee.EventDateTime
		{
			get { return MessageAttachee.EventDateTime; }
		}

		ZString IACEBillManifestMessageAttachee.ForeignDeparturePort
		{
			get { return MessageAttachee.ForeignDeparturePort; }
		}

		ZString IACEBillManifestMessageAttachee.BillPKAsString
		{
			get { return MessageAttachee.BillPKAsString; }
		}

		AMSBillEDIMessageCollection IACEBillManifestMessageAttachee.BillMessages
		{
			get { return MessageAttachee.BillMessages; }
		}

		#endregion

		#region ICommonBillManifestMessageAttachee Members

		void ICommonBillManifestMessageAttachee.UpdateOutgoingBillStatus()
		{
			MessageAttachee.UpdateOutgoingBillStatus();
		}

		IPort ICommonBillManifestMessageAttachee.PortDetails
		{
			get { return this; }
		}

		#endregion

		#region IManifestMessageAttachee Members

		ZString IManifestMessageAttachee.ApplicationCode
		{
			get { return MessageAttachee.ApplicationCode; }
		}

		ZString IManifestMessageAttachee.SupApplicationCode
		{
			get { return MessageAttachee.SupApplicationCode; }
		}

		ZString IManifestMessageAttachee.JobNumber
		{
			get { return MessageAttachee.JobNumber; }
		}

		ZBool IManifestMessageAttachee.IsPaperlessMIBParticipant
		{
			get { return MessageAttachee.IsPaperlessMIBParticipant; }
		}

		ZBool IManifestMessageAttachee.IsOutboundCargo
		{
			get { return MessageAttachee.IsOutboundCargo; }
		}

		ZString IManifestMessageAttachee.InBondNumber
		{
			get { return MessageAttachee.InBondNumber; }
		}

		void IManifestMessageAttachee.UpdatConveyanceEventInformation(ZString eventCode, ZDateTime eventDate)
		{
			MessageAttachee.UpdatConveyanceEventInformation(eventCode, eventDate);
		}

		void IManifestMessageAttachee.UpdateDispositionInformation(ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString dispositionCode, ZDateTime dispositionDate)
		{
			MessageAttachee.UpdateDispositionInformation(billOfLadingIssuerCode, billOfLadingNumber, dispositionCode, dispositionDate);
		}

		void IManifestMessageAttachee.UpdateIncomingBillStatus(ZString issuerCode, ZString billOfLading, ZString subtype, bool isFailure, CBPEDIMessage responseMessage)
		{
			MessageAttachee.UpdateIncomingBillStatus(issuerCode, billOfLading, subtype, isFailure, responseMessage);
		}

		void IManifestMessageAttachee.UpdateEstimatedDateOfArrival(ZDateTime estimatedDateOfArrival)
		{
			MessageAttachee.UpdateEstimatedDateOfArrival(estimatedDateOfArrival);
		}

		void IManifestMessageAttachee.LinkMessageToBill(ZString issuerCode, ZString billOfLading, CBPEDIMessage responseMessage)
		{
		}

		#endregion

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get { return MessageAttachee.Branch; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return MessageAttachee.MessageStatus; }
			set { (this as IMessageAttachee).MessageStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return MessageAttachee.Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return MessageAttachee.TopLevelBusinessObject; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return MessageAttachee.TopLevelBizObjReferenceNumber; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return MessageAttachee.TopLevelBusinessObjectLogs; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return MessageAttachee.Factory; }
		}

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return MessageAttachee.ControllerID; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return MessageAttachee.BusinessObjectPK; }
		}

		#endregion

		#region IPort Members

		ZInt IPort.NumberOfBillsOfLadingForPort
		{
			get { return 1; }
		}

		ZString IPort.FIRMSCode
		{
			get { return MessagePort.FIRMSCode; }
		}

		ZString IPort.Time
		{
			get { return MessagePort.Time; }
		}

		#endregion

		#region IBillOfLading Members

		IManifestMessageAttachee IBaseBillOfLading.MessageAttachee
		{
			get { return this; }
		}

		ZString ICommonBillOfLading.BillActionCode
		{
			get { return AMSBillSendingActionCodeList.Codes.DeleteBill; }
		}

		ZString ICommonBillOfLading.AmendmentCode
		{
			get { return MessageBill.AmendmentCode; }
		}

		ZString ICommonBillOfLading.IssuerCode
		{
			get { return MessageBill.IssuerCode; }
		}

		ZString ICommonBillOfLading.BillOfLadingSequenceNumber
		{
			get { return MessageBill.BillOfLadingSequenceNumber; }
		}

		ZString ICommonBillOfLading.ForeignPort
		{
			get { return MessageBill.ForeignPort; }
		}

		ZDecimal ICommonBillOfLading.ManifestQuantity
		{
			get { return MessageBill.ManifestQuantity; }
		}

		ZString ICommonBillOfLading.ManifestUnits
		{
			get { return MessageBill.ManifestUnits; }
		}

		ZDecimal ICommonBillOfLading.Weight
		{
			get { return MessageBill.Weight; }
		}

		ZString ICommonBillOfLading.WeightUnit
		{
			get { return MessageBill.WeightUnit; }
		}

		ZString ICommonBillOfLading.BillOfLadingStatusIndicator
		{
			get { return MessageBill.BillOfLadingStatusIndicator; }
		}

		ZBool ICommonBillOfLading.IsMasterInbond
		{
			get { return MessageBill.IsMasterInbond; }
		}

		ZString ICommonBillOfLading.HouseBillNumber
		{
			get { return MessageBill.HouseBillNumber; }
		}

		ZString ICommonBillOfLading.FIRMS
		{
			get { return MessageBill.FIRMS; }
		}

		ZDecimal ICommonBillOfLading.Volume
		{
			get { return MessageBill.Volume; }
		}

		ZString ICommonBillOfLading.VolumeUnit
		{
			get { return MessageBill.VolumeUnit; }
		}

		ZString ICommonBillOfLading.PlaceOfReceiptByCarrier
		{
			get { return MessageBill.PlaceOfReceiptByCarrier; }
		}

		ZString ICommonBillOfLading.SpaceCharterBLReference
		{
			get { return MessageBill.SpaceCharterBLReference; }
		}

		ZString ICommonBillOfLading.SecondNotifyParty1
		{
			get { return MessageBill.SecondNotifyParty1; }
		}

		ZString ICommonBillOfLading.SecondNotifyParty2
		{
			get { return MessageBill.SecondNotifyParty2; }
		}

		ZString ICommonBillOfLading.LastForeignPortBeforeDepartingForTheUS
		{
			get { return MessageBill.LastForeignPortBeforeDepartingForTheUS; }
		}

		ZString ICommonBillOfLading.ModeOfTransportationFromThePlacePriorToLoading
		{
			get { return MessageBill.ModeOfTransportationFromThePlacePriorToLoading; }
		}

		ZString ICommonBillOfLading.MethodOfPaymentForTransportation
		{
			get { return MessageBill.MethodOfPaymentForTransportation; }
		}

		ZString ICommonBillOfLading.ContractualPossessionForeignPort
		{
			get { return MessageBill.ContractualPossessionForeignPort; }
		}

		IEnumerable<IShipmentReferenceDetail> ICommonBillOfLading.ShipmentReferenceDetails(ActionCode actionCode)
		{
			return MessageBill.ShipmentReferenceDetails(actionCode);
		}

		IEnumerable<IEntity> IACEBillOfLading.Entities(ActionCode actionCode)
		{
			return MessageBill.Entities(actionCode);
		}

		IMovemenDetails IACEBillOfLading.MovemenDetails
		{
			get { return MessageBill.MovemenDetails; }
		}

		IEnumerable<IACEContainer> IACEBillOfLading.Containers
		{
			get { return MessageBill.Containers; }
		}

		#endregion

	}
}
