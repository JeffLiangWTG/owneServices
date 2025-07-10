using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.NewService.Test
{
	public static class HttpContextExtensions
	{
		public static void SetBody(this HttpRequest request, string contentStr)
		{
			var content = Encoding.UTF8.GetBytes(contentStr);
			var requestBodyStream = new MemoryStream();
			requestBodyStream.Seek(0, SeekOrigin.Begin);
			requestBodyStream.Write(content, 0, content.Length);
			request.Body = requestBodyStream;
			request.Body.Seek(0, SeekOrigin.Begin);
		}

		public static string GetBody(this HttpResponse response)
		{
			var compressionType = response.GetCompressionType();
			switch (compressionType)
			{
				case CompressionType.GZip:
					using (var gZipStream = new GZipStream(response.Body, CompressionMode.Decompress))
					using (var reader = new StreamReader(gZipStream))
					{
						return reader.ReadToEnd();
					}
				case CompressionType.Deflate:
					using (var deflateStream = new DeflateStream(response.Body, CompressionMode.Decompress))
					using (var reader = new StreamReader(deflateStream))
					{
						return reader.ReadToEnd();
					}
				case CompressionType.None:
				default:
					using (var reader = new StreamReader(response.Body))
					{
						return reader.ReadToEnd();
					}
			}
		}

		public static CompressionType GetCompressionType(this HttpResponse response)
		{
			var encoding = response.Headers.ContentEncoding.ToString();
			if (!string.IsNullOrEmpty(encoding))
			{
				if (string.Equals(CompressionType.GZip.ToString(), encoding, StringComparison.OrdinalIgnoreCase))
				{
					return CompressionType.GZip;
				}
				if(string.Equals(CompressionType.Deflate.ToString(), encoding, StringComparison.OrdinalIgnoreCase))
				{
					return CompressionType.Deflate;
				}
			}
			return CompressionType.None;
		}
	}
}
