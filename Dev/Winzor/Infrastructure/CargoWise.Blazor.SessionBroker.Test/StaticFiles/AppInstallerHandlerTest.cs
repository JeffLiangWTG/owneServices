using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.StaticFiles
{
	public class AppInstallerHandlerTest
	{
		Mock<IInstallerPaths> mockInstallerPaths;

		const string TestAppInstallerPath = @".\test.appinstaller";

		const string TestAppInstallerContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""http://localhost:5000/staticfiles/CargowiseAppx.appinstaller""
	Version=""24.7.25.14"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<MainPackage
		Name=""CargoWise-Client""
		Version=""24.7.25.14""
		Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU""
		ProcessorArchitecture=""x64""
		Uri=""http://localhost:5000/staticfiles/Cargowise.msix"" />
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";

		[SetUp]
		public void SetUp()
		{
			mockInstallerPaths = new Mock<IInstallerPaths>();
			mockInstallerPaths.Setup(p => p.ClientAppInstallerPath).Returns(TestAppInstallerPath);
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(TestAppInstallerPath);
		}

		[Test]
		public async Task GetAppInstallerInfoShouldReturnCorrectValue()
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(TestAppInstallerPath, TestAppInstallerContent);

			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var appInstallerInfo = appInstallerHandler.GetAppInstallerInfo();
			Assert.That(appInstallerInfo.Version, Is.EqualTo(new Version("24.7.25.14")));
			Assert.That(appInstallerInfo.Publisher, Is.EqualTo("CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU"));
			Assert.That(appInstallerInfo.PackageName, Is.EqualTo("CargoWise-Client"));
		}

		[Test]
		public async Task GetAppInstallerInfoShouldCachedAndInstallerPathsInvokedOnce()
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(TestAppInstallerPath, TestAppInstallerContent);

			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var appInstallerInfo = appInstallerHandler.GetAppInstallerInfo();
			Assert.That(appInstallerInfo.Version, Is.EqualTo(new Version("24.7.25.14")));
			appInstallerInfo = appInstallerHandler.GetAppInstallerInfo();
			Assert.That(appInstallerInfo.Version, Is.EqualTo(new Version("24.7.25.14")));

			mockInstallerPaths.Verify(p => p.ClientAppInstallerPath, Times.Once);
		}

		[Test]
		public void GetAppInstallerInfoShouldReturnNullAndLogWhenThereIsNoAppInstallerFile()
		{
			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var appInstallerInfo = appInstallerHandler.GetAppInstallerInfo();
			Assert.That(appInstallerInfo, Is.Null);
			mockLogger.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Failed to read App Installer content from path: {TestAppInstallerPath}")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public async Task GetAppInstallerContentWithNewUriShouldReturnCorrectXml()
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(TestAppInstallerPath, TestAppInstallerContent);

			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			mockCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
				.Returns(Mock.Of<ICacheEntry>());

			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var authorityUri = new Uri("https://wi00740418.blazor.sand.wtg.zone/");
			var appInstallerRequestPath = "/_clientapi/installer/test.appinstaller";
			var msixPackageRequestPath = "/_sessionbroker/installer/test.msix";

			var content = appInstallerHandler.GetAppInstallerContentWithNewUri(authorityUri, appInstallerRequestPath, msixPackageRequestPath);
			var contentAsString = Encoding.UTF8.GetString(content);
			var xml = XDocument.Parse(contentAsString);
			Assert.That(xml.Root, Is.Not.Null);
			Assert.That(xml.Root.Attribute("Uri").Value, Is.EqualTo("https://wi00740418.blazor.sand.wtg.zone/_clientapi/installer/test.appinstaller"));

			var mainPackage = xml.Descendants()
				.FirstOrDefault(e => e.Name.LocalName.Equals("MainPackage", StringComparison.OrdinalIgnoreCase));
			Assert.That(mainPackage, Is.Not.Null);
			Assert.That(mainPackage.Attribute("Uri").Value, Is.EqualTo("https://wi00740418.blazor.sand.wtg.zone/_sessionbroker/installer/test.msix"));

			var expectedXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller Uri=""https://wi00740418.blazor.sand.wtg.zone/_clientapi/installer/test.appinstaller"" Version=""24.7.25.14"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
  <MainPackage Name=""CargoWise-Client"" Version=""24.7.25.14"" Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU"" ProcessorArchitecture=""x64"" Uri=""https://wi00740418.blazor.sand.wtg.zone/_sessionbroker/installer/test.msix"" />
  <UpdateSettings>
    <OnLaunch HoursBetweenUpdateChecks=""0"" />
  </UpdateSettings>
</AppInstaller>";
			Assert.That(contentAsString, Is.EqualTo(expectedXml));
		}

		//    Expected invocation on the mock once, but was 11 times: c => c.CreateEntry(It.IsAny<object>())
		[Test]
		public async Task GetAppInstallerContentWithNewUriShouldUseCacheCorrectly()
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(TestAppInstallerPath, TestAppInstallerContent);

			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();

			using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, memoryCache, mockInstallerPaths.Object);

			var authorityUri = new Uri("https://wi00740418.blazor.sand.wtg.zone/");
			var appInstallerRequestPath = "/_clientapi/installer/test.appinstaller";
			var msixPackageRequestPath = "/_sessionbroker/installer/test.msix";

			// establish cache entry
			_ = appInstallerHandler.GetAppInstallerContentWithNewUri(authorityUri, appInstallerRequestPath, msixPackageRequestPath);

			// use Range to create multiple tasks
			var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
			{
				var content = appInstallerHandler.GetAppInstallerContentWithNewUri(authorityUri, appInstallerRequestPath, msixPackageRequestPath);
				Assert.That(content.Length, Is.GreaterThan(100));
			})).ToArray();

			await Task.WhenAll(tasks);

			// Verify that the cache contains the expected entry
			var cacheKey = $"AppInstallerUri_{authorityUri}";
			Assert.That(memoryCache.TryGetValue(cacheKey, out _), Is.True, "Cache should contain the entry");

			mockLogger.Verify(
				x => x.Log(
					LogLevel.Debug,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating new App Installer content")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public void GetAppInstallerContentWithNewUriShouldReturnNullAndLogWhenNoAppInstallerFile()
		{
			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			mockCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
				.Returns(Mock.Of<ICacheEntry>());

			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var authorityUri = new Uri("https://wi00740418.blazor.sand.wtg.zone/");
			var appInstallerRequestPath = "/_clientapi/installer/test.appinstaller";
			var msixPackageRequestPath = "/_sessionbroker/installer/test.msix";

			var content = appInstallerHandler.GetAppInstallerContentWithNewUri(authorityUri, appInstallerRequestPath, msixPackageRequestPath);
			Assert.That(content, Is.Null);
			mockLogger.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Failed to read App Installer content from path: {TestAppInstallerPath}")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public async Task GetAppInstallerContentWithNewUriShouldReturnNullAndLogWhenAppInstallerFileHasNoMainPackageAsync()
		{
			var invalidAppInstallerContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""http://localhost:5000/staticfiles/CargowiseAppx.appinstaller""
	Version=""24.7.25.14"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";
			await MockInstallerFileHelper.CreateMockInstallerFile(TestAppInstallerPath, invalidAppInstallerContent);

			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			mockCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
				.Returns(Mock.Of<ICacheEntry>());

			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var authorityUri = new Uri("https://wi00740418.blazor.sand.wtg.zone/");
			var appInstallerRequestPath = "/_clientapi/installer/test.appinstaller";
			var msixPackageRequestPath = "/_sessionbroker/installer/test.msix";

			var content = appInstallerHandler.GetAppInstallerContentWithNewUri(authorityUri, appInstallerRequestPath, msixPackageRequestPath);
			Assert.That(content, Is.Null);
			mockLogger.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Failed to read MainPackage: {TestAppInstallerPath}")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}

		[Test]
		public async Task GetAppInstallerContentWithNewUriShouldReturnNullAndLogWhenAppInstallerFileIsNotAValidXML()
		{
			var invalidAppInstallerContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstalle
";
			await MockInstallerFileHelper.CreateMockInstallerFile(TestAppInstallerPath, invalidAppInstallerContent);

			var mockLogger = new Mock<ILogger<AppInstallerHandler>>();
			var mockCache = new Mock<IMemoryCache>();
			mockCache.Setup(c => c.CreateEntry(It.IsAny<object>()))
				.Returns(Mock.Of<ICacheEntry>());

			var appInstallerHandler = new AppInstallerHandler(mockLogger.Object, mockCache.Object, mockInstallerPaths.Object);

			var authorityUri = new Uri("https://wi00740418.blazor.sand.wtg.zone/");
			var appInstallerRequestPath = "/_clientapi/installer/test.appinstaller";
			var msixPackageRequestPath = "/_sessionbroker/installer/test.msix";

			var content = appInstallerHandler.GetAppInstallerContentWithNewUri(authorityUri, appInstallerRequestPath, msixPackageRequestPath);
			Assert.That(content, Is.Null);
			mockLogger.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Failed to parse xml content: {invalidAppInstallerContent}, exception:")),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
		}
	}
}
