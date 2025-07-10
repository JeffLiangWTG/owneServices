using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class CharacteristicCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.CharacteristicCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_CharacteristicCodes.xml";

		protected override string DataSource => "AU Characteristic Codes";

		const bool IsAutoExpiryOn = true;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRCC, defaultStartDate);
		}

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 4),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 6, 100),
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = codeListConverter.Convert(line);
						if (code.ZZD_Code != null && code.ZZD_Description != null)
						{
							xmlWriter.PopulateData(code);
						}
						else
						{
							Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
						}
					}
				}
			}
		}
	}
}
