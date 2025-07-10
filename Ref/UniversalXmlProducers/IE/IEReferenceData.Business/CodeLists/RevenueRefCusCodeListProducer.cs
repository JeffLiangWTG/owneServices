using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Business;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Business
{
	public class RevenueRefCusCodeListProducer
	{
		public RevenueRefCusCodeListProducer(string codeType)
		{
			CodeType = codeType;
		}

		public string ConvertCodeListToXml(IEnumerable<IRevenueCodeDescriptionPair> codeDescriptionList, DateTime publicationDateTime, string outputFilePath, string dataGrouping, string dependingOn = null, bool requiresAttribute = false)
		{
			return ConvertCodeListToXml(codeDescriptionList, publicationDateTime, UpdateType.Full, outputFilePath, dataGrouping, dependingOn, requiresAttribute);
		}

		public string ConvertCodeListToXml(IEnumerable<IRevenueCodeDescriptionPair> codeDescriptionList, DateTime publicationDateTime, UpdateType updateType, string outputFilePath, string dataGrouping, string dependingOn = null, bool requiresAttribute = false)
		{
			ErrorBuilder.Clear();
			if (!string.IsNullOrEmpty(outputFilePath))
			{
				var refCusCodeLists = PopulateRefCusCodeList(codeDescriptionList, requiresAttribute);
				if (refCusCodeLists.Count > 0)
				{
					Helper.ExportToXmlFile(
						dataSource: $"{dataGrouping} {CodeType}",
						outputFile: Path.Combine(outputFilePath, $"RefCusCodeListZZ_{dataGrouping}_{CodeType}.xml"),
						xmlWriterConfig: GetWriterConfiguration(CodeType, requiresAttribute: requiresAttribute, dataGrouping: dataGrouping),
						publicationDateTime: publicationDateTime,
						updateType: updateType,
						codeList: refCusCodeLists,
						dependingOn: dependingOn
					);
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"There were no valid records for {CodeType}, unable to generate XML file.");
				}
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Output file path is empty for Producer of type {CodeType}");
			}
			return ErrorBuilder.ToString();
		}

		List<RefCusCodeList> PopulateRefCusCodeList(IEnumerable<IRevenueCodeDescriptionPair> codeDescriptionList, bool requiresLevelAttribute)
		{
			var result = new List<RefCusCodeList>();
			foreach (var codeList in codeDescriptionList)
			{
				if (CheckDataIsValid(codeList))
				{
					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = codeList.Code,
						ZZD_Description = codeList.Description
					};
					if (requiresLevelAttribute)
					{
						var attributeList = new List<RefCusCodeListAttribute>
						{
							new RefCusCodeListAttribute()
							{
								ZZE_ZXE_NKName = Constants.CodeListAttributes.Level,
								ZZE_Value = Constants.CodeListAttributes.House,
							}
						};
						if (codeList.Code != Constants.NCTSPreviousDocumentsCodeLists.GoodsDeclarationForExportationCode)
						{
							attributeList.Add(
								new RefCusCodeListAttribute()
								{
									ZZE_ZXE_NKName = Constants.CodeListAttributes.Level,
									ZZE_Value = Constants.CodeListAttributes.Header,
								});
							attributeList.Add(
								new RefCusCodeListAttribute()
								{
									ZZE_ZXE_NKName = Constants.CodeListAttributes.Level,
									ZZE_Value = Constants.CodeListAttributes.Item,
								});
						}
						refCusCodeList.RefCusCodeListAttributes = attributeList.ToArray();
					}

					result.Add(refCusCodeList);
				}
			}
			return result;
		}

		bool CheckDataIsValid(IRevenueCodeDescriptionPair listItem)
		{
			var result = !string.IsNullOrWhiteSpace(listItem.Code) && !string.IsNullOrWhiteSpace(listItem.Description);
			if (!result)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import record from {CodeType} due to empty Code or Description. DETAILS:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {listItem.Code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {listItem.Description}");
			}
			return result;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		string CodeType { get; }

		static XmlWriterConfiguration GetWriterConfiguration(string codeType, bool requiresAttribute = false, string attributeName = "", string dataGrouping = Constants.IECountryCode)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, dataGrouping);

			var writerConfiguration = new XmlWriterConfiguration();

			if (requiresAttribute)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
				var refCusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, true);
				refCusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);
			}
			else
			{
				writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
			}

			return writerConfiguration;
		}
	}
}
