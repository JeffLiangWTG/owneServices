using System.Text;
using CargoWise.Blazor.SessionBroker.Helpers;

namespace CargoWise.Blazor.SessionBroker.Authentication;

public interface IAuthenticationConfigAccessor
{
	Enterprise.Integration.IOIDCConfig GetOIDCConfig();
	string GetDomainHint();
}

public class AuthenticationConfigAccessor : IAuthenticationConfigAccessor
{
	public const string OIDCConfig = "OIDCConfig";
	public const string WinzorOIDCConfig = "WinzorOIDCConfig";
	public const string DomainHint = "DomainHint";

	readonly IRegistryAccessor registryAccessor;
	public AuthenticationConfigAccessor(IRegistryAccessor registryAccessor)
	{
		this.registryAccessor = registryAccessor;
	}

	public Enterprise.Integration.IOIDCConfig GetOIDCConfig()
	{
		var data = registryAccessor.GetBinaryValue(WinzorOIDCConfig);
		var oidcConfig = SerializationHelper.Deserialise<DeserializedOIDCConfig>(data);

		if (oidcConfig == null || !oidcConfig.IsOIDCEnabled)
		{
			data = registryAccessor.GetBinaryValue(OIDCConfig);
			oidcConfig = SerializationHelper.Deserialise<DeserializedOIDCConfig>(data);
		}
		return oidcConfig;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	public string GetDomainHint()
	{
		var data = registryAccessor.GetBinaryValue(DomainHint);
		return data is not null ? Encoding.Unicode.GetString(data) : "Azure";
	}
}
