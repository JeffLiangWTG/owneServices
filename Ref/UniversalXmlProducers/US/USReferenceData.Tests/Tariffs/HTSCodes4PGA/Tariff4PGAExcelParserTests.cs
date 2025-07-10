using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class Tariff4PGAExcelParserTests
	{
		[Test]
		public void TestParse()
		{
			AssertParse(@"New Master DEA Drug Code-HTS-Schedule B Reference Table 05012019.xlsx", 233);
		}

		[Test]
		public void TestParse_EmptyFile()
		{
			AssertParse(@"DEA Empty File.xlsx", 0);
		}

		[Test]
		public void TestParse_ColumnDoesNotMatch()
		{
			AssertParse(@"DEA Column Schedule B does not exist.xlsx", 174);
			AssertParse(@"DEA Column HTS Code does not exist.xlsx", 59);
			AssertParse(@"DEA no Column exist.xlsx", 0);
		}

		void AssertParse(string fileName, int resultCount)
		{
			var filePath = Path.Combine(inputPath, fileName);
			var tariff4PGAList = new List<Tariff4PGA>();
			var parser = new Tariff4PGAExcelParser(tariff4PGAList, filePath);
			parser.Parse();
			Assert.That(tariff4PGAList.Count == resultCount);
		}

		[SetUp]
		public void Setup()
		{
			inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Tariffs\TestFiles\Input");
		}
		string inputPath;
	}
}
