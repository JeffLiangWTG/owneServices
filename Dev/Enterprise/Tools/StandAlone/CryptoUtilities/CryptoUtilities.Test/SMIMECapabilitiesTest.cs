using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class SMIMECapabilitiesTest : NUnit.Framework.TestCase
	{
		public void TestGetInstance()
		{
			SMIMECapabilities myCapabilities = SMIMECapabilities.GetInstance();
			AssertNotNull("MyCapabilities", myCapabilities);
		}
	}
}
