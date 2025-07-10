using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	public class AppInstallerFileInfo : CustomPhysicalFileInfo, IFileInfo
	{
		readonly byte[] contentWithReplacedUri;

		public AppInstallerFileInfo(
			IFileInfo physicalFileInfo,
			byte[] appInstallerContent,
			IFileResponseHeaderResolver fileResponseHeaderResolver)
			: base(physicalFileInfo, fileResponseHeaderResolver)
		{
			ArgumentNullException.ThrowIfNull(nameof(appInstallerContent));
			this.contentWithReplacedUri = appInstallerContent;
		}

		public override long Length => contentWithReplacedUri.Length;

		public override string PhysicalPath
		{
			get
			{
				// if the PhysicalPath is not null or empty, the SendFileAsyncCore method in Http.Extensions will read the physical file instead of using the CreateReadStream method, so return null here
				// see https://github.com/dotnet/aspnetcore/blob/release/8.0/src/Http/Http.Extensions/src/SendFileResponseExtensions.cs#L87
				return null;
			}
		}

		public override Stream CreateReadStream() => new MemoryStream(contentWithReplacedUri);
	}
}
