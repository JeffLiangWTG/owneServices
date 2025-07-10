using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class PerformanceReportingUrlGenerator : IPerformanceReportingUrlGenerator
	{
		static readonly TimeSpan TokenExpirationTime = TimeSpan.FromMinutes(10);

		public IPerformanceReportingAuthTokenResult GetToken(string correlationId, OrgContact contact = null, CancellationToken ct = default)
		{
			var tokenProvider = ObjectFactory.Get<IAuthTokenProvider>();
			var (authToken, tokenValidationMessage) = tokenProvider.GetToken(correlationId, TokenExpirationTime, contact, ct);
			if (!string.IsNullOrEmpty(tokenValidationMessage))
			{
				return new PerformanceReportingAuthTokenResult(null, Res.GetString("DA2BE5B4-94F9-4252-9290-CFD614962601", "Unable to get permission to show the report: {0}", tokenValidationMessage));
			}

			if (string.IsNullOrEmpty(authToken))
			{
				throw new InvalidOperationException($"{nameof(authToken)} must not be null or empty.");
			}

			var token = new PerformanceReportingAuthToken(authToken);

			return new PerformanceReportingAuthTokenResult(token, null);
		}

		public (Uri url, string errorMessage) Generate(string path, IPerformanceReportingAuthToken token, bool isLayoutHidden)
		{
			if (!Uri.TryCreate(FreightDataRegistry.Instance.ReportingUrl.Value, UriKind.Absolute, out var baseUrl))
			{
				return (null, Res.GetString("37DE9743-CBF3-4BA6-B36C-D812C7785757", "Invalid registry item: Freight > Global Tracking > Performance Reporting > Reporting URL."));
			}

			var queryString = new QueryString();
			if (token?.Value != null)
			{
				queryString.Add((NoResString)"token", token.Value);
			}
			if (isLayoutHidden)
			{
				queryString.Add(name: (NoResString)"nolayout", value: (NoResString)"true");
			}

			var urlBuilder = new UriBuilder(baseUrl) { Path = path ?? string.Empty, Query = queryString.ToString() };
			return (urlBuilder.Uri, errorMessage: null);
		}
	}
}
