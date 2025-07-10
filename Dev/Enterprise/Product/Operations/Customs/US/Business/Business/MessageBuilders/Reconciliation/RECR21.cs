using System.Collections.Generic;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	static class RECR21Populator
	{
		public static RECR21 Populate(int trailerNumber, List<IReconciliationImportEntryFee> fees)
		{
			RECR21 r21 = null;
			if (fees.Count > 0)
			{
				r21 = new RECR21();
				r21.TrailerNumber = trailerNumber;
				r21.FirstFeeClass = fees[0].FeeClass;
				r21.FirstOriginalFee = fees[0].OriginalFee;
				r21.FirstEstimateReconciliationFee = fees[0].EstimatedReconciliationFee;

				if (fees.Count > 1)
				{
					r21.SecondFeeClass = fees[1].FeeClass;
					r21.SecondOriginalFee = fees[1].OriginalFee;
					r21.SecondEstimateReconciliationFee = fees[1].EstimatedReconciliationFee;

					if (fees.Count > 2)
					{
						r21.ThirdFeeClass = fees[2].FeeClass;
						r21.ThirdOriginalFee = fees[2].OriginalFee;
						r21.ThirdEstimatedReconciliationFee = fees[2].EstimatedReconciliationFee;
					}
				}
			}
			return r21;
		}
	}
}
