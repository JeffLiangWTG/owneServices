using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ImportTradeControlOrdianceAppendixRefCusCodeListParserTest
	{
		[Test]
		public void TestParse()
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TestFiles\boukan-i.csv");
			var parser = new ImportTradeControlOrdinanceAppendixRefCusCodeListParser();

			var isRead = CsvReaderHelper.TryRead(filePath, out var records);
			Assert.That(isRead, Is.True);

			var isParsed = parser.TryParse(records, out var actuals);
			Assert.That(isParsed, Is.True);

			List<RefCusCodeList> expecteds = new List<RefCusCodeList>()
			{
				new RefCusCodeList()
				{
					ZZD_Code = "1010",
					ZZD_Description = "1- 1",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1020",
					ZZD_Description = "1- 2",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1030",
					ZZD_Description = "1- 3",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1040",
					ZZD_Description = "1- 4",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1050",
					ZZD_Description = "1- 5",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1060",
					ZZD_Description = "1- 6",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1070",
					ZZD_Description = "1- 7",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1080",
					ZZD_Description = "1- 8",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1090",
					ZZD_Description = "1- 9",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1100",
					ZZD_Description = "1-10",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1110",
					ZZD_Description = "1-11",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1112",
					ZZD_Description = "1-11の2",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1120",
					ZZD_Description = "1-12",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1130",
					ZZD_Description = "1-13",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1140",
					ZZD_Description = "1-14",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1142",
					ZZD_Description = "1-14の2",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1143",
					ZZD_Description = "1-14の3",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1150",
					ZZD_Description = "1-15",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1160",
					ZZD_Description = "1-16",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1170",
					ZZD_Description = "1-17",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1172",
					ZZD_Description = "1-17の2",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1180",
					ZZD_Description = "1-18",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1190",
					ZZD_Description = "1-19",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1192",
					ZZD_Description = "1-19の2",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1200",
					ZZD_Description = "1-20",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1210",
					ZZD_Description = "1-21",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "1220",
					ZZD_Description = "1-22",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				},
				new RefCusCodeList()
				{
					ZZD_Code = "2000",
					ZZD_Description = "2",
					ZZD_ZZK_NKCodeType = "ITCOA",
					ZZD_ZZZ_NKDataGrouping = "JP",
					ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00)
				}
			};

			for (int i = 0; i < actuals.Count; i++)
			{
				var actual = actuals[i];
				var expected = expecteds[i];
				Assert.That(actual.ZZD_Code, Is.EqualTo(expected.ZZD_Code));
				Assert.That(actual.ZZD_Description, Is.EqualTo(expected.ZZD_Description));
				Assert.That(actual.ZZD_StartDate, Is.EqualTo(expected.ZZD_StartDate));
				Assert.That(actual.ZZD_EndDate, Is.EqualTo(expected.ZZD_EndDate));
				Assert.That(actual.ZZD_ZZK_NKCodeType, Is.EqualTo(expected.ZZD_ZZK_NKCodeType));
				Assert.That(actual.ZZD_ZZZ_NKDataGrouping, Is.EqualTo(expected.ZZD_ZZZ_NKDataGrouping));
			}
		}
	}
}
