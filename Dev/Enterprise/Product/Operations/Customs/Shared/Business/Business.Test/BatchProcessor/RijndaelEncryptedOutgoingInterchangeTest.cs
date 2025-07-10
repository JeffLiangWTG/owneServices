using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class RijndaelEncryptedOutgoingInterchangeTest : TestCaseWithFactory
	{
		public void TestEncryptedContentToSend()
		{
			string content = testOutgoingInterchange.ContentToSend;
			byte[] guidBytes = Convert.FromBase64String(content.Substring(0, 24));
			byte[] sixteenGuidBytes = new byte[16];
			Array.Copy(guidBytes, 0, sixteenGuidBytes, 0, 16);
			Guid initVector = new Guid(sixteenGuidBytes);

			string decryptedMessage = new TwoWayEncoder(initVector).Decrypt(content.Substring(24));
			AssertEquals("Message should be decrypted ok", "testinterchange", decryptedMessage);
		}

		public void TestEncryptionAlgorithmNotChanged()
		{
			AssertEquals(
				"The algorithm has changed, this will cause big problems as all clients and the locally hosted https communicator versions must be in sync",
				"AQAAAAIAAwAEBQYHCAkKCw==FFdkuCWKmHFvowvCIHRdgCcJmrERLEQI/WUNMANLuJQ=",
				testOutgoingInterchange.ContentToSend);
		}

		TestRijndaelEncryptedOutgoingInterchange testOutgoingInterchange;

		protected override void SetUp()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_BodyText = "testinterchange";
			interchange.EI_To = "recipient";
			interchange.EI_From = "sender";
			testOutgoingInterchange = new TestRijndaelEncryptedOutgoingInterchange(interchange);
			base.SetUp();
		}

		class TestRijndaelEncryptedOutgoingInterchange : RijndaelEncryptedOutgoingInterchange
		{
			public TestRijndaelEncryptedOutgoingInterchange(EDIInterchange interchange) : base(interchange)
			{
			}

			public new string ContentToSend
			{
				get { return base.ContentToSend; }
			}

			protected override Guid NewInitVector()
			{
				return new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
			}
		}
	}
}
