using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NCTSCodeListParser
	{
		public NCTSCodeListParser(StringBuilder errorBuilder)
		{
			this.errorBuilder = errorBuilder;
		}
		readonly StringBuilder errorBuilder;

		public IEnumerable<RefCusCodeList> ParseXML(XDocument sourceXML, IUCCExportCodeListDetail codeListDetail)
		{
			return sourceXML != null ? PopulateRefCusCodeList(sourceXML, codeListDetail.CodeType, codeListDetail.DataSource, codeListDetail.XmlDataItemForCode, codeListDetail.AttributeValues) : Enumerable.Empty<RefCusCodeList>();
		}

		IReadOnlyList<RefCusCodeList> PopulateRefCusCodeList(XDocument inputDoc, string codeType, string dataSource, string xmlDataItemForCode, IReadOnlyList<(string attributeName, string attributeValue)> attributeValues)
		{
			var outputCodeList = new List<RefCusCodeList>();
			if (inputDoc != null)
			{
				var entries = inputDoc.Descendants().Where(e => e.Name.LocalName == Constants.NCTS.rdentry);

				foreach (var entry in entries)
				{
					var status = entry.Elements().Descendants().First(s => s.Name.LocalName == Constants.NCTS.state).Value;
					if (status == Constants.NCTS.valid)
					{
						var validFrom = entry.Elements().Descendants().First(v => v.Name.LocalName == Constants.NCTS.activefrom).Value;
						var dataItem = entry.Elements().Where(d => d.Name.LocalName == Constants.NCTS.dataitem);
						var code = dataItem.FirstOrDefault(i => i.FirstAttribute.Value == xmlDataItemForCode)?.Value ?? string.Empty;
						var remark = dataItem.FirstOrDefault(i => i.FirstAttribute.Value == Constants.NCTS.remark)?.Value ?? string.Empty;
						var descListElem = entry.Elements().Where(l => l.Name.LocalName == Constants.NCTS.lsdlist);

						string GetDescription(string language) => descListElem.Elements().FirstOrDefault(ds => ds.Name.LocalName == Constants.NCTS.descElemName && (string)ds.Attribute(Constants.NCTS.lang) == language)?.Value;

						var englishDescription = GetDescription(Constants.NCTS.English);
						if (!string.IsNullOrEmpty(englishDescription))
						{
							var nctsCode = new RefCusCodeList
							{
								ZZD_Code = code,
								ZZD_StartDate = DateTime.ParseExact(validFrom, Constants.NCTS.startDateFormat, CultureInfo.InvariantCulture),
								ZZD_Description = englishDescription
							};

							var refCusCodeListLanguages = new List<RefCusCodeListLanguage>();
							var languagesExceptEnglish = descListElem.Elements()
								.Where(ds => ds.Name.LocalName == Constants.NCTS.descElemName)
								.Select(x => (string)x.Attribute(Constants.NCTS.lang))
								.Except(new[] { Constants.NCTS.English });
							foreach (var languageCode in languagesExceptEnglish.ToArray())
							{
								var description = GetDescription(languageCode);
								if (!string.IsNullOrEmpty(description))
								{
									refCusCodeListLanguages.Add(new RefCusCodeListLanguage
									{
										ZXA_ZX6_NKLanguage = languageCode.ToUpper(CultureInfo.InvariantCulture),
										ZXA_Description = description
									});
								}
							}
							nctsCode.RefCusCodeListLanguages = refCusCodeListLanguages.ToArray();
							nctsCode.RefCusCodeListAttributes = attributeValues.Select(av => new RefCusCodeListAttribute
							{
								ZZE_ZXE_NKName = av.attributeName,
								ZZE_Value = av.attributeValue
							}).ToArray();

							if (codeType == Constants.ZZRefCusCodeList.NctsFunctionalErrorCodesIeCA)
							{
								nctsCode.RefCusCodeListAttributes = nctsCode.RefCusCodeListAttributes.Append(new RefCusCodeListAttribute
								{
									ZZE_ZXE_NKName = Constants.NCTS.remark,
									ZZE_Value = remark.Left(255)
								}).ToArray();
							}
							outputCodeList.Add(nctsCode);
						}
						else
						{
							errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"English description missing => Skip record! NctsCodeListXmlProducer: DataSource '{dataSource}', CodeType '{codeType}, dataItem '{code}'");
						}
					}
				}
			}
			return outputCodeList;
		}

		public static IEnumerable<T> Merge<T>(IEnumerable<IEnumerable<T>> sourceLists, Func<T, object> getMergeKey, Func<IEnumerable<T>, T> getMerged) where T : Common.UniversalXmlWriter.RefDataRepoModelEntityType
		{
			return sourceLists.SelectMany(l => l).GroupBy(getMergeKey).Select(group => getMerged(group));
		}

		public static RefCusCodeList MergeRefCusCodeList(IEnumerable<RefCusCodeList> list)
		{
			var result = list.FirstOrDefault();
			if (result != null)
			{
				result.RefCusCodeListAttributes = Merge(
					list.Select(codeList => codeList.RefCusCodeListAttributes),
					attr => (attr.ZZE_ZXE_NKName, attr.ZZE_Value),
					dupAttrs => dupAttrs.FirstOrDefault()
				).ToArray();
			}
			return result;
		}
	}
}
