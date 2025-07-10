using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	public sealed class AirCargoAdvanceScreening : DocDataObject, IDataSourceProvider
	{
		public AirCargoAdvanceScreening(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region HAWB

		public ZString HAWB
		{
			get => hawb;
			set
			{
				if (SetNonPersistentPropertyValue(HAWBInfo, ref hawb, value))
				{
				}
			}
		}

		ZString hawb;

		public ZPropertyInfo HAWBInfo => GetZPropertyInfo(nameof(HAWB));

		#endregion

		#region MAWB

		public ZString MAWB
		{
			get => mawb;
			set
			{
				if (SetNonPersistentPropertyValue(MAWBInfo, ref mawb, value))
				{
				}
			}
		}

		ZString mawb;

		public ZPropertyInfo MAWBInfo => GetZPropertyInfo(nameof(MAWB));

		#endregion

		#region ConsolNumber

		public ZString ConsolNumber
		{
			get => consolNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsolNumberInfo, ref consolNumber, value))
				{
				}
			}
		}

		ZString consolNumber;

		public ZPropertyInfo ConsolNumberInfo => GetZPropertyInfo(nameof(ConsolNumber));

		#endregion

		#region ConsolType

		public ICodeDescription ConsolType
		{
			get => consolType;
			set => consolType = SetChild(consolType, value);
		}

		ICodeDescription consolType;

		#endregion

		#region SendersFirmsCode

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

		#region NotifyPartysAcasCode

		public ZString NotifyPartysAcasCode
		{
			get => notifyPartysAcasCode;
			set
			{
				if (SetNonPersistentPropertyValue(NotifyPartysAcasCodeInfo, ref notifyPartysAcasCode, value))
				{
				}
			}
		}

		ZString notifyPartysAcasCode;

		public ZPropertyInfo NotifyPartysAcasCodeInfo => GetZPropertyInfo(nameof(NotifyPartysAcasCode));

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region HarmonizedCodes

		public IReadOnlyCollection<ZString> HarmonizedCodes { get; set; }

		#endregion

		#region FlightNumber

		public ZString FlightNumber
		{
			get => flightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(FlightNumberInfo, ref flightNumber, value))
				{
				}
			}
		}

		ZString flightNumber;

		public ZPropertyInfo FlightNumberInfo => GetZPropertyInfo(nameof(FlightNumber));

		#endregion

		#region ETA

		public ZDateTime ETA
		{
			get => eta;
			set
			{
				if (SetNonPersistentPropertyValue(ETAInfo, ref eta, value))
				{
				}
			}
		}

		ZDateTime eta;

		public ZPropertyInfo ETAInfo => GetZPropertyInfo(nameof(ETA));

		#endregion

		#region NumberOfPacks

		public ZInt NumberOfPacks
		{
			get => numberOfPacks;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfPacksInfo, ref numberOfPacks, value))
				{
				}
			}
		}

		ZInt numberOfPacks;

		public ZPropertyInfo NumberOfPacksInfo => GetZPropertyInfo(nameof(NumberOfPacks));

		#endregion

		#region Weight

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		#endregion

		#region Departure

		public IUnloco Departure
		{
			get => departure;
			set => departure = SetChild(departure, value);
		}

		IUnloco departure;

		public ZString PortOfOriginIata
		{
			get => portOfOriginIata;
			set
			{
				if (SetNonPersistentPropertyValue(PortOfOriginIataInfo, ref portOfOriginIata, value))
				{
				}
			}
		}

		ZString portOfOriginIata;

		public ZPropertyInfo PortOfOriginIataInfo => GetZPropertyInfo(nameof(PortOfOriginIata));

		#endregion

		#region Arrival

		public IUnloco Arrival
		{
			get => arrival;
			set => arrival = SetChild(arrival, value);
		}

		IUnloco arrival;

		public ZString PortOfFirstArrivalIata
		{
			get => portOfFirstArrivalIata;
			set
			{
				if (SetNonPersistentPropertyValue(PortOfFirstArrivalIataInfo, ref portOfFirstArrivalIata, value))
				{
				}
			}
		}

		ZString portOfFirstArrivalIata;

		public ZPropertyInfo PortOfFirstArrivalIataInfo => GetZPropertyInfo(nameof(PortOfFirstArrivalIata));

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CTO

		public Address CTO
		{
			get => cto;
			set => cto = SetChild(cto, value);
		}

		Address cto;

		#endregion

		#region Shipper

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		Address shipper;

		#endregion

		#region Consignee

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		Address consignee;

		#endregion

		#region BookingParty

		public Address BookingParty
		{
			get => bookingParty;
			set => bookingParty = SetChild(bookingParty, value);
		}

		Address bookingParty;

		#endregion

		#region NotifyParty

		public Address NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}

		Address notifyParty;

		#endregion

		#region NotifyPartyType

		public ICodeDescription NotifyPartyType
		{
			get => notifyPartyType;
			set => notifyPartyType = SetChild(notifyPartyType, value);
		}

		ICodeDescription notifyPartyType;

		#endregion

		#region CanSendMessage

		public ZBool CanSendMessage => State == AcasState.None
			|| State == AcasState.AcknowledgementRequired
			|| State == AcasState.AmendmentRequired
			|| State == AcasState.HoldRemoved
			|| State == AcasState.AssessmentComplete;

		#endregion

		#region State

		public AcasState State { get; set; } = AcasState.None;

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

		#region IDataSourceProvider members

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

		#endregion
	}
}
