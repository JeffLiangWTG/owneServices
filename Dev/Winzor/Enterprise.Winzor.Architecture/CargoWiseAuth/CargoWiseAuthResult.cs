using Enterprise.ZArchitecture.Core;

namespace Enterprise.Winzor.Architecture;

public record CargoWiseAuthResult
{
	public bool IsSuccess { get; init; }
	public string FailedMessage { get; init; }
	public LoginAuthenticationInfo AuthenticatedUser { get; init; }
	public bool CanRetryLogin { get; init; }

	CargoWiseAuthResult()
	{
	}

	public static CargoWiseAuthResult Failed(string failedMessage, bool canRetryLogin = false)
	{
		return new CargoWiseAuthResult
		{
			IsSuccess = false,
			FailedMessage = failedMessage,
			CanRetryLogin = canRetryLogin,
		};
	}

	public static CargoWiseAuthResult Success(LoginAuthenticationInfo authenticatedUser)
	{
		return new CargoWiseAuthResult
		{
			IsSuccess = true,
			AuthenticatedUser = authenticatedUser
		};
	}
}
