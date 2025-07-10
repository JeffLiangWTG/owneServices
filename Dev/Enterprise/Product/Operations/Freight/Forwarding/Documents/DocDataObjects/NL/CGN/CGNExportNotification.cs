using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class CGNExportNotification : DocDataObject, IDataSourceProvider
	{
		public CGNExportNotification(ZString sourceType, ZString sourceID)
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

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region SendingPartyCGN

		public RegistrationNumber SendingPartyCGNNumber
		{
			get => sendingPartyCGNNumber;
			set => sendingPartyCGNNumber = SetChild(sendingPartyCGNNumber, value);
		}

		RegistrationNumber sendingPartyCGNNumber;

		public ZString SendingPartyCGN
		{
			get => sendingPartyCGN;
			set
			{
				if (SetNonPersistentPropertyValue(SendingPartyCGNInfo, ref sendingPartyCGN, value))
				{
					Validate(SendingPartyCGNInfo);
				}
			}
		}
		ZString sendingPartyCGN;

		public ZPropertyInfo SendingPartyCGNInfo => GetZPropertyInfo(nameof(SendingPartyCGN));

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CarrierCGN

		public ZString CarrierCGN
		{
			get => carrierCGN;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierCGNInfo, ref carrierCGN, value))
				{
					Validate(CarrierCGNInfo);
				}
			}
		}
		ZString carrierCGN;

		public ZPropertyInfo CarrierCGNInfo => GetZPropertyInfo(nameof(CarrierCGN));

		public RegistrationNumber CarrierCGNNumber
		{
			get => carrierCGNNumber;
			set => carrierCGNNumber = SetChild(carrierCGNNumber, value);
		}

		RegistrationNumber carrierCGNNumber;

		#endregion

		#region Master Air Waybill

		public ZString MasterAirWaybill
		{
			get => masterAirWaybill;
			set
			{
				if (SetNonPersistentPropertyValue(MasterAirWaybillInfo, ref masterAirWaybill, value))
				{
					Validate(MasterAirWaybillInfo);
				}
			}
		}
		ZString masterAirWaybill;

		public ZPropertyInfo MasterAirWaybillInfo => GetZPropertyInfo(nameof(MasterAirWaybill));

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}
		IUnloco operationalPort;

		#endregion

		#region Shipments

		public IReadOnlyCollection<Shipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}

		IReadOnlyCollection<Shipment> shipments;

		#endregion
	}
}
