using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	public class OdataActionResult : IHttpActionResult
	{
		readonly string dataSetName;

		readonly string tabularModel;

		readonly ODataApiController oDataApiController;

		readonly bool isMetaDataRequest;

		public OdataActionResult(string tabularModel, string dataSetName, ODataApiController oDataApiController, bool isMetaDataRequest)
		{
			this.tabularModel = tabularModel;
			this.dataSetName = dataSetName;
			this.oDataApiController = oDataApiController;
			this.isMetaDataRequest = isMetaDataRequest;
		}
		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			if (isMetaDataRequest)
			{
				return oDataApiController.DoMetaDataRequest();
			}
			return oDataApiController.DoDataRequest(tabularModel, dataSetName);
		}
	}
}
