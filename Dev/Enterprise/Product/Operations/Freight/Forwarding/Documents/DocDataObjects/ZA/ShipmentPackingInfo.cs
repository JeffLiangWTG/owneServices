using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA
{
	public class ShipmentPackingInfo : DocDataObject
	{
		public ShipmentPackingInfo(object identifier)
		: base(identifier)
		{
		}

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region TotalWeight

		public IMeasurement TotalWeight
		{
			get => totalWeight;
			set => totalWeight = SetChild(totalWeight, value);
		}

		IMeasurement totalWeight;

		#endregion

		#region OuterPacks

		public ZInt OuterPacks
		{
			get => outerPacks;
			set
			{
				if (SetNonPersistentPropertyValue(OuterPacksInfo, ref outerPacks, value))
				{
					Validate(OuterPacksInfo);
				}
			}
		}
		ZInt outerPacks;

		public ZPropertyInfo OuterPacksInfo => GetZPropertyInfo(nameof(OuterPacks));

		#endregion

		#region PackType

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}
		ICodeDescription packType;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		IAddress consignee;

		#endregion

		#region Consignor

		public IAddress Consignor
		{
			get => consignor;
			set => consignor = SetChild(consignor, value);
		}

		IAddress consignor;

		#endregion

		#region GoodsInfoCollection

		public IReadOnlyCollection<GoodsInfo> GoodsInfoCollection
		{
			get => goodsInfoCollection;
			set => goodsInfoCollection = SetChildCollection(goodsInfoCollection, value);
		}

		IReadOnlyCollection<GoodsInfo> goodsInfoCollection;

		#endregion

		#region AllGoodsInfoIncludeCoLoadCollection

		public IEnumerable<GoodsInfo> AllGoodsInfoIncludeCoLoadCollection => GetGoodsInfos(this);

		IEnumerable<GoodsInfo> GetGoodsInfos(ShipmentPackingInfo shipment)
		{
			var shipmentType = shipment?.ShipmentType?.Code ?? ZString.Empty;

			if (shipment?.GoodsInfoCollection != null
				&& (shipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead
					|| shipmentType == Core.Constants.ShipmentTypes.StandardHouse
					|| shipmentType == Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse))
			{
				foreach (var goodsInfo in shipment.GoodsInfoCollection)
				{
					yield return goodsInfo;
				}
			}

			if (shipment?.ShipmentPackingInfos != null)
			{
				foreach (var coLoadShipment in shipment.ShipmentPackingInfos)
				{
					foreach (var goodsInfo in GetGoodsInfos(coLoadShipment))
					{
						yield return goodsInfo;
					}
				}
			}
		}

		#endregion

		#region CoLoadShipments

		public IReadOnlyCollection<ShipmentPackingInfo> ShipmentPackingInfos
		{
			get => shipmentPackingInfos;
			set => shipmentPackingInfos = SetChildCollection(shipmentPackingInfos, value);
		}

		IReadOnlyCollection<ShipmentPackingInfo> shipmentPackingInfos;

		#endregion
	}
}
