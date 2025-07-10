using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class ECCNNumbersParserTest
	{
		[Test]
		public void TestECCNNumbersParser()
		{
			using (var streamHtml1 = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ECCN_ear.html"))
			using (var readerHtml1 = new StreamReader(streamHtml1, Encoding.UTF8))
			using (var streamHtml2 = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ECCN_Number.html"))
			using (var readerHtml2 = new StreamReader(streamHtml2, Encoding.UTF8))
			{
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync("EARFilesCreatedByBISURL")).Returns(Task.FromResult(readerHtml1.ReadToEnd()));
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync("EARFilesBaseURL/regulations/ear/part-774/supplement-1-774/commerce-control-list#category0")).Returns(Task.FromResult(readerHtml2.ReadToEnd()));
				var parser = new ECCNNumbersDownloadAndConvertToXMLParserTest(OutputFileDirectoryPath, mockHttpClientHelper.Object);
				parser.SetPublicationDateTime(new DateTime(2024, 04, 10));

				parser.ConvertCodeListToXML();

				var exceptXML = ReadTestFile("ExpectedECCNNumbers.xml");
				var generatedXML = File.ReadAllText(parser.OutputFilePath_Exposed);

				var builder = DiffBuilder.Compare(exceptXML)
					.WithTest(generatedXML)
					.IgnoreWhitespace()
					.WithNodeMatcher(new DefaultNodeMatcher(ElementSelectors.ByNameAndText))
					.Build();

				Assert.IsFalse(builder.HasDifferences(), $"ECCNNumbers. Differences: {string.Join(Environment.NewLine, builder.Differences)}");
			}
		}

		string OutputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles\");

		string ReadTestFile(string fileName)
		{
			var result = string.Empty;
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles." + fileName))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.USReferenceData.Tests.config.json");
			mockHttpClientHelper = new Mock<IHttpClientHelper>();
			assembly = Assembly.GetExecutingAssembly();
		}
		Mock<IHttpClientHelper> mockHttpClientHelper;
		Assembly assembly;

		[TearDown]
		public void TearDown()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.USReferenceData.CmdLine.config.json");
		}

		class ECCNNumbersDownloadAndConvertToXMLParserTest : ECCNNumbersParser
		{
			public ECCNNumbersDownloadAndConvertToXMLParserTest(string outputFileDirectoryPath, IHttpClientHelper mockHttpClientHelper) : base(ApplicationConfig.InstanceForTest.EARFilesCreatedByBISURL, outputFileDirectoryPath)
			{
				this.mockHttpClientHelper = mockHttpClientHelper;

				Type parentType = typeof(ECCNNumbersParser);
				FieldInfo privateField = parentType.GetField("baseURL", BindingFlags.NonPublic | BindingFlags.Instance);
				if (privateField != null)
				{
					privateField.SetValue(this, ApplicationConfig.InstanceForTest.EARFilesBaseURL);
				}
			}
			IHttpClientHelper mockHttpClientHelper;

			public void SetPublicationDateTime(DateTime dateTimeForTest) => dateTime = dateTimeForTest;

			public string OutputFilePath_Exposed => base.OutputFilePath;

			DateTime dateTime { get; set; }
			protected override DateTime PublicationDateTime => dateTime;

			public override IHttpClientHelper ServiceClient => this.mockHttpClientHelper;
		}
	}
}
