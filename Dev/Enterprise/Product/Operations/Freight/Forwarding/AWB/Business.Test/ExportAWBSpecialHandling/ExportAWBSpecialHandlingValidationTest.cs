using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class ExportAWBSpecialHandlingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSpecialHandlingCodeIsMandatory()
		{
			var sph = Factory.New<ExportAWBSpecialHandling>();

			sph.EP_SpecialHandling = "_W_";
			AssertHasErrors(sph.EP_SpecialHandlingInfo);

			sph.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			AssertNoErrors(sph.EP_SpecialHandlingInfo);

			sph.EP_SpecialHandling = "";
			AssertHasErrors(sph.EP_SpecialHandlingInfo);

			sph.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments;
			AssertNoErrors(sph.EP_SpecialHandlingInfo);
		}

		public void TestSpecialHandlingCodeHasDuplicateItems()
		{
			ExportAWBHeader header = Factory.New<ExportAWBHeader>();
			ExportAWBSpecialHandling specialHandling1 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling1.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			Assert(specialHandling1.IsSecurityStatus);
			AssertNoErrors(specialHandling1.EP_SpecialHandlingInfo);

			ExportAWBSpecialHandling specialHandling2 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			Assert(specialHandling2.IsSecurityStatus);
			AssertHasError(specialHandling2.EP_SpecialHandlingInfo, "List contains duplicate Special Handling Codes.");
		}

		public void TestCheckOneSecurityStatusInSpecialHandlingItemsOnly()
		{
			ExportAWBHeader header = Factory.New<ExportAWBHeader>();

			ExportAWBSpecialHandling specialHandling1 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling1.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			Assert(specialHandling1.IsSecurityStatus);
			AssertNoErrors(specialHandling1.EP_SpecialHandlingInfo);

			ExportAWBSpecialHandling specialHandling2 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			Assert(!specialHandling2.IsSecurityStatus);
			AssertNoErrors(specialHandling2.EP_SpecialHandlingInfo);

			ExportAWBSpecialHandling specialHandling3 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling3.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			Assert(specialHandling3.IsSecurityStatus);
			AssertHasErrors(specialHandling3.EP_SpecialHandlingInfo);

			specialHandling3.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods;
			Assert(!specialHandling3.IsSecurityStatus);
			AssertNoErrors(specialHandling3.EP_SpecialHandlingInfo);
		}
	}
}
