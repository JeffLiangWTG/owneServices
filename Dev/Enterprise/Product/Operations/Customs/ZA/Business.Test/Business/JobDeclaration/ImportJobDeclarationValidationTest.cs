using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ImportJobDeclarationValidation))]
	sealed class ImportJobDeclarationValidationTest : ImportCommonJobDeclarationValidationAbstractTest
	{
		public void TestCheckJE_MergeBy_IncompatibleMergeByTypeAndPreviousProcedureCode()
		{
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NON", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NOP", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "TRF", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "TRD", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "CLS", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "CLD", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "PNO", previousProcedureCode: "00", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "PNP", previousProcedureCode: "00", instruction, invoiceLine);

			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NON", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NOP", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "TRF", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "TRD", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "CLS", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "CLD", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "PNO", previousProcedureCode: "11", instruction, invoiceLine);
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "PNP", previousProcedureCode: "11", instruction, invoiceLine);
		}

		void AssertUnsupportedMergeByMessageError(bool assertion, string mergeBy, string previousProcedureCode, CusEntryInstruction instruction, JobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_Procedure = "20" + previousProcedureCode;
			declaration.JE_MergeBy = mergeBy;
			AssertEquals("Pre-req", previousProcedureCode, invoiceLine.JI_Calc_PreviousProcedure);
			declaration.Validation.ValidateJE_MergeBy();
			AssertEquals(assertion, declaration.JE_MergeByInfo.HasMessageError(ValidationConstants.InvoiceLine.MergingNotRecommendedWherePreviousProcedureCodeIsNonZero));
		}

		public void TestJE_HouseBill()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CargoCarrier = "12345678";
			declaration.JE_HouseBill = "12345678HBILL";
			AssertHasWarning(declaration.JE_HouseBillInfo, ImportJobDeclarationValidation.CargoCarrierAlreadySentToCustoms);
			declaration.JE_HouseBill = "HBILL";
			AssertNoWarning(declaration.JE_HouseBillInfo, ImportJobDeclarationValidation.CargoCarrierAlreadySentToCustoms);
		}

		public void TestVesselValidationDetectsEmptyRadioCallSign()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_LloydsNumber = "8811923";
			vessel.RV_Code = "BLAH";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertHasMessageError(declaration.JE_VesselNameInfo, ImportJobDeclarationValidation.RadioCallSignMandatory);
			vessel.RV_RadioCallSign = "123";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertNoMessageError(declaration.JE_VesselNameInfo, ImportJobDeclarationValidation.RadioCallSignMandatory);
		}

		public void TestCheckJE_DateOfArrival()
		{
			var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road };
			foreach (var transportMode in transportModes)
			{
				declaration.JE_TransportMode = transportMode;
				declaration.JE_DateOfArrival = ZDateTime.Empty;
				AssertHasMessageError("Arrival Date is Required for " + transportMode + " Imports", declaration.JE_DateOfArrivalInfo, ImportJobDeclarationValidation.DateOfArrivalRequired);
				declaration.JE_DateOfArrival = new ZDateTime(2016, 6, 17);
				AssertNoMessageError("Arrival Date is Required for " + transportMode + " Imports", declaration.JE_DateOfArrivalInfo, ImportJobDeclarationValidation.DateOfArrivalRequired);
			}
		}

		public void TestMessageErrorOnEmptyMasterBillWhenImport()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = ZString.Empty;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.FixedTransportInstallations;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
			declaration.JE_TransportMode = "";
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
		}

		public void TestJE_VoyageFlightNoHasNoMessageErrorIfEmptyAndDeclarationIsIMXOrImport()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoMandatory("Vehicle Reg No"));
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoMandatory("Vehicle Reg No"));
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoMandatory("Flight/Folio"));
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoMandatory("Flight/Folio"));
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoMandatory("Voyage"));
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoMandatory("Voyage"));
		}

		public void TestJE_VesselNameImport()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageError(declaration.JE_VesselNameInfo, JobDeclarationValidation.VesselMandatory);
			declaration.JE_VesselName = "####";
			AssertHasMessageError(declaration.JE_VesselNameInfo, ListValidation.InvalidCodeMessageError);
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_LloydsNumber = "8811923";
			vessel.RV_Code = "BLAH";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertNoMessageError(declaration.JE_VesselNameInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_CargoCarrier()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_HouseBill = "AAA";
			declaration.JE_CargoCarrier = ZString.Empty;
			AssertHasMessageError(declaration.JE_CargoCarrierInfo, ImportJobDeclarationValidation.MandatoryIfHouseBillCaptured);
			declaration.JE_CargoCarrier = "CCC";
			AssertNoMessageError(declaration.JE_CargoCarrierInfo, ImportJobDeclarationValidation.MandatoryIfHouseBillCaptured);
			AssertHasMessageError(declaration.JE_CargoCarrierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_CargoCarrier = "DDD";
			AssertHasMessageError(declaration.JE_CargoCarrierInfo, ImportJobDeclarationValidation.MustBeEmptyIfHouseBillEmpty);
			declaration.JE_CargoCarrier = ZString.Empty;
			AssertNoMessageError(declaration.JE_CargoCarrierInfo, ImportJobDeclarationValidation.MustBeEmptyIfHouseBillEmpty);
		}

		public void TestCheckJE_VATClaimBackIndicator()
		{
			declaration.JE_VATClaimBackIndicator = "";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VATClaimBackIndicator = "N";
			AssertNoMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertNoMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_VATClaimBackIndicator = "A";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ImportJobDeclarationValidation.GovernmentVATNoRequired);
			AssertEquals("The Importer on this Declaration must have a valid VAT Registration Number in order to qualify for VAT 201 returns", ImportJobDeclarationValidation.GovernmentVATNoRequired);
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			declaration.JE_OH_Importer = testOrg.PK;
			declaration.JE_VATClaimBackIndicator = "";
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ImportJobDeclarationValidation.GovernmentVATNoRequired);
			var cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			Factory.Save();
			declaration.JE_VATClaimBackIndicator = "";
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ImportJobDeclarationValidation.GovernmentVATNoRequired);
			cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "123321";
			Factory.Save();
			declaration.JE_VATClaimBackIndicator = "";
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertNoNotifications(declaration.JE_VATClaimBackIndicatorInfo);
		}

		protected override ZString MessageType => ZAJobMessageTypeList.Codes.Import;
	}
}
