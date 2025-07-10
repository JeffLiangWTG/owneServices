using System.Globalization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Unity;
using Unity.Resolution;
using Application = CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration.Application;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Utilities
{
	public class FileProcessor : IFileProcessor
	{
		public required string[] FoldersToScan { get; set; }
		public required string FileExtensionToMonitor { get; set; }
		public required string ArchiveFolder { get; set; }
		public required string ErrorFolder { get; set; }
		public int OverdueMonths { get; set; }
		public required string LastRunTimeFile { get; set; }

		//1. ForceParse folder has higher priority than UxmlFiles folder
		//2. Only one xml file will be parsed in one run even when it fails, except for IOException (Requirement of WI00905517)
		public async Task ScanFolders()
		{
			foreach (var folderToScan in FoldersToScan)
			{
				Console.WriteLine($"Scanning {folderToScan} for {FileExtensionToMonitor} files");
				var hasFileParsed = false;

				try
				{
					if (!Directory.Exists(folderToScan))
					{
						Console.WriteLine($"Folder does not exists. Creating folder to watch for incoming {FileExtensionToMonitor} files.");
						Directory.CreateDirectory(folderToScan);
					}

					var currentFilesInDirectory = Directory.GetFiles(folderToScan, FileExtensionToMonitor);
					foreach (var file in currentFilesInDirectory)
					{
						if (await RunUniversalXmlParser(file))
						{
							hasFileParsed = true;
							break;
						}
					}
					if (hasFileParsed)
					{
						break;
					}
				}
				catch (UnauthorizedAccessException ex)
				{
					Console.Error.WriteLine($"Unauthorize creating directory {folderToScan}.");
					Console.Error.WriteLine(ex);
				}
				catch (IOException ex)
				{
					Console.Error.WriteLine($"Cannot get files from {folderToScan}.");
					Console.Error.WriteLine(ex);
					continue;
				}
			}
		}

		public void DeleteOverdueFiles()
		{
			var folderList = new[] { ArchiveFolder, ErrorFolder };
			for (int i = 0; i < folderList.Length; i++)
			{
				var path = folderList[i];
				if (!Directory.Exists(path))
				{
					Console.WriteLine($"Folder {path} does not exist.");
					return;
				}

				if (!File.Exists(LastRunTimeFile))
				{
					File.Create(LastRunTimeFile).Dispose();
				}

				if (ShouldDeleteFiles(LastRunTimeFile))
				{
					var dayOfLastYear = DateTime.UtcNow.AddMonths(-OverdueMonths);
					var fileArray = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

					foreach (var file in fileArray)
					{
						var fileInfo = new FileInfo(file);
						var time = fileInfo.LastWriteTimeUtc;
						if (time < dayOfLastYear)
						{
							DeleteFile(file);
						}
					}

					if (i == folderList.Length - 1)
					{
						using (var stream = File.Open(LastRunTimeFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
						using (StreamWriter writer = new StreamWriter(stream))
						{
							writer.Write(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
							writer.Flush();
						}
					}
				}
			}
		}

		public static bool ShouldDeleteFiles(string filePath)
		{
			var lastRunTime = File.GetLastWriteTimeUtc(filePath);
			if (DateTime.UtcNow.Subtract(lastRunTime).TotalDays > 30)
			{
				return true;
			}
			return false;
		}

		static void DeleteFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		static string MoveFile(string fileName, string folder, string dataSource)
		{
			Argument.NotNullOrEmpty(fileName, nameof(fileName));
			Argument.NotNullOrEmpty(folder, nameof(folder));

			var fileCompressor = Application.UnityContainer.Resolve<IFileCompressor>();

			if (!string.IsNullOrEmpty(dataSource))
			{
				foreach (var invalidChar in Path.GetInvalidFileNameChars())
				{
					dataSource = dataSource.Replace(invalidChar, ' ');
				}
				folder = Path.Combine(folder, dataSource);
			}
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			var destinationFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMddHHmmss}.zip";
			var destination = Path.Combine(folder, destinationFileName);

			if (File.Exists(destination))
			{
				File.Delete(destination);
			}
			bool compressResult = fileCompressor.CompressFile(fileName, destination);
			if (compressResult)
			{
				DeleteFile(fileName);
			}

			return destination;
		}

		async Task<bool> RunUniversalXmlParser(string fileName)
		{
			var containerFactory = Application.UnityContainer.Resolve<IContainerFactory>();
			using var innerContainer = containerFactory.Create(fileName);

			try
			{
				var sourceData = CreateSourceData(fileName);
				var sourceDataWriter = innerContainer.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", sourceData));
				var parser = innerContainer.Resolve<IUniversalXmlParser>(new ParameterOverride("universalXmlSourceDataWriter", sourceDataWriter));
				var isForceParsingRequired = IsForceParsingRequired(fileName);

				using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.AppContext)}:{sourceData.SDA_PK}");
					Console.WriteLine($"Parser starting to process {fileName}");
					await parser.ParseAsync(stream, isForceParsingRequired);
				}
				var processedFile = MoveFile(fileName, ArchiveFolder, sourceData.SDA_SubSource);
				if (!string.IsNullOrEmpty(processedFile))
				{
					Console.WriteLine($"Processed File {fileName} moved to {processedFile}");
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				Console.Error.WriteLine($"Unauthorized while parsing {fileName}.");
				Console.Error.WriteLine(ex);
			}
			catch (IOException ex)
			{
				const int ERROR_SHARING_VIOLATION = 0x20;
				const int ERROR_LOCK_VIOLATION = 0x21;
				int errorCode = ex.HResult & 0x0000FFFF;
				if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION)
				{
					Console.WriteLine($"Parsing canceled because {fileName} is being used by another process.");
				}
				else
				{
					Console.Error.WriteLine($"IOException occurred while parsing {fileName}.");
					Console.Error.WriteLine(ex);
				}
				return false;
			}
			catch
			{
				Console.Error.WriteLine($"Parsing Failed for {fileName}.");

				var errorFile = MoveFile(fileName, ErrorFolder, string.Empty);
				if (!string.IsNullOrEmpty(errorFile))
				{
					Console.Error.WriteLine($"Error File {fileName} moved to {errorFile}");
				}
				throw;
			}
			return true;
		}

		static SourceData CreateSourceData(string fileName)
		{
			return new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filename = fileName,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_ContentText = string.Empty,
				SDA_Status = StatusProvider.GetQUEStatus(),
				SDA_SourceTime = DateTime.Now,
				SDA_SubSource = string.Empty
			};
		}

		static bool IsForceParsingRequired(string fileName)
		{
			Argument.NotNullOrEmpty(fileName, nameof(fileName));

			var fileWatcherConfig = Application.UnityContainer.Resolve<IFileProcessorConfig>();

			return fileName.Contains(fileWatcherConfig.IncomingXmlFilesForceParseDir);
		}
	}
}
