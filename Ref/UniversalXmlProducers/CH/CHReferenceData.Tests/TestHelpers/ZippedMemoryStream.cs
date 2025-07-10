using System.IO;
using System.IO.Compression;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests;

class ZippedMemoryStream : MemoryStream
{
	internal ZippedMemoryStream(string name, Stream source)
	{
		var buffer = new MemoryStream();
		using (var zipArchive = new ZipArchive(buffer, ZipArchiveMode.Create))
		{
			using var zipEntryStream = zipArchive.CreateEntry(name).Open();
			source.CopyTo(zipEntryStream);
		}
		Write(buffer.ToArray());
	}
}
