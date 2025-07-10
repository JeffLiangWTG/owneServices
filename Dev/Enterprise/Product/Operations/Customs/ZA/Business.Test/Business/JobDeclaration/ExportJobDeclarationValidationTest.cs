using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ExportCommonJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNoNotificationOnFinalDestinationForExportWhenNoEntry()
		{
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "AU"));
			declaration.JE_RL_NKFinalDestination = uNLOCO.RL_Code;
			AssertNoNotifications(declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestFinalDestinationCannotBeEmptyWhenExport()
		{
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageError(declaration.JE_RL_NKFinalDestinationInfo, JobDeclarationValidation.FinalDestMandatory);
		}

		public void TestPortOfLoadingMustBeValidWhenExporting()
		{
			declaration.JE_RL_NKPortOfLoading = "OZZO";
			AssertHasMessageError(declaration.JE_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestPortOfLoadingMustBeInZAWhenExporting()
		{
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertHasMessageError(declaration.JE_RL_NKPortOfLoadingInfo, ExportJobDeclarationValidation.PortOfLoadingMustBeInZA);
		}

		public void TestPortOfLoadingCannotBeEmptyWhenExporting()
		{
			declaration.JE_RL_NKPortOfLoading = "";
			AssertHasMessageError(declaration.JE_RL_NKPortOfLoadingInfo, ExportJobDeclarationValidation.PortOfLoadingMandatory);
		}

		public void TestJE_RL_NKOriginShouldBeBLNSWhenExport()
		{
			declaration.JE_RL_NKOrigin = "AUSYD";
			AssertHasMessageError(declaration.JE_RL_NKOriginInfo, ExportJobDeclarationValidation.CountryOfOriginMustBeZAOrBLNS);
			declaration.JE_RL_NKOrigin = "ZAAAM";
			AssertNoMessageError(declaration.JE_RL_NKOriginInfo, ExportJobDeclarationValidation.CountryOfOriginMustBeZAOrBLNS);
		}

		public void TestSupplierMustHaveValidIDOrVATNumberIfNotRegisteredExporter()
		{
			var supplier = OrgHeader.New(Factory);
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertHasMessageError(declaration.JE_OH_SupplierInfo, "The Supplier does not have a Customs Supplier code which is required.");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, ValidationConstants.Declaration.UnregisteredTraderCustomsCode);
			supplier.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.VATCode, "");
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError(declaration.JE_OH_SupplierInfo, "The currently selected Supplier has a Customs Supplier code of 70707070. An ID Number or Government Tax File code must be setup when using Customs Supplier code 70707070");
			supplier.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890128");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.VATCode, "");
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertEquals("No message error", false, declaration.JE_OH_SupplierInfo.GetMessageErrors().ContainsNotificationContaining("Customs Supplier code"));
		}

		public void TestCheckJE_ExportDate()
		{
			var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road };
			foreach (var transportMode in transportModes)
			{
				declaration.JE_TransportMode = transportMode;
				declaration.JE_ExportDate = ZDateTime.Empty;
				AssertHasMessageError("Departure Date is Required for " + transportMode + " Exports", declaration.JE_ExportDateInfo, ExportJobDeclarationValidation.DateOfDepartureRequired);
				declaration.JE_ExportDate = new ZDateTime(2016, 6, 17);
				AssertNoMessageError("Departure Date is Required for " + transportMode + " Exports", declaration.JE_ExportDateInfo, ExportJobDeclarationValidation.DateOfDepartureRequired);
			}
		}

		public void TestMessageErrorOnEmptyMasterBillWhenExport()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
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

		public void TestJE_VoyageFlightNoHasMessageErrorIfEmptyAndDeclarationIsExport()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ExportJobDeclarationValidation.VoyageFlightNoMandatory("Voyage"));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ExportJobDeclarationValidation.VoyageFlightNoMandatory("Vehicle Reg No"));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, ExportJobDeclarationValidation.VoyageFlightNoMandatory("Flight/Folio"));
		}

		public void TestJE_VesselNameExport()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageError(declaration.JE_VesselNameInfo, ExportJobDeclarationValidation.VesselMandatory);
			declaration.JE_VesselName = "####";
			AssertHasMessageError(declaration.JE_VesselNameInfo, ListValidation.InvalidCodeMessageError);
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_LloydsNumber = "8811923";
			vessel.RV_Code = "BLAH";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertNoMessageError(declaration.JE_VesselNameInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCannotChangeJE_OH_SupplierWhenThereIsWHSTransaction()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "O234";
			org1.MainAddress.OA_Address1 = "1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "O235";
			org2.MainAddress.OA_Address1 = "1";

			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);

			var declaration = declarationMock.Object;
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var errorMessage = "There is an Inventory transaction created against this job.\r\nPlease cancel it before changing this value.";
			AssertEquals("HasWHSTransaction", true, declaration.HasWHSTransaction);
			AssertNoError(declaration.JE_OH_SupplierInfo, errorMessage);

			declaration.JE_OH_Supplier = org2.PK;
			AssertNoError(declaration.JE_OH_SupplierInfo, errorMessage);

			declaration.JE_OH_Supplier = org1.PK;
			AssertNoError(declaration.JE_OH_SupplierInfo, errorMessage);
			Factory.Save();
			AssertNoError(declaration.JE_OH_SupplierInfo, errorMessage);

			declaration.JE_OH_Supplier = org2.PK;
			AssertHasError(declaration.JE_OH_SupplierInfo, errorMessage);

			declaration.JE_OH_Supplier = org1.PK;
			AssertNoError(declaration.JE_OH_SupplierInfo, errorMessage);
		}

		public void TestCheckJE_VATClaimBackIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_VATClaimBackIndicator = "";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_VATClaimBackIndicator = "N";
			AssertNoMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_VATClaimBackIndicator = "Y";
			AssertNoMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_VATClaimBackIndicator = "A";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_VATClaimBackIndicator = "Y";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ExportJobDeclarationValidation.GovernmentVATNoRequired);
			AssertEquals("The Exporter on this Declaration must have a valid VAT Registration Number in order to qualify for VAT 201 returns", ExportJobDeclarationValidation.GovernmentVATNoRequired);

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			declaration.JE_OH_Supplier = testOrg.PK;
			declaration.JE_VATClaimBackIndicator = "";
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ExportJobDeclarationValidation.GovernmentVATNoRequired);

			var cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			Factory.Save();
			declaration.JE_VATClaimBackIndicator = "";
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ExportJobDeclarationValidation.GovernmentVATNoRequired);

			cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "123321";
			Factory.Save();
			declaration.JE_VATClaimBackIndicator = "";
			declaration.JE_VATClaimBackIndicator = "Y";
			AssertNoNotifications(declaration.JE_VATClaimBackIndicatorInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
		}
		JobDeclaration declaration;
	}
}
