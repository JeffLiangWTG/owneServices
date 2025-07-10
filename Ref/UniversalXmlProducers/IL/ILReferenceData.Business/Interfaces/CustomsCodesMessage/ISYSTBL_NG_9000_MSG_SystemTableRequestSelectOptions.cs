namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public interface ISYSTBL_NG_9000_MSG_SystemTableRequestSelectOptions
	{
		bool GetAsDataTable { get; }
		int PageNumber { get; }
		int PageSize { get; }
	}
}
