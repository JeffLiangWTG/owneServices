namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public sealed class SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapper : ISYSTBL_NG_9000_MSG_SystemTableRequestSelectOptions
	{
		public bool GetAsDataTable => true;
		public int PageNumber => 1;
		public int PageSize => 999;
	}
}
