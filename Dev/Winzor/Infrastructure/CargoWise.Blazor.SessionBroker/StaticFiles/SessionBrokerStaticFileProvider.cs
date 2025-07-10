using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	public class SessionBrokerStaticFileProvider : IFileProvider
	{
		readonly IFileProvider innerFileProvider;
		readonly IAppInstallerHandler appInstallerHandler;
		readonly IFileResponseHeaderResolver fileResponseHeaderResolver;
		readonly IHttpContextAccessor httpContextAccessor;

		static readonly ImmutableHashSet<string> DownloadableFileNameList =
			ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
				InstallerFileNames.ClientMsixPackage,
				InstallerFileNames.ClientAppInstaller);

		static readonly ImmutableDictionary<string, string> FileUrlAndFileNameDictionary =
			ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, new Dictionary<string, string>
		{
			{ "client.msix", InstallerFileNames.ClientMsixPackage },
			{ "client.appinstaller", InstallerFileNames.ClientAppInstaller },
		});

		public SessionBrokerStaticFileProvider(
			IAppInstallerHandler appInstallerHandler,
			IFileResponseHeaderResolver fileResponseHeaderResolver,
			IHttpContextAccessor httpContextAccessor,
			IInstallerPaths installerPaths)
		{
			innerFileProvider = new PhysicalFileProvider(Path.GetFullPath(installerPaths.InstallerDirectoryPath));
			this.appInstallerHandler = appInstallerHandler;
			this.fileResponseHeaderResolver = fileResponseHeaderResolver;
			this.httpContextAccessor = httpContextAccessor;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return new NotFoundDirectoryContents();
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			var fileName = Path.GetFileName(subpath);

			if (FileUrlAndFileNameDictionary.TryGetValue(fileName, out var newFileName))
			{
				fileName = newFileName;
			}

			if (!DownloadableFileNameList.Contains(fileName))
			{
				return new NotFoundFileInfo(subpath);
			}

			var physicalFileInfo = innerFileProvider.GetFileInfo(fileName);

			if (fileName.Equals(InstallerFileNames.ClientAppInstaller, StringComparison.OrdinalIgnoreCase))
			{
				var authority = new Uri($"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/");
				var appInstallerContent = appInstallerHandler.GetAppInstallerContentWithNewUri(
					authorityUri: authority,
					appInstallerRequestPath: Arguments.ClientAppInstallerDownloadUrl,
					msixPackageRequestPath: Arguments.ClientMsixPackageDownloadUrl);
				return new AppInstallerFileInfo(physicalFileInfo, appInstallerContent, fileResponseHeaderResolver);
			}
			else
			{
				return new CustomPhysicalFileInfo(physicalFileInfo, fileResponseHeaderResolver);
			}
		}

		public IChangeToken Watch(string filter)
		{
			return NullChangeToken.Singleton;
		}
	}
}
