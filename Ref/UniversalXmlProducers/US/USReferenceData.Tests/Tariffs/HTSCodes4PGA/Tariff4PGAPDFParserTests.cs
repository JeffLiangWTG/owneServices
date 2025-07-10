using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class Tariff4PGAPDFParserTests
	{
		[Test]
		public void TestParse_EmptyAppndixFile()
		{
			var filePath = Path.Combine(inputPath, @"Appendix_X_PGA_07202022_508c_0 - empty.pdf");
			var tariff4PGAList = new List<Tariff4PGA>();
			var parser = new Tariff4PGAPDFParser(tariff4PGAList,filePath);

			var result = parser.Parse();
			Assert.AreEqual(@"Page 2/12 PGA =  ignored.
Page 3/12 PGA =  ignored.
Page 4/12 PGA =  ignored.
Page 5/12 PGA =  ignored.
Page 6/12 PGA =  ignored.
Page 7/12 PGA =  ignored.
Page 8/12 PGA =  ignored.
Page 9/12 PGA =  ignored.
Page 10/12 PGA =  ignored.
Page 11/12 PGA =  ignored.
Page 12/12 PGA =  ignored.
", result);
			Assert.That(tariff4PGAList.Count == 0);
		}

		[Test]
		public void TestParse_FullAppndixFile()
		{
			var filePath = Path.Combine(inputPath, @"Appendix_X_PGA_07202022_508c_0.pdf");
			var tariff4PGAList = new List<Tariff4PGA>();
			var parser = new Tariff4PGAPDFParser(tariff4PGAList, filePath);

			var result = parser.Parse();
			Assert.AreEqual(@"Page 3/12 PGA = ATF ignored.
Page 4/12 PGA = DEA ignored.
Page 8/12 PGA = FWS ignored.
", result);
			Assert.That(tariff4PGAList.Where(w => w.PGACode.Equals("AMS", StringComparison.Ordinal) && w.IsMandatory).Count() == 6);
			Assert.That(tariff4PGAList.Where(w => w.PGACode.Equals("EPA", StringComparison.Ordinal) && w.IsMandatory).Count() == 7);
			Assert.That(tariff4PGAList.Where(w => w.PGACode.Equals("EPA", StringComparison.Ordinal) && !w.IsMandatory).Count() == 350);
			Assert.That(tariff4PGAList.Where(w => w.PGACode.Equals("NMFS", StringComparison.Ordinal) && w.IsMandatory).Count() == 34);
			Assert.That(tariff4PGAList.Where(w => w.PGACode.Equals("TTB", StringComparison.Ordinal) && w.IsMandatory).Count() == 35);
			Assert.That(tariff4PGAList.Count == 432);
		}

		[SetUp]
		public void Setup()
		{
			inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Tariffs\TestFiles\Input");
		}
		string inputPath;
	}
}
