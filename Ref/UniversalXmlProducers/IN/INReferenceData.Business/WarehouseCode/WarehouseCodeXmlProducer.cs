using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.INReferenceData.Business;

public class WarehouseCodeXmlProducer
{
	readonly string DataSource;

	public WarehouseCodeXmlProducer(string dataSource)
	{
		DataSource = dataSource;
	}

	public string ProduceXml()
	{
		var error = "";

		try
		{
			ProduceXmlCore();
		}
		catch (UnhandledApplicationException e)
		{
			error = e.Message;
		}

		return error;
	}

	protected virtual void ProduceXmlCore()
	{
		try
		{
			var responseContent = Downloader.DownloadData();
			var warehouseCodes = WarehouseCodeParser.ParseResponse(responseContent);
			ExportToXml(warehouseCodes);
		}
		catch (Exception ex)
		{
			throw new UnhandledApplicationException("Error while processing warehouse codes", ex);
		}
	}

	WarehouseCodeDownloader Downloader => downloader ??= new WarehouseCodeDownloader();
	WarehouseCodeDownloader downloader;

	#region Output Files

	protected void ExportToXml(List<RefCusCodeList> warehouseCodes)
	{
		Helper.Assume(!warehouseCodes.IsNullOrEmpty(), "No warehouse codes to export");
		var xmlWriterConfig = XMLWriterHelper.GetWarehouseCodeXMLWriterConfiguration();
		XMLWriterHelper.ExportToXMLFile(
			xmlWriterConfig,
			warehouseCodes,
			DataSource,
			DateTime.Now,
			UpdateType.Full,
			GetOutputFilePath());
	}

	protected string GetOutputFilePath()
	{
		var fileName = "RefWarehouseCodeZZ_IN.xml";
		var outputPath = AppConfig.Shared.OutputDirectory;
		var filePath = Path.Combine(outputPath, fileName);
		if (!outputFilePaths.Contains(filePath))
		{
			outputFilePaths.Add(filePath);
		}

		return filePath;
	}

	protected void DeleteOutputFiles()
	{
		outputFilePaths.Where(File.Exists).ToList().ForEach(File.Delete);
	}

	readonly List<string> outputFilePaths = [];

	#endregion
}
