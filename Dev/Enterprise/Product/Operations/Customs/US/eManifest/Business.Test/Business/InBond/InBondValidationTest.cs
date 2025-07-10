using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class InBondValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_DestinationPortDCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var validCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "Test Name", startDate, endDate);
			newFactory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(inBond.BM_DestinationPortCodeInfo, "????", validCode.ZZD_Code);
		}

		public void TestCheckBM_ExportDate()
		{
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			ValidationTestHelper.AssertFieldIsNotMandatory(inBond.BM_ExportDateInfo);
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inBond.BM_ExportDateInfo);
			inBond.BM_InBondEntryType = InbondTypes.Codes.TransportationAndExportation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inBond.BM_ExportDateInfo);
		}

		public void TestCheckBM_ForeignDestPortKCode()
		{
			var validCode = "01";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, validCode, "Test", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();
			
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			ValidationTestHelper.AssertInvalidCodeMessageError(inBond.BM_ForeignDestPortKCodeInfo, "????", validCode);
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(inBond.BM_ForeignDestPortKCodeInfo, "????", validCode);
			inBond.BM_InBondEntryType = InbondTypes.Codes.TransportationAndExportation;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(inBond.BM_ForeignDestPortKCodeInfo, "????", validCode);
		}

		public void TestCheckBM_InBondCarrierID()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inBond.BM_InBondCarrierIDInfo);
		}

		public void TestCheckBM_InBondEntryType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(inBond.BM_InBondEntryTypeInfo, "??", InbondTypes.Codes.ImmediateTransportation);
		}

		public void TestCheckBM_PedimentoNumber()
		{
			inBond.BM_ForeignDestPortKCode = "20100";
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			ValidationTestHelper.AssertFieldIsNotMandatory(inBond.BM_PedimentoNumberInfo);
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inBond.BM_PedimentoNumberInfo);
			inBond.BM_InBondEntryType = InbondTypes.Codes.TransportationAndExportation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inBond.BM_PedimentoNumberInfo);
			inBond.BM_ForeignDestPortKCode = "13870";
			ValidationTestHelper.AssertFieldIsNotMandatory(inBond.BM_PedimentoNumberInfo);
			inBond.BM_PedimentoNumber = "123456";
			AssertHasMessageError(inBond.BM_PedimentoNumberInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
			inBond.BM_PedimentoNumber = "123456789012345";
			AssertNoMessageError(inBond.BM_PedimentoNumberInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
		}

		public void TestCheckBM_RL_NKDestinationPort()
		{
			var validCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).RL_Code;
			ValidationTestHelper.AssertInvalidCodeMessageError(inBond.BM_RL_NKDestinationPortInfo, "????", validCode);
		}

		public void TestCheckBM_RL_NKForeignDestPort()
		{
			var validCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).RL_Code;
			ValidationTestHelper.AssertInvalidCodeMessageError(inBond.BM_RL_NKForeignDestPortInfo, "????", validCode);
		}

		public void TestCheckBM_OnwardCarrier()
		{
			var validCode = Factory.NewWithValidTestData<USCarrierCombined>().UI_Code;
			ValidationTestHelper.AssertInvalidCodeMessageError(inBond.BM_OnwardCarrierInfo, "????", validCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			inBond = shipment.InBond;
		}

		InBond inBond;
	}
}
