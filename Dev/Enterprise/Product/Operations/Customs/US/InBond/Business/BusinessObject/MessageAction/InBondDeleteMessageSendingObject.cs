using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondDeleteMessageSendingObject : IInBondQPHeader
	{
		public InBondDeleteMessageSendingObject(IInBondMessagingHeader moveHeader)
		{
			this.moveHeader = moveHeader;
			InitDeleteMessageArguments(moveHeader);
		}

		readonly IInBondQPHeader moveHeader;
		INBQP10 inbqp10;

		void InitDeleteMessageArguments(IInBondMessagingHeader moveHeader)
		{
			var messages = moveHeader.Messages;
			var lastOutgoingMessage = (MQEDIMessage)messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondTransaction, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, EM_MessageSubTypeList.Codes.InBondDepartureOriginal);
			if (lastOutgoingMessage != null)
			{
				inbqp10 = lastOutgoingMessage.MessageBlock.MessageBlocks.OfType<INBQP10>().FirstOrDefault();
			}
		}

		ZString IInBondQPHeader.EntryType
		{
			get { return inbqp10 == null ? moveHeader.EntryType : inbqp10.InbondEntryType; }
		}

		ZString IInBondQPHeader.InBondNumber
		{
			get { return inbqp10 == null ? moveHeader.InBondNumber : inbqp10.InbondNumber; }
		}

		ZString IInBondQPHeader.InbondCarrierSCAC
		{
			get { return inbqp10 == null ? moveHeader.InbondCarrierSCAC : inbqp10.CarrierCode; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return moveHeader.Factory; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return moveHeader.MessageStatus; }
			set { moveHeader.MessageStatus = value; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return moveHeader.TransportMode; }
		}

		#region No Modify Implementation Members

		ZBool IInBondQPHeader.BTAIndicator
		{
			get { return moveHeader.BTAIndicator; }
		}

		ZBool IInBondQPHeader.IsAir
		{
			get { return moveHeader.IsAir; }
		}

		IEnumerable<IInBondBillDetails> IInBondQPHeader.Bills
		{
			get { return moveHeader.Bills; }
		}

		ZDateTime IInBondQPHeader.ETAatUnlading
		{
			get { return moveHeader.ETAatUnlading; }
		}

		ZString IInBondQPHeader.FTZFirmsCode
		{
			get { return moveHeader.FTZFirmsCode; }
		}

		ZString IInBondQPHeader.InbondCarrierSCACOrFirms
		{
			get { return ((IInBondQPHeader)this).FTZFirmsCode; }
		}

		ZBool IInBondQPHeader.FTZIndicator
		{
			get { return moveHeader.FTZIndicator; }
		}

		IInBondBillDetails IInBondQPHeader.FindBillMatchingNumber(ZString masterBillNumber, ZString houseBillNumber)
		{
			return moveHeader.FindBillMatchingNumber(masterBillNumber, houseBillNumber);
		}

		IInBondBillDetails IInBondQPHeader.FindBillMatchingSeqNo(ZString sequenceNumber)
		{
			return moveHeader.FindBillMatchingSeqNo(sequenceNumber);
		}

		IInBondContainer IInBondQPHeader.FindContainer(ZString containerNumber, ZString billNumber)
		{
			return moveHeader.FindContainer(containerNumber, billNumber);
		}

		ZString IInBondQPHeader.ForeignDestination
		{
			get { return moveHeader.ForeignDestination; }
		}

		ZString IInBondQPHeader.ImportTransportMode
		{
			get { return moveHeader.ImportTransportMode; }
		}

		ZString IInBondQPHeader.ImportingCarrierCountryCode
		{
			get { return moveHeader.ImportingCarrierCountryCode; }
		}

		ZString IInBondQPHeader.ImportingCarrierSCAC
		{
			get { return moveHeader.ImportingCarrierSCAC; }
		}

		ZString IInBondQPHeader.ImportingCarrierVoyageNumber
		{
			get { return moveHeader.ImportingCarrierVoyageNumber; }
		}

		ZString IInBondQPHeader.ImportingConveyanceName
		{
			get { return moveHeader.ImportingConveyanceName; }
		}

		ZString IInBondQPHeader.InBondCarrierID
		{
			get { return moveHeader.InBondCarrierID; }
		}

		ZString IInBondQPHeader.JobNumber
		{
			get { return moveHeader.JobNumber; }
		}

		ZString IInBondQPHeader.PortOfUnlading
		{
			get { return moveHeader.PortOfUnlading; }
		}

		ZString IInBondQPHeader.USDestination
		{
			get { return moveHeader.USDestination; }
		}

		ZDecimal IInBondQPHeader.Value
		{
			get { return moveHeader.Value; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode
		{
			get { return moveHeader.EntryFilerCode; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get { return moveHeader.ProcessingDistrictPort; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get { return moveHeader.ProcessingOfficeCode; }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return moveHeader.CompanyPK; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return moveHeader.Branch; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return moveHeader.Messages; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return moveHeader.TopLevelBizObjReferenceNumber; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return moveHeader.TopLevelBusinessObject; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return moveHeader.TopLevelBusinessObjectLogs; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return moveHeader.BusinessObjectPK; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return moveHeader.ControllerID; }
		}
		#endregion
	}
}
