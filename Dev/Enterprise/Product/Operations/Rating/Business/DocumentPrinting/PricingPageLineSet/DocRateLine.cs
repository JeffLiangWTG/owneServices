using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class DocRateLine : ISortableDocLine, IDocLine
	{
		public DocRateLine(RateLine rateLine)
		{
			RateLine = rateLine;
		}

		#region SortableCharge

		public RateLine RateLine { get; }

		public OrgHeader ClientForSorting { get; set; }

		int ISortableDocLine.OrgLevelSortOrder
		{
			get
			{
				int result = 0;

				var chargeCodePK = RateLine.TL_AC;

				if (ClientForSorting.RatingDocumentsChargeOrders != null && RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.Value)
				{
					foreach (var chargeOrder in ClientForSorting.RatingDocumentsChargeOrders)
					{
						if (chargeOrder.RCO_AC_ChargeCode == chargeCodePK)
						{
							result = chargeOrder.RCO_PrintOrder;
							break;
						}
					}
				}
				return result;
			}
		}

		int ISortableDocLine.ChargePrintSeqSortOrder => RateLine.ChargeCode != null
			? (int)RateLine.ChargeCode.AC_PrintSequence
			: 0;

		int ISortableDocLine.UserEnteredSortOrder => RateLine.TL_LineOrder;

		string ISortableDocLine.AlphabeticalSortOrder => RateLine.GetMultilingualRateDesc();

		#endregion

		#region IDocLine

		bool IDocLine.PreventGrouping => false;

		#endregion
	}
}
