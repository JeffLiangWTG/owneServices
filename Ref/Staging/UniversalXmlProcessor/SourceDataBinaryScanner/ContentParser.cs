using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public class ContentParser : IContentParser
	{
		public async Task CompressedContent(SourceData sourceData, IUniversalXmlParser uxmlParser, IStagingRepository stagingRepository)
		{
			try
			{
				if (sourceData.SDA_Filename.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
				{
					using (var zipArchive = new ZipArchive(new MemoryStream(sourceData.SDA_Content), ZipArchiveMode.Read))
					{
						var xmlFile = zipArchive.Entries.FirstOrDefault(o => o.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
						if (xmlFile == null)
						{
							Console.Error.WriteLine($"No XML file found in the zip archive: {sourceData.SDA_Filename}");
							return;
						}

						var tempFilePath = Path.GetTempFileName();
						try
						{
							await using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
							await using (var xmlStream = xmlFile.Open())
							{
								await xmlStream.CopyToAsync(tempFileStream);
							}

							await using (var tempFileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
							{
								await uxmlParser.ParseAsync(tempFileStream, false);
							}
						}
						finally
						{
							if (File.Exists(tempFilePath))
							{
								File.Delete(tempFilePath);
							}
						}
					}
				}
				else if (sourceData.SDA_Filename.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
				{
					var tempFilePath = Path.GetTempFileName();
					try
					{
						await using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
						await using (var gzipFile = new GZipStream(new MemoryStream(sourceData.SDA_Content), CompressionMode.Decompress, true))
						{
							await gzipFile.CopyToAsync(tempFileStream);
						}

						// Process the temporary file
						await using (var tempFileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
						{
							await uxmlParser.ParseAsync(tempFileStream, false);
						}
					}
					finally
					{
						if (File.Exists(tempFilePath))
						{
							File.Delete(tempFilePath);
						}
					}
				}
			}
			catch
			{
				Console.Error.WriteLine($"Parsing Failed for uploaded file {sourceData.SDA_Filename}.");
				throw;
			}
		}

		public async Task XmlContent(SourceData sourceData, IUniversalXmlParser uxmlParser, IStagingRepository stagingRepository)
		{
			try
			{
				var stream = new MemoryStream(sourceData.SDA_Content);
				await uxmlParser.ParseAsync(stream, false);
			}
			catch
			{
				Console.Error.WriteLine($"Parsing Failed for uploaded file {sourceData.SDA_Filename}.");
				throw;
			}
		}
	}
}
