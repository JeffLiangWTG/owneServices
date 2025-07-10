using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class SACUTradeGroupParserTest
	{
		[Test]
		public void TestXmlExport()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Output.RefCusTradeGroup_BR_SACU.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACUTradeGroup.txt"))
			{
				var publicationDate = new DateTime(2022, 07, 14, 00, 00, 00);
				var parser = new SACUTradeGroupParser("BR SACU Trade Group");

				using (var reader = new StreamReader(inputStream))
				{
					var inputData = reader.ReadToEnd().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					parser.ExportToXMLFile(inputData, TestOutputFilePath, publicationDateTime: publicationDate);

					using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
					{
						StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					}
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePath);
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Output.Temp.RefCusTradeGroup_BR_SACU.xml");
	}
}
