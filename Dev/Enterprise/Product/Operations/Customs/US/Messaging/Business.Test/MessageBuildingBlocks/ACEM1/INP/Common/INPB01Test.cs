using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class INPB01Test : TestCase
	{
		public void TestBillOfLadingSequenceNumberShouldKeepLast12PlacesWhenLengthViolation()
		{
			var inpb01 = new INPB01()
			{
				BillOfLadingSequenceNumber = "123456789123456789",
				ForeignPort = "23900",
				ManifestQuantity = 20,
				ManifestUnitCode = "BOX",
				Weight = 114514,
				WeightUnit = "KG",
				BillOfLadingStatusIndicator = "B",
				MasterInbondIndicator = "1"
			};
			var text = inpb01.Serialise(false);
			AssertEquals("inpb01.BillOfLadingSequenceNumber", "B01789123456789239000000000020BOX  0000114514KGB1                               ", text);
		}
	}
}
