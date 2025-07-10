using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitHouseManifest : DocDataObject, IDataSourceProvider
	{
		public CargoControlAndTransitHouseManifest(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region Header

		#region SendingParty

		public IAddress SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		IAddress sendingParty;

		#endregion SendingParty

		#region ReceivingAgent

		public IAddress ReceivingAgent
		{
			get => receivingAgent;
			set => receivingAgent = SetChild(receivingAgent, value);
		}

		IAddress receivingAgent;

		#endregion

		#region MAWB

		public ZString Mawb
		{
			get => mawb;
			set
			{
				if (SetNonPersistentPropertyValue(MawbInfo, ref mawb, value))
				{
					Validate(MawbInfo);
				}
			}
		}

		ZString mawb;

		public ZPropertyInfo MawbInfo => GetZPropertyInfo(nameof(Mawb));

		#endregion MAWB

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

		#endregion ConsolNumber

		#region Packs

		public ZInt Packs
		{
			get => packs;
			set
			{
				if (SetNonPersistentPropertyValue(PacksInfo, ref packs, value))
				{
					Validate(PacksInfo);
				}
			}
		}

		ZInt packs;

		public ZPropertyInfo PacksInfo => GetZPropertyInfo(nameof(Packs));

		#endregion Packs

		#region Weight

		public IMeasurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		IMeasurement weight;

		#endregion

		#region AirportOfDeparture

		public IUnloco AirportOfDeparture
		{
			get => airportOfDeparture;
			set => airportOfDeparture = SetChild(airportOfDeparture, value);
		}

		IUnloco airportOfDeparture;

		#endregion AirportOfDeparture

		#region AirportOfDestination

		public IUnloco AirportOfDestination
		{
			get => airportOfDestination;
			set => airportOfDestination = SetChild(airportOfDestination, value);
		}

		IUnloco airportOfDestination;

		#endregion AirportOfDestination

		#region PortOfFirstArrival

		public IUnloco PortOfFirstArrival
		{
			get => portOfFirstArrival;
			set => portOfFirstArrival = SetChild(portOfFirstArrival, value);
		}

		IUnloco portOfFirstArrival;

		#endregion PortOfFirstArrival

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion PortOfOrigin

		#endregion Header

		#region Shipments

		public IReadOnlyCollection<CargoControlAndTransitDetail> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}
		IReadOnlyCollection<CargoControlAndTransitDetail> shipments;

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
					Validate(ErrorPlaceHolderInfo);
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion ErrorPlaceHolder

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion
	}
}
