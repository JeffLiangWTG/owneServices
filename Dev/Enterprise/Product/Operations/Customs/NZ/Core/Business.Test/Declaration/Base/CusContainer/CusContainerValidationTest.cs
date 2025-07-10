using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		public override void TestContainersRequirePackagesValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HouseBill";
			CusContainer container = declaration.CusContainers.AddNew();
			using (declaration.TemporarilySetTSWMessagingValidation())
			{
				container.CO_ContainerNumber = "CRXU1234568";
				AssertHasError("Simulating sending entry - message becomes a hard error", container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
			}

			container.Validation.ValidateCO_ContainerNumber();
			AssertHasMessageError("Normal circumstances, notification is a message error", container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);

			Package package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_HouseBill;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			using (declaration.TemporarilySetTSWMessagingValidation())
			{
				AssertNoError(container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
			}

			container.Validation.ValidateCO_ContainerNumber();
			AssertNoMessageError(container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);

			using (declaration.TemporarilySetTSWMessagingValidation())
			{
				package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
				AssertHasError(container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
			}

			container.Validation.ValidateCO_ContainerNumber();
			AssertHasMessageError(container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
		}

		public void TestWeightValidation()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Container.CO_ContainerNumber = "OOCL0000006";
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Container.CO_Weight = 10m;
			AssertHasMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
			Container.CO_Weight = 0m;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertHasMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Container.CO_Weight = 10m;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
			Container.CO_Weight = 0m;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorCannotHaveWeightOnAnEmptyContainer);
			AssertNoMessageError(Container.CO_WeightInfo, CusContainerValidation.MessageErrorMustHaveWeightOnNonEmptyContainer);
		}

		public void TestContainerTypeNotValidated()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_HouseBill = "H5234234";
			Container.CO_ContainerNumber = "OOCL0000006";
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			Container.CO_RC = ZGuid.Empty;
			AssertNoErrors(Container.CO_RCInfo);
		}

		public void TestPackagesValidatedOnECIContainers()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_HouseBill = "GIMMEALLYOURLOVIN";
			Container.CO_ContainerNumber = "OOCL0000006";
			AssertHasMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
			Package package = Declaration.Packages.AddNew();
			package.CW_HouseBill = Declaration.JE_HouseBill;
			package.CW_ContainerNoOrEquipmentNo = Container.CO_ContainerNumber;
			AssertNoMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
		}

		public void TestPackagesNotValidatedOnEmptyContainers()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_HouseBill = "TestHB";
			Container.CO_ContainerNumber = "YKKU0073923";
			AssertHasMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Container.Validation.ValidateCO_ContainerNumber();
			AssertNoMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
		}

		public void TestNoMixedContainersOnTSWConsignment()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Container.CO_ContainerNumber = "YKKU8374731";
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoMessageErrors("CO_FCL_LCL_AIR", Container.CO_FCL_LCL_AIRInfo);

			var container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "HLMU9384663";
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			container2.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoMessageErrors("CO_FCL_LCL_AIR", container2.CO_FCL_LCL_AIRInfo);

			var container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "PONU7450938";
			container3.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			container3.Validation.ValidateCO_FCL_LCL_AIR();
			AssertHasError("TSW write-off consignments cannot have mixed empty & non-empty containers", container3.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);

			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertHasError("TSW write-off consignments cannot have mixed empty & non-empty containers", Container.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);

			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			container2.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoError("TSW write-off - the entire consignment must be empties (each consignment item specifying one empty container) for the container write-off rules to work.", container2.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);

			container3.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoError("Container3", container3.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);

			Container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoError("Container1", Container.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);
		}

		public void TestTabbingToEmptyContainerLineDoesNotShowMixedContainersError()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Container.CO_ContainerNumber = "YKKU8374731";
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoError("TSW write-off - the entire consignment must be empties (each consignment item specifying one empty container) for the container write-off rules to work.", Container.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);

			var container2 = Declaration.CusContainers.AddNew();
			// re validate first line to simulate functional data entry & processing on grid
			Container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertNoError("When tabbing to next container line previous line should not show mixed containers error until container details have actually been entered", Container.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);

			container2.CO_ContainerNumber = "HLMU9384663";
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container2.Validation.ValidateCO_FCL_LCL_AIR();
			AssertHasError("TSW write-off - the entire consignment must be empties (each consignment item specifying one empty container) for the container write-off rules to work.", container2.CO_FCL_LCL_AIRInfo, CusContainerValidation.ConsignmentCannotMixContainers);
		}

		#region TestValidateContainerNo
		public void TestValidateContainerNo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Container.CO_ContainerNumber = "";
			AssertHasMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.MessageErrorMustHaveContainerNumber);
			Container.CO_ContainerNumber = "OOCL0000006";
			AssertNoMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.MessageErrorMustHaveContainerNumber);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Container.CO_ContainerNumber = "";
			AssertHasMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.MessageErrorMustHaveContainerNumber);
			Container.CO_ContainerNumber = "OOCL0000006";
			AssertNoMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.MessageErrorMustHaveContainerNumber);
		}

		public void TestCREContainersAreLinkedtoInvoiceLine()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MasterBill = "OB00239234";
			Declaration.JE_HouseBill = "T3028487";

			Container.CO_ContainerNumber = "OOCU0000001";
			AssertNoMessageError(Container.CO_ContainerNumberInfo, CusContainerValidation.CREContainerRequiresInvoiceLine);

			var container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "YKKU0394836";

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;

			var invLine1 = Declaration.InvoiceLines.AddNew();
			invLine1.JI_Tariff = "2301.10.00.01A";
			var invLine2 = Declaration.InvoiceLines.AddNew();
			invLine2.JI_Tariff = "2701.11.00.00C";
			container2.Validation.ValidateCO_ContainerNumber();
			AssertHasMessageError("Invoice lines have not been linked to containers", container2.CO_ContainerNumberInfo, CusContainerValidation.CREContainerRequiresInvoiceLine);

			invLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invLine2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			Container.Validation.ValidateCO_ContainerNumber();
			AssertNoMessageError("Invoice Lines have been allocated to respective containers", Container.CO_ContainerNumberInfo, CusContainerValidation.CREContainerRequiresInvoiceLine);
			container2.Validation.ValidateCO_ContainerNumber();
			AssertNoMessageError("Invoice Lines have been allocated to respective containers", container2.CO_ContainerNumberInfo, CusContainerValidation.CREContainerRequiresInvoiceLine);
		}

		public void TestEmptyContainerDecErrorsWithInvoicLineEntry()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			Container.CO_ContainerNumber = "OOCU0000001";
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertNoError(Container.CO_ContainerNumberInfo, CusContainerValidation.EmptyContainerEntryCannotHaveInvoiceLines);

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;

			var invLine1 = Declaration.InvoiceLines.AddNew();
			invLine1.JI_Tariff = "2301.10.00.01A";
			Container.Validation.ValidateCO_ContainerNumber();
			AssertHasError(Container.CO_ContainerNumberInfo, CusContainerValidation.EmptyContainerEntryCannotHaveInvoiceLines);
		}

		#endregion

		#region TestValidateContainerSizeForSea
		public void TestValidateContainerSizeForSea()
		{
			ContainerSizeList sizeList = new ContainerSizeList();
			Assert("Precondition: 50 should be an invalid code", !sizeList.ContainsCode("50"));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			foreach (CodeDescriptionPair size in sizeList)
			{
				Container.CO_ContainerSize = size.Code;
				AssertNoMessageErrors(size.Code + " should be valid.", Container.CO_ContainerSizeInfo);
			}
			Container.CO_ContainerSize = ZString.Empty;
			AssertHasMessageError(Container.CO_ContainerSizeInfo, CusContainerValidation.MessageErrorMustHaveContainerSize);
			AssertNoMessageError(Container.CO_ContainerSizeInfo, ListValidation.InvalidCodeMessageError);
			Container.CO_ContainerSize = "50";
			AssertNoMessageError(Container.CO_ContainerSizeInfo, CusContainerValidation.MessageErrorMustHaveContainerSize);
			AssertHasMessageError(Container.CO_ContainerSizeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			foreach (CodeDescriptionPair size in sizeList)
			{
				Container.CO_ContainerSize = size.Code;
				AssertNoMessageErrors(size.Code + " should be valid.", Container.CO_ContainerSizeInfo);
			}
			Container.CO_ContainerSize = ZString.Empty;
			AssertNoMessageErrors(Container.CO_ContainerSizeInfo);
			Container.CO_ContainerSize = "50";
			AssertNoMessageErrors(Container.CO_ContainerSizeInfo);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Container.CO_ContainerSize = ZString.Empty;
			AssertNoMessageError("This validation is not required for TSW declarations", Container.CO_ContainerSizeInfo, CusContainerValidation.MessageErrorMustHaveContainerSize);
		}
		#endregion

		#region TestValidateContainerModeForSea
		public void TestValidateContainerModeForSea()
		{
			ContainerModeList modeList = new ContainerModeList();
			Assert("Precondition: ZZZ should be an invalid code", !modeList.ContainsCode("ZZZ"));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Container.CO_FCL_LCL_AIR = ZString.Empty;
			AssertHasMessageError("Container.CO_FCL_LCL_AIRInfo", Container.CO_FCL_LCL_AIRInfo, CusContainerValidation.MessageErrorContainerModeRequired);
			foreach (CodeDescriptionPair mode in modeList)
			{
				Container.CO_FCL_LCL_AIR = mode.Code;
				AssertNoMessageErrors("Container.CO_FCL_LCL_AIRInfo", Container.CO_FCL_LCL_AIRInfo);
			}
			Container.CO_FCL_LCL_AIR = "ZZZ";
			AssertHasMessageError("Container.CO_FCL_LCL_AIRInfo", Container.CO_FCL_LCL_AIRInfo, ListValidation.InvalidCodeMessageError);
		}
		#endregion

		#region TestMCDValidationGetsCalledWhenModeIsChangedOnFormalEntry
		public void TestMCDValidationGetsCalledWhenModeIsChangedOnFormalEntry()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			CusContainer container = Declaration.CusContainers.AddNew();
			AssertNoMessageErrors(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, JobDeclarationValidationTest.MessageErrorImportFCLEntryRequiresMCD);
		}
		#endregion

		#region TestValidatePalletNumber
		public void TestValidatePalletNumber()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Container.CO_ContainerNumber = "P1234";
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered)", true, Container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered));
			Container.CO_ContainerNumber = "P12";
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered)", false, Container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered));
			Container.CO_ContainerNumber = "P1";
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered)", false, Container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered));
			Container.CO_ContainerNumber = "P";
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered)", true, Container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.MessageErrorInvalidPalletNumberEntered));
		}
		#endregion

		#region TestValidateCannotHavePalletContainerForAir
		public void TestValidateCannotHavePalletContainerForAir()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Container.CO_ContainerNumber = "P1";
			AssertEquals("HasMessageError(CusContainerValidation.CannotHaveContainersOnAirJob)", true, Container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.CannotHaveContainersOnAirJob));
			Container.CO_ContainerNumber = "";
			AssertEquals("HasMessageError(CusContainerValidation.CannotHaveContainersOnAirJob)", false, Container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.CannotHaveContainersOnAirJob));
		}
		#endregion

		#region TestValidateMessageErrorContainersExportedUnderSEPMustHaveSealNumber
		public void TestValidateMessageErrorContainersExportedUnderSEPMustHaveSealNumber()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Container.CO_Seal = ZString.Empty;
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber)", false, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.SecureExportPartnership, "328979879342");
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Container.CO_Seal = ZString.Empty;
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber)", true, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber)", false, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber)", true, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
			Container.CO_Seal = "SEAL1";
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorSealNumberIsRequired)", false, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
		}
		#endregion

		#region TestValidateMessageErrorContainersImportedUnderSEPMustHaveSealNumber
		public void TestValidateMessageErrorContainersImportedUnderSEPMustHaveSealNumber()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Container.CO_Seal = ZString.Empty;
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber)", false, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber)", false, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
			Container.CO_Seal = "SEAL1";
			AssertEquals("HasMessageError(CusContainerValidation.MessageErrorSealNumberIsRequired)", false, Container.CO_SealInfo.HasMessageError(CusContainerValidation.MessageErrorContainersExportedUnderSEPMustHaveSealNumber));
		}
		#endregion

		#region TestValidateCO_OA_PackingLocation
		public void TestStuffingEstablishment()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			Container.CO_ContainerNumber = "MNLU0392386";
			Container.PackingLocationOrgPK = ZGuid.Empty;
			AssertNoNotifications("Export sea entry for TSW should not validate this field", Container.PackingLocationOrgPKInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			Container.PackingLocationOrgPK = ZGuid.Empty;
			Container.Validation.ValidateCO_OA_PackingLocation();
			AssertHasWarning(Container.PackingLocationOrgPKInfo, CusContainerValidation.StuffingEstablishmentRecommended);

			var stuffingLocation = Factory.NewWithValidTestData<OrgHeader>();
			stuffingLocation.MainAddress.CompanyName = "";
			stuffingLocation.MainAddress.OA_Address1 = "";
			stuffingLocation.MainAddress.OA_RL_NKRelatedPortCode = "";
			Container.PackingLocationOrgPK = stuffingLocation.PK;
			AssertNoWarning(Container.PackingLocationOrgPKInfo, CusContainerValidation.StuffingEstablishmentRecommended);
			AssertHasMessageError(Container.CO_OA_PackingLocationInfo, CusContainerValidation.StuffingEstablishmentAddressRequired);

			stuffingLocation.MainAddress.CompanyName = "Container Packing Inc.";
			stuffingLocation.MainAddress.OA_Address1 = "500 Mitchum St";
			stuffingLocation.MainAddress.OA_City = "Long Beach";
			stuffingLocation.MainAddress.OA_RL_NKRelatedPortCode = "USLBH";
			stuffingLocation.MainAddress.Postcode = "57500";
			Container.PackingLocationOrgPK = stuffingLocation.PK;
			Container.Validation.ValidateCO_OA_PackingLocation();
			AssertNoWarning(Container.PackingLocationOrgPKInfo, CusContainerValidation.StuffingEstablishmentRecommended);
			AssertNoMessageError(Container.CO_OA_PackingLocationInfo, CusContainerValidation.StuffingEstablishmentAddressRequired);
		}
		#endregion

		#region TestCheckCO_MAF_ContainerType
		public void TestCheckCO_MAF_ContainerType()
		{
			Declaration.JE_MessageType = NZJobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Container.CO_ContainerNumber = "MNLU0392386";
			AssertNoMessageErrors("Import entry when TSW not activated should not validate this Container Type", Container.CO_MAF_ContainerTypeInfo);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Container.CO_MAF_ContainerType = "";
			AssertHasMessageError(Container.CO_MAF_ContainerTypeInfo, CusContainerValidation.EquipmentSizeAndTypeDescriptionCode);

			Container.CO_MAF_ContainerType = "90";
			AssertHasMessageError(Container.CO_MAF_ContainerTypeInfo, ListValidation.InvalidCodeMessageError);

			Container.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C42;
			AssertNoMessageErrors(Container.CO_MAF_ContainerTypeInfo);
		}
		#endregion

		#region TestCheckSealingParty
		public void TestCheckSealingParty()
		{
			Declaration.JE_MessageType = NZJobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Container.CO_ContainerNumber = "MNLU0392386";
			AssertNoMessageErrors("Export entry when TSW not activated should not validate this field", Container.CO_SealingPartyInfo);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.SecureExportPartnership, "SEP-03948X");

			Container.CO_SealingParty = "";
			AssertHasMessageError(Container.CO_SealingPartyInfo, CusContainerValidation.SealingPartyRequired);

			Container.CO_SealingParty = "SECURE PACKING P/L";
			AssertNoMessageError(Container.CO_SealingPartyInfo, CusContainerValidation.SealingPartyRequired);
		}
		#endregion

		#region Implementation
		#region Container
		protected CusContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Declaration.CusContainers.AddNew();
				}
				return fContainer;
			}
		}
		CusContainer fContainer;
		#endregion

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = (JobDeclaration)GetJobDeclaration();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region GetJobDeclaration
		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
		#endregion
	}
}
