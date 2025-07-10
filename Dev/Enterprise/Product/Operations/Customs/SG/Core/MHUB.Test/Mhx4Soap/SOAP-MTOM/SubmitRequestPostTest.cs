using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Testing
{
	sealed class SubmitRequestPostTest : TestCaseWithFactory
	{
		public void TestExecutePostRequestRetrievePayload()
		{
			var fakedSubmitRequestPostForTest = new SubmitRequestPostForTest();
			fakedSubmitRequestPostForTest.CannedDataToSendBack = cannedReplyOMG;
			var pairs = new Dictionary<string, string>();
			pairs.Add("A", "aaaaaa");
			pairs.Add("B", "bbbbbbb");
			var actualReplyBytes = fakedSubmitRequestPostForTest.ExecutePostRequestRetrievePayload(new Uri("https://www.fake.com"), pairs, Encoding.ASCII.GetBytes("I am your payload"), "ROOTME", new CookieContainer(), WebRequest.DefaultWebProxy);
			var actualReply = Encoding.ASCII.GetString(actualReplyBytes);
			AssertEquals("OMG", actualReply);
			var whatWeSentToServer = Encoding.ASCII.GetString(fakedSubmitRequestPostForTest.Wrapper.MemoryStreamRequest.GetBuffer());
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.SOAP_MTOM.TestFiles.SubmitRequestPost.RetrieveRequest.txt"))
			{
				var expectedUpload = new StreamReader(stream).ReadToEnd();
				AssertEquals(@"Expected vs actual upload stream. If you need to edit the content of the expected file, DO NOT edit it as text. There are hidden characters. Instead do this: File.WriteAllBytes('path\to\SubmitRequestPost.TestFile.RetrieveRequest.txt', fakedSubmitRequestPostForTest.Wrapper.MemoryStreamRequest.GetBuffer()). Then rebuild to re-embed.", expectedUpload, whatWeSentToServer);
			}
		}

		public void TestExecutePostRequestSubmitPayload()
		{
			var cannedReplyUploadWasOk = @"--uuid:DELIMITMEBABY
Content-Id: <rootpart*DELIMITMEBABY@example.jaxws.sun.com>
Content-Type: application/xop+xml;charset=utf-8;type=""text/xml""
Content-Transfer-Encoding: binary

<?xml version='1.0' encoding='UTF-8'?><S:Envelope xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/""><S:Body><ns2:uploadResponse xmlns:ns2=""http://service.wsvc.mhb.crimsonlogic.com/wsdl""><responseCode>0</responseCode><uuid>pyn3s/5KplvWYmwAG6yhUXc5Yt9HROVU0dAlClqhonyGvw+0ze8nAbufPQxYhYs/</uuid></ns2:uploadResponse></S:Body></S:Envelope>

--uuid:DELIMITMEBABY--";
			var fakedSubmitRequestPostForTest = new SubmitRequestPostForTest();
			fakedSubmitRequestPostForTest.CannedDataToSendBack = cannedReplyUploadWasOk;
			var pairs = new Dictionary<string, string>();
			pairs.Add("A", "<xml_SOAP_request_goes_here/>");
			var actualReply = fakedSubmitRequestPostForTest.ExecutePostRequestSubmitPayload(new Uri("https://www.fake.com"), pairs, Encoding.ASCII.GetBytes("I am your payload"), "ROOTME", new CookieContainer(), WebRequest.DefaultWebProxy);
			AssertEquals(cannedReplyUploadWasOk, actualReply);
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.SOAP_MTOM.TestFiles.SubmitRequestPost.SubmitRequest.txt"))
			using (var br = new BinaryReader(stream))
			{
				var expectedBytes = br.ReadBytes((int)stream.Length);
				AssertEquals(@"Expected vs actual upload stream. If you need to edit the content of the expected file, DO NOT edit it as text. There are hidden characters. Instead do this: File.WriteAllBytes('path\to\SubmitRequestPost.TestFile.SubmitRequest.txt', fakedSubmitRequestPostForTest.Wrapper.MemoryStreamRequest.GetBuffer()). Then rebuild to re-embed.", expectedBytes, fakedSubmitRequestPostForTest.Wrapper.MemoryStreamRequest.GetBuffer());
			}
		}

		internal const string cannedReplyOMG = @"--uuid:c025c412-aeea-4338-86a1-447c5788f4ef
Content-Id: <rootpart*c025c412-aeea-4338-86a1-447c5788f4ef@example.jaxws.sun.com>
Content-Type: application/xop+xml;charset=utf-8;type=""text/xml""
Content-Transfer-Encoding: binary

<?xml version='1.0' encoding='UTF-8'?><S:Envelope xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/""><S:Body><ns2:downloadResponse xmlns:ns2=""http://service.wsvc.mhb.crimsonlogic.com/wsdl""><responseCode>0</responseCode><uuid>RP+5ExZJp4y7zPtERU23x1UXd75K/OuTy5rIc06gx/kT1D9kjxEIj+TVXWNBKWGv</uuid><payload><xop:Include xmlns:xop=""http://www.w3.org/2004/08/xop/include"" href=""cid:dcb65df7-c4c1-43b0-a805-1fafa00dc497@example.jaxws.sun.com""/></payload></ns2:downloadResponse></S:Body></S:Envelope>
--uuid:c025c412-aeea-4338-86a1-447c5788f4ef
Content-Id: <dcb65df7-c4c1-43b0-a805-1fafa00dc497@example.jaxws.sun.com>
Content-Type: application/octet-stream
Content-Transfer-Encoding: binary

                OMG
--uuid:c025c412-aeea-4338-86a1-447c5788f4ef--";
	}
}
