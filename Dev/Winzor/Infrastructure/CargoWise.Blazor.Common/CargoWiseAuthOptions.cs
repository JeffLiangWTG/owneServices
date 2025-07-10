namespace CargoWise.Blazor.Common;

public record CargoWiseAuthOptions
{
	public string ClientToken { get; init; }
	public string SessionToken { get; init; }
	public string IdentityToken { get; init; }
	public bool? AllowAnonymousDeveloperLogins { get; init; }
}
