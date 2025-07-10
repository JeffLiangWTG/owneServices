
namespace Enterprise.Freight.Business.Extensions
{
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	public static class OrgHeaderExtensions
	{
		public static OrgHeader WithCustomsCode(this OrgHeader org, ZString code, ZString codeValue, string countryCode = null)
		{
			org.CustomsCodes.AddNew(code, codeValue, countryCode);
			return org;
		}
	}
}
