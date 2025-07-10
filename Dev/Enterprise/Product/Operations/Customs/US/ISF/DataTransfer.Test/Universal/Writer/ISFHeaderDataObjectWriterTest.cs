using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Constants = Enterprise.Customs.US.ISF.Business.ISFConstants;
using USPackingingUnitList = Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer.Testing
{
	sealed class ISFHeaderDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			var header = Factory.New<CusISFHeader>();
			header.Lines.AddNew();
			var writer = new ISFHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header),
					writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))));
			var shipment = writer.GetDataObject(header);
			var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

			writer = new ISFHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			shipment = writer.GetDataObject(header);
			commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
		}

		public void TestISFHeaderMappings()
		{
			var headerBO = SetupISFHeader();
			var headerWriter = new ISFHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, headerBO)));
			var headerData = headerWriter.GetDataObject(headerBO);
			AssertEquals(Core.Constants.TransportModes.Sea, headerData.TransportMode.GetCodeAsUpperCase());
			AssertEquals(ContainerModeList.Codes.Containerized, headerData.CustomsContainerMode.GetCodeAsUpperCase());
			AssertEquals(Core.Constants.TransportModes.Sea, headerData.TransportMode.GetCodeAsUpperCase());
			AssertEquals("INB", headerData.Branch.GetCodeAsUpperCase());
			AssertEquals("THIS IS TEST REF", headerData.OwnerRef);
			AssertEquals("HWB012345", headerData.WayBillNumber);
			AssertEquals(WayBillTypeList.Codes.House, headerData.WayBillType.GetCodeAsUpperCase());
			AssertEquals("USLAX", headerData.PortOfDischarge.Code);
			AssertEquals("CNSHA", headerData.PortOfDestination.Code);
			AssertEquals(1234m, headerData.GoodsValue);
			AssertEquals(1000, headerData.TotalNoOfPacks);
			AssertEquals(USPackingingUnitList.ShippingOrPackingingUnitList.Codes.Bag, headerData.TotalNoOfPacksPackageType.GetCodeAsUpperCase());
			AssertEquals(2100m, headerData.TotalWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, headerData.TotalWeightUnit.GetCodeAsUpperCase());
			AssertEquals("headerData.AddInfoCollection.Count", 18, headerData.AddInfoCollection.Count);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.EntryType), headerBO.BF_EntryType);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFShipmentType), headerBO.BF_ShipmentType);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.CarrierSCAC), headerBO.BF_SCAC);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ActionReason), headerBO.BF_ActionReasonCode);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ImporterIDType), headerBO.BF_ImporterCodeType);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ImporterID), headerBO.BF_ImporterCode);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ImporterName), headerBO.BF_ImporterFullName);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ImporterDOB), headerBO.BF_DateOfBirth.ToShortDateString());
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ImporterIssueCountry), headerBO.BF_CountryOfIssue);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ConsigneeIDType), headerBO.BF_ConsigneeCodeType);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ConsigneeID), headerBO.BF_ConsigneeCode);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFBondHolder), headerBO.BF_BondNumberOrHolder);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFBondActivityCode), headerBO.BF_BondActivityCode);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFBondType), headerBO.BF_BondType);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFSuretyCode), headerBO.BF_SuretyCode);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFBondRefNo), headerBO.BF_BondReferenceNumber);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.ISFShipmentSubType), headerBO.BF_ShipmentSubType);
			AssertAddInfoContents(headerData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoConstants.SendEquipment), headerBO.BF_SendEquipment);
			AssertEquals("headerData.OrganizationAddressCollection.Count", 7, headerData.OrganizationAddressCollection.Count);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ImporterDocumentaryAddress)), headerBO.Importer.MainAddress);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.BuyingParty)), headerBO.BuyingParty.Address);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.SellingParty)), headerBO.SellingParty.Address);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ScheduledContainerStuffingLocation)), headerBO.StuffingLocation.Address);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.Consolidator)), headerBO.Consolidator.Address);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.BookingPartyDocumentaryAddress)), headerBO.BookingParty.Address);
			AssertOrganizationAddressContents(headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ShipToParty)), headerBO.MainShipToParty.Address);
			AssertEquals("headerData.EntryNumberCollection.Count", 2, headerData.EntryNumberCollection.Count);
			AssertEntryNumberContents(headerData.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.EntryNumberConstants.ISF), headerBO.BF_CustomsReference, ZString.Empty);
			AssertEntryNumberContents(headerData.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.EntryNumberConstants.ENS), headerBO.BF_EntryNumber, ZString.Empty);
			AssertEquals("headerData.DateCollection.Count", 1, headerData.DateCollection.Count);
			AssertDateContents(headerData.DateCollection.FirstOrDefault(), headerBO.BF_LastAcceptedDate);
			AssertEquals("headerData.AdditionalBillCollection.Count", 3, headerData.AdditionalBillCollection.Count);
			AssertAdditionalBillContents(headerData.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == BillTypeList.Codes.OceanBillOfLading), headerBO.BF_OceanBill);
			AssertAdditionalBillContents(headerData.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == BillTypeList.Codes.HouseBillOfLading), headerBO.BF_HouseBill);
			AssertAdditionalBillContents(headerData.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == BillTypeList.Codes.MasterBillOfLading), headerBO.BF_MasterBill);
			AssertEquals("headerData.AdditionalReferenceCollection.Count", 3, headerData.AdditionalReferenceCollection.Count);
			AssertAdditionalReferenceContents(headerData.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == BillTypeList.Codes.FullNameOfISFImporter), headerBO.BF_ImporterFullName);
			AssertAdditionalReferenceContents(headerData.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == BillTypeList.Codes.SuretyCode), headerBO.BF_SuretyCode);
			AssertAdditionalReferenceContents(headerData.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == BillTypeList.Codes.BondReferenceNumber), headerBO.BF_BondReferenceNumber);
			AssertEquals("headerData.CustomizedFieldCollection.Count", 5, headerData.CustomizedFieldCollection.Count);
			AssertCustomizedFieldContents(headerData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.CustomizedFieldConstants.CustomAttribOne), headerBO.CustomAttribute1);
			AssertCustomizedFieldContents(headerData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.CustomizedFieldConstants.CustomAttribTwo), headerBO.CustomAttribute2);
			AssertCustomizedFieldContents(headerData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "WriteKey"), "WriteValue");
			AssertNotNull("headerData.CommercialInfo", headerData.CommercialInfo);
			AssertEquals("headerData.CommercialInfo.CommercialInvoiceCollection", 1, headerData.CommercialInfo.CommercialInvoiceCollection.Count);
			var commercialInvoiceHeader = headerData.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals("commercialInvoiceHeader.CommercialInvoiceLineCollection.Count", 2, commercialInvoiceHeader.CommercialInvoiceLineCollection.Count);
			AssertNotNull("headerData.ContainerCollection", headerData.ContainerCollection);
			AssertEquals("headerData.ContainerCollection", 2, headerData.ContainerCollection.Count);
		}

		public void TestISFHeaderExportCustomizedFields()
		{
			var header = SetupISFHeader();
			header.SetUserDefinedValue(Constants.CustomizedFieldConstants.CustomAttribOne, new ZInt(1234));
			header.SetUserDefinedValue(Constants.CustomizedFieldConstants.CustomAttribTwo, new ZInt(5678));
			header.SetUserDefinedValue(CusISFHeader.Schema.CustomAttribute1, new ZDecimal(345.678));
			header.SetUserDefinedValue(CusISFHeader.Schema.CustomAttribute2, new ZDecimal(456.789));
			Factory.SaveForTesting();
			var headerWriter = new ISFHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var shipmentCreated = headerWriter.GetDataObject(header);
			AssertMultilineASCIIEquals("CustomizedFieldCollection data", @"CustomAttrib1 - Integer - 1234
CustomAttrib2 - Integer - 5678
CustomAttribute1 - Decimal - 345.678
CustomAttribute2 - Decimal - 456.789
WriteKey - String - WriteValue", string.Join("\r\n", shipmentCreated.CustomizedFieldCollection.Select(x => $"{x.Key.GetValueOrDefault()} - {x.DataType.GetValueOrDefault()} - {x.Value.GetValueOrDefault()}")));
		}

		public void TestExportCommodityLines()
		{
			var header = SetupISFHeader();
			header.Lines.DeleteAll();
			var manufacturer0 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			var address0 = Factory.New<OrgAddress>();
			address0.OA_Address1 = "IAN TEST ADDRESS1";
			var orgHeader0 = Factory.New<OrgHeader>();
			orgHeader0.OH_Code = "INCTESTMANU";
			orgHeader0.OH_FullName = "INC TEST COMPANY";
			address0.OA_OH = orgHeader0.PK;
			manufacturer0.E2_OA_Address = address0.PK;
			var manufacturer1 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			var address1 = Factory.New<OrgAddress>();
			address1.OA_Address1 = "TEST ADDRESS2";
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTMANU";
			orgHeader1.OH_FullName = "TEST COMPANY";
			address1.OA_OH = orgHeader1.PK;
			manufacturer1.E2_OA_Address = address1.PK;
			var line0 = header.Lines.AddNew();
			line0.BL_ManufacturerDocAddressPK = manufacturer0.PK;
			line0.BL_TextProductCode = "BANANAS";
			line0.BL_FormattedHarmonisedNum = "1010101010";
			line0.BL_RN_NKGoodsOrigin = "CN";
			line0.CustomAttribute1 = "ATTRIBUTE1";
			line0.CustomAttribute2 = "ATTRIBUTE2";
			var line1 = header.Lines.AddNew();
			line1.BL_ManufacturerDocAddressPK = manufacturer0.PK;
			line1.BL_TextProductCode = "BANANAS";
			line1.BL_FormattedHarmonisedNum = "1010101020";
			line1.BL_RN_NKGoodsOrigin = "CN";
			line1.CustomAttribute1 = "CUSTOMSONE";
			line1.CustomAttribute2 = "CUSTOMSTWO";
			var line2 = header.Lines.AddNew();
			line2.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			line2.BL_TextProductCode = "ORANGES";
			line2.BL_FormattedHarmonisedNum = "22221010";
			line2.BL_RN_NKGoodsOrigin = "US";
			line2.CustomAttribute1 = "AAAAAAA";
			line2.CustomAttribute2 = "BBBBBBB";
			var line3 = header.Lines.AddNew();
			line3.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			line3.BL_TextProductCode = "APPLES";
			line3.BL_FormattedHarmonisedNum = "92101000";
			line3.BL_RN_NKGoodsOrigin = "AU";
			var line4 = header.Lines.AddNew();
			line4.BL_TextProductCode = "APPLES";
			line4.BL_FormattedHarmonisedNum = "92101010";
			Factory.SaveForTesting();
			var headerWriter = new ISFHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = headerWriter.GetDataObject(header);
			AssertNotNull(headerData.CommercialInfo);
			AssertEquals(1, headerData.CommercialInfo.CommercialInvoiceCollection.Count);
			AssertEquals(5, headerData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Count);
			var invoiceLineCollection = headerData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
			AssertEquals("9210.10.10", invoiceLineCollection[0].HarmonisedCode);
			if (manufacturer0.PK > manufacturer1.PK)
			{
				AssertEquals("9210.10.00", invoiceLineCollection[1].HarmonisedCode);
				AssertEquals("2222.10.10", invoiceLineCollection[2].HarmonisedCode);
				AssertEquals("1010.10.1010", invoiceLineCollection[3].HarmonisedCode);
				AssertEquals("1010.10.1020", invoiceLineCollection[4].HarmonisedCode);
			}
			else
			{
				AssertEquals("1010.10.1010", invoiceLineCollection[1].HarmonisedCode);
				AssertEquals("1010.10.1020", invoiceLineCollection[2].HarmonisedCode);
				AssertEquals("9210.10.00", invoiceLineCollection[3].HarmonisedCode);
				AssertEquals("2222.10.10", invoiceLineCollection[4].HarmonisedCode);
			}
		}

		void AssertAddInfoContents(UniversalDataBuss.DataObjects.Universal.AddInfo addInfoData, ZString expectedData)
		{
			AssertNotNull(addInfoData);
			AssertEquals(expectedData, addInfoData.Value.GetValueOrDefault());
		}

		void AssertOrganizationAddressContents(OrganizationAddress addressData, OrgAddress expectedAddress)
		{
			AssertNotNull(addressData);
			var orgHeader = Factory.Load<OrgHeader>(expectedAddress.OA_OH);
			AssertNotNull(orgHeader);
			AssertEquals(orgHeader.OH_Code, addressData.OrganizationCode.GetValueOrDefault());
		}

		void AssertEntryNumberContents(UniversalDataBuss.DataObjects.Universal.EntryNumber entryNumberData, ZString expectedNumber, ZString expectedStatus)
		{
			AssertNotNull(entryNumberData);
			AssertEquals(expectedNumber, entryNumberData.Number.GetValueOrDefault());
			AssertEquals(expectedStatus, entryNumberData.EntryStatus.GetCodeAsUpperCase());
		}

		void AssertDateContents(Date dateData, ZDateTime expectedDate)
		{
			AssertNotNull(dateData);
			AssertEquals(expectedDate, dateData.Value.GetValueOrDefault());
		}

		void AssertAdditionalBillContents(AdditionalBill additionalBillData, ZString expectedBillNumber)
		{
			AssertNotNull(additionalBillData);
			AssertEquals(expectedBillNumber, additionalBillData.BillNumber.GetValueOrDefault());
		}

		void AssertAdditionalReferenceContents(AdditionalReference referenceData, ZString expectedNumber)
		{
			AssertNotNull(referenceData);
			AssertEquals(expectedNumber, referenceData.ReferenceNumber.GetValueOrDefault());
		}

		void AssertCustomizedFieldContents(CustomizedField customizedFieldData, ZString expectedData)
		{
			AssertNotNull(customizedFieldData);
			AssertEquals(expectedData, customizedFieldData.Value.GetValueOrDefault());
		}

		CusISFHeader SetupISFHeader()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			var nonUSCompany = Factory.New<GlbCompany>();
			nonUSCompany.GC_Code = "INC";
			nonUSCompany.GC_Name = "IAN TEST COMPANY";
			nonUSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			nonUSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var nonUSBranch = nonUSCompany.Branches.AddNew();
			nonUSBranch.GB_Code = "INB";
			nonUSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMPTST";
			var shipToParty = Factory.New<OrgHeader>();
			shipToParty.OH_Code = "INCSHPTST";
			var buyingParty = Factory.New<OrgHeader>();
			buyingParty.OH_Code = "INCBYPTST";
			var stuffingLocation = Factory.New<OrgHeader>();
			stuffingLocation.OH_Code = "INCSTFTST";
			var sellingParty = Factory.New<OrgHeader>();
			sellingParty.OH_Code = "INCSELTST";
			var consolidator = Factory.New<OrgHeader>();
			consolidator.OH_Code = "INCSOLTST";
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_Code = "INCBOKPTST";
			header.BF_GB = nonUSBranch.PK;
			header.BF_SCAC = "AAAA";
			header.BF_OH_Importer = importer.PK;
			header.BF_OwnerReference = "THIS IS TEST REF";
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_CustomsReference = "ABC-12345678901";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_FirstAcceptedDate = new ZDate(2015, 01, 01);
			header.BF_LastAcceptedDate = new ZDate(2015, 01, 01);
			header.BF_OceanBill = "MWB012345";
			header.BF_HouseBill = "HWB012345";
			header.BF_MasterBill = "MHB012345";
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ImporterCode = "12-12321212";
			header.BF_ImporterFullName = "IAN TEST IMPORTER";
			header.BF_DateOfBirth = ZDateTime.BrettsBirthday;
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.Australia;
			header.BF_ConsigneeCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			header.BF_ConsigneeCode = "12-3456789XY";
			header.BF_BondNumberOrHolder = "BND1234567";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ImporterOrBroker;
			header.BF_BondType = BondTypeList.Codes.ContinuousBond;
			header.BF_SuretyCode = "791";
			header.BF_BondReferenceNumber = "BD323423";
			header.BF_EntryNumber = "ENT21486235";
			header.CustomAttribute1 = "HDRATTRIB1";
			header.CustomAttribute2 = "HDRATTRIB2";
			header.SetUserDefinedValue("WriteKey", new ZString("WriteValue"));
			header.BF_RL_NKPortOfUnload = "USLAX";
			header.BF_RL_NKPlaceOfDelivery = "CNSHA";
			header.MainShipToParty.OrganisationPK = shipToParty.PK;
			header.BuyingParty.OrganisationPK = buyingParty.PK;
			header.StuffingLocation.OrganisationPK = stuffingLocation.PK;
			header.SellingParty.OrganisationPK = sellingParty.PK;
			header.Consolidator.OrganisationPK = consolidator.PK;
			header.BookingParty.OrganisationPK = bookingParty.PK;
			header.BF_SendEquipment = ISF.Business.YesNoDefaultList.Codes.Yes;
			header.BF_ShipmentSubType = ShipmentSubTypeList.Codes.LowValueEntriesShipments;
			header.BF_EstimatedValue = 1234m;
			header.BF_EstimatedQuantity = 1000;
			header.BF_EstimatedQuantityUQ = USPackingingUnitList.ShippingOrPackingingUnitList.Codes.Bag;
			header.BF_EstimatedWeight = 2100;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Kilograms;
			var lineOne = header.Lines.AddNew();
			lineOne.BL_TextProductCode = "IAN TEST PRO ONE";
			lineOne.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.UnitedStates;
			var lineTwo = header.Lines.AddNew();
			lineTwo.BL_TextProductCode = "IAN TEST PRO TWO";
			lineTwo.BL_HarmonisedNum = "9201.10.00";
			var equipOne = header.Equipments.AddNew();
			equipOne.BE_EquipCode = "20";
			equipOne.BE_ContainerNum = "CONT1234";
			var equipTwo = header.Equipments.AddNew();
			equipTwo.BE_EquipCode = "40";
			equipTwo.BE_ContainerNum = "CONT2445";
			return header;
		}
	}
}
