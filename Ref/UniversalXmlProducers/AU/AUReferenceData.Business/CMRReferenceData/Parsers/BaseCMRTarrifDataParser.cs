using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public interface ITariffDataParser
	{
		string FileNamePrefix { get; }

		void Parse(string content, string[] args, string outputDirectory);
	}

	public abstract class BaseCMRTariffDataParser
		: CommonCMRDataParser, ITariffDataParser
	{
		protected BaseCMRTariffDataParser(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}
		readonly IDateTimeProvider dateTimeProvider;

		protected DateTime CurrentLocalDateTime => dateTimeProvider.CurrentLocalDateTime;

		protected abstract void ParseCore(IXmlWriter xmlWriter, string[] args, string content);

		public void Parse(string content, string[] args, string outputDirectory)
		{
			var publishedDate = CurrentLocalDateTime;
			var writer = new XmlWriter(BuildXMLConfiguration(publishedDate));
			writer.SetDataSource(DataSource);
			writer.SetUpdateType(UpdateType.Full);
			writer.SetPublicationTime(publishedDate);

			ParseCore(writer, args, content);

			var outputFileNamePrefix = IsOutputExpiredOnly(args) ? "(EXPIRED)" : "";
			var outputPath = Path.Combine(outputDirectory, $"{outputFileNamePrefix}{OutputXMLName}");
			writer.SaveXml(outputPath);

			Console.WriteLine($"End of parsing {DataSource}");
		}

		protected static bool IsOutputExpiredOnly(string[] args)
		{
			return args.Any(x => x.Equals("--OUTPUT-EXPIRED-DATA-ONLY", StringComparison.OrdinalIgnoreCase));
		}

		protected virtual bool IsTariffIncludedInOutput(RefCusTariff tariff, DateTime todaysDate, bool isOutputExpiredOnly)
		{
			return isOutputExpiredOnly ? tariff.ZZ1_EndDate.Date < todaysDate.Date : tariff.ZZ1_EndDate.Date >= todaysDate.Date;
		}

		string ITariffDataParser.FileNamePrefix => FileNamePrefix;

		public virtual string TariffsSourceFilePath => ApplicationConfig.AUTariffsOutputPath;
	}
}
