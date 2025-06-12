using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests
{
	static class EncoderDecoder
	{
		public static Stream CompressAndEncode(Stream source)
		{
			SeekBegin(source);

			var result = Compress(source);
			SeekBegin(result);
			result = EncodeStream(result);
			SeekBegin(result);

			return result;
		}

		public static Stream DecodeAndDecompress(Stream source)
		{
			SeekBegin(source);

			var result = DecodeStream(source);
			SeekBegin(result);
			result = Decompress(result);
			SeekBegin(result);

			return result;
		}

		static Stream Compress(Stream sourceStream)
		{
			var result = new MemoryStream();
			var compressedStream = new GZipStream(result, CompressionMode.Compress, true);
			var buffer = new byte[BufferSize];
			sourceStream.Seek(0, SeekOrigin.Begin);
			long retval = sourceStream.Read(buffer, 0, BufferSize);
			while (retval > 0)
			{
				compressedStream.Write(buffer, 0, (int) retval);
				retval = sourceStream.Read(buffer, 0, BufferSize);
			}
			compressedStream.Close();

			return result;
		}

		static Stream Decompress(Stream sourceStream)
		{
			var result = new MemoryStream();
			var buffer = new byte[BufferSize];

			var decompressedStream = new GZipStream(sourceStream, CompressionMode.Decompress, true);
			long retval = decompressedStream.Read(buffer, 0, BufferSize);
			while (retval == BufferSize)
			{
				result.Write(buffer, 0, (int) retval);
				result.Flush();
				retval = decompressedStream.Read(buffer, 0, BufferSize);
			}
			result.Write(buffer, 0, (int) retval);
			result.Flush();
			result.Position = 0;
			decompressedStream.Close();

			return result;
		}

		static Stream EncodeStream(Stream source)
		{
			var result = new MemoryStream();
			var transform = new ToBase64Transform();
			var cryptStream = new CryptoStream(result, transform, CryptoStreamMode.Write);

			var buffer = new byte[BufferSize];
			int bytesRead;

			while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
			{
				cryptStream.Write(buffer, 0, bytesRead);
			}
			cryptStream.FlushFinalBlock();

			return result;
		}

		static Stream DecodeStream(Stream source)
		{
			var result = new MemoryStream();
			var transform = new FromBase64Transform();
			var cryptStream = new CryptoStream(source, transform, CryptoStreamMode.Read);

			var buffer = new byte[BufferSize];
			int bytesRead;

			while ((bytesRead = cryptStream.Read(buffer, 0, buffer.Length)) > 0)
			{
				result.Write(buffer, 0, bytesRead);
			}
			result.Flush();

			return result;
		}

		static void SeekBegin(Stream source)
		{
			source.Seek(0, SeekOrigin.Begin);
		}

		const int BufferSize = 4096;
	}
}