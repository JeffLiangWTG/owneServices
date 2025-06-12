using System.Web.Mvc;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
