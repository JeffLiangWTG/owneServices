using System;
using System.Web.Http.Results;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ODataApiControllerTest : BiControllerTest
	{
		public void TestGetOData()
		{
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeDataWarehouseServer"))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" }))
			{
				var controller = new SomeOdataApiController();

				using (SystemDataRegistry.Instance.BiODataAPI.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var result = controller.GetOData(dataSetName: "BadTest");
					AssertEquals("Data BadTest isn't retrieved.", ((BadRequestErrorMessageResult)result).Message);
				}
			}
		}

		protected class SomeOdataApiController : ODataApiController
		{
			protected override string RequestUrl
			{
				get
				{
					return "XXX";
				}
			}
		}
	}
}
