using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class TariffAgreementsCodeListParser : BaseRefCusCodeListParser<(Stream, string)>
	{
		public TariffAgreementsCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.TariffAgreementsCodes.Code;
		protected override bool HasAttributes => true;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists((Stream, string) dataSource)
		{
			var stream = dataSource.Item1;
			Contract.Assume(stream != null);

			var agreements = GetLaiaAgreementDTOs(dataSource.Item2);

			var xls = new XlsFile(stream, true);

			Contract.Assume(xls != null);

			var result = new List<RefCusCodeList>();

			var agreementsXlsValues = GetAgreementTableValues(xls).ToLookup(x => x.LegalActInImportEntry);

			foreach (var agreement in agreements)
			{
				if (!agreementsXlsValues.Contains(agreement.LegalAct))
				{
					throw new InvalidOperationException($"Legal Act {agreement.LegalAct} does not exists in Agreements.xlsx");
				}

				var agreementsXlsValue = agreementsXlsValues[agreement.LegalAct].FirstOrDefault();
				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = agreementsXlsValue.Code,
					ZZD_Description = agreement.Name,
					RefCusCodeListAttributes = GetRefCusCodeListAttributeValue(agreement).ToArray()
				};

				result.Add(refCusCodeList);
			}

			return result;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.TariffAgreementsCodes.Description,
			};
		}

		protected override IEnumerable<RefCusCodeListAttributeName> GetRefCusCodeListAttributeNames()
		{
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.Country.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.Country.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.Country.Description
			};
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.TradeGroup.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.TradeGroup.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.TradeGroup.Description
			};
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.Type.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.Type.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.Type.Description
			};
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.LegalActInImportEntry.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.LegalActInImportEntry.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.LegalActInImportEntry.Description
			};
			yield return new RefCusCodeListAttributeName
			{
				ZXE_Name = Constants.RefCusCodeListAttributes.AgreementCodeInImportEntry.Code,
				ZXE_Description = Constants.RefCusCodeListAttributes.AgreementCodeInImportEntry.Description,
				ZXE_ZZK_NKCodeType = CodeType,
				ZXE_ZZZ_NKDataGrouping = Constants.DataGroupingCodes.Brazil,
				ZXE_AllowDuplicates = false,
				ZXE_ColumnCaption = Constants.RefCusCodeListAttributes.AgreementCodeInImportEntry.Description
			};
		}

		static IEnumerable<AgreementsDTO> GetAgreementTableValues(XlsFile xls)
		{
			Contract.Assume(xls != null);

			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			var result = new List<AgreementsDTO>();

			for (var rowId = 3; rowId <= rowCount; rowId++)
			{
				var agreement = new AgreementsDTO
				{
					Code = xls.GetCellValue(rowId, 1)?.ToString() ?? string.Empty,
					Country = xls.GetCellValue(rowId, 4)?.ToString() ?? string.Empty,
					LegalActInImportEntry = xls.GetCellValue(rowId, 9)?.ToString() ?? string.Empty,
					TradeGroup = xls.GetCellValue(rowId, 5)?.ToString().Replace("TRADEGROUP", string.Empty).Trim() ?? string.Empty,
				};

				result.Add(agreement);
			}

			return result;
		}

		static IEnumerable<RefCusCodeListAttribute> GetRefCusCodeListAttributeValue(LaiaAgreementDTO attributes)
		{
			if (attributes != null)
			{
				var attributeName = GetAttributeNameByCountry(attributes.Country);
				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = attributeName,
					ZZE_Value = attributes.Country
				};
				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.Type.Code,
					ZZE_Value = attributes.Subject
				};
				yield return new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.LegalActInImportEntry.Code,
					ZZE_Value = attributes.LegalAct
				};

				if (attributes.Id != "N/A")
				{
					yield return new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = Constants.RefCusCodeListAttributes.AgreementCodeInImportEntry.Code,
						ZZE_Value = attributes.Id
					};
				}
			}
		}

		public static string GetAttributeNameByCountry(string country)
		{
			string name;

			switch (country)
			{
				case "OMC":
				case "Mercosul":
				case "SGPC":
				case "SACU":
					name = Constants.RefCusCodeListAttributes.TradeGroup.Code;
					break;
				default:
					name = Constants.RefCusCodeListAttributes.Country.Code;
					break;
			}

			return name;
		}

		public static List<LaiaAgreementDTO> GetLaiaAgreementDTOs(string html)
		{
			var result = new List<LaiaAgreementDTO>();
			var page = new HtmlDocument();
			page.LoadHtml(html);

			var treeNode = page.DocumentNode?.SelectSingleNode("//table[@class='listing']");
			Contract.Assume(treeNode != null);

			foreach (var dto in treeNode
				.Descendants("tr")
				.Skip(1)
				.Where(tr => tr.Elements("td").Count() == 6)
				.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
				.Select(x => CreateLaiaAgreementDTO(x)))
			{
				result.Add(dto);
			}

			return result;
		}

		static LaiaAgreementDTO CreateLaiaAgreementDTO(List<string> row)
		{
			return new LaiaAgreementDTO
			{
				Id = row[3],
				Country = row[0],
				Subject = row[1],
				LegalAct = row[4],
				Name = row[2]
			};
		}
	}

	public class LaiaAgreementDTO
	{
		public string Id { get; set; }
		public string Country { get; set; }
		public string Subject { get; set; }
		public string LegalAct { get; set; }
		public string Name { get; set; }
	}

	class AgreementsDTO
	{
		public string Code { get; set; }
		public string Country { get; set; }
		public string LegalActInImportEntry { get; set; }
		public string TradeGroup { get; set; }
	}
}
