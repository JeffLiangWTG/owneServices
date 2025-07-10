
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class OrgAddressDependentCollectionExtensions
	{
		public static OrgAddress AddNew(this OrgAddressDependentCollection addresses, string code, string address1)
		{
			var address = addresses.AddNew();
			address.OA_Code = code;
			address.OA_Address1 = address1;

			return address;
		}
	}
}

#endif