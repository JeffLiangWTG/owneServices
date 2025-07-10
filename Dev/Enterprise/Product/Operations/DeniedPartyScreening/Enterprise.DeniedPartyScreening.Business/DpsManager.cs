using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.Http;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class DpsManager : IDpsManager
	{
		public IDpsServiceV4 GetDpsService(string url)
		{
			var httpClient = GetHttpClientWithHeaders(url);
			return new DpsServiceV4(httpClient);
		}

		public void DeactivateBillingEntities(Guid[] entityPKs)
		{
			throw new NotImplementedException();
		}

		public async Task<DpsResponse> Screen(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, IDpsServiceV4 service)
		{
			var services = GetDpsServices(service);

			return await GetScreenResult(dpsRequestHeaderWithAddressMatching, services);
		}

		static HttpClient GetHttpClientWithHeaders(string url)
		{
			var httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			var httpClient = httpClientFactory
				.CreateNew(
					new HttpClientHandlerWithDiagnostics(new CookieContainer()),
					GetTimeout(DpsContext.Request));

			httpClient.BaseAddress = new Uri(url);
			httpClient.DefaultRequestHeaders.Authorization = TokenManager.GetInstance(MDMProductCodes.DPS).GetAuthorizationHeaderValue();
			httpClient.DefaultRequestHeaders.Add("LicenceCode", HMACSHA256Helper.GetComputedLicenceCode(GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-")));

			if (!string.IsNullOrEmpty(EnvProxy.Instance.CurrentUser.LoginName))
			{
				httpClient.DefaultRequestHeaders.Add("UserNameBase64Encoded", Convert.ToBase64String(Encoding.UTF8.GetBytes(EnvProxy.Instance.CurrentUser.LoginName)));
			}

			return httpClient;
		}

		public List<IDpsServiceV4> GetDpsServices(IDpsServiceV4 service)
		{
			var services = new List<IDpsServiceV4>();
			var isInternal = ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();

			if (service == null)
			{
				var registryValue = OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.Value;
				var urlRows = registryValue.Cast<DpsWebServiceItem>();

				if (isInternal)
				{
					services.Add(GetDpsService(urlRows.Single(x => x.Role == RoleHelper.Code.Staging).WebServiceUrl));
				}
				else
				{
					services.AddRange(urlRows.Where(x => x.Role != RoleHelper.Code.Staging).Select(urlRow => GetDpsService(urlRow.WebServiceUrl)));
				}
			}
			else
			{
				services.Add(service);
			}

			return services;
		}

		public async Task<DpsResponse> GetScreenResult(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, List<IDpsServiceV4> services)
		{
			return await ScreenWithFailover(services, dpsRequestHeaderWithAddressMatching);
		}

		async Task<DpsResponse> ScreenWithFailover(List<IDpsServiceV4> services, DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching)
		{
			DpsResponse result = null;
			await FailoverHandler(services, async service =>
			{
				result = await service.SearchDeniedParties(dpsRequestHeaderWithAddressMatching);
			});

			return result;
		}

		async Task FailoverHandler(IEnumerable<IDpsServiceV4> services, Func<IDpsServiceV4, Task> asyncAction)
		{
			var exceptionInfo = new StringBuilder((NoResString)"Denied Party Screening has encountered errors with each of the service urls: ");
			exceptionInfo.AppendLine();
			var aggregateExceptions = new List<Exception>();

			foreach (var service in services)
			{
				try
				{
					await asyncAction(service);
					return;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					exceptionInfo.AppendLine();
					exceptionInfo.AppendLine(service.BaseUrl);

					switch (ex)
					{
						case TaskCanceledException _:
							exceptionInfo.AppendLine((NoResString)@"Service is accessible but has not responded in a timely manner. Please refer to the registry setting ""Screening Request"" under Master Data -> Organizations -> Denied Party Screening -> Timeouts.");
							break;

						case ApiException apiException:
							exceptionInfo.AppendLine($@"Service is unaccessible with Http response status code: {apiException.StatusCode}");
							break;

						default:
							exceptionInfo.AppendLine(ex.Message);
							break;
					}

					aggregateExceptions.Add(ex);
				}
			}

			throw new DpsCommunicationException(exceptionInfo.ToString(), new AggregateException(aggregateExceptions));
		}

		static TimeSpan GetTimeout(DpsContext context)
		{
			int registryValue;

			switch (context)
			{
				case DpsContext.Request:
					registryValue = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForScreeningRequests.Value;
					break;
				case DpsContext.RescreeningServiceTask:
					registryValue = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForRescreeningServiceTask.Value;
					break;
				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Denied Party Screening Service has no timeout configured for context: {0}", context.ToString()));
			}

			return new TimeSpan(0, 0, registryValue);
		}
	}
}
