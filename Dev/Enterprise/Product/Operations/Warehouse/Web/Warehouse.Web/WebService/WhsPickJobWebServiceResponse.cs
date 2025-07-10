using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public abstract class WhsPickJobWebServiceResponse<TPickJob> : WebServiceResponse
		where TPickJob : WhsPickJobInfo
	{
		public TPickJob Job { get; set; }
	}
}
