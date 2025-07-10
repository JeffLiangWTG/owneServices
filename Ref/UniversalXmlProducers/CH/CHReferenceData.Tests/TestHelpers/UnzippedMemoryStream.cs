using System.IO;
using System.IO.Compression;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests;

class UnzippedMemoryStream : MemoryStream
{
	internal UnzippedMemoryStream(Stream source)
	{
		using (var zipArchive = new ZipArchive(source, ZipArchiveMode.Read))
		{
			using var zipEntryStream = zipArchive.Entries[0].Open();
			zipEntryStream.CopyTo(this);
		}
		Position = 0;
	}
}
