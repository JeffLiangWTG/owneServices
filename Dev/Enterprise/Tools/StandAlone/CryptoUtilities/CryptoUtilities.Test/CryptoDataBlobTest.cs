using System;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class CryptoDataBlobTest : NUnit.Framework.TestCase
	{
		public void TestCryptoDataBlob()
		{
			CryptoDataBlob myBlob = new CryptoDataBlob("1.2.840.113549.1.9.5", new DateTime(2001, 02, 23));
			AssertNotNull("MyBlob", myBlob);
			myBlob.Dispose();
			myBlob = new CryptoDataBlob(CryptoAttribute.AttributeType.szOID_RSA_SMIMECapabilities, SMIMECapabilities.GetInstance().CapabilitiesStruct);
			AssertNotNull("MyBlob", myBlob);
			myBlob.Dispose();
		}
		public void TestDispose()
		{
			CryptoDataBlob myBlob = new CryptoDataBlob("1.2.840.113549.1.9.5", new DateTime(2001, 02, 23));
			AssertNotNull("MyBlob", myBlob);
			myBlob.Dispose();
			AssertEquals("MyBlob.BLOBStruct.pbData", IntPtr.Zero, myBlob.BLOBStruct.pbData);
			AssertEquals("MyBlob.BLOBStruct.cData", 0, myBlob.BLOBStruct.cbData);
		}
	}
}
