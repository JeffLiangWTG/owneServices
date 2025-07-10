using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class RECR89Generator
	{
		public RECR89Generator()
		{
			feesSummary = new Dictionary<string, OriginalEstimate>();
		}

		public IEnumerable<RECR89> Generate()
		{
			if (feesSummary.Count == 0)
			{
				var r89 = new RECR89();
				r89.FeeSummaryTrailerNumber = 1;
				yield return r89;
			}
			else
			{
				int i = 0;
				int countOf89 = 1;

				RECR89 r89 = null;
				foreach (KeyValuePair<string, OriginalEstimate> pair in feesSummary)
				{
					if (r89 == null)
					{
						r89 = new RECR89();
						r89.FeeSummaryTrailerNumber = countOf89++;
					}
					string feeClass = pair.Key;
					decimal originalFee = pair.Value.OriginalFee;
					decimal estimateFee = pair.Value.EstimateFee;
					i++;
					switch (i)
					{
						case 1:
							r89.FeeClass = feeClass;
							r89.TotalOriginalFee = originalFee;
							r89.TotalEstimateReconciliationFee = estimateFee;
							break;
						case 2:
							r89.FeeClass1 = feeClass;
							r89.TotalOriginalFee1 = originalFee;
							r89.TotalEstimateReconciliationFee1 = estimateFee;
							break;
						case 3:
							r89.FeeClass2 = feeClass;
							r89.TotalOriginalFee2 = originalFee;
							r89.TotalReconciliationFee2 = estimateFee;
							break;
					}
					if (i == 3)
					{
						yield return r89;
						r89 = null;
						i = 0;
					}
				}
				if (r89 != null)
				{
					yield return r89;
				}
			}
		}

		internal void AddFees(IEnumerable<IReconciliationImportEntryFee> fees, bool isWaiveRefund)
		{
			foreach (IReconciliationImportEntryFee fee in fees)
			{
				string feeClass = fee.FeeClass;
				OriginalEstimate result;
				if (!feesSummary.TryGetValue(feeClass, out result))
				{
					result = new OriginalEstimate();
					feesSummary.Add(feeClass, result);
				}
				result.OriginalFee += fee.OriginalFee;

				var reconFee = isWaiveRefund && fee.EstimatedReconciliationFee < fee.OriginalFee ? fee.OriginalFee : fee.EstimatedReconciliationFee;
				result.EstimateFee += reconFee;
			}
		}

		internal void AddRefundedFees(IEnumerable<ZString> fees)
		{
			foreach (string fee in fees)
			{
				if (!feesSummary.ContainsKey(fee))
				{
					feesSummary.Add(fee, new OriginalEstimate());
				}
			}
		}

		class OriginalEstimate
		{
			public decimal OriginalFee;
			public decimal EstimateFee;
		}

		readonly Dictionary<string, OriginalEstimate> feesSummary;
	}
}
