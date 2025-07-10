using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class ECCNCodeListBuilder
	{
		public ECCNCodeListBuilder(StringBuilder errorCollector)
		{
			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}
			ErrorCollector = errorCollector;
		}
		StringBuilder ErrorCollector;

		public void GenerateUniversalReferenceDataXml(IEnumerable<ECCNCodeListData> content, DateTime publicationDate, string outputPath)
		{
			var convertedContent = ConvertToRefCusCodeList(content);

			if (convertedContent.Any())
			{
				var dataSource = $"{XMLWriterDataSource}";
				Helper.ExportToXMLFile(dataSource, Path.Combine(outputPath, GetOutputFileName(dataSource, publicationDate)), getXmlWriterConfiguration(), publicationDate, UpdateType.Full, convertedContent);
			}
		}

		protected IEnumerable<RefCusCodeList> ConvertToRefCusCodeList(IEnumerable<ECCNCodeListData> data)
		{
			var results = new List<RefCusCodeList>();
			var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
			var previousCode = string.Empty;
			var uniqueItems = new HashSet<string>();
			data = data.OrderBy(p => p.Code);
			RefCusCodeList refCusCodeList = null;

			foreach (var ad in data)
			{
				if (IsValid(ad))
				{
					var uniqueId = ad.Code + " " + ad.TariffCode;
					if (!uniqueItems.Add(uniqueId))
					{
						ErrorCollector.AppendLine(Invariant($"Duplicate ECCN code/Tariff Code relation exists: '{uniqueId}'"));
					}

					if (ad.Code != previousCode)
					{
						previousCode = ad.Code;
						refCusCodeList = new RefCusCodeList()
						{
							ZZD_Code = ad.Code,
							ZZD_Description = ad.Code
						};
						refCusCodeListAttributes = new List<RefCusCodeListAttribute>();

						results.Add(refCusCodeList);
					}

					var refCusCodeListAttribute = new RefCusCodeListAttribute()
					{
						ZZE_Value = ad.TariffCode.Substring(0, 8),
					};
					refCusCodeListAttributes.Add(refCusCodeListAttribute);

					refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
				}
			}

			return results;
		}

		protected bool IsValid(ECCNCodeListData data)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(data.Code))
			{
				validationErrors.Append("ECCN Code is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(data.TariffCode))
			{
				validationErrors.Append("Tariff Code is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = FormattableString.Invariant($"Validation error: Key '{data.Code}_{data.TariffCode}' Errors: {validationErrors}");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected static XmlWriterConfiguration getXmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			entityConfig.IncludeColumn(x => x.ZZD_Code, true);
			entityConfig.IncludeColumn(x => x.ZZD_Description);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.ECCNCodeTypeDefaults.CodeType);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinimumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaximumDateTime);
			entityConfig.IncludeColumn(x => x.RefCusCodeListAttributes);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			var attribConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			attribConfig.IncludeColumnWithConstantValue(x => x.ZZE_ZXE_NKName, false, Constants.ECCNCodeTypeDefaults.LicenseType);
			attribConfig.IncludeColumn(x => x.ZZE_Value, true);

			writerConfig.IncludeEntityTypeConfiguration(attribConfig);

			return writerConfig;
		}

		static string FilePrefix => "RefCusCodeList";
		static string XMLWriterDataSource => "ECCN Codes";
		static string GetOutputFileName(string sourceName, DateTime publicationDate) => FormattableString.Invariant($"{FilePrefix}_{sourceName}_{publicationDate:HHmmssfff}.xml");
	}
}
