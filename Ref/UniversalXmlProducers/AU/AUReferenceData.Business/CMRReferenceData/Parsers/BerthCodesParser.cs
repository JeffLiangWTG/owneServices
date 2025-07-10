using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class BerthCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.BerthCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_BerthCodes.xml";

		protected override string DataSource => "AU CMR Berth Codes";

		const bool IsAutoExpiryOn = true;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRBC, defaultStartDate, CMRConstants.CodeListAttributeNames.BerthCodeExternalIdentifier);
		}

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 7, 17),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_StartDate, 25, 8),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_EndDate, 34, 8, Constants.RefData_Common.MaximumDateTime),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 43, 35)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_BerthCodeExternalIdentifier => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 79, 20)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_BerthPortCode => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 1, 5)
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
			var berthCodeExternalIdentifierAttributeConverter = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_BerthCodeExternalIdentifier, 1);
			var berthPortCodeAttributeConverter = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_BerthPortCode, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				var cusCodeListDic = new Dictionary<string, RefCusCodeList>();
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = codeListConverter.Convert(line);
						var berthCodeExternalIdentifierAttribute = berthCodeExternalIdentifierAttributeConverter.Convert(line);
						var berthPortCodeAttribute = berthPortCodeAttributeConverter.Convert(line);
						if (code.ZZD_Code != null && code.ZZD_Description != null)
						{
							if (cusCodeListDic.TryGetValue(code.ZZD_Code, out var cusCodeList))
							{
								if (DateTime.Compare(cusCodeList.ZZD_StartDate, code.ZZD_StartDate) < 0)
								{
									cusCodeListDic.Remove(code.ZZD_Code);
								}
								else
								{
									continue;
								}
							}

							if (berthPortCodeAttribute.ZZE_Value != null)
							{
								berthPortCodeAttribute.ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.BerthPortCode;
								code.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { berthPortCodeAttribute };
							}

							if (berthCodeExternalIdentifierAttribute.ZZE_Value != null)
							{
								var berthCodeExternalIdentifierAttributeList = new RefCusCodeListAttribute[] { berthCodeExternalIdentifierAttribute };
								code.RefCusCodeListAttributes = code.RefCusCodeListAttributes?.Concat(berthCodeExternalIdentifierAttributeList).ToArray() ?? berthCodeExternalIdentifierAttributeList;
							}
							cusCodeListDic.Add(code.ZZD_Code, code);
						}
						else
						{
							Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
						}
					}
				}

				foreach (var codeList in cusCodeListDic.Values)
				{
					xmlWriter.PopulateData(codeList);
				}
			}
		}
	}
}
