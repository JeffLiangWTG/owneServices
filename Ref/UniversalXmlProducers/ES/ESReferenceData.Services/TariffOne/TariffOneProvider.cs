using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using CargoWise.RefDbRepo.ESReferenceData.Services.Helpers;

namespace CargoWise.RefDbRepo.ESReferenceData.Services;

public class TariffOneProvider(string mainPath, string tariffOneURL)
{
	readonly string mainPath = mainPath;
	readonly string tariffOneURL = tariffOneURL;

	public static string FilesFolder => "TariffOne";

	public string GetFileContent(TariffOneFiles fileName)
		=> File.ReadAllText(GetFilePath(fileName));

	public StreamReader GetFileStream(TariffOneFiles fileName)
		=> new(GetFilePath(fileName));

	string GetFilePath(TariffOneFiles fileName)
	{
		var filesPath = Path.Combine(mainPath, FilesFolder);
		var jsonName = Enum.GetName(typeof(TariffOneFiles), fileName) + ".jsonl";
		return Path.Combine(filesPath, jsonName);
	}

	public string GetLatestDataFileName()
	{
		using var httpClient = new HttpClient();
		using var request = new HttpRequestMessage(HttpMethod.Get, tariffOneURL);
		try
		{
			var response = httpClient.Send(request);
			using var reader = new StreamReader(response.Content.ReadAsStream());
			var json = reader.ReadToEnd();
			return json;
		}
		catch (Exception e)
		{

			var error = new StringBuilder();
			error.AppendLine(CultureInfo.InvariantCulture, $"Unable to download Json from the following URL: {tariffOneURL}");
			error.AppendLine(e.Message);
			throw new JSONException(error.ToString());
		}
	}

	public string CreateOrUpdateDataFilesIfNeeded(string dataExportJson)
	{
		var errors = new StringBuilder();

		var dataExport = GetDataExport(dataExportJson, errors);
		if (dataExport != null)
		{
			var fileName = dataExport.name;
			var fileHasChanges = CheckFileHasChanges(fileName);
			if (fileHasChanges)
			{
				DownloadAndExtractFile(fileName, errors);
			}
		}

		return errors.ToString();
	}

	static DataExportSchema GetDataExport(string dataExportJson, StringBuilder errors)
	{
		var dataExport = JsonHelper.GetListItemsFromJsonl<DataExportSchema>(JsonHelper.ToJsonl(dataExportJson), errors);
		if (dataExport != null && dataExport.Count == 1)
		{
			return dataExport[0];
		}
		else
		{
			errors.AppendLine("DataExport load failed. Expected not null and only 1 record. Json Details:");
			errors.AppendLine(dataExportJson);
		}

		return null;
	}

	bool CheckFileHasChanges(string fileName)
	{
		var filesPath = Path.Combine(mainPath, FilesFolder);
		var filePath = Path.Combine(filesPath, fileName);

		return !File.Exists(filePath);
	}

	void DownloadAndExtractFile(string fileName, StringBuilder errors)
	{
		var filesPath = Path.Combine(mainPath, FilesFolder);

		if (Directory.Exists(filesPath))
		{
			Directory.Delete(filesPath, true);
		}

		Directory.CreateDirectory(filesPath);

		if (DownloadZip(filesPath, fileName, errors))
		{
			UnzipFile(Path.Combine(filesPath, fileName), filesPath, errors);
		}
	}

	bool DownloadZip(string outputPath, string fileName, StringBuilder errors)
	{
		var fileURL = $"{tariffOneURL}/{fileName}";
		using var httpClient = new HttpClient();
		using var request = new HttpRequestMessage(HttpMethod.Get, fileURL);
		try
		{
			var response = httpClient.Send(request);
			if (response.IsSuccessStatusCode)
			{
				using var fs = new FileStream(Path.Combine(outputPath, fileName), FileMode.CreateNew);
				response.Content.CopyTo(fs, null, CancellationToken.None);
				return true;
			}
			else
			{
				errors.AppendLine(CultureInfo.InvariantCulture, $"Unable to download zip from the following URL: {fileURL}");
				errors.AppendLine(CultureInfo.InvariantCulture, $"Reponse Code: {response.StatusCode}. Details:");
				return false;
			}
		}
		catch (Exception e)
		{
			errors.AppendLine(CultureInfo.InvariantCulture, $"Exception downloading zip from the following URL: {fileURL}");
			errors.AppendLine(e.Message);
			throw new JSONException(errors.ToString());
		}
	}

	static void UnzipFile(string compressedFile, string outputPath, StringBuilder errors)
	{
		if (!File.Exists(compressedFile))
		{
			throw new FileNotFoundException($"Zip file '{compressedFile}' does not exist");
		}

		try
		{
			var zipHelper = ZipFileHelper.GetZipHelper(compressedFile);

			zipHelper.UnzipFile(compressedFile, outputPath);
		}
		catch (Exception ex)
		{
			errors.AppendLine(CultureInfo.InvariantCulture, $"Failed to extract: {compressedFile}");
			errors.AppendLine(ex.Message);
			throw new FileNotFoundException(errors.ToString());
		}
		var filesExtracted = Directory.GetFiles(outputPath, "*", new EnumerationOptions { RecurseSubdirectories = true });
		ContainsAllRequiredFiles(string.Join('-', filesExtracted), errors);
	}

	static void ContainsAllRequiredFiles(string currentFiles, StringBuilder errors)
	{
		foreach(var file in MandatoryFiles)
		{
			if (!currentFiles.Contains(file))
			{
				errors.AppendLine(CultureInfo.InvariantCulture, $"Unable to find mandatory file: {file}");
			}
		}
	}

	static string[] MandatoryFiles
		=>
		[
			nameof(TariffOneFiles.nomenclatures),
			nameof(TariffOneFiles.sections),
			nameof(TariffOneFiles.measures),
		];

	public enum TariffOneFiles
	{
		nomenclatures,
		sections,
		measures,
	}
}
