using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Blazor.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Winzor.Architecture;

/// <summary>
/// Authentication state provided by CargoWise instance is managed here
/// </summary>
public partial class CargoWiseAuthStateProvider : ICargoWiseAuthStateProvider
{
	readonly ILogger<ICargoWiseAuthStateProvider> logger;
	readonly ITokenValidatorWrapper tokenValidator;

	/// <summary>
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="tokenValidator"></param>
	public CargoWiseAuthStateProvider(
		ILogger<ICargoWiseAuthStateProvider> logger,
		ITokenValidatorWrapper tokenValidator)
	{
		this.logger = logger;
		this.tokenValidator = tokenValidator;
	}

	LoginAuthenticationInfo authenticatedUser;

	/// <summary>
	/// Completion source is set when login completes
	/// Used by the backchannel hosted service
	/// </summary>
	public TaskCompletionSource<bool> AuthenticationComplete { get; } = new ();

	/// <summary>
	/// the CW authenicated user
	/// </summary>
	public LoginAuthenticationInfo AuthenticatedUser
	{
		get { return authenticatedUser ?? (authenticatedUser = LoginAuthenticationInfo.NewFailedLogin(null)); }
		private set { authenticatedUser = value; }
	}

	/// <summary>
	/// Initialised after successful login using a client token
	/// </summary>
	public Uri BackChannelUrl { get; private set; }

	/// <summary>
	/// Get the Authenicated state
	/// </summary>
	/// <value>The authentication state for the logged in user, null if user login is not valid</value>
	public AuthenticationState AuthenticationState
	{
		get
		{
			if (AuthenticatedUser != null && AuthenticatedUser.LoginValidated && AuthenticatedUser.IsOK)
			{
				var identity = new ClaimsIdentity(
					new[]
					{
						new Claim(ClaimTypes.Name, AuthenticatedUser.User.FullName),
						new Claim(ClaimTypes.Sid, AuthenticatedUser.User.PK.ToString()),
						new Claim("2FA", AuthenticatedUser.User.IsTwoFactorAuthenticationEnabled.ToString()),
					}, (NoResString)"CW authentication type");

				var user = new ClaimsPrincipal(identity);

				return new AuthenticationState(user);
			}

			return null;
		}
	}

	/// <summary>
	/// Login a customer to the system
	/// </summary>
	/// <param name="creds">A tuple with the username (Item1) and password (Item2) </param>
	public CargoWiseAuthResult Login(Tuple<string, string> creds)
	{
		var authenticatedUser = Env.LoginController.LoginUser(creds.Item1, creds.Item2);

		if (authenticatedUser.IsOK)
		{
			var loginAutomaticResult = Env.LoginController.LoginLocationExAutomatically(authenticatedUser);
			if (loginAutomaticResult.IsOK)
			{
				logger.LogInformation((NoResString)"Logged in user: {UserName}", authenticatedUser.User.LoginName);
				AuthenticatedUser = authenticatedUser;
				return CargoWiseAuthResult.Success(AuthenticatedUser);
			}

			return CargoWiseAuthResult.Failed(loginAutomaticResult.FailureMessage);
		}
		else
		{
			return CargoWiseAuthResult.Failed(authenticatedUser.FailureMessage);
		}
	}

	/// <summary>
	/// Login with Identity token
	/// </summary>
	/// <param name="identityToken"></param>
	CargoWiseAuthResult LoginWithIdentityToken(string identityToken)
	{
		logger.LogDebug($"Login from OIDC Auth Cookie");
		var oidcConfig = ObjectFactory.Get<IOIDCConfig>();

		JwtSecurityToken jwtSecurityToken = null;

		Task.Run(async () =>
		{
			var validateParameters = new TokenValidatorParameters(
					oidcConfig.AuthorityURL,
					oidcConfig.ClientIdentifier,
					identityToken,
					new ConcurrentDictionary<string, IConfigurationManagerWithLock>(),
					logger,
					CancellationToken.None);

			jwtSecurityToken = await tokenValidator
				.ValidateIdentityTokenAsync(validateParameters)
				.ConfigureAwait(false);
#pragma warning disable VSTHRD002
		}).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

		if (jwtSecurityToken is null)
		{
			return CargoWiseAuthResult.Failed(Res.GetString("CB6DB6E1-A4ED-43BE-93FE-9447A707CA41", "Identity token failed validation: jwtSecurityToken is empty"), canRetryLogin: true);
		}

		var loginAuthenticationInfo = OIDCUserLogin.VerifyUserByOidcClaims(oidcConfig, jwtSecurityToken.Claims);

		loginAuthenticationInfo.TakeThreadOwnershipForUser();

		if (!loginAuthenticationInfo.IsOK)
		{
			logger.LogWarning($@"{nameof(LoginWithIdentityToken)} failed.
FailureMessage: {loginAuthenticationInfo?.FailureMessage}
ExtendedErrorInformation: {loginAuthenticationInfo?.ExtendedErrorInformation}");
			LoginDirector.UpdateLoginState(loginAuthenticationInfo, false);
			return CargoWiseAuthResult.Failed(loginAuthenticationInfo.FailureMessage, canRetryLogin: true);
		}

		logger.LogInformation($"Token based login ...");
		return LoginAuthentication(loginAuthenticationInfo.User, canRetryLogin: true);
	}

	/// <summary>
	/// Logs in from StmAccessToken, will initialise the BackChannelUrl property if appropriate
	/// </summary>
	/// <param name="clientToken"></param>
	/// <returns></returns>
	CargoWiseAuthResult LoginFromStmAccessToken(string clientToken)
	{
		logger.LogDebug($"Login from client token");

		if (AuthenticationState is not null)
		{
			return CargoWiseAuthResult.Failed(Res.GetString("84FF37FE-380A-4E72-A9CC-994380A5343E", "Login from client token must not be called after login has completed"));
		}

		if (string.IsNullOrEmpty(clientToken))
		{
			return CargoWiseAuthResult.Failed(Res.GetString("E0845296-75F9-44F5-A880-68AEB0659749", "Missing client access token"));
		}

		logger.LogDebug((NoResString)"Creating an instance of tokenized access control object");
		ITokenizedAccessControl accessControl = new TokenizedAccessControl();
		logger.LogDebug((NoResString)"Tokenizing access control object is created");
		if (!accessControl.TryConsume(clientToken, "BLC", out var accessTokenInfo))
		{
			return CargoWiseAuthResult.Failed(MSG_AccessTokenInvalid);
		}

		if (accessTokenInfo.ParentTableCode != GlbStaffSchema.Constants.Prefix)
		{
			logger.LogWarning($"Invalid access token detected: could not handle tokens from table {accessTokenInfo.ParentTableCode}");
			return CargoWiseAuthResult.Failed(MSG_AccessTokenInvalid);
		}

		logger.LogDebug((NoResString)"Fetching staff based on access token info");
		var staff = User.LoadUserFromStaffPK(accessTokenInfo.ParentId);
		logger.LogDebug($"Fetched user is a developer {staff.IsDeveloper}");

		var result = LoginAuthentication(staff);
		if (result.IsSuccess)
		{
			// The scope will be either a backchannel url or a user name
			// If it is a url we need to connect to a terminal server and establish a backchannel
			if (Uri.TryCreate(accessTokenInfo.Scope, UriKind.Absolute, out var backChannelUrl))
			{
				BackChannelUrl = backChannelUrl;
			}
		}
		return result;
	}

#if DEBUG
	CargoWiseAuthResult LoginAsAnonymous(bool isAnonymous = true)
	{
		if (isAnonymous)
		{
			var sw = Stopwatch.StartNew();
			AuthenticatedUser = Env.LoginController.LoginUserDeveloper();
			var isLocationLoggedIn = Env.LoginController.LoginLocationAutomatically(AuthenticatedUser);
			logger.LogInformation("Logged in user: {UserName}", AuthenticatedUser.User.LoginName);
			logger.LogDebug($"Logged in developer to location (took: {sw.Elapsed})");
			LoginDirector.UpdateLoginState(AuthenticatedUser, isLocationLoggedIn);
			return CargoWiseAuthResult.Success(AuthenticatedUser);
		}
		return CargoWiseAuthResult.Failed("Anonymous login is disabled");
	}
#endif

	public CargoWiseAuthResult Login(ICargoWiseAuthStateProvider cargoWiseAuthStateProvider, CargoWiseAuthOptions cargoWiseAuthOptions)
	{
		CargoWiseAuthResult result = null;
		try
		{
			if (!string.IsNullOrWhiteSpace(cargoWiseAuthOptions?.ClientToken))
			{
				result = LoginFromStmAccessToken(cargoWiseAuthOptions.ClientToken);
			}
			else if (!string.IsNullOrWhiteSpace(cargoWiseAuthOptions?.IdentityToken))
			{
				result = LoginWithIdentityToken(cargoWiseAuthOptions.IdentityToken);
			}
			else
			{
#if DEBUG
				var allowAnonymousLogin = !(bool)(cargoWiseAuthOptions?.AllowAnonymousDeveloperLogins.HasValue) || (bool)cargoWiseAuthOptions?.AllowAnonymousDeveloperLogins;
				result = LoginAsAnonymous(allowAnonymousLogin);
#else
				throw new InvalidOperationException("No valid login type determined");
#endif
			}

			return result;
		}
		finally
		{
			cargoWiseAuthStateProvider.AuthenticationComplete.SetResult(result?.IsSuccess ?? false);
		}
	}

	CargoWiseAuthResult LoginAuthentication(IUser staff, bool canRetryLogin = false)
	{
		logger.LogDebug((NoResString)"Login started");
		var authInfo = LoginAuthenticationInfo.NewSuccessfulLogin(staff);
		logger.LogDebug((NoResString)"LoginAuthenticationInfo created");
		var loggedInResult = Env.LoginController.LoginLocationExAutomatically(authInfo);
		logger.LogDebug((NoResString)"Login completed (loggedIn: {0}, UserIsNull: {1})", loggedInResult, Env.CurrentUser is null);

		LoginDirector.UpdateLoginState(authInfo, loggedInResult.IsOK);
		AuthenticatedUser = authInfo;

		if (!loggedInResult.IsOK || Env.CurrentUser is null)
		{
			logger.LogWarning($"User login failed: {loggedInResult.FailureMessage} \r\n {loggedInResult.ExtendedErrorInformation}");
			if (!string.IsNullOrWhiteSpace(loggedInResult.FailureMessage))
			{
				return CargoWiseAuthResult.Failed(loggedInResult.FailureMessage);
			}
			return CargoWiseAuthResult.Failed(string.IsNullOrEmpty(authInfo.FailureMessage) ? Res.GetString("AE136AA5-4612-416B-BBA8-C8C958A43529", "Login unsuccessful due to an unidentified issue.") : authInfo.FailureMessage, canRetryLogin);
		}

		logger.LogInformation((NoResString)"logged in user: {UserName}", authInfo.User.LoginName);

		return CargoWiseAuthResult.Success(authInfo);
	}

	static string MSG_AccessTokenInvalid => Res.GetString("8A810BF6-D943-49BC-89D7-49B038BFF614", "Invalid client access token");
}
