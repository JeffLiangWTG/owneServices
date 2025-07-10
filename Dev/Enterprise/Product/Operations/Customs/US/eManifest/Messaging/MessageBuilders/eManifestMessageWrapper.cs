using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using EDIMessage = Enterprise.Customs.US.eManifest.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class eManifestMessageWrapper : IControllerIDProvider, ICompleteManifest, IReleaseStatusAttachee, ICrewOrEquipmentRegistration
	{
		public eManifestMessageWrapper(Trip trip, ZString messageType)
		{
			this.trip = trip;
			MessageType = messageType;
		}

		#region Implementation of IControllerIDProvider

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.US.eManifest; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return trip.PK.ToGuid(); }
		}

		#endregion

		#region Implementation of ICompleteManifest

		public ZString DepartmentOfTransportationNumber
		{
			get { return trip.DepartmentOfTransportationNumber; }
		}

		public ZString CarrierCode
		{
			get { return trip.BH_CarrierSCAC; }
		}

		public ZString OriginatorFullName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		public ZString OriginatorPhone
		{
			get
			{
				var originator = GlbStaff.CurrentUser;
				return originator.GS_PublishWorkPhone && !originator.GS_WorkPhone.IsEmpty ? originator.GS_WorkPhone
								: originator.HomeBranch?.GB_Phone ?? ZString.Empty;
			}
		}

		public ZString MethodOfTransportation
		{
			get { return trip.BH_ImportTransportMode; }
		}

		public ZString TripReference
		{
			get { return MessageType == MessageTypes.Codes.UnassociatedShipments && !trip.IsLodged ? "SYSTEM" : CarrierCode + trip.BH_VoyageNumber; }
		}

		public ZDateTime EstimatedDateOfArrival
		{
			get { return trip.BH_ETA; }
		}

		public ZString FirstExpectedPortOfArrival
		{
			get { return trip.BH_PortUnladingDCode; }
		}

		public ZString AmendmentReasonCode { get; set; }

		public ZString TransitDirectionCode
		{
			get { return trip.BH_TransitDirection; }
		}

		public ZString TransmissionReferenceNumber
		{
			get { return MessageType + EDIMessage.MessageNumberPlaceHolder; } //NOTE: This will be returned in response message
		}

		public ZString MessageType { get; private set; }

		public void CalculateMessageTypeForCancellation()
		{
			MessageType = !trip.IsLodged || IsFinalized ? MessageTypes.Codes.eManifest : MessageTypes.Codes.PreliminaryTrip;
		}

		public ZBool IsFinalized
		{
			get { return trip.IsFinalized; }
		}

		public IEnumerable<ICrew> CrewMembers
		{
			get { return crewMembers ?? (crewMembers = from staff in trip.CrewMembers select (ICrew)new CrewWrapper(MessageType, staff)); }
		}

		IEnumerable<ICrew> crewMembers;

		public IConveyance Conveyance
		{
			get { return conveyance ?? (conveyance = new ConveyanceWrapper(trip.Conveyance)); }
		}

		IConveyance conveyance;

		public IEnumerable<IEquipment> Equipment
		{
			get { return from equipment in trip.Equipment select (IEquipment)new EquipmentWrapper(equipment); }
		}

		public IEnumerable<IShipment> Shipments
		{
			get { return shipments ?? (shipments = (from ShipmentAction action in trip.ShipmentsActions select (IShipment)new ShipmentWrapper(action)).ToArray()); }
		}

		IEnumerable<IShipment> shipments;

		#endregion

		#region Implementation of IEDIMessageCollectionProvider

		public EDIMessageCollection Messages
		{
			get { return trip.Messages; }
		}

		public BusinessObjectFactory Factory
		{
			get { return trip.Factory; }
		}

		#endregion

		#region Implementation of IEDIFACTMessageAttachee

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			trip.Messages.Add(message);
		}

		public ZString MessageStatus
		{
			get { return trip.BH_MessageStatus; }
			set { trip.BH_MessageStatus = value; }
		}

		public ZString JobStatus
		{
			get { return trip.BH_ReleaseStatus; }
			set { trip.BH_ReleaseStatus = value; }
		}

		public bool HasChanges
		{
			get { return trip.HasChanges; }
		}

		public ZString JobIdentification
		{
			get { return trip.BH_JobReference; }
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return trip; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

		#region Implementation of IReleaseStatusAttachee

		void IReleaseStatusAttachee.UpdateReleaseStatus(ZString shipmentControlNumber, ZString releaseStatus, ZDateTime releaseStatusDate)
		{
			var shipment = Shipments.FirstOrDefault(s => s.ShipmentControlNumber == shipmentControlNumber) as ShipmentWrapper;
			if (shipment != null)
			{
				shipment.UpdateReleaseStatus(releaseStatus, releaseStatusDate);
			}
		}

		void IReleaseStatusAttachee.UpdateReleaseStatusOnAllLinkedShipments(ZString releaseStatus, ZDateTime releaseStatusDate)
		{
			foreach (var shipment in Shipments.Cast<ShipmentWrapper>().Where(s => s.IsLinked))
			{
				shipment.UpdateReleaseStatus(releaseStatus, releaseStatusDate);
			}
		}

		#endregion

		public void SetACEId(ZString infoType, ZString key, ZString aceId)
		{
			if (infoType == RegistrationInfoTypes.Codes.Crew)
			{
				var crew = trip.CrewMembers.FirstOrDefault(c => CrewDataMatches(c, key));
				if (crew != null)
				{
					var provider = ((ICertificatesProvider)crew.Staff ?? crew.Contact) ?? crew;
					if (provider.Certificates.All(c => c.XZ_Type != CrewACEIdTypes.Codes.Id))
					{
						var cert = provider.Certificates.AddNew();
						cert.XZ_Type = CrewACEIdTypes.Codes.Id;
						cert.XZ_Comment = CrewACEIdTypes.Descriptions.Id;
						cert.XZ_RefNumber = aceId;
					}
				}
			}
		}

		static bool CrewDataMatches(CrewMember crew, ZString key)
		{
			var crewKey = new StringBuilder();
			crewKey.Append(crew.CP_FullName.Replace(" ", "").ToUpper());
			crewKey.Append(crew.CP_Gender);
			crewKey.Append(crew.CP_DateOfBirth.Date);
			crewKey.Append(crew.CP_RN_NKNationality);
			crewKey.Append(crew.DriversLicense);
			return crewKey.ToString() == key;
		}

		readonly Trip trip;
	}
}
