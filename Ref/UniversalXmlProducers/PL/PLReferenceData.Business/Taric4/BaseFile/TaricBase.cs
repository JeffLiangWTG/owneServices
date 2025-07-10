using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates.UpdateStrategy;
using System.Xml;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.BaseFile;

public static class TaricBase
{
	public enum ProcessingMode
	{
		Download,
		Normalize,
		GetUpdates
	}

	public static bool Process(ProcessingMode mode, string filePath)
	{
		if (mode is not (ProcessingMode.Download or ProcessingMode.Normalize or ProcessingMode.GetUpdates))
		{
			throw new ArgumentException($"Unsupported mode: {mode}");
		}


		if (mode is ProcessingMode.Normalize or ProcessingMode.Download)
		{
			if (mode is ProcessingMode.Download)
			{
				DownloadAndUnpackTaricBaseFile(filePath);
			}
			TaricFileNormalizer.Normalize(filePath, Taric4Constants.NORMALIZED_BASE_FILE_FULL_PATH);
		}

		var documentFilePath = mode is ProcessingMode.GetUpdates
			? filePath
			: Taric4Constants.NORMALIZED_BASE_FILE_FULL_PATH;

		IUpdateStrategy updateStrategy = mode switch
		{
			ProcessingMode.Download => new DefaultUpdateStrategy(GetDatabaseDate(documentFilePath)),
			ProcessingMode.GetUpdates => new LastSuccessfulUpdateStrategy(GetLastSuccessfulFilePath()),
			_ => null,
		};

		return updateStrategy is null
			|| (UpdateHelper.GenerateAndRunUpdateRequests(updateStrategy)
				&& MergeUpdatesAndSaveTaricBaseFile(documentFilePath));

		static DateTime GetDatabaseDate(string filePath)
		{
			using var reader = XmlReader.Create(filePath);
			return reader.ReadToDescendant("ResultsInfo") && reader.ReadToDescendant("databaseDate") && reader.Read()
				? Convert.ToDateTime(reader.Value, CultureInfo.InvariantCulture)
				: throw new Exception("ResultsInfo/databaseDate element not found.");
		}

		static string GetLastSuccessfulFilePath() =>
			Path.Combine(ApplicationConfig.Instance.DownloadsPLPath, AppConfigHelper.GetAppSettingsValue("LastSuccessfulUpdateRunFileName"));
	}

	static void DownloadAndUnpackTaricBaseFile(string targetFilePath)
	{
		var downloadDirectory = AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.TariffUpdateFolder);
		var taric4DownloadManager = Services.Taric4.Taric4FileDownloaderFactory.CreateFileDownloader();

		var downloadedFilename = taric4DownloadManager.DownloadTaric4BaseFile(downloadDirectory);

		var downloadedFilePath = Path.Combine(downloadDirectory, downloadedFilename);
		Helper.UnpackFileEntryFromArchive(downloadedFilePath, "base", targetFilePath);

		Helper.MoveFileToArchiveFolder(downloadedFilePath);
	}

	public static bool MergeUpdatesAndSaveTaricBaseFile(string sourceFilePath, string processFolderPath = null, string targetFilePath = null)
	{
		var processingDir = processFolderPath ?? AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.TariffUpdateFolder);
		var mergeIsNotRequired = !Directory.Exists(processingDir);

		return mergeIsNotRequired
			|| UpdateTaricBaseWithLatestUpdates(
				sourceFilePath,
				processingDir,
				targetFilePath ?? Taric4Constants.NORMALIZED_BASE_FILE_FULL_PATH);
	}

	static bool UpdateTaricBaseWithLatestUpdates(string baseFile, string processFolderPath, string saveFilePath)
	{
		try
		{
			var baseData = XmlParser.DeserializeFromFile<IsztarHistoryResponse>(baseFile);
			var lastUpdateDate = baseData.ResultsInfo.databaseDate;

			// all filenames should be named like that updateYYYYMMDD.xml based on specification
			var updateFiles = Directory.GetFiles(processFolderPath)
					.Where(x => Regex.Matches(Path.GetFileName(x), @"^update([0-9]{4}[0-1][0-9][0-3][0-9])\.xml$") is MatchCollection matches
						&& matches.Count > 0 && matches[0].Groups.Count > 1
						&& DateTime.ParseExact(matches[0].Groups[1].Value, "yyyyMMdd", CultureInfo.CurrentCulture) >= lastUpdateDate)
					// order by name so newest is last;
					.Order()
					.ToArray();

			foreach (var updateFile in updateFiles)
			{
				lastUpdateDate = MergeAndMoveToArchive(baseData, updateFile, lastUpdateDate);
			}

			baseData.ResultsInfo.databaseDate = lastUpdateDate;
			XmlParser.SerializeToFile(baseData, saveFilePath);
			return true;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex);
		}
		return false;
	}

	static DateTime MergeAndMoveToArchive(IsztarHistoryResponse baseData, string updateFile, DateTime lastUpdateDate)
	{
		var result = lastUpdateDate;
		try
		{
			var update = XDocument.Load(updateFile);
			var updateData = XmlParser.Deserialize<IsztarHistoryResponse>(update);

			if (updateData.ResultsInfo.totalRecords != "0")
			{
				new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
			}

			result = updateData.ResultsInfo.endDate;

			Helper.MoveFileToArchiveFolder(updateFile);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Exception caught for : {updateFile}");
			Console.Error.WriteLine(ex);
		}

		return result;
	}
}
