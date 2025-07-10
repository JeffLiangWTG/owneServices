using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public sealed class DemandeDeTracing : DocDataObject, IDataSourceProvider
	{
		public DemandeDeTracing(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region ConsolNumber

		public ZString ConsolNumber
		{
			get => consolNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsolNumberInfo, ref consolNumber, value))
				{
					Validate(ConsolNumberInfo);
				}
			}
		}
		ZString consolNumber;

		public ZPropertyInfo ConsolNumberInfo => GetZPropertyInfo(nameof(ConsolNumber));

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region BookingConfirmationReference

		public ZString BookingConfirmationReference
		{
			get => bookingConfirmationReference;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationReferenceInfo, ref bookingConfirmationReference, value))
				{
					Validate(BookingConfirmationReferenceInfo);
				}
			}
		}
		ZString bookingConfirmationReference;

		public ZPropertyInfo BookingConfirmationReferenceInfo => GetZPropertyInfo(nameof(BookingConfirmationReference));

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region WaybillNumber

		public ZString WaybillNumber
		{
			get => waybillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(WaybillNumberInfo, ref waybillNumber, value))
				{
					Validate(WaybillNumberInfo);
				}
			}
		}

		ZString waybillNumber;

		public ZPropertyInfo WaybillNumberInfo => GetZPropertyInfo(nameof(WaybillNumber));

		#endregion

		#region SendingForwarder

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}

		Address sendingForwarder;

		#endregion

		#region ReceivingForwarder

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}

		Address receivingForwarder;

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion

		#region SendingPartySON

		public RegistrationNumber SendingPartySON
		{
			get => sendingPartySON;
			set => sendingPartySON = SetChild(sendingPartySON, value);
		}

		RegistrationNumber sendingPartySON;

		#endregion

		#region SendingPartyCI5

		public RegistrationNumber SendingPartyCI5
		{
			get => sendingPartyCI5;
			set => sendingPartyCI5 = SetChild(sendingPartyCI5, value);
		}

		RegistrationNumber sendingPartyCI5;

		#endregion

		#region Containers

		public IReadOnlyCollection<DemandeDeTracingContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<DemandeDeTracingContainer> containers;

		#endregion

		#region Notifications

		new public IEnumerable<INotification> Notifications
		{
			get
			{
				return notifications ?? (notifications = new DemandeDeTracingNotificationCollector(this));
			}
		}
		DemandeDeTracingNotificationCollector notifications;

		public sealed class DemandeDeTracingNotificationCollector : ZNotificationCollector
		{
			public DemandeDeTracingNotificationCollector(DemandeDeTracing demandeDeTracing)
				: base(demandeDeTracing, true, true, PropertyDescriptionType.None)
			{
			}
		}

		#endregion

		#region ETD

		public ZDateTime ETD
		{
			get => etd;
			set
			{
				if (SetNonPersistentPropertyValue(ETDInfo, ref etd, value))
				{
					Validate(ETDInfo);
				}
			}
		}

		ZDateTime etd;

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(nameof(ETD));

		#endregion

		#region ETA

		public ZDateTime ETA
		{
			get => eta;
			set
			{
				if (SetNonPersistentPropertyValue(ETAInfo, ref eta, value))
				{
					Validate(ETAInfo);
				}
			}
		}

		ZDateTime eta;

		public ZPropertyInfo ETAInfo => GetZPropertyInfo(nameof(ETA));

		#endregion
	}
}
