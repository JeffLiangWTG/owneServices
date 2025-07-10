using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitDetail : DocDataObject, IDataSourceProvider
	{
		public CargoControlAndTransitDetail(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentNumberInfo);
				}
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region Hawb

		public ZString Hawb
		{
			get => hawb;
			set
			{
				if (SetNonPersistentPropertyValue(HawbInfo, ref hawb, value))
				{
					Validate(HawbInfo);
				}
			}
		}
		ZString hawb;

		public ZPropertyInfo HawbInfo => GetZPropertyInfo(nameof(Hawb));

		#endregion Hawb

		#region Origin

		public IUnloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		IUnloco origin;

		#endregion Origin

		#region Destination

		public IUnloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		IUnloco destination;

		#endregion Destination

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

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		#endregion

		#region Status

		public ZString Status
		{
			get => status;
			set
			{
				if (SetNonPersistentPropertyValue(StatusInfo, ref status, value))
				{
					Validate(StatusInfo);
				}
			}
		}

		ZString status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		#endregion Status

		#region EventDateTime

		public ZDateTime EventDateTime
		{
			get => eventDateTime;
			set
			{
				if (SetNonPersistentPropertyValue(EventDateTimeInfo, ref eventDateTime, value))
				{
					Validate(EventDateTimeInfo);
				}
			}
		}

		ZDateTime eventDateTime;

		public ZPropertyInfo EventDateTimeInfo => GetZPropertyInfo(nameof(EventDateTime));

		#endregion EventDateTime

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

		#endregion GoodsDescription

		#region IDataSourceProvider members

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

		#endregion
	}
}
