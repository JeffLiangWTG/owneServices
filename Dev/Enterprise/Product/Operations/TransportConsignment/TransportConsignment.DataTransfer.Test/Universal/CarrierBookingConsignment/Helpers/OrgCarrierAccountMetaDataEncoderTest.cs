using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportConsignment.DataTransfer.Universal.Helpers;

namespace Enterprise.TransportConsignment.DataTransfer.Test
{
	class OrgCarrierAccountMetaDataEncoderTest : TestCaseWithFactory
	{
		public void EncodeAndDecodeTest()
		{
			var textToBeEncrpyted = "TESTTEXT";
			var encrypted = OrgCarrierAccountMetaDataEncoder.Encrypt(new Guid("174FA7D4-1056-4B6F-BBF0-FF3B6224B17E"), textToBeEncrpyted);
			var decryptedText = OrgCarrierAccountMetaDataEncoder.Decrypt(new Guid("174FA7D4-1056-4B6F-BBF0-FF3B6224B17E"), encrypted);
			AssertEquals(textToBeEncrpyted, decryptedText);
		}
	}
}
