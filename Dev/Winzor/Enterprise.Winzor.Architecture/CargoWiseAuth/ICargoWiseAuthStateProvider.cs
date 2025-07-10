using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components.Authorization;

namespace Enterprise.Winzor.Architecture;

/// <summary>
/// Authentication state provided by CargoWise instance is managed here
/// </summary>	
public interface ICargoWiseAuthStateProvider
{
	/// <summary>
	/// the CW authenicated user
	/// </summary>
	LoginAuthenticationInfo AuthenticatedUser { get; }

	/// <summary>
	/// Completion source is set when login completes
	/// Used by the backchannel hosted service
	/// </summary>
	TaskCompletionSource<bool> AuthenticationComplete { get; }

	/// <summary>
	/// Initialised after successful login using a client token
	/// </summary>
	Uri BackChannelUrl { get; }

	/// <summary>
	/// Get the Authenicated state
	/// </summary>
	/// <value>The authentication state for the logged in user, null if user login is not valid</value>
	AuthenticationState AuthenticationState { get; }

	/// <summary>
	/// Login a customer to the system
	/// </summary>
	/// <param name="creds">A tuple with the username (Item1) and password (Item2) </param>
	CargoWiseAuthResult Login(Tuple<string, string> creds);

	/// <summary>
	/// Login as anonymous or from StmAccessToken or OIDC, will initialise the BackChannelUrl property if appropriate
	/// </summary>
	/// <param name="cargoWiseAuthStateProvider"></param>
	/// <param name="cargoWiseAuthOptions"></param>
	/// <returns></returns>
	CargoWiseAuthResult Login(ICargoWiseAuthStateProvider cargoWiseAuthStateProvider, CargoWiseAuthOptions cargoWiseAuthOptions);
}
