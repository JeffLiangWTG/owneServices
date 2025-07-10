using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWiseNext.Infrastructure.Installations.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.StaticFiles
{
	public class SessionBrokerStaticFileProviderTest
	{
		Mock<IHttpContextAccessor> mockHttpContextAccessor;

		[SetUp]
		public void Setup()
		{
			mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
			mockHttpContextAccessor.Setup(c => c.HttpContext.Request.Headers).Returns(new HeaderDictionary());
			mockHttpContextAccessor.Setup(c => c.HttpContext.Request.Scheme).Returns("http");
			mockHttpContextAccessor.Setup(c => c.HttpContext.Request.Host).Returns(new HostString("localhost"));
		}

		[TestCase(@"CargoWise.msix", nameof(CustomPhysicalFileInfo))]
		[TestCase(@"CargowiseAppx.Appinstaller", nameof(AppInstallerFileInfo))]
		[TestCase(@"cargowise.msix", nameof(CustomPhysicalFileInfo))]
		[TestCase(@"Cargowiseappx.Appinstaller", nameof(AppInstallerFileInfo))]
		[TestCase(@"client.msix", nameof(CustomPhysicalFileInfo))]
		[TestCase(@"client.appinstaller", nameof(AppInstallerFileInfo))]
		[TestCase(@"Client.msix", nameof(CustomPhysicalFileInfo))]
		[TestCase(@"client.AppInstaller", nameof(AppInstallerFileInfo))]
		public void GetFileInfoShouldUseCustomPhysicalFileInfoWhenFileIsDownloadAble(string subPath, string className)
		{
			var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();
			var mockFileResponseHeaderResolver = new Mock<IFileResponseHeaderResolver>();

			var fileProvider = new SessionBrokerStaticFileProvider(mockAppInstallerHandler.Object, mockFileResponseHeaderResolver.Object, mockHttpContextAccessor.Object, InstallerPathsForTest.Instance);

			var fileInfo = fileProvider.GetFileInfo(subPath);
			Assert.That(fileInfo.GetType().Name, Is.EqualTo(className));
		}

		[TestCase(@"..\asd.txt")]
		[TestCase(@"..\123.exe")]
		[TestCase(@"..\cilent.msix")]
		[TestCase(@"..\cargowise.AppInstaller")]
		public void GetFileInfoShouldReturnNotFoundFileInfoWhenTheFileIsNotDownloadable(string subPath)
		{
			var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();
			var mockFileResponseHeaderResolver = new Mock<IFileResponseHeaderResolver>();

			var fileProvider = new SessionBrokerStaticFileProvider(mockAppInstallerHandler.Object, mockFileResponseHeaderResolver.Object, mockHttpContextAccessor.Object, InstallerPathsForTest.Instance);

			var fileInfo = fileProvider.GetFileInfo(subPath);
			Assert.That(fileInfo, Is.InstanceOf<NotFoundFileInfo>());
		}
	}
}
