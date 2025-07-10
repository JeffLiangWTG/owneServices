using System;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public static class GZipHelper
	{
		public static void UnzipFile(string compressedFile, string outputFile)
		{
			using (var inputStream = new FileStream(compressedFile, FileMode.Open, FileAccess.Read))
			using (var zipStream = new GZipInputStream(inputStream))
			using (var outputStream = File.Create(outputFile))
			{
				zipStream.CopyTo(outputStream);
			}
		}

		public static void UnzipFolder(string compressedFile, string destDir)
		{
			try
			{
				var compressedData = File.ReadAllBytes(compressedFile);

				using (var memoryStream = new MemoryStream(compressedData, writable: false))
				using (var gzipStream = new GZipInputStream(memoryStream))
				using (var tarStream = TarArchive.CreateInputTarArchive(gzipStream, System.Text.Encoding.Default))
				{
					tarStream.ExtractContents(destDir);
				}
			}
			catch (SharpZipBaseException ex)
			{
				throw new InvalidOperationException($"Unable to decompress file[{compressedFile}] with error - {ex.Message}.");
			}
		}
	}
}
