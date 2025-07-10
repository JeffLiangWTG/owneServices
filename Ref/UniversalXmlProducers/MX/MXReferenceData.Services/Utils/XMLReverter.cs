using System;
using System.IO;
using System.IO.Compression;

namespace CargoWise.RefDbRepo.MXReferenceData.Services
{
	public static class XMLReverter
	{
		public static string RevertAndUnzipXML(Stream streamRead)
		{
			using (var archive = new ZipArchive(streamRead))
			{
				var fileEntry = archive.Entries[0];

				if (fileEntry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
				{
					using (var stream = new StreamReader(fileEntry.Open()))
					{
						return stream.ReadToEnd();
					}
				}
			}

			return string.Empty;
		}
	}
}
