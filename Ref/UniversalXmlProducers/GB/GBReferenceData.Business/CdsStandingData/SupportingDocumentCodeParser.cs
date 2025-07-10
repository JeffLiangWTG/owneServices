using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Business;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData
{
	public class SupportingDocumentCodeParser : CommonCodeParser
	{
		readonly IWebpageTableToDataTable webpageTableToDataTableNational;
		readonly IWebpageTableToDataTable webpageTableToDataTableUnion;
		readonly IWebpageTableToDataTable webpageTableToDataTableStatus;
		DateTime PublishDate = DateTime.MinValue;
		readonly Regex noStatusCodesRegex = new Regex(@"(status code\b).*(\brequired)", RegexOptions.IgnoreCase);
		readonly Regex statementRegex = new Regex(@"(?!^‘)(?<!\w)[‘’""“”'](?<statement>[^\n]{5,100}?)\.?([‘’""“”']| for exemption covering )(?<=^‘?complete .+)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
		readonly Regex statementWithoutQuotesRegex = new Regex(@"^‘?complete statement:?\s+(?<statement>[^‘’""“”'\n]+?)\.?\s+(?-i)Use of this code", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public SupportingDocumentCodeParser(string nationalHtml, string statusHtml, string unionHtml, IWebClientWrapper webWrapper) : base(webWrapper)
		{
			xmlWriter = CreateXmlWriter();
			webpageTableToDataTableNational = new CDSWebpageTableToDataTable(nationalHtml);
			webpageTableToDataTableUnion = new CDSWebpageTableToDataTable(unionHtml);
			webpageTableToDataTableStatus = new CDSWebpageTableToDataTable(statusHtml);
		}

		protected override string CodeType => "DC44";
		protected override IXmlWriter CreateXmlWriter() => new XmlWriter(Helper.GetRefCusCodeListWriterConfigurationWithAttributes("CDS"));
		protected override DateTime GetPublishDate() => PublishDate;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
		protected override void ProduceData()
		{
			var validStatusCodes = GetStatusCodes();
			if (validStatusCodes == null || !validStatusCodes.Any())
			{
				throw new NotSupportedException("No valid codes found on webpage, No valid codes found in html.");
			}

			var allDocuments = GetDocumentData();
			distinctCodeListTobeDeclared = new List<RefCusCodeList>();

			foreach (DataRow row in allDocuments.Rows)
			{
				var code = row[0]?.ToString() ?? string.Empty;
				var dirText = row[1]?.ToString().ToUpper(CultureInfo.CurrentCulture) ?? string.Empty;
				var descrip = row[2]?.ToString() ?? string.Empty;
				var details = row[3]?.ToString() ?? string.Empty;
				var docStatuses = row[4]?.ToString() ?? string.Empty;
				var referenceNumber = row[5]?.ToString() ?? string.Empty;
				var reason = row[6]?.ToString() ?? string.Empty;

				descrip = $"{descrip} {details}" ?? string.Empty;
				descrip = ConvertLineFeeds(descrip);
				if (descrip.Length > 2000)
				{
					descrip = descrip.Substring(0, 2000);
				}

				foreach (var direction in GetDirectionRefCusCodeListAttributes(dirText))
				{
					var codelist = GetRefCusCodeList(code, direction, descrip);
					var codelistAttributes = codelist?.RefCusCodeListAttributes?.ToList() ?? new List<RefCusCodeListAttribute>() { };

					if (!string.IsNullOrEmpty(docStatuses))
					{
						var statusCodes = GetStatusCodesForDoc(docStatuses, validStatusCodes);
						foreach (var statusCode in statusCodes)
						{
							AddRefCusCodeListAttribute(codelistAttributes, "ACTAV", statusCode);
						}
					}
					if (!string.IsNullOrEmpty(details))
					{
						var statements = GetStatementsToComplete(details);
						foreach (var statement in statements)
						{
							AddRefCusCodeListAttribute(codelistAttributes, "StatementText", statement);
						}
					}
					AddRefCusCodeListAttribute(codelistAttributes, "LEVEL", "ITEM");

					if (!string.IsNullOrEmpty(referenceNumber) && referenceNumber.Trim() == "M")
					{
						AddRefCusCodeListAttribute(codelistAttributes, "ReferenceNumber", "Y");
					}
					if (!string.IsNullOrEmpty(reason) && reason.Trim() == "M")
					{
						AddRefCusCodeListAttribute(codelistAttributes, "Reason", "Y");
					}

					codelist.RefCusCodeListAttributes = codelistAttributes.ToArray();
				}
			}
			distinctCodeListTobeDeclared.ForEach(x => xmlWriter.PopulateData(x));
		}

		protected static void AddRefCusCodeListAttribute(List<RefCusCodeListAttribute> codelistAttributes, string zze_ZXE_NKName, string zze_Value)
		{
			if (zze_Value.Length > 255)
			{
				zze_Value = zze_Value.Substring(0, 255);
			}

			var index = codelistAttributes.FindIndex(x => string.Equals(x.ZZE_ZXE_NKName, zze_ZXE_NKName, StringComparison.Ordinal) && string.Equals(x.ZZE_Value, zze_Value, StringComparison.Ordinal));
			if (index != -1)
			{
				codelistAttributes.RemoveAt(index);
				codelistAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = zze_ZXE_NKName, ZZE_Value = zze_Value });
			}
			else
			{
				codelistAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = zze_ZXE_NKName, ZZE_Value = zze_Value });
			}
		}

		RefCusCodeList GetRefCusCodeList(string code, string direction, string description = "")
		{
			var existingRefCusCodeList = distinctCodeListTobeDeclared?.Find(x => string.Equals(x.ZZD_Code, code, StringComparison.Ordinal) && string.Equals(x.ZZD_ZZK_NKCodeType, direction == "IMPORT" ? "DC44I" : "DC44E", StringComparison.Ordinal));
			if (existingRefCusCodeList != null)
			{
				return existingRefCusCodeList;
			}
			else
			{
				existingRefCusCodeList = new RefCusCodeList() { ZZD_Code = code, ZZD_Description = description, ZZD_ZZK_NKCodeType = direction == "IMPORT" ? "DC44I" : "DC44E" };
				distinctCodeListTobeDeclared?.Add(existingRefCusCodeList);
				return existingRefCusCodeList;
			}
		}

		static List<string> GetDirectionRefCusCodeListAttributes(string direction)
		{
			if (string.IsNullOrEmpty(direction))
			{
				direction = "B";
			}
			var result = new List<string>();
			if (direction == "I" || direction == "B" || direction == "IMPORT" || direction == "BOTH")
			{
				result.Add("IMPORT");
			}

			if (direction == "E" || direction == "B" || direction == "EXPORT" || direction == "BOTH")
			{
				result.Add("EXPORT");
			}
			return result;
		}

		public List<string> GetStatusCodesForDoc(string docStatusesStr, List<string> validStatusCodes)
		{
			var statuses = new List<string>();

			if (!noStatusCodesRegex.IsMatch(docStatusesStr))
			{
				var docStatuses = docStatusesStr.Split(new char[] { ',', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
				statuses.AddRange(docStatuses.Intersect(validStatusCodes));
			}

			return statuses;
		}

		public IEnumerable<string> GetStatementsToComplete(string details)
		{
			return statementRegex.Matches(details).Cast<Match>()
				.Concat(statementWithoutQuotesRegex.Matches(details).Cast<Match>())
				.Select(m => m.Groups["statement"].Value.Trim())
				.Distinct()
				.OrderBy(value => value)
				.ToArray();
		}

		protected static DateTime CalculatePublishDate(IWebpageTableToDataTable nationalPage, IWebpageTableToDataTable unionPage)
		{
			var result = new DateTime(1900, 01, 01, 00, 00, 00);

			var nationalDate = ScrapePublishDate(nationalPage, result);
			var unionDate = ScrapePublishDate(unionPage, result);

			result = nationalDate > unionDate ? nationalDate : unionDate;

			return result;
		}

		protected List<string> GetStatusCodes()
		{
			var statusData = GetByteArrayFromLink(webpageTableToDataTableStatus, StatusDataAnchorText);
			var statuses = ODTFileHelper.GetTableFromCellContent(statusData, StatusDataTag);

			return statuses.Rows.Cast<DataRow>().Select(x => x[0].ToString() ?? string.Empty).ToList();
		}

		protected DataTable GetDocumentData()
		{
			var unionData = GetDatedByteArrayFromLink(webpageTableToDataTableUnion, UnionDataAnchorText);
			var nationalData = GetDatedByteArrayFromLink(webpageTableToDataTableNational, NationalDataAnchorText);

			SetPublishDate(unionData.LastModified, nationalData.LastModified);

			var unionDocuments = ODTFileHelper.GetTableFromCellContent(unionData.Content, DocumentDataTag);
			var nationalDocuments = ODTFileHelper.GetTableFromCellContent(nationalData.Content, DocumentDataTag);

			var unionColumns = new string[] { "Document Code to be declared", "Import/ Export/ Both", "Description and Usage of Code", "Details to be entered on the declaration", "Status Codes", "Document ID", "Document reason" };
			var nationalColumns = new string[] { "Document Code to be declared", "Import/ Export/ Both", "Description and Usage of Code", "Details to be entered on the declaration", "Status Code(s)", "Document ID", "Document Reason" };
			var newColumns = new string[] { "Code", "I/E/B", "Description", "Details", "StatusCodes", "ID", "Reason" };

			if (AdjustColumnsForMerge(unionDocuments, unionColumns, newColumns) &&
				AdjustColumnsForMerge(nationalDocuments, nationalColumns, newColumns))
			{
				unionDocuments.Merge(nationalDocuments);
			}
			else
			{
				unionDocuments.Rows.Clear();
				throw new NotSupportedException("Unexpected columns, The data that was downloaded for supporting documents contains unexpected columns.");
			}

			return unionDocuments;
		}

		protected void SetPublishDate(DateTime unionDate, DateTime nationalDate)
		{
			PublishDate = unionDate;

			if (PublishDate < nationalDate)
			{
				PublishDate = nationalDate;
			}

			if (PublishDate == DateTime.MinValue)
			{
				PublishDate = CalculatePublishDate(webpageTableToDataTableNational, webpageTableToDataTableUnion);
			}
		}

		const string UnionDataAnchorText = "Data Element 2/3 Documents and Other Reference Codes (Union) (Appendix 5A)";
		const string NationalDataAnchorText = "Appendix 5A: DE 2/3: National Document Codes";
		const string StatusDataAnchorText = "Appendix 5B: DE 2/3: Document Status Codes";
		const string DocumentDataTag = "Document Code to be declared";
		const string StatusDataTag = "Status";
		List<RefCusCodeList> distinctCodeListTobeDeclared;
	}
}
