using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	public static class StreamExtensions
	{
		const int bufferSize = 65536;

		public static Stream DecodeAndDecompressPayload(this Stream compresseAndEncodedAndStream)
		{
			var destinationStream = new MemoryStream();

			using (var decodedStream = compresseAndEncodedAndStream.DecodeStream())
			{
				decodedStream.Position = 0;
				decodedStream.ReadByte();  // Read p symbol, otherwise DeflateStream not working
				decodedStream.ReadByte(); // Read z symbol, otherwise DeflateStream not working
				var deflator = new DeflateStream(decodedStream, CompressionMode.Decompress);
				deflator.CopyTo(destinationStream);
			}

			destinationStream.Position = 0;
			return destinationStream;
		}


		static Stream DecodeStream(this Stream source)
		{
			var result = new MemoryStream();
			ICryptoTransform transform = new FromBase64Transform();
			using (var cryptStream = new CryptoStream(source, transform, System.Security.Cryptography.CryptoStreamMode.Read))
			{
				byte[] buffer = new byte[bufferSize];
				int bytesRead;

				while ((bytesRead = cryptStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					result.Write(buffer, 0, bytesRead);
				}

				result.Flush();
				result.Position = 0;
			}

			return result;
		}

		public static Stream ToStream(this string text)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(text);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static void WriteToXmlWriter(this Stream stream, XmlWriter writer)
		{
			using (var reader = new StreamReader(stream))
			{
				char[] buffer = new char[bufferSize];
				int position = reader.Read(buffer, 0, buffer.Length);

				while (position > 0)
				{
					writer.WriteRaw(buffer, 0, position);
					position = reader.Read(buffer, 0, buffer.Length);
				}
			}
		}
	}
}