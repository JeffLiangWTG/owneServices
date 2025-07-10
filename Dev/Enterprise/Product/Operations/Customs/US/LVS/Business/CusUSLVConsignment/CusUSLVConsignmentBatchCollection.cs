using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.US.LVS.Business;

public class CusUSLVConsignmentBatchCollection
{
	public CusUSLVConsignmentBatchCollection(IList<CusUSLVConsignment> consignments)
	{
		batches = GenerateBatches(consignments);
	}

	public bool MoveNextBatch()
	{
		var nextIndex = currentBatchIndex + 1;
		var result = !batches.IsNullOrEmpty() && nextIndex < batches.Count;
		if (result)
		{
			currentBatchIndex = nextIndex;
		}
		else
		{
			if (!batches.IsNullOrEmpty())
			{
				currentBatchIndex -= batches.Count;
			}
		}

		return result;
	}

	IList<IList<CusUSLVConsignment>> GenerateBatches(IList<CusUSLVConsignment> consignments)
	{
		IList<IList<CusUSLVConsignment>> result = null;
		if (!consignments.IsNullOrEmpty())
		{
			result = new List<IList<CusUSLVConsignment>>();
			var workingBatch = new List<CusUSLVConsignment>();
			var workingBatchLineCount = 0;
			foreach (var consignment in consignments)
			{
				if (workingBatchLineCount > 0 && workingBatchLineCount + consignment.CusUSLVItems.Count > BatchThreshold)
				{
					result.Add(workingBatch);
					workingBatch = [];
					workingBatchLineCount = 0;
				}

				workingBatch.Add(consignment);
				workingBatchLineCount += consignment.CusUSLVItems.Count;
			}

			result.Add(workingBatch);
		}

		return result;
	}

	public IList<CusUSLVConsignment> CurrentBatch => currentBatchIndex < batches?.Count ? batches[currentBatchIndex] : null;

	public int TotalBatchCount => batches?.Count ?? 0;

	int currentBatchIndex = -1;
	readonly IList<IList<CusUSLVConsignment>> batches;
	const int BatchThreshold = 999;
}
