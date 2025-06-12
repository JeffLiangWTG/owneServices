using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;
using CargoWise.Billing.CollectorService.Plugin;
using ICSharpCode.SharpZipLib.Zip;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common
{
	public static class StreamExtensions
	{
		public const int BufferSize = 8192;
		const int ZipHeaderBytes = 0x04034b50;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream Compress(this Stream sourceStream)
		{
			var result = new VirtualStream();
			var compressedStream = new GZipStream(result, CompressionMode.Compress, true);
			int bufferSize = 8192;
			var buffer = new byte[bufferSize];
			sourceStream.Seek(0, SeekOrigin.Begin);
			long retval = sourceStream.Read(buffer, 0, bufferSize);
			while (retval > 0)
			{
				compressedStream.Write(buffer, 0, (int)retval);
				retval = sourceStream.Read(buffer, 0, bufferSize);
			}
			compressedStream.Close();

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream DecompressGZipStream(this Stream sourceStream)
		{
			var result = new VirtualStream();
			const int bufferSize = 8192;
			var buffer = new byte[bufferSize];

			var decompressedStream = new GZipStream(sourceStream, CompressionMode.Decompress, true);
			result = Decompress(bufferSize, buffer, decompressedStream);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream DecompressZipStream(this Stream sourceStream)
		{
			var result = new VirtualStream();
			const int bufferSize = 4096;

			var buffer = new byte[bufferSize];

			var zipInputStream = new ZipInputStream(sourceStream);
			var zipEntry = zipInputStream.GetNextEntry();
			result = Decompress(bufferSize, buffer, zipInputStream);

			return result;
		}


		static VirtualStream Decompress(int bufferSize, byte[] buffer, Stream stream)
		{
			var result = new VirtualStream();
			long retval = stream.Read(buffer, 0, bufferSize);
			while (retval == bufferSize)
			{
				result.Write(buffer, 0, (int)retval);
				result.Flush();
				retval = stream.Read(buffer, 0, bufferSize);
			}

			result.Write(buffer, 0, (int)retval);
			result.Flush();
			result.Position = 0;
			stream.Close();

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream EncodeStream(this Stream source)
		{
			var result = new VirtualStream();
			ICryptoTransform transform = new ToBase64Transform();
			CryptoStream cryptStream = new CryptoStream(result, transform, CryptoStreamMode.Write);

			byte[] buffer = new byte[BufferSize];
			int bytesRead;

			while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
			{
				cryptStream.Write(buffer, 0, bytesRead);
			}
			cryptStream.FlushFinalBlock();

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream DecodeStream(this Stream source)
		{
			var result = new VirtualStream();
			using (var transform = new FromBase64Transform())
			{
				var cryptStream = new CryptoStream(source, transform, System.Security.Cryptography.CryptoStreamMode.Read);

				byte[] buffer = new byte[BufferSize];
				int bytesRead;

				while ((bytesRead = cryptStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					result.Write(buffer, 0, bytesRead);
				}
				result.Flush();
			}
			return result;
		}

		public static Stream CompressAndEncode(this Stream source)
		{
			source.SeekBegin();
			using (var compressedStream = source.Compress())
			{
				compressedStream.SeekBegin();
				var result = compressedStream.EncodeStream();
				result.SeekBegin();
				return result;
			}
		}

		static bool IsZipCompressedData(byte[] data)
		{
			return data != null && data.Length >= 4 && BitConverter.ToInt32(data, 0) == ZipHeaderBytes;
		}


		public static Stream DecodeAndDecompress(this Stream source)
		{
			Stream result;
			source.SeekBegin();

			using (var decodedStream = source.DecodeStream())
			{
				result = IsZipCompressedData(decodedStream.ReadBytes(4)) ? decodedStream.DecompressZipStream() : decodedStream.DecompressGZipStream();
			}

			result.SeekBegin();
			return result;
		}

		public static byte[] ReadBytes(this Stream stream, int count)
		{
			stream.SeekBegin();

			byte[] bytes = new byte[count];
			int offset = 0;

			while (offset < count)
			{
				int read = stream.Read(bytes, offset, count - offset);
				if (read == 0) break;
				offset += read;
			}

			stream.SeekBegin();
			return bytes;
		}

		public static void WriteTo(this Stream source, Stream output)
		{
			int bytesRead = 0;

			byte[] bytes = new byte[BufferSize];
			do
			{
				bytesRead = source.Read(bytes, 0, BufferSize);
				output.Write(bytes, 0, bytesRead);
			} while (bytesRead > 0);

			output.Flush();
		}

		public static void WriteToXmlWriter(this TextReader reader, XmlWriter writer)
		{
			char[] buffer = new char[BufferSize];
			int position = reader.Read(buffer, 0, buffer.Length);
			while (position > 0)
			{
				writer.WriteRaw(buffer, 0, position);
				position = reader.Read(buffer, 0, buffer.Length);
			}
		}

		public static void WriteToStream(this XmlReader reader, Stream stream)
		{
			var writer = new StreamWriter(stream);
			char[] buffer = new char[BufferSize];
			int position = reader.ReadValueChunk(buffer, 0, buffer.Length);
			while (position > 0)
			{
				writer.Write(buffer, 0, position);
				position = reader.ReadValueChunk(buffer, 0, buffer.Length);
			}
			writer.Flush();
		}

		public static void SeekBegin(this Stream source)
		{
			source.Seek(0, SeekOrigin.Begin);
		}

		public static string ReadToEnd(this Stream source)
		{
			source.SeekBegin();
			var reader = new StreamReader(source);
			return reader.ReadToEnd();
		}

		public static long GetLengthEx(this Stream source)
		{
			long len = 0L;

			try
			{
				len = source.Length;
			}
			catch (Exception) { }

			return len;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static Stream ReplaceTextBlock(this Stream stream, string startBlockString, string endBlockString, string replaceString, int bufferSize = BufferSize)
		{
			var resultStream = new VirtualStream();
			stream.Position = 0;
			var reader = new StreamReader(stream);
			var writer = new StreamWriter(resultStream);

			if (bufferSize < startBlockString.Length) bufferSize = startBlockString.Length + 1;
			if (bufferSize < endBlockString.Length) bufferSize = startBlockString.Length + 1;

			char[] buffer1 = new char[bufferSize];
			char[] buffer2 = new char[bufferSize];

			int read = reader.Read(buffer1, 0, buffer1.Length);
			if (read != buffer1.Length) Array.Resize<char>(ref buffer1, read);
			bool IsStartBlockFound = false;

			while (true)
			{
				buffer2 = new char[bufferSize];
				read = reader.Read(buffer2, 0, buffer2.Length);
				int buffer2Length = buffer2.Length;
				if (read != buffer2.Length) Array.Resize<char>(ref buffer2, read);

				string sumString = new string(buffer1) + new string(buffer2);

				if (IsStartBlockFound)
				{
					int endSkipIndex = sumString.IndexOf(endBlockString, StringComparison.InvariantCulture);
					if (endSkipIndex != -1)
					{
						writer.Write(replaceString);
						buffer1 = sumString.Substring(endSkipIndex + endBlockString.Length).ToCharArray();
						IsStartBlockFound = false;
						continue;
					}
				}
				else
				{
					int startBlockIndex = sumString.IndexOf(startBlockString, StringComparison.InvariantCulture);
					if (startBlockIndex != -1)
					{
						IsStartBlockFound = true;
						writer.Write(sumString.ToCharArray(), 0, startBlockIndex);
						buffer1 = sumString.Substring(startBlockIndex + startBlockString.Length).ToCharArray();
						continue;
					}

					writer.Write(buffer1);

				}

				buffer1 = buffer2;
				if (read == 0 || read != buffer2Length) break;
			}

			writer.Write(buffer2);
			writer.Flush();

			resultStream.Position = 0;
			return resultStream;
		}
	}
}


