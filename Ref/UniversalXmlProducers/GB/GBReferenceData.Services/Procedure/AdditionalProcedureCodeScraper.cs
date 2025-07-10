using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Services;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure
{
	internal class AdditionalProcedureCodeScraper
	{
		const string HeaderXPath = "//div[@class='govspeak']/table[thead][1]/thead[1]/tr";
		const string DataXPath = "//div[@class='govspeak']/table[thead]/tbody[1]/tr";
		static readonly string[] yesValues = new string[] { "Y", "Yes" };
		readonly IWebClientWrapper webClientWrapper;

		public AdditionalProcedureCodeScraper(IWebClientWrapper webWrapper)
		{
			webClientWrapper = webWrapper;
		}

		internal IReadOnlyCollection<AdditionalProcedureCodeRaw> ExtractCodesWithDescription(string url)
		{
			var converter = new CDSWebpageTableToDataTable(webClientWrapper.GetContent(url));
			using (var dt = converter.ExtractDatatableFromGrid(HeaderXPath, DataXPath))
			{
				if (dt.Columns.Count == 2)
				{
					return dt.Rows.Cast<DataRow>()
						.Select(row => new AdditionalProcedureCodeRaw()
						{
							Code = row.Field<string>(0),
							Description = row.Field<string>(1)
						})
						.ToList()
						.AsReadOnly();
				}
				else
				{
					return new List<AdditionalProcedureCodeRaw>().AsReadOnly();
				}
			}
		}

		public static DataTable GetDocumentData(byte[] rawData) => ODTFileHelper.GetTableFromCellContent(rawData, "DE 1/10\nDE 1/11", "DE\n1/10", "Correlation matrix");

		public static IEnumerable<AdditionalProcedureMapping> GetMappings(DataTable data)
		{
			var result = new List<AdditionalProcedureMapping>();
			var columns = data.Columns.Cast<DataColumn>().Skip(1).ToList();
			var rows = data.Rows.Cast<DataRow>().ToList();
			foreach (var col in columns)
			{
				var procedureCode = col.ColumnName.Replace(" ", "").Right(4);
				var mappedCodes = rows.Where(r => yesValues.Contains(r.Field<string>(col.Ordinal).Replace(" ", "")))
					.Select(r => r.Field<string>(0).Replace(" ", "").Right(3))
					.ToList();
				result.Add(new AdditionalProcedureMapping
				{
					ProcedureCode = procedureCode,
					AdditionalProcedureCodes = mappedCodes
				});
			}
			return result;
		}

		public byte[] GetContent(string url)
		{
			var page = new CDSWebpageTableToDataTable(webClientWrapper.GetContent(url));
			var urlFromAnchor = page.ExtractSingleUrlFromFileExtension(".ods");
			return urlFromAnchor == null ? new byte[0] : webClientWrapper.GetContentAsByteArray(urlFromAnchor.OriginalString);
		}
	}
}
