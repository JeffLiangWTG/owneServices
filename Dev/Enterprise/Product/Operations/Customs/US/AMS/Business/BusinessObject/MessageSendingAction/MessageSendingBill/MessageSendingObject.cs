using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingObject : AutoMessageSendingObject,
		IACEBillManifestMessageAttachee,
		IACEBillOfLading,
		IPort
	{
		public MessageSendingObject(CusInBondMoveDetail moveDetail, ActionCode actionCode)
			: base(moveDetail.Factory)
		{
			this.MoveDetail = moveDetail;
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}
			UpdateAction(actionCode);
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(MessageSendingObject);
			}

			public MessageSendingObject LoadOrNew(CusInBondMoveDetail moveDetail, ActionCode actionCode)
			{
				var obj = Factory.Load<MessageSendingObject>(moveDetail.PK) ?? new MessageSendingObject(moveDetail, actionCode);
				obj.UpdateAction(actionCode);
				return obj;
			}
		}

		public void UpdateAction(ActionCode actionCode)
		{
			this.ActionCode = actionCode;
			MB_CustomsStatus = MoveDetail.B9_CustomsStatus;
			MB_MessageStatus = MoveDetail.B9_MessageStatus;
			MB_BillActionCode = GetDefaultActionCode(MB_CustomsStatus);
			if (ActionCodeTool.IsInBondType(actionCode))
			{
				var moveHeader = MoveHeader;
				if (moveHeader != null)
				{
					MB_RelatedDetails = moveHeader.MovementUniqueCode.Left(Schema.MB_RelatedDetailsMaxLength);
				}
			}
			var bill = MoveDetail.Bill;
			if (bill != null)
			{
				if (!bill.B0_BillActionCode.IsEmpty && !bill.B0_BillAmendmentCode.IsEmpty)
				{
					MB_BillActionCode = bill.B0_BillActionCode;
					MB_AmendmentCode = bill.B0_BillAmendmentCode;
				}
				MB_IssuerCode = bill.B0_IssuerCode;
				var billNumber = bill.B0_MasterBillNumber.KeepAlphanumericCharacters();
				MB_BillOfLadingSequenceNumber = billNumber.Right(Schema.MB_BillOfLadingSequenceNumberMaxLength);
				bill.ValidationModes = ValidationModes.UseParentValidateMode;
				RegisterEditableChildObject(bill);
			}

			if (ActionCodeTool.IsVesselEvent(actionCode))
			{
				var header = Header;
				if (header != null)
				{
					MB_Date = actionCode == ActionCode.VesselArrival ? header.BH_ETA
							: actionCode == ActionCode.VesselDeparture ? header.BH_SailingDate
							: ZDateTime.Empty;

					if (actionCode == ActionCode.ChangeEstDateOfArrival)
					{
						var firstMatchedBill = header.Bills.FirstOrDefault(x => x.B0_InBondPortOfDestDCode == MB_PortOfUnlading);
						if (firstMatchedBill != null)
						{
							MB_Date = firstMatchedBill.B0_DateOfDischarge;
						}
					}
				}
			}

			ShouldValidateDate = null;
			MB_Send = true;
		}

		public void GeneratePendingOriginalAdd()
		{
			UpdateAction(ActionCode.Creating);
			var pendingMsg = new ACEAMSMessageBuilder(this, ActionCode.Creating).PopulateMessage();
			if (pendingMsg != null)
			{
				pendingMsg.EM_SendWithMessageErrors = true;
				pendingMsg.EM_Status = EDIMessage.Status.Pending;
			}
		}

		public void PurgeActionCodeAndAmendmentCode()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.B0_BillActionCode = ZString.Empty;
				bill.B0_BillAmendmentCode = ZString.Empty;
			}
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return CusInBondMoveDetailSchema.PK; }
		}

		public void UnRegisterBillAsEditableChildObject()
		{
			var bill = Bill;
			if (bill != null)
			{
				UnRegisterEditableChildObject(bill);
			}
		}

		public override ZBool MB_Send
		{
			get { return base.MB_Send; }
			set
			{
				var oldValue = MB_Send;
				base.MB_Send = value;
				if (!IsCopying && oldValue != MB_Send)
				{
					var bill = MoveDetail.Bill;
					if (bill != null)
					{
						bill.ValidationModes = MB_Send ? ValidationModes.UseParentValidateMode : ValidationModes.None;
					}
					if (MB_Send)
					{
						RegisterBillAsEditableChildObject();
					}
					else
					{
						UnRegisterBillAsEditableChildObject();
					}
				}
			}
		}

		[List(nameof(AmendmentCodeList))]
		public override ZString MB_AmendmentCode
		{
			get { return base.MB_AmendmentCode; }
			set
			{
				var oldValue = MB_AmendmentCode;
				base.MB_AmendmentCode = value;
				if (!IsCopying && oldValue != MB_AmendmentCode)
				{
					MB_MessageContentsInfo.RefreshBinding();
				}
			}
		}

		protected override bool MB_AmendmentCode_ReadOnly
		{
			get { return !ActionCodeTool.IsAmendingType(ActionCode); }
		}

		[List(nameof(BillActionCodeList))]
		public override ZString MB_BillActionCode
		{
			get { return base.MB_BillActionCode; }
			set
			{
				var oldValue = MB_BillActionCode;
				base.MB_BillActionCode = value;
				if (!IsCopying && oldValue != MB_BillActionCode)
				{
					MB_MessageContentsInfo.RefreshBinding();
				}

				if (!IsAmendingDelete)
				{
					MB_PortOfUnladingOverride = ZString.Empty;
				}
			}
		}

		protected override bool MB_BillActionCode_ReadOnly
		{
			get { return !ActionCodeTool.IsAmendingType(ActionCode) && !ActionCodeTool.IsInBondArrivalExportationTOL(ActionCode); }
		}

		public override ZDateTime MB_Date
		{
			get { return base.MB_Date; }
			set
			{
				base.MB_Date = value;
				MB_MessageContentsInfo.RefreshBinding();
			}
		}

		public override ZString MB_ForeignDeparturePort
		{
			get { return base.MB_ForeignDeparturePort; }
			set
			{
				base.MB_ForeignDeparturePort = value;
				MB_MessageContentsInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude]
		public override ZString MB_PortOfUnlading
		{
			get { return ActionCodeTool.IsVesselArrivalEventRelevent(ActionCode) ? ((IPort)this).DistrictPortOfUnladingCode : ZString.Empty; }
		}

		public override ZBool MB_VesselOverride
		{
			get => base.MB_VesselOverride;
			set
			{
				base.MB_VesselOverride = value;
				MB_MessageContentsInfo.RefreshBinding();
			}
		}

		#region MB_PortOfUnladingOverride

		[BusinessObjectTestExclude]
		[List(nameof(ScheduleDList))]
		public override ZString MB_PortOfUnladingOverride
		{
			get
			{
				return base.MB_PortOfUnladingOverride;
			}
			set
			{
				var oldValue = base.MB_PortOfUnladingOverride;
				base.MB_PortOfUnladingOverride = value;
				if (!IsCopying && oldValue != base.MB_PortOfUnladingOverride)
				{
					MB_MessageContentsInfo.RefreshBinding();
				}
			}
		}

		protected bool MB_PortOfUnladingOverride_ReadOnly
		{
			get { return !IsAmendingDelete; }
		}

		public bool IsAmendingDelete
		{
			get { return ActionCodeTool.IsAmendingType(ActionCode) && (MB_BillActionCode == AMSBillSendingActionCodeList.Codes.DeleteBill || MB_BillActionCode == AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd); }
		}

		#endregion

		#region Customs Status

		protected override ZString GetMB_CustomsStatusDescription()
		{
			return Factory.GetCachedValue<AMSBillCustomsStatusList>().GetDescriptionFromCode(MB_CustomsStatus) ?? ZString.Empty;
		}

		#endregion

		#region Message Status

		protected override ZString GetMB_MessageStatusDescription()
		{
			return Factory.GetCachedValue<AMSBillMessageStatusList>().GetDescriptionFromCode(MB_MessageStatus);
		}

		#endregion

		public CodeDescriptionPairList BillActionCodeList
		{
			get
			{
				if (ActionCodeTool.IsInBondArrivalExportationTOL(ActionCode))
				{
					return InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode);
				}
				else
				{
					return AMSBillSendingActionCodeList.GetBillActionCodeList(Factory, ActionCode);
				}
			}
		}

		public AMSAmendmentCodeList AmendmentCodeList
		{
			get { return Factory.GetCachedValue<AMSAmendmentCodeList>(); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleDList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public bool IsCreatingActionAndBillAlreadyOnFile
		{
			get { return ActionCode == ActionCode.Creating && IsBillAlreadyOnFile; }
		}

		public bool IsBillAlreadyOnFile
		{
			get { return MoveDetail.IsBillAlreadyOnFile; }
		}

		public ZBool ShouldSendManifestAmendmentMessage
		{
			get { return MoveHeader.ShouldSendManifestAmendmentMessage; }
		}

		#region Implementation
		public delegate bool ShouldValidateDateDelegate();
		public ShouldValidateDateDelegate ShouldValidateDate;

		protected override void AddToFactoryCache()
		{
			// should be called after MoveDetail is set
		}

		protected override ZGuid GetPK()
		{
			return MoveDetail.PK;
		}

		void RegisterBillAsEditableChildObject()
		{
			var bill = Bill;
			if (bill != null)
			{
				RegisterEditableChildObject(bill);
			}
		}

		protected override ZString GetMB_MessageContents()
		{
			var result = ZString.Empty;
			if (MB_Send)
			{
				var generator = new AMSInputBlockControlGenerator();

				if (ActionCode == ActionCode.PermitToTransfer)
				{
					generator.AddMessageBlocks(new PTTMessageBlockBuilder(this).Build());
				}
				else if (ActionCodeTool.IsInBondVesselArrivalDeparture(ActionCode))
				{
					generator.AddMessageBlocks(new InBondVesselEventMessageBlockBuilder(this).Build());
				}
				else if (IsAmendingDelete)
				{
					generator.AddMessageBlocks(new ACEAMSMessageBlockBuilder(new DeleteMessageSendingObject(this), ActionCode).Build());
				}
				else
				{
					generator.AddMessageBlocks(new ACEAMSMessageBlockBuilder(this, ActionCode).Build());
				}
				result = generator.Serialise(true);
			}
			return result;
		}

		public ActionCode ActionCode
		{
			get;
			private set;
		}
		public readonly CusInBondMoveDetail MoveDetail;

		CusInBondBill Bill
		{
			get { return MoveDetail.Bill; }
		}

		ZString GetDefaultActionCode(ZString customStatus)
		{
			var result = ZString.Empty;
			switch (ActionCode)
			{
				case ActionCode.AmendingDelete:
					result = AMSBillSendingActionCodeList.Codes.DeleteBill;
					break;
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingUpdate:
					result = customStatus == AMSBillCustomsStatusList.Codes.OnFile ? AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd : AMSBillSendingActionCodeList.Codes.AddBill;
					break;
				case ActionCode.InBondArrival:
					result = InBondAndVesselEventMessageCodeList.Codes.ArriveInBond;
					break;
				case ActionCode.InBondDiversion:
					result = InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion;
					break;
				case ActionCode.InBondExportation:
					result = InBondAndVesselEventMessageCodeList.Codes.ExportInBond;
					break;
				case ActionCode.InBondTransferOfLiability:
					result = InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability;
					break;
				case ActionCode.CancelPermitToTransfer:
					result = InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading;
					break;
				case ActionCode.VesselArrival:
					result = InBondAndVesselEventMessageCodeList.Codes.VesselArrival;
					break;
				case ActionCode.VesselDeparture:
					result = InBondAndVesselEventMessageCodeList.Codes.VesselDeparture;
					break;
				case ActionCode.ChangeEstDateOfArrival:
					result = InBondAndVesselEventMessageCodeList.Codes.ChangeInTheEstimatedDateOfArrival;
					break;
			}
			return result;
		}

		ZString GetVesselStatus(ZString result)
		{
			switch (ActionCode)
			{
				case ActionCode.VesselArrival:
					result = AMSBillMessageStatusList.Codes.AwaitingArrival;
					break;
				case ActionCode.VesselDeparture:
					result = AMSBillMessageStatusList.Codes.AwaitingDeparture;
					break;
				case ActionCode.ChangeEstDateOfArrival:
					result = AMSBillMessageStatusList.Codes.Updating;
					break;
			}
			return result;
		}

		ZString GetBillStatus(ZString result)
		{
			switch (ActionCode)
			{
				case ActionCode.InBondArrival:
					result = AMSBillMessageStatusList.Codes.AwaitingArrival;
					break;
				case ActionCode.InBondDiversion:
					result = AMSBillMessageStatusList.Codes.AwaitingDiversion;
					break;
				case ActionCode.InBondExportation:
					result = AMSBillMessageStatusList.Codes.AwaitingExportation;
					break;
				case ActionCode.InBondTransferOfLiability:
					result = AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability;
					break;
				case ActionCode.SubsequentInBondAmendment:
				case ActionCode.SubsequentInBondOriginal:
					result = AMSBillMessageStatusList.Codes.AwaitingDeparture;
					break;
				case ActionCode.SubsequentInBondDelete:
				case ActionCode.CancelPermitToTransfer:
					result = AMSBillMessageStatusList.Codes.Deleting;
					break;
				case ActionCode.PermitToTransfer:
					result = AMSBillMessageStatusList.Codes.AwaitingPermitToTransfer;
					break;
				default:
					if (ActionCode == ActionCode.Creating || MB_BillActionCode == AMSBillSendingActionCodeList.Codes.AddBill)
					{
						result = AMSBillMessageStatusList.Codes.Adding;
					}
					else if (MB_BillActionCode == AMSBillSendingActionCodeList.Codes.DeleteBill)
					{
						result = AMSBillMessageStatusList.Codes.Deleting;
					}
					else
					{
						result = AMSBillMessageStatusList.Codes.Updating;
					}
					break;
			}
			return result;
		}

		IManifestMessageAttachee MessageAttachee
		{
			get { return MoveHeader; }
		}

		CusInBondHeader Header
		{
			get { return MoveDetail.Header; }
		}

		CusInBondMoveHeader MoveHeader
		{
			get { return MoveDetail.MoveHeader; }
		}

		public ActionCode GetActualActionCode()
		{
			var result = ActionCode;
			switch (ActionCode)
			{
				case ActionCode.AmendingAdd:
					switch (MB_BillActionCode)
					{
						case AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity:
							result = ActionCode.AmendingUpdate;
							break;
						case AMSBillSendingActionCodeList.Codes.DeleteBill:
						case AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd:
							result = ActionCode.AmendingDelete;
							break;
					}
					break;
				case ActionCode.SubsequentInBondAmendment:
					if (MB_BillActionCode == AMSBillSendingActionCodeList.Codes.CancelSubInBond)
					{
						result = ActionCode.SubsequentInBondDelete;
					}
					break;
			}
			return result;
		}

		#endregion

		#region IBillOfLading Members

		IManifestMessageAttachee IBaseBillOfLading.MessageAttachee
		{
			get { return MessageAttachee; }
		}

		ICommonBillOfLading CommonBillOfLading
		{
			get { return MoveDetail; }
		}

		IACEBillOfLading ACEBillOfLading
		{
			get { return MoveDetail; }
		}

		ZString ICommonBillOfLading.BillActionCode
		{
			get { return MB_BillActionCode; }
		}

		ZString ICommonBillOfLading.AmendmentCode
		{
			get { return MB_AmendmentCode; }
		}

		ZString ICommonBillOfLading.IssuerCode
		{
			get { return MB_IssuerCode; }
		}

		ZString ICommonBillOfLading.BillOfLadingSequenceNumber
		{
			get { return MB_BillOfLadingSequenceNumber; }
		}

		ZString ICommonBillOfLading.ForeignPort
		{
			get { return CommonBillOfLading.ForeignPort; }
		}

		ZDecimal ICommonBillOfLading.ManifestQuantity
		{
			get { return CommonBillOfLading.ManifestQuantity; }
		}

		ZString ICommonBillOfLading.ManifestUnits
		{
			get { return CommonBillOfLading.ManifestUnits; }
		}

		ZDecimal ICommonBillOfLading.Weight
		{
			get { return CommonBillOfLading.Weight; }
		}

		ZString ICommonBillOfLading.WeightUnit
		{
			get { return CommonBillOfLading.WeightUnit; }
		}

		ZString ICommonBillOfLading.BillOfLadingStatusIndicator
		{
			get { return CommonBillOfLading.BillOfLadingStatusIndicator; }
		}

		ZBool ICommonBillOfLading.IsMasterInbond
		{
			get { return CommonBillOfLading.IsMasterInbond; }
		}

		ZString ICommonBillOfLading.HouseBillNumber
		{
			get { return CommonBillOfLading.HouseBillNumber; }
		}

		ZString ICommonBillOfLading.FIRMS
		{
			get { return CommonBillOfLading.FIRMS; }
		}

		ZDecimal ICommonBillOfLading.Volume
		{
			get { return CommonBillOfLading.Volume; }
		}

		ZString ICommonBillOfLading.VolumeUnit
		{
			get { return CommonBillOfLading.VolumeUnit; }
		}

		ZString ICommonBillOfLading.PlaceOfReceiptByCarrier
		{
			get { return CommonBillOfLading.PlaceOfReceiptByCarrier; }
		}

		ZString ICommonBillOfLading.SpaceCharterBLReference
		{
			get { return CommonBillOfLading.SpaceCharterBLReference; }
		}

		ZString ICommonBillOfLading.SecondNotifyParty1
		{
			get { return CommonBillOfLading.SecondNotifyParty1; }
		}

		ZString ICommonBillOfLading.SecondNotifyParty2
		{
			get { return CommonBillOfLading.SecondNotifyParty2; }
		}

		ZString ICommonBillOfLading.LastForeignPortBeforeDepartingForTheUS
		{
			get { return CommonBillOfLading.LastForeignPortBeforeDepartingForTheUS; }
		}

		ZString ICommonBillOfLading.ModeOfTransportationFromThePlacePriorToLoading
		{
			get { return CommonBillOfLading.ModeOfTransportationFromThePlacePriorToLoading; }
		}

		ZString ICommonBillOfLading.MethodOfPaymentForTransportation
		{
			get { return CommonBillOfLading.MethodOfPaymentForTransportation; }
		}

		ZString ICommonBillOfLading.ContractualPossessionForeignPort
		{
			get { return CommonBillOfLading.ContractualPossessionForeignPort; }
		}

		IEnumerable<IShipmentReferenceDetail> ICommonBillOfLading.ShipmentReferenceDetails(ActionCode actionCode)
		{
			return CommonBillOfLading.ShipmentReferenceDetails(actionCode);
		}

		IEnumerable<IEntity> IACEBillOfLading.Entities(ActionCode actionCode)
		{
			return ACEBillOfLading.Entities(actionCode);
		}

		IMovemenDetails IACEBillOfLading.MovemenDetails
		{
			get { return ACEBillOfLading.MovemenDetails; }
		}

		IEnumerable<IACEContainer> IACEBillOfLading.Containers
		{
			get { return ACEBillOfLading.Containers; }
		}

		#endregion

		#region IBillManifestMessageAttachee Members

		void ICommonBillManifestMessageAttachee.UpdateOutgoingBillStatus()
		{
			if (ActionCodeTool.IsVesselEvent(ActionCode))
			{
				var moveHeader = MoveHeader;
				moveHeader.BM_CustomsStatus = GetVesselStatus(moveHeader.BM_CustomsStatus);
			}
			else
			{
				MoveDetail.B9_MessageStatus = GetBillStatus(MoveDetail.B9_MessageStatus);
			}
		}

		IPort ICommonBillManifestMessageAttachee.PortDetails
		{
			get { return this; }
		}

		IACEBillOfLading IACEBillManifestMessageAttachee.BillOfLadingDetails
		{
			get { return this; }
		}

		ZDateTime IACEBillManifestMessageAttachee.EventDateTime
		{
			get { return MB_Date; }
		}

		ZString IACEBillManifestMessageAttachee.ForeignDeparturePort
		{
			get { return MB_ForeignDeparturePort; }
		}

		ZString IACEBillManifestMessageAttachee.BillPKAsString
		{
			get { return Bill?.PK.ToString() ?? ZString.Empty; }
		}

		AMSBillEDIMessageCollection IACEBillManifestMessageAttachee.BillMessages
		{
			get { return Bill?.Messages; }
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

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get { return MessageAttachee.Branch; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return MessageAttachee.MessageStatus; }
			set { MessageAttachee.MessageStatus = value; }
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

		#endregion

		#region IManifestMessageAttachee Members

		ZString IManifestMessageAttachee.ApplicationCode
		{
			get { return MessageAttachee.ApplicationCode; }
		}

		ZString IManifestMessageAttachee.UniqueVoyageIdentifier
		{
			get { return MessageAttachee.UniqueVoyageIdentifier; }
		}

		ZString IManifestMessageAttachee.SupApplicationCode
		{
			get { return MessageAttachee.SupApplicationCode; }
		}

		ZString IManifestMessageAttachee.JobNumber
		{
			get { return MessageAttachee.JobNumber; }
		}

		ZString IManifestMessageAttachee.CarrierCode
		{
			get
			{
				if (ActionCodeTool.IsPermitToTransferAction(ActionCode))
				{
					return MoveHeader.CarrierCodeForPTT;
				}

				return MessageAttachee.CarrierCode;
			}
		}

		ZString IManifestMessageAttachee.ModeOfTransportationCode
		{
			get { return MessageAttachee.ModeOfTransportationCode; }
		}

		ZString IManifestMessageAttachee.ConveyanceCountryCode
		{
			get { return MessageAttachee.ConveyanceCountryCode; }
		}

		ZString IManifestMessageAttachee.ConveyanceName
		{
			get { return MessageAttachee.ConveyanceName; }
		}

		ZString IManifestMessageAttachee.VoyageNumber
		{
			get { return MessageAttachee.VoyageNumber; }
		}

		ZString IManifestMessageAttachee.ManifestSequenceNumber
		{
			get { return MessageAttachee.ManifestSequenceNumber; }
			set { MessageAttachee.ManifestSequenceNumber = value; }
		}

		ZBool IManifestMessageAttachee.IsPaperlessMIBParticipant
		{
			get { return MessageAttachee.IsPaperlessMIBParticipant; }
		}

		ZString IManifestMessageAttachee.ConveyanceCode
		{
			get { return MessageAttachee.ConveyanceCode; }
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

		#region IPort Members

		ZString IPort.DistrictPortOfUnladingCode
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null)
				{
					if (header.IsNVOCCHeader)
					{
						result = header.BH_PortUnladingDCode;
					}
					else
					{
						var bill = MoveDetail.Bill;
						if (bill != null)
						{
							return bill.B0_InBondPortOfDestDCode;
						}
					}
				}

				return result;
			}
		}

		ZDate IPort.OriginalEstimatedDate
		{
			get
			{
				var result = ZDate.Empty;
				var header = Header;
				if (header != null)
				{
					if (header.IsNVOCCHeader)
					{
						result = header.BH_ETA.Date;
					}
					else
					{
						var bill = MoveDetail.Bill;
						if (bill != null)
						{
							return bill.B0_DateOfDischarge;
						}
					}
				}

				return result;
			}
		}

		ZInt IPort.NumberOfBillsOfLadingForPort
		{
			get { return 1; }
		}

		ZString IPort.FIRMSCode
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null)
				{
					if (header.IsNVOCCHeader)
					{
						result = header.BH_FIRMS;
					}
					else
					{
						var bill = MoveDetail.Bill;
						if (bill != null)
						{
							return bill.B0_Firms;
						}
					}
				}

				return result;
			}
		}

		ZString IPort.Time
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null)
				{
					if (header.IsNVOCCHeader)
					{
						result = header.BH_ETA.ToString("HHmm");
					}
					else
					{
						result = "0000";
					}
				}

				return result;
			}
		}

		#endregion
	}
}
