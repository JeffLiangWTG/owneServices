using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using AuthenticationService.Client;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Rating.Business
{
	public class WTGAuthTokenProvider : IAuthTokenProvider
	{
		readonly LoginInfo environmentLoginInfoFromConstructor;

		public WTGAuthTokenProvider()
			: this(new WTGAuthServiceClientFactory())
		{
		}

		public WTGAuthTokenProvider(IWTGAuthServciceClientFactory authServiceClientFactory)
		{
			Argument.NotNull(authServiceClientFactory, nameof(authServiceClientFactory));

			this.authServiceClientFactory = authServiceClientFactory;
			this.ClientID = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			environmentLoginInfoFromConstructor = GetEnvironmentLoginInfo();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Doesn't matter in our case, we specify in the name that the value is in seconds")]
		public (string Token, string ValidationMessage) GetToken(
			string correlationID,
			int secondsBeforeTokenExpiry = 30,
			Action<LoginInfo> overrideLoginInfo = null,
			CancellationToken cancellationToken = default,
			bool useEnvironmentLoginInfoFromConstructor = false)
		{
			lock (lockObject)
			{
				return GetTokenCore(correlationID, secondsBeforeTokenExpiry, overrideLoginInfo, cancellationToken, useEnvironmentLoginInfoFromConstructor);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Doesn't matter in our case, we specify in the name that the value is in seconds")]
		protected virtual (string Token, string ValidationMessage) GetTokenCore(
			string correlationID,
			int secondsBeforeTokenExpiry = 30,
			Action<LoginInfo> overrideLoginInfo = null,
			CancellationToken cancellationToken = default,
			bool useEnvironmentLoginInfoFromConstructor = false)
		{
			try
			{
				var environmentLoginInfo = useEnvironmentLoginInfoFromConstructor
					? environmentLoginInfoFromConstructor
					: GetEnvironmentLoginInfo();

				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var clientNumber = string.IsNullOrEmpty(environmentLoginInfo.CompanyCode)
					? Invariant($"{registrationKey.SystemId}") // not a client facing string
					: Invariant($"{registrationKey.SystemId}.{environmentLoginInfo.CompanyCode}"); // not a client facing string

				var login = new LoginInfo
				{
					Type = LoginType.CW1Client,
					DatabaseNumber = registrationKey.DatabaseNumber,
					EnterpriseCode = registrationKey.EnterpriseCode,
					ServerCode = registrationKey.ServerCode,
					Password = registrationKey.Password,
					CompanyCode = environmentLoginInfo.CompanyCode,
					CompanyName = environmentLoginInfo.CompanyName,
					CountryCode = environmentLoginInfo.CountryCode,
					BranchCode = environmentLoginInfo.BranchCode,
					BranchPort = environmentLoginInfo.BranchPort,
					DepartmentCode = environmentLoginInfo.DepartmentCode,
					ClientNumber = clientNumber,
					UserCode = environmentLoginInfo.UserCode,
					UserFullName = environmentLoginInfo.UserFullName,
					UserEmail = environmentLoginInfo.UserEmail,
					Roles = GetRoles(),
					IsTestSystem = registrationKey.DatabaseType != DatabaseTypes.Codes.Production,
				};

				if (overrideLoginInfo != null)
				{
					overrideLoginInfo(login);
				}

				var tokenKey = GenerateTokenKey(login);

				// If we already have a token which will not expire within secondsBeforeTokenExpiry
				if (tokens.TryGetValue(tokenKey, out var tokenInfo))
				{
					if (!tokenInfo.ExpiryDate.HasValue || tokenInfo.ExpiryDate.Value > DateTimeOffset.UtcNow.AddSeconds(secondsBeforeTokenExpiry))
					{
						return (tokenInfo.Token, string.Empty);
					}
				}

				if (string.IsNullOrEmpty(AuthServiceURL))
				{
					return (null,
						Res.GetString("883be1bd-5d41-4cc1-bcb6-6c824521934b",
							"Authentication service URL is not configured"));
				}

				using (var authService = authServiceClientFactory.Create(AuthServiceURL))
				{
					var newToken = authService.GetToken(login, correlationID, cancellationToken);

					tokens[tokenKey] = new TokenInfo { Token = newToken, ExpiryDate = GetExpiryDate(newToken) };

					return (newToken, string.Empty);
				}
			}
			catch (HttpRequestException ex)
			{
				using (Db.DisposableActionForDbConnection())
				{
					ex.NotifyUserIfRequired(AuthServiceURL);
				}

				return (null,
					Res.GetString("11339a62-5f1a-11e9-b4f3-1c1b0d09faa1",
						"Unable to connect to the authentication service at the moment"));
			}
			catch (AuthServiceException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
			{
				return (null,
					Res.GetString("075e18a4-7125-4db6-b0e4-73d5239dac93",
						"The CW1 system cannot be validated. Please check if it has a valid license."));
			}
			catch (AuthServiceException ex) when (ex.StatusCode == HttpStatusCode.RequestTimeout)
			{
				return (null,
					Res.GetString("b5d3bed7-b9af-4ff4-aae9-4fce82ea01e2",
						"CargoWise could NOT get an authentication token due to time out. Please check your internet connection and try again in a few minutes."));
			}
			catch (AuthServiceException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
			{
				return (null,
					Res.GetString("8626f076-4456-4ead-b075-970fc7b6d080",
						"CargoWise could NOT get an authentication token due to the service being unavailable. Please check your internet connection and try again in a few minutes."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				using (Db.DisposableActionForDbConnection())
				{
					ErrorReporter.ReportOnce("AuthService.GetToken.Failure", Invariant($"An unexpected error occurred while requesting an authentication token: {ex.Message}"), ex);
				}

				return (null,
					Res.GetString("d06add17-cb2f-4384-80a0-d1c4acc4461b",
						"Unable to request an authentication token due to unexpected error. An Error Report has been sent to the development team."));
			}
		}

		string[] GetRoles()
		{
			var roles = new List<string>();

			using (Db.DisposableActionForDbConnection())
			{
				if (Env.Security.WiseRatesCargoSphereContractManagement.IsAllowed)
				{
					roles.Add(WTGRoles.CargoSphereRateAdmin);
				}
				if (Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed)
				{
					roles.Add(WTGRoles.CargoSphereStandardUser);
				}

				if (Env.Security.WiseRatesCargoguideRateSearch.IsAllowed)
				{
					roles.Add(WTGRoles.CargoguideStandardUser);
				}
				else
				{
					roles.Add(WTGRoles.CargoguideAutoCostingUser);
				}
			}

			return roles.ToArray();
		}

		public string ClientID { get; }

		public string AuthServiceURL => RatingDataRegistry.Instance.WTGAuthServiceUrl.Value;

		string GenerateTokenKey(LoginInfo loginInfo)
		{
			var propertyInfos = loginInfo.GetType().GetProperties();

			var tokenKey = propertyInfos
				.Where(propertyInfo => propertyInfo.Name != nameof(loginInfo.Type))
				.Select(propertyInfo => GetLoginInfoValue(propertyInfo, loginInfo))
				.ToStringWithDelimiterBetweenStrings(".");

			return tokenKey;
		}

		static string GetLoginInfoValue(PropertyInfo propertyInfo, LoginInfo loginInfo)
		{
			var value = propertyInfo.GetValue(loginInfo);

			if (value == null)
			{
				return string.Empty;
			}
			else if (value is IEnumerable<UrsSecurityAccessRights> rights)
			{
				return string.Join("|", rights.Select(FlattenAccessRights));
			}
			else if (value is string[] array)
			{
				return array.ToStringWithDelimiterBetweenStrings(",");
			}
			else
			{
				return value.ToString();
			}
		}

		static string FlattenAccessRights(UrsSecurityAccessRights accessRights)
		{
			if (accessRights == null)
			{
				return string.Empty;
			}

			var allowed = accessRights.Allowed != null
				? string.Join(",", accessRights.Allowed?.Companies ?? Array.Empty<string>()) + "|" + string.Join(",", accessRights.Allowed?.Countries ?? Array.Empty<string>())
				: string.Empty;
			var denied = accessRights.Denied != null
				? string.Join(",", accessRights.Denied?.Companies ?? Array.Empty<string>()) + "|" + string.Join(",", accessRights.Denied?.Countries ?? Array.Empty<string>())
				: string.Empty;

			return string.Join(",", $"{accessRights.TransportMode}|{allowed}|{denied}");
		}

		internal static DateTimeOffset? GetExpiryDate(string token)
		{
			var payload = ExtractPayload(token);

			if (payload.Expiration.HasValue)
			{
				return DateTimeOffset.FromUnixTimeSeconds(payload.Expiration.Value);
			}
			else
			{
				return null;
			}
		}

		static JwtPayload ExtractPayload(string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				throw new ArgumentNullException(nameof(token));
			}

			var jwtParts = token.Split('.');
			if (jwtParts.Length != 3)
			{
				throw new ArgumentException("Not a valid JWT token", nameof(token));
			}

			return JwtPayload.Base64UrlDeserialize(jwtParts[1]);
		}

		LoginInfo GetEnvironmentLoginInfo()
		{
			return new LoginInfo
			{
				CompanyCode = GlbCompany.CurrentCompany.GC_Code,
				CompanyName = GlbCompany.CurrentCompany.GC_Name,
				CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				BranchCode = GlbBranch.CurrentBranch.GB_Code,
				BranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort,
				DepartmentCode = GlbDepartment.CurrentDepartment.GE_Code,
				UserCode = GlbStaff.CurrentUser.GS_Code,
				UserFullName = GlbStaff.CurrentUser.GS_FullName,
				UserEmail = GlbStaff.CurrentUser.GS_EmailAddress,
			};
		}

		readonly IWTGAuthServciceClientFactory authServiceClientFactory;
		readonly object lockObject = new object();

		readonly Dictionary<string, TokenInfo> tokens = new Dictionary<string, TokenInfo>();
		class TokenInfo
		{
			public string Token;
			public DateTimeOffset? ExpiryDate;
		}
	}
}
