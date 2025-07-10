
namespace Enterprise.MasterFiles.Business
{
	public struct ProductLoadResult
	{
		public ProductLoadResult(OrgSupplierPart bestMatchingProduct, int totalMatchCount, int totalNumerOfPartsCount)
		{
			this.BestMatchingProduct = bestMatchingProduct;
			this.TotalMatchCount = totalMatchCount;
			this.TotalNumerOfPartsCount = totalNumerOfPartsCount;
		}

		public readonly OrgSupplierPart BestMatchingProduct;
		public readonly int TotalMatchCount;
		public readonly int TotalNumerOfPartsCount;
	}
}
