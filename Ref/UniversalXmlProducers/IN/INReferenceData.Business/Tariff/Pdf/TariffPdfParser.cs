using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CargoWise.RefDbRepo.INReferenceData.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class TariffPdfParser : ITariffPdfParser
	{
		public TariffPdfParser(IFolderLocationProvider locationProvider, ILogger logger)
		{
			this.locationProvider = locationProvider;
			this.logger = logger;
		}

		public string ParseFilesAsJson(string pdfRootFolder, DateTime? date)
		{
			var jsonCreationFolder = locationProvider.GetJsonCreationFolder(date);
			if (string.IsNullOrEmpty(jsonCreationFolder) || string.IsNullOrEmpty(pdfRootFolder))
			{
				logger.Log(LogType.ReviewRequired, "Invalid folder path");
				return null;
			}

			var folderInfo = new DirectoryInfo(pdfRootFolder);
			if (!folderInfo.Exists)
			{
				logger.Log(LogType.ReviewRequired, $"Folder {pdfRootFolder} doesnot exist");
				return null;
			}

			var pdfFiles = folderInfo.GetFiles("*.pdf");

			foreach (var item in pdfFiles)
			{
				try
				{
					var result = ParseDataAsJson(item.FullName);
					var jsonFilePath = Path.Combine(jsonCreationFolder, $"{Path.GetFileNameWithoutExtension(item.Name)}.json");
					File.WriteAllText(jsonFilePath, result);
				}
				catch (UnhandledApplicationException ex)
				{
					logger.Log(LogType.ReviewRequired, $"Skipping the file {item.FullName}, because ", ex);
				}
			}

			return jsonCreationFolder;
		}

		#region Implementation

		string ParseDataAsJson(string filePath)
		{
			try
			{
				return ParseDataCore(filePath);
			}
			catch (Exception ex)
			{
				throw new UnhandledApplicationException($"Error parsing PDF file {filePath}", ex);
			}
		}

		string ParseDataCore(string filePath)
		{
			logger.Log(LogType.Info, "Parsing PDF file", filePath);
			using (var reader = new PdfReader(filePath))
			using (var pdfDocument = new PdfDocument(reader))
			{
				var tableData = new List<TariffDataItem>();
				var positionProvider = new ColumnPositionProvider(1);

				for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
				{
					logger.Log(LogType.Info, "Parsing PDF page", i);
					var strategy = new TariffTableDataExtractionStrategy(positionProvider, logger);
					PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(i), strategy);
					strategy.ExtractTableData(tableData, out var endOfData);
					if (endOfData)
					{
						break;
					}
				}

				var dummyRow = tableData.Find(row => row.DummyData);
				if (dummyRow != null)
				{
					tableData.Remove(dummyRow);
					logger.Log(LogType.Info, "Removing dummy record");
				}

				Validator.Validate(tableData, Path.GetFileNameWithoutExtension(filePath));
				return JsonSerializer.Serialize(tableData, Constants.Tariff.Processing.JsonSerializerOptions);
			}
		}

		#endregion

		TariffDataValidator Validator => validator ?? (validator = new TariffDataValidator(logger));
		TariffDataValidator validator;

		readonly IFolderLocationProvider locationProvider;
		readonly ILogger logger;
	}
}
