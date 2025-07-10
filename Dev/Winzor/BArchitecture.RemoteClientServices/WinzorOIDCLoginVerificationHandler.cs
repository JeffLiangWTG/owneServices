using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.ZArchitecture.Core;
using WTG.OpenIDConnect.Login;
using WTG.OpenIDConnect.Token;

namespace WinzorFramework.RemoteClientServices
{
	public class WinzorOIDCLoginVerificationHandler : IOIDCLoginServer
	{
		public bool IsSupported => true; // this property is not used in Winzor. Set this to false for any unexpected future reference

		public OIDCLoginResponseMessage LoginRemote(OIDCLoginRequestMessage loginRequest, CancellationToken cancellationToken)
		{
			return Login(loginRequest, cancellationToken);
		}

		public OIDCLoginResponseMessage LoginLocal(
			OIDCLoginRequestMessage loginRequest,
			OIDCWebLauncher webLauncher,
			CancellationToken cancellationToken,
			OIDCLoginFactory oidcLoginFactory)
		{
			return Login(loginRequest, cancellationToken);
		}

		OIDCLoginResponseMessage Login(OIDCLoginRequestMessage loginRequest, CancellationToken cancellationToken)
		{
			var msg = CreateMessage(loginRequest);

			try
			{
				var response = CargoWiseClientInvoker.Invoke(async cws =>
				{
					cancellationToken.ThrowIfCancellationRequested();
					try
					{
						return await cws.OIDCSettingsVerification.VerifyLoginAsync(msg).WaitAsync(cancellationToken);
					}
					catch (OperationCanceledException)
					{
						await cws.OIDCSettingsVerification.CancelLoginAsync(msg.CancellationTokenId);
						return OIDCLoginServerResponseCreator.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled);
					}
				});

				if (response.Error == OIDCLoginResponseMessage.ErrorType.None)
				{
					return response;
				}

				return OIDCLoginServerResponseCreator.CreateFailedResponse(loginRequest, response);
			}
			catch (OperationCanceledException)
			{
				return OIDCLoginServerResponseCreator.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled);
			}
		}

		public JwtSecurityToken ValidateIdentityToken(OIDCLoginRequestMessage request, OIDCLoginResponseMessage response,
			CancellationToken cancellationToken)
		{
			return CargoWiseClientInvoker.Invoke(async _ => await ValidateIdentityTokenAsync(request, response, cancellationToken));
		}

		public async Task<JwtSecurityToken> ValidateIdentityTokenAsync(
			OIDCLoginRequestMessage request,
			OIDCLoginResponseMessage response,
			CancellationToken cancellationToken)
		{
			return await TokenValidator.ValidateAccessToken(request.Authority, request.ClientID, response.IdentityToken, ConfigurationHelper.ConfigurationManagerCache, new OIDCTokenValidationLogger(), cancellationToken).ConfigureAwait(false);
		}

		OIDCVerifySettingsRequestMessage CreateMessage(OIDCLoginRequestMessage loginRequest)
		{
			var msg = new OIDCVerifySettingsRequestMessage();

			var oidcSettings = new OpenIdConnectSettings();
			oidcSettings.Authority = new Uri(loginRequest.Authority);
			oidcSettings.ClientId = loginRequest.ClientID;
			oidcSettings.AdditionalScopes = loginRequest.Scopes;
			oidcSettings.IdentityProvider = loginRequest.ServerType.ToString();
			oidcSettings.LoginPrompt = loginRequest.Prompt.ToString();
			oidcSettings.DomainHint = loginRequest.DomainHint;

			// The successful/failed messages from the loginRequest are a whole HTML page,
			// which will then be unexpectedly wrapped in another html template.
			// Instead, we should rather pass the message string directly to the msg.

			// This project did not call the legacy ResourceStringsAnalyzer.exe, so does not import the new ResourceStrings targets file.
			// Therefore, these GetMultilingualString calls only attempt to retrieve the translations, while
			// Enterprise/Product/Core/Security/Security/Enterprise.Security/OIDCUserLogin.cs. is where the Resource Strings are defined.
			// There, they are saved to the ZRS file with Enterprise.Security's AssemblyId.
			// It is questionable whether the translation has ever worked here since an AssemblyId is not provided.
#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods; See comment above
			msg.SuccessfulResponseString = ResString.GetMultilingualString("73B4140D-BC2A-4C42-B419-ECAE5AB88182", "You've been successfully authenticated. Please close this browser tab, as it is no longer required, and continue to the application.");
			msg.FailedResponseString = ResString.GetMultilingualString("2115A02E-4819-4C24-84DB-3391D86FB90E", "Oops, something went wrong. Please try again or contact your System Administrator.");
#pragma warning restore CW1178 // Do Not Invoke Old Res.GetString Methods
			msg.TimeoutSeconds = loginRequest.TimeoutSeconds;
			msg.CancellationTokenId = Guid.NewGuid().ToString();

			msg.OpenIdConnectSettings = oidcSettings;
			return msg;
		}
	}
}
