using System;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class CryptoAttributeCollectionTest : NUnit.Framework.TestCase
	{
		public void TestCryptoAttributeCollection()
		{
			CryptoAttributeCollection myCollection = new CryptoAttributeCollection(new CryptoAttribute(CryptoAttribute.AttributeType.szOID_RSA_signingTime, DateTime.Now));
			AssertNotNull("MyCollection", myCollection);
			myCollection.Dispose();
		}

		public void TestDispose()
		{
			CryptoAttributeCollection myCollection = new CryptoAttributeCollection(new CryptoAttribute(CryptoAttribute.AttributeType.szOID_RSA_signingTime, DateTime.Now));
			myCollection.Dispose();
			AssertEquals("AttributePointerArray", IntPtr.Zero, myCollection.AttributePointerArray);
			AssertEquals("AttributePointerArrayCount", 0, myCollection.AttributePointerArrayCount);
		}
	}
}
