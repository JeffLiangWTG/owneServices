namespace Enterprise.MasterFiles.Business
{
	public interface ISalesRelatedBusinessObject
	{
		void AddFetchHintsForSalesEstimatedValueChange();
		void OnSalesEstimatedValueChange();
		bool IsDeleted { get; }
	}
}
