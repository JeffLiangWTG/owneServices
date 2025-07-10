using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	public interface IFileResponseHeaderResolver
	{
		DateTimeOffset GetLastModified();
	}

	class FileResponseHeaderResolver : IFileResponseHeaderResolver
	{
		readonly IAppInstallerHandler appInstallerHandler;
		readonly Lazy<DateTimeOffset> lastModified;

		public FileResponseHeaderResolver(IAppInstallerHandler appInstallerHandler)
		{
			this.appInstallerHandler = appInstallerHandler;
			lastModified = new Lazy<DateTimeOffset>(ConvertVersionToLastModified, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public DateTimeOffset GetLastModified() => lastModified.Value;

		/// <summary>
		/// Convert assembly version (24.7.1.5) to http last-modified header style date string (Mon, 01 Jul 2024 00:00:05 GMT)
		/// <br></br>
		/// The Revision (fourth digit) number is the seconds since 00:00:00
		/// </summary>
		internal DateTimeOffset ConvertVersionToLastModified()
		{
			var appInstallerInfo = appInstallerHandler.GetAppInstallerInfo();
			var version = appInstallerInfo?.Version ?? new Version();

			var year = version.Major % 2000 + 2000;
			var month = version.Minor <= 0 ? 1 : version.Minor;
			var day = version.Build <= 0 ? 1 : version.Build;
			var seconds = version.Revision < 0 ? 0 : version.Revision;

			var lastModified = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			lastModified = lastModified.AddMonths(month - 1).AddDays(day - 1).AddSeconds(seconds);
			return new DateTimeOffset(lastModified, TimeSpan.Zero);
		}
	}
}
