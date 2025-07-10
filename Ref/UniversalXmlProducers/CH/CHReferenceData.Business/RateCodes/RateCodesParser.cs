using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterData;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes
{
	public abstract class RateCodesParser
	{
		DownloadResult masterdataDownload { get; }

		protected RateCodesParser(DownloadResult masterdataDownload)
		{
			this.masterdataDownload = masterdataDownload;
			LogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + $"-{RateType}RateCodes.log");
		}

		protected abstract string RateType { get; }

		protected abstract string RateTypeDescription { get; }

		protected abstract IMasterDataRateType[] GetRateTypes(tariffMasterData masterData);

		public void ConvertToRefXML(string outputFilePath)
		{
			var inputDoc = Helper.DeserializeXML<tariffMasterData>(masterdataDownload.Content);

			var logFileText = inputDoc.created.ToString("r");
			if (!File.Exists(LogFilePath) || !logFileText.Equals(File.ReadAllText(LogFilePath), System.StringComparison.Ordinal))
			{
				var writerConfiguration = GetXmlWriterConfiguration();
				var outputCodeList = new List<RefCusRateCode>();

				foreach (var rateType in from f in GetRateTypes(inputDoc)
									orderby f.ValidTo
									group f by f.Value into g
									select g.Last())
				{
					outputCodeList.Add(new RefCusRateCode()
					{
						ZY1_RateCode = rateType.Value,
						ZY1_Description = rateType.MeaningDe,
						RefCusRateCodeLanguages = new[]
					{
						new RefCusRateCodeLanguage() { ZXC_ZX6_NKLanguage = "FR", ZXC_Description = rateType.MeaningFr},
						new RefCusRateCodeLanguage() { ZXC_ZX6_NKLanguage = "IT", ZXC_Description = rateType.MeaningIt},
						new RefCusRateCodeLanguage() { ZXC_ZX6_NKLanguage = "EN", ZXC_Description = rateType.MeaningEn},
					}
					});
				}

				var outputTypeList = new[] { new RefCusRateType() { RefCusRateCodes = outputCodeList.ToArray() } };

				var published = inputDoc.created;
				var dataSource = $"CH {RateTypeDescription}";
				Helper.ExportToXMLFile(dataSource, outputFilePath, writerConfiguration, published, outputTypeList);

				File.WriteAllText(LogFilePath, logFileText);
			}
		}

		internal XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeTypeConfiguration = new EntityTypeConfiguration<RefCusRateType>(false);
			refCusCodeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZR_RateType, true, RateType);
			refCusCodeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZR_ZZZ_NKDataGrouping, true, "CH");
			refCusCodeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZR_Description, false, RateTypeDescription);
			refCusCodeTypeConfiguration.IncludeColumn(x => x.RefCusRateCodes);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeTypeConfiguration);

			var refCusRateCodeConfiguration = new EntityTypeConfiguration<RefCusRateCode>(true);
			refCusRateCodeConfiguration.IncludeColumn(x => x.ZY1_RateCode, true);
			refCusRateCodeConfiguration.IncludeColumn(x => x.ZY1_Description);
			refCusRateCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZY1_InternalUse, false, "0");
			refCusRateCodeConfiguration.IncludeColumn(x => x.RefCusRateCodeLanguages);
			refCusRateCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZY1_ZZZ_NKDataGrouping, false, "CH");
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateCodeConfiguration);

			var refCusRateCodeLanguageConfiguration = new EntityTypeConfiguration<RefCusRateCodeLanguage>(true);
			refCusRateCodeLanguageConfiguration.IncludeColumn(x => x.ZXC_ZX6_NKLanguage, true);
			refCusRateCodeLanguageConfiguration.IncludeColumn(x => x.ZXC_Description);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateCodeLanguageConfiguration);

			return writerConfiguration;
		}

		public string LogFilePath { get; }
	}
}
