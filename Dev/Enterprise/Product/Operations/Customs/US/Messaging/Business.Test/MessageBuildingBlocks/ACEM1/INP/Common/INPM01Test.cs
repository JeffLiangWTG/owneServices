using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class INPM01Test : TestCase
	{
		public void TestVesselMatchingDetailShouldNotTrimBeginningSpaces()
		{
			var inpm01 = new INPM01()
			{
				VesselName = " HELLO WORLD",
				VoyageNumber = " 1234"
			};
			var data = inpm01.Serialise(false);
			inpm01 = new INPM01();
			inpm01.Deserialise(data);
			AssertEquals("inpm01.VesselName", " HELLO WORLD", inpm01.VesselName);
			AssertEquals("inpm01.VoyageNumber", " 1234", inpm01.VoyageNumber);
		}

		public void TestVoyageNumberShouldTruncateTo5PlacesWhenLengthViolation()
		{
			var inpm01 = new INPM01()
			{
				VoyageNumber = "123456789",
				CarrierCode = "OOLU",
				ModeOfTransportationCode = "10",
				VesselCountryCode = "US",
				VesselName = "Aeolus",
				ManifestSequenceNumber = "000001",
				VesselCode = "114514"
			};
			var text = inpm01.Serialise(false);
			AssertEquals("inpm01.VoyageNumber", "M01OOLU10USAEOLUS                 12345     000001 114514                       ", text);
		}
	}
}
