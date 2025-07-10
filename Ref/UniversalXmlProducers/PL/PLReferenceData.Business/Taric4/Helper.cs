using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

static class Helper
{
	public static void SaveToFile(this XDocument document, string filePath)
	{
		var directoryName = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}

		using var file = File.Create(filePath);
		document.Save(file);
	}

	public static void MoveFileToArchiveFolder(string filePath, string archiveFolderPath = null)
	{
		archiveFolderPath ??= AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.TariffArchiveFolder);

		if (!Directory.Exists(archiveFolderPath))
		{
			Directory.CreateDirectory(archiveFolderPath);
		}

		var destinationPath = Path.Combine(archiveFolderPath, Path.GetFileName(filePath));

		File.Move(filePath, destinationPath, overwrite: true);
	}

	public static void UnpackFileEntryFromArchive(string archiveFilePath, string fileEntry, string targetFilePath)
	{
		using var zippedContent = ZipFile.Open(archiveFilePath, ZipArchiveMode.Read);
		using var entryStream = GetEntryStreamFromZipArchive(zippedContent, fileEntry);
		using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: false);
		entryStream.CopyTo(fileStream);
	}

	public static XDocument UnpackStreamAndLoadXml(MemoryStream stream, string fileEntry)
	{
		using var zippedContent = new ZipArchive(stream);
		using var entryStream = GetEntryStreamFromZipArchive(zippedContent, fileEntry);
		return XDocument.Load(entryStream);
	}

	static Stream GetEntryStreamFromZipArchive(ZipArchive zipData, string searchedFile)
	{
		var zipEntry = zipData is not null && zipData.Entries.Count > 0
			? zipData.Entries.FirstOrDefault(x => x.Name.Contains(searchedFile))
			: throw new Exception("Zip without entries have been passed.");

		return zipEntry is not null && zipEntry.Length > 0
			? zipEntry.Open()
			: throw new Exception("Searched Zip Entry not found or is empty.");
	}

	public static DateTime ToSmallDateTime(this DateTime datetime) => new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, 00);
}
