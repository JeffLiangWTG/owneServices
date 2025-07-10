using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

#nullable enable
namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313", Justification = "Primary constructor")]
	public record AppInstallerInfo(string PackageName, string Publisher, Version Version);

	public interface IAppInstallerHandler
	{
		AppInstallerInfo? GetAppInstallerInfo();
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly, false positive, see https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/2927
		byte[]? GetAppInstallerContentWithNewUri(Uri authorityUri, string appInstallerRequestPath, string msixPackageRequestPath);
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
	}

	public class AppInstallerHandler : IAppInstallerHandler
	{
		readonly ILogger<AppInstallerHandler> logger;
		readonly IMemoryCache cache;
		readonly IInstallerPaths installerPaths;
		readonly Lazy<AppInstallerInfo?> appInstallerInfo;

		public AppInstallerHandler(ILogger<AppInstallerHandler> logger, IMemoryCache cache, IInstallerPaths installerPaths)
		{
			this.logger = logger;
			this.cache = cache;
			this.installerPaths = installerPaths;

			appInstallerInfo = new Lazy<AppInstallerInfo?>(GetAppInstallerInfoInternal);
		}

		public AppInstallerInfo? GetAppInstallerInfo() => appInstallerInfo.Value;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		AppInstallerInfo? GetAppInstallerInfoInternal()
		{
			var appInstallerFilePath = installerPaths.ClientAppInstallerPath;

			var content = GetAppInstallerContentFromFile(appInstallerFilePath);
			if (string.IsNullOrEmpty(content))
			{
				logger.LogError($"Failed to read App Installer content from path: {appInstallerFilePath}");
				return null;
			}

			XDocument xml;
			try
			{
				xml = XDocument.Parse(content);
			}
			catch (XmlException ex)
			{
				logger.LogError($"Failed to parse xml content: {content}, exception: ", ex);
				return null;
			}

			if (xml?.Root == null)
			{
				logger.LogError($"Failed to read xml document: {appInstallerFilePath}");
				return null;
			}

			var mainPackage = xml.Descendants()
				.FirstOrDefault(e => e.Name.LocalName.Equals("MainPackage", StringComparison.OrdinalIgnoreCase));
			if (mainPackage == null)
			{
				logger.LogError($"Failed to read MainPackage: {appInstallerFilePath}");
				return null;
			}

			var version = mainPackage.Attribute("Version")?.Value;
			var name = mainPackage.Attribute("Name")?.Value;
			var publisher = mainPackage.Attribute("Publisher")?.Value;

			if (version == null || name == null || publisher == null)
			{
				logger.LogError("Could not find version or package name in the app installer file.");
				return null;
			}

			return new AppInstallerInfo(name, publisher, new Version(version));
		}

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly, false positive, see https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/2927
		public byte[]? GetAppInstallerContentWithNewUri(Uri authorityUri, string appInstallerRequestPath, string msixPackageRequestPath)
		{
			var cacheKey = $"AppInstallerUri_{authorityUri}";
			return cache.GetOrCreate(cacheKey, entry => GetAppInstallerContentInternal(authorityUri, appInstallerRequestPath, msixPackageRequestPath));
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		byte[]? GetAppInstallerContentInternal(Uri authorityUri, string appInstallerRequestPath, string msixPackageRequestPath)
		{
			logger.LogDebug($"Creating new App Installer content with new Uri: {authorityUri}");

			var appInstallerFilePath = installerPaths.ClientAppInstallerPath;

			var content = GetAppInstallerContentFromFile(appInstallerFilePath);
			if (string.IsNullOrEmpty(content))
			{
				logger.LogError($"Failed to read App Installer content from path: {appInstallerFilePath}");
				return null;
			}

			XDocument xmlDoc;
			try
			{
				xmlDoc = XDocument.Parse(content);
			}
			catch (XmlException ex)
			{
				logger.LogError($"Failed to parse xml content: {content}, exception: ", ex);
				return null;
			}

			if (xmlDoc?.Root == null)
			{
				logger.LogError($"Failed to read xml document: {appInstallerFilePath}");
				return null;
			}

			var appInstallerUri = new Uri(authorityUri, appInstallerRequestPath);
			xmlDoc.Root.SetAttributeValue("Uri", appInstallerUri);

			var mainPackage = xmlDoc.Descendants()
				.FirstOrDefault(e => e.Name.LocalName.Equals("MainPackage", StringComparison.OrdinalIgnoreCase));
			if (mainPackage == null)
			{
				logger.LogError($"Failed to read MainPackage: {appInstallerFilePath}");
				return null;
			}

			var msixPackageUri = new Uri(authorityUri, msixPackageRequestPath);
			mainPackage.SetAttributeValue("Uri", msixPackageUri);

			using (var memoryStream = new MemoryStream())
			{
				var settings = new XmlWriterSettings
				{
					Encoding = new UTF8Encoding(false), // false means no BOM
					Indent = true,
					OmitXmlDeclaration = false,
				};

				using (var writer = XmlWriter.Create(memoryStream, settings))
				{
					xmlDoc.Save(writer);
				}

				return memoryStream.ToArray();
			}
		}
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly

		string? GetAppInstallerContentFromFile(string appInstallerFilePath)
		{
			if (!File.Exists(appInstallerFilePath))
			{
				return null;
			}

			var content = File.ReadAllText(appInstallerFilePath);
			if (string.IsNullOrEmpty(content))
			{
				return null;
			}

			return content;
		}
	}
}
