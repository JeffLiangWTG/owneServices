using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public static class BatchCalculator
	{
		public static (int From, int To) CalculateFromToIndexForBatch(int batchNumber, int pagesPerBatch)
		{
			var from = (batchNumber - 1) * pagesPerBatch * Constants.CUSNumbers.ItemsPerPage;
			var to = from + pagesPerBatch * Constants.CUSNumbers.ItemsPerPage - 1;
			return (from, to);
		}
	}
}
