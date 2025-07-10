using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Testing
{
	sealed class ByteDataPayloadExtensionsTest : TestCase
	{
		public void TestWriteMultipartFormData()
		{
			using (var ms = new MemoryStream())
			{
				var bytesToWrite = Encoding.ASCII.GetBytes("This is what we are going to upload");
				var boundary = "BOUNDARY";
				var fileKey = "FILEKEY";
				bytesToWrite.WriteMultipartFormData(ms, boundary, fileKey);
				ms.Position = 0;
				var bytesBack = ms.GetBuffer();
				using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.SOAP_MTOM.TestFiles.ByteDataPayloadExtensions.txt"))
				using (var br = new BinaryReader(stream))
				{
					var expectedBytes = br.ReadBytes((int)stream.Length);
					AssertEquals(expectedBytes, bytesBack);
				}
			}
			/*
				The content of the expected file, IGNORING the sixteen NUL (ascii zero) bytes before the word "This", looks like this:

	--uuid:BOUNDARY
	Content-Id: <FILEKEY@example.jaxws.sun.com>
	Content-Type: application/octet-stream
	Content-Transfer-Encoding: binary

					This is what we are going to upload

				*/
		}
	}
}
