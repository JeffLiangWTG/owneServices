
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	static class OrgAddressExtensions
	{
		public static ZString GetCountryCode(this OrgAddress orgAddress) => orgAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
	}
}
