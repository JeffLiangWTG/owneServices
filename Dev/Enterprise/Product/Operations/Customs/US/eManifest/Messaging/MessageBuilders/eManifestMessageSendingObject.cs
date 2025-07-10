using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class eManifestMessageSendingObject : AutoeManifestMessageSendingObject, ICompleteManifest, ICrewOrEquipmentRegistration
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public eManifestMessageSendingObject(eManifestMessageSendingObjectParent sendingObjectParent, ZString messageType, MessageSubTypes actionCode, ZShort order)
			: base(sendingObjectParent.Factory)
		{
			this.MessageType = messageType;
			this.ActionCode = actionCode;
			this.SendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
			this.Order = order;
		}
		internal readonly eManifestMessageSendingObjectParent SendingObjectParent;

		public Trip Trip => SendingObjectParent.Trip;
		public MessageSubTypes ActionCode { get; }

		public override ZShort Order { get; }

		public override ZString MessageDescription
		{
			get
			{
				var description = MessageTypes.GetDescriptionFromCode(MessageType);
				if (ActionCode == MessageSubTypes.Confirmation)
				{
					description = Res.GetString("63A62751-66AD-402E-BB05-E56A9E8B8B8B", "Confirm") + " " + description;
				}
				return description;
			}
		}

		[ResourceStringData("NPBO:Enterprise.Customs.US.eManifest.Messaging.eManifestMessageSendingObject|ShouldSend", ShortCaption = "Send?", Caption = "Send?")]
		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				if (value != ShouldSend)
				{
					base.ShouldSend = value;
				}
			}
		}

		public CodeDescriptionPairList MessageTypes => Factory.GetCachedValue<MessageTypes>();

		#region ICompleteManifest
		ZString ICompleteManifest.TransmissionReferenceNumber => MessageType + Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder;

		ZBool ICompleteManifest.IsFinalized => Trip.IsFinalized;

		IEnumerable<IShipment> ICompleteManifest.Shipments
		{
			get { return shipments ?? (shipments = (from ShipmentAction action in Trip.ShipmentsActions select (IShipment)new ShipmentWrapper(action)).ToArray()); }
		}
		IEnumerable<IShipment> shipments;

		ZString ITrip.TransitDirectionCode => Trip.BH_TransitDirection;

		public IEnumerable<IEquipment> Equipment => equipment ?? (equipment = from equipments in Trip.Equipment select (IEquipment)new EquipmentWrapper(equipments));
		IEnumerable<IEquipment> equipment;

		ZString ITrip.DepartmentOfTransportationNumber => Trip.DepartmentOfTransportationNumber;

		ZString Customs.Business.MessageBuilders.eManifest.ITrip.CarrierCode => Trip.BH_CarrierSCAC;

		ZString Customs.Business.MessageBuilders.eManifest.ITrip.MethodOfTransportation => Trip.BH_ImportTransportMode;

		ZString Customs.Business.MessageBuilders.eManifest.ITrip.TripReference => MessageType == Enterprise.Customs.US.eManifest.Business.MessageTypes.Codes.UnassociatedShipments && !Trip.IsLodged ? "SYSTEM" : Trip.BH_CarrierSCAC + Trip.BH_VoyageNumber;

		ZDateTime Customs.Business.MessageBuilders.eManifest.ITrip.EstimatedDateOfArrival => Trip.BH_ETA;

		ZString Customs.Business.MessageBuilders.eManifest.ITrip.FirstExpectedPortOfArrival => Trip.BH_PortUnladingDCode;

		ZString Customs.Business.MessageBuilders.eManifest.ITrip.AmendmentReasonCode { get; set; }
		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return MessageStatus; }
			set { MessageStatus = value; }
		}

		public ZString MessageStatus
		{
			get { return Trip.BH_MessageStatus; }
			set { Trip.BH_MessageStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return Trip.BH_ReleaseStatus; }
			set { Trip.BH_ReleaseStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification => Trip.BH_JobReference;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => Trip;

		EDIMessageCollection IEDIMessageCollectionProvider.Messages => Trip.Messages;

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			Trip.Messages.Add(message);
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

		internal eManifestStatusCalculator StatusCalculator => statusCalculator ?? (statusCalculator = new eManifestStatusCalculator(MessageType));
		eManifestStatusCalculator statusCalculator;

		#region ICrewOrEquipmentRegistration
		public ZString CarrierCode => Trip.BH_CarrierSCAC;

		public ZString OriginatorFullName => GlbStaff.CurrentUser.GS_FullName;

		public ZString OriginatorPhone
		{
			get
			{
				var originator = GlbStaff.CurrentUser;
				return originator.GS_PublishWorkPhone && !originator.GS_WorkPhone.IsEmpty ? originator.GS_WorkPhone
								: originator.HomeBranch?.GB_Phone ?? ZString.Empty;
			}
		}

		public IEnumerable<ICrew> CrewMembers
		{
			get { return crewMembers ?? (crewMembers = from staff in Trip.CrewMembers select (ICrew)new CrewWrapper(MessageType, staff)); }
		}

		IEnumerable<ICrew> crewMembers;

		public IConveyance Conveyance
		{
			get { return conveyance ?? (conveyance = new ConveyanceWrapper(Trip.Conveyance)); }
		}

		IConveyance conveyance;
		#endregion

		public override void ValidateShouldSend()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateShouldSend();
			}
		}
	}
}
