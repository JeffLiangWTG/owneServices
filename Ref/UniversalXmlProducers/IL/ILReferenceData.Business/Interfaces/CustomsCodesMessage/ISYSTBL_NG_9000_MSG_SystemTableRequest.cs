namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public interface ISYSTBL_NG_9000_MSG_SystemTableRequest
	{
		IRequestContentHeader RequestContentHeader { get; }
		ISYSTBL_NG_9000_MSG_SystemTableRequestSelectOptions SelectOptions { get; }
		string TableName { get; }
	}
}
