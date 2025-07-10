using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.NewService
{
	public static class CompressionHelper
	{
		public static CompressionType GetCompressionType(HttpRequest request)
		{
			var compressionType = CompressionType.None;

			var acceptEncoding = request?.Headers?.AcceptEncoding;

			if (acceptEncoding.HasValue)
			{
				foreach (var headerValue in request.Headers.AcceptEncoding)
				{
					if (headerValue != null)
					{
						if (headerValue.Contains(CompressionType.GZip.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							compressionType = CompressionType.GZip;
							break;
						}
						else if (headerValue.Contains(CompressionType.Deflate.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							compressionType = CompressionType.Deflate;
							break;
						}
					}
				}
			}

			return compressionType;
		}

		public static Stream SetupCompressedStream(Stream stream, CompressionType compressionType)
		{
			switch (compressionType)
			{
				case CompressionType.GZip:
					return new GZipStream(stream, CompressionMode.Compress);

				case CompressionType.Deflate:
					return new DeflateStream(stream, CompressionMode.Compress);

				default:
					return stream;
			}
		}
	}
}
