using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CBP7512DocumentLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZString MarksAndNumbers { get; set; }
		public ZString DescriptionAndQtyOfMerchandise { get; set; }
		public ZString WeightFormatted { get; set; }
		public ZString GoodsValueInLocalCurrency { get; set; }
		public ZBool GoodsValueEstimated { get; set; }
		public ZString Rate { get; set; }
		public ZString Duty { get; set; }

		public void UpdateFrom(CusInBondMoveLineItem moveLineItem)
		{
			MarksAndNumbers = moveLineItem.BI_MarksAndNumbers;
			DescriptionAndQtyOfMerchandise = moveLineItem.BI_Description;

			if (moveLineItem.BI_Weight > 0)
			{
				var weight = moveLineItem.Weight;
				if (Core.Constants.Weight.IsImperial(weight.Unit))
				{
					WeightFormatted = weight.InPoundsSafe.Round(0).ToString() + " L";
				}
				else
				{
					WeightFormatted = weight.InKilogramsSafe.Round(0).ToString() + " K";
				}
			}

			if (moveLineItem.BI_MonetaryValue > 0)
			{
				GoodsValueInLocalCurrency = moveLineItem.BI_MonetaryValue.ToStringTrimZeros();
				GoodsValueEstimated = moveLineItem.BI_IsMonetaryValueEstimated;
			}

			Rate = moveLineItem.BI_RateComment;
			Duty = moveLineItem.BI_DutyComment;
		}
	}
}
