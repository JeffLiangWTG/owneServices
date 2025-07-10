using System.Data;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData
{
	public class CustomsOfficeGenericSublocationCodeParser : IFacilityDataParser
	{
		readonly WebpageTableToDataTable _webpageTableToDataTable;
		readonly string _exportFilePath;
		readonly IXmlWriter _xmlWriter;
		const string _publishDateXpath = @"//*[@id=""wb-dtmd""]";
		const string _headerXpath = "//html[1]//body[1]//main[1]//table[1]//thead[1]//tr";
		const string _dataXpath = "//html[1]//body[1]//main[1]//table[1]//tbody[1]//tr";

		public CustomsOfficeGenericSublocationCodeParser(WebpageTableToDataTable webpageTableToDataTable, string exportFilePath)
		{
			_webpageTableToDataTable = webpageTableToDataTable;
			_exportFilePath = exportFilePath;
			_xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusCodelistConfiguration(Constants.CodeType.SUBLC));
		}

		public void ExportXml()
		{
			_xmlWriter.SetPublicationTime(_webpageTableToDataTable.ExtractPublishDate(_publishDateXpath));
			_xmlWriter.SetUpdateType(UpdateType.Full);
			_xmlWriter.SetDataSource(Constants.DataSource.StandingData + " CusOffice Generic");
			ProduceData();
			_xmlWriter.SaveXml(_exportFilePath);
		}

		DataTable ScrapeWebPage()
		{
			return _webpageTableToDataTable.ExtractDatatableFromGrid(_headerXpath, _dataXpath);
		}

		void ProduceData()
		{
			using (var dataTable = ScrapeWebPage())
			{
				foreach (DataRow row in dataTable.Rows)
				{
					PopulateCodeListFromTableRow(row);
				}
			}

		}

		void PopulateCodeListFromTableRow(DataRow row)
		{
			var codelist = new RefCusCodeList()
			{
				ZZD_Code = row["Sublocation code"]?.ToString() ?? string.Empty,
				ZZD_Description = "CUSTOMS GENERIC - " + (row["Location"]?.ToString() ?? string.Empty)
			};

			codelist.RefCusCodeListAttributes = new[]
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "PORT", ZZE_Value = row["Port"]?.ToString() ?? string.Empty },
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Province", ZZE_Value = CaProvinceLookup.GetProvince(row["Province"]?.ToString() ?? string.Empty) }
			};

			_xmlWriter.PopulateData(codelist);
		}
	}
}
