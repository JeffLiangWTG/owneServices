using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust.Extensions;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.IdentitySecurity;

namespace Enterprise.Freight.Business
{
	public sealed class CargoTrackerUrlGenerator : ICargoTrackerUrlGenerator
	{
		static readonly TimeSpan TokenExpirationTime = TimeSpan.FromMinutes(10);

		public CargoTrackerAuthTokenResult GetToken(string correlationId, OrgContact contact = null, CancellationToken ct = default)
		{
			var tokenProvider = ObjectFactory.Get<IAuthTokenProvider>();
			var (authToken, tokenValidationMessage) = tokenProvider.GetToken(correlationId, TokenExpirationTime, contact, ct);
			if (!string.IsNullOrEmpty(tokenValidationMessage))
			{
				return new CargoTrackerAuthTokenResult { ErrorMessage = Res.GetString("c59df408-682d-4c6e-b610-8960c60ff158", "Unable to get permission to show Cargo Tracker: {0}", tokenValidationMessage) };
			}

			if (string.IsNullOrEmpty(authToken))
			{
				throw new InvalidOperationException($"{nameof(authToken)} must not be null or empty");
			}

			var token = new CargoTrackerAuthToken(CargoTrackerAuthTokenType.Rating, authToken);

			return new CargoTrackerAuthTokenResult { Token = token };
		}

		public CargoTrackerAuthTokenResult GetSelfSignedSystemToSystemToken(string licenseCode)
		{
			if (string.IsNullOrWhiteSpace(licenseCode) || licenseCode.Length < 9)
			{
				return new CargoTrackerAuthTokenResult { ErrorMessage = Res.GetString("ec3c3d5a-3387-4c0e-88fd-f6365602694b", "{0} application must have a valid license code.", BrandingFactory.Instance.ProductName) };
			}
			var systemInfo = SystemDataRegistry.Instance.SystemToSystemCertificate?.Value;
			if (systemInfo is null || !systemInfo.IsSetUp())
			{
				return new CargoTrackerAuthTokenResult { ErrorMessage = $"{nameof(SystemDataRegistry.SystemToSystemCertificate)} is not valid. Please ensure System to System Trust configuration has been setup and the TCM task has run successfully" };
			}
			if (string.IsNullOrWhiteSpace(FreightDataRegistry.Instance.CargoTrackerApiAudienceId?.Value))
			{
				return new CargoTrackerAuthTokenResult { ErrorMessage = Res.GetString("4509e54f-d2dc-48c8-bc78-42bb8cf80fae", "Invalid registry item value: Freight > Global Tracking > Cargo Tracker > Cargo Tracker API Audience ID.") };
			}
			if (string.IsNullOrWhiteSpace(FreightDataRegistry.Instance.AisApiAudienceId?.Value))
			{
				return new CargoTrackerAuthTokenResult { ErrorMessage = Res.GetString("725270e2-7d3e-4a1d-a97c-17846268bcc7", "Invalid registry item value: Freight > Global Tracking > Cargo Tracker > AIS API Audience ID.") };
			}

#pragma warning disable CS0618 // To be replaced once a new implementation for generating a self-signed S2ST token is available
			var privateKey = RSAKeyProvider.ImportPrivateKey(systemInfo.PrivateKey);
#pragma warning restore CS0618
			var thumbPrint = new X509Certificate2(systemInfo.CertificateBytes).Thumbprint;

			var (enterpriseCode, databaseServerCode) = GetEnterpriseCodeAndDatabaseServerCode(licenseCode);
			var payload = new JwtPayload
			{
				{ (NoResString)"iss", "WTG" },
				{ (NoResString)"azp", systemInfo.ClientId },
				{ (NoResString)"iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
				{ (NoResString)"nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
				{ (NoResString)"exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds() },
				{ (NoResString)"aud", new[]
					{
						FreightDataRegistry.Instance.CargoTrackerApiAudienceId.Value,
						FreightDataRegistry.Instance.AisApiAudienceId.Value
					}
				},
				{ (NoResString)"SystemCode", enterpriseCode },
				{ (NoResString)"ServerCode", databaseServerCode },
				{ (NoResString)"jti", Guid.NewGuid().ToString() },
				{ (NoResString)"kid", thumbPrint },
			};
			var certificate = new X509Certificate2(systemInfo.CertificateBytes);
			var accessToken = JwtSecurity.GenerateSignedJwt(privateKey, certificate, payload);
			var token = new CargoTrackerAuthToken(CargoTrackerAuthTokenType.SelfSignedSystemToSystem, accessToken);

			return new CargoTrackerAuthTokenResult
			{
				Token = token
			};
		}

		public (Uri url, string errorMessage) Generate(CargoTrackerAuthToken token, string licenseCode, string consignmentNumber)
		{
			if (string.IsNullOrEmpty(licenseCode))
			{
				return (null, Res.GetString("ec3c3d5a-3387-4c0e-88fd-f6365602694b", "{0} application must have a valid license code.", BrandingFactory.Instance.ProductName));
			}

			if (string.IsNullOrEmpty(consignmentNumber))
			{
				return (null, Res.GetString("d43f0be0-9fb9-42f9-b424-54d47071fd10", "Consol form must have a valid job number."));
			}

			if (!Uri.TryCreate(FreightDataRegistry.Instance.CargoTrackerUrl.Value, UriKind.Absolute, out var baseUrl))
			{
				return (null, Res.GetString("6d3a5f50-603d-4bee-8d31-0fdf4470e497", "Invalid registry item: Freight > Global Tracking > Cargo Tracker > Cargo Tracker URL."));
			}

			var (enterpriseCode, databaseServerCode) = GetEnterpriseCodeAndDatabaseServerCode(licenseCode);
			var clientCode = $"{enterpriseCode}{databaseServerCode}";

			var url = BuildCargoTrackerUrl(baseUrl, clientCode, consignmentNumber, token);

			return (url, null);
		}

		static Uri BuildCargoTrackerUrl(Uri baseUrl, string clientCode, string consignmentNumber, CargoTrackerAuthToken token)
		{
			var result = new UriBuilder(baseUrl);

			result.Path = "/clients/" + clientCode + "/consignments/" + consignmentNumber + "/tracking";
			if (!string.IsNullOrWhiteSpace(token?.Value))
			{
				result.Query = GetQuery(token).ToString();
			}

			return result.Uri;
		}

		static QueryString GetQuery(CargoTrackerAuthToken token)
		{
			var query = new QueryString();

			switch (token.Type)
			{
				case CargoTrackerAuthTokenType.Rating:
				case CargoTrackerAuthTokenType.SelfSignedSystemToSystem:
					query["token"] = token.Value;
					break;
				default:
					throw new InvalidOperationException($"Unexpected CargoTrackerAuthTokenType type {token.Type}");
			}

			return query;
		}

		static (string enterpriseCode, string databaseServerCode) GetEnterpriseCodeAndDatabaseServerCode(string licenseCode)
		{
			var enterpriseCode = licenseCode.Substring(0, 3);
			var databaseServerCode = licenseCode.Substring(6, 3);
			return (enterpriseCode, databaseServerCode);
		}
	}
}
