using System.Net;
using System.Web.Http;
using CargoWise.PAVE.Common.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	abstract class PaveControllerTestCase<TService, TController> : TestCase
		where TController : BasePaveController<TService>, new()
	{
		readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
		};

		protected static string GetResult(IHttpActionResult result)
		{
			var response = result.ExecuteAsync(new System.Threading.CancellationToken()).GetAwaiter().GetResult();
			return response.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
		}

		protected static HttpStatusCode GetStatusCode(IHttpActionResult result)
		{
			var response = result.ExecuteAsync(new System.Threading.CancellationToken()).GetAwaiter().GetResult();
			return response.StatusCode;
		}

		protected string SerializeToPaveResponseJson<T>(T obj = null, PaveError error = null)
			where T : class
		{
			var paveResponse = new PaveResponse<T>(obj, error);
			return JsonConvert.SerializeObject(paveResponse, serializerSettings);
		}

		public abstract void TestOnCreateController_ShouldUseDbConnectionCorrectly();
	}
}
