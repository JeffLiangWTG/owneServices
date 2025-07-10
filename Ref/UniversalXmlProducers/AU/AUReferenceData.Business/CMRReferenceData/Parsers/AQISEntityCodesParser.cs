using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AQISEntityCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISEntityCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISEntityCodes.xml";

		protected override string DataSource => "AU AQIS Entity Codes";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) => CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRAE, Constants.DataGrouping);

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 16),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 18, 234)
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);

			foreach (var line in content.NonEmptyLines())
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
