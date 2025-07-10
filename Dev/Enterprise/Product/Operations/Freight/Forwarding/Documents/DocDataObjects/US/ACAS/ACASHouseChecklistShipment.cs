using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class ACASHouseChecklistShipment : DocDataObject, IDataSourceProvider
	{
		public ACASHouseChecklistShipment(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region Numbers and References

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

		#region WayBillNumber

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
					Validate(DisplayInformationInfo);
				}
			}
		}

		ZString displayInformation;

		public ZPropertyInfo DisplayInformationInfo => GetZPropertyInfo(nameof(DisplayInformation));

		#endregion

		#region ACASEventDate

		public ZDateTime ACASEventDate
		{
			get => acasEventDate;
			set
			{
				if (SetNonPersistentPropertyValue(ACASEventDateInfo, ref acasEventDate, value))
				{
					Validate(ACASEventDateInfo);
				}
			}
		}
		ZDateTime acasEventDate;

		public ZPropertyInfo ACASEventDateInfo => GetZPropertyInfo(nameof(ACASEventDate));

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

		#endregion

		#region Weights and Measures

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

		#region IDataSourceProvider members

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

		#endregion
	}
}
