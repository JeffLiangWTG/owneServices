using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Testing
{
	sealed class DictionaryExtensionsTest : TestCase
	{
		public void TestWriteMultipartFormData()
		{
			using (var ms = new MemoryStream())
			{
				var boundary = "BOUNDARY";
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("Name", "Daniel");
				dictionary.Add("Eyes", "Blue");
				dictionary.WriteMultipartFormData(ms, boundary);
				ms.Position = 0;
				var readBack = new StreamReader(ms).ReadToEnd();
				AssertEquals(@"--uuid:BOUNDARY
Content-Id: <rootpart*BOUNDARY@example.jaxws.sun.com>
Content-Type: application/xop+xml; charset=utf-8;type=""text/xml""
Content-Transfer-Encoding: binary

Daniel
--uuid:BOUNDARY
Content-Id: <rootpart*BOUNDARY@example.jaxws.sun.com>
Content-Type: application/xop+xml; charset=utf-8;type=""text/xml""
Content-Transfer-Encoding: binary

Blue
", readBack);
			}
		}
	}
}
