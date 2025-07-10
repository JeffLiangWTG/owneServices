using System;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class CryptoAttributeTest : NUnit.Framework.TestCase
	{
		public void TestCryptoAttribute()
		{
			CryptoAttribute myAttribute = new CryptoAttribute(CryptoAttribute.AttributeType.szOID_RSA_signingTime, new DateTime(2003, 4, 29));
			AssertNotNull("MyAttribute", myAttribute);
			myAttribute.Dispose();
		}

		public void TestDispose()
		{
			CryptoAttribute myAttribute = new CryptoAttribute(CryptoAttribute.AttributeType.szOID_RSA_signingTime, new DateTime(2003, 4, 29));
			myAttribute.Dispose();
			AssertEquals("ContextPointer", IntPtr.Zero, myAttribute.AttributeStruct.rgValue);
		}
	}
}
