using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FTZMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2011, 11, 02)]
		public void TestBuildingMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.FillWithValidTestData();
			warehouseOrg.OH_FullName = "Warehouse Organisation";
			warehouseOrg.OH_IsWarehouseClient = true;
			warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "887766554433");

			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			declaration.IOROrgPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "1530100";
			declaration.FTZControlNumber = "00000001";
			declaration.US_SchDEntry = "1234";
			declaration.US_F_DirectDelivery = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_F_RoutingDetails = "3311ABC12";
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
			declaration.US_UC_NKCountryOfExport = "AU";
			declaration.US_SchDLoading = "12345";
			declaration.JE_VoyageFlightNo = "1234567890";
			declaration.US_DateOfExport = new ZDateTime(2011, 10, 24);
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 25);
			declaration.US_SchDArrival = "1234";
			declaration.US_EntryDate = new ZDateTime(2011, 10, 26);
			declaration.US_US_NKLocationOfGoods = "1234";
			declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;

			var uscarrier = Factory.NewWithValidTestData<USCarrierCombined>();
			uscarrier.UI_Code = "1234";
			uscarrier.UI_Name = "A carrier";

			declaration.JE_MasterBillIssuerSCAC = uscarrier.UI_Code;
			var bill = declaration.Bills.AddNew();

			declaration.JE_MasterBill = "1234567890123456789";
			bill.CU_BillNum = "123456789012";
			bill.CU_NoOfPacks = 1000m;

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "12345678901";

			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "APLU5006003";

			var invoice = declaration.Invoices.AddNew();
			invoice.Charges.AddNew("OFT", 10m, "USD");
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "D";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 1001m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;
			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678901234567890");
			invoice.JZ_OH_Buyer = importer.PK;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 1001m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();

			AssertMultilineASCIIEquals("e214 FTZ Admission Add",
@"B  8888XJ5FT                                               <<MSGNO PLACEHOLDER>>
10A1530100  1100000001N1234YXJ53311ABC128877665544331234123456789012            
20A401234A CARRIER              1234567890     2011102420111025123420111025     
401234567890123456789                123456789012        0000001000AU12345      
4112345678901                                                                   
50000011234567890B  HAU000000010000KG 000000001000D  123                        
5100000001000000000010010000000010Z00009900                                     
60A DESCRIPTION                                MID9876543210987654321012        
Y  8888XJ5FT", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 04, 02)]
		public void TestBillSentWithSCAC()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.FillWithValidTestData();
			warehouseOrg.OH_FullName = "Warehouse Organisation";
			warehouseOrg.OH_IsWarehouseClient = true;
			warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "887766554433");
			declaration.IOROrgPK = importer.PK;
			declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.FTZAdmissionNumber = "1530100|14|00000001";
			declaration.US_SchDEntry = "1234";
			declaration.US_F_DirectDelivery = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_SchDLoading = "12345";
			declaration.JE_VoyageFlightNo = "1234567890";
			declaration.US_DateOfExport = ZDateTime.Today;
			declaration.US_SchDArrival = "1234";

			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "00000164";

			var invoice = declaration.Invoices.AddNew();
			invoice.Charges.AddNew("OFT", 10m, "USD");
			invoice.JZ_CU_RelatedHouseBill = declaration.PrimaryMasterBill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();

			AssertMultilineASCIIEquals("e214 FTZ Admission Add",
@"B  8888XJ5FT                                               <<MSGNO PLACEHOLDER>>
10A1530100  1400000001N1234YXJ5         887766554433    123456789012            
20A  APLUAMERICAN PRESIDENT LINE1234567890     2014040220140402123420140402     
40APLU00000164                                                       12345      
50000011234567890B  HAU000000010000KG                                           
5100000000000000000000010000000000 00000000                                     
60A DESCRIPTION                                MID9876543210987654321012        
Y  8888XJ5FT", message.EM_FormattedMessageText);
		}
	}
}
