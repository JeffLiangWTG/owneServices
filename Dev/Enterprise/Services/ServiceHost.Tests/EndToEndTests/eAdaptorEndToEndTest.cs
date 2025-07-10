using System;
using System.Net.Http;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;

namespace Enterprise.Services.ServiceHost.Tests
{
	public abstract class eAdaptorEndToEndTest : TestCaseWithFactory
	{
		public string PostRequest(string requestXml) => eAdaptorTestHelper.PostRequest(requestXml);
		public string PostRequest(IDataObject requestObject) => eAdaptorTestHelper.PostRequest(requestObject);
	}

	public abstract class eAdaptorEndToEndNonTransactionedTest : NonTransactionedTestCase
	{
		public string PostRequest(string requestXml) => eAdaptorTestHelper.PostRequest(requestXml);

		public string PostRequest(IDataObject requestObject) => eAdaptorTestHelper.PostRequest(requestObject);
	}

	public static class eAdaptorTestHelper
	{
		public static string PostRequest(string requestXml)
		{
			return PostRequest(new StringContent(requestXml));
		}

		public static string PostRequest(IDataObject requestObject)
		{
			using (var requestStream = TopLevelDataObjectConverter.SerializeToStream(requestObject))
			{
				return PostRequest(new StreamContent(requestStream));
			}
		}

		public static UniversalResponse<T> PostRequestWithUniversalResponse<T>(IDataObject requestObject)
			where T : TopLevelDataObject, new()
		{
			using (var requestStream = TopLevelDataObjectConverter.SerializeToStream(requestObject))
			{
				var response = PostRequest(new StreamContent(requestStream));
				return new UniversalResponse<T>(response);
			}
		}

		static string PostRequest(HttpContent requestContent)
		{
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = requestContent;
				controller.Request = request;
				using (var response = controller.Post())
				{
					return response.Content.ReadAsStringAsync().Result;
				}
			}
		}

		public static void TurnOnVerboseLogging() => eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
	}
}
