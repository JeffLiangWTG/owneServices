using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData
{
	public class SufferanceWarehouseOperatorAndSublocationCodeParser : IFacilityDataParser
	{
		readonly WebpageTableToDataTable _webpageTableToDataTable;
		readonly string _exportFilePath;
		readonly IXmlWriter _xmlWriter;
		const string _publishDateXpath = @"//*[@id=""wb-dtmd""]";
		const string _headerXpath = "//html[1]//body[1]//main[1]//table[1]//thead[1]//tr";
		const string _dataXpath = "//html[1]//body[1]//main[1]//table[1]//tbody[1]//tr";

		public SufferanceWarehouseOperatorAndSublocationCodeParser(WebpageTableToDataTable webpageTableToDataTable, string exportFilePath)
		{
			_webpageTableToDataTable = webpageTableToDataTable;
			_exportFilePath = exportFilePath;
			_xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusCodelistConfiguration(Constants.CodeType.SUBLC));
		}

		public void ExportXml()
		{
			_xmlWriter.SetPublicationTime(_webpageTableToDataTable.ExtractPublishDate(_publishDateXpath));
			_xmlWriter.SetUpdateType(UpdateType.Full);
			_xmlWriter.SetDataSource(Constants.DataSource.StandingData + " Sufferance");
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
			var description = row["Warehouse"]?.ToString().Split(new string[] { "\r\n", "\n", Environment.NewLine }, System.StringSplitOptions.None).Select(x => CleanTags(x)).ToArray();
			var codelist = new RefCusCodeList()
			{
				ZZD_Code = row["Code"]?.ToString() ?? string.Empty,
				ZZD_Description = string.Join(" ",description.Where(x => !string.IsNullOrWhiteSpace(x)))
			};

			var codeListAttributes = new List<RefCusCodeListAttribute>();

			codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "PORT", ZZE_Value = row["Port relation"]?.ToString() ?? string.Empty });
			codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Province", ZZE_Value = CaProvinceLookup.GetProvince(row["Province"]?.ToString() ?? string.Empty) });
			codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Type", ZZE_Value = row["Type"]?.ToString() ?? string.Empty });
			GetAddress(row["Address"]?.ToString(),ref codeListAttributes);
			codelist.RefCusCodeListAttributes = codeListAttributes.ToArray();
			_xmlWriter.PopulateData(codelist);
		}

		static string CleanTags(string value)
		{
			return HttpUtility.HtmlDecode(Regex.Replace(value, @"<[^>]*>", string.Empty));
		}

		static void GetAddress(string address, ref List<RefCusCodeListAttribute> codeListAttributes)
		{
			var postCode = GetPostCode(address);
			if (postCode != null)
			{
				address = address.Replace(postCode, string.Empty);
				codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "PostCode", ZZE_Value = GetFormattedPostCode(postCode) });
			}
			var addressLines = address.Split(new string[] { "\r\n", "\n", Environment.NewLine }, System.StringSplitOptions.None).Select(x => CleanTags(x)).ToArray();
			if (addressLines.Length == 1)
			{
				var streetExtractRegex = @"^((.)*\s([Pp]{1}lace|[Aa]{1}venue|[Ss]{1}treet|[Ss]{1}t|[Rr]{1}oad|[Rr]{1}d|[Ww]{1}ay|[Dd]{1}rive)[ .,]*([NSEW.,]{2,3})?)|((.)*[NSEW.,]{2,3})|.*[,]";
				var street = string.Empty;
				foreach (Match match in Regex.Matches(addressLines[0], streetExtractRegex))
				{
					street += match.Value;
				}
				if (!string.IsNullOrWhiteSpace(street))
				{
					var city = addressLines[0].Replace(street, string.Empty).Trim();
					codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Street", ZZE_Value = street.Trim() });
					if (!string.IsNullOrEmpty(city))
					{
						codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "CITY", ZZE_Value = city });
					}
				}
				else
				{
					var splittedAddressLines = addressLines[0].Trim().Split(' ');
					var city = splittedAddressLines.Last();
					if (splittedAddressLines.Length > 2)
					{
						codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "CITY", ZZE_Value = city });
						codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Street", ZZE_Value = addressLines[0].Replace(city, string.Empty).Trim() });
					}
					else
					{
						codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Street", ZZE_Value = addressLines[0].Trim() });
					}
				}
			}
			else
			{
				var cityFound = false;
				var street = string.Empty;
				for (int i = addressLines.Length - 1; i >= 0; i--)
				{
					if (!string.IsNullOrWhiteSpace(addressLines[i]))
					{
						if (!cityFound && i > 0)
						{
							codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "CITY", ZZE_Value = addressLines[i].Trim() });
							cityFound = true;
							continue;
						}
						street = addressLines[i].Trim() + " " + street;
					}
				}
				if (!string.IsNullOrEmpty(street))
				{
					codeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Street", ZZE_Value = street.Trim() });
				}
			}
		}

		static string GetPostCode(string address)
		{
			var match = Regex.Match(address, "[A-Za-z]{1}[0-9]{1}[A-Za-z]{1}((&nbsp;)|( )|( ))?[0-9]{1}[A-Za-z]{1}[0-9]{1}");
			if (match.Success)
			{
				return match.Value;
			}
			return null;
		}

		static string GetFormattedPostCode(string postCode)
		{
			return $"{postCode.Substring(0, 3)} {postCode.Substring(postCode.Length - 3, 3)}";
		}
	}
}
