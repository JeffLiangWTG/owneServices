using System.Web.Http;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	public class FinanceOdataAPIController : ODataApiController
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Model name")]
		public const string TabularModel = "Finance";
		protected override string RequestUrl
		{
			get
			{
				var enterpriseValue = WebDataRegistry.Instance.RootServicesUri.Value;
				return $"{enterpriseValue}api/analytics/odata/finance";
			}
		}

		[Route("api/analytics/odata/finance/{dataSetName}")]
		[HttpGet]
		public IHttpActionResult GetFinanceOdataDataQuery(string dataSetName = null)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var businessAreaCheckPoint = Env.Security.ODataFinance;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					return BadAPIRequestMessage(businessAreaCheckPoint.ErrorMessageForNotAllowed);
				}
			}
			return GetOData(TabularModel, dataSetName);
		}
	}
}
