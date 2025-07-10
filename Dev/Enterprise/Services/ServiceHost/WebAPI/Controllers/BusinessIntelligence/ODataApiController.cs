using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Bi.Registration.Common;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	[Authorize]
	[CW1IdentityBasicAuthentication]
	public abstract class ODataApiController : BiWebServicesController
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Marker")]
		public const string MetaDataMarker = "$metadata";

		protected bool isBiODataApiRegistrySet =>
			!new BiReportUser().IsNull && SystemDataRegistry.Instance.BiODataAPI.Value;

		public IHttpActionResult GetOData(string tabularModel = null, string dataSetName = null)
		{
			if (isBiODataApiRegistrySet)
			{
				var isMetaDataRequest = Request.RequestUri.ToString().Contains(MetaDataMarker);
				return new OdataActionResult(tabularModel, dataSetName, this, isMetaDataRequest);
			}
			else
			{
				return BadRequest(Res.GetString("018EC7B6-3747-4B05-BB29-17CB29A05BC1", "Data {0} isn't retrieved.", dataSetName));
			}
		}

		public async Task<HttpResponseMessage> DoMetaDataRequest()
		{
			HttpResponseMessage serverResponse = null;
			using (ImpersonatedRequestHandler = new ImpersonatedWebRequestHandler(new BiReportUser()))
			using (var client = new HttpClient(ImpersonatedRequestHandler))
			{
				ImpersonatedRequestHandler.UseDefaultCredentials = false;

				var metaRequestUrl = $"{ReportService.PowerBiWebPortalUrl}/api/v2.0/$metadata#DataSetData";

				HttpRequestMessage proxyRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(metaRequestUrl));
				try
				{
					serverResponse = await client.SendAsync(proxyRequest).ConfigureAwait(false);
				}
				catch (HttpRequestException ex)
				{
					var errorResponse = await CannotConnectToReportServerMessage(ex.Message);
					errorResponse.RequestMessage = proxyRequest;
					return errorResponse;
				}

				if (serverResponse.StatusCode == HttpStatusCode.Unauthorized)
				{
					return await ReportService.ShowUnauthorisedUserMessage();
				}
				else
				{
					using (var byteStream = await serverResponse.Content.ReadAsStreamAsync())
					{
						using (var reader = new StreamReader(byteStream))
						{
							serverResponse.Content.Headers.ContentType.Parameters.Clear();
							serverResponse.Content.Headers.ContentType.MediaType = "application/xml";
						}
					}
				}

				return serverResponse;
			}
		}

		public async Task<HttpResponseMessage> DoDataRequest(string tabularModel, string dataSetName)
		{
			HttpResponseMessage serverResponse = null;

			using (ImpersonatedRequestHandler = new ImpersonatedWebRequestHandler(new BiReportUser()))
			using (var client = new HttpClient(ImpersonatedRequestHandler))
			{
				ImpersonatedRequestHandler.UseDefaultCredentials = false;

				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				string datasetPath = string.Empty;
				datasetPath = $"{ReportService.PowerBiWebPortalUrl}/api/v2.0/Datasets(Path='/{registrationKey.EnterpriseCode}/{registrationKey.ServerCode}/Analytics/{tabularModel}/{dataSetName}')/data{Request.RequestUri.Query}";

				HttpRequestMessage proxyRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(datasetPath));
				try
				{
					serverResponse = await client.SendAsync(proxyRequest).ConfigureAwait(false);
				}
				catch (HttpRequestException ex)
				{
					var errorResponse = await CannotConnectToReportServerMessage(ex.Message);
					errorResponse.RequestMessage = proxyRequest;
					return errorResponse;
				}

				if (serverResponse.StatusCode == HttpStatusCode.Unauthorized)
				{
					return await ReportService.ShowUnauthorisedUserMessage();
				}
				else
				{
					using (var byteStream = await serverResponse.Content.ReadAsStreamAsync())
					{
						using (var reader = new StreamReader(byteStream))
						{
							serverResponse.Content.Headers.ContentType.Parameters.Clear();

							var datasetContent = reader.ReadToEnd();
							var enterpriseValue = WebDataRegistry.Instance.RootServicesUri.Value;
							if (!enterpriseValue.EndsWith("/", StringComparison.OrdinalIgnoreCase))
							{
								enterpriseValue = enterpriseValue + "/";
							}

							var modifiedContent = datasetContent.Replace($"{ReportService.PowerBiWebPortalUrl}/api/v2.0", RequestUrl);
							serverResponse.Content = new StringContent(modifiedContent);
							serverResponse.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("odata.metadata", (NoResString)"minimal"));
							serverResponse.Content.Headers.ContentType.MediaType = "application/json";
						}
					}
				}

				return serverResponse;
			}
		}

		protected abstract string RequestUrl { get; }
	}
}
