using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.STLBillingCollector.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.STLBillingCollector.Tests.STLCollectorTest
{
	[TestFixture]
	public class STLCollectorTest
	{
		[Test]
		public void TestGenerateStlCollectorDataXmlFromNuget_Success()
		{
			var sourcePage = TestHelper.ReadManifestResourceContent($"{TestInputFilesDir}.{ValidInputFile}");
			httpClientMock.Setup(x => x.GetWebPageAsync(sourceUrl)).Returns(Task.FromResult(sourcePage));
			var mockPackageStream = TestHelper.ReadManifestResourceAsStream($"{TestInputFilesDir}.{ValidNugetFile}");
			httpClientMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(mockPackageStream);
			var outputBasePath = TestHelper.GenerateTempDirectory().FullName;
			var error = new STLBillingCollectorForTest(httpClientMock.Object,errorStringBuilder).ProcessStlBillingCollectors("CargoWise.Billing.Collectors",outputBasePath);
			Assert.IsEmpty(error);
		}

		[Test]
		public void TestGenerateStlCollectorDataXmlFromNameSpace()
		{
			 outputBasePath = TestHelper.GenerateTempDirectory().FullName;
			 tempDllDirectory = TestHelper.GenerateTempDirectory().FullName;
			var sourceNamespace = "CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors";
			var billingAssembly = AppDomain.CurrentDomain.GetAssemblies()
				.FirstOrDefault(assembly =>
				{
					var name = assembly.GetName().Name;
					return name != null &&
					       name.Equals(sourceNamespace, StringComparison.OrdinalIgnoreCase);
				});
			new STLBillingCollectorForTest(httpClientMock.Object,errorStringBuilder).ProcessAndExportBillingEntities(sourceNamespace,outputBasePath, billingAssembly);
			var expectedFileContent = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.STLBillingCollector.Tests.STLCollectorTest.TestFiles.Output.RefStlScript.xml").Replace("\r\n", "\n");
			var actualFilePath = Path.Combine(outputBasePath, "RefStlScript.xml");
			var actualContent = File.ReadAllText(actualFilePath);
			Assert.That(actualContent.Replace("\r\n", "\n"), Is.EqualTo(expectedFileContent), $"Content mismatch in file: {actualFilePath}");
		}

		#region Implementation
		Mock<IHttpClientHelper> httpClientMock;
		StringBuilder errorStringBuilder;
		const string TestInputFilesDir = "CargoWise.RefDbRepo.STLBillingCollector.Tests.STLCollectorTest.TestFiles.Input";
		const string ValidInputFile = "CargoWise.Billing.Collectors.Versions.html";
		const string ValidNugetFile = "PackageFileInBase64.txt";
		readonly string sourceUrl = AppConfig.Nuget.FeedUrl;
		[TearDown]
		public void TearDown()
		{
			if (!string.IsNullOrEmpty(outputBasePath) && Directory.Exists(outputBasePath))
			{
				Directory.Delete(outputBasePath, recursive: true);
			}

			if (!string.IsNullOrEmpty(tempDllDirectory) && Directory.Exists(tempDllDirectory))
			{
				Directory.Delete(tempDllDirectory, recursive: true);
			}
		}

		[SetUp]
		public void SetUp()
		{
			httpClientMock = new Mock<IHttpClientHelper>();
			errorStringBuilder = new StringBuilder();
			var testBillingPath = $"{TestPath}\\CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.dll";
			if (!File.Exists(testBillingPath))
			{
				throw new FileNotFoundException($"Could not find {testBillingPath}");
			}
			Assembly.LoadFrom(testBillingPath);
		}
		private static readonly string TestPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		private string outputBasePath;
		private string tempDllDirectory;
		#endregion
	}
}
