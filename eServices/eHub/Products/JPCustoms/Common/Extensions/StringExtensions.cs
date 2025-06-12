using System.IO;
using System.Text;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Products.JPCustoms.Common.Extensions
{
	public static class StringExtensions
	{
		public static string DecodeAndDecompress(this string text)
		{
			using (var encodedStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				using (var stream = encodedStream.DecodeAndDecompress())
				{
					return stream.ReadToEnd();
				}
			}
		}

		public static string CompressAndEncode(this string text)
		{
			using (var plainTextStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				using (var compressAndEncodedStream = plainTextStream.CompressAndEncode())
				{
					return compressAndEncodedStream.ReadToEnd();
				}
			}
		}

		public static string TruncateForLogging(this string message)
		{
			if (string.IsNullOrEmpty(message)) return string.Empty;
			return message.Length > 200 ? message.Substring(0, 200) : message;
		}

	}
}
