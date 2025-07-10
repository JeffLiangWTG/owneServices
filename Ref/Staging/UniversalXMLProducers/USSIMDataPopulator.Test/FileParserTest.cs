using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator.Test
{
	[TestFixture]
	public class FileParserTest
	{
		[Test]
		public void ParsePDFShouldReturnCodes()
		{
			var pdfPath = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\Download1b888426-c871-77bc-496f-e457f38be151\PDFSnippet.pdf");
			var codes = FileParser.ParsePDF(pdfPath);
			var expectedCodes = new List<string>()
			{
				"ORS", "TRB", "TRK", "TTA",
				"TTM", "TTE", "TTY", "LES",
				"SWO", "SKJ", "ALB", "YFT",
				"SBF", "BET", "PBF", "BFT",
				"TUN", "TUS"
			};
			Assert.AreEqual(codes, expectedCodes);
		}

		[Test]
		public void ParseXLSShouldReturnCusCodeLists()
		{
			var xlsxPath = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\Download290949ab-ebde-2fb5-4678-20c2ca670eaa\XlsxSnippet.xlsx");
			var mandatoryDataCodes = new List<string>()
			{
				"DEU",
				"BSJ"
			};
			var cusCodeLists = FileParser.ParseXLS(xlsxPath, mandatoryDataCodes, new DateTime(2017, 2, 1));

			var expectedCusCodeLists = new string[][]
			{
				new string[] { "YCL", "Cycleptus elongatus/Blue sucker" },
				new string[] { "DEU", "Deltistes luxatus/Lost River sucker" },
				new string[] { "BUN", "Barbus longiceps" },
				new string[] { "BSJ", "Barbus plebejus" }
			};

			Assert.AreEqual(cusCodeLists
				.Select(l => new [] { l.ZZD_Code, l.ZZD_Description }),
				expectedCusCodeLists);

			Assert.AreEqual(cusCodeLists
				.Where(l => l.RefCusCodeListAttributes != null)
				.Select(l => l.ZZD_Code),
				mandatoryDataCodes);
		}
	}
}
