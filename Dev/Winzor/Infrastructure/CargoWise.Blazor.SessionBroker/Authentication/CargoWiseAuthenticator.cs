using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWiseNext.Infrastructure.Authentication;
using Enterprise.Security.Shared;
using Microsoft.Extensions.Logging;
using WTG.OpenIDConnect.Token;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CargoWise.Blazor.SessionBroker.Authentication
{
	public class CargoWiseAuthenticator : ICargoWiseAuthenticator
	{
		readonly IDatabaseAccessor databaseAccessor;
		readonly IAuthenticationDatabaseAccessor authDbAccessor;
		readonly IAuthenticationConfigAccessor registryAccessor;
		readonly ILogger logger;
		readonly ISiteOfflineChecker siteOfflineChecker;

		public CargoWiseAuthenticator(
			IDatabaseAccessor databaseAccessor,
			IAuthenticationDatabaseAccessor authDbAccessor,
			IAuthenticationConfigAccessor registryAccessor,
			ILogger logger,
			ISiteOfflineChecker siteOfflineChecker)
		{
			this.databaseAccessor = databaseAccessor;
			this.authDbAccessor = authDbAccessor;
			this.registryAccessor = registryAccessor;
			this.logger = logger;
			this.siteOfflineChecker = siteOfflineChecker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public async Task<VerificationResult> VerifyAccessTokenAsync(
			CargoWiseAuthCookie cargoWiseAuthCookie,
			CancellationToken cancellationToken)
		{
			logger.LogInformation("VerifyAccessTokenAsync: Start Validating Identity token");
			if (cargoWiseAuthCookie is null)
			{
				logger.LogWarning("cargoWiseAuthCookie is null");
				return VerificationResult.GeneralFailure;
			}

			try
			{
				if (siteOfflineChecker.IsSiteOffline())
				{
					logger.LogWarning("Site is offline");
					return VerificationResult.SiteOffline;
				}

				logger.LogInformation("Get OIDC Config");
				var oidcConfig = registryAccessor.GetOIDCConfig();

				logger.LogInformation("Validating identity token ...");
				var jwtSecurityToken = await TokenValidator.ValidateIdentityToken(
						oidcConfig.AuthorityURL,
						oidcConfig.ClientIdentifier,
						cargoWiseAuthCookie.IdentityToken,
						new ConcurrentDictionary<string, IConfigurationManagerWithLock>(),
						logger,
						cancellationToken);
				logger.LogInformation("Validating identity token completed");
				if (jwtSecurityToken is null)
				{
					logger.LogWarning("Invalid or expired access token, jwtSecurityToken = null");
					return VerificationResult.GeneralFailure;
				}

				logger.LogInformation("VerifyUserByOidcClaims: Validating user based on user claims ...");
				var loginAuthenticationInfo = VerifyUserByOidcClaims(oidcConfig, jwtSecurityToken.Claims);
				if (loginAuthenticationInfo.isSuccess)
				{
					logger.LogInformation($"VerifyUserByOidcClaims: Valid user: {loginAuthenticationInfo.userID}");
					return VerificationResult.Success;
				}

				logger.LogWarning($"VerifyUserByOidcClaims: failed.");
				return VerificationResult.AuthorisationFailed;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Unexpected error while validating the identity token");
				return VerificationResult.GeneralFailure;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public (bool isSuccess, string userID) VerifyUserByOidcClaims(
			Enterprise.Integration.IOIDCConfig oidcConfig,
			IEnumerable<Claim> claims)
		{
			var claimData = new OidcLoginClaimData(claims, oidcConfig);
			if (claimData.Error == null)
			{
				if (!claimData.DbIdentifierToClaimMap.Any())
				{
					logger.LogWarning("Found NO identifier in mappings.");
					return (false, null);
				}

				foreach (var dbIdentifierToClaimMapItem in claimData.DbIdentifierToClaimMap)
				{
					if (TryGetUserFromDb(dbIdentifierToClaimMapItem, out var userId))
					{
						return (true, userId);
					}
				}

				logger.LogInformation("Found NO matched user.");
				return (false, null);
			}
			else
			{
				logger.LogInformation(claimData.Error);
				return (false, null);
			}
		}

		bool TryGetUserFromDb(KeyValuePair<string, string> dbIdentifierToClaimMapItem, out string foundUserIdentifier)
		{
			logger.LogInformation($"Check user with: {dbIdentifierToClaimMapItem}");
			var (schemaColumn, _) = OidcLoginDatabaseSchemaHelper.GetWhitelistedSchemaColumnFromName(dbIdentifierToClaimMapItem.Key);

			const string userIDParamName = "@claimValue";

			var sql = @$"
				SELECT TOP 1 {schemaColumn.Name}
				FROM {schemaColumn.TableName}
				WHERE {schemaColumn.Name}={userIDParamName}";

			foundUserIdentifier = databaseAccessor.ExecuteScalar<string>(
				commandText: sql,
				commandType: System.Data.CommandType.Text,
				parameters: (userIDParamName, dbIdentifierToClaimMapItem.Value));

			logger.LogInformation($"Found user: {foundUserIdentifier}, with {dbIdentifierToClaimMapItem}");

			return !string.IsNullOrWhiteSpace(foundUserIdentifier);
		}

		public bool ValidateClientToken(string clientToken)
			=> authDbAccessor.TryPeekAccessToken(clientToken);
	}

	public interface ICargoWiseAuthenticator
	{
		Task<VerificationResult> VerifyAccessTokenAsync(
			CargoWiseAuthCookie cargoWiseAuthCookie,
			CancellationToken cancellationToken);

		bool ValidateClientToken(string clientToken);
	}

	public enum VerificationResult
	{
		/// <summary>
		/// The verification failed for a generic reason.
		/// </summary>
		GeneralFailure,

		/// <summary>
		/// The verification succeeded.
		/// </summary>
		Success,

		/// <summary>
		/// Authentication succeeded (i.e. we established who the user is) but
		/// authorisation failed (i.e. they are not allowed to use CargoWise)
		/// </summary>
		AuthorisationFailed,

		/// <summary>
		/// This instance of CargoWise is configured with a site offline message.
		/// Access should not be allowed at this time.
		/// </summary>
		SiteOffline
	}
}
