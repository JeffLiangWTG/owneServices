using CargoWiseOne.WebInfrastructure;

namespace NetCore.WebInfrastructure;

public interface IWebConfigurationProvider
{
	bool TryGetWebDbConfigFromIISRegistry(out WebDbConfigurationInfo? webDbConfig);
}
