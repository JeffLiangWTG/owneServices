using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	public class CustomPhysicalFileInfo : IFileInfo
	{
		readonly IFileInfo physicalFileInfo;
		readonly IFileResponseHeaderResolver fileResponseHeaderResolver;

		public CustomPhysicalFileInfo(IFileInfo physicalFileInfo, IFileResponseHeaderResolver fileResponseHeaderResolver)
		{
			this.physicalFileInfo = physicalFileInfo;
			this.fileResponseHeaderResolver = fileResponseHeaderResolver;
		}

		public DateTimeOffset LastModified => fileResponseHeaderResolver.GetLastModified();

		public bool Exists => physicalFileInfo.Exists;
		public bool IsDirectory => physicalFileInfo.IsDirectory;
		public virtual long Length => physicalFileInfo.Length;
		public string Name => physicalFileInfo.Name;
		public virtual string PhysicalPath => physicalFileInfo.PhysicalPath;
		public virtual Stream CreateReadStream() => physicalFileInfo.CreateReadStream();
	}
}
