using System;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Freight.OnlineSailingSchedules.Exceptions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallProvider : IPortCallProvider
	{
		public PortCall GetPortCall(string searchParams, IServiceRequestManager requestManager)
		{
			Argument.NotNull(requestManager, nameof(requestManager));
			if (requestManager.IsSuppressed())
			{
				return new PortCall();
			}
			IApiResponse<PortCall> response = null;
			var endpoint = (NoResString)"api/v2/portcalls?filter=" + WebUtility.UrlEncode(searchParams);
			var client = ObjectFactory.Get<IApiClient>("HttpClient", FreightDataRegistry.Instance.OnlineSailingSchedulesUrl.Value);
			client.Timeout = requestManager.RequestTimeout;
			client.AccessToken = ((NoResString)"Basic", Env.CurrentCompany.GetEHubAuthValue());
			client.ExceptionFactory = OnlineSailingScheudlesExceptionFactory.Handle;
			client.ContentSerializer = new JavaScriptSerializerHttpContentSerializer();
			try
			{
				response = TryGetResponse(endpoint, client);
				response.EnsureSuccessStatusCodeAsync(true).GetAwaiter().GetResult();
				requestManager.OnSuccessfulRequest();
				return response.Content ?? new PortCall();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				requestManager.HandleException(new RequestHandlingException(searchParams, response?.Response, ex), new Uri(client.Client.BaseAddress + endpoint));
			}
			finally
			{
				client.Dispose();
			}

			return new PortCall();
		}

		protected virtual IApiResponse<PortCall> TryGetResponse(string endpoint, IApiClient client)
		{
			return client.GetAsync<PortCall>(endpoint).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}
