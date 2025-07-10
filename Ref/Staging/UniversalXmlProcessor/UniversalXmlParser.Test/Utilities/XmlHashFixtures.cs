using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Utilities
{
	[TestFixture]
	public class XmlHashFixtures : BaseUnitTestFixture
	{
		static IEnumerable<TestCaseData> HashXMLStreamTestCases()
		{

			var expctedHashWithoutPublicationTime = "E7E8C06F69F3D8D465589197533F7DDEEC467A446C89B94467763C260888AABD";
			var expctedHashWithPublicationTime = "830B4B910DCFA25012D735FCBD9744DB1E98D6AC84524604D71AC2BB48D3A213";

			var excludeOneElement = new string[] { "PublicationTime" };
			var excludeThreeElements = new string[] { "PublicationTime", "RemoveMe", "AnotherOne" };
			var excludeEmptyArray = new string[] { };

			yield return new TestCaseData(
				@"<UniversalReferenceData>
				<DataSource>Test Data Source data</DataSource>
				<UpdateType>Partial</UpdateType>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AppProgramArgs>TEST</AppProgramArgs>
				</UniversalReferenceData>",
				excludeEmptyArray,
				expctedHashWithoutPublicationTime
				)
			{
				TestName = "HashXMLStream_WithoutPublicationTime"
			};

			yield return new TestCaseData(
				@"<UniversalReferenceData>
				<DataSource>Test Data Source data</DataSource>
				<PublicationTime>2024-03-06T01:56:00</PublicationTime>
				<UpdateType>Partial</UpdateType>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AppProgramArgs>TEST</AppProgramArgs>
				</UniversalReferenceData>",
				null,
				expctedHashWithPublicationTime
				)
			{
				TestName = "HashXMLStream_WithPublicationTime"
			};


			yield return new TestCaseData(
				@"<UniversalReferenceData>
				<DataSource>Test Data Source data</DataSource>
				<PublicationTime>2024-03-06T01:56:00</PublicationTime>
				<UpdateType>Partial</UpdateType>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AppProgramArgs>TEST</AppProgramArgs>
				</UniversalReferenceData>",
				excludeOneElement,
				expctedHashWithoutPublicationTime
			)
			{
				TestName = "HashXMLStream_ExcludePublicationTime"
			};

			yield return new TestCaseData(
				@"<UniversalReferenceData>
				<!-- This is a comment -->
				<DataSource>Test Data Source data</DataSource>
				<PublicationTime>2024-03-06T01:56:00</PublicationTime>
				<UpdateType>Partial</UpdateType>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AppProgramArgs>TEST</AppProgramArgs>
				<!-- This is another comment -->
				</UniversalReferenceData>",
				excludeOneElement,
				expctedHashWithoutPublicationTime
			)
			{
				TestName = "HashXMLStream_WithComments"
			};

			yield return new TestCaseData(
				@"<UniversalReferenceData>

				<DataSource>Test Data Source data</DataSource>


				<PublicationTime>2024-03-06T01:56:00</PublicationTime>
				<UpdateType>Partial</UpdateType>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AppProgramArgs>TEST</AppProgramArgs>


				</UniversalReferenceData>",
					excludeOneElement,
					expctedHashWithoutPublicationTime
				)
			{
				TestName = "HashXMLStream_WithEmptyLines"
			};


			yield return new TestCaseData(
				@"<UniversalReferenceData>
				<DataSource>Test Data Source data</DataSource>
				<PublicationTime>2024-03-06T01:56:00</PublicationTime>
				<UpdateType>Partial</UpdateType>
				<RemoveMe>im not required</RemoveMe>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AnotherOne>im getting removed</AnotherOne>
				<AppProgramArgs>TEST</AppProgramArgs>
				</UniversalReferenceData>",
				excludeThreeElements,
				expctedHashWithoutPublicationTime
			)
			{
				TestName = "HashXMLStream_ExcludeMultipleElements"
			};

			yield return new TestCaseData(
				@"<UniversalReferenceData>
				<DataSource>Test Data Source data</DataSource>
				<UpdateType>Partial</UpdateType>
				<AppName>CargoWise.RefDbRepo.testData.CmdLine.dll</AppName>
				<AppProgramArgs>TEST</AppProgramArgs>
				</UniversalReferenceData>",
				null,
				expctedHashWithoutPublicationTime
			)
			{
				TestName = "HashXMLStream_ExcludeIsNull"
			};

		}

		[TestCaseSource(nameof(HashXMLStreamTestCases))]
		public void HashXMLExcludingElements(string inputXml, string[] excludeElements, string expectedHash)
		{
			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(inputXml)))
			{
				Assert.AreEqual(expectedHash, XmlHash.HashStream(inputStream, excludeElements));
			}
		}

		[TestCase("FileHashSeekFail.zip")]
		public async Task EmptyHashWithZipStreamXML(string fileName)
		{
			using (var xml = GetType().Assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.{fileName}"))
			{
				var memo = new MemoryStream();
				await xml.CopyToAsync(memo);

				// Extract the XML file from the compressed content
				using (var zipArchive = new ZipArchive(new MemoryStream(memo.ToArray()), ZipArchiveMode.Read))
				{
					var xmlFile = zipArchive.Entries.FirstOrDefault(o => o.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
					Assert.IsNotNull(xmlFile, "No XML file found in the compressed archive.");

					using (var xmlStream = xmlFile.Open())
					{
						var excludeElements = new string[] { "PublicationTime" };
						var exception = Assert.Throws<NotSupportedException>(() =>
						{
							XmlHash.HashStream(xmlStream, excludeElements);
						});
						Assert.AreEqual("This operation is not supported.", exception.Message);
					}
				}
			}
		}

	}
}
