using System;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing
{
	public abstract class ProcessManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		public void RunProcess(string outputPath, StringBuilder errorCollector)
		{
			try
			{
				var configProvider = GetConfigProvider();
				var downloadManager = GetDownloadManager(errorCollector);
				var builder = GetPortBuilder();
				var ccsukDataTask = GetCCSUKLocationLoader().GetLocations();
				var sourceDataList = ConfigLoader.LoadConfigFile(configProvider);

				foreach (var source in sourceDataList)
				{
					try
					{
						var processor = GetProcessor(source, errorCollector, downloadManager);

						var data = processor.Extract();
						if (data.Count == 0)
						{
							errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Warning: source '{source.Name}' returned no data.");
						}
						else
						{
							builder.BuildXml(DateTime.Now, data, outputPath, ccsukDataTask);
						}
					}
					catch (Exception ex)
					{
						errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for Source '{source.Name}' Exception: {ex.GetBaseException().Message}");
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Processing failed", ex);
			}
		}

		protected static DataProcessor GetProcessor(ICDSPortSource source, StringBuilder errorCollector, IDownloadManager downloadManager)
		{
			DataProcessor processor;
			if (source.UseODS)
			{
				processor = new ODSProcessor(source, errorCollector, downloadManager);
			}
			else
			{
				processor = new CSVProcessor(source, downloadManager);
			}

			return processor;
		}

		public abstract IDownloadManager GetDownloadManager(StringBuilder errorCollector);
		public abstract IConfigProvider GetConfigProvider();
		public abstract IPortBuilder GetPortBuilder();
		public abstract CCSUKLocationLoader GetCCSUKLocationLoader();
	}

	public class WebClientProcessManager : ProcessManager
	{
		public WebClientProcessManager(IPortBuilder portBuilder, IRefDataLoader refDataLoader)
		{
			PortBuilder = portBuilder;
			RefDataLoader = refDataLoader;
		}
		readonly IPortBuilder PortBuilder;
		readonly IRefDataLoader RefDataLoader;

		const string ConfigFileName = "CDSPortConfig.xml";

		public override IConfigProvider GetConfigProvider() => new ConfigProvider(ConfigFileName);

		public override IDownloadManager GetDownloadManager(StringBuilder errorCollector) => new DownloadManager(new WebClientWrapper());

		public override IPortBuilder GetPortBuilder() => PortBuilder;

		public override CCSUKLocationLoader GetCCSUKLocationLoader() => new CCSUKLocationLoader(RefDataLoader);
	}
}
