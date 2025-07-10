using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE11Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var asese11Block = new ASESE11()
			{
				EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate,
				LocationOfGoodsFIRMS = "S001",
				ElectedExamSiteFIRMS = "A002",
				VoyageFlightTripManifestNumber = "QF20",
				GeneralOrderGONumber = "12345678",
				OriginatingWarehouseEntryFilerCode = "XJ5",
				OriginatingWarehouseEntryNumber = "23456789",
				ConveyanceNameOrFTZZoneID = "VESSEL"
			};

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)asese11Block).Update(declaration, notifications);
			AssertEquals(EntryDateElectionCodeList.Codes.PresentationDate, declaration.US_EntryDateElectionCode);
			AssertEquals("S001", declaration.US_US_NKLocationOfGoods);
			AssertEquals("A002", declaration.US_US_NKCentralizedExamSite);
			AssertEquals("QF20", declaration.JE_VoyageFlightNo);
			AssertEquals("12345678", declaration.US_GeneralOrderNo);
			AssertEquals("XJ5", declaration.US_WHSEntryFilerCode);
			AssertEquals("23456789", declaration.US_WHSEntryNumber);
			AssertEquals("VESSEL", declaration.JE_VesselName);

			AssertNotEquals("SEL", declaration.US_FTZNo);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			((IBIRDHeaderRecord)asese11Block).Update(declaration, notifications);
			AssertEquals("SEL", declaration.US_FTZNo);
		}
	}
}
