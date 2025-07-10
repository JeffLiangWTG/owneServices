using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class AQISProducerCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISProducerCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISProducerCodes_{0}.xml";

		protected override string DataSource => "AU AQIS Producer Codes";

		protected static string CodeType { get => CMRConstants.CodeTypes.CMRPR; }

		protected override bool SaveWriter => false;

		const bool IsAutoExpiryOn = true;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CodeType, defaultStartDate, CMRConstants.CodeListAttributeNames.AQISProducerCountryCode);
		}

		static PropertyMapping<RefCusCodeList>[] CodeListMappings =>
		[
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 8),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 54, 240)
		];

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_AQISProducerCountryCode =>
		[
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 10, 2)
		];

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_AQISProducerLocality =>
		[
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 13, 40)
		];

		LineToEntityConverter<RefCusCodeList> CodeListConverter => codeListConverter ??= new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
		LineToEntityConverter<RefCusCodeList> codeListConverter;

		LineToEntityConverter<RefCusCodeListAttribute> ProducerCountryCodeAttributeConverter => producerCountryCodeAttributeConverter ??= new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_AQISProducerCountryCode, 1);
		LineToEntityConverter<RefCusCodeListAttribute> producerCountryCodeAttributeConverter;

		LineToEntityConverter<RefCusCodeListAttribute> ProducerLocalityAttributeConverter => producerLocalityAttributeConverter ??= new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_AQISProducerLocality, 1);
		LineToEntityConverter<RefCusCodeListAttribute> producerLocalityAttributeConverter;

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListDict = ParseCodeLists(content);

			foreach (var codeList in codeListDict)
			{
				var dataSource = string.Format(CultureInfo.InvariantCulture, "{0} {1}", DataSource, codeList.Key);
				xmlWriter.SetDataSource(dataSource);
				xmlWriter.SetUpdateType(UpdateType.Full);
				xmlWriter.SetPublicationTime(PublishedDate);

				foreach (var code in codeList.Value)
				{
					xmlWriter.PopulateData(code);
				}

				var outputFileName = string.Format(CultureInfo.InvariantCulture, OutputXMLName, codeList.Key);
				var outputPath = Path.Combine(OutputDirectory, outputFileName);
				xmlWriter.SaveXml(outputPath);
				Console.WriteLine($"End of parsing {dataSource}");
			}
		}

		protected RefCusCodeList ProcessLine(string line)
		{
			return ProcessLineWithCountryAttribute(line, out _);
		}

		SortedDictionary<char, List<RefCusCodeList>> ParseCodeLists(string content)
		{
			var codeListDictionary = new SortedDictionary<char, List<RefCusCodeList>>();

			foreach (var line in content.NonEmptyLines())
			{
				var code = ProcessLineWithCountryAttribute(line, out var producerCountryCodeAttribute);
				var producerCountryCode = producerCountryCodeAttribute?.ZZE_Value;
				if (producerCountryCode != null)
				{
					var countryCodeFirstCharacter = char.ToUpperInvariant(producerCountryCode[0]);
					if (codeListDictionary.TryGetValue(countryCodeFirstCharacter, out var codeList))
					{
						codeList.Add(code);
					}
					else
					{
						codeListDictionary[countryCodeFirstCharacter] = [code];
					}
				}
			}
			return codeListDictionary;
		}

		RefCusCodeList ProcessLineWithCountryAttribute(string line, out RefCusCodeListAttribute countryAttribute)
		{
			countryAttribute = null;
			var code = CodeListConverter.Convert(line);
			if (code.ZZD_Code != null && code.ZZD_Description != null)
			{
				var producerCountryCodeAttribute = ProducerCountryCodeAttributeConverter.Convert(line);
				var producerLocalityAttribute = ProducerLocalityAttributeConverter.Convert(line);

				if (!string.IsNullOrEmpty(producerCountryCodeAttribute.ZZE_Value))
				{
					producerCountryCodeAttribute.ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerCountryCode;
					code.RefCusCodeListAttributes = [producerCountryCodeAttribute];
					countryAttribute = producerCountryCodeAttribute;
				}
				else
				{
					Console.Error.WriteLine($"Skipped line '{line}' as it doesn't have country code");
					return null;
				}

				if (producerLocalityAttribute.ZZE_Value != null)
				{
					producerLocalityAttribute.ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerLocality;
					code.RefCusCodeListAttributes = code.RefCusCodeListAttributes?.Prepend(producerLocalityAttribute).ToArray();
				}
			}
			else
			{
				Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
				return null;
			}
			return code;
		}
	}
}
