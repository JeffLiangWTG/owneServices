using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Shared.Mime
{
	public static class MimeUtils
	{
		public static Dictionary<string, string> ParseHeaders(string message)
		{
			using (var messageStream = new MemoryStream(Encoding.Default.GetBytes(message)))
				return ParseHeaders(messageStream);
		}

		public static Dictionary<string, string> ParseHeaders(Stream message)
		{
			var mimeKitHeaders = MimeKit.HeaderList.Load(message);
			return mimeKitHeaders.ToDictionary<MimeKit.Header, string, string>(h => h.Field, h => h.Value, StringComparer.OrdinalIgnoreCase);
		}

		public static Stream FormatHeaders(Dictionary<string, string> headers)
		{
			var mimeKitHeaders = new MimeKit.HeaderList();
			foreach (var header in headers)
				mimeKitHeaders.Add(header.Key, header.Value);
			var headersStream = new MemoryStream();
			mimeKitHeaders.WriteTo(headersStream);
			headersStream.Position = 0;
			return headersStream;
		}

		public static string FormatHeaders(Dictionary<string, string> headers, Encoding encoding)
		{
			return encoding.GetString(((MemoryStream)FormatHeaders(headers)).ToArray());
		}

		public static Stream ExtractHeaders(Stream message)
		{
			byte[] split = new byte[] { 0x0D, 0x0A, 0x0D, 0x0A };
			byte[] readBuffer = new byte[split.Length];
			byte[] writeBuffer = new byte[4096];
			int next, writeCount;

			writeCount = message.Read(readBuffer, 1, readBuffer.Length - 1);
			Buffer.BlockCopy(readBuffer, 1, writeBuffer, 0, writeCount);

			MemoryStream headerStream = new MemoryStream();
			Action writeBufferToHeaderStream = () => { headerStream.Write(writeBuffer, 0, writeCount); writeCount = 0; };

			while ((next = message.ReadByte()) != -1)
			{
				byte nextByte = (byte)next;

				int i;
				for (i = 0; i < split.Length - 1; i++)
					readBuffer[i] = readBuffer[i + 1];
				readBuffer[i] = nextByte;
				writeBuffer[writeCount++] = nextByte;
				if (writeCount == writeBuffer.Length)
					writeBufferToHeaderStream();

				for (i = 0; i < split.Length && readBuffer[i] == split[i]; i++) { }
				if (i == split.Length)
					break;
			}

			writeBufferToHeaderStream();
			headerStream.Position = 0;
			return headerStream;
		}
	}
}
