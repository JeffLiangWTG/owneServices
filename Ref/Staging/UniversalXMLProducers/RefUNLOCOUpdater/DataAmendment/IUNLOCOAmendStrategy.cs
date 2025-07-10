namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.DataAmendment
{
	public interface IUNLOCOAmendStrategy<T> 
	{
		void Amend(T entity);
		bool VerifyStrategy();
		void LoadAmendmentData();
		void FinalizeAmendment();
	}
}
