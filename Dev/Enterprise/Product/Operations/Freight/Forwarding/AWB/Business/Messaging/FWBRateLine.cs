using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	[DebuggerDisplay("IsRateDescriptionEmpty: {IsRateDescriptionEmpty} NatureAndQtyOfGoods: {NatureAndQtyOfGoods.Text}")]
	sealed class FWBRateLine
	{
		public ZString CommodityItemNumber { get; set; }

		public ZString NoOfPiecesOrRCP { get; set; }
		public ZString WeightInLBsOrKGs { get; set; }

		public ZDecimal GrossWeight { get; set; }
		public ZDecimal ChargeableWeight { get; set; }

		public ZString RateClass { get; set; }
		public ZDecimal RateChargeOrDiscount { get; set; }
		public ZDecimal Total { get; set; }

		public FWBNatureAndQtyOfGoods NatureAndQtyOfGoods { get; set; }

		public ZBool IsRateDescriptionEmpty
		{
			get
			{
				return (NoOfPiecesOrRCP == "0" || NoOfPiecesOrRCP.IsEmpty)
					&& GrossWeight == 0M
					&& CommodityItemNumber == ""
					&& RateChargeOrDiscount == 0M
					&& WeightInLBsOrKGs.Trim() == ""
					&& RateClass.Trim() == ""
					&& ChargeableWeight == 0M;
			}
		}
	}
}
