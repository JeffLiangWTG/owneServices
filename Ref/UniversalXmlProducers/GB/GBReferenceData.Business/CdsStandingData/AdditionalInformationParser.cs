using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData
{
	public class AdditionalInformationParser : CommonCodeParser
	{
		readonly IWebpageTableToDataTable webpageTableToDataTable;
		DateTime PublishDate = DateTime.MinValue;

		public AdditionalInformationParser(string html, IWebClientWrapper webWrapper) : base(webWrapper)
		{
			xmlWriter = CreateXmlWriter();
			webpageTableToDataTable = new CDSWebpageTableToDataTable(html);
		}

		protected override string CodeType => "ADDIN";
		protected override DateTime GetPublishDate() => PublishDate;

		protected override void ProduceData()
		{
			var additionalInformation = GetAdditionalData();

			foreach (DataRow row in additionalInformation.Rows)
			{
				PopulateCodeListFromTableRow(row);
			}
		}

		void PopulateCodeListFromTableRow(DataRow row)
		{
			var code = row[0]?.ToString().ToUpper(CultureInfo.CurrentCulture) ?? string.Empty;
			var description = (row[1]?.ToString() ?? string.Empty)
					+ " "
					+ (row[2]?.ToString() ?? string.Empty);
			var direction = row[3]?.ToString().ToUpper(CultureInfo.CurrentCulture).Trim();

			description = ConvertLineFeeds(description);

			if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(description))
			{
				if (code.Contains("-"))
				{
					HandleRangedCodes(code, description, direction);
					return;
				}

				var codelist = new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_Description = TruncateToMaximumLength(description)
				};

				var codelistAttributes = GetDirectionRefCusCodeListAttributes(direction).ToList();
				codelistAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "LEVEL", ZZE_Value = GetLevel(code, description) });

				codelist.RefCusCodeListAttributes = codelistAttributes.ToArray();
				xmlWriter.PopulateData(codelist);
			}
		}

		protected static string GetLevel(string code, string description)
		{
			var level = "ITEM";

			if (code == "RRS01" || description.ToUpper(CultureInfo.CurrentCulture).Contains("NOTE: THIS MUST BE ENTERED AT HEADER LEVEL"))
			{
				level = "HEADER";
			}

			return level;
		}

		void HandleRangedCodes(string code, string description, string direction)
		{
			var codelists = new List<RefCusCodeList>();

			if (code.Contains('-'))
			{
				if (Regex.IsMatch(code, @"(BR|Ag)nnn \([0-9]{3}-[0-9]{3}\)"))
				{
					return;
				}

				if (Regex.IsMatch(code, @"POD[0-9]* - POD[0-9]*"))
				{
					var range = Regex.Matches(code, @"[0-9][0-9]*");
					if (range.Count == 2)
					{
						for (int i = Convert.ToInt16(range[0].Value, CultureInfo.CurrentCulture); i <= Convert.ToInt16(range[1].Value, CultureInfo.CurrentCulture); i++)
						{
							var zZD_Code = "POD" + i.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0');
							codelists.Add(
								new RefCusCodeList()
								{
									ZZD_Code = zZD_Code,
									ZZD_Description = $"Authorisation by customs declaration only: Throughout period for the customs procedure. A standard throughout period of up to {i} months is permitted. If the processing period is likely to exceed this period, the applicant must inform NIRU and ask for an extension. {zZD_Code} may only be used where AI code 00100 is also declared in DE 2/2."
								});
						}
					}
				}

				if (Regex.IsMatch(code, @"PRO11 - 13"))
				{
					var textToReplace = @"For PRO11, ‘civil aircraft’. For PRO12, ‘aircraft engines’. For PRO13, ‘any other goods imported occasionally’.";
					codelists.Add(new RefCusCodeList()
					{
						ZZD_Code = "PRO11",
						ZZD_Description = TruncateToMaximumLength(description.Replace(textToReplace, @"Authorisation by customs declaration only: end use relief, civil aircraft"))
					});
					codelists.Add(new RefCusCodeList()
					{
						ZZD_Code = "PRO12",
						ZZD_Description = TruncateToMaximumLength(description.Replace(textToReplace, @"Authorisation by customs declaration only: end use relief, aircraft engines"))
					});
					codelists.Add(new RefCusCodeList()
					{
						ZZD_Code = "PRO13",
						ZZD_Description = TruncateToMaximumLength(description.Replace(textToReplace, @"Authorisation by customs declaration only: end use relief, any other goods imported occasionally"))
					});
				}
			}
			else
			{
				codelists.Add(
					new RefCusCodeList()
					{
						ZZD_Code = code,
						ZZD_Description = TruncateToMaximumLength(description)
					});
			}

			foreach (var codelist in codelists)
			{
				var codelistAttributes = GetDirectionRefCusCodeListAttributes(direction).ToList();
				codelistAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "LEVEL", ZZE_Value = "ITEM" });

				codelist.RefCusCodeListAttributes = codelistAttributes.ToArray();
				xmlWriter.PopulateData(codelist);
			}
		}

		static string TruncateToMaximumLength(string text) => text.Length <= DescriptionMaxLength ? text : string.Concat(text.AsSpan(0, DescriptionMaxLength - 1), "…");
		const int DescriptionMaxLength = 2000;

		List<RefCusCodeListAttribute> GetDirectionRefCusCodeListAttributes(string direction)
		{
			var result = new List<RefCusCodeListAttribute>();

			if (importDirectionList.Contains(direction))
			{
				result.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Direction", ZZE_Value = "IMPORT" });
			}

			if (exportDirectionList.Contains(direction))
			{
				result.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Direction", ZZE_Value = "EXPORT" });
			}

			return result;
		}

		protected DataTable GetAdditionalData()
		{
			var unionData = GetDatedByteArrayFromLink(webpageTableToDataTable, UnionDataAnchorText);
			var nationalData = GetDatedByteArrayFromLink(webpageTableToDataTable, NationalDataAnchorText);

			SetPublishDate(unionData.LastModified, nationalData.LastModified);

			var unionDocuments = ODTFileHelper.GetTableFromCellContent(unionData.Content, DocumentDataTag);
			var nationalDocuments = ODTFileHelper.GetTableFromCellContent(nationalData.Content, DocumentDataTag);

			var unionColumns = new string[] { "AI Statement Code to be declared", "Description and Usage of Code", "Details to be entered on the declaration or clearance request", "Import/Export/Both" };
			var nationalColumns = new string[] { "AI Statement Code to be declared", "Description and Usage of Code", "Details to be entered on the declaration or clearance request", "Import/ Export/ Both" };
			var newColumns = new string[] { "Code", "Description", "Details", "I/E/B" };

			if (AdjustColumnsForMerge(unionDocuments, unionColumns, newColumns) &&
				AdjustColumnsForMerge(nationalDocuments, nationalColumns, newColumns))
			{
				unionDocuments.Merge(nationalDocuments);
			}
			else
			{
				unionDocuments.Rows.Clear();
				throw new NotSupportedException("Unexpected columns, The data that was downloaded for additional information contains unexpected columns.");
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
				PublishDate = ScrapePublishDate(webpageTableToDataTable, new DateTime(1900, 01, 01, 00, 00, 00));
			}
		}

		readonly IEnumerable<string> importDirectionList = new string[] { "I", "IMPORT", "IMP", "B", "BOTH", "BTH" };
		readonly IEnumerable<string> exportDirectionList = new string[] { "E", "EXPORT", "EXP", "B", "BOTH", "BTH" };

		const string UnionDataAnchorText = "Appendix 4A: Union Codes which may be declared on customs declarations or customs clearance requests";
		const string NationalDataAnchorText = "Appendix 4B: National Codes which may be declared on customs declarations or clearance requests";
		const string DocumentDataTag = "AI Statement Code to be declared";
	}
}
