
namespace Enterprise.MasterFiles.Business.Customs.US
{
	public static class AutocreatefromMID
	{
		public const string FullName = "AUTO CREATED FROM MID";
		public const string Address1 = "A MESSAGE HAS BEEN SENT TO US CUSTOMS TO REQUEST";

		public static bool IsMIDOrganization(OrgHeader orgHeader)
		{
			return orgHeader != null && orgHeader.OH_FullName == FullName && orgHeader.MainAddress.OA_Address1 == Address1;
		}
	}
}
