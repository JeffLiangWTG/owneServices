using System;
using System.Data;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData
{
	public class ErrorCodeParser : CommonCodeParser
	{
		readonly IWebpageTableToDataTable webpageTableToDataTable;
		DateTime PublishDate = DateTime.UtcNow;

		public ErrorCodeParser(IWebClientWrapper webWrapper, string errorCodePageHtml) : base(webWrapper)
		{
			xmlWriter = CreateXmlWriter();
			webpageTableToDataTable = new CDSWebpageTableToDataTable(errorCodePageHtml);
		}

		protected override string CodeType => "ERRCD";
		protected override DateTime GetPublishDate() => PublishDate;

		protected override void ProduceData()
		{
			var errorCodes = GetData();

			foreach (DataRow row in errorCodes.Rows)
			{
				PopulateCodeListFromTableRow(row);
			}
		}

		void PopulateCodeListFromTableRow(DataRow row)
		{
			var code = row[0]?.ToString().ToUpper(CultureInfo.CurrentCulture) ?? string.Empty;
			var description = FormatDescription(row[1]?.ToString(), row[2]?.ToString());

			if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(description))
			{
				xmlWriter.PopulateData(new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_Description = description
				});
			}
		}

		protected DataTable GetData()
		{
			var (lastModified, content) = GetDatedByteArrayFromLink(webpageTableToDataTable, ConfigurationProvider.CDSErrorCodeAnchorText);
			PublishDate = lastModified;
			var errorCodes = ODTFileHelper.GetTableFromCellContent(content, ErrorCodeDataTag);
			var errorColumns = new string[] { "CDS Error Code", "CDS Error Code Description", "CDS Error Code Explanation" };
			var newColumns = new string[] { "Code", "Description", "Explanation" };

			if (!AdjustColumnsForMerge(errorCodes, errorColumns, newColumns))
			{
				errorCodes.Rows.Clear();
				throw new NotSupportedException("Unexpected columns, The data that was downloaded for CDS Error codes contains unexpected columns.");
			}

			return errorCodes;
		}

		protected static string FormatDescription(string description, string explanation)
		{
			var result = $"{explanation?.Trim()} {description?.Trim()}".Trim();

			result = ConvertLineFeeds(result);

			return result;
		}

		const string ErrorCodeDataTag = "CDS Error Code";
	}
}
