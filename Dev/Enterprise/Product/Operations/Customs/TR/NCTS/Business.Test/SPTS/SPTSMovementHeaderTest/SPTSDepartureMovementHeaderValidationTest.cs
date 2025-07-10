using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSDepartureMovementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_InlandTransportMode()
		{
			var sptsHeader = Factory.New<SPTSHeader>();
			var header = sptsHeader.MovementHeader;
			header.BM_InlandTransportMode = "";
			AssertHasMessageErrorContaining(header.BM_InlandTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BM_InlandTransportMode = "SEA";
			AssertNoMessageErrorContaining(header.BM_InlandTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestBM_InlandTransportMode_Container()
		{
			var sptsHeader = Factory.New<SPTSHeader>();
			var header = sptsHeader.MovementHeader;
			var bill = sptsHeader.Bills.AddNew();
			header.BM_InlandTransportMode = SPTSTransportModeList.Codes.SEA;
			var msg = "A container is required";

			bill.B0_ServiceType = "N";
			header.Validation.ValidateAll();
			AssertNoMessageError(header.BM_InlandTransportModeInfo, msg);

			bill.B0_ServiceType = "Y";
			header.Validation.ValidateAll();
			AssertHasMessageError(header.BM_InlandTransportModeInfo, msg);

			header.BM_InlandTransportMode = SPTSTransportModeList.Codes.AIR;
			AssertNoMessageError(header.BM_InlandTransportModeInfo, msg);

			var cont = sptsHeader.HeaderContainers.AddNew();
			header.BM_InlandTransportMode = SPTSTransportModeList.Codes.SEA;
			AssertNoMessageError(header.BM_InlandTransportModeInfo, msg);
		}

		public void TestCheckBM_OA_InBondCarrier()
		{
			var header = Factory.New<SPTSDepartureMovementHeader>();
			header.BM_OA_InBondCarrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.BM_OA_InBondCarrierInfo, MandatoryValidation.YouHaveNotEntered);
			header.BM_OA_InBondCarrier = header.PK;
			AssertNoMessageErrorContaining(header.BM_OA_InBondCarrierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_PortOfPresentationCode()
		{
			var header = Factory.New<SPTSDepartureMovementHeader>();
			header.BM_PortOfPresentationCode = "";
			AssertHasMessageErrorContaining(header.BM_PortOfPresentationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BM_PortOfPresentationCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty;
			AssertNoMessageErrorContaining(header.BM_PortOfPresentationCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_DestinationPortCode()
		{
			var header = Factory.New<SPTSDepartureMovementHeader>();
			header.BM_DestinationPortCode = "";
			AssertHasMessageErrorContaining(header.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BM_DestinationPortCode = "Test";
			AssertNoMessageErrorContaining(header.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
