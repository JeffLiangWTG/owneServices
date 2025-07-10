using System.Security.Cryptography;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class CryptoUtilitiesExceptionTest : NUnit.Framework.TestCase
	{
		public void TestCryptoUtilitiesExceptionConstructorWithErrorNumber()
		{
			var exception = new CryptoUtilitiesException("Message", 4);
			AssertEquals("Message Error Number: 4", exception.Message);
		}

		public void TestCryptoUtilitiesExceptionConstructorWithException()
		{
			var exception = new CryptoUtilitiesException(new CryptographicException("CryptographicException Constructor"));
			AssertEquals("CryptoUtility Exception. CryptographicException Constructor", exception.Message);
		}

		public void TestInvalidCertificateFile()
		{
			var exception = CryptoUtilitiesException.InvalidCertificateFile(3);
			AssertEquals(
				"The supplied file does not appear to be a valid certificate. The certificate must be in 'DER encoded binary X.509 (.CER)' format.\r\n" +
				"Try importing it into Internet Explorer and then exporting as 'DER encoded binary X.509 (.CER)'. Error Number: 3",
				exception.Message
			);
		}

		public void TestInvalidPrivateKeyFile()
		{
			var exception = CryptoUtilitiesException.InvalidPrivateKeyFile(234);
			AssertEquals("The supplied file is not a valid private key file. Error Number: 234", exception.Message);
			exception = CryptoUtilitiesException.InvalidPrivateKeyFile(unchecked((int)0xC000A000));
			AssertStartsWith("exception Message for STATUS_INVALID_SIGNATURE", "The cryptographic signature is invalid.", exception.Message);
			exception = CryptoUtilitiesException.InvalidPrivateKeyFile(unchecked((int)0x80070056));
			AssertStartsWith("exception Message for ERROR_INVALID_PASSWORD", "The supplied password for your private key file is incorrect or the certificate is not compatible with the current operating system -", exception.Message);
			exception = CryptoUtilitiesException.InvalidPrivateKeyFile(unchecked((int)0x80090024));
			AssertStartsWith("exception Message for NTE_TEMPORARY_PROFILE", "The profile for the user is a temporary profile. Please check service task account profile.", exception.Message);
		}
	}
}
