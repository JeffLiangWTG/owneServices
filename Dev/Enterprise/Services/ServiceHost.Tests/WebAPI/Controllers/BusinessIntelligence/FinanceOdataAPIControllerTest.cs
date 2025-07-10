using System.Web.Http.Results;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test;

namespace Enterprise.Services.ServiceHost.Tests
{
	class FinanceOdataAPIControllerTest : BiControllerTest
	{
		public void TestGetFinanceOdataDataQuery()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.ODataFinance.IsAllowed = false;
				var controller = new FinanceOdataAPIController();
				
				var result = controller.GetFinanceOdataDataQuery("TestDataSet");
				AssertEquals("Incorrect API Request: You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\nManage -> Business Intelligence & Analytics -> Allow O data -> Allow O data for Finance", ((BadRequestErrorMessageResult)result).Message);
			}
		}
	}
}
