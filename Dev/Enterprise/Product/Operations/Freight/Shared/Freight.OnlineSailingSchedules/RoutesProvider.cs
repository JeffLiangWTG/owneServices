using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Freight.OnlineSailingSchedules.Exceptions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class RoutesProvider : IRoutesProvider
	{
		public RoutesProvider(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		public Route[] GetRoutes(string searchParams, IServiceRequestManager requestManager)
		{
			Argument.NotNull(requestManager, nameof(requestManager));
			if (requestManager.IsSuppressed())
			{
				return Array.Empty<Route>();
			}

			IApiResponse<ServiceModel.Route[]> response = null;
			var endpoint = "api/v2/routes/search?" + searchParams;
			var client = ObjectFactory.Get<IApiClient>("HttpClient", FreightDataRegistry.Instance.OnlineSailingSchedulesUrl.Value);
			client.Timeout = requestManager.RequestTimeout;
			client.AccessToken = ((NoResString)"Basic", GetAuthToken());
			client.ExceptionFactory = OnlineSailingScheudlesExceptionFactory.Handle;
			client.ContentSerializer = new JavaScriptSerializerHttpContentSerializer();
			try
			{
				response = TryGetResponse(endpoint, client);
				response.EnsureSuccessStatusCodeAsync(true).GetAwaiter().GetResult();
				requestManager.OnSuccessfulRequest();
				return GetResponseData(response.Content);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				requestManager.HandleException(new RequestHandlingException(searchParams, response?.Response, ex), new Uri(client.Client.BaseAddress + endpoint));
			}
			finally
			{
				client.Dispose();
			}

			return Array.Empty<Route>();
		}

		protected virtual IApiResponse<ServiceModel.Route[]> TryGetResponse(string endpoint, IApiClient client)
		{
			return client.GetAsync<ServiceModel.Route[]>(endpoint).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		#region Implementation

		string GetAuthToken()
		{
			var tokenOverride = FreightDataRegistry.Instance.OnlineSailingSchedulesTokenOverride.Value;
			return !string.IsNullOrEmpty(tokenOverride) ? tokenOverride : Env.CurrentCompany.GetEHubAuthValue();
		}

		Route[] GetResponseData(ServiceModel.Route[] routes)
		{
			if (routes == null)
			{
				return Array.Empty<Route>();
			}
			return routes.Select(x =>
			{
				var route = new Route(factory);
				route.SetValues(x);
				return route;
			}).ToArray();
		}

		#endregion
	}
}
