using System.Globalization;
using System.Text.RegularExpressions;
using CargoWiseOne.WebInfrastructure;
using Microsoft.Extensions.Logging;

namespace NetCore.WebInfrastructure;

#if DEBUG
[WTG.StaticAnalysis.Annotation.CodeAlive("This class is common middleware for upcoming .NET8 web services, attribute will be removed.")]
#endif
public sealed class WebConfigurationProvider : IWebConfigurationProvider
{
	readonly ILogger logger;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
	readonly Regex iisAppDomainNamePattern = new (@"\/LM\/W3SVC\/(?<site>\d+)\/ROOT\/(?<path>[\w+\/.]+)", RegexOptions.IgnoreCase);

	public WebConfigurationProvider(ILogger<WebConfigurationProvider> logger)
	{
		this.logger = logger;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "string inside LogInformation")]
	public bool TryGetWebDbConfigFromIISRegistry(out WebDbConfigurationInfo? webDbConfig)
	{
		webDbConfig = default;

		try
		{
			var iisPhysicalPath = GetIISPhysicalPath();
			if (string.IsNullOrWhiteSpace(iisPhysicalPath))
			{
				logger.LogInformation("IIS Physical Path not found");
				return false;
			}

			var webAppPath = GetWebAppPathFromFullIISAppPath(iisPhysicalPath);
			if (webAppPath == null)
			{
				logger.LogInformation("IIS Physical Path cannot be parsed");
				return false;
			}

			webDbConfig = WebDbConfiguration.GetConfiguration(webAppPath);

			return true;
		}
		catch (Exception ex)
		{
			logger.LogInformation($"Error getting Web DB config from IIS configuration: {ex.Message}");
			return false;
		}
	}

	string? GetIISPhysicalPath()
	{
		var envIisPhyPath = Environment.GetEnvironmentVariable("ASPNETCORE_IIS_PHYSICAL_PATH");
		return envIisPhyPath;
	}

	internal WebAppPath? GetWebAppPathFromFullIISAppPath(string appVirtualPath)
	{
		var match = iisAppDomainNamePattern.Match(appVirtualPath);

		if (!match.Success)
		{
			return null;
		}

		var siteId = long.Parse(match.Groups["site"].Value, CultureInfo.InvariantCulture);
		var virtualAppPath = match.Groups["path"].Value;
		var webAppPathString = $"{siteId}/{virtualAppPath}";
		return WebAppPath.Parse(webAppPathString);
	}
}
