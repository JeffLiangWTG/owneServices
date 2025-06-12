using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace eServices.BuildTools.PackTool.Tests
{
	public partial class PackCommandServiceTests
	{
		[TestCase("eServices.BuildTools.SqlDeploy.1.0.0-rc.3", "eServices.BuildTools.SqlDeploy.1.0.0-rc.4", false, "")]
		[TestCase("eServices.BuildTools.SqlDeploy.1.0.0-rc.3", "eServices.BuildTools.SqlDeploy.1.0.1", true, "lib/net48/eServices.BuildTools.SqlDeploy.dll (*)\r\nlib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)\r\nREADME.md (*)")]
		public void PackagesAreDifferent_Decompilation(string refFileName, string difFileName, bool expectedResult, string expectedReport)
		{
			var refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{refFileName}.nupkg");
			var difFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"Content\\{difFileName}.nupkg");
			var packCommandService = new PackCommandService(new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }), new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			var areDifferent = packCommandService.PackagesAreDifferent(refFilePath, difFilePath, TestContext.CurrentContext.WorkDirectory, out var differenceReport);

			Assert.Multiple(() =>
			{
				Assert.That(areDifferent, Is.EqualTo(expectedResult));
				Assert.That(differenceReport, Is.EqualTo(expectedReport));
			});
		}

		[Test]
		public void PackCommandService_Invoke_NoReleasePublished_NoPrereleasePublished()
		{
			var packageId = "eServices.BuildTools.NewTool";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.norelease.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.noprerelease.json")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: N/A
				Published pre-release: N/A
				Packed version: 1.0.0
				Packed pre-release: 1.0.0-rc.1

				Branch: Testing
				Version:1.0.0 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.0", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.0 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				""")), Times.Once);

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.0-rc.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.0-rc.1 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				""")), Times.Once);

			buildHandler.Verify(x => x.DeletePackage(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.1.0.0.nupkg")), Times.Once);
		}

		[Test]
		public void PackCommandService_Invoke_NoReleasePublished_PrereleasePublished()
		{
			var packageId = "eServices.BuildTools.NewTool";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.norelease.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0-rc.1.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/flatcontainer/{packageId.ToLowerInvariant()}/1.0.0-rc.1/{packageId.ToLowerInvariant()}.nuspec"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0-rc.1.xml")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			string refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\Pack\\Published\\{packageId}.1.0.0-rc.1.nupkg");
			string difFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Pack\\{packageId}.1.0.0.nupkg");
			string compareWorkDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Content\\Pack\\Compare\\");
			string differenceReport = """
				lib/net48/eServices.BuildTools.NewTool.dll (*)
				lib/net8.0/eServices.BuildTools.NewTool.dll (*)
				""";
			packCommandService.Setup(x => x.PackagesAreDifferent(refFilePath, difFilePath, compareWorkDir, out differenceReport)).Returns(true);

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: N/A
				Published pre-release: 1.0.0-rc.1
				Packed version: 1.0.0
				Packed pre-release: 1.0.0-rc.2

				Branch: Testing
				Version:1.0.0 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.NewTool.dll (*)
				lib/net8.0/eServices.BuildTools.NewTool.dll (*)

				Branch: New Tool
				Version:1.0.0-rc.1 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.0", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.0 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.NewTool.dll (*)
				lib/net8.0/eServices.BuildTools.NewTool.dll (*)
				
				Branch: New Tool
				Version:1.0.0-rc.1 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				""")), Times.Once);

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.0-rc.2", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.0-rc.2 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.NewTool.dll (*)
				lib/net8.0/eServices.BuildTools.NewTool.dll (*)
				
				Branch: New Tool
				Version:1.0.0-rc.1 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				""")), Times.Once);

			buildHandler.Verify(x => x.DeletePackage(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.1.0.0.nupkg")), Times.Once);
		}

		[Test]
		public void PackCommandService_Invoke_NoReleasePublished_PrereleasePublished_NoDifferences()
		{
			var packageId = "eServices.BuildTools.NewTool";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.norelease.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0-rc.1.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/flatcontainer/{packageId.ToLowerInvariant()}/1.0.0-rc.1/{packageId.ToLowerInvariant()}.nuspec"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0-rc.1.xml")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			string refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\Pack\\Published\\{packageId}.1.0.0-rc.1.nupkg");
			string difFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Pack\\{packageId}.1.0.0.nupkg");
			string compareWorkDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Content\\Pack\\Compare\\");
			string differenceReport = "";
			packCommandService.Setup(x => x.PackagesAreDifferent(refFilePath, difFilePath, compareWorkDir, out differenceReport)).Returns(false);

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: N/A
				Published pre-release: 1.0.0-rc.1
				Packed version: 1.0.0
				Packed pre-release: 1.0.0-rc.1
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.0", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: New Tool
				Version:1.0.0 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				""")), Times.Once);

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.0-rc.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: New Tool
				Version:1.0.0-rc.1 Author:Testing PackTime:9999-99-99T99:99:99
				Initial release.
				""")), Times.Once);

			buildHandler.Verify(x => x.DeletePackage(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.1.0.0.nupkg")), Times.Once);
		}

		[Test]
		public void PackCommandService_Invoke_ReleasePublished_NoPrereleasePublished()
		{
			var packageId = "eServices.BuildTools.SqlDeploy";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0.json")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			string refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\Pack\\Published\\{packageId}.1.0.0.nupkg");
			string difFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Pack\\{packageId}.1.0.0.nupkg");
			string compareWorkDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Content\\Pack\\Compare\\");
			string differenceReport = """
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""";
			packCommandService.Setup(x => x.PackagesAreDifferent(refFilePath, difFilePath, compareWorkDir, out differenceReport)).Returns(true);

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: 1.0.0
				Published pre-release: N/A
				Packed version: 1.0.1
				Packed pre-release: 1.0.1-rc.1

				Branch: Testing
				Version:1.0.1 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.1 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""")), Times.Once);

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.1-rc.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.1-rc.1 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""")), Times.Once);

			buildHandler.Verify(x => x.DeletePackage(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.1.0.0.nupkg")), Times.Once);
		}

		[Test]
		public void PackCommandService_Invoke_ReleasePublished_NoPrereleasePublished_NoDifferences()
		{
			var packageId = "eServices.BuildTools.SqlDeploy";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0.json")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			string refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\Pack\\Published\\{packageId}.1.0.0.nupkg");
			string difFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Pack\\{packageId}.1.0.0.nupkg");
			string compareWorkDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Content\\Pack\\Compare\\");
			string differenceReport = "";
			packCommandService.Setup(x => x.PackagesAreDifferent(refFilePath, difFilePath, compareWorkDir, out differenceReport)).Returns(false);

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: 1.0.0
				Published pre-release: N/A
				Packed version: *no differences*
				Packed pre-release: *no differences*
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			buildHandler.Verify(x => x.DeletePackage(It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void PackCommandService_Invoke_ReleasePublished_PrereleasePublished()
		{
			var packageId = "eServices.BuildTools.SqlDeploy";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.1-rc.1.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/flatcontainer/{packageId.ToLowerInvariant()}/1.0.1-rc.1/{packageId.ToLowerInvariant()}.nuspec"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.1-rc.1.xml")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			string refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\Pack\\Published\\{packageId}.1.0.1-rc.1.nupkg");
			string difFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Pack\\{packageId}.1.0.0.nupkg");
			string compareWorkDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Content\\Pack\\Compare\\");
			string differenceReport = """
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""";
			packCommandService.Setup(x => x.PackagesAreDifferent(refFilePath, difFilePath, compareWorkDir, out differenceReport)).Returns(true);

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: 1.0.0
				Published pre-release: 1.0.1-rc.1
				Packed version: 1.0.1
				Packed pre-release: 1.0.1-rc.2

				Branch: Testing
				Version:1.0.1 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)

				WI00867228 - Fix eHubTransactionsCore test setup ðŸ¤  BuildTools fixes
				https://devops.wisetechglobal.com/wtg/eServices/_git/eServices/pullrequest/313958
				Version:1.0.1-rc.1 Author:CORP\Brett.Lyons PackTime:2025-02-25T09:01:56
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.1 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)

				WI00867228 - Fix eHubTransactionsCore test setup ðŸ¤  BuildTools fixes
				https://devops.wisetechglobal.com/wtg/eServices/_git/eServices/pullrequest/313958
				Version:1.0.1-rc.1 Author:CORP\Brett.Lyons PackTime:2025-02-25T09:01:56
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""")), Times.Once);

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.1-rc.2", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				Branch: Testing
				Version:1.0.1-rc.2 Author:Testing PackTime:9999-99-99T99:99:99
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)

				WI00867228 - Fix eHubTransactionsCore test setup ðŸ¤  BuildTools fixes
				https://devops.wisetechglobal.com/wtg/eServices/_git/eServices/pullrequest/313958
				Version:1.0.1-rc.1 Author:CORP\Brett.Lyons PackTime:2025-02-25T09:01:56
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""")), Times.Once);

			buildHandler.Verify(x => x.DeletePackage(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.1.0.0.nupkg")), Times.Once);
		}

		[Test]
		public void PackCommandService_Invoke_ReleasePublished_PrereleasePublished_NoDifferences()
		{
			var packageId = "eServices.BuildTools.SqlDeploy";
			var publishUrl = "https://localhost/nuget/WTG-Internal/v3/index.json";
			var httpClient = new Mock<HttpClient>(MockBehavior.Strict);

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == publishUrl), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\nuget.index.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=false"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.0.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/search?q={packageId}&prerelease=true"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.1-rc.1.json")))
				});

			httpClient.Setup(x => x.Send(It.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.ToString() == $"https://localhost/nuget/WTG-Internal/v3/flatcontainer/{packageId.ToLowerInvariant()}/1.0.1-rc.1/{packageId.ToLowerInvariant()}.nuspec"), It.IsAny<CancellationToken>()))
				.Returns(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.1.0.1-rc.1.xml")))
				});

			var buildHandler = new Mock<IBuildHandler>();
			buildHandler.Setup(x => x.ProjectFile).Returns(new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\{packageId}.csproj")));
			buildHandler.Setup(x => x.PackageId).Returns(packageId);
			buildHandler.Setup(x => x.PackageVersion).Returns("1.0.0");
			buildHandler.Setup(x => x.PackOutputPath).Returns(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack"));
			buildHandler.Setup(x => x.NuspecOutputPath).Returns("Pack\\");

			var packCommandService = new Mock<PackCommandService>(httpClient.Object, new FakeLogger<PackCommandService>(TestContext.Out.WriteLine));

			string? author = "Testing";
			string releaseNotesHeader = "Branch: Testing";
			packCommandService.Setup(x => x.FormatReleaseNotesHeader(It.IsAny<DirectoryInfo>(), httpClient.Object, out author, out releaseNotesHeader));

			string refFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Content\\Pack\\Published\\{packageId}.1.0.1-rc.1.nupkg");
			string difFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Pack\\{packageId}.1.0.0.nupkg");
			string compareWorkDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Content\\Pack\\Compare\\");
			string differenceReport = "";
			packCommandService.Setup(x => x.PackagesAreDifferent(refFilePath, difFilePath, compareWorkDir, out differenceReport)).Returns(false);

			packCommandService.Object.Invoke(buildHandler.Object, new Uri(publishUrl));

			var reportDetails = RegexAuthPackTime().Replace(File.ReadAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.pack-report.txt")),
				AuthorPackTimeReplacement);
			Assert.That(reportDetails, Is.EqualTo("""
				Published version: 1.0.0
				Published pre-release: 1.0.1-rc.1
				Packed version: 1.0.1
				Packed pre-release: 1.0.1-rc.1
				"""));

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				WI00867228 - Fix eHubTransactionsCore test setup ðŸ¤  BuildTools fixes
				https://devops.wisetechglobal.com/wtg/eServices/_git/eServices/pullrequest/313958
				Version:1.0.1 Author:CORP\Brett.Lyons PackTime:2025-02-25T09:01:56
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""")), Times.Once);

			buildHandler.Verify(x => x.BuildPackageVersion("1.0.1-rc.1", It.Is<string>(r => RegexAuthPackTime().Replace(r, AuthorPackTimeReplacement) == """
				WI00867228 - Fix eHubTransactionsCore test setup ðŸ¤  BuildTools fixes
				https://devops.wisetechglobal.com/wtg/eServices/_git/eServices/pullrequest/313958
				Version:1.0.1-rc.1 Author:CORP\Brett.Lyons PackTime:2025-02-25T09:01:56
				Changes [modified=(*) added=(+) deleted=(-)]:
				lib/net48/eServices.BuildTools.SqlDeploy.dll (*)
				lib/net8.0/eServices.BuildTools.SqlDeploy.dll (*)
				""")), Times.Once);

			buildHandler.Verify(x => x.DeletePackage(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Pack", $"{packageId}.1.0.0.nupkg")), Times.Once);
		}

		[GeneratedRegex(@"Author:Testing PackTime:\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}")]
		private static partial Regex RegexAuthPackTime();
		private const string AuthorPackTimeReplacement = "Author:Testing PackTime:9999-99-99T99:99:99";
	}
}