using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitRateLine : DocDataObject
	{
		#region NoOfPiecesOrRCP

		public ZInt NoOfPieces
		{
			get => noOfPieces;
			set
			{
				if (SetNonPersistentPropertyValue(NoOfPiecesInfo, ref noOfPieces, value))
				{
				}
			}
		}
		ZInt noOfPieces;

		public ZPropertyInfo NoOfPiecesInfo => GetZPropertyInfo(nameof(NoOfPieces));

		#endregion

		#region RateClass

		public ZString RateClass
		{
			get => rateClass;
			set
			{
				if (SetNonPersistentPropertyValue(RateClassInfo, ref rateClass, value))
				{
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
				}
			}
		}
		ZString commodityItemNumber;

		public ZPropertyInfo CommodityItemNumberInfo => GetZPropertyInfo(nameof(CommodityItemNumber));

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		IMeasurement weight;

		#endregion

		#region ChargeableWeight

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
				}
			}
		}
		ZDecimal total;

		public ZPropertyInfo TotalInfo => GetZPropertyInfo(nameof(Total));

		#endregion

		#region NatureAndQtyOfGoods

		public ZString NatureAndQtyOfGoods
		{
			get => natureAndQtyOfGoods;
			set
			{
				if (SetNonPersistentPropertyValue(NatureAndQtyOfGoodsInfo, ref natureAndQtyOfGoods, value))
				{
				}
			}
		}

		ZString natureAndQtyOfGoods;

		public ZPropertyInfo NatureAndQtyOfGoodsInfo => GetZPropertyInfo(nameof(NatureAndQtyOfGoods));

		#endregion

		#region IsHSCodeLine

		public ZBool IsHSCodeLine
		{
			get => isHSCodeLine;
			set
			{
				if (SetNonPersistentPropertyValue(IsHSCodeLineInfo, ref isHSCodeLine, value))
				{
				}
			}
		}

		ZBool isHSCodeLine;

		public ZPropertyInfo IsHSCodeLineInfo => GetZPropertyInfo(nameof(IsHSCodeLine));

		#endregion
	}
}
