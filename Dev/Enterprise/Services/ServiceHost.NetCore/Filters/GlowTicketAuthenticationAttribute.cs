using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationTicket = CargoWise.Authentication.Glow.Ticketing.AuthenticationTicket;
using GlowAuthenticationResult = CargoWise.Authentication.Primitives.AuthenticationResult;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Services.ServiceHost.NetCore
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class GlowTicketAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
	{
		readonly string AuthenticationType = (NoResString)"Bearer";
		public bool StrictEndpoint { get; set; }

		static byte[] GetBinaryRegistryItemValue(StringRegistryItem binaryRegistryItem)
			=> binaryRegistryItem.DataType.Serialise(binaryRegistryItem.Value);

		public Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			ValidateToken(context);
			return Task.CompletedTask;
		}

		void ValidateToken(AuthorizationFilterContext context)
		{
			var httpContext = context.HttpContext;
			var request = httpContext.Request;
			var authHeader = request.Headers.TryGetValue((NoResString)"Authorization", out var authorization)
				? authorization.ToString() : null;
			var token = authHeader?.Substring(AuthenticationType.Length + 1).Trim(); // trim "Bearer "
			var requestMethodAndUri = FormattableString.Invariant($"Method: {request.Method}, RequestUri: {request.Path}."); // RequestInfos

			if (string.IsNullOrWhiteSpace(token) || !authHeader.StartsWith(AuthenticationType, StringComparison.OrdinalIgnoreCase))
			{
				SetErrorResult(context, ApiProxyAuthenticationResult.TokenNotProvided.ToString("G"));
				return;
			}

			var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
			byte[] encryptionKey;
			try
			{
				if (string.IsNullOrEmpty(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value))
				{
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
				GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationHmacKey.Name, TimeSpan.Zero);
			}
			var hmacKey = GetBinaryRegistryItemValue(GlowRegistry.Instance.GlowAuthenticationHmacKey);
			var ticket = AuthenticationTicket.FromCookieValue(token, encryptionKey, hmacKey);

			if (ticket == null)
			{
				SetErrorResult(context, ApiProxyAuthenticationResult.InvalidToken.ToString("G"), GlowAuthenticationResult.AbnormalFailure.ToString("G"));
				GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name, TimeSpan.FromMinutes(1));
				GlowRegistry.Instance.RemoveItemFromCacheIfOlderThan(GlowRegistry.Instance.GlowAuthenticationHmacKey.Name, TimeSpan.FromMinutes(1));
				return;
			}

			if (ticket.AuthenticationResult == GlowAuthenticationResult.SessionLimitReached)
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

			if (HasExpired(ticket, configuration))
			{
				SetErrorResult(context, ApiProxyAuthenticationResult.SessionExpired.ToString("G"), GlowAuthenticationResult.SessionExpired.ToString("G"));
				return;
			}

			var identity = new GlowAuthenticationTicketIdentity(ticket);
			var principal = new GenericPrincipal(identity, null);
			httpContext.User = principal;
		}

		string ReadGlowAuthEncryptionKeyFromDatabase()
		{
			var sqlText = $"SELECT {StmDataSchema.Constants.SD_BinaryValue} FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} WHERE {StmDataSchema.Constants.SD_Name} = '{GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name}'"; // SQL
			using (Db.DisposableActionForDbConnection())
#pragma warning disable CW1107 // Use the BusinessObjectFactory rather than hitting the DB directly - Temporarily suppressed this as we are focusing on the move to .NET Core. Once that is completed, this will need to be addressed.
			using (var cmd = Db.Connection.Command(sqlText)) // just need SD_BinaryValue
#pragma warning restore CW1107
			{
				return cmd.ExecuteScalar() is byte[] bytes ? BitConverter.ToString(bytes).Replace("-", string.Empty) : null; // separator
			}
		}

		static void SetErrorResult(AuthorizationFilterContext context, string cw1AuthResult = null, string glowAuthenticationResult = null)
		{
			context.Result = new GlowAuthenticationFailureResult(context.HttpContext.Request, cw1AuthResult, glowAuthenticationResult);
		}

		static bool HasExpired(AuthenticationTicket ticket, IConfiguration configuration)
		{
#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
			var now = DateTime.UtcNow;
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
			var clockSkew = configuration[Constants.AuthClockSkewSettingName];
			if (!string.IsNullOrEmpty(clockSkew))
			{
				now = now.Subtract(TimeSpan.Parse(clockSkew, CultureInfo.InvariantCulture));
			}

			return ticket.ExpiresAtUtc < now;
		}

		bool CheckEndpoint(AuthenticationTicket ticket, HttpRequest request) =>
			!StrictEndpoint || request.Path == ticket.Endpoint;
	}
}
