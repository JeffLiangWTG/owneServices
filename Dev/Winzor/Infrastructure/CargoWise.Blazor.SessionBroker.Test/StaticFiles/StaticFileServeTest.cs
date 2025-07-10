using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWise.Blazor.Testing.Common;
using CargoWiseNext.Infrastructure.Installations;
using CargoWiseNext.Infrastructure.Installations.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.StaticFiles
{
	[KillBlazorAppProcesses]
	public class StaticFileServeTest
	{
		[TearDown]
		public void TearDown()
		{
			File.Delete(InstallerPathsForTest.Instance.ClientAppInstallerPath);
			File.Delete(InstallerPathsForTest.Instance.ClientMsixPackagePath);
		}

		[Test]
		public async Task InvalidStaticFileRequestsRejected()
		{
			using var oidcConfig = Application.ObjectFactory.Substitute(OIDCConfigSetupHelper.SetOidcConfigTest("http://localhost:3001/", Guid.NewGuid().ToString()));
			await using var factory = new CustomWebApplicationFactory<Startup>();
			var client = factory.CreateClient();
			var response = await client.GetAsync("/installer/invalidrequest.msi");
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task FileContentMatchesExpectedContent(string installerFileName, string installerRequestPath)
		{
			var expectedContent = await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Encoding.UTF8.GetBytes(expectedContent));

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton(appInstallerHandlerMock.Object);
			}));

			var client = factory.CreateClient();
			var response = await client.GetAsync(installerRequestPath);
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.That(responseContent, Is.EqualTo(expectedContent));
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "d973c0b7-23f8-4799-b2b9-e966cb57f5e2", "\"1bf53eb256d4024\"", "GET")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}", "f247ba05-6c3b-4407-8b56-49d7bba58537", "\"1bf53eb256d4000\"", "GET")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "d973c0b7-23f8-4799-b2b9-e966cb57f5e2", "\"1bf53eb256d4024\"", "HEAD")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}", "f247ba05-6c3b-4407-8b56-49d7bba58537", "\"1bf53eb256d4000\"", "HEAD")]
		public async Task ETagAppliedToInstallers(string installerFileName, string installerRequestPath, string content, string expectedETag, string method)
		{
			var httpMethod = method switch
			{
				"GET" => HttpMethod.Get,
				"HEAD" => HttpMethod.Head,
				_ => throw new ArgumentOutOfRangeException(nameof(method))
			};

			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName), content);

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Array.Empty<byte>());

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton(appInstallerHandlerMock.Object);
			}));

			var client = factory.CreateClient();
			using var request = new HttpRequestMessage(httpMethod, installerRequestPath);
			var response = await client.SendAsync(request);

			Assert.That(response.Headers.ETag!.Tag, Is.EqualTo(expectedETag));
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task InstallersServedWithPublicMustRevalidateNoCacheCacheHeaders(string installerFileName, string installerRequestPath)
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Array.Empty<byte>());

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
				{
					services.AddSingleton(appInstallerHandlerMock.Object);
				}));

			var client = factory.CreateClient();
			var response = await client.GetAsync(installerRequestPath);

			Assert.That(response.Headers.CacheControl!.NoCache, Is.True);
			Assert.That(response.Headers.CacheControl.MustRevalidate, Is.True);
			Assert.That(response.Headers.CacheControl.Public, Is.True);
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task HeadersOnGetAndHeadRequestsForInstallersAreTheSame(string installerFileName, string installerRequestPath)
		{
			var expectedContent = await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			await using var factory = new CustomWebApplicationFactory<Startup>();
			var client = factory.CreateClient();
			var getResponse = await client.GetAsync(installerRequestPath);
			using var headRequest = new HttpRequestMessage(HttpMethod.Head, installerRequestPath);
			var headResponse = await client.SendAsync(headRequest);

			var getHeaders = getResponse.Headers.OrderBy(h => h.Key);
			var headHeaders = headResponse.Headers.OrderBy(h => h.Key);

			Assert.Multiple(() =>
			{
				foreach (var header in getHeaders.Zip(headHeaders))
				{
					Assert.That(header.First.Key, Is.EqualTo(header.Second.Key));
					Assert.That(header.First.Value, Is.EqualTo(header.Second.Value));
				}
			});
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "d973c0b7-23f8-4799-b2b9-e966cb57f5e2", "\"1bf53eb256d4024\"")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}", "f247ba05-6c3b-4407-8b56-49d7bba58537", "\"1bf53eb256d4024\"")]
		public async Task ETagMatchesStatusCode304ResponseBodyEmpty(string installerFileName, string installerRequestPath, string content, string expectedETag)
		{
			var expectedContent = await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Encoding.UTF8.GetBytes(content));

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton(appInstallerHandlerMock.Object);
			}));

			var client = factory.CreateClient();
			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, installerRequestPath)
			{
				Headers =
				{
					IfNoneMatch =
					{
						new EntityTagHeaderValue(expectedETag)
					}
				}
			};
			var response = await client.SendAsync(httpRequestMessage);
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
			Assert.That(responseContent.Length, Is.Zero);
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "d973c0b7-23f8-4799-b2b9-e966cb57f5e2")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}", "f247ba05-6c3b-4407-8b56-49d7bba58537")]
		public async Task ETagDoesNotMatchStatusCode200ResponseBodyMatchesContent(string installerFileName, string installerRequestPath, string content)
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName), content);

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Encoding.UTF8.GetBytes(content));

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton(appInstallerHandlerMock.Object);
			}));

			var client = factory.CreateClient();
			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, installerRequestPath)
			{
				Headers =
				{
					IfNoneMatch =
					{
						new EntityTagHeaderValue($"\"{Guid.NewGuid()}\"")
					}
				}
			};
			var response = await client.SendAsync(httpRequestMessage);
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(responseContent, Is.EqualTo(content));
		}

		[TestCase("/")]
		[TestCase("/_sessionbroker")]
		[TestCase("/_sessionbroker/auth")]
		public async Task ETagNotAppliedOnOtherRoutes(string route)
		{
			await using var factory = new CustomWebApplicationFactory<Startup>();
			var client = factory.CreateClient();
			var response = await client.GetAsync(route);

			Assert.That(response.Headers.ETag, Is.Null);
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "24.7.1.0", "Mon, 01 Jul 2024 00:00:00 GMT", "GET")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "24.7.1.1", "Mon, 01 Jul 2024 00:00:01 GMT", "GET")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "24.7.1.65", "Mon, 01 Jul 2024 00:01:05 GMT", "GET")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "24.7.1.0", "Mon, 01 Jul 2024 00:00:00 GMT", "HEAD")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "24.7.1.1", "Mon, 01 Jul 2024 00:00:01 GMT", "HEAD")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "24.7.1.65", "Mon, 01 Jul 2024 00:01:05 GMT", "HEAD")]
		public async Task LastModifiedHeaderMatchTheFileVersionDateAsync(string installerFileName, string installerRequestPath, string version, string expectedLastModified, string method)
		{
			var httpMethod = method switch
			{
				"GET" => HttpMethod.Get,
				"HEAD" => HttpMethod.Head,
				_ => throw new ArgumentOutOfRangeException(nameof(method))
			};

			var testAppInstallerContent = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""http://localhost:5000/staticfiles/CargowiseAppx.appinstaller""
	Version=""{version}"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<MainPackage
		Name=""CargoWise-Client""
		Version=""{version}""
		Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU""
		ProcessorArchitecture=""x64""
		Uri=""http://localhost:5000/staticfiles/Cargowise.msix"" />
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";

			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));
			await MockInstallerFileHelper.CreateMockInstallerFile(InstallerPathsForTest.Instance.ClientAppInstallerPath, testAppInstallerContent);

			using var f = new CustomWebApplicationFactory<Startup>();
			using var factory = f.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton<IAppInstallerHandler, AppInstallerHandler>();
			}));
			var client = factory.CreateClient();
			using var request = new HttpRequestMessage(httpMethod, installerRequestPath);
			var response = await client.SendAsync(request);

			var expectedLastModifiedDateTime = DateTimeOffset.Parse(expectedLastModified);
			Assert.That(response.Content.Headers.LastModified, Is.EqualTo(expectedLastModifiedDateTime));
		}

		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task AppInstallerShouldHasReplacedUri(string installerFileName, string installerRequestPath)
		{
			var version = "24.7.1.5";
			var testAppInstallerContent = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""http://not-localhost:444/test.appinstaller""
	Version=""{version}"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<MainPackage
		Name=""CargoWise-Client""
		Version=""{version}""
		Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU""
		ProcessorArchitecture=""x64""
		Uri=""http://not-localhost:4234/test.msix"" />
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";

			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName), testAppInstallerContent);

			using var f = new CustomWebApplicationFactory<Startup>();
			using var factory = f.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton<IAppInstallerHandler, AppInstallerHandler>();
			}));
			var client = factory.CreateClient();
			var response = await client.GetAsync(installerRequestPath);
			var responseContent = await response.Content.ReadAsStringAsync();

			var xml = XDocument.Parse(responseContent);
			Assert.That(xml.Root, Is.Not.Null);
			Assert.That(xml.Root.Attribute("Uri").Value, Is.EqualTo("http://localhost/_clientapi/installer/client.appinstaller"));

			var mainPackage = xml.Descendants()
				.FirstOrDefault(e => e.Name.LocalName.Equals("MainPackage", StringComparison.OrdinalIgnoreCase));
			Assert.That(mainPackage, Is.Not.Null);
			Assert.That(mainPackage.Attribute("Uri").Value, Is.EqualTo("http://localhost/_sessionbroker/installer/client.msix"));
		}

		[TestCase("https", "cw1.cargowise.com")]
		[TestCase("http", "another-app.what.app1:8080")]
		public async Task AppInstallerShouldHasReplacedUriWithXForwardedHeaders(string schema, string host)
		{
			var installerFilePath = InstallerPathsForTest.Instance.ClientAppInstallerPath;
			var installerRequestPath = $"{Arguments.ClientAppInstallerDownloadUrl}";

			var version = "24.7.1.5";
			var testAppInstallerContent = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""http://localhost:5000/_clientapi/installer/test.appinstaller""
	Version=""{version}"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<MainPackage
		Name=""CargoWise-Client""
		Version=""{version}""
		Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU""
		ProcessorArchitecture=""x64""
		Uri=""http://localhost:5000/_sessionbroker/installer/test.msix"" />
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";

			await MockInstallerFileHelper.CreateMockInstallerFile(installerFilePath, testAppInstallerContent);

			using var f = new CustomWebApplicationFactory<Startup>();
			using var factory = f.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton<IAppInstallerHandler, AppInstallerHandler>();
			}));
			var client = factory.CreateClient();
			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, installerRequestPath)
			{
				Headers =
				{
					{ "X-Forwarded-Proto", schema },
					{ "X-Forwarded-Host", host },
				}
			};
			var response = await client.SendAsync(httpRequestMessage);
			var responseContent = await response.Content.ReadAsStringAsync();

			var xml = XDocument.Parse(responseContent);
			Assert.That(xml.Root, Is.Not.Null);
			Assert.That(xml.Root.Attribute("Uri").Value, Is.EqualTo($"{schema}://{host}/_clientapi/installer/client.appinstaller"));

			var mainPackage = xml.Descendants()
				.FirstOrDefault(e => e.Name.LocalName.Equals("MainPackage", StringComparison.OrdinalIgnoreCase));
			Assert.That(mainPackage, Is.Not.Null);
			Assert.That(mainPackage.Attribute("Uri").Value, Is.EqualTo($"{schema}://{host}/_sessionbroker/installer/client.msix"));

			var expectedAppInstallerContent = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""{schema}://{host}/_clientapi/installer/client.appinstaller""
	Version=""{version}"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<MainPackage
		Name=""CargoWise-Client""
		Version=""{version}""
		Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU""
		ProcessorArchitecture=""x64""
		Uri=""{schema}://{host}/_sessionbroker/installer/client.msix"" />
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";
			Assert.That(XDocument.Parse(responseContent).Root.ToString(), Is.EqualTo(XDocument.Parse(expectedAppInstallerContent).Root.ToString()));
			Assert.That(response.Content.Headers.ContentLength, Is.Not.EqualTo(testAppInstallerContent.Length));
		}

		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task AppInstallerShouldResponseTwiceAndNoExceptionOccurred(string installerFileName, string installerRequestPath)
		{
			var version = "24.7.1.5";
			var testAppInstallerContent = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
	Uri=""http://not-localhost:444/test.appinstaller""
	Version=""{version}"" xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017/2"">
	<MainPackage
		Name=""CargoWise-Client""
		Version=""{version}""
		Publisher=""CN=WTG Internal Code Signing, O=Wisetech Global Limited, L=Alexandria, S=New South Wales, C=AU""
		ProcessorArchitecture=""x64""
		Uri=""http://not-localhost:4234/test.msix"" />
	<UpdateSettings>
		<OnLaunch
			HoursBetweenUpdateChecks=""0"" />
	</UpdateSettings>
</AppInstaller>
";

			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName), testAppInstallerContent);

			using var f = new CustomWebApplicationFactory<Startup>();
			using var factory = f.WithWebHostBuilder(b => b.ConfigureServices(services =>
			{
				services.AddSingleton<IAppInstallerHandler, AppInstallerHandler>();
			}));
			var client = factory.CreateClient();
			var response = await client.GetAsync(installerRequestPath);
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			var xml = XDocument.Parse(responseContent);
			Assert.That(xml.Root, Is.Not.Null);

			response = await client.GetAsync(installerRequestPath);
			response.EnsureSuccessStatusCode();
			responseContent = await response.Content.ReadAsStringAsync();

			xml = XDocument.Parse(responseContent);
			Assert.That(xml.Root, Is.Not.Null);
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task HeadRequestSupportedForPath(string installerFileName, string installerRequestPath)
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Array.Empty<byte>());

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
				{
					services.AddSingleton(appInstallerHandlerMock.Object);
				}));

			var client = factory.CreateClient();
			using var request = new HttpRequestMessage(HttpMethod.Head, installerRequestPath);
			var response = await client.SendAsync(request);
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.That(responseContent, Is.Empty);
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		}

		[Test]
		public async Task RequestStaticFilesNotInListShouldReturn404NotFoundAsync()
		{
			var fileName = "test-sessionbroker-static-file.exe";
			var filePath = Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, fileName);
			await MockInstallerFileHelper.CreateMockInstallerFile(filePath);

			try
			{
				var downloadUrl = $"/_sessionbroker/installer/{fileName}";

				await using var factory = new CustomWebApplicationFactory<Startup>();
				var client = factory.CreateClient();
				var response = await client.GetAsync(downloadUrl);

				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
			}
			finally
			{
				try
				{
					File.Delete(filePath);
				}
				catch
				{
				}
			}
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "d973c0b7-23f8-4799-b2b9-e966cb57f5e2", 8, "d973c0b7")]
		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "1234567890", 3, "123")]
		public async Task RequestMsixPackageFileShouldSupportHttpRangeMethodAsync(string installerFileName, string installerRequestPath, string content, int expectedLength, string expectedRangeContent)
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName), content);

			await using var factory = new CustomWebApplicationFactory<Startup>();
			var client = factory.CreateClient();
			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, installerRequestPath)
			{
				Headers =
				{
					Range = new RangeHeaderValue(0, expectedLength - 1),
				}
			};
			var response = await client.SendAsync(httpRequestMessage);
			var responseContent = await response.Content.ReadAsStringAsync();

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.PartialContent));
			Assert.That(response.Content.Headers.ContentRange, Is.Not.Null);
			Assert.That(response.Content.Headers.ContentLength, Is.EqualTo(expectedLength));
			Assert.That(responseContent, Is.EqualTo(expectedRangeContent));
		}

		static readonly object[] InstallerFileMissingCases =
		{
			new object[] { HttpMethod.Head, $"{Arguments.ClientMsixPackageDownloadUrl}" },
			new object[] { HttpMethod.Head, $"{Arguments.ClientAppInstallerDownloadUrl}" },
			new object[] { HttpMethod.Get, $"{Arguments.ClientMsixPackageDownloadUrl}" },
			new object[] { HttpMethod.Get, $"{Arguments.ClientAppInstallerDownloadUrl}" },
		};

		[TestCaseSource(nameof(InstallerFileMissingCases))]
		public async Task InstallerFileMissing4xxReturned(HttpMethod method, string installerRequestPath)
		{
			await using var factory = new CustomWebApplicationFactory<Startup>();
			var client = factory.CreateClient();
			using var request = new HttpRequestMessage(method, installerRequestPath);
			var response = await client.SendAsync(request);

			Assert.That((int)response.StatusCode, Is.GreaterThanOrEqualTo((int)HttpStatusCode.BadRequest));
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}")]
		public async Task SupportedFileRequestStatusCodeIsSuccess(string installerFileName, string installerRequestPath)
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Array.Empty<byte>());

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
				{
					services.AddSingleton(appInstallerHandlerMock.Object);
				}));

			var client = factory.CreateClient();
			var response = await client.GetAsync(installerRequestPath);

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		}

		[TestCase(InstallerFileNames.ClientMsixPackage, $"{Arguments.ClientMsixPackageDownloadUrl}", "application/msix")]
		[TestCase(InstallerFileNames.ClientAppInstaller, $"{Arguments.ClientAppInstallerDownloadUrl}", "application/appinstaller")]
		public async Task InstallerFileMimeTypeIsCorrect(string installerFileName, string installerRequestPath, string contentType)
		{
			await MockInstallerFileHelper.CreateMockInstallerFile(Path.Combine(InstallerPathsForTest.Instance.InstallerDirectoryPath, installerFileName));

			var appInstallerHandlerMock = new Mock<IAppInstallerHandler>();
			appInstallerHandlerMock.Setup(x => x.GetAppInstallerContentWithNewUri(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(Array.Empty<byte>());

			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(b => b.ConfigureServices(services =>
				{
					services.AddSingleton(appInstallerHandlerMock.Object);
				}));

			var client = factory.CreateClient();
			var response = await client.GetAsync(installerRequestPath);
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(response.Content.Headers.ContentType.ToString, Is.EqualTo(contentType));
		}
	}
}
