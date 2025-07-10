using System;
using System.Linq;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class JsonTariffParserFixture : TestBase
	{
		[Test]
		public void TestParse_20200725_01()
		{
			TestParse("20200725_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ..."
				},
				"20200725_01_CNRefCusTariff.xml", "20200725_01_CNRefCusCodeList.xml");
		}

		[Test]
		public void TestParse_20201201_04()
		{
			TestParse("20201201_04_Response.json", new[]
			{
				"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 3002120091, Created at 2020-12-01, From 2020-07-25 to 2020-11-29 ...",
				"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 3002120092, Created at 2020-12-01, From 2020-07-25 to 2020-11-29 ...",
			}, "20201201_04_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20201201_09()
		{
			TestParse("20201201_09_Response.json", new[]
				{
					"[20-09-21 00:00:00 Warn]:Multiply effective data, picking the last one: HSData for 7404000010, Created at 2020-11-05, From 2020-11-05",
					"[20-09-21 00:00:00 Warn]:                         Other effective data: HSData for 7404000010, Created at 2020-11-05, From 2020-07-25 to 2020-11-05",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 7404000010, Created at 2020-11-05, From 2020-11-05 ..."
				},
				"20201201_09_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20210311_01()
		{
			TestParse("20210311_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1515300000, Created at 2021-01-07, From 2021-01-07 ..."
				},
				"20210311_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20210916_01()
		{
			TestParse("20210916_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Debug]:Tariff 3907209010. Use HS_BOOK 3907209010 instead of HS_CODE ",
					"[20-09-21 00:00:00 Debug]:Tariff 3907209090. Use HS_BOOK 3907209090 instead of HS_CODE ",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2909440000, Created at 2021-09-16, From 2021-01-07 to 2021-09-11 ...",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2909440010, Created at 2021-09-16, From 2021-09-11 ...",
					"[20-09-21 00:00:00 Info]:Tariff 2909440010 has no VAT Data, use VAT Data of 29094400",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2909440090, Created at 2021-09-16, From 2021-09-11 ...",
					"[20-09-21 00:00:00 Info]:Tariff 2909440090 has no VAT Data, use VAT Data of 29094400",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2909499000, Created at 2021-09-16, From 2021-01-07 to 2021-09-11 ...",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2909499010, Created at 2021-09-16, From 2021-09-11 ...",
					"[20-09-21 00:00:00 Info]:Tariff 2909499010 has no VAT Data, use VAT Data of 29094990",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2909499090, Created at 2021-09-16, From 2021-09-11 ...",
					"[20-09-21 00:00:00 Info]:Tariff 2909499090 has no VAT Data, use VAT Data of 29094990",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 3907209010, Created at 2021-09-13, From 2021-09-07 ...",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 3907209090, Created at 2021-09-13, From 2021-09-07 ...",
				},
				"20210916_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20211215_01()
		{
			TestParse("20211215_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 6101901010, Created at 2021-12-15, From 2021-12-15 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 6101901010: 0.912元/升 => 0.912 * [095]"
				},
				"20211215_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20220317_01()
		{
			TestParse("20220317_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 8106909010, Created at 2022-01-25, From 2022-01-25 ..."
				},
				"20220317_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20220907_01_Add_RefCusTariffAttribute_SupportsTSD()
		{
			TestParse("20220907_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 8111001010, Created at 2022-01-25, From 2022-01-25 ..."
				},
				"20220907_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20230908_01_FTA_NICARAGUA()
		{
			TestParse("20230908_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 8501320090, Created at 2023-09-01, From 2023-09-01 ..."
				},
				"20230908_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20240430_01_FTA_ECUADOR()
		{
			TestParse("20240430_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2519909100, Created at 2024-01-16, From 2024-01-01 ..."
				},
				"20240430_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20240129_01()
		{
			TestParse("20240129_01_Response.json", new[]
			{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701120001, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701120001: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701130001, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701130001: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701130001: 关税配 额税 率：15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701140001, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701140001: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701140001: 关税配 额税 率：15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701910001, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701910001: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701991010, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701991010: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701991010: 关税配 额税 率：15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701992001, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701992001: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701992001: 关税配 额税 率：15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 1701999001, Created at 2024-01-16, From 2024-01-01 ...",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701999001: 国别关 税配额 税率： 15 => VFD * 0.15",
					"[20-09-21 00:00:00 Debug]:Special Rate for 1701999001: 关税配 额税 率：15 => VFD * 0.15",
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 9001909070, Created at 2024-01-16, From 2024-01-01 ...",
				},
				"20240129_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20240701_01_FTA_SERBIA()
		{
			TestParse("20240701_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 0302920010, Created at 2024-01-16, From 2024-01-01 ..."
				},
				"20240701_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20240918_01_FTA_HONDURAS()
		{
			TestParse("20240918_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 2825800010, Created at 2024-09-18, From 2024-09-18 ..."
				},
				"20240918_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20250117_01_FTA_MALDIVES()
		{
			TestParse("20250117_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 9705100010, Created at 2025-01-17, From 2025-01-01 ..."
				},
				"20250117_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20241205_01_Response_SP_LDC()
		{
			TestParse("20241205_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 0101210010, Created at 2024-01-16, From 2024-01-01 ..."
				},
				"20241205_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_20250410_01_Response_AU_LIST()
		{
			TestParse("20250410_01_Response.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 0101309010, Created at 2025-01-17, From 2025-01-01 ..."
				},
				"20250410_01_CNRefCusTariff.xml");
		}

		[Test]
		public void TestParse_RessponseWithNullValues()
		{
			var exception = Assert.Throws<InvalidOperationException>(() => TestParse("Response_NullValues.json", new[]
			{
				"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...",
				"[20-09-21 00:00:00 Error]:Tariff 9001100001 has no effective VAT Data",
				"[20-09-21 00:00:00 Error]:Tariff 9001100001 has no effective MFN Data",
				"[20-09-21 00:00:00 Error]:Tariff 9001100001 has no effective GEN Data",
				"[20-09-21 00:00:00 Warn]:Tariff 9001100001 has no effective PT Data",
				"[20-09-21 00:00:00 Warn]:Tariff 9001100001 has no effective YS Data"
			},
				"CNRefCusTariff_NullValues.xml"));

			Assert.AreEqual("Errors occurred while running CN Tariff Update Program.", exception.Message);
			Assert.AreEqual(@"[20-09-21 00:00:00 Error]:Tariff 9001100001 has no effective VAT Data", exception.InnerException.Message);
		}

		[Test]
		public void TestParse_PTListAllDeleted()
		{
			TestParse("20240705_PTListAllDeleted.json", new[]
				{
					"[20-09-21 00:00:00 Info]:Populating XML for tariff HSData for 5402491000, Created at 2024-01-16, From 2024-01-01 ..."
				},
				"20240705_01_CNRefCusTariff_PTListAllDeleted.xml",
				message: "This test targets the case that all items in PT_LIST are deleted(\"VPT_OPT\": \"d\"). In that case, we need to create the LCD Rates to let RefDataRepo to expire the LCD RefCusRates.");
		}

		void TestParse(string inputJsonFile, string[] logs, string expectedTariffXml = null, string expectedCodeListXml = null, string message = null)
		{
			var now = new DateTime(2020, 9, 21);
			GlobalOption.Instance.NowGetter = () => now;
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			using (GlobalOption.Instance.CreateDisposableLog("CN Tariff Update Program"))
			{
				var pageContent = TestHelper.ReadManifestResourceContent(inputJsonFile.ToInputResourceFullPath());
				var updateResponse = JsonConvert.DeserializeObject<GetUpdatesResponse>(pageContent);
				var parser = new JsonTariffParser(updateResponse.RESULT_DATA_LIST.HS_TAX.ToArray());
				var result = parser.GetTariffList().ToList();

				if (logs != null)
				{
					Assert.AreEqual(string.Join(Environment.NewLine, logs), GlobalOption.Instance.Log.All);
				}

				if (!string.IsNullOrEmpty(expectedTariffXml))
				{
					var tempTariffXmlFileName = TestHelper.CreateTempXmlFileName();

					var tariffWriter = new CNRefCusTariffUniversalXMLWriter(now, true);
					tariffWriter.Write(result, tempTariffXmlFileName);

					TestHelper.AssertXmlFileContentEquals(expectedTariffXml.ToExpectedResourceFullPath(), tempTariffXmlFileName, message);
				}

				if (!string.IsNullOrEmpty(expectedCodeListXml))
				{
					var tempCodeListXmlFileName = TestHelper.CreateTempXmlFileName();

					var codeWriter = new CNRefCusCodeListUniversalXMLWriter(now, parser.AdditionalElementHelper);
					codeWriter.Write(tempCodeListXmlFileName);

					TestHelper.AssertXmlFileContentEquals(expectedCodeListXml.ToExpectedResourceFullPath(), tempCodeListXmlFileName);
				}
			}
		}
	}
}
