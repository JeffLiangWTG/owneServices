using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ContainerLoadPlan : DocDataObject, IDataSourceProvider
	{
		public ContainerLoadPlan(string sourceId, string sourceType, string documentName)
		{
			this.SourceID = sourceId;
			this.SourceType = sourceType;
			this.DocumentName = documentName;
		}

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

		#region TransitBerthCode

		public ZString TransitBerthCode
		{
			get => transitBerthCode;
			set
			{
				if (SetNonPersistentPropertyValue(TransitBerthCodeInfo, ref transitBerthCode, value.ToUpperInvariant()))
				{
					Validate(TransitBerthCodeInfo);
				}
			}
		}

		ZString transitBerthCode;

		public ZPropertyInfo TransitBerthCodeInfo => GetZPropertyInfo(nameof(TransitBerthCode));

		#endregion

		#region TradeFlag

		public ZString TradeFlag
		{
			get => tradeFlag;
			set
			{
				if (SetNonPersistentPropertyValue(TradeFlagInfo, ref tradeFlag, value))
				{
				}
			}
		}

		ZString tradeFlag;

		public ZPropertyInfo TradeFlagInfo => GetZPropertyInfo(nameof(TradeFlag));

		#endregion

		#region SendingAgent

		public Address SendingAgent
		{
			get => sendingAgent;
			set => sendingAgent = SetChild(sendingAgent, value);
		}

		Address sendingAgent;

		#endregion

		#region CurrentUser

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		Address currentUser;

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region DepartureCFSAddress

		public Address DepartureCFSAddress
		{
			get => departureCFSAddress;
			set => departureCFSAddress = SetChild(departureCFSAddress, value);
		}

		Address departureCFSAddress;

		#endregion

		#region PortOfLoading

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}

		IUnloco portOfLoading;

		#endregion

		#region PortOfDischarge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		IUnloco portOfDischarge;

		#endregion

		#region PortOfTranship

		public IUnloco PortOfTranship
		{
			get => portOfTranship;
			set => portOfTranship = SetChild(portOfTranship, value);
		}

		IUnloco portOfTranship;

		#endregion

		#region PlaceOfDelivery

		public IUnloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}

		IUnloco placeOfDelivery;

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region Vessel

		public Vessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}

		Vessel vessel;

		#endregion

		#region VoyageFlightNumber

		public ZString VoyageFlightNumber
		{
			get => voyageFlightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNumberInfo, ref voyageFlightNumber, value))
				{
					Validate(VoyageFlightNumberInfo);
				}
			}
		}

		ZString voyageFlightNumber;

		public ZPropertyInfo VoyageFlightNumberInfo => GetZPropertyInfo(nameof(VoyageFlightNumber));

		#endregion

		#region Containers

		public IReadOnlyCollection<ContainerLoadPlanContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<ContainerLoadPlanContainer> containers;

		#endregion

		public ZString SourceID { get; }

		public ZString SourceType { get; }

		public ZString DocumentName { get; }
	}
}
