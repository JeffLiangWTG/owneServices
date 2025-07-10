using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using GlowAuthenticationResult = CargoWise.Authentication.Primitives.AuthenticationResult;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class GlowTicketAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Bearer header token")]
		const string AuthenticationType = "Bearer";
		public bool AllowMultiple => false;
		public bool StrictEndpoint { get; set; }

		static byte[] GetBinaryRegistryItemValue(StringRegistryItem binaryRegistryItem)
			=> binaryRegistryItem.DataType.Serialise(binaryRegistryItem.Value);

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			ValidateToken(context);
			return Task.CompletedTask;
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		void ValidateToken(HttpAuthenticationContext context)
		{
			var actionContext = context.ActionContext;
			var request = actionContext.Request;
			var authHeader = request.Headers?.Authorization;
			var token = authHeader?.Parameter;
			var requestMethodAndUri = FormattableString.Invariant($"Method: {request.Method}, RequestUri: {request.RequestUri}."); // RequestInfos

			if (token == null || !authHeader.Scheme.Equals(AuthenticationType, StringComparison.OrdinalIgnoreCase))
			{
				SetErrorResult(
					context,
					ApiProxyAuthenticationResult.TokenNotProvided.ToString("G")
					);
				return;
			}

			byte[] encryptionKey;
			try
			{
				if (string.IsNullOrEmpty(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value))
				{
					//workaround for Issue 01631321,registry.Value not as same as DB, so clear cache here.
					GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name, TimeSpan.Zero);
				}
				encryptionKey = GetBinaryRegistryItemValue(GlowRegistry.Instance.GlowAuthenticationEncryptionKey);
			}
			catch (RegistryValidationException ex)
			{
				var databaseValue = ReadGlowAuthEncryptionKeyFromDatabase();
				var databaseValueInfo = databaseValue != null ? $"Length of DatabaseValue: {databaseValue.Length} hexadecimal characters" : (NoResString)"DatabaseValue: null"; // exception message.
				throw new RegistryValidationException($@"{ex.Message}
ErrorValue: {GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value}
{databaseValueInfo}", ex); // exception message.
			}

			if (string.IsNullOrEmpty(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value))
			{
				//workaround for Issue 01631321,registry.Value not as same as DB, so clear cache here.
				GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationHmacKey.Name, TimeSpan.Zero);
			}
			var hmacKey = GetBinaryRegistryItemValue(GlowRegistry.Instance.GlowAuthenticationHmacKey);
			var ticket = AuthenticationTicket.FromCookieValue(token, encryptionKey, hmacKey);

			if (ticket == null)
			{
				SetErrorResult(
					context,
					ApiProxyAuthenticationResult.InvalidToken.ToString("G"),
					GlowAuthenticationResult.AbnormalFailure.ToString("G")
					);
				GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name, TimeSpan.FromMinutes(1));
				GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationHmacKey.Name, TimeSpan.FromMinutes(1));
				return;
			}

			if(ticket.AuthenticationResult == GlowAuthenticationResult.SessionLimitReached)
			{
				SetErrorResult(context, ApiProxyAuthenticationResult.InvalidToken.ToString("G"), GlowAuthenticationResult.SessionLimitReached.ToString("G"));
				return;
			}

			if (!CheckEndpoint(ticket, request))
			{
				SetErrorResult(context, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
				return;
			}

			if (ticket.AuthenticationResult != GlowAuthenticationResult.Success)
			{
				SetErrorResult(context, ticket.AuthenticationResult.ToString("D"), requestMethodAndUri);
				return;
			}

			if (HasExpired(ticket))
			{
				SetErrorResult(
					context,
					ApiProxyAuthenticationResult.SessionExpired.ToString("G"),
					GlowAuthenticationResult.SessionExpired.ToString("G")
					);
				return;
			}

			var identity = new GlowAuthenticationTicketIdentity(ticket);
			var principal = new GenericPrincipal(identity, null);
			context.Principal = principal;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string ReadGlowAuthEncryptionKeyFromDatabase()
		{
			var sqlText = $"SELECT {StmDataSchema.Constants.SD_BinaryValue} FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} WHERE {StmDataSchema.Constants.SD_Name} = '{GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name}'"; // SQL
			using (Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sqlText)) // just need SD_BinaryValue
			{
				return cmd.ExecuteScalar() is byte[] bytes ? BitConverter.ToString(bytes).Replace("-", string.Empty) : null; // separator
			}
		}

		static void SetErrorResult(HttpAuthenticationContext context, string cw1AuthResult = null, string glowAuthenticationResult = null)
		{
			context.ErrorResult = new GlowAuthenticationFailureResult(context.Request, cw1AuthResult, glowAuthenticationResult);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		static bool HasExpired(AuthenticationTicket ticket)
		{
			var now = DateTime.UtcNow;
			var clockSkew = ConfigurationManager.AppSettings[Constants.AuthClockSkewSettingName];
			if (!string.IsNullOrEmpty(clockSkew))
			{
				now = now.Subtract(TimeSpan.Parse(clockSkew, CultureInfo.InvariantCulture));
			}

			return ticket.ExpiresAtUtc < now;
		}

		bool CheckEndpoint(AuthenticationTicket ticket, HttpRequestMessage request)
		{
			if (!StrictEndpoint)
			{
				return true;
			}
			return request.RequestUri.PathAndQuery == ticket.Endpoint;
		}
	}
}
