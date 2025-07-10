using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSRegistrationTypes;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CACFIATest
	{
		class CFIAAIRSRegistrationTypeFileDownloaderForTest : CFIAAIRSRegistrationTypeFileDownloader
		{
			public CFIAAIRSRegistrationTypeFileDownloaderForTest(string rootURLForTest, PreProcessChecker checker) : base(checker)
			{
				RootURLForTest = rootURLForTest;
			}

			public string RootURLForTest { get; set; }

			protected override string RootURL => RootURLForTest;
		}

		const string parentPath = "CFIAAIRSRegistrationTypes";


		[Test]
		public void TestReadHtmlAndCreateXML()
		{
			var helper = new Mock<IHttpClientHelper>();
			helper.Setup(x => x.GetMatchedEntityCodesAsync("XXX", It.IsAny<string[]>()))
				.Returns<string, string[]>((url, inputs) =>
				{
					if (inputs[0] == "Australia")
					{
						return Task.FromResult(new[] { "AU" });
					}
					else if (inputs[0] == "New Zealand")
					{
						return Task.FromResult(new[] { "NZ" });
					}
					else if (inputs[0] == "European Union")
					{
						return Task.FromResult(new[] { "EU" });
					}
					return Task.FromResult(new string[0]);
				}
			);

			var path1 = Path.GetTempFileName();
			var path2 = Path.GetTempFileName();
			using (var stream1 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.DownloadPageInEnglish.html", parentPath)))
			using (var stream2 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.DownloadPageInFrench.html", parentPath)))
			using (var stream3 = TestHelper.GetTestInputFile("CFIAAIRSRegistrationTypes.xml"))
			using (var file1 = File.Create(path1))
			using (var file2 = File.Create(path2))
			{
				stream1.Seek(0, SeekOrigin.Begin);
				stream1.CopyTo(file1);
				stream2.Seek(0, SeekOrigin.Begin);
				stream2.CopyTo(file2);
				stream1.Close();
				stream2.Close();
				file1.Close();
				file2.Close();
				var exportFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\UniversalXmlProducers\CA\CAReferenceData.Tests\TestFiles\Output\CFIAAIRSRegistrationTypes.xml");
				var reader = new DataReader(path1, path2, exportFilePath, helper.Object, "http://localhost:17489/api/EntityMatcher/GetCodes?entityClass={0}&amp;language=EN");
				var result = reader.ReadHtmlAndExportXML();
				Assert.IsTrue(result);
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(exportFilePath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(stream3);

				var contentAdjustPublicationTime = Regex.Replace(
					xmlDoc.InnerXml,
					@"<PublicationTime>.*?<\/PublicationTime>",
					"<PublicationTime>2025-06-06T10:36:51</PublicationTime>",
					RegexOptions.Singleline
				);
				Assert.AreEqual(contentAdjustPublicationTime, expectedXmlDoc.InnerXml);
				stream3.Close();
				File.Delete(exportFilePath);
			}
		}

		[Test]
		public void TestExtractCountryCodes()
		{
			var helper = new Mock<IHttpClientHelper>();
			helper.Setup(x => x.GetMatchedEntityCodesAsync("XXX", It.IsAny<string[]>()))
				.Returns<string, string[]>((url, inputs) =>
				{
					var list = new List<string>();
					foreach (var input in inputs)
					{
						if (input == "Australia")
						{
							list.Add("AU");
						}
						else if (input == "New Zealand")
						{
							list.Add("NZ");
						}
						else if (input == "European Union")
						{
							list.Add("EU");
						}
					}
					list.Sort();
					return Task.FromResult(list.ToArray());
				}
			);
			var dataReader = new DataReader(string.Empty, string.Empty, string.Empty, helper.Object, "XXX");
			var result = dataReader.ExtractCountryCodes("Official Meat Inspection Certificate(New Zealand and Australia)");
			var description = result.Item1;
			var countryCodes = result.Item2;
			Assert.AreEqual("Official Meat Inspection Certificate", description);
			Assert.AreEqual("AU,NZ", countryCodes);

			result = dataReader.ExtractCountryCodes("Official Meat Inspection Certificate(other than New Zealand and Australia)");
			description = result.Item1;
			countryCodes = result.Item2;
			Assert.AreEqual(null, description);
			Assert.AreEqual(null, countryCodes);
		}

		[SetUp]
		public void SetUp()
		{
			checker = new PreProcessChecker(Constants.ProgramFunctions.CFIAAIRSRegistrationTypes);
		}
		PreProcessChecker checker;
	}
}
