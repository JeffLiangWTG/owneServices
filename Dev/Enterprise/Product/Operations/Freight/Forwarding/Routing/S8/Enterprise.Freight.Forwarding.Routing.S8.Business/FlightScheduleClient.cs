
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Routing.S8.Business.SoapConstants;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	class FlightScheduleClient : IFlightScheduleClient
	{
		readonly IApiClient apiClient;
		readonly Uri uri;

		public FlightScheduleClient(Uri uri)
		{
			Argument.NotNull(uri, nameof(uri));
			this.uri = uri;
			apiClient = ObjectFactory.Get<IApiClient>("HttpClient", uri.ToString());
			apiClient.Accept = new List<string> { (NoResString)"application/soap+xml" };
			apiClient.MediaType = XmlContentType;
			apiClient.Timeout = GetRequestTimeoutInSeconds();
			apiClient.ContentSerializer = new SoapStringContentSerializer();
			apiClient.RetryCount = 0;
		}

		TimeSpan GetRequestTimeoutInSeconds()
		{
			var res = FreightDataRegistry.Instance.S8CommunicationTimeoutInSeconds.Value * 1000;

			if (res <= 0
				|| res == int.MaxValue)
			{
				res = 5;
			}

			return TimeSpan.FromSeconds(res);
		}

		public string GetFlight(string token, string sset, string flightDate, string airline, int flightNumber)
		{
			Argument.NotNull(sset, nameof(sset));
			Argument.NotNull(airline, nameof(airline));

			var message = flightDate.IsNullOrEmpty() ? string.Format(CultureInfo.InvariantCulture, SoapMessage.FlightWithoutDateMessageTemplate, nameof(SoapAction.GetFlight), SecurityElement.Escape(token), sset, airline, flightNumber)
				: string.Format(CultureInfo.InvariantCulture, SoapMessage.FlightWithDateMessageTemplate, nameof(SoapAction.GetFlight), SecurityElement.Escape(token), sset, flightDate, airline, flightNumber);
			var response = Post(SoapAction.GetFlight, message, CancellationToken.None).EnsureSuccessStatusCodeAsync().GetAwaiter().GetResult();
			return ParseTagValue(response.Content, nameof(SoapAction.GetFlight));
		}

		public string SolveRouting(string token, string problem)
		{
			Argument.NotNull(problem, nameof(problem));

			var message = string.Format(CultureInfo.InvariantCulture, SoapMessage.SolveRoutingMessageTemplate, nameof(SoapAction.SolveRouting), SecurityElement.Escape(token), problem);
			var response = Post(SoapAction.SolveRouting, message, CancellationToken.None).EnsureSuccessStatusCodeAsync().GetAwaiter().GetResult();
			return ParseTagValue(response.Content, nameof(SoapAction.SolveRouting));
		}

		public string LogInS8C(string userID, string password, string computer, string loginID, string program, string clientVersion)
		{
			Argument.NotNull(userID, nameof(userID));
			Argument.NotNull(password, nameof(password));
			Argument.NotNull(loginID, nameof(loginID));
			Argument.NotNull(program, nameof(program));

			var message = string.Format(CultureInfo.InvariantCulture, SoapMessage.LoginMessageTemplate, nameof(SoapAction.LogInS8C), userID, password, computer, loginID, program, clientVersion);
			var response = Post(SoapAction.LogInS8C, message, CancellationToken.None).EnsureSuccessStatusCodeAsync().GetAwaiter().GetResult();
			return ParseTagValue(response.Content, nameof(SoapAction.LogInS8C));
		}

		#region Implementation

		IApiResponse<string> Post(string action, string message, CancellationToken cancellationToken)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(message, nameof(message));

			apiClient.CustomHeaders = new List<(string, string)> { ((NoResString)SoapAction.HeaderName, action) };
			return apiClient.PostAsync<string>(uri.ToString(), message, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		static string ParseTagValue(string response, string action)
		{
			Argument.NotNull(response, nameof(response));
			Argument.NotNull(action, nameof(action));
			
			var matched = Regex.Match(response, $"<{action}Result>(?<TagValue>(.|\n)*?)<\\/{action}Result>");
			if (!matched.Success)
			{
				return null;
			}

			var tagValue = matched.Groups["TagValue"]?.Value ?? matched.Value;
			return tagValue;
		}

		public void Dispose()
		{
			apiClient.Dispose();
		}

		#endregion
	}
}
