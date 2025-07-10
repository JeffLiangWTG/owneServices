using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class JobDeclarationValidationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCheckJE_DeclarationLanguage()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			ValidationTestHelper.AssertInvalidCodeMessageError(dec.JE_DeclarationLanguageInfo, "XX", string.Empty);
		}

		public void TestCheckJE_RL_NKFinalDestination_ListValidation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var info = declaration.JE_RL_NKFinalDestinationInfo;
			var helper = new MasterFilesTestHelper(Factory);

			CombineAssertions(() =>
			{
				declaration.JE_RL_NKFinalDestination = ZString.Empty;
				AssertEquals("JE_RL_NKFinalDestination empty", false, HasMessageError());

				declaration.JE_RL_NKFinalDestination = "ZDSD@";
				AssertEquals("Invalid JE_RL_NKFinalDestination", true, HasMessageError());

				declaration.JE_RL_NKFinalDestination = helper.USLAX.RL_Code;
				AssertEquals("Valid JE_RL_NKFinalDestination", false, HasMessageError());
			});

			bool HasMessageError() => info.HasMessageError("This port code is invalid. Please check against the transport mode and shipment type.");
		}

		public void TestCheckJE_PaidBy()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_PaidBy = "";
			AssertNoErrors(dec.JE_PaidByInfo);

			dec.JE_PaidBy = "XX";
			AssertHasErrorContaining(dec.JE_PaidByInfo, ListValidation.InvalidCodeError);

			dec.JE_PaidBy = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK;
			AssertNoErrors(dec.JE_PaidByInfo);
		}

		public void TestCheckJE_TransportModeMandatory()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsExWarehouse).Returns(false);
			mockDeclaration.Setup(m => m.IsNonTransportDeclarationType).Returns(false);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(mockDeclaration.Object.JE_TransportModeInfo);
		}

		public void TestCheckJE_TransportMode_NotRequired_ExWarehouse()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsExWarehouse).Returns(true);
			mockDeclaration.Setup(m => m.IsNonTransportDeclarationType).Returns(false);
			ValidationTestHelper.AssertFieldIsNotMandatory(mockDeclaration.Object.JE_TransportModeInfo);
		}

		public void TestCheckJE_TransportMode_NotRequired_NonTransportDeclarationType()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsExWarehouse).Returns(false);
			mockDeclaration.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			ValidationTestHelper.AssertFieldIsNotMandatory(mockDeclaration.Object.JE_TransportModeInfo);
		}

		public void TestCheckJE_TransportMode_NotRequired_ExWarehouseAndNonTransportDeclarationType()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsExWarehouse).Returns(true);
			mockDeclaration.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			ValidationTestHelper.AssertFieldIsNotMandatory(mockDeclaration.Object.JE_TransportModeInfo);
		}

		public void TestCheckJE_TransportMode_ListValidation()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(Factory.New<BaseJobDeclaration>().JE_TransportModeInfo, "~", TransportTypeList.Codes.Air);
		}

		[TestDate(2011, 01, 01, 00, 00, 10)]
		public void TestCheckJE_DateOfFirstArrival()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertEquals("No notifications, because 'IsFirstArrivalDateAndPortUsed' is false in the base Customs", false, declaration.JE_DateOfFirstArrivalInfo.HasNotifications());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				// BaseTestJobDeclaration has overriden property 'IsFirstArrivalDateAndPortUsed' set to true
				var testDeclaration = (BaseJobDeclaration)Factory.New<Integration.Customs.AU.IJobDeclaration>();
				testDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testDeclaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
				testDeclaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
				AssertHasMessageErrorContaining(testDeclaration.JE_DateOfFirstArrivalInfo, "Date of Arrival can not be before the Export Date");

				testDeclaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
				AssertNoMessageErrorContaining(testDeclaration.JE_DateOfFirstArrivalInfo, "Date of Arrival can not be before the Export Date");

				testDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				testDeclaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
				testDeclaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
				AssertHasWarning(testDeclaration.JE_DateOfFirstArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");

				testDeclaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
				testDeclaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
				AssertNoWarning(testDeclaration.JE_DateOfFirstArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");
				AssertNoMessageErrorContaining(testDeclaration.JE_DateOfArrivalInfo, "Date of Arrival can not be before the Export Date");

				testDeclaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 09, 23, 10, 10);
				AssertNoWarning(testDeclaration.JE_DateOfFirstArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");
				AssertHasMessageErrorContaining(testDeclaration.JE_DateOfFirstArrivalInfo, "Date of Arrival can not be before the Export Date");
			}
		}

		public void TestCheckJE_MessageType_ExportMandatory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertErrorIfNotEntered(declaration.JE_MessageTypeInfo);
		}

		public void TestCheckJE_MessageType_ImportMandatory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertErrorIfNotEntered(declaration.JE_MessageTypeInfo);
		}

		public void TestDrawbackDeclarationMustBeStandaloneJob()
		{
			const string expectedErrorMessage = "A Drawback Declaration must be a 'stand alone' job.";
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
				AssertHasError("Drawback has error", declaration.JE_MessageTypeInfo, expectedErrorMessage);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertNoError("Not Drawback", declaration.JE_MessageTypeInfo, expectedErrorMessage);
			});
		}

		public void TestValidateJE_MessageType_EnsureOnlySingleInvoiceForGlobalManifestDeclaration()
		{
			const string expectedErrorMessage = "Declaration created from a Global Manifest should not have more than one invoice.";
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mockDeclaration.Object;

			CombineAssertions(() =>
			{
				declaration.Invoices.AddNew();
				declaration.Invoices.AddNew();
				declaration.Validation.ValidateJE_MessageType();
				AssertNoError("Transfer to Manifest false", declaration.JE_MessageTypeInfo, expectedErrorMessage);

				mockDeclaration.Setup(m => m.SupportTransferFromCustomsToManifestEvent).Returns(true);
				declaration.Logs.AddNew(Events.TransferFromManifestToCustoms, "MK322423");
				declaration.Validation.ValidateJE_MessageType();
				AssertHasError("Multiple invoices and event to tranfer", declaration.JE_MessageTypeInfo, expectedErrorMessage);

				declaration.Invoices.DeleteAll();
				declaration.Invoices.AddNew();
				declaration.Validation.ValidateJE_MessageType();
				AssertNoError("Single invoices and event to tranfer", declaration.JE_MessageTypeInfo, expectedErrorMessage);
			});
		}

		public void TestCheckJE_MessageType_ErrorIfChangedAfterSendingMessage()
		{
			AssertErrorsAfterChangingJE_MessageType<BaseJobDeclaration>(Factory, false, false, true);
		}

		public void TestCheckJE_MessageType_MessageErrorIfControllerChangedAfterSendingMessage()
		{
			AssertErrorsAfterChangingJE_MessageType<BaseJobDeclaration>(Factory, true, false, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterSendingMessage()
		{
			AssertErrorsAfterChangingJE_MessageType<BaseJobDeclaration>(Factory, false, true, false);
		}

		public void TestCheckJE_MasterBill()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			var mockValidation = new Mock<BaseJobDeclarationValidation>(mockDeclaration.Object) { CallBase = true };
			mockDeclaration
				.Protected()
				.Setup<JobDeclarationValidation>("GetNewValidation")
				.Returns(mockValidation.Object);
			mockValidation.Setup(m => m.IsMasterBillMandatory).Returns(true);

			//master bills
			mockDeclaration.Object.JE_MasterBill = "";
			AssertEquals("No bills created yet", 0, mockDeclaration.Object.Bills.FindByBillType(BillTypeList.Codes.MasterBill).Length);
			AssertEquals("Masterbill is mandatory", true, mockDeclaration.Object.JE_MasterBillInfo.HasMessageErrors());

			mockDeclaration.Object.JE_MasterBill = "1";
			AssertEquals("Masterbill is mandatory", false, mockDeclaration.Object.JE_MasterBillInfo.HasMessageErrors());

			mockDeclaration.Object.JE_MasterBill = "";
			AssertHasMessageErrors(mockDeclaration.Object.JE_MasterBillInfo);
		}

		public void TestBondedWarehouseLicenceIsLogged()
		{
			Env.Licence.BondedWarehouse.ForceLogout();
			var newFactory = new BusinessObjectFactory();
			var importer = newFactory.New<OrgHeader>();
			importer.OH_Code = "IM3@42";
			importer.OH_FullName = "BOB";
			importer.MainAddress.OA_Address1 = "BOB'S ADDRESS";
			importer.OH_IsConsignee = true;
			importer.CompanyData.OB_IMUsedBondedWhs = ZBool.True;

			var declarationMock = newFactory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(true);

			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var loginHasBeenAttempted = true;
			var count = 0;
			var bondedWarehouseLicenceLogin = new LicenceLoginEventHandler((object sender, LicenceLoginEventArgs e) =>
			{
				count++;
				e.LoginHasBeenAttempted = loginHasBeenAttempted;
				e.LicenceCheckPoint.Login(new TestLicensedComponent());
			});

			try
			{
				declaration.BondedWarehouseLicenceLogin += bondedWarehouseLicenceLogin;
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
				declaration.RunPreSaveValidation();
				AssertEquals("licence login count", 0, count);
				AssertEquals(false, Env.Licence.BondedWarehouse.IsLoggedIn);
				var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				var line = invoice.JobComInvoiceLines.AddNew();
				line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", true, declaration.IsInwardBondedWarehousingEnabled);
				declaration.RunPreSaveValidation();
				AssertEquals(true, Env.Licence.BondedWarehouse.IsLoggedIn);
				AssertEquals("licence login count", 1, count);

				line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
				declaration.RunPreSaveValidation();
				AssertEquals("licence login count", 1, count);

				line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", true, declaration.IsInwardBondedWarehousingEnabled);
				declaration.RunPreSaveValidation();
				AssertEquals("licence login count", 2, count);

				Env.Licence.BondedWarehouse.ForceLogout();
				Env.Licence.BondedWarehouse.AllowUsageForTest = false;
				declaration.RunPreSaveValidation();
				AssertEquals("licence login count", 3, count);
				AssertEquals(false, Env.Licence.BondedWarehouse.IsLoggedIn);
				AssertHasErrors(declaration.JE_OH_ImporterInfo);

				loginHasBeenAttempted = false;
				declaration.RunPreSaveValidation();
				AssertEquals("licence login count", 4, count);
				AssertEquals(false, Env.Licence.BondedWarehouse.IsLoggedIn);
				AssertNoErrors(declaration.JE_OH_ImporterInfo);

				loginHasBeenAttempted = true;
				Env.Licence.BondedWarehouse.AllowUsageForTest = true;
				line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
				declaration.RunPreSaveValidation();
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
				AssertEquals("licence login count", 4, count);
				AssertEquals(false, Env.Licence.BondedWarehouse.IsLoggedIn);

				line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", true, declaration.IsInwardBondedWarehousingEnabled);
				declaration.RunPreSaveValidation();
				AssertEquals(true, Env.Licence.BondedWarehouse.IsLoggedIn);
				AssertEquals("licence login count", 5, count);

				line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("declaration.IsOutwardBondedWarehousingEnabled", true, declaration.IsOutwardBondedWarehousingEnabled);
				AssertEquals("licence login count", 6, count);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("licence login count", 6, count);

				line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				declaration.JE_OH_Importer = ZGuid.Empty;
				AssertEquals("declaration.IsInwardBondedWarehousingEnabled", false, declaration.IsInwardBondedWarehousingEnabled);
				AssertEquals("licence login count", 6, count);

				var importer2 = newFactory.New<OrgHeader>();
				importer2.OH_Code = "WEM3@42";
				importer2.OH_FullName = "WENDY";
				importer2.MainAddress.OA_Address1 = "WENDY'S ADDRESS";
				importer2.OH_IsConsignee = true;
				importer2.CompanyData.OB_IMUsedBondedWhs = ZBool.False;
				declaration.JE_OH_Importer = importer2.PK;
				AssertEquals("licence login count", 6, count);

				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("licence login count", 7, count);
			}
			finally
			{
				Env.Licence.BondedWarehouse.AllowUsageForTest = null;
			}
		}

		public void TestCannotChangeJE_OH_ImporterWhenThereIsWHSTransaction()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "O234";
			org1.MainAddress.OA_Address1 = "1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "O235";
			org2.MainAddress.OA_Address1 = "1";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var errorMessage = "There is an Inventory transaction created against this job.\r\nPlease cancel it before changing this value.";
			AssertEquals("HasWHSTransaction", true, declaration.HasWHSTransaction);
			AssertNoError(declaration.JE_OH_ImporterInfo, errorMessage);

			declaration.JE_OH_Importer = org2.PK;
			AssertNoError(declaration.JE_OH_ImporterInfo, errorMessage);

			declaration.JE_OH_Importer = org1.PK;
			AssertNoError(declaration.JE_OH_ImporterInfo, errorMessage);
			Factory.Save();
			AssertNoError(declaration.JE_OH_ImporterInfo, errorMessage);

			declaration.JE_OH_Importer = org2.PK;
			AssertHasError(declaration.JE_OH_ImporterInfo, errorMessage);

			declaration.JE_OH_Importer = org1.PK;
			AssertNoError(declaration.JE_OH_ImporterInfo, errorMessage);
		}

		public void TestCannotChangeJE_MessageTypeWhenThereIsWHSTransaction()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive", true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var errorMessage = "There is an Inventory transaction created against this job.\r\nPlease cancel it before changing this value.";
			AssertEquals("HasWHSTransaction", true, declaration.HasWHSTransaction);
			AssertNoError(declaration.JE_MessageTypeInfo, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoError(declaration.JE_MessageTypeInfo, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoError(declaration.JE_MessageTypeInfo, errorMessage);
			Factory.Save();
			AssertNoError(declaration.JE_MessageTypeInfo, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertHasError(declaration.JE_MessageTypeInfo, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoError(declaration.JE_MessageTypeInfo, errorMessage);
		}

		public void TestCheckJE_HouseBill()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsHouseBillMandatory).Returns(true);

			//house bills
			mockDeclaration.Object.JE_HouseBill = "";
			AssertEquals("No bills created yet", 0, mockDeclaration.Object.Bills.FindByBillType(BillTypeList.Codes.HouseBill).Length);
			AssertEquals("Housebill is mandatory", true, mockDeclaration.Object.JE_HouseBillInfo.HasMessageErrors());

			mockDeclaration.Object.JE_HouseBill = "1";
			AssertEquals("Housebill is mandatory", false, mockDeclaration.Object.JE_HouseBillInfo.HasMessageErrors());

			mockDeclaration.Object.JE_HouseBill = "";
			AssertHasMessageErrors(mockDeclaration.Object.JE_HouseBillInfo);

			mockDeclaration.Object.JE_HouseBill = "123,456";
			AssertHasWarning(mockDeclaration.Object.JE_HouseBillInfo, BaseJobDeclarationValidation.CorrectWayToEnterHouseBills);

			mockDeclaration.Object.JE_HouseBill = "123";
			AssertNoWarning(mockDeclaration.Object.JE_HouseBillInfo, BaseJobDeclarationValidation.CorrectWayToEnterHouseBills);

			mockDeclaration.Object.JE_HouseBill = "";
			AssertNoWarning(mockDeclaration.Object.JE_HouseBillInfo, BaseJobDeclarationValidation.CorrectWayToEnterHouseBills);
		}

		public void TestValidateDuplicateHouseBillNumber()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "1";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.New<BaseJobDeclaration>();
			declaration2.JE_HouseBill = "1";
			var msgText = BillValidator.AlreadyContainsHouseBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName);
			AssertHasWarning(declaration2.JE_HouseBillInfo, msgText);

			declaration2.JE_HouseBill = "2";
			AssertNoWarning(declaration2.JE_HouseBillInfo, msgText);

			declaration2.JE_MasterBill = "1";
			declaration2.JE_HouseBill = "1";
			AssertNoWarning(declaration2.JE_HouseBillInfo, msgText);

			declaration2.JE_GB = ZGuid.Empty;
			declaration2.JE_MasterBill = ZString.Empty;
			declaration2.Validation.ValidateJE_HouseBill();
			AssertNoWarning(declaration2.JE_HouseBillInfo, msgText);
		}

		public void TestCheckJE_VesselName_RegistrySetShouldErrorOnInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "KUGANE";
				vessel.RV_IsActive = false;
				vessel.RV_LloydsNumber = "1111111";

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_VesselName = "KUGANE";
				AssertHasError(declaration.JE_VesselNameInfo, "This Vessel is inactive.");
			}
		}

		public void TestCheckJE_VesselName_LlyodsNumberValidation()
		{
			const string expectedMessageError = "Lloyds number must be 7 characters in length";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HYOGO MARU";
			vessel.RV_LloydsNumber = "111111";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = vessel.RV_Name;

			CombineAssertions(() =>
			{
				AssertHasMessageError("Sea", declaration.JE_VesselNameInfo, expectedMessageError);
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.Validation.ValidateJE_VesselName();
				AssertNoMessageError("Not Sea", declaration.JE_VesselNameInfo, expectedMessageError);
			});
		}

		public void TestCheckJE_VesselName_HasDuplicates()
		{
			const string expectedMessageError = "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel.";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNoMessageError("No duplicate", declaration.JE_VesselNameInfo, expectedMessageError);
				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				declaration.Validation.ValidateJE_VesselName();
				AssertHasMessageError("Has Duplicate", declaration.JE_VesselNameInfo, expectedMessageError);
			});
		}

		public void TestCheckJE_LloydsIMO()
		{
			const string expectedMessageError = "Lloyds number must be 7 characters in length";
			var declaration = Factory.New<BaseJobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_LloydsIMO = "123456";
				AssertHasWarning("Sea", declaration.JE_LloydsIMOInfo, expectedMessageError);
				declaration.JE_LloydsIMO = "1234567";
				AssertNoWarnings("Valid Lloyds", declaration.JE_LloydsIMOInfo);

				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.JE_LloydsIMO = "123456";
				AssertNoWarning("Not Sea", declaration.JE_LloydsIMOInfo, expectedMessageError);
			});
		}

		public void TestPortCodeValidation_CS00171842()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "KRSEL";
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, "Please check against the transport mode and shipment type.");
		}

		public void TestCheckJE_ContainerMode_Mandatory_TransportMode()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.Validation.ValidateJE_ContainerMode();
				AssertHasMessageError("Sea Mode", declaration.JE_ContainerModeInfo, ContainerModeMandatory);
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.Validation.ValidateJE_ContainerMode();
				AssertNoMessageError("Not Sea Mode", declaration.JE_ContainerModeInfo, ContainerModeMandatory);
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = "~";
				AssertNoMessageError("Entered Container Mode", declaration.JE_ContainerModeInfo, ContainerModeMandatory);
			});
		}

		public void TestCheckJE_ContainerMode_Mandatory_MessageType()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.Validation.ValidateJE_ContainerMode();
				AssertHasMessageError("Import Message Type", declaration.JE_ContainerModeInfo, ContainerModeMandatory);
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.Validation.ValidateJE_ContainerMode();
				AssertNoMessageError("ExWarehouse Message Type", declaration.JE_ContainerModeInfo, ContainerModeMandatory);
			});
		}

		public void TestCheckJE_ContainerMode_UnnecessaryContainerWarning()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var oneUnnecessaryContainerString = ((BaseJobDeclarationValidation)declaration.Validation).GetUnnecessaryContainerWarning(1);
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CusContainers.AddNew();
				declaration.JE_ContainerMode = Constants.ContainerModes.Bulk;
				AssertHasWarning("Sea, Not ExWarehouse, Not Is Containerised", declaration.JE_ContainerModeInfo, oneUnnecessaryContainerString);
				declaration.JE_ContainerMode = Constants.ContainerModes.Containerised;
				AssertNoWarning("Is Containerised", declaration.JE_ContainerModeInfo, oneUnnecessaryContainerString);
				declaration.JE_TransportMode = Constants.TransportModes.Road;
				declaration.JE_ContainerMode = Constants.ContainerModes.Bulk;
				AssertNoWarning("Not Sea", declaration.JE_ContainerModeInfo, oneUnnecessaryContainerString);
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.CusContainers.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_ContainerMode();
				AssertNoWarning("No Containers", declaration.JE_ContainerModeInfo, oneUnnecessaryContainerString);
				declaration.CusContainers.AddNew();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.Validation.ValidateJE_ContainerMode();
				AssertNoWarning("Ex Warehouse Type", declaration.JE_ContainerModeInfo, oneUnnecessaryContainerString);
			});
		}

		public void TestCheckJE_ContainerMode_ListValidation()
		{
			CombineAssertions(() =>
			{
				const string invalidContainerModeMessageError = "Please enter a valid Container Type. The code you have selected is not in the Container Types List.";
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Constants.ContainerModes.Liquid;
				AssertNoMessageError("Valid", declaration.JE_ContainerModeInfo, invalidContainerModeMessageError);
				declaration.JE_ContainerMode = "XXX";
				AssertHasMessageError("Invalid", declaration.JE_ContainerModeInfo, invalidContainerModeMessageError);
				foreach (var transportMode in new[] { Constants.TransportModes.Air, Constants.TransportModes.Road, Constants.TransportModes.Rail, Constants.TransportModes.FixedTransportInstallations })
				{
					declaration.JE_TransportMode = transportMode;
					declaration.Validation.ValidateJE_ContainerMode();
					AssertNoMessageError($"Transport Mode: {transportMode}", declaration.JE_ContainerModeInfo, invalidContainerModeMessageError);
				}
			});
		}

		public void TestCheckJE_RS_NKServiceLevel_ListValidation()
		{
			CombineAssertions(() =>
			{
				var serviceLevel = Factory.New<RefServiceLevel>();
				serviceLevel.RS_Code = "STD";
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_RS_NKServiceLevel = "STD";
				AssertNoWarning("Valid Service Level", declaration.JE_RS_NKServiceLevelInfo, ListValidation.InvalidCodeMessage);
				declaration.JE_RS_NKServiceLevel = "SDS";
				AssertHasWarning("Invalid Service Level", declaration.JE_RS_NKServiceLevelInfo, ListValidation.InvalidCodeMessage);
				declaration.JE_JS = ZGuid.NewZGuid();
				declaration.Validation.ValidateJE_RS_NKServiceLevel();
				AssertNoWarning("Not Stand Alone", declaration.JE_RS_NKServiceLevelInfo, ListValidation.InvalidCodeMessage);
			});
		}

		public void TestCheckJE_TotalWeight_WithSeverityLevelOfTotalWeightValidation_InWarningMode()
		{
			using (CustomsDataRegistry.Instance.SeverityLevelOfTotalWeightValidation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				BaseJobComInvoiceLine line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line.JI_CustomsUnitQty = "KG";
				line.JI_CustomsQuantity = 5000;
				BaseJobComInvoiceLine line2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line2.JI_CustomsUnitQty = "BOX";
				line2.JI_CustomsQuantity = 51;

				declaration.JE_TotalWeightUnit = "T";
				declaration.JE_TotalWeight = 6m;
				AssertNoWarnings(declaration.JE_TotalWeightInfo);

				declaration.JE_TotalWeight = 4m;

				AssertHasWarning(declaration.JE_TotalWeightInfo, ((BaseJobDeclarationValidation)declaration.Validation).MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
				declaration.JE_TotalWeight = 50m;
				AssertNoWarnings(declaration.JE_TotalWeightInfo);
			}
		}

		public void TestCheckJE_TotalWeight_WithSeverityLevelOfTotalWeightValidation_InMessageErrorMode()
		{
			using (CustomsDataRegistry.Instance.SeverityLevelOfTotalWeightValidation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				BaseJobComInvoiceLine line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line.JI_CustomsUnitQty = "KG";
				line.JI_CustomsQuantity = 5000;
				BaseJobComInvoiceLine line2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line2.JI_CustomsUnitQty = "BOX";
				line2.JI_CustomsQuantity = 51;

				declaration.JE_TotalWeightUnit = "T";
				declaration.JE_TotalWeight = 6m;
				AssertNoMessageErrors(declaration.JE_TotalWeightInfo);

				declaration.JE_TotalWeight = 4m;
				AssertHasMessageError(declaration.JE_TotalWeightInfo, ((BaseJobDeclarationValidation)declaration.Validation).MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

				declaration.JE_TotalWeight = 50m;
				AssertNoMessageErrors(declaration.JE_TotalWeightInfo);
			}
		}

		public void TestCheckJE_TotalWeightInvalidCustomsWeight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceLine line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.JI_CustomsQuantity = 51;
			line.JI_CustomsUnitQty = "BOX";

			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalWeight = 6m;
			AssertNoMessageErrors(declaration.JE_TotalWeightInfo);
		}

		public static void AssertErrorsAfterChangingJE_MessageType<T>(BusinessObjectFactory factory, bool setUserIsController, bool messageErrorOverride, bool shouldBeError)
			where T : BaseJobDeclaration
		{
			const string expectedErrorText = "You may not change the shipment type because messages have been sent.";
			var declaration = factory.New<T>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(setUserIsController))
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.BlueErrorMessageTypeAfterMessaging, declaration.GetDefaultDataGroupingCode(), ZDateTime.Today, messageErrorOverride))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					AssertEquals("No expected Shipment Type notifications", true, declaration.JE_MessageTypeInfo.Notifications == null || !declaration.JE_MessageTypeInfo.Notifications.ContainsNotificationContaining(expectedErrorText));

					var message = declaration.Messages.AddNew();
					message.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Receive;
					factory.Save();

					AssertEquals("A message on the declaration means a change in message type is forbidden", true, declaration.DeclarationMessagesHaveBeenSent());
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					if (shouldBeError)
					{
						AssertHasError("Error for MessageType change", declaration.JE_MessageTypeInfo, expectedErrorText);
					}
					else
					{
						AssertHasMessageError("Message Error for MessageType change", declaration.JE_MessageTypeInfo, expectedErrorText);
					}
				});
			}
		}

		const string ContainerModeMandatory = "Container type is required for sea shipment.";
	}
}

