using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PackOrderToPackageWebServiceResponse : WebServiceResponse
	{
		public Guid NewPackagePK { get; set; }
		public string NewPackageID { get; set; }
	}
}
