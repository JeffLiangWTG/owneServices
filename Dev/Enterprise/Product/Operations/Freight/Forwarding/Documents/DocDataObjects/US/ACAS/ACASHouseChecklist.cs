using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class ACASHouseChecklist : DocDataObject, IDataSourceProvider
	{
		public ACASHouseChecklist(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region State

		public AcasHouseChecklistMessageState State { get; set; } = AcasHouseChecklistMessageState.None;

		#endregion

		#region DisplayInformation

		public ZString DisplayInformation
		{
			get => displayInformation;
			set
			{
				if (SetNonPersistentPropertyValue(DisplayInformationInfo, ref displayInformation, value))
				{
				}
			}
		}

		ZString displayInformation;

		public ZPropertyInfo DisplayInformationInfo => GetZPropertyInfo(nameof(DisplayInformation));

		#endregion

		#region SendersAcasCode

		public ZString SendersAcasCode
		{
			get => sendersAcasCode;
			set
			{
				if (SetNonPersistentPropertyValue(SendersAcasCodeInfo, ref sendersAcasCode, value))
				{
				}
			}
		}

		ZString sendersAcasCode;

		public ZPropertyInfo SendersAcasCodeInfo => GetZPropertyInfo(nameof(SendersAcasCode));

		#endregion

		#region Numbers and References

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

		#region WaybillNumber

		public ZString WayBillNumber
		{
			get => wayBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(WayBillNumberInfo, ref wayBillNumber, value))
				{
					Validate(WayBillNumberInfo);
				}
			}
		}
		ZString wayBillNumber;

		public ZPropertyInfo WayBillNumberInfo => GetZPropertyInfo(nameof(WayBillNumber));

		#endregion

		#endregion

		#region Locations

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

		#region PortOfFirstArrival

		public IUnloco PortOfFirstArrival
		{
			get => portOfFirstArrival;
			set => portOfFirstArrival = SetChild(portOfFirstArrival, value);
		}
		IUnloco portOfFirstArrival;

		#endregion

		#endregion

		#region Quantities

		#region TotalNoOfPacks

		public ZInt TotalNoOfPacks
		{
			get => totalNoOfPacks;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNoOfPacksInfo, ref totalNoOfPacks, value))
				{
					Validate(TotalNoOfPacksInfo);
				}
			}
		}
		ZInt totalNoOfPacks;

		public ZPropertyInfo TotalNoOfPacksInfo => GetZPropertyInfo(nameof(TotalNoOfPacks));

		#endregion

		#region Weight

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		#endregion

		#endregion

		#region Addresses

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region BookingParty

		public Address BookingParty
		{
			get => bookingParty;
			set => bookingParty = SetChild(bookingParty, value);
		}

		Address bookingParty;

		#endregion

		#endregion

		#region Shipments

		public IReadOnlyCollection<ACASHouseChecklistShipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}
		IReadOnlyCollection<ACASHouseChecklistShipment> shipments;

		#endregion

		#region IDataSourceProvider members

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

		#endregion
	}
}
