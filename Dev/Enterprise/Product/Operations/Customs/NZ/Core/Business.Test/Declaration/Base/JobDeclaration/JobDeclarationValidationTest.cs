using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	class JobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJE_Cal_GoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_RL_NKFinalDestination = "NZWLG";

			var forwardingHeader = Factory.NewWithValidTestData<OrgHeader>();
			forwardingHeader.OH_Code = "FWCODE";

			declaration.JE_OH_Forwarder = forwardingHeader.PK;
			declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.FW;

			forwardingHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OH-000", Core.Constants.CountryCodes.NewZealand);
			forwardingHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OA-000", Core.Constants.CountryCodes.NewZealand);

			Factory.Save();

			ValidationTestHelper.AssertErrorIfInvalidCode(declaration.JE_Cal_GoodsLocationInfo, "XX-XX-00", "FW-OA-000");
		}

		public void TestMasterBillDoesNotContainInvalidCharacters()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.JE_MasterBill = "081–11111111";
			AssertHasErrorContaining(Declaration.JE_MasterBillInfo, BillNumberHasNonASCIIValue);

			Declaration.JE_MasterBill = "081-11111111";
			AssertNoErrorContaining(Declaration.JE_MasterBillInfo, BillNumberHasNonASCIIValue);

			Declaration.JE_MasterBill = "081	11111111";
			AssertHasErrorContaining(Declaration.JE_MasterBillInfo, BillNumberHasTabs);

			Declaration.JE_MasterBill = "08111111111";
			AssertNoErrorContaining(Declaration.JE_MasterBillInfo, BillNumberHasTabs);
		}

		public void TestHouseBillDoesNotContainInvalidCharacters()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.JE_HouseBill = "HB1–159938";
			AssertHasErrorContaining(Declaration.JE_HouseBillInfo, BillNumberHasNonASCIIValue);

			Declaration.JE_HouseBill = "HB1-159938";
			AssertNoErrorContaining(Declaration.JE_HouseBillInfo, BillNumberHasNonASCIIValue);

			Declaration.JE_HouseBill = "HB1	733";
			AssertHasErrorContaining(Declaration.JE_HouseBillInfo, BillNumberHasTabs);

			Declaration.JE_HouseBill = "HB1733";
			AssertNoErrorContaining(Declaration.JE_HouseBillInfo, BillNumberHasTabs);
		}

		public void TestJE_HouseBill()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_HouseBill = ZString.Empty;
			AssertNoMessageErrors("TSW declaration does not require H/B", Declaration.JE_HouseBillInfo);
			Declaration.JE_HouseBill = "J003928";
			AssertNoMessageErrors(Declaration.JE_HouseBillInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_HouseBill = ZString.Empty;
			AssertNoMessageErrors("TSW declaration does not require H/B", Declaration.JE_HouseBillInfo);
			Declaration.JE_HouseBill = "J003928";
			AssertNoMessageErrors(Declaration.JE_HouseBillInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_HouseBill = ZString.Empty;
			AssertHasMessageErrors("TSW declaration does require a H/B value for a write-off message", Declaration.JE_HouseBillInfo);
			Declaration.JE_HouseBill = "J003928";
			AssertNoMessageErrors(Declaration.JE_HouseBillInfo);
		}

		public void TestCheckJE_ApplicationCode()
		{
			AssertEquals("Default value", JobApplicationCodeList.Codes.CUS, Declaration.JE_ApplicationCode);
			AssertNoErrors(Declaration.JE_ApplicationCodeInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = "";
			AssertHasErrors(Declaration.JE_ApplicationCodeInfo);

			Declaration.JE_ApplicationCode = "ABC";
			AssertHasErrors(Declaration.JE_ApplicationCodeInfo);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertNoErrors(Declaration.JE_ApplicationCodeInfo);
		}

		public void TestApplicationCodeIsLockedDownAfterEntry()
		{
			AssertEquals("JE_ApplicationCode should currently be accesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
			AssertEquals("Default value", JobApplicationCodeList.Codes.CUS, Declaration.JE_ApplicationCode);

			AssertEquals("JE_ApplicationCode should currently be accesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			Declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			AssertEquals("JE_ApplicationCode should now be inaccesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);

			Declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Declaration.DeclarationNumber = "59775527";
			AssertEquals("JE_ApplicationCode should now be inaccesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
		}

		public void TestJE_MessageTypeReportNoErrorWhenIsDrawback()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			AssertEquals("Precondition : declaration.IsDrawback", true, declaration.IsDrawback);
			declaration.Validation.ValidateJE_MessageType();
			AssertNoErrors(declaration.JE_MessageTypeInfo);
		}

		public void TestCheckJE_MessageSubType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			AssertEquals("Precondition: declaration.CusEntryHeader.CH_LastEntryStyle", string.Empty, declaration.CusEntryHeader.CH_LastEntryStyle);
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoErrors(declaration.JE_MessageSubTypeInfo);

			declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoErrors(declaration.JE_MessageSubTypeInfo);

			declaration.CusEntryHeader.EntryNumber = "123";
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, MessageErrorEntryStyleMustNotChangeAfterMessageHasBeenSent);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, MessageErrorEntryStyleMustNotChangeAfterMessageHasBeenSent);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertNoMessageErrors(declaration.JE_MessageSubTypeInfo);

			declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Temporary;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, MessageErrorEntryStyleMustNotChangeAfterMessageHasBeenSent);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, MessageErrorEntryStyleMustNotChangeAfterMessageHasBeenSent);
			AssertHasWarning(declaration.JE_MessageSubTypeInfo, AddingIPIEntry);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertNoErrors(declaration.JE_MessageSubTypeInfo);

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrors("Mandatory validation required for TSW declarations", declaration.JE_MessageSubTypeInfo);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;
			declaration.CusEntryHeader.EntryNumber = "78829930";
			AssertNoErrors(declaration.JE_MessageSubTypeInfo);

			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoError(declaration.JE_MessageSubTypeInfo, MessageErrorEntryStyleMustNotChangeAfterMessageHasBeenSent);
			AssertHasWarning(declaration.JE_MessageSubTypeInfo, AddingIPIEntry);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertHasError(declaration.JE_MessageSubTypeInfo, CannotChangeIPIEntry);
		}

		public void TestCheckJE_MessageSubTypeWhenFormalChangedToECI()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertNoError(declaration.JE_MessageSubTypeInfo, CannotChangeFormalToECI);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.FormalEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasError(declaration.JE_MessageSubTypeInfo, CannotChangeFormalToECI);

			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertNoError(declaration.JE_MessageSubTypeInfo, CannotChangeFormalToECI);
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.FormalEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasError(declaration.JE_MessageSubTypeInfo, CannotChangeFormalToECI);
		}

		public void TestCheckJE_MessageSubTypeFromAndToIPI()
		{
			var allEntries = new string[]
			   {
				MessageSubTypeCombinedList.Codes.Normal,
				MessageSubTypeCombinedList.Codes.Simplified,
				MessageSubTypeCombinedList.Codes.Temporary,
				MessageSubTypeCombinedList.Codes.Sight,
				MessageSubTypeCombinedList.Codes.Periodic
			   };

			for (int i = 0; i < allEntries.Length; i++)
			{
				for (int j = 0; j < allEntries.Length; j++)
				{
					if (i != j)
					{
						var oldEntry = allEntries[i];
						var newEntry = allEntries[j];

						CheckJE_MessageSubTypeFromAndToIPICore(oldEntry, newEntry);
					}
				}
			}
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterSendingMessage()
		{
			JobDeclarationValidationBaseOnlyTest.AssertErrorsAfterChangingJE_MessageType<JobDeclaration>(Factory, setUserIsController: false, messageErrorOverride: true, shouldBeError: false);
		}

		public void TestJE_SendMCDContainerQuarantineDeclaration()
		{
			CusContainer container = Declaration.CusContainers.AddNew();

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertHasMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertNoMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertNoMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertNoMessageError(Declaration.JE_SendMCDContainerQuarantineDeclarationInfo, MessageErrorImportFCLEntryRequiresMCD);
		}

		public void TestPeriodicVesselIsOk()
		{
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "PERIODIC VARIOUS";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_VesselName = "PERIODIC VARIOUS";
			AssertNoMessageErrors(Declaration.JE_VesselNameInfo);
			Declaration.JE_VesselName = "I DON'T EXIST";
			AssertHasMessageErrors(Declaration.JE_VesselNameInfo);
		}

		public void TestPeriodicFlightIsOk()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_VoyageFlightNo = "PD0001";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0002";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0003";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0004";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0005";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0006";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0007";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0008";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0009";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0010";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0011";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0012";
			AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
			Declaration.JE_VoyageFlightNo = "PD0013";
			AssertHasMessageErrors(Declaration.JE_VoyageFlightNoInfo);
		}

		public void TestMiscOrgValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid otherOrgPK = Factory.New<OrgHeader>().PK;
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;
			AssertNoErrorContaining(declaration.JE_OH_SupplierInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			AssertNoErrorContaining(declaration.JE_OH_ImporterInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertNoErrorContaining(declaration.JE_OH_SupplierInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			AssertNoErrorContaining(declaration.JE_OH_ImporterInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);

			declaration.JE_OH_Supplier = miscOrgPK;
			declaration.JE_OH_Importer = miscOrgPK;
			AssertNoErrorContaining(declaration.JE_OH_SupplierInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			AssertNoErrorContaining(declaration.JE_OH_ImporterInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertHasErrorContaining(declaration.JE_OH_SupplierInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			AssertHasErrorContaining(declaration.JE_OH_ImporterInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertNoErrors(declaration.JE_OH_SupplierInfo);
			AssertNoErrors(declaration.JE_OH_ImporterInfo);

			declaration.JE_OH_Supplier = otherOrgPK;
			declaration.JE_OH_Importer = otherOrgPK;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertNoErrorContaining(declaration.JE_OH_SupplierInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			AssertNoErrorContaining(declaration.JE_OH_ImporterInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertNoErrorContaining(declaration.JE_OH_SupplierInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			AssertNoErrorContaining(declaration.JE_OH_ImporterInfo, JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
		}

		public void TestCheckJE_DateOfArrival()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_DateOfArrivalInfo);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageError(Declaration.JE_DateOfArrivalInfo, "You have not entered an Arrival.");
			AssertEquals(1, Declaration.JE_DateOfArrivalInfo.GetMessageErrors().Take(2).Count());
			AssertNoErrors(Declaration.JE_DateOfArrivalInfo);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_DateOfArrivalInfo);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_DateOfArrivalInfo);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			Declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_DateOfArrivalInfo);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNoNotifications(Declaration.JE_DateOfArrivalInfo);
		}

		public void TestCheckJE_ExportDate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_ExportDateInfo);
			AssertNoMessageErrors(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_ExportDateInfo);
			AssertNoMessageErrors(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageError(Declaration.JE_ExportDateInfo, "You have not entered a Date of Export.");
			AssertEquals(1, Declaration.JE_ExportDateInfo.GetMessageErrors().Take(2).Count());
			AssertNoErrors(Declaration.JE_ExportDateInfo);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_ExportDateInfo);
			AssertNoMessageErrors(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Excise;
			Declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertHasErrors(Declaration.JE_ExportDateInfo);
			AssertNoMessageErrors(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);
			Declaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoNotifications(Declaration.JE_ExportDateInfo);
		}

		public void TestCheckJE_TotalWeight()
		{
			AssertNoNotifications(Declaration.JE_TotalWeightInfo);

			JobComInvoiceLine line = Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.JI_CustomsUnitQty = StatisticalUQList.Codes.Kilograms;
			line.JI_CustomsQuantity = 5000;
			JobComInvoiceLine line2 = Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line2.JI_CustomsUnitQty = StatisticalUQList.Codes.Number;
			line2.JI_CustomsQuantity = 51;

			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			Declaration.JE_TotalWeight = 6m;
			AssertNoWarnings(Declaration.JE_TotalWeightInfo);

			Declaration.JE_TotalWeight = 4m;

			AssertHasMessageError(Declaration.JE_TotalWeightInfo, Declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
			Declaration.JE_TotalWeight = 50m;
			AssertNoMessageError(Declaration.JE_TotalWeightInfo, Declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

			line.JI_CustomsQuantity = 65535;
			AssertHasMessageError(Declaration.JE_TotalWeightInfo, Declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

			line.JI_CustomsQuantity = 0;
			AssertNoMessageError(Declaration.JE_TotalWeightInfo, Declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
		}

		public void TestJE_RL_NKPortOfFirstArrivalDoesNotValidate()
		{
			Declaration.JE_RL_NKPortOfFirstArrival = "ZZZZZ";
			AssertNoNotifications(Declaration.JE_RL_NKPortOfFirstArrivalInfo);
		}

		public void TestErrorThrownForInvalidPortOfLoading()
		{
			// NZABY has no Seaport or Airport
			Declaration.JE_TransportMode = "AIR";
			AssertNoNotifications("Port of Loading should not have errors", Declaration.JE_RL_NKPortOfLoadingInfo);
			Declaration.JE_RL_NKPortOfLoading = "NZABY";
			AssertHasMessageErrorContaining(Declaration.JE_RL_NKPortOfLoadingInfo, PortCodeInvalidForTransportMode);

			Declaration.JE_TransportMode = "SEA";
			Declaration.JE_RL_NKPortOfLoading = "NZABY";
			AssertHasMessageErrorContaining(Declaration.JE_RL_NKPortOfLoadingInfo, PortCodeInvalidForTransportMode);

			// NZORR has Seaport no Airport
			Declaration.JE_TransportMode = "AIR";
			Declaration.JE_RL_NKPortOfLoading = "NZORR";
			AssertHasMessageErrorContaining(Declaration.JE_RL_NKPortOfLoadingInfo, PortCodeInvalidForTransportMode);

			Declaration.JE_TransportMode = "SEA";
			Declaration.JE_RL_NKPortOfLoading = "NZORR";
			AssertNoMessageErrorContaining(Declaration.JE_RL_NKPortOfLoadingInfo, PortCodeInvalidForTransportMode);

			// NZZQN has Airport no Seaport
			Declaration.JE_TransportMode = "AIR";
			Declaration.JE_RL_NKPortOfLoading = "NZZQN";
			AssertNoMessageErrorContaining(Declaration.JE_RL_NKPortOfLoadingInfo, PortCodeInvalidForTransportMode);

			Declaration.JE_TransportMode = "SEA";
			Declaration.JE_RL_NKPortOfLoading = "NZZQN";
			AssertHasMessageErrorContaining(Declaration.JE_RL_NKPortOfLoadingInfo, PortCodeInvalidForTransportMode);
		}

		public void TestJE_ContainerModeDoesNotValidate()
		{
			Declaration.JE_ContainerMode = "";
			AssertNoNotifications(Declaration.JE_ContainerModeInfo);
		}

		public void TestJE_DateOfFirstArrivalDoesNotValidate()
		{
			Declaration.JE_DateOfFirstArrival = ZDateTime.Invalid;
			AssertNoNotifications(Declaration.JE_DateOfFirstArrivalInfo);
			Declaration.JE_DateOfArrival = new ZDateTime(2005, 1, 1);
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 1, 1);
			AssertNoNotifications(Declaration.JE_DateOfFirstArrivalInfo);
		}

		public void TestCheckJE_VesselName()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "BUNGA BIDARA", "BUNGA BIDARA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_VesselName = "BUNGA BIDARA";
			AssertNoMessageError(Declaration.JE_VesselNameInfo, VesselNotOnCustomsSupportedList);

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_VesselName = "blah";
			AssertHasMessageError(Declaration.JE_VesselNameInfo, VesselNotOnCustomsSupportedList);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "QF117", "QF117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_VoyageFlightNo = "QF117";

			AssertNoMessageError(Declaration.JE_VoyageFlightNoInfo, FlightNotOnCustomsSupportedList);

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_VoyageFlightNo = "blah";
			AssertHasMessageError(Declaration.JE_VoyageFlightNoInfo, FlightNotOnCustomsSupportedList);

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_VoyageFlightNo = "F9918-WEST";
			AssertHasMessageError(Declaration.JE_VoyageFlightNoInfo, VoyageHasTooManyCharacters);

			Declaration.JE_VoyageFlightNo = "F9918W";
			AssertNoMessageError(Declaration.JE_VoyageFlightNoInfo, VoyageHasTooManyCharacters);
		}

		public void TestAllocatedContact()
		{
			OrgHeader partyToCheck = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode = partyToCheck.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_CustomsRegNo = "989083B";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_OH_Importer = partyToCheck.PK;
			AssertEquals("Organisation without a contact or company communication information should error.", true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			AssertHasMessageError(Declaration.JE_OH_ImporterInfo, "This Organization has no allocated contact person for New Zealand Customs and no communication information entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details and add or edit a contact as this organizations primary contact representative.");

			partyToCheck.MainAddress.OA_Phone = "+61 2 8750 3000";
			partyToCheck.MainAddress.OA_Email = "info@partytocheck.com.au";
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = partyToCheck.PK;
			AssertNoMessageError(Declaration.JE_OH_ImporterInfo, "This Organization has no allocated contact person for New Zealand Customs and no communication information entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details and add or edit a contact as this organizations primary contact representative.");
			AssertHasMessageError(Declaration.JE_OH_ImporterInfo, "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");

			var customsContact = partyToCheck.Contacts.AddNew();
			customsContact.OC_ContactName = "John Smith";
			var contactAllocation = customsContact.Allocations.AddNew();
			contactAllocation.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;

			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = partyToCheck.PK;
			AssertNoMessageErrorContaining(Declaration.JE_OH_ImporterInfo, "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");
			AssertHasWarning(Declaration.JE_OH_ImporterInfo, "This Organization has John Smith as the allocated contact person for New Zealand Customs but that contact has no communication information entered.\r\nCommunication information will fall back to the company organization values.\r\nEdit the allocated contact to send specific information for John Smith, the primary contact representative if desired.");

			partyToCheck.MainAddress.OA_Phone = "";
			partyToCheck.MainAddress.OA_Email = "";
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = partyToCheck.PK;
			AssertNoWarning(Declaration.JE_OH_ImporterInfo, "This Organization has John Smith as the allocated contact person for New Zealand Customs but that contact has no communication information entered.\r\nCommunication information will fall back to the company organization values.\r\nEdit the allocated contact to send specific information for John Smith, the primary contact representative if desired.");
			AssertHasMessageError(Declaration.JE_OH_ImporterInfo, "This Organization has no mandatory communication information details, required by New Zealand Customs, entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details.\r\nAlternatively, update John Smith, the allocated contact for this Organization, with their phone/fax/email details.");

			customsContact.OC_ContactName = "John Smith";
			customsContact.OC_Phone = "+61 2 8750 3200";
			customsContact.OC_Email = "john.smith@partytocheck.com.au";
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = partyToCheck.PK;
			AssertNoWarnings(Declaration.JE_OH_ImporterInfo);
			AssertNoMessageErrors(Declaration.JE_OH_ImporterInfo);

			Declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertNoMessageError(Declaration.JE_OH_ImporterInfo, "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");
		}

		public void TestAllocatedContactForMISCOrg()
		{
			OrgHeader partyToCheck = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode = partyToCheck.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_CustomsRegNo = "989083B";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_OH_Importer = partyToCheck.PK;
			AssertEquals("Organisation without a contact or company communication information should error.", true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			AssertHasMessageError(Declaration.JE_OH_ImporterInfo, "This Organization has no allocated contact person for New Zealand Customs and no communication information entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details and add or edit a contact as this organizations primary contact representative.");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_OH_Importer = Declaration.CachedMiscOrgPK;
			Declaration.MiscImporterName = "JOHN'S SPARE BOLTS";
			AssertEquals("MISC Organisation without a contact or company communication information should NOT error on a Simplified Entry.", false, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			AssertNoMessageError(Declaration.JE_OH_ImporterInfo, "This Organization has no allocated contact person for New Zealand Customs and no communication information entered.\r\nEdit the Organization to add the company telephone/mobile, fax and/or email details and add or edit a contact as this organizations primary contact representative.");
			AssertNoWarnings(Declaration.JE_OH_ImporterInfo);
			AssertNoMessageErrors(Declaration.JE_OH_ImporterInfo);
		}

		public void TestCheckJE_OH_NotifyParty()
		{
			OrgHeader notifyParty = Factory.New<OrgHeader>();
			notifyParty.FillWithValidTestData();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoNotifications(declaration.JE_OH_NotifyPartyInfo);

			declaration.JE_OH_NotifyParty = notifyParty.PK;

			AssertHasMessageError(declaration.JE_OH_NotifyPartyInfo, JobDeclarationValidation.MessageErrorDeliveryAuthorityMissingCustomsClientCode);

			notifyParty.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "12345678A");

			declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoNotifications(declaration.JE_OH_NotifyPartyInfo);
		}

		public void TestMessageErrorWhereDeliveryNotificationUsedButNotSetUp()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var notifyParty = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Declaration.JE_OH_NotifyParty = notifyParty.PK;
			AssertHasMessageError("No NZ Customs contact has been set up for this organisation", Declaration.JE_OH_NotifyPartyInfo, "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");

			var contact = notifyParty.Contacts.AddNew();
			contact.OC_ContactName = "Bill Smith";
			contact.OC_Mobile = "+61 418 5489 5480";
			var nzCustomsAllocated = contact.Allocations.AddNew();
			nzCustomsAllocated.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			Declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoMessageError(Declaration.JE_OH_NotifyPartyInfo, "This Organization needs an allocated contact person for New Zealand Customs.\r\nEdit the organization details to add or edit a contact to allocate that person as this organizations primary contact representative.");
			AssertHasMessageError("NZ Customs contact does not have an email address set up", Declaration.JE_OH_NotifyPartyInfo, "The allocated customs contact for this Organization needs a valid email address.\r\nEdit the organization details to edit the allocated contact to include a valid email address.");

			contact.OC_Email = "bsmith@TestCompany.com.au";
			Declaration.Validation.ValidateJE_OH_NotifyParty();
			AssertNoMessageError(Declaration.JE_OH_NotifyPartyInfo, "The allocated customs contact for this Organization needs a valid email address.\r\nEdit the organization details to edit the allocated contact to include a valid email address.");
		}

		public void TestValidateJE_RL_NKPortOfDeliveryNotify()
		{
			Assert("Precondition: NZAKL exists.", Declaration.Lookups.DeliveryNotifyPortList.Cast<RefUNLOCO>().Any(x => x.RL_Code == "NZAKL"));
			Declaration.JE_RL_NKPortOfDeliveryNotify = "NZAKL";
			AssertNoMessageError(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_RL_NKPortOfDeliveryNotify = "南京";
			AssertHasError(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo, EnglishCharactersValidation.GetNotificationMessage(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo));

			Assert("Precondition: DORNE doesn't exist.", Declaration.Lookups.DeliveryNotifyPortList.Cast<RefUNLOCO>().All(x => x.RL_Code != "DORNE"));
			Declaration.JE_RL_NKPortOfDeliveryNotify = "DORNE";
			AssertHasMessageError(Declaration.JE_RL_NKPortOfDeliveryNotifyInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_RL_NKPortOfDeliveryNotifyInfo.ClearValue();
			AssertNoMessageErrors("We don't set message error when notification port is empty, for now.", Declaration.JE_RL_NKPortOfDeliveryNotifyInfo);
		}

		public void TestTotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning()
		{
			var validation = new JobDeclarationValidationForTestAccess(Declaration);
			Assert("TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning is always true in NZ.", validation.TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning);
		}

		public void TestCatersForSpecialNZValidation()
		{
			Declaration.JE_TransportMode = "AIR";
			Declaration.JE_VoyageFlightNo = "";
			Declaration.JE_MasterBill = "08298739457";
			AssertNoWarning(Declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);

			Declaration.JE_VoyageFlightNo = "VARIOUS";
			Declaration.JE_MasterBill = "08654297913";
			AssertNoWarning(Declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);

			Declaration.JE_VoyageFlightNo = "QF1";
			Declaration.JE_MasterBill = "08600238210";
			AssertHasWarning(Declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);

			Declaration.JE_MasterBill = "08100492874";
			AssertNoWarning(Declaration.JE_MasterBillInfo, Enterprise.Customs.Business.BillValidator.AirlinePrefixValidationMessage);
		}

		JobDeclaration declaration;
		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = GetNewJobDeclaration();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				}
				return declaration;
			}
		}

		protected virtual JobDeclaration GetNewJobDeclaration() => JobDeclaration.New(Factory);

		CusContainer container;
		protected CusContainer Container => container ?? (container = Declaration.CusContainers.AddNew());

		void CheckJE_MessageSubTypeFromAndToIPICore(string originalEntry, string nonOriginalEntry)
		{
			SetupDeclarationAndChangeEntryFromNormalToIPI(out var declaration, out var _, originalEntry);

			declaration.JE_MessageSubType = nonOriginalEntry;
			AssertHasError("An IPI entry can NOT be changed to a non-orginal entry", declaration.JE_MessageSubTypeInfo, CannotChangeIPIEntry);

			SetupDeclarationAndChangeEntryFromNormalToIPI(out declaration, out var originalEntryNumber, originalEntry);
			declaration.JE_MessageSubType = originalEntry;
			AssertNoError("An IPI entry can be reverted back to its original entry", declaration.JE_MessageSubTypeInfo, CannotChangeIPIEntry);
			AssertEquals("The original entry should be assoicated with once reverted", originalEntryNumber, declaration.CusEntryHeader.EntryNumber);
		}

		void SetupDeclarationAndChangeEntryFromNormalToIPI(out JobDeclaration declaration, out ZString originalEntryNumber, string originalEntry)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			declaration.JE_MessageSubType = originalEntry;
			declaration.CusEntryHeader.EntryNumber = "123";
			originalEntryNumber = declaration.CusEntryHeader.EntryNumber;
			declaration.CusEntryHeader.CH_LastEntryStyle = originalEntry;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.CusEntryHeader.EntryNumber = "456";
			declaration.CusEntryHeader.CH_LastEntryStyle = originalEntry;

			AssertHasWarning("Changing to IPI Entry - Original Entry type = " + originalEntry, declaration.JE_MessageSubTypeInfo, AddingIPIEntry);
		}

		const string BillNumberHasNonASCIIValue = "Bill number field can not have Non ASCII characters.";
		const string BillNumberHasTabs = "Bill number field can not contain Tab characters.";
		const string FlightNotOnCustomsSupportedList = "Flight number is not in the list of valid Flights supported by NZ Customs.";
		const string VesselNotOnCustomsSupportedList = "Vessel name is not in the list of valid Vessel names supported by NZ Customs.";
		const string CannotChangeFormalToECI = "This formal entry has already been sent to Customs and therefore cannot be changed to this type of entry. It must be cancelled first if you wish to now submit it as a different type of entry.";
		const string MessageErrorEntryStyleMustNotChangeAfterMessageHasBeenSent = "Entry Style cannot change after a message has been sent - You must cancel this entry before resubmitting as another type.";
		public const string AddingIPIEntry = "You are adding a Primary Industries Import entry to this existing declaration.";
		const string CannotChangeIPIEntry = "Once you have added a Primary Industries Import entry to an existing declaration you cannot change the Entry Style, unless reverting it back to the original Entry Style.";
		const string VoyageHasTooManyCharacters = "Vessel Voyage exceeds 8 characters and will be truncated in entry message.";
		public const string MessageErrorImportFCLEntryRequiresMCD = "Import FCL Entries must send an MCD Other Info Code with appropriate MCD Flags.";
		const string PortCodeInvalidForTransportMode = "This Port Code is invalid for the transport mode of this declaration.";

		sealed class JobDeclarationValidationForTestAccess : JobDeclarationValidation
		{
			public JobDeclarationValidationForTestAccess(JobDeclaration declaration)
			: base(declaration)
			{
			}

			public new bool TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning => base.TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning;
		}
	}
}
