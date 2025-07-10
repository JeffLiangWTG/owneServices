using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class DecTpAccessParser
	{
		public DecTpAccessParser(string jsonForLoadSetting)
		{
			loader = new DecTpAccessLoader(string.IsNullOrEmpty(jsonForLoadSetting) ? null :
							JsonConvert.DeserializeObject<DecTpAccessLoader.LoadSetting>(jsonForLoadSetting));
		}

		public DecTpAccessParser(DecTpAccessLoader.LoadSetting loadSetting = null)
		{
			loader = new DecTpAccessLoader(loadSetting);
		}
		readonly DecTpAccessLoader loader;

		public void ExportToXMLFile(Stream inputStream, string outputFileName, string dataSource, DateTime publicationTime)
		{
			var xmlWriterConfiguration = GetXmlWriterConfiguration();
			var updateType = string.IsNullOrEmpty(loader.Setting.FixedType) ? UpdateType.Full : UpdateType.Partial;
			Helper.ExportToXMLFile(dataSource, outputFileName, xmlWriterConfiguration, publicationTime, GetEntities(inputStream), updateType);
		}

		IEnumerable<RefDataRepoModelEntityType> GetEntities(Stream inputStream)
		{
			var entities = new List<RefDataRepoModelEntityType>();
			entities.AddRange(GetRefCusCodeLists(inputStream));
			return entities;
		}

		IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream inputStream)
		{
			var result = new List<RefCusCodeList>();

			var designatedSupervisionSites = loader.GetDesignatedSupervisionSites(inputStream);
			var dssCodeTypes = loader.DssCodeTypes;
			var dssInfos = from dss in designatedSupervisionSites
										 join ct in dssCodeTypes on dss.Type equals ct.Code
										 select new { dss, type = ct };

			foreach (var info in dssInfos)
			{
				var dss = info.dss;

				var codeList = new RefCusCodeList
				{
					ZZD_ZZK_NKCodeType = info.type.Code,
					ZZD_Code = dss.ParsedCustomsCode.code,
					ZZD_Description = dss.Name,
				};

				var attributes = new List<RefCusCodeListAttribute>();
				var suffix = dss.ParsedCustomsCode.suffix;
				if (!string.IsNullOrEmpty(suffix))
				{
					attributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = Constants.DecTpAccess.AttributeNames.CodeSuffix, ZZE_Value = suffix });
				}

				if (!string.IsNullOrEmpty(dss.CustomsDistrict))
				{
					attributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = Constants.DecTpAccess.AttributeNames.CustomsOffice, ZZE_Value = dss.CustomsDistrict });
				}

				codeList.RefCusCodeListAttributes = attributes.ToArray();

				result.Add(codeList);
			}

			return result;
		}

		static XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeType = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeType.IncludeColumn(x => x.ZZK_CodeType, true);
			codeType.IncludeColumn(x => x.ZZK_Description);
			codeType.IncludeColumnWithConstantValue(x => x.ZZK_MaxLength, false, Constants.DecTpAccess.CodeTypeMaxLength);
			codeType.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, Constants.CNCountryCode);
			codeType.IncludeColumn(x => x.RefCusCodeTypeLanguages);
			//xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeType);

			var codeTypeLanguate = new EntityTypeConfiguration<RefCusCodeTypeLanguage>(true);
			codeTypeLanguate.IncludeColumnWithConstantValue(x => x.ZXI_ZX6_NKLanguage, true, Constants.DecTpAccess.LanguageCode);
			codeTypeLanguate.IncludeColumn(x => x.ZXI_Description);
			//xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeTypeLanguate);

			var attributeName = new EntityTypeConfiguration<RefCusCodeListAttributeName>(true);
			attributeName.IncludeColumn(x => x.ZXE_Name, true);
			attributeName.IncludeColumn(x => x.ZXE_Description);
			attributeName.IncludeColumn(x => x.ZXE_ZZK_NKCodeType, true);
			attributeName.IncludeColumnWithConstantValue(x => x.ZXE_ZZZ_NKDataGrouping, true, Constants.CNCountryCode);
			attributeName.IncludeColumn(x => x.ZXE_IsMandatory);
			attributeName.IncludeColumnWithConstantValue(x => x.ZXE_AllowDuplicates, false, Constants.BitFalse);
			attributeName.IncludeColumnWithConstantValue(x => x.ZXE_IsValueMandatory, false, Constants.BitTrue);
			attributeName.IncludeColumnWithConstantValue(x => x.ZXE_ValueDataType, false, Constants.DataTypes.String);
			attributeName.IncludeColumn(x => x.ZXE_MinLengthOrValue);
			attributeName.IncludeColumn(x => x.ZXE_MaxLengthOrValue);
			attributeName.IncludeColumnWithConstantValue(x => x.ZXE_DecimalPlaces, false, 0);
			attributeName.IncludeColumn(x => x.ZXE_ColumnCaption);
			attributeName.IncludeColumn(x => x.RefCusCodeListAttributeNameLanguages);
			//xmlWriterConfiguration.IncludeEntityTypeConfiguration(attributeName);

			var attributeNameLanguage = new EntityTypeConfiguration<RefCusCodeListAttributeNameLanguage>(true);
			attributeNameLanguage.IncludeColumnWithConstantValue(x => x.ZXH_ZX6_NKLanguage, true, Constants.DecTpAccess.LanguageCode);
			attributeNameLanguage.IncludeColumn(x => x.ZXH_Description);
			attributeNameLanguage.IncludeColumn(x => x.ZXH_Name);
			attributeNameLanguage.IncludeColumn(x => x.ZXH_ColumnCaption);
			//xmlWriterConfiguration.IncludeEntityTypeConfiguration(attributeNameLanguage);

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, new DateTime(1900, 1, 1));
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, new DateTime(2079, 6, 6, 23, 59, 00));
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CNCountryCode);
			codeList.IncludeColumn(x => x.RefCusCodeListAttributes);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			var codeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttribute.IncludeColumn(x => x.ZZE_Value);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeListAttribute);

			return xmlWriterConfiguration;
		}
	}
}
