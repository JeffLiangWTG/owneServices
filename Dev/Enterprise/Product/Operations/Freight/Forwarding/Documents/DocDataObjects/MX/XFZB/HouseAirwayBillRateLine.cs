using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX
{
	sealed class HouseAirwayBillRateLine : DocDataObject
	{
		#region GrossWeightAndUnit

		public IMeasurement GrossWeight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		IMeasurement weight;

		#endregion

		#region NoOfPiecesOrRCP

		public ZInt NoOfPieces
		{
			get => noOfPieces;
			set
			{
				if (SetNonPersistentPropertyValue(NoOfPiecesInfo, ref noOfPieces, value))
				{
					Validate(NoOfPiecesInfo);
				}
			}
		}
		ZInt noOfPieces;

		public ZPropertyInfo NoOfPiecesInfo => GetZPropertyInfo(nameof(NoOfPieces));

		#endregion

		#region NatureAndQtyOfGoods

		public ZString NatureAndQtyOfGoods
		{
			get => natureAndQtyOfGoods;
			set
			{
				if (SetNonPersistentPropertyValue(NatureAndQtyOfGoodsInfo, ref natureAndQtyOfGoods, value))
				{
					Validate(NatureAndQtyOfGoodsInfo);
				}
			}
		}

		ZString natureAndQtyOfGoods;

		public ZPropertyInfo NatureAndQtyOfGoodsInfo => GetZPropertyInfo(nameof(NatureAndQtyOfGoods));

		#endregion

		#region RateClass

		public ZString RateClass
		{
			get => rateClass;
			set
			{
				if (SetNonPersistentPropertyValue(RateClassInfo, ref rateClass, value))
				{
					Validate(RateClassInfo);
				}
			}
		}
		ZString rateClass;

		public ZPropertyInfo RateClassInfo => GetZPropertyInfo(nameof(RateClass));

		#endregion

		#region CommodityItemNumber

		public ZString CommodityItemNumber
		{
			get => commodityItemNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CommodityItemNumberInfo, ref commodityItemNumber, value))
				{
					Validate(CommodityItemNumberInfo);
				}
			}
		}
		ZString commodityItemNumber;

		public ZPropertyInfo CommodityItemNumberInfo => GetZPropertyInfo(nameof(CommodityItemNumber));

		#endregion

		#region ChargeableWeightAndUnit

		public IMeasurement ChargeableWeight
		{
			get => chargeableWeight;
			set => chargeableWeight = SetChild(chargeableWeight, value);
		}

		IMeasurement chargeableWeight;

		#endregion

		#region RateChargeOrDiscount

		public ZDecimal RateChargeOrDiscount
		{
			get => rateChargeOrDiscount;
			set
			{
				if (SetNonPersistentPropertyValue(RateChargeOrDiscountInfo, ref rateChargeOrDiscount, value))
				{
					Validate(RateChargeOrDiscountInfo);
				}
			}
		}
		ZDecimal rateChargeOrDiscount;

		public ZPropertyInfo RateChargeOrDiscountInfo => GetZPropertyInfo(nameof(RateChargeOrDiscount));

		#endregion

		#region Total

		public ZDecimal Total
		{
			get => total;
			set
			{
				if (SetNonPersistentPropertyValue(TotalInfo, ref total, value))
				{
					Validate(TotalInfo);
				}
			}
		}
		ZDecimal total;

		public ZPropertyInfo TotalInfo => GetZPropertyInfo(nameof(Total));

		#endregion
	}
}
