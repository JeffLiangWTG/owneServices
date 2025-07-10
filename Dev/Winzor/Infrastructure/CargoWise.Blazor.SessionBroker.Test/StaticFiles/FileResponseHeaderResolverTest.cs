using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.StaticFiles
{
	public class FileResponseHeaderResolverTest
	{
		[Test]
		public void GetLastModifiedShouldReturnCorrectValueAndOnlyInvokedOnce()
		{
			var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();

			mockAppInstallerHandler.Setup(h => h.GetAppInstallerInfo())
				.Returns(new AppInstallerInfo("CargoWise", "WiseTech", new Version("0.0.0.0")));

			var fileResponseHeaderResolver = new FileResponseHeaderResolver(mockAppInstallerHandler.Object);

			Assert.That(fileResponseHeaderResolver.GetLastModified(), Is.EqualTo(DateTimeOffset.Parse("Sat, 01 Jan 2000 00:00:00 GMT")));
			Assert.That(fileResponseHeaderResolver.GetLastModified(), Is.EqualTo(DateTimeOffset.Parse("Sat, 01 Jan 2000 00:00:00 GMT")));

			mockAppInstallerHandler.Verify(v => v.GetAppInstallerInfo(), Times.Once);
		}

		[Test]
		public void GetLastModifiedShouldReturnDefaultDateWhenAppInstallerVersionIsNull()
		{
			var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();

			var fileResponseHeaderResolver = new FileResponseHeaderResolver(mockAppInstallerHandler.Object);
			Assert.That(fileResponseHeaderResolver.GetLastModified(), Is.EqualTo(DateTimeOffset.Parse("Sat, 01 Jan 2000 00:00:00 GMT")));
		}

		[TestCase(null,                      "2000-01-01 00:00:00+00:00")]
		[TestCase("0.0.0.0",                 "2000-01-01 00:00:00+00:00")]
		[TestCase("8.0.0.0",                 "2008-01-01 00:00:00+00:00")]
		[TestCase("24.13.1.0",               "2025-01-01 00:00:00+00:00")]
		[TestCase("24.1.32.1",               "2024-02-01 00:00:01+00:00")]
		[TestCase("24.1.1.0",                "2024-01-01 00:00:00+00:00")]
		[TestCase("24.1.1.1",                "2024-01-01 00:00:01+00:00")]
		[TestCase("24.1.1.65",               "2024-01-01 00:01:05+00:00")]
		[TestCase("24.1.1.3600",             "2024-01-01 01:00:00+00:00")]
		[TestCase("24.12.31.100",            "2024-12-31 00:01:40+00:00")]
		[TestCase("2024.12.31.100",          "2024-12-31 00:01:40+00:00")]
		[TestCase("65535.1.1.1",             "3535-01-01 00:00:01+00:00")]
		[TestCase("65535.65535.65535.65535", "9175-08-05 18:12:15+00:00")]

		public void ConvertVersionToLastModifiedShouldReturnCorrectDate(string versionString, string expectedLastModified)
		{
			var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();

			mockAppInstallerHandler.Setup(h => h.GetAppInstallerInfo())
				.Returns(new AppInstallerInfo("CargoWise", "WiseTech", string.IsNullOrEmpty(versionString) ? new Version() : new Version(versionString)));

			var fileResponseHeaderResolver = new FileResponseHeaderResolver(mockAppInstallerHandler.Object);

			Assert.That(fileResponseHeaderResolver.ConvertVersionToLastModified(), Is.EqualTo(DateTimeOffset.Parse(expectedLastModified)));
		}
	}
}
