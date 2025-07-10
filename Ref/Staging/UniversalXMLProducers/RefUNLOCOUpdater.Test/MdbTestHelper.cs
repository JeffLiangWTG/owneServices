using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	public static class MdbTestHelper
	{
		public static string DownloadMdbFile(string streamPath, string mdbDownloadPath, Type type)
		{
			if (Directory.Exists(mdbDownloadPath))
			{
				Directory.Delete(mdbDownloadPath, true);
			}

			Directory.CreateDirectory(mdbDownloadPath);
			using (var file = type.Assembly.GetManifestResourceStream(streamPath))
			using (var stream = new ZipArchive(file, ZipArchiveMode.Read))
			{
				var mdbFile = stream.Entries.FirstOrDefault(o => o.FullName.EndsWith(".mdb"));
				var tempPath = Path.Combine(mdbDownloadPath, mdbFile.FullName);

				using (var archive = mdbFile.Open())
				using (var fileStream = File.Create(tempPath))
				{
					archive.CopyTo(fileStream);
				}
				return tempPath;
			}
		}

		public static string Url => @"http://www.unece.org/cefact/codesfortrade/codes_index.html";
	}
}
