using System;
using System.IO;
using System.Text;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap
{
	public static class ByteDataPayloadExtensions
	{
		public const string HeaderTemplate = "--uuid:{0}\r\nContent-Id: <{1}@example.jaxws.sun.com>\r\nContent-Type: application/octet-stream\r\nContent-Transfer-Encoding: binary\r\n\r\n";

		public static void WriteMultipartFormData(this byte[] fileBytes, Stream stream, string mimeBoundary, string fileReferenceKey)
		{
			if (fileBytes == null)
			{
				throw new ArgumentNullException(nameof(fileBytes));
			}
			if (fileBytes.Length == 0)
			{
				throw new ArgumentException("File bytes may not be empty.", nameof(fileBytes));
			}
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (mimeBoundary == null)
			{
				throw new ArgumentNullException(nameof(mimeBoundary));
			}
			if (mimeBoundary.Length == 0)
			{
				throw new ArgumentException("MIME boundary may not be empty.", nameof(mimeBoundary));
			}
			if (fileReferenceKey == null)
			{
				throw new ArgumentNullException(nameof(fileReferenceKey));
			}
			if (fileReferenceKey.Length == 0)
			{
				throw new ArgumentException("File reference key may not be empty.", nameof(fileReferenceKey));
			}
			string header = String.Format(System.Globalization.CultureInfo.InvariantCulture, HeaderTemplate, mimeBoundary, fileReferenceKey);
			byte[] headerbytes = Encoding.ASCII.GetBytes(header);
			stream.Write(headerbytes, 0, headerbytes.Length);
			stream.Write(new byte[16], 0, 16); // 16-bit padding
			stream.Write(fileBytes, 0, fileBytes.Length);
			byte[] newlineBytes = Encoding.ASCII.GetBytes("\r\n");
			stream.Write(newlineBytes, 0, newlineBytes.Length);
		}
	}
}
