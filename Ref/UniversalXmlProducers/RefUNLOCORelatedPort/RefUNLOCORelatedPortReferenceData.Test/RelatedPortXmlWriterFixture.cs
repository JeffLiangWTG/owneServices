using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Test
{
	[TestFixture]
	public class RelatedPortXmlWriterFixture
	{
		[Test]
		public void TestRelatedPortParser()
		{
			var dummyData = new List<PortMapping>
			{
				new PortMapping() { relatedUnlocos = new List<string> { "AUSYD", "NZAKL" } },
				new PortMapping() { relatedUnlocos = new List<string> { "CNSHA", "USLAX", "USCHI" } }
			};

			var publicationTime = new DateTime(2024, 4, 20);
			RelatedPortXmlWriter.Write(dummyData, WriterOutputFilePath, publicationTime);

			var result = File.ReadAllText(WriterOutputFilePath);
			var expected = File.ReadAllText(GetTestFilePath("ExpectedRelatedPortWriterOutput.xml"));
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestRelatedPortParserWithDuplicates()
		{
			var dummyData = new List<PortMapping>
				{
					new PortMapping() { relatedUnlocos = new List<string> { "AUSYD", "NZAKL" } },
					new PortMapping() { relatedUnlocos = new List<string> { "AUSYD", "NZAKL", "CNSHA", "USLAX", "USCHI" } }
				};

			var publicationTime = new DateTime(2024, 4, 20);
			RelatedPortXmlWriter.Write(dummyData, WriterOutputFilePath, publicationTime);

			var result = File.ReadAllText(WriterOutputFilePath);
			var expected = File.ReadAllText(GetTestFilePath("ExpectedRelatedPortWriterDuplicatesOutput.xml"));
			Assert.AreEqual(expected, result);
		}

		string WriterOutputFilePath => GetTestFilePath("RelatedPortXmlWriterOutput.xml");

		string GetTestFilePath(string fileName)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"TestFiles\{fileName}");
		}

		[TearDown]
		protected void TearDown()
		{
			if (File.Exists(WriterOutputFilePath))
			{
				File.Delete(WriterOutputFilePath);
			}
		}
	}
}
