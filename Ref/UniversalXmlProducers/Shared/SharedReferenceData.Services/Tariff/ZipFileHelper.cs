using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff
{
	public abstract class ZipFileHelper
	{
		const int ZIP_LEAD_BYTES = 0x04034b50;
		const ushort GZIP_LEAD_BYTES = 0x8b1f;

		static bool IsPkZipCompressedData(byte[] data)
		{
			// if the first 4 bytes of the array are the ZIP signature then it is compressed data
			return BitConverter.ToInt32(data, 0) == ZIP_LEAD_BYTES;
		}

		static bool IsGZipCompressedData(byte[] data)
		{
			// if the first 2 bytes of the array are theG ZIP signature then it is compressed data;
			return BitConverter.ToUInt16(data, 0) == GZIP_LEAD_BYTES;
		}

		public static ZipFileHelper GetZipHelper(string compressedFile)
		{
			if (!File.Exists(compressedFile))
			{
				throw new FileNotFoundException($"Zip file '{compressedFile}' does not exist");
			}

			try
			{
				using (var inputStream = new FileStream(compressedFile, FileMode.Open, FileAccess.Read))
				{
					var leadingBytes = new byte[4];

					inputStream.Read(leadingBytes, 0, 4);

					if (IsPkZipCompressedData(leadingBytes))
					{
						return new PKZipHelper();
					}

					if (IsGZipCompressedData(leadingBytes))
					{
						return new GZipHelper();
					}

					throw new ZipException("Unknown Zip format");
				}
			}
			catch (Exception ex)
			{
				throw new ZipException($"Failed to determine ZipHelper for {compressedFile}", ex);
			}
		}

		public abstract void UnzipFile(string compressedFile, string outputPath);
	}

	internal class PKZipHelper : ZipFileHelper
	{
		public override void UnzipFile(string compressedFile, string outputPath)
		{
			var fastZip = new FastZip();

			fastZip.ExtractZip(compressedFile, outputPath, string.Empty);
		}
	}

	class GZipHelper : ZipFileHelper
	{
		public override void UnzipFile(string compressedFile, string outputPath)
		{
			var outputFile = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(compressedFile) + ".xml");

			using (var inputStream = new FileStream(compressedFile, FileMode.Open, FileAccess.Read))
			using (var zipStream = new GZipInputStream(inputStream))
			using (var outputStream = File.Create(outputFile))
			{
				zipStream.CopyTo(outputStream);
			}
		}
	}
}
