using Enterprise.CryptoUtilities.SMIME;
using NUnit.Framework;

namespace Enterprise.CMREdifact.CryptoUtilities.SMIME.Testing
{
	sealed class SingleAttachmentMIMEMessageTest : TestCase
	{
		public void TestSingleAttachmentMIMEMessage()
		{
			string filename = "test.edi";
			SingleAttachmentMIMEMessage message = new SingleAttachmentMIMEMessage(filename, new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, "application/edifact; name=" + filename, "attachment; filename=" + filename);
			AssertNotNull("Message", message);
		}

		public void TestRawMIMEString()
		{
			string filename = "test.edi";
			SingleAttachmentMIMEMessage message = new SingleAttachmentMIMEMessage(filename, new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, "application/edifact; name=" + filename, "attachment; filename=" + filename);
			AssertEquals("RawMIMEString", @"Mime-Version: 1.0
Content-Transfer-Encoding: base64
Content-Type: application/edifact; name=test.edi
Content-Disposition: attachment; filename=test.edi

AQIDBAUGBwgJAA==
", message.RawMIMEString);
		}
	}
}
