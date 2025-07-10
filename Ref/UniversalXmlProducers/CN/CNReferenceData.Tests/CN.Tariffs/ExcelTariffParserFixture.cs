using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class ExcelTariffParserFixture : TestBase
	{
		[Test]
		public void TestInputToOutput()
		{
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 8, 20);

			GlobalOption.Instance.CreateDisposableLog("CN Tariff From Excel Program");

			using (var reader = new TariffDataExcelReaderForTesting(null))
			{
				reader.ReadFromExcels();

				var tariffWriter = new CNRefCusTariffUniversalXMLWriter(GlobalOption.Instance.FirstDayOfThisMonth);
				tariffWriter.Write(reader.OutputRefCusTariffs);

				var codeWriter = new CNRefCusCodeListUniversalXMLWriter(GlobalOption.Instance.FirstDayOfThisMonth, reader.AdditionalElementHelper);
				codeWriter.Write();

				TestHelper.AssertXmlFileContentEquals(Constants.FileNames.Xml_CN_RefCusTariff.ToExpectedResourceFullPath(), Constants.FileNames.Xml_CN_RefCusTariff);
				TestHelper.AssertXmlFileContentEquals(Constants.FileNames.Xml_CN_RefCusCodeList.ToExpectedResourceFullPath(), Constants.FileNames.Xml_CN_RefCusCodeList);
			}
		}

		class TariffDataExcelReaderForTesting : TariffDataExcelReader, IDisposable
		{
			public TariffDataExcelReaderForTesting(DateTime? effectiveDate) : base(effectiveDate)
			{
			}

			protected override XlsFile GetXlsFile(ExcelReadConfiguration readConfig)
			{
				using (var stream = TestHelper.GetManifestResourceStream(resources[readConfig].ToInputResourceFullPath()))
				{
					streams.Add(stream);
					return new XlsFile(stream, false);
				}
			}

			readonly List<Stream> streams = new List<Stream>();

			public void Dispose() => streams.ForEach(stream => stream.Dispose());

			static readonly Dictionary<ExcelReadConfiguration, string> resources = new Dictionary<ExcelReadConfiguration, string>()
			{
				{ ExcelReadConfiguration.Tariff,          "（慧咨）01-2020税则主表.xlsx" },
				{ ExcelReadConfiguration.CIQ,             "（慧咨）03-2020 CIQ.xlsx"},
				{ ExcelReadConfiguration.ExciseRate,      "（慧咨）04-2020消费税率表.xlsx"},
				{ ExcelReadConfiguration.UsaAddRate,      "（慧咨）06-2020对美加征关税.xlsx"},
				{ ExcelReadConfiguration.DutyRate,        "（慧咨）07-2020协定特惠税率表.xlsx"},
				{ ExcelReadConfiguration.ExportDutyRate,  "（慧咨）08-2020出口商品税率表.xlsx"},
			};
		}
	}
}
