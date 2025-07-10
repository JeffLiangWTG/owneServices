using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class TaricUpdateParserTest
{
	[Test]
	public void ConstructorValidation()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			new TaricUpdateParser(null);
		}, "Constructor argument is null");
	}

	[Test]
	public void ParseAndSaveTariffUpdateCreatesFile()
	{
		var inputResourceName = "CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.TestFiles.Input.KopertaContent.xml";
		byte[] input = Encoding.UTF8.GetBytes(TestHelper.ReadManifestResourceContent(inputResourceName));
		const string resultFolder = "..\\UXmlFiles\\Process\\BaseUpdate";
		var taricConfigMock = new Mock<ITaricUpdateParserConfig>();
		taricConfigMock.SetupGet(x => x.TariffUpdateFolder).Returns(resultFolder);

		var resultLocation = Path.Combine(resultFolder,"update20200720.xml");
		string result;
		try
		{
			var parseAndSaveTariffUpdateResult = new TaricUpdateParser(taricConfigMock.Object).ParseAndSave(input);

			Assert.AreEqual(true, parseAndSaveTariffUpdateResult, "ParseAndSaveTariffUpdate should succeed");

			result = File.ReadAllText(resultLocation);
		}
		finally
		{
			if(File.Exists(resultLocation))
			{
				File.Delete(resultLocation);
			}
		}

		var outputResourceName = "CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.TestFiles.Output.update20200720_KopertaTest.xml";
		var expected = TestHelper.ReadManifestResourceContent(outputResourceName);
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCase("WrongRootName", false, false, 0, "Invalid xml have been passed.")]
	[TestCase("KopertaContent", false, false, 0, "Empty value have been passed in xml root.")]
	[TestCase("KopertaContent", true, false, 0, "Zip without entries have been passed.")]
	[TestCase("KopertaContent", true, true, 0, "Searched Zip Entry not found or is empty.")]
	[TestCase("KopertaContent", true, true, -1, "akceptujeZobowiazania must be set to 'TRUE'.")]
	[TestCase("KopertaContent", true, true, -2, "startDate value must be lower than endDate.")]
	[TestCase("KopertaContent", true, true, -3, "Time difference between startDate and endDate must be lower than 7 days.")]
	[TestCase("KopertaContent", true, true, -7, "endDate value must be lower or equal current date.")]
	public void ParseAndSaveTariffUpdateOutputError(string rootName, bool withZip, bool withZipEntry, int errorCode, string expectedError)
	{
		var base64Content = withZip ? CreateBase64ZipContent(withZipEntry, errorCode) : string.Empty;
		var input = CreateTestInput(rootName, base64Content);

		bool result;
		string resultOutput;
		using (var sw = new StringWriter())
		{
			Console.SetError(sw);
			var taricConfigMock = new Mock<ITaricUpdateParserConfig>();
			result = new TaricUpdateParser(taricConfigMock.Object).ParseAndSave(input);
			resultOutput = sw.ToString();
		}

		Assert.Multiple(() =>
		{
			Assert.That(result, Is.EqualTo(false));
			Assert.That(resultOutput, Contains.Substring(expectedError));
		});
	}

	static byte[] CreateTestInput(string rootName, string base64Content)
	{
		const string testXmlTempate = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><{0}>{1}</{0}>";
		string testXml = string.Format(CultureInfo.InvariantCulture, testXmlTempate, rootName, base64Content);
		var input = Encoding.UTF8.GetBytes(testXml);
		return input;
	}

	string CreateBase64ZipContent(bool withEntry, int errorCode = 0)
	{
		using (var memstream = new MemoryStream())
		{
			using (var zip = new ZipArchive(memstream, ZipArchiveMode.Update))
			{
				if (withEntry)
				{
					var entry = zip.CreateEntry("IsztarHistoryResponse.xml");
					if (errorCode != 0)
					{
						var responseWithError = new IsztarHistoryResponse()
						{
							ResultsInfo = new ResultsInfo()
							{
								totalRecords = "0",
								errorCode = errorCode,
								errorCodeSpecified = true
							}
						};
						using (StreamWriter writer = new StreamWriter(entry.Open()))
						{
							var responseXml = XmlParser.Serialize(responseWithError).ToString();
							writer.Write(responseXml);
						}
					}
				}
			}
			return Convert.ToBase64String(memstream.ToArray());
		}
	}
}
