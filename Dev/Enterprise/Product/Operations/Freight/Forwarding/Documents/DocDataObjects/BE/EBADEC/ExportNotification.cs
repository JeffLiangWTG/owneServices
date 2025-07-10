using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class ExportNotification : DocDataObject, IDataSourceProvider
	{
		public ExportNotification(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		#endregion

		#region General

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

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}
		IUnloco portOfOrigin;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}
		IUnloco portOfDestination;

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}
		ICodeDescription shipmentType;

		#endregion

		#endregion

		#region Addresses

		#region DepartureCTOAddress

		public IAddress DepartureCTOAddress
		{
			get => departureCTOAddress;
			set => departureCTOAddress = SetChild(departureCTOAddress, value);
		}

		IAddress departureCTOAddress;

		#endregion

		#region SendingForwarderAddress

		public IAddress SendingForwarderAddress
		{
			get => sendingForwarderAddress;
			set => sendingForwarderAddress = SetChild(sendingForwarderAddress, value);
		}

		IAddress sendingForwarderAddress;

		#endregion

		#region CurrentUser

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		IAddress currentUser;

		#endregion

		#endregion

		#region MessageSpecific

		#region SendingPartyCodes

		public RegistrationNumber SendingPartyCode
		{
			get => sendingPartyCode;
			set => sendingPartyCode = SetChild(sendingPartyCode, value);
		}

		RegistrationNumber sendingPartyCode;

		#endregion

		#region TransportModeToTerminal
		public ICodeDescription TransportModeToTerminal
		{
			get => transportModeToTerminal;
			set => transportModeToTerminal = SetChild(transportModeToTerminal, value);
		}
		ICodeDescription transportModeToTerminal;

		#endregion

		#region VesselType

		public ZString VesselType
		{
			get => vesselType;
			set
			{
				if (SetNonPersistentPropertyValue(VesselTypeInfo, ref vesselType, value))
				{
					Validate(VesselTypeInfo);
				}
			}
		}
		ZString vesselType;

		public ZPropertyInfo VesselTypeInfo => GetZPropertyInfo(nameof(VesselType));

		#endregion

		#region Terminal

		public ZString Terminal
		{
			get => terminal;
			set
			{
				if (SetNonPersistentPropertyValue(TerminalInfo, ref terminal, value))
				{
					Validate(TerminalInfo);
				}
			}
		}
		ZString terminal;

		public ZPropertyInfo TerminalInfo => GetZPropertyInfo(nameof(Terminal));

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get => bookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(BookingReferenceInfo, ref bookingReference, value))
				{
					Validate(BookingReferenceInfo);
				}
			}
		}
		ZString bookingReference;

		public ZPropertyInfo BookingReferenceInfo => GetZPropertyInfo(nameof(BookingReference));

		#endregion

		#region ContainersOrVehicles

		public IReadOnlyCollection<PackingLine> PackLines
		{
			get => packLines;
			set => packLines = SetChildCollection(packLines, value);
		}

		IReadOnlyCollection<PackingLine> packLines;

		#endregion

		#region IsFerryTerminal
		public ZBool IsFerryTerminal
		{
			get => isFerryTerminal;
			set
			{
				if (SetNonPersistentPropertyValue(IsFerryTerminalInfo, ref isFerryTerminal, value))
				{
					Validate(IsFerryTerminalInfo);
				}
			}
		}

		ZBool isFerryTerminal;

		public ZPropertyInfo IsFerryTerminalInfo => GetZPropertyInfo(nameof(IsFerryTerminal));

		#endregion IsFerryTerminal

		#endregion
	}
}
