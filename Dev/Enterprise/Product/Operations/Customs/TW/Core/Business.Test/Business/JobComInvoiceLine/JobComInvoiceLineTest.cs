using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Constants;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;
using static Enterprise.Integration.Customs;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : BaseJobComInvoiceLineAbstractTest
	{
		public void TestGetCMHeaderByMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;

			AssertNotNull("Has NX101", invoiceLine.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101));
			AssertNull("No NX601", invoiceLine.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX601));

			invoicelineLinkCMHeader.IsLinkedCMHeader = false;
			AssertNull("Unlink NX101", invoiceLine.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101));
		}

		public void TestIsLinkedNX101WithCertificate19()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			AssertEquals(false, invoiceLine.IsLinkedNX101WithCertificate19);

			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			AssertEquals(true, invoiceLine.IsLinkedNX101WithCertificate19);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			AssertEquals(false, invoiceLine.IsLinkedNX101WithCertificate19);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.IsLinkedNX101WithCertificate19);
		}

		public void TestIsLinkedNX101WithCertificate15()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			AssertEquals(false, invoiceLine.IsLinkedNX101WithCertificate15);

			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			AssertEquals(true, invoiceLine.IsLinkedNX101WithCertificate15);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			AssertEquals(false, invoiceLine.IsLinkedNX101WithCertificate15);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.IsLinkedNX101WithCertificate15);
		}

		public void TestHasLinkedToOtherSameTypeControllingMessageHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			messageHeader2.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			var messageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader3.TW1_CertificateType = CertificateTypeList.Codes.Code6;
			var messageHeader4 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader4.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			messageHeader4.TW1_CertificateType = CertificateTypeList.Codes.Code6;
			var messageHeader5 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader5.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader5.TW1_CertificateType = CertificateTypeList.Codes.Code8;

			line.AssignCMHeaderToInvoices(messageHeader1);
			CombineAssertions(() =>
			{
				AssertEquals("Line - NX101 should be false", false, line.HasLinkedToOtherSameTypeControllingMessageHeader(messageHeader1));
				AssertEquals("Line - NX401 should be false", false, line.HasLinkedToOtherSameTypeControllingMessageHeader(messageHeader2));
				AssertEquals("Line - NX101 should be true", true, line.HasLinkedToOtherSameTypeControllingMessageHeader(messageHeader3));
				AssertEquals("Line - NX601 should be false", false, line.HasLinkedToOtherSameTypeControllingMessageHeader(messageHeader4));
				AssertEquals("Line - NX101 should be true", true, line.HasLinkedToOtherSameTypeControllingMessageHeader(messageHeader5));
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalBizoCaptionsAndDescriptions()
		{
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.CertificateOfOriginNumberInfo, "Certificate of Origin", "The certificate of origin number requested by customs from the importer/exporter.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.CertificateOfOriginNumberItemNumberInfo, "Certificate of Origin Line No.", "The line numbers of the certificate of origin requested by customs from the importer/exporter.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.CitesPermitInfo, "CITES Import Permit", "The import permit number of the Convention on International Trade in Endangered Species.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.ExemptionCodeInfo, "Exemption Reason Code", "The reason of imported goods exempted from type approval.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.FormattedAdValoremDutyRateInfo, "Ad-Valorem Duty Rate", "Ad-Valorem Duty", "The Ad-Valorem Duty rate for imported goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.FormattedSpecificDutyRateInfo, "Specific Duty Rate", "Specific Duty", "The rate and unit of Ad-Valorem Duty for import goods per unit.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.HighTechLicenseInfo, "SHTC Import Permit", "The import permit number of the Strategic High Tech Commodity.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_BrandNameInfo, "Brand Name", "The trademark, brand name and other identification logo of the product. Mandatory for vehicles.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_Calc_InvoiceInfo, "Inv. # for Line", "Invoice Number", "Inv. No.", "The invoice number of the invoice line.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CountryOfOriginInfo, "Goods Origin", "Country/region code of the origin of goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CustomsSecondQuantityInfo, "Statistical Quantity", "Stats. Qty", "Statistical Quantity as indicated by the Tariff item entered.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CustomsValueInfo, "Customs Value", "The price of the dutiable imported goods, used in calculating Ad-valorem Duty.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_DescriptionInfo, "English Description", "English Desc.", "The description of product. Only Western European languages characters are accepted in this field.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_FormattedTariffInfo, "Tariff", "The tariff code of the product.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_HazMatCodeInfo, "DG Code", "The standard classification code for dangerous goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_InvoiceQuantityInfo, "Quantity", "Qty", "The quantity of the dutiable imported goods, used in calculating Specific Duty.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_InvoiceUQInfo, "UQ", "The unit of the product quantity.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_ModelInfo, "Model", "The model of the product. Mandatory for vehicles and food containers.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_NetWeightInfo, "Net Weight", "The gross weight minus packaging (inner and outer packages) weight. System converts the weight in KGM automatically for customs declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_NDescriptionInfo, "Chinese Description", "Chinese Desc.", "The description of product. Both Chinese and Western European languages characters are accepted in this field.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PreviousEntryLineNumberInfo, "Previous Entry Line No.", "The previous entry line number of the re-export/re-import.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PreviousEntryNumberInfo, "Previous Entry Number", "The previous entry number of the re-export/re-import.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PrimaryPreferenceInfo, "Preference", "The duty rates for the first, second and third columns of the tariff.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.PreviousBondedEntryNumberInfo, "Previous Bonded Entry Number", "Pre. Bonded Entry No", "The previous bonded entry number of the bonded goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.PreviousBondedEntryLineNumberInfo, "Previous Bonded Entry Line Number", "Pre. Bonded Entry LNO", "The previous bonded entry line number of the bonded goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.QuotaPermitNumberInfo, "Tariff Rate Quota Certificate", "Tariff Rate Quota Cert", "Quota Cert", "Certificate number of Tariff Rate Quota");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.TrademarkStorageDocsGuidInfo, "Trademark Image", "Select the Trademark Image with the file type: [TDM Trademark Image] from eDocs. It's printed on the export/import entry.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AdditionalDutyRateInfo, "Additional Duty Rate", "Additional Rate", "ADT Rate", "Rate of Additional Duty");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AlcoholAgeInfo, "Age", "The number of years for which the alcohol has been stored.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AlcoholCountryRegionInfo, "Country / Region", "The geographical indication of the alcohol.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AlcoholEndOfShelfLifeInfo, "End of Shelf Life", "The end of shelf life for the commodity applying for inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AlcoholPercentageInfo, "Alcohol by Volume (%)", "The percentage of alcohol by volume.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AlcoholYearInfo, "Year", "The year of the alcohol.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AlteredLotNoAmtInfo, "With Lot Number Altered", "The volume of the product with which the manufacturing lot number has been altered.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AntiDumpingDutyRateInfo, "Anti-Dumping Duty Rate", "Anti-Dumping Rate", "ADD Rate", "Rate of Anti-Dumping Duty");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_BarCodeInfo, "Barcode", "The standard barcode of the product that can be read by the scanner.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_BondedGoodsCodeInfo, "Bonded Goods Code", "The code to identify bonded goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_BottledDateInfo, "Bottled Date", "The date of the wine applying for inspection was bottled.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CarConditionInfo, "Condition", "The condition of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CompositionsInfo, "Specification", "The measurements, specification and component of the goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CusValueConvRatioInfo, "Customs Value Conversion Ratio", "The conversion ratio used to calculate the customs value, special duty rate, specific duty amount and other taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CustomsOwnerPartNoInfo, "Customs Owner Part No.", "Owner Part No.", "The owner's part number.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CustomsSupplierPartNoInfo, "Customs Supplier Part No.", "Supplier Part No.", "The supplier's part number.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CountervailingDutyRateInfo, "Countervailing Duty Rate", "Countervailing Rate", "CVD Rate", "Rate of Countervailing Duty");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CylindersInfo, "Number of Cylinder(s)", "The number of cylinders of the engine of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_DeclarationGoodsDescriptionInfo, "Declaration Goods Description", "The Chinese description and English description entered will populate to this field automatically. The content that exceeds the length limit (512 characters) imposed by customs is only for document printing.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_DisplacementInfo, "Displacement(cc)", "Indicates the capacity of a car engine cylinder, measured in cc's.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_DtyPymntMthdInfo, "Duty Payment Method", "The payment method of import duty.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_EngineTypeInfo, "Engine Type", "The engine type of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_EnteredUnitPriceInfo, "Unit Price", "The unit price of product.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_EPTDigit1Info, "Container Material", "Material", "The first digit of environmental protection tariff indicating the material of container. The environmental protection tariff entered will be declared in the Assigned Number field.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_EPTDigit2Info, "Container Capacity (cc)", "Capacity (cc)", "The second digit of environmental protection tariff indicating the capacity of container (cc). The environmental protection tariff entered will be declared in the Assigned Number field.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_EPTDigit3Info, "No. of Container Material(s)", "No. of Material(s)", "The third digit of environmental protection tariff indicating the quantity of container. The environmental protection tariff entered will be declared in the Assigned Number field.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_EquipmentPrintModeInfo, "Standard Equipment", "Type of Catalyst Converter");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_ExpirationDateInfo, "Expiration Date", "The expiry date of the commodity.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_GearsInfo, "Number of Gear(s)", "The number of gears of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_GroupInfo, "Grouping", "The grouping of products. It will be sent with the goods description for customs declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_HasCatalystConverterInfo, "Catalytic Converter?", "Indicates whether a vehicle using unleaded gasoline is equipped with a catalyst conversion device in the exhaust pipe.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_IMPTariffInfo, "Import Country's Tariff", "Import Tariff", "IMP Tariff", "Indicates the tariff of the Import country. Default the first 8 characters of Invoice Line Tariff when Certificate Type is '15'.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_LHDInfo, "Left Side Steering", "Indicates whether the car is left side steering.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_ManufacturerRelationshipInfo, "Manufacturer Relationship", "Manufacturer Relationship", "Manufacturer Rel.", "Indicates the relationship between manufacturer and seller. When the Certificate Type is  '09', '11', '13', '14', '19', this column must be filled in.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_ModelYearInfo, "Model Year", "The model year of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_NoOriginalLotNoAmtInfo, "Without Original Lot Number", "The volume of the product without manufacturing lot number at the factory.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_NumberOfDoorInfo, "Number of Door(s)", "The number of doors of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PermitQtyInfo, "Permit Quantity", "Permit QTY", "Permit QTY", "Indicates the quantity for Certificate of Origin.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PHValueNumericInfo, "pH Value", "The pH Value of the product at the final equilibrium state. When the goods type entered is \"2\" in accordance with Taiwan Food and Drug Administration, this field is mandatory.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PTCriteriaInfo, "Preferential Treatment Criteria", "Preferential Criteria", "PT Criteria", "Indicates the standard of preferential tariff treatment. When the Certificate Type is  '09', '11', '13', '14', '15', '19', this column must be filled in.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PTCriteria2Info, "Other Preferential Treatment Criteria", "Other PT Criteria", "Other Criteria", "Indicates the other standard of preferential tariff treatment. When the Certificate Type is  '09', '11', '13', '14', '19', this column must be filled in.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_RetaliatoryDutyRateInfo, "Retaliatory Duty Rate", "Retaliatory Rate", "RTD Rate", "Rate of Retaliatory Duty");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_RemovedLotNoAmtInfo, "With Lot Number Removed", "The volume of the product with which the manufacturing lot number has been removed.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_SeatsInfo, "Number of Seat(s)", "The number of seats of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_SterilizationValueNumericInfo, "Sterilization Fo Value", "The thermal death time required. When the goods type entered is \"1\" in accordance with Taiwan Food and Drug Administration, this field is mandatory.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_TariffAdditionalCodeInfo, "Tariff Additional Code", "The code of the product subject to duty deduction or exemption in accordance with Import Tariff Additional Code.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_TariffPrintLengthInfo, "Tariff Printing", "Tariff Printing", "Tariff Printing", "Indicates whether to print the tariff for printing the Certificate of Origin. Tariff Printing cannot be 'N' when the Certificate Type is '15'.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_TextileWidthInfo, "Textile Width", "When the unit of the Customs Quantity is set to MTK - Square Meter, you need to enter the textile length in the Invoice Qty field and enter the textile width under the Other Details tab. The system will convert the unit of length and width into meter and then calculate the Customs Quantity automatically.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_TpfPymntMthdInfo, "TPF Payment Method", "The payment method of Trade Promotion Fee.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_TransmissionInfo, "Transmission", "The transmission shifting mode of the car.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_VatPymntMthdInfo, "Business Tax Payment Method", "The payment method for business tax.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.TypeApprovalAuthorizedPartyInfo, "Authorized Person ID", "The VAT number, ID card number, foreign resident card number or passport number of the authorized person of type approval.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.TypeApprovalCertificateNoInfo, "Certificate", "The certificate number recognized by CCC code in accordance with the regulations of the Bureau of Standards, Metrology and Inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.TypeApprovalPartyIdentifierInfo, "Authorized Person ID Type", "The ID type of VAT number, ID card number, foreign resident card number or passport number.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_ProductGradeInfo, "Grade", "The simplified description of the grading information of the commodity applying for inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_ProductThicknessInfo, "Thickness", "The simplified description of the thickness of the commodity applying for inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_TariffExtensionCodeInfo, "Tariff Extension Code", "The tariff extension code of the commodity applying for inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_GoodsTypeInfo, "Goods Type", "The type of the goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_QuarantineFeaturesInfo, "Features", "The color and characteristics of the imported animal or the botanical nomenclature of the plant.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_QuarantineTreatmentInfo, "Treatment", "The description of the quarantine treatment completed for the declared quarantine goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AnimalAgeMonthInfo, "Age (Month)", "The age of the imported/exported animal (enter age - month). If the age of each animal differs, fill in the oldest of all.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AnimalAgeYearInfo, "Age (Year)", "The age of the imported/exported animal (enter age - year). If the age of each animal differs, fill in the oldest of all.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AnimalFemaleQtyInfo, "Female Quantity", "The number of females of the imported/exported animals.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_AnimalMaleQtyInfo, "Male Quantity", "The number of males of the imported/exported animals.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_MicrochipIDInfo, "Microchip Number", "Microchip No.", "The microchip number implanted in the animal to identify the individual animal.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_VaccinationTypeDateInfo, "Vaccination Type/Date", "The vaccination type and date of the animal applying for quarantine (applicable to some live animals).");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_InnerPackTypeInfo, "Type", "The code of the package type.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_InnerPackingMaterialInfo, "Material", "The code of the packaging material.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_InnerPackDescriptionInfo, "Description", "The description of the package specification.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.PreviousPermitNoInfo, "Previous Permit No", "The previous permit number that was issued before. The previous permit number of the commodity that failed the inspection conducted by the Bureau of Standards, Metrology and Inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.PartyIdentifierInfo, "Authorized Person ID Type", "The ID type of VAT number, ID card number, foreign resident card number or passport number.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.CertificateNoInfo, "Certificate", "The certificate number recognized by CCC code in accordance with the regulations of the Taiwan Food and Drug Administration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.AuthorizedPersonInfo, "Authorized Person ID", "The VAT number, ID card number, foreign resident card number or passport number of the authorized person of medical instrument.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_CustomsThirdQuantityInfo, "Licensing Quantity", "The statistical quantity calculated based on the unit defined by the controlling agency.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(InvoiceLine.JI_PackagingQTYInfo, "Number of Package", "Package", "Indicates the number of inner packages of goods.");
				BusinessObjectCaptionTestHelper.AssertCaptions(InvoiceLine.JI_PackagingUQInfo, "Packaging UQ");
			});
		}

		public void TestJI_OriginCriteriaAttribute()
		{
			var info = InvoiceLine.JI_OriginCriteriaInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("Caption", "Origin Criteria", resourceStringData.Caption);
		}

		public void TestJI_PermitUQAttributes()
		{
			var property = InvoiceLine.JI_PermitUQInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Indicates the quantity unit for Certificate of Origin.", resStrings.FullDescription);
		}

		public void TestJI_PermitUnitPriceAttributes()
		{
			var property = InvoiceLine.JI_PermitUnitPriceInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Permit Unit Price", resStrings.Caption);
				AssertEquals("Indicates the unit price for Certificate of Origin. Default from the Documentary Unit Price of Invoice Line when Certificate Type is '15'.", resStrings.FullDescription);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPlaces == 6);
			});
		}

		[ExpectNoExceptions]
		public void TestJI_CustomPermitUQAttributes()
		{
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(InvoiceLine.JI_CustomPermitUQInfo, "Permit Qty. Unit (For Printing)", "Custom UQ");
		}

		public void TestCalculateFromNetWeightToInvoiceQuantityIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = Constants.UnitOfQuantityCodes.Kilograms;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			invoiceLine.JI_NetWeight = 10m;
			AssertEquals("Invoice Quantity", 1m, invoiceLine.JI_InvoiceQuantity);

			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_NetWeight = 100m;
			AssertEquals("Invoice Quantity", 45.359237m, invoiceLine.JI_InvoiceQuantity);

			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Invoice Quantity", 0.1m, invoiceLine.JI_InvoiceQuantity);

			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals("Invoice Quantity", 45.359237m, invoiceLine.JI_InvoiceQuantity);

			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_InvoiceUQ = Constants.UnitOfQuantityCodes.Gram;
			AssertEquals("Invoice Quantity", 45359.237m, invoiceLine.JI_InvoiceQuantity);
		}

		public void TestCalculateFromInvoiceQuantityToNetWeightIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_InvoiceUQ = Constants.UnitOfQuantityCodes.Tonnes;
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Net Weight", 1m, invoiceLine.JI_NetWeight);

			invoiceLine.JI_NetWeight = 0m;
			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertEquals("Net Weight", 100000m, invoiceLine.JI_NetWeight);

			invoiceLine.JI_NetWeight = 0m;
			invoiceLine.JI_InvoiceUQ = Constants.UnitOfQuantityCodes.Gram;
			AssertEquals("Net Weight", 0.1m, invoiceLine.JI_NetWeight);

			invoiceLine.JI_NetWeight = 0m;
			invoiceLine.JI_InvoiceUQ = Constants.UnitOfQuantityCodes.Pound;
			AssertEquals("Net Weight", 45.359m, invoiceLine.JI_NetWeight);

			invoiceLine.JI_NetWeight = 0m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Net Weight", 45359.237m, invoiceLine.JI_NetWeight);
		}

		public void TestDocumentaryQuantityConverter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var converter = invoiceLine.DocumentaryQuantityConverter;
			AssertType<DocumentaryQuantityConverter>(converter);
		}

		public void TestUpdateJI_CustomsQuantityWhenJI_TariffChanged()
		{
			CreateTariffForTest();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 50m;
			invoiceLine.JI_Tariff = "01012100003";
			AssertEquals(50m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_Tariff = "01012100004";
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_Tariff = "01012100003";
			AssertEquals(50m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_Tariff = "01012100005";
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestUpdateJI_CustomsQuantityWhenJI_NetWeightChanged()
		{
			CreateTariffForTest();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01012100003";
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 50m;
			AssertEquals(50m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = 0m;
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = 40m;
			AssertEquals(40m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = -1m;
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestUpdateJI_CustomsQuantityWhenJI_NetWeightUQChanged()
		{
			CreateTariffForTest();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01012100003";
			invoiceLine.JI_NetWeight = 50m;
			invoiceLine.JI_NetWeightUQ = "KG";
			AssertEquals(50m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = ZString.Empty;
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = "LB";
			AssertEquals(22.679618m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = "YA";
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
		}

		void CreateTariffForTest()
		{
			var tariffTypeHSN = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			var atTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			var hsnCu1Tariff = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffUOM(hsnCu1Tariff.PK, "CU1", "KG");
			var hsnCu2Tariff = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffUOM(hsnCu2Tariff.PK, "CU2", "KG");

			var atCu1Tariff = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffUOM(atCu1Tariff.PK, "CU1", "KG");
			var atCu2Tariff = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffUOM(atCu2Tariff.PK, "CU2", "KG");
			Factory.Save();
		}

		public override void TestCustomsQtyCalculatedByNetWeightOfProductWhenInvoiceQtySet()
		{
			CreateTariffForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var supplier = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "TESTTEST1";
			product1.OP_StockKeepingUnit = "NO";
			product1.OP_NetWeight = 3m;
			product1.OP_WeightUQ = "KG";

			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_OH = supplier.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			product1.PivotsForBinding.AddNew();

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "TESTTEST2";
			product2.OP_StockKeepingUnit = "NO";
			product2.OP_NetWeight = 4m;
			product2.OP_WeightUQ = "KG";

			var relation2 = product2.RelatedOrganisations.AddNew();
			relation2.OU_OH = supplier.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			product2.PivotsForBinding.AddNew();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01012100003";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_InvoiceQuantity = 18m;

			var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceline2.JI_Tariff = "01012100003";
			invoiceline2.JI_CustomsUnitQty = "KG";
			invoiceline2.JI_PartNo = product2.OP_PartNum;
			invoiceline2.JI_InvoiceUQ = "NO";
			invoiceline2.JI_InvoiceQuantity = 18m;

			AssertEquals(54m, invoiceLine.JI_NetWeight);
			AssertEquals(72m, invoiceline2.JI_NetWeight);
			AssertEquals(54m, invoiceLine.JI_CustomsQuantity);
			AssertEquals(72m, invoiceline2.JI_CustomsQuantity);
		}

		public override void TestCustomsQtyCalculatedWhenInvoiceQtySet()
		{
			CreateTariffForTest();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01012100003";
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_InvoiceUQ = "DOZ";
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_InvoiceQuantity = 18m;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);
		}

		public void TestJI_InvoiceQuantityAttributes()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.JI_InvoiceQuantityInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Quantity", resStrings.Caption);
				AssertEquals("Short Caption", "Qty", resStrings.ShortCaption);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPlaces == 4);
			});
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be TW", Core.Constants.CountryCodes.Taiwan, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Taiwan, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestShouldDeleteAllTaxesBeforeDeletingLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();

			var invoiceLineTax = jobComInvoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax.JLT_Tariff = "REPROCESSED2";

			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(JobComInvoiceLineTax));
			query.AddToFilter(JobComInvoiceLineTaxSchema.JLT_JI, jobComInvoiceLine.PK);
			var taxArray = Factory.Load(typeof(JobComInvoiceLineTax), query);
			AssertEquals(1, taxArray.Length);

			jobComInvoiceLine.Delete();
			Factory.Save();
			query = new ZDBOnlyQuery(typeof(JobComInvoiceLineTax));
			query.AddToFilter(JobComInvoiceLineTaxSchema.PK, ((JobComInvoiceLineTax)taxArray[0]).PK);
			taxArray = Factory.Load(typeof(JobComInvoiceLineTax), query);
			Assert(!taxArray.Any());
		}

		public void TestSetDefaultPaymentMethodWhenCountryOfOriginChanged()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeDTY = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateTypeCOM = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			var refCusRateTypeSSG = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG");

			var refCusProcedure = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "71", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);

			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "HWS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTA", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "SSG", refCusRateTypeSSG.PK);

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			referenceDataHelper.CreateTaxOrFee("DDF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);

			refCusProcedure.Attributes.AddNew("COMPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("DTYPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("SSGPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("TATPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("TPFPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("VATPaymentMethod", "DEF");
			Factory.Save();

			var tradeGroupAll = referenceDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			referenceDataHelper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var atTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var rateType = UniversalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var rateCode = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", rateType.PK);
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();

			var alcoholChildtariff = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffRelationship(alcoholChildtariff.PK, atTariffType.PK, "21039090202");
			var rate = UniversalTestHelper.CreateRate(alcoholChildtariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "150 * [LTR]");
			UniversalTestHelper.CreateCusApplicability(rate, tradeGroupAll, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			Factory.Save();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Procedure = "71";
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax.JLT_Tariff = "REPROCESSED2";
			AssertEquals(ZString.Empty, invoiceLineTax.JLT_MethodOfPayment);

			invoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("DEF", invoiceLineTax.JLT_MethodOfPayment);
		}

		public void TestSetDefaultPaymentMethodWhenProcedureChanged()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeDTY = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateTypeCOM = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			var refCusRateTypeSSG = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG");

			var refCusProcedure = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "71", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);

			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "HWS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTA", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "SSG", refCusRateTypeSSG.PK);

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			referenceDataHelper.CreateTaxOrFee("DDF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);

			refCusProcedure.Attributes.AddNew("COMPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("DTYPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("SSGPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("TATPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("TPFPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("VATPaymentMethod", "DEF");
			Factory.Save();

			var tradeGroupAll = referenceDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "All Countries");
			referenceDataHelper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var atTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var rateType = UniversalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var rateCode = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", rateType.PK);
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();

			var alcoholChildtariff = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffRelationship(alcoholChildtariff.PK, atTariffType.PK, "21039090202");
			var rate = UniversalTestHelper.CreateRate(alcoholChildtariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "150 * [LTR]");
			UniversalTestHelper.CreateCusApplicability(rate, tradeGroupAll, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			Factory.Save();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CountryOfOrigin = "US";
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax.JLT_Tariff = "REPROCESSED2";
			AssertEquals(ZString.Empty, invoiceLineTax.JLT_MethodOfPayment);

			invoiceLine.JI_Procedure = "71";
			AssertEquals("DEF", invoiceLineTax.JLT_MethodOfPayment);
		}

		public void TestSetRORDutyTreatmentDefaultPaymentMethodFromCEI_RORPaymentMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_RORPaymentMethod = RORPaymentMethodList.Codes.RorPayment;
			var jobComInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var atTax = jobComInvoiceLine.Taxes.AddNew();
			atTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			var ctTax = jobComInvoiceLine.Taxes.AddNew();
			ctTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			var ssTax = jobComInvoiceLine.Taxes.AddNew();
			ssTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			var ttTax = jobComInvoiceLine.Taxes.AddNew();
			ttTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;

			CombineAssertions(() =>
			{
				jobComInvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				AssertEquals("VAT Payment Method", "ROR", jobComInvoiceLine.JI_VatPymntMthd);
				AssertEquals("DTY Payment Method", "ROR", jobComInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TPF Payment Method", "ROR", jobComInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("AT Payment Method", ZString.Empty, atTax.JLT_MethodOfPayment);
				AssertEquals("CT Payment Method", "ROR", ctTax.JLT_MethodOfPayment);
				AssertEquals("SS Payment Method", "ROR", ssTax.JLT_MethodOfPayment);
				AssertEquals("TT Payment Method", ZString.Empty, ttTax.JLT_MethodOfPayment);
			});

			instruction.CEI_RORPaymentMethod = RORPaymentMethodList.Codes.NonCashPayment;
			jobComInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			atTax = jobComInvoiceLine.Taxes.AddNew();
			atTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			ctTax = jobComInvoiceLine.Taxes.AddNew();
			ctTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			ssTax = jobComInvoiceLine.Taxes.AddNew();
			ssTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			ttTax = jobComInvoiceLine.Taxes.AddNew();
			ttTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;

			CombineAssertions(() =>
			{
				jobComInvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				AssertEquals("VAT Payment Method", "DEF", jobComInvoiceLine.JI_VatPymntMthd);
				AssertEquals("DTY Payment Method", "DEF", jobComInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TPF Payment Method", "DEF", jobComInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("AT Payment Method", ZString.Empty, atTax.JLT_MethodOfPayment);
				AssertEquals("CT Payment Method", "DEF", ctTax.JLT_MethodOfPayment);
				AssertEquals("SS Payment Method", "DEF", ssTax.JLT_MethodOfPayment);
				AssertEquals("TT Payment Method", ZString.Empty, ttTax.JLT_MethodOfPayment);
			});

			instruction.CEI_RORPaymentMethod = ZString.Empty;
			jobComInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			atTax = jobComInvoiceLine.Taxes.AddNew();
			atTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			ctTax = jobComInvoiceLine.Taxes.AddNew();
			ctTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			ssTax = jobComInvoiceLine.Taxes.AddNew();
			ssTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			ttTax = jobComInvoiceLine.Taxes.AddNew();
			ttTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;

			CombineAssertions(() =>
			{
				jobComInvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				AssertEquals("VAT Payment Method", "ROR", jobComInvoiceLine.JI_VatPymntMthd);
				AssertEquals("DTY Payment Method", "ROR", jobComInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TPF Payment Method", "ROR", jobComInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("AT Payment Method", ZString.Empty, atTax.JLT_MethodOfPayment);
				AssertEquals("CT Payment Method", "ROR", ctTax.JLT_MethodOfPayment);
				AssertEquals("SS Payment Method", "ROR", ssTax.JLT_MethodOfPayment);
				AssertEquals("TT Payment Method", ZString.Empty, ttTax.JLT_MethodOfPayment);
			});
		}

		public void TestDutyPaymentMethodDescription()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_DtyPymntMthd = ZString.Empty;
			AssertEquals("Duty-Free", invoiceLine.JI_DtyPymntMthdDescription);

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			AssertEquals("Cash Payment", invoiceLine.JI_DtyPymntMthdDescription);

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("Non-Cash Payment", invoiceLine.JI_DtyPymntMthdDescription);
		}

		public void TestDutyPaymentMethodDefaultByProcedure()
		{
			CreateCusProcedureForTestPaymentMethodDefault();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "5N";
			AssertEquals("DEF", invoiceLine.JI_DtyPymntMthd);

			invoiceLine.JI_Procedure = "5P";
			AssertEquals("CAS", invoiceLine.JI_DtyPymntMthd);

			invoiceLine.JI_Procedure = "";
			AssertNullOrEmpty(invoiceLine.JI_DtyPymntMthd);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = "5N";
			AssertNullOrEmpty(invoiceLine.JI_DtyPymntMthd);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = "5P";
			AssertEquals("CAS", invoiceLine.JI_DtyPymntMthd);

			invoiceLine.JI_Procedure = "5R";
			AssertNullOrEmpty(invoiceLine.JI_DtyPymntMthd);
		}

		public void TestVATPaymentMethodDescription()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_VatPymntMthd = ZString.Empty;
			AssertEquals("Tax-Free", invoiceLine.JI_VatPymntMthdDescription);

			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			AssertEquals("Cash Payment", invoiceLine.JI_VatPymntMthdDescription);

			invoiceLine.JI_VatPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("Non-Cash Payment", invoiceLine.JI_VatPymntMthdDescription);
		}

		public void TestVATPaymentMethodDefaultByProcedure()
		{
			CreateCusProcedureForTestPaymentMethodDefault();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "5N";
			AssertEquals("DEF", invoiceLine.JI_VatPymntMthd);

			invoiceLine.JI_Procedure = "5P";
			AssertEquals("CAS", invoiceLine.JI_VatPymntMthd);

			invoiceLine.JI_Procedure = "";
			AssertNullOrEmpty(invoiceLine.JI_VatPymntMthd);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = "5N";
			AssertNullOrEmpty(invoiceLine.JI_VatPymntMthd);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = "5P";
			AssertEquals("CAS", invoiceLine.JI_VatPymntMthd);

			invoiceLine.JI_Procedure = "5R";
			AssertNullOrEmpty(invoiceLine.JI_VatPymntMthd);
		}

		public void TestTPFPaymentMethodDescription()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_TpfPymntMthd = ZString.Empty;
			AssertEquals("Tax-Free", invoiceLine.JI_TpfPymntMthdDescription);

			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			AssertEquals("Cash Payment", invoiceLine.JI_TpfPymntMthdDescription);

			invoiceLine.JI_TpfPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("Non-Cash Payment", invoiceLine.JI_TpfPymntMthdDescription);
		}

		public void TestTPFPaymentMethodDefaultByProcedure()
		{
			CreateCusProcedureForTestPaymentMethodDefault();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "5N";
			AssertEquals("CAS", invoiceLine.JI_TpfPymntMthd);

			invoiceLine.JI_Procedure = "5P";
			AssertEquals("DEF", invoiceLine.JI_TpfPymntMthd);

			invoiceLine.JI_Procedure = "";
			AssertNullOrEmpty(invoiceLine.JI_TpfPymntMthd);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = "5N";
			AssertNullOrEmpty(invoiceLine.JI_TpfPymntMthd);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = "5P";
			AssertEquals("DEF", invoiceLine.JI_TpfPymntMthd);

			invoiceLine.JI_Procedure = "5R";
			AssertNullOrEmpty(invoiceLine.JI_TpfPymntMthd);
		}

		void CreateCusProcedureForTestPaymentMethodDefault()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var refCusProcedure5N = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "5N", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			refCusProcedure5N.Attributes.AddNew("COMPaymentMethod", "CAS");
			refCusProcedure5N.Attributes.AddNew("DTYPaymentMethod", "DEF");
			refCusProcedure5N.Attributes.AddNew("SSGPaymentMethod", "CAS");
			refCusProcedure5N.Attributes.AddNew("TATPaymentMethod", "DEF");
			refCusProcedure5N.Attributes.AddNew("TPFPaymentMethod", "CAS");
			refCusProcedure5N.Attributes.AddNew("VATPaymentMethod", "DEF");

			var refCusProcedure5P = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "5P", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			refCusProcedure5P.Attributes.AddNew("COMPaymentMethod", "DEF");
			refCusProcedure5P.Attributes.AddNew("DTYPaymentMethod", "CAS");
			refCusProcedure5P.Attributes.AddNew("SSGPaymentMethod", "DEF");
			refCusProcedure5P.Attributes.AddNew("TATPaymentMethod", "CAS");
			refCusProcedure5P.Attributes.AddNew("TPFPaymentMethod", "DEF");
			refCusProcedure5P.Attributes.AddNew("VATPaymentMethod", "CAS");

			var refCusProcedure5R = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "5R", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
		}

		public void TestCVAfterReconReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertHasCustomAttribute<ReadOnlyAttribute>(invoiceLine.GetType(), "JI_CVAfterRecon", true, attrib => attrib.IsReadOnly);
		}

		[TestDate(2021, 01, 02)]
		public void TestJI_CVAfterRecon()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 28.57m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 16372m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 2300m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 50m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "IL";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8419.90.20.00-6";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "IL";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 145.8m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "8419.20.00.00-5";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "IL";
			invoiceLine3.JI_Procedure = "50";
			invoiceLine3.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "4819.10.00.00-1";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "IL";
			invoiceLine4.JI_Procedure = "50";
			invoiceLine4.JI_InvoiceQuantity = 1;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 108m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "84199020006";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "IL";
			invoiceLine5.JI_Procedure = "50";
			invoiceLine5.JI_InvoiceQuantity = 1;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 145.8m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "84199020006";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "IL";
			invoiceLine6.JI_Procedure = "50";
			invoiceLine6.JI_InvoiceQuantity = 2;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 160m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "84199020006";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "IL";
			invoiceLine7.JI_Procedure = "50";
			invoiceLine7.JI_InvoiceQuantity = 2;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 88m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(252815m, invoiceLine1.JI_CVAfterRecon);
				AssertEquals(4763m, invoiceLine2.JI_CVAfterRecon);
				AssertEquals(252814m, invoiceLine3.JI_CVAfterRecon);
				AssertEquals(3528m, invoiceLine4.JI_CVAfterRecon);
				AssertEquals(4763m, invoiceLine5.JI_CVAfterRecon);
				AssertEquals(10455m, invoiceLine6.JI_CVAfterRecon);
				AssertEquals(5750m, invoiceLine7.JI_CVAfterRecon);
			});
		}

		public void TestSetBrandNameIfTrademarkStorageDocsGuidSelected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.TrademarkStorageDocsGuid = Guid.NewGuid();
			AssertEquals("FIG", invoiceLine1.JI_BrandName);

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.TrademarkStorageDocsGuid = Guid.Empty;
			AssertNullOrEmpty(invoiceLine2.JI_BrandName);

			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.TrademarkStorageDocsGuid = ZGuid.Invalid;
			AssertNullOrEmpty(invoiceLine3.JI_BrandName);

			var invoiceLine4 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_BrandName = "XXX";
			invoiceLine4.TrademarkStorageDocsGuid = Guid.NewGuid();
			AssertEquals("XXX", invoiceLine4.JI_BrandName);
		}

		public void TestTrademarkImageWithOrgSupplierPart()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "supplier";
			supplier.OH_IsConsignor = true;
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PRO1";
			var relatedOrganization = part1.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;

			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("foo"), "sample.pdf", MessageConstants.DocumentTypes.TDM);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(Convert.FromBase64String(imageBase64ForTesting), "TestBitmap.bmp", MessageConstants.DocumentTypes.TDM);
			var doc3 = part1.DocManagerInfo().AddFileOrDocument(Convert.FromBase64String(imageBase64ForTesting), "TestBitmap.bmp", MessageConstants.DocumentTypes.TDM);
			Factory.Save();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PRO1";
			AssertEquals("default the link to the product TDM eDocs to the invoice line trademark When the product code has an eDoc uploaded where the document type is TDM.", doc3.UniqueKey, invoiceLine.TrademarkStorageDocsGuid);
			AssertEquals("default the link to the product TDM eDocs to the invoice line trademark When the product code has an eDoc uploaded where the document type is TDM.", doc3, invoiceLine.TrademarkStorageDoc);
			AssertNotNull("default the link to the product TDM eDocs to the invoice line trademark When the product code has an eDoc uploaded where the document type is TDM.", invoiceLine.TrademarkImage);

			invoiceLine.TrademarkStorageDocsGuid = doc2.UniqueKey;
			AssertEquals(doc2, invoiceLine.TrademarkStorageDoc);
			AssertNotNull(invoiceLine.TrademarkImage);

			invoiceLine.TrademarkStorageDocsGuid = doc1.UniqueKey;
			AssertEquals(doc1, invoiceLine.TrademarkStorageDoc);
			AssertNull(invoiceLine.TrademarkImage);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(ZGuid.Empty, invoiceLine.TrademarkStorageDocsGuid);
		}

		public void TestTrademarkImageWithCommercialInvoice()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var docManagerInfo = ((IDocManagerSupport)invoice).DocManagerInfo;
			var doc1 = docManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("foo"), "sample.pdf", MessageConstants.DocumentTypes.TDM);
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals(1, new TrademarkEDocList(invoiceLine.TrademarkStorageDocs).Count);
			AssertEquals(doc1.UniqueKey, invoiceLine.TrademarkStorageDocs.FirstIeDoc().UniqueKey);
			Factory.Save();
			doc1.ParentMain.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declaration = otherFactory.New<JobDeclaration>();
			var invoiceInOtherFactory = otherFactory.Load<JobComInvoiceHeader>(invoice.PK);
			declaration.Invoices.Add(invoiceInOtherFactory);
			var invoiceLineInOtherFactory = invoiceInOtherFactory.InvoiceLines.AddNew() as JobComInvoiceLine;
			AssertEquals(1, new TrademarkEDocList(invoiceLineInOtherFactory.TrademarkStorageDocs).Count);
			AssertEquals(doc1.UniqueKey, invoiceLineInOtherFactory.TrademarkStorageDocs[3][0].UniqueKey);

			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(Convert.FromBase64String(imageBase64ForTesting), "TestBitmap.bmp", MessageConstants.DocumentTypes.TDM);
			AssertEquals(2, new TrademarkEDocList(invoiceLineInOtherFactory.TrademarkStorageDocs).Count);
			AssertEquals(doc2.UniqueKey, invoiceLineInOtherFactory.TrademarkStorageDocs[1][0].UniqueKey);
		}

		public override void TestJI_FormattedTariff()
		{
			var tariff = "87149990905";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "8714.99.90.90-5", InvoiceLine.JI_FormattedTariff);
			tariff = "8714.99.90.90-5";
			InvoiceLine.JI_FormattedTariff = tariff;
			AssertEquals("JI_FormattedTariff", "8714.99.90.90-5", InvoiceLine.JI_FormattedTariff);
		}

		public void TestTariffFormatter()
		{
			var tariffFormatter = (ITariffFormatProvider)InvoiceLine;
			AssertType(typeof(TaiwanTariffFormatter), tariffFormatter.TariffFormatter);
		}

		public void TestPackagesPivot()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			AssertEquals(typeof(InvoiceLinePackagePivotCollection), invoiceLine.PackagesPivot.GetType());
		}

		public void TestGetNewValidationForInvoiceLineCusLinkPackage()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var package = new InvoiceLineCusLinkPackage(invoiceLine);
			AssertEquals(typeof(InvoiceLinePackageValidation), package.GetNewValidation().GetType());
		}

		public void TestPackagesForInvoiceLinesForBindingOnly()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			AssertEquals(typeof(InvoiceLineCusLinkPackageCollection), invoiceLine.PackagesForInvoiceLinesForBindingOnly.GetType());
		}

		public void TestTotalQuantityForPackagesPivot()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";

			AssertEquals(0m, invoiceLine.TotalQuantityForPackagesPivot);

			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var collection = supporter.CusPackPivots;

			var bill = supporter.Declaration.PrimaryMasterBill;
			var packingInformation = bill.Declaration.PackingInformationCollection.AddNew();
			packingInformation.HouseBillContainer = new HouseBillContainer(bill, null);
			var package1 = bill.PackingGroups[0].Packages.AddNew();
			var pivot1 = (ICusQuantityPivot)collection.AddPivotFor(package1);
			pivot1.Quantity = 30;
			AssertEquals(30m, invoiceLine.TotalQuantityForPackagesPivot);

			var package2 = bill.PackingGroups[0].Packages.AddNew();
			var pivot2 = (ICusQuantityPivot)collection.AddPivotFor(package2);
			pivot2.Quantity = 40;
			AssertEquals(70m, invoiceLine.TotalQuantityForPackagesPivot);
		}

		public void TestDefaultValues()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(ZString.Empty, jobComInvoiceLine.JI_HasCatalystConverter);
		}

		public void TestJI_HasCatalystConverter()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			jobComInvoiceLine.JI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.Yes;
			jobComInvoiceLine.JI_EquipmentPrintMode = "XXX";
			AssertEquals("EquipmentPrintMode is 'XXX'", jobComInvoiceLine.JI_EquipmentPrintMode, "XXX");
			AssertEquals("JI_EquipmentPrintModeInfo.ReadOnly'", false, jobComInvoiceLine.JI_EquipmentPrintModeInfo.ReadOnly);

			jobComInvoiceLine.JI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.No;
			AssertEquals("EquipmentPrintMode is ''", jobComInvoiceLine.JI_EquipmentPrintMode, ZString.Empty);
			AssertEquals("JI_EquipmentPrintModeInfo.ReadOnly'", true, jobComInvoiceLine.JI_EquipmentPrintModeInfo.ReadOnly);

			jobComInvoiceLine.JI_EquipmentPrintMode = "XXX";
			jobComInvoiceLine.JI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.Yes;
			AssertEquals("EquipmentPrintMode is 'XXX'", jobComInvoiceLine.JI_EquipmentPrintMode, "XXX");
			AssertEquals("JI_EquipmentPrintModeInfo.ReadOnly'", false, jobComInvoiceLine.JI_EquipmentPrintModeInfo.ReadOnly);
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			var tWCompany = Factory.New<GlbCompany>();
			tWCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var tWBranch = tWCompany.Branches.AddNew();
			tWBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan)).RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = tWBranch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestPrePermitNoCusSupporting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			jobComInvoiceLine.AssignCMHeaderToInvoices(controllingMessageHeader);

			jobComInvoiceLine.PreviousPermitNo = "11111111111111";
			var previousPermitNoCusSupportingCollection = new PreviousPermitNoCusSupportingCollection(jobComInvoiceLine);
			previousPermitNoCusSupportingCollection.Load();
			var prePermitNoCusSupporting = previousPermitNoCusSupportingCollection.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertSame(prePermitNoCusSupporting, jobComInvoiceLine.PrePermitNoCusSupporting);
				AssertEquals("11111111111111", prePermitNoCusSupporting.CSI_ReferenceNumber);
			});

			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, jobComInvoiceLine.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PreviousPermitNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, cusSupportingInfo.Length);

			jobComInvoiceLine.PreviousPermitNo = ZString.Empty;
			Factory.Save();
			cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(0, cusSupportingInfo.Length);
		}

		public void TestShippingIdentificationDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var shippingIdentificationDataCollection1 = new ShippingIdentificationDataCollection(jobComInvoiceLine);
			shippingIdentificationDataCollection1.Load();
			AssertEquals(0, shippingIdentificationDataCollection1.Count);

			var shippingIdentificationData = jobComInvoiceLine.ShippingIdentificationDataCollection.AddNew();
			shippingIdentificationData.TW_ManufacturedLotNo = "XXX";
			Factory.Save();
			var shippingIdentificationDataCollection2 = new ShippingIdentificationDataCollection(jobComInvoiceLine);
			shippingIdentificationDataCollection2.Load();
			AssertEquals(1, shippingIdentificationDataCollection2.Count);
		}

		public void TestPermitCusSupportingCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var permitNumber = jobComInvoiceLine.PermitCusSupportingCollection.AddNew();
			permitNumber.CSI_ReferenceNumber = "XX123456789";
			permitNumber.CSI_LineNo = 5;
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, jobComInvoiceLine.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PermitNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, cusSupportingInfo.Length);
		}

		public void TestAssignedJobComInvLineRefsCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var assignedNumber = jobComInvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber.JG_ReferenceNumber = "XXX";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(JobComInvLineRefs));
			query.AddToFilter(JobComInvLineRefsSchema.JG_JI, jobComInvoiceLine.PK);
			query.AddToFilter(JobComInvLineRefsSchema.JG_ReferenceType, JobComInvLineRefsType.Codes.AssignedNumber);
			var jobComInvLineRefs = Factory.Load(typeof(JobComInvLineRefs), query);
			AssertEquals(1, jobComInvLineRefs.Length);
		}

		public void TestExemptionOfControllingAgenciesCusSupportings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var exemptionOfControllingAgenciesCusSupportings = jobComInvoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			var exemptionOfControllingAgencie = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgencie.CSI_ReferenceNumber = "XX123456789";
			exemptionOfControllingAgencie.CSI_LineNo = 5;

			exemptionOfControllingAgencie = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgencie.CSI_ReferenceNumber = ZString.Empty;
			Factory.Save();

			AssertEquals(1, exemptionOfControllingAgenciesCusSupportings.Count);

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, jobComInvoiceLine.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PermitExemptionCodes);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, cusSupportingInfo.Length);
			AssertEquals("XX123456789", ((CusSupportingInfo)cusSupportingInfo[0]).CSI_ReferenceNumber);
		}

		public void TestNewOwnerProductSyncManager()
		{
			var newOwnerProductSyncManager = InvoiceLine.NewOwnerProductSyncManager;
			AssertNotNull(newOwnerProductSyncManager);
		}

		public void TestJI_OwnerProduct()
		{
			var newOwnerProductSyncManager = InvoiceLine.NewOwnerProductSyncManager;
			newOwnerProductSyncManager.ReloadPart = false;
			InvoiceLine.JI_OwnerProduct = Part1.PK;
			AssertEquals(true, newOwnerProductSyncManager.ReloadPart);
		}

		public void TestNewOwnerProduct()
		{
			InvoiceLine.JI_OwnerProduct = Part1.PK;
			AssertEquals(Part1, InvoiceLine.NewOwnerProduct);
		}

		public void TestJI_NewOwnerPartNo()
		{
			InvoiceLine.JI_NewOwnerPartNo = Part1.OP_PartNum;
			InvoiceLine.JI_NewPartAttribute1 = "XX1";
			InvoiceLine.JI_NewPartAttribute2 = "XX2";
			InvoiceLine.JI_NewPartAttribute3 = "XX3";
			InvoiceLine.JI_NewSerialNumber = "SN";

			InvoiceLine.JI_NewOwnerPartNo = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceLine.JI_NewPartAttribute1);
			AssertEquals(ZString.Empty, InvoiceLine.JI_NewPartAttribute2);
			AssertEquals(ZString.Empty, InvoiceLine.JI_NewPartAttribute3);
			AssertEquals(ZString.Empty, InvoiceLine.JI_NewSerialNumber);
		}

		public void TestJI_PartNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();

			var importerOrgHeader = Factory.New<OrgHeader>();
			importerOrgHeader.OH_Code = "IMP01";
			importerOrgHeader.OH_FullName = "importer";
			declaration.JE_OH_Importer = importerOrgHeader.PK;

			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_OH_Owner = OwnerOrgHeader.PK;
			jobComInvoiceLine.JI_CEI = instruction.PK;
			jobComInvoiceLine.JI_PartAttrib1 = "XXX";

			importerOrgHeader.MiscServ.OM_IMPartAttrib1Type = "OW2";
			importerOrgHeader.MiscServ.OM_IMPartAttrib1Name = "TTT2";

			OwnerOrgHeader.MiscServ.OM_IMPartAttrib1Type = "OW2";
			OwnerOrgHeader.MiscServ.OM_IMPartAttrib1Name = "TTT2";

			Factory.Save();

			jobComInvoiceLine.JI_PartNo = Part1.OP_PartNum;
			AssertEquals(jobComInvoiceLine.JI_NewOwnerPartNo, jobComInvoiceLine.JI_PartNo);
			AssertEquals(jobComInvoiceLine.JI_PartAttrib1, jobComInvoiceLine.JI_NewPartAttribute1);
			AssertEquals(ZString.Empty, jobComInvoiceLine.JI_NewPartAttribute2);
			AssertEquals(ZString.Empty, jobComInvoiceLine.JI_NewPartAttribute3);

			jobComInvoiceLine.JI_NewPartAttribute1 = ZString.Empty;
			jobComInvoiceLine.JI_NewOwnerPartNo = ZString.Empty;
			jobComInvoiceLine.JI_PartNo = Part2.OP_PartNum;
			AssertEquals(jobComInvoiceLine.JI_NewOwnerPartNo, jobComInvoiceLine.JI_PartNo);
			AssertEquals(ZString.Empty, jobComInvoiceLine.JI_NewPartAttribute1);
		}

		public void TestJI_CEI_Description()
		{
			var instruction = Declaration.CusEntryInstruction;
			instruction.CEI_Description = "XXX";
			InvoiceLine.JI_CEI = instruction.PK;
			AssertEquals("XXX", InvoiceLine.JI_CEI_Description);

			Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, InvoiceLine.JI_CEI_Description);
		}

		public void TestInvoiceLineLinkControllingMsgHeadersWhenDetachInvoice()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var header = jobDeclaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";

			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);

			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;

			Factory.Save();
			AssertEquals(3, InvoiceLineLinkControllingMsgHeaderCollectionTest.GetLinkPKs(Factory, line.PK).Length);

			var newFactory = new BusinessObjectFactory();
			jobDeclaration = newFactory.Load<JobDeclaration>(jobDeclaration.PK);
			header = jobDeclaration.Invoices[0];
			AssertEquals(1, jobDeclaration.InvoiceLines.Count);//If I don't have this line of code,CreateNewInvoiceLineCollectionWhenDeclarationIsNull() will not have data
			header.JZ_JE = ZGuid.Empty;

			var genPivot = InvoiceLineLinkControllingMsgHeaderCollectionTest.GetGenPivots(Factory, line.PK).First();
			newFactory.Save();
			AssertNull(Factory.Load<GenPivot>(genPivot.PK));
		}

		public void TestInvoiceLineLinkControllingMsgHeadersWhenAttachInvoice()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";

			var header = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			header.InvoiceLines.AddNew();

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			jobDeclaration = newFactory.Load<JobDeclaration>(jobDeclaration.PK);
			header = newFactory.Load<JobComInvoiceHeader>(header.PK);
			header.JZ_JE = jobDeclaration.PK;
			var line = (JobComInvoiceLine)header.InvoiceLines[0];
			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
		}

		public void TestCustomsQuantityInKG()
		{
			AssertEquals("CustomsQuantityInKG", ZDecimal.Zero, InvoiceLine.CustomsQuantityInKG);

			InvoiceLine.JI_CustomsQuantity = 10m;
			InvoiceLine.JI_CustomsUnitQty = string.Empty;
			AssertEquals("CustomsQuantityInKG", ZDecimal.Zero, InvoiceLine.CustomsQuantityInKG);

			InvoiceLine.JI_CustomsUnitQty = "KGM";
			AssertEquals("CustomsQuantityInKG", 10m, InvoiceLine.CustomsQuantityInKG);

			InvoiceLine.JI_CustomsUnitQty = "TNE";
			AssertEquals("CustomsQuantityInKG", 10000m, InvoiceLine.CustomsQuantityInKG);
		}

		public void TestSetDefaultValues()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst1 = declaration1.CusEntryInstruction;
			testInst1.CEI_Style = "D1";
			testInst1.CEI_Description = "D1 Description";
			var testInvoiceHeader = declaration1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var testInvLine1 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("N", testInvLine1.JI_TariffPrintLength);
			Factory.Save();

			AssertEquals(ZString.Empty, testInvLine1.JI_HasCatalystConverter);
			AssertEquals(ZString.Empty, testInvLine1.JI_PrimaryPreference);

			AssertEquals(testInst1.PK, testInvLine1.JI_CEI);
			AssertEquals(testInst1.CEI_Description, testInvLine1.JI_CEI_Description);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();

			var testInvLine2 = declaration2.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals(declaration2.CusEntryInstruction.PK, testInvLine2.JI_CEI);
			AssertEquals(declaration2.CusEntryInstruction.CEI_Description, testInvLine2.JI_CEI_Description);

			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			AssertEquals(ZString.Empty, invoiceLine.JI_HasCatalystConverter);

			var factory3 = new BusinessObjectFactory();
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(factory3);
			var preferencePRE = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Preference, "Preference", "TW");
			var preferenceSTD = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Standard, "Standard", "TW");
			var declaration3 = factory3.NewWithValidTestData<JobDeclaration>();
			factory3.Save();

			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;
			var tradeGroup = universalReferenceTestDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "TEST1", date1, date2);
			universalReferenceTestDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, date1.Date, date2.Date);
			var hsnTariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType("TW", "HSN");
			factory3.Save();
			var rateType = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType("TW", Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(factory3, "DTA", rateType.PK);
			factory3.Save();
			var cusTariff = universalReferenceTestDataHelper.CreateTariff("TW", hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			var testRate1 = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceSTD.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(testRate1, tradeGroup, date1, date2, "add11", "ord11");
			var testRate2 = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePRE.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(testRate2, tradeGroup, date1, date2, "add21", "ord21");
			factory3.Save();

			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testInvLine3 = declaration3.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals("no default JI_PrimaryPreference value due to Preference list is empty when UseUniversalTariff and No UniversalTariff", ZString.Empty, testInvLine1.JI_PrimaryPreference);
			testInvLine3.JI_Tariff = "123456789";
			testInvLine3.JI_CountryOfOrigin = "AU";
			AssertEquals("Has default JI_PrimaryPreference value when UseUniversalTariff and has UniversalTariff", Constants.PreferenceCodes.Standard, testInvLine3.JI_PrimaryPreference);

			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, testInvLine3.JI_PrimaryPreference);
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(Constants.PreferenceCodes.Standard, testInvLine3.JI_PrimaryPreference);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = "IMP";
			var invoice1 = declaration4.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var entryInstruction = declaration4.CusEntryInstruction;

			invoiceLine1.JI_CusValueConvRatio = 1;
			entryInstruction.CEI_Style = "F1";
			AssertEquals(1m, invoiceLine1.JI_CusValueConvRatio);
			entryInstruction.CEI_Style = "F2";
			AssertEquals(ZDecimal.Zero, invoiceLine1.JI_CusValueConvRatio);

			invoiceLine1.JI_CusValueConvRatio = 1;
			entryInstruction.CEI_Style = ZString.Empty;
			entryInstruction.CEI_ReasonForDuty = "02";
			AssertEquals(1m, invoiceLine1.JI_CusValueConvRatio);
			entryInstruction.CEI_ReasonForDuty = "01";
			AssertEquals(ZDecimal.Zero, invoiceLine1.JI_CusValueConvRatio);

			invoiceLine1.JI_CusValueConvRatio = 1;
			entryInstruction.CEI_ReasonForDuty = ZString.Empty;
			entryInstruction.CEI_CustomsOffice = "CA";
			AssertEquals(1m, invoiceLine1.JI_CusValueConvRatio);
			entryInstruction.CEI_CustomsOffice = "CB";
			AssertEquals(ZDecimal.Zero, invoiceLine1.JI_CusValueConvRatio);
		}

		public void TestCarInfoRules()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Gears = 3;
			invoiceLine.JI_Transmission = TransmissionCodeList.Codes.Manual;
			AssertEquals(new ZShort(3), invoiceLine.JI_Gears);
			AssertEquals(false, invoiceLine.JI_GearsInfo.ReadOnly);

			invoiceLine.JI_Transmission = TransmissionCodeList.Codes.CVT;
			AssertEquals(ZShort.Zero, invoiceLine.JI_Gears);
			AssertEquals(true, invoiceLine.JI_GearsInfo.ReadOnly);

			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_NumberOfDoor = 5;
			invoiceLine1.JI_LHD = LeftSideSteeringCodeList.Codes.Right;
			invoiceLine1.JI_CarType = CarTypeCodeList.Codes.A1;
			AssertEquals(new ZShort(5), invoiceLine1.JI_NumberOfDoor);
			AssertEquals(LeftSideSteeringCodeList.Codes.Right, invoiceLine1.JI_LHD);
			AssertEquals(false, invoiceLine1.JI_NumberOfDoorInfo.ReadOnly);
			AssertEquals(false, invoiceLine1.JI_LHDInfo.ReadOnly);

			invoiceLine1.JI_CarType = CarTypeCodeList.Codes.H1;
			AssertEquals(ZShort.Zero, invoiceLine1.JI_NumberOfDoor);
			AssertEquals(ZString.Empty, invoiceLine1.JI_LHD);
			AssertEquals(true, invoiceLine1.JI_NumberOfDoorInfo.ReadOnly);
			AssertEquals(true, invoiceLine1.JI_LHDInfo.ReadOnly);

			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_NumberOfDoor = 5;
			invoiceLine2.JI_Displacement = "6000";
			invoiceLine2.JI_Cylinders = 12;
			invoiceLine2.JI_Seats = 2;
			invoiceLine2.JI_Transmission = TransmissionCodeList.Codes.Auto;
			invoiceLine2.JI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.Yes;
			invoiceLine2.JI_LHD = LeftSideSteeringCodeList.Codes.Left;

			invoiceLine2.JI_CarType = CarTypeCodeList.Codes.A1;

			AssertEquals(new ZShort(5), invoiceLine2.JI_NumberOfDoor);
			AssertEquals("6000", invoiceLine2.JI_Displacement);
			AssertEquals(new ZShort(12), invoiceLine2.JI_Cylinders);
			AssertEquals(new ZShort(2), invoiceLine2.JI_Seats);
			AssertEquals(TransmissionCodeList.Codes.Auto, invoiceLine2.JI_Transmission);
			AssertEquals(CatalystConverterPrintModeList.Codes.Yes, invoiceLine2.JI_HasCatalystConverter);
			AssertEquals(LeftSideSteeringCodeList.Codes.Left, invoiceLine2.JI_LHD);
			AssertEquals(false, invoiceLine2.JI_NumberOfDoorInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.JI_DisplacementInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.JI_CylindersInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.JI_SeatsInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.JI_TransmissionInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.JI_HasCatalystConverterInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.JI_LHDInfo.ReadOnly);

			invoiceLine2.JI_CarType = CarTypeCodeList.Codes.J1;

			AssertEquals(ZShort.Zero, invoiceLine2.JI_NumberOfDoor);
			AssertEquals(ZString.Empty, invoiceLine2.JI_Displacement);
			AssertEquals(ZShort.Zero, invoiceLine2.JI_Cylinders);
			AssertEquals(ZShort.Zero, invoiceLine2.JI_Seats);
			AssertEquals(ZString.Empty, invoiceLine2.JI_Transmission);
			AssertEquals(ZString.Empty, invoiceLine2.JI_HasCatalystConverter);
			AssertEquals(ZString.Empty, invoiceLine2.JI_LHD);
			AssertEquals(true, invoiceLine2.JI_NumberOfDoorInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.JI_DisplacementInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.JI_CylindersInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.JI_SeatsInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.JI_TransmissionInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.JI_HasCatalystConverterInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.JI_LHDInfo.ReadOnly);
		}

		public void TestEmptyCarInfoFields()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "86";

			invoiceLine.JI_CarType = CarTypeCodeList.Codes.A1;
			invoiceLine.JI_Model = "SSS";
			invoiceLine.JI_BrandName = "AAA";
			invoiceLine.JI_Transmission = TransmissionCodeList.Codes.Auto;
			invoiceLine.JI_EngineType = EngineTypeCodeList.Codes.CG;
			invoiceLine.JI_LHD = LeftSideSteeringCodeList.Codes.Left;
			invoiceLine.JI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.Yes;
			invoiceLine.JI_EquipmentPrintMode = EquipmentPrintModeList.Codes.EEC;
			invoiceLine.JI_CarCondition = CarConditionCodeList.Codes.CrashTruck;
			invoiceLine.JI_ModelYear = 2018;
			invoiceLine.JI_Displacement = "2500";
			invoiceLine.JI_NumberOfDoor = 5;
			invoiceLine.JI_Seats = 5;
			invoiceLine.JI_Cylinders = 6;
			invoiceLine.JI_Gears = 6;
			invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();

			AssertEquals(CarTypeCodeList.Codes.A1, invoiceLine.JI_CarType);
			AssertEquals("SSS", invoiceLine.JI_Model);
			AssertEquals("AAA", invoiceLine.JI_BrandName);
			AssertEquals(TransmissionCodeList.Codes.Auto, invoiceLine.JI_Transmission);
			AssertEquals(EngineTypeCodeList.Codes.CG, invoiceLine.JI_EngineType);
			AssertEquals(LeftSideSteeringCodeList.Codes.Left, invoiceLine.JI_LHD);
			AssertEquals(CatalystConverterPrintModeList.Codes.Yes, invoiceLine.JI_HasCatalystConverter);
			AssertEquals(EquipmentPrintModeList.Codes.EEC, invoiceLine.JI_EquipmentPrintMode);
			AssertEquals(CarConditionCodeList.Codes.CrashTruck, invoiceLine.JI_CarCondition);
			AssertEquals(new ZShort(2018), invoiceLine.JI_ModelYear);
			AssertEquals("2500", invoiceLine.JI_Displacement);
			AssertEquals(new ZShort(5), invoiceLine.JI_NumberOfDoor);
			AssertEquals(new ZShort(5), invoiceLine.JI_Seats);
			AssertEquals(new ZShort(6), invoiceLine.JI_Cylinders);
			AssertEquals(new ZShort(6), invoiceLine.JI_Gears);
			AssertEquals(1, invoiceLine.ChassisJobComInvLineRefsCollection.Count);

			invoiceLine.JI_Tariff = "87";
			AssertEquals(CarTypeCodeList.Codes.A1, invoiceLine.JI_CarType);
			AssertEquals("SSS", invoiceLine.JI_Model);
			AssertEquals("AAA", invoiceLine.JI_BrandName);
			AssertEquals(TransmissionCodeList.Codes.Auto, invoiceLine.JI_Transmission);
			AssertEquals(EngineTypeCodeList.Codes.CG, invoiceLine.JI_EngineType);
			AssertEquals(LeftSideSteeringCodeList.Codes.Left, invoiceLine.JI_LHD);
			AssertEquals(CatalystConverterPrintModeList.Codes.Yes, invoiceLine.JI_HasCatalystConverter);
			AssertEquals(EquipmentPrintModeList.Codes.EEC, invoiceLine.JI_EquipmentPrintMode);
			AssertEquals(CarConditionCodeList.Codes.CrashTruck, invoiceLine.JI_CarCondition);
			AssertEquals(new ZShort(2018), invoiceLine.JI_ModelYear);
			AssertEquals("2500", invoiceLine.JI_Displacement);
			AssertEquals(new ZShort(5), invoiceLine.JI_NumberOfDoor);
			AssertEquals(new ZShort(5), invoiceLine.JI_Seats);
			AssertEquals(new ZShort(6), invoiceLine.JI_Cylinders);
			AssertEquals(new ZShort(6), invoiceLine.JI_Gears);
			AssertEquals(1, invoiceLine.ChassisJobComInvLineRefsCollection.Count);

			invoiceLine.JI_Tariff = "40";
			AssertEquals(ZString.Empty, invoiceLine.JI_CarType);
			AssertEquals("SSS", invoiceLine.JI_Model);
			AssertEquals("AAA", invoiceLine.JI_BrandName);
			AssertEquals(ZString.Empty, invoiceLine.JI_Transmission);
			AssertEquals(ZString.Empty, invoiceLine.JI_EngineType);
			AssertEquals(ZString.Empty, invoiceLine.JI_LHD);
			AssertEquals(ZString.Empty, invoiceLine.JI_HasCatalystConverter);
			AssertEquals(ZString.Empty, invoiceLine.JI_EquipmentPrintMode);
			AssertEquals(ZString.Empty, invoiceLine.JI_CarCondition);
			AssertEquals(ZShort.Zero, invoiceLine.JI_ModelYear);
			AssertEquals(ZString.Empty, invoiceLine.JI_Displacement);
			AssertEquals(ZShort.Zero, invoiceLine.JI_NumberOfDoor);
			AssertEquals(ZShort.Zero, invoiceLine.JI_Seats);
			AssertEquals(ZShort.Zero, invoiceLine.JI_Cylinders);
			AssertEquals(ZShort.Zero, invoiceLine.JI_Gears);
			AssertEquals(0, invoiceLine.ChassisJobComInvLineRefsCollection.Count);
		}

		public override void TestWipeNKTaxType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure("TW", "IM", "5E", ZString.Empty, ZString.Empty, "外交郵袋", "IMP", group: "G3,D2,");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";

			var header = dec.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line.JI_ZZF_NKTaxType = "605";
			line.JI_Procedure = "5E";
			Assert(line.ShouldWipeNKTaxType);
			AssertEquals(line.ShouldWipeNKTaxType ? "" : "605", line.JI_ZZF_NKTaxType);

			line.JI_ZZF_NKTaxType = "605";
			line.JI_Procedure = "ZZ";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals("605", line.JI_ZZF_NKTaxType);

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.JI_Procedure = "5E";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals("605", line.JI_ZZF_NKTaxType);
		}

		public override void TestJI_PreviousProcedure()
		{
			void TestRunner(ZString fullCode, ZString expectedPPC)
			{
				InvoiceLine.JI_Procedure = fullCode;
				AssertEquals(fullCode, InvoiceLine.JI_Procedure);
				AssertEquals(expectedPPC, InvoiceLine.JI_Calc_PreviousProcedure);
			}

			CombineAssertions(delegate
			{
				TestRunner("12", "");
				TestRunner("", "");
			});
		}

		public void TestTypeApprovalCertificateNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var typeApprovalCertificateNumbers = jobComInvoiceLine.TypeApprovalCertificateNumbers;
			AssertEquals(typeof(TypeApprovalCertificateNumberCusSupporting), typeApprovalCertificateNumbers.GetType());

			typeApprovalCertificateNumbers.CSI_ReferenceNumber = "ABC11111111111";
			typeApprovalCertificateNumbers.CSI_ReferenceNumber2 = "XXX";
			typeApprovalCertificateNumbers.CSI_Description = "53";
			typeApprovalCertificateNumbers.CSI_Code = "C";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, jobComInvoiceLine.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, cusSupportingInfo.Length);

			typeApprovalCertificateNumbers = (TypeApprovalCertificateNumberCusSupporting)cusSupportingInfo[0];
			AssertEquals("ABC11111111111", typeApprovalCertificateNumbers.CSI_ReferenceNumber);
			AssertEquals("XXX", typeApprovalCertificateNumbers.CSI_ReferenceNumber2);
			AssertEquals("53", typeApprovalCertificateNumbers.CSI_Description);
			AssertEquals("C", typeApprovalCertificateNumbers.CSI_Code);

			typeApprovalCertificateNumbers.CSI_ReferenceNumber = ZString.Empty;
			typeApprovalCertificateNumbers.CSI_ReferenceNumber2 = ZString.Empty;
			typeApprovalCertificateNumbers.CSI_Description = ZString.Empty;
			typeApprovalCertificateNumbers.CSI_Code = ZString.Empty;
			Factory.Save();

			query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, jobComInvoiceLine.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber);
			cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(0, cusSupportingInfo.Length);
		}

		public void TestCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals(jobComInvoiceLine.JI_Calc_CIF_InLocalCurrency, jobComInvoiceLine.JI_CustomsValue);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals(jobComInvoiceLine.JI_Calc_FOB_InLocalCurrency, jobComInvoiceLine.JI_CustomsValue);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertNullOrEmpty("ChargeTypeList not has 'EXW'", chargeTypeList1.GetDescriptionFromCode("EXW"));
			AssertNullOrEmpty("ChargeTypeList not has 'OTH'", chargeTypeList1.GetDescriptionFromCode("OTH"));

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertNullOrEmpty("ChargeTypeList not has 'EXW'", chargeTypeList1.GetDescriptionFromCode("EXW"));
			AssertNullOrEmpty("ChargeTypeList not has 'OTH'", chargeTypeList1.GetDescriptionFromCode("OTH"));
		}
		public void TestMedicalInstrumentOrFood()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var medicalInstrumentOrFood = jobComInvoiceLine.MedicalInstrumentOrFoodCusSupporting;
			AssertEquals(typeof(MedicalInstrumentOrFoodCusSupporting), medicalInstrumentOrFood.GetType());

			medicalInstrumentOrFood.CSI_ReferenceNumber = "ABC11111111111";
			medicalInstrumentOrFood.CSI_ReferenceNumber2 = "XXX11111111111";
			medicalInstrumentOrFood.CSI_Code = "AC";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, jobComInvoiceLine.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, cusSupportingInfo.Length);

			medicalInstrumentOrFood = (MedicalInstrumentOrFoodCusSupporting)cusSupportingInfo[0];
			AssertEquals("ABC11111111111", medicalInstrumentOrFood.CSI_ReferenceNumber);
			AssertEquals("XXX11111111111", medicalInstrumentOrFood.CSI_ReferenceNumber2);
			AssertEquals("AC", medicalInstrumentOrFood.CSI_Code);
		}

		public void TestPartyIdentifier()
		{
			InvoiceLine.PartyIdentifier = PartyIdentifierCodeList.Codes._174;
			InvoiceLine.AuthorizedPerson = "XXXXXXXXXXXXXX";
			AssertEquals("XXXXXXXXXXXXXX", InvoiceLine.AuthorizedPerson);

			InvoiceLine.PartyIdentifier = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceLine.AuthorizedPerson);
		}

		static readonly Dictionary<ZString, ImmutableHashSet<string>> EditableProperties = new Dictionary<ZString, ImmutableHashSet<string>>()
		{
			{
				JobComInvoiceLine.Schema.PartyIdentifier, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX603
				)
			},
			{
				JobComInvoiceLine.Schema.CertificateNo, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.AuthorizedPerson, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_PHValue, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_PHValueNumeric, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_BarCode, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_SterilizationValue, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_SterilizationValueNumeric, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX201_01,
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX401,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_GoodsType, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.PreviousPermitNo, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_ProductThickness, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_ProductGrade, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_TariffExtensionCode, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			},
			{
				JobComInvoiceLine.Schema.JI_InnerPackDescription, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_InnerPackingMaterial, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				JobComInvoiceLine.Schema.JI_InnerPackType, ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX601
					)
			},
		};

		static readonly Dictionary<ZString, ImmutableHashSet<string>> EditableCollections = new Dictionary<ZString, ImmutableHashSet<string>>()
		{
			{
				"FoodDataCollection", ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
				)
			},
			{
				"StorageAndShippingConditionJobComInvLineRefsCollection", ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX601
					)
			},
			{
				"ShippingIdentificationDataCollection", ImmutableHashSet.Create(
					ControllingMessageTypeList.Codes.NX301,
					ControllingMessageTypeList.Codes.NX301_AX,
					ControllingMessageTypeList.Codes.NX301_DN,
					ControllingMessageTypeList.Codes.NX601,
					ControllingMessageTypeList.Codes.NX603
					)
			}
		};

		void AssertPropertyReadOnlyAndClearValue(Func<JobComInvoiceLine, ZPropertyInfo> selector)
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = decl.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var invoice = decl.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			var propertyInfo = selector.Invoke(line);
			var propertyName = propertyInfo.Name;
			var ediableMessageTypes = EditableProperties[propertyName];
			foreach (var msgType in ediableMessageTypes)
			{
				controllingMessageHeader.TW1_ControllingMessageType = msgType;
				var invoiceLineLinkControllingMsgHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault();
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
				propertyInfo.SetNotEmptyValue();
				Assert($"{propertyName} should not be empty", !propertyInfo.Value.IsEmpty);
				Assert($"Should be ediable when linked to a {msgType} controlling message header.", !propertyInfo.ReadOnly);

				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = false;
				Assert($"Should be readonly when doesn't link to a {msgType} controlling message header.", propertyInfo.ReadOnly);
				Assert($"{propertyName} should be empty", propertyInfo.Value.IsEmpty);
			}

			var readOnlyMessageTypes = new ControllingMessageTypeList().Cast<ICodeDescription>().Select(x => x.Code).Except(ediableMessageTypes);
			foreach (var msgType in readOnlyMessageTypes)
			{
				controllingMessageHeader.TW1_ControllingMessageType = msgType;
				var invoiceLineLinkControllingMsgHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault();
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
				Assert($"Should be readonly when linked to a {msgType} controlling message header.", propertyInfo.ReadOnly);
			}
		}

		void AssertBusinessObjectCollectionReadOnlyAndClearValue(Func<JobComInvoiceLine, ZString> selector)
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = decl.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var invoice = decl.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			var collectionName = selector.Invoke(line);
			var collection = (IBusinessObjectCollection)line.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).First(prop => prop.Name == collectionName).GetValue(line);
			var ediableMessageTypes = EditableCollections[collectionName];
			foreach (var msgType in ediableMessageTypes)
			{
				controllingMessageHeader.TW1_ControllingMessageType = msgType;
				var invoiceLineLinkControllingMsgHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault();
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
				collection.AddNew();
				AssertEquals($"{collectionName} should not be empty", 1, collection.Count);
				Assert($"Should be ediable when linked to a {msgType} controlling message header.", !collection.ReadOnly);

				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = false;
				Assert($"Should be readonly when doesn't link to a {msgType} controlling message header.", collection.ReadOnly);
				AssertEquals($"{collectionName} should be empty", 0, collection.Count);
			}

			var readOnlyMessageTypes = new ControllingMessageTypeList().Cast<ICodeDescription>().Select(x => x.Code).Except(ediableMessageTypes);
			foreach (var msgType in readOnlyMessageTypes)
			{
				controllingMessageHeader.TW1_ControllingMessageType = msgType;
				var invoiceLineLinkControllingMsgHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault();
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
				Assert($"Should be readonly when linked to a {msgType} controlling message header.", collection.ReadOnly);
			}
		}

		public void TestFoodAndDrugReadOnly()
		{
			AssertPropertyReadOnlyAndClearValue(x => x.PartyIdentifierInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.CertificateNoInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.AuthorizedPersonInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_PHValueInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_PHValueNumericInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_BarCodeInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_SterilizationValueInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_SterilizationValueNumericInfo);
			AssertBusinessObjectCollectionReadOnlyAndClearValue(x => nameof(x.FoodDataCollection));
			AssertBusinessObjectCollectionReadOnlyAndClearValue(x => nameof(x.StorageAndShippingConditionJobComInvLineRefsCollection));
		}

		public void TestJI_PHValueNumeric()
		{
			var line = Factory.New<JobComInvoiceLine>();
			line.JI_PHValue = "Er";
			AssertEquals(ZDecimal.Zero, line.JI_PHValueNumeric);

			line.JI_PHValue = "12";
			AssertEquals(12m, line.JI_PHValueNumeric);

			line.JI_PHValueNumeric = 20m;
			AssertEquals("20", line.JI_PHValue);
		}

		public void TestJI_SterilizationValueNumeric()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_SterilizationValue = "Er";
			AssertEquals(ZDecimal.Zero, line.JI_SterilizationValueNumeric);

			line.JI_SterilizationValue = "12";
			AssertEquals(12m, line.JI_SterilizationValueNumeric);

			line.JI_SterilizationValueNumeric = 20m;
			AssertEquals("20", line.JI_SterilizationValue);
		}

		public void TestStorageAndShippingConditionCollection_ChildEditable()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(true, jobComInvoiceLine.IsRegisteredEditableChildObject(jobComInvoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection));
		}

		public void TestDefaultProcedure()
		{
			var hsnTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = UniversalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			var rateCodeDTS = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", rateType.PK);
			var preference = UniversalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = UniversalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = UniversalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = UniversalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preference.PK, "0", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_TariffAdditionalCode = "X";
			invoiceLine.JI_Tariff = "21039090200";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals("31", invoiceLine.JI_Procedure);

			invoiceLine.JI_Procedure = ZString.Empty;
			invoiceLine.JI_Tariff = "21039090201";
			AssertEquals("51", invoiceLine.JI_Procedure);

			invoiceLine.JI_TariffAdditionalCode = ZString.Empty;
			invoiceLine.JI_Procedure = ZString.Empty;
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_Tariff = "21039090201";
			AssertEquals("50", invoiceLine.JI_Procedure);

			invoiceLine.JI_Procedure = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = "STD";
			AssertEquals("50", invoiceLine.JI_Procedure);

			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "TW";
			AssertEquals("50", invoiceLine.JI_Procedure);

			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_Procedure = ZString.Empty;
			invoiceLine.JI_Tariff = "21039090200";
			AssertEquals("31", invoiceLine.JI_Procedure);

			invoiceLine.JI_Tariff = "21039090201";
			AssertEquals("31", invoiceLine.JI_Procedure);
		}

		public void TestDefaultingInvoiceLineTaxWhenAlwaysCalculateAdditionalTaxIsFalse()
		{
			using (TWCustomsDataRegistry.Instance.AlwaysCalculateAdditionalTax.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				TariffDataForTestHelper.GenerateAdditionalTaxTariff(Factory);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				InvoiceLine.JI_Tariff = "90031";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("CT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("OtherBeverage", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90032";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("AT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Distilled", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90033";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("TT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Gasoline", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90034";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("SS", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Forniture", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90035";
				AssertEquals(false, InvoiceLine.Taxes.Any());

				InvoiceLine.JI_Tariff = "90036";
				AssertEquals(false, InvoiceLine.Taxes.Any());
			}
		}

		public void TestDefaultingInvoiceLineTaxWhenAlwaysCalculateAdditionalTaxIsTrue()
		{
			using (TWCustomsDataRegistry.Instance.AlwaysCalculateAdditionalTax.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TariffDataForTestHelper.GenerateAdditionalTaxTariff(Factory);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				InvoiceLine.JI_Tariff = "90031";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("CT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("OtherBeverage", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90032";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("AT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Distilled", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90033";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("TT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Gasoline", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90034";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("SS", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Forniture", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90035";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("CT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("PassengerCar", InvoiceLine.Taxes[0].JLT_Tariff);
				});

				InvoiceLine.JI_Tariff = "90036";
				CombineAssertions(() =>
				{
					AssertEquals(1, InvoiceLine.Taxes.Count);
					AssertEquals("AT", InvoiceLine.Taxes[0].JLT_Type);
					AssertEquals("Ethyl", InvoiceLine.Taxes[0].JLT_Tariff);
				});
			}
		}

		public void TestJI_TariffChangeResettingCustomQuantities()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffUOM(tariff1, "CU1", "KG");
			helper.CreateTariffUOM(tariff1, "CU2", "L");

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffUOM(tariff2, "CU1", "BAG");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "90031";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "PKG";
			AssertEquals("PKG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(10m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_Tariff = "900301";
			CombineAssertions("Only valid tariffs trigger defaulting: ", () =>
			{
				AssertEquals(false, invoiceLine.JI_CustomsUnitQty.IsEmpty);
				AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
			});

			invoiceLine.JI_Tariff = "2713200000";
			CombineAssertions("Valid tariffs trigger defaulting: ", () =>
			{
				AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("L", invoiceLine.JI_CustomsSecondUnitQty);
			});

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_Tariff = "2713200001";
			CombineAssertions("Tariffs with only one UQ will empty the second UQ: ", () =>
			{
				AssertEquals("BAG", invoiceLine.JI_CustomsUnitQty);
				AssertEquals(10m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals(100m, invoiceLine.JI_CustomsSecondQuantity);
			});
		}

		public void TestFoodDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();

			ICusCodeDataTypeSupporter supporter = jobComInvoiceLine;
			supporter.AssertType(typeof(FoodData), CusCodeDataTypeList.Codes.Food);
			supporter.AssertType(null, "XXX");

			var foodData = jobComInvoiceLine.FoodDataCollection.AddNew();
			foodData.CY_Data = "XXX";
			foodData.Content = 1111111111;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(foodData.PK);
			AssertEquals(typeof(FoodData), codeData.GetType());
		}

		public void TestDeleteRowsWithEmptyAssignedNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var jobComInvoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var assignedNumber1 = jobComInvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber1.JG_ReferenceNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(0, jobComInvoiceLine.AssignedJobComInvLineRefsCollection.Count);
			var assignedNumber2 = jobComInvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber2.JG_ReferenceNumber = "123456";
			Factory.Save();
			AssertEquals(1, jobComInvoiceLine.AssignedJobComInvLineRefsCollection.Count);
		}

		public void TestDeleteRowsWithEmptyPermitNumberAndLineNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var jobComInvoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			jobComInvoiceLine.PermitCusSupportingCollection.AddNew();
			Factory.Save();
			AssertEquals(0, jobComInvoiceLine.PermitCusSupportingCollection.Count);
			var permitNumber2 = jobComInvoiceLine.PermitCusSupportingCollection.AddNew();
			permitNumber2.CSI_ReferenceNumber = ZString.Empty;
			permitNumber2.CSI_LineNo = 123;
			Factory.Save();
			AssertEquals(1, jobComInvoiceLine.PermitCusSupportingCollection.Count);
			var permitNumber3 = jobComInvoiceLine.PermitCusSupportingCollection.AddNew();
			permitNumber3.CSI_ReferenceNumber = "123456";
			permitNumber3.CSI_LineNo = ZShort.Zero;
			Factory.Save();
			AssertEquals(2, jobComInvoiceLine.PermitCusSupportingCollection.Count);
			var permitNumber4 = jobComInvoiceLine.PermitCusSupportingCollection.AddNew();
			permitNumber4.CSI_ReferenceNumber = "123456";
			permitNumber4.CSI_LineNo = 123;
			Factory.Save();
			AssertEquals(3, jobComInvoiceLine.PermitCusSupportingCollection.Count);

			var permitNumber5 = jobComInvoiceLine.PermitCusSupportingCollection.AddNew();
			permitNumber5.CSI_ReferenceNumber = ZString.Empty;
			permitNumber5.CSI_LineNo = ZShort.Zero;
			Factory.Save();
			AssertEquals(3, jobComInvoiceLine.PermitCusSupportingCollection.Count);
		}

		[TestDate(2019, 5, 14)]
		public void TestDateOfValuation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MWB123456";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = declaration.InvoiceLines.AddNew();
			invLine.JI_JZ = invHeader.PK;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			var entryLine1 = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine1.CL_CH = entry.PK;

			var entryInstruction1 = declaration.CusEntryInstruction;

			var today = TestDateAttribute.Date;
			var dateForDuty = new ZDateTime(2019, 05, 15);

			invLine.JI_CL = entryLine1.PK;
			invLine.JI_CEI = entryInstruction1.PK;

			entryInstruction1.CEI_DateForDuty = today;
			declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			Factory.Save();
			AssertEquals(today, invLine.DateOfValuation);

			entryInstruction1.CEI_DateForDuty = dateForDuty;
			declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			Factory.Save();
			AssertEquals(dateForDuty, invLine.DateOfValuation);

			entryInstruction1.CEI_DateForDuty = ZDateTime.Invalid;
			declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			Factory.Save();
			AssertEquals(today, invLine.DateOfValuation);

			invLine.JI_CEI = ZGuid.Empty;
			declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			Factory.Save();
			AssertEquals(today, invLine.DateOfValuation);
		}

		public void TestUpdateDetailsFromProductOnPartChangeCore()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BAG";
			part.OP_PartNum = "NEWPROD1";
			part.OP_Desc = "PRODUCT1";
			part.OP_Brand = "Apple";
			part.OP_Model = "Phone";
			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var header = declaration.Invoices.AddNew();
			var invoiceline = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			declaration.JE_OH_Supplier = supplier.PK;

			Assert(invoiceline.JI_BrandName.IsEmpty);
			Assert(invoiceline.JI_Model.IsEmpty);

			invoiceline.JI_PartNo = part.OP_PartNum;

			AssertEquals("JI_BrandName", "Apple", invoiceline.JI_BrandName);
			AssertEquals("JI_Model", "Phone", invoiceline.JI_Model);
		}

		#region UpdateDetailsFromProductOnPartChangeCore
		public void TestUpdateDetailsFromPivotOnPartChangeCoreByCommercialInvoice()
		{
			var testHelper = new TestTWCreator(Factory);
			var supplier = testHelper.CreateOrganizationForSupplier();
			testHelper.CreateOrgSupplierPart(supplier);

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = "IMP";
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertUpdateDetailsFromPivotOnPartChangeWhenPartNoisEmpty(invoice);

			invoice.JZ_MessageType = "IMP";
			AssertUpdateDetailsFromPivotOnPartChangeWhenMessageTypeisImport(invoice);

			invoice.JZ_MessageType = "EXP";
			AssertUpdateDetailsFromPivotOnPartChangeWhenMessageTypeisExport(invoice);
		}

		public void TestUpdateDetailsFromPivotOnPartChangeCore()
		{
			var testHelper = new TestTWCreator(Factory);
			var supplier = testHelper.CreateOrganizationForSupplier();
			testHelper.CreateOrgSupplierPart(supplier);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var header = declaration.Invoices.AddNew();
			declaration.JE_OH_Supplier = supplier.PK;
			AssertUpdateDetailsFromPivotOnPartChangeWhenPartNoisEmpty(header);

			declaration.JE_MessageType = "IMP";
			AssertUpdateDetailsFromPivotOnPartChangeWhenMessageTypeisImport(header);

			declaration.JE_MessageType = "EXP";
			AssertUpdateDetailsFromPivotOnPartChangeWhenMessageTypeisExport(header);
		}

		public void TestIsModeOfStatisticsRequirePermitNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();
			line.JI_Procedure = ProcedureCodes._01;
			Assert("EXP, 01", line.IsModeOfStatisticsRequirePermitNumber);

			line.JI_Procedure = ProcedureCodes._1A;
			Assert("EXP, 1A", line.IsModeOfStatisticsRequirePermitNumber);

			line.JI_Procedure = ProcedureCodes._8A;
			Assert("EXP, 8A", line.IsModeOfStatisticsRequirePermitNumber);

			line.JI_Procedure = ProcedureCodes._8D;
			Assert("EXP, 8D", line.IsModeOfStatisticsRequirePermitNumber);

			line.JI_Procedure = ProcedureCodes._02;
			Assert("EXP, 02", !line.IsModeOfStatisticsRequirePermitNumber);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			line.JI_Procedure = ProcedureCodes._01;
			Assert("IMP, 01", !line.IsModeOfStatisticsRequirePermitNumber);
		}

		public void TestSetDeclarationGoodsDescriptionOnPartChanged()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BAG";
			part.OP_PartNum = "NEWPROD1";
			part.OP_Desc = "PRODUCT1";
			part.OP_Brand = "Apple";
			part.OP_Model = "Phone";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_NDescription = "CI_NDescription: 中文";
			pivot.CI_Description = "CI_Description: test";

			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Factory.Save();

			CombineAssertions("JI_DeclGoodsDescMode is BTH", () =>
			{
				pivot.CI_DeclGoodsDescMode = DeclarationGoodsDescriptionModeList.Codes.BTH;
				var declaration = Factory.New<JobDeclaration>();
				var header = declaration.Invoices.AddNew();
				var invoiceline = header.InvoiceLines.AddNew() as JobComInvoiceLine;
				declaration.JE_OH_Supplier = supplier.PK;
				invoiceline.JI_PartNo = part.OP_PartNum;
				AssertEquals("JI_Description from CI", "CI_Description: test", invoiceline.JI_Description);
				AssertEquals("JI_NDescription", "CI_NDescription: 中文", invoiceline.JI_NDescription);

				pivot.CI_Description = ZString.Empty;
				var invoiceline2 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceline2.JI_PartNo = part.OP_PartNum;
				AssertEquals("JI_Description from OP", "PRODUCT1", invoiceline2.JI_Description);
				AssertEquals("JI_NDescription", "CI_NDescription: 中文", invoiceline2.JI_NDescription);
			});

			CombineAssertions("JI_DeclGoodsDescMode is CHT", () =>
			{
				pivot.CI_Description = "CI_Description: test";
				pivot.CI_DeclGoodsDescMode = DeclarationGoodsDescriptionModeList.Codes.CHT;
				var declaration = Factory.New<JobDeclaration>();
				var header = declaration.Invoices.AddNew();
				var invoiceline = header.InvoiceLines.AddNew() as JobComInvoiceLine;
				declaration.JE_OH_Supplier = supplier.PK;
				Assert(invoiceline.JI_NDescription.IsEmpty);
				Assert(invoiceline.JI_Description.IsEmpty);

				invoiceline.JI_PartNo = part.OP_PartNum;
				Assert(invoiceline.JI_Description.IsEmpty);
				AssertEquals("JI_NDescription", "CI_NDescription: 中文", invoiceline.JI_NDescription);
			});

			CombineAssertions("JI_DeclGoodsDescMode is ENG", () =>
			{
				pivot.CI_DeclGoodsDescMode = DeclarationGoodsDescriptionModeList.Codes.ENG;
				var declaration = Factory.New<JobDeclaration>();
				var header = declaration.Invoices.AddNew();
				var invoiceline = header.InvoiceLines.AddNew() as JobComInvoiceLine;
				declaration.JE_OH_Supplier = supplier.PK;
				Assert(invoiceline.JI_NDescription.IsEmpty);
				Assert(invoiceline.JI_Description.IsEmpty);

				invoiceline.JI_PartNo = part.OP_PartNum;
				Assert(invoiceline.JI_NDescription.IsEmpty);
				AssertEquals("JI_Description from CI", "CI_Description: test", invoiceline.JI_Description);

				pivot.CI_Description = ZString.Empty;
				var invoiceline2 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceline2.JI_PartNo = part.OP_PartNum;
				Assert(invoiceline2.JI_NDescription.IsEmpty);
				AssertEquals("JI_Description from OP", "PRODUCT1", invoiceline2.JI_Description);
			});
		}

		void AssertUpdateDetailsFromPivotOnPartChangeWhenPartNoisEmpty(JobComInvoiceHeader invoice)
		{
			var invoiceline = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			Assert(invoiceline.JI_CountryOfOrigin.IsEmpty);
			Assert(invoiceline.JI_Procedure.IsEmpty);
			Assert(invoiceline.JI_EnteredUnitPrice.IsEmpty);
			Assert(invoiceline.JI_TariffAdditionalCode.IsEmpty);
			Assert(invoiceline.JI_Compositions.IsEmpty);
			Assert(invoiceline.JI_CustomsOwnerPartNo.IsEmpty);
			Assert(invoiceline.JI_CustomsSupplierPartNo.IsEmpty);
			AssertEquals("PermitCusSupportingCollection Count", 0, invoiceline.PermitCusSupportingCollection.Count);
			AssertEquals("AssignedJobComInvLineRefsCollection Count", 0, invoiceline.AssignedJobComInvLineRefsCollection.Count);
		}

		void AssertUpdateDetailsFromPivotOnPartChangeWhenMessageTypeisImport(JobComInvoiceHeader invoice)
		{
			Assert(invoice.IsImport);
			var invoiceline = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline.JI_CountryOfOrigin = "CN";
			invoiceline.JI_PartNo = "NEWPROD1";
			AssertEquals("JI_CountryOfOrigin", "TW", invoiceline.JI_CountryOfOrigin);
			AssertEquals("JI_NDescription", "CI_NDescription: 中文", invoiceline.JI_NDescription);
			AssertEquals("Import JI_Procedure", "31", invoiceline.JI_Procedure);
			AssertEquals("UnitPrice", 100m, invoiceline.JI_EnteredUnitPrice);
			AssertEquals("JI_TariffAdditionalCode", "test", invoiceline.JI_TariffAdditionalCode);
			AssertEquals("JI_Compositions", "規格：BOX 100*50mm and made from paper", invoiceline.JI_Compositions);
			AssertEquals("CSI_ReferenceNumber", "12345678901234", invoiceline.PermitCusSupportingCollection[0].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(123), invoiceline.PermitCusSupportingCollection[0].CSI_LineNo);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber123", invoiceline.AssignedJobComInvLineRefsCollection[0].JG_ReferenceNumber);
			AssertEquals("JI_CustomsOwnerPartNo", "CI_CustomsOwnerPartNo test", invoiceline.JI_CustomsOwnerPartNo);
			AssertEquals("JI_CustomsSupplierPartNo", "CI_CustomsSupplierPartNo test", invoiceline.JI_CustomsSupplierPartNo);
			AssertEquals("JI_AlcoholPercentage", 30m, invoiceline.JI_AlcoholPercentage);
		}

		void AssertUpdateDetailsFromPivotOnPartChangeWhenMessageTypeisExport(JobComInvoiceHeader invoice)
		{
			Assert(invoice.IsExport);
			invoice.JZ_RX_NKInvoice_Currency = "TWD";
			var invoiceline = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline.JI_EnteredUnitPrice = ZDecimal.Zero;
			invoiceline.JI_PartNo = "NEWPROD1";
			AssertEquals("Export JI_Procedure", "01", invoiceline.JI_Procedure);
			AssertEquals("UnitPrice", 100m, invoiceline.JI_EnteredUnitPrice);
			AssertEquals("JI_AlcoholPercentage", 0m, invoiceline.JI_AlcoholPercentage);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoiceline = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline.JI_EnteredUnitPrice = ZDecimal.Zero;
			invoiceline.JI_PartNo = "NEWPROD1";
			AssertEquals("UnitPrice", ZDecimal.Zero, invoiceline.JI_EnteredUnitPrice);
		}
		#endregion

		public void TestUpdateAssignedJobComInvLineRefsCollection()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var pivot = part.PivotsForBinding.AddNew();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var header = declaration.Invoices.AddNew();
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceline = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline.JI_PartNo = part.OP_PartNum;

			AssertEquals("AssignedJobComInvLineRefsCollection Count", 0, invoiceline.AssignedJobComInvLineRefsCollection.Count);

			var productAssigned = pivot.AssignedCusClassPartPivotRefCollection.AddNew();
			productAssigned.CIR_ReferenceNumber = "ReferenceNumber1";
			Factory.Save();
			var invoiceline2 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline2.JI_PartNo = part.OP_PartNum;
			AssertEquals("AssignedJobComInvLineRefsCollection Count", 1, invoiceline2.AssignedJobComInvLineRefsCollection.Count);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber1", invoiceline2.AssignedJobComInvLineRefsCollection[0].JG_ReferenceNumber);

			var productAssigned2 = pivot.AssignedCusClassPartPivotRefCollection.AddNew();
			productAssigned2.CIR_ReferenceNumber = "ReferenceNumber2";
			Factory.Save();
			var invoiceline3 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline3.JI_PartNo = part.OP_PartNum;
			AssertEquals("AssignedJobComInvLineRefsCollection Count", 2, invoiceline3.AssignedJobComInvLineRefsCollection.Count);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber1", invoiceline3.AssignedJobComInvLineRefsCollection[0].JG_ReferenceNumber);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber2", invoiceline3.AssignedJobComInvLineRefsCollection[1].JG_ReferenceNumber);

			var invoiceline4 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLineReference = invoiceline4.AssignedJobComInvLineRefsCollection.AddNew();
			invoiceLineReference.JG_ReferenceNumber = "ReferenceNumber2";
			Factory.Save();
			AssertEquals("AssignedJobComInvLineRefsCollection Count", 1, invoiceline4.AssignedJobComInvLineRefsCollection.Count);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber2", invoiceline4.AssignedJobComInvLineRefsCollection[0].JG_ReferenceNumber);
			invoiceline4.JI_PartNo = part.OP_PartNum;
			AssertEquals("AssignedJobComInvLineRefsCollection Count", 2, invoiceline4.AssignedJobComInvLineRefsCollection.Count);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber1", invoiceline4.AssignedJobComInvLineRefsCollection[1].JG_ReferenceNumber);
			AssertEquals("JG_ReferenceNumber", "ReferenceNumber2", invoiceline4.AssignedJobComInvLineRefsCollection[0].JG_ReferenceNumber);
		}

		public void TestUpdatPermitCusSupportingCollection()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var pivot = part.PivotsForBinding.AddNew();
			var productPermit = pivot.ProductPermitCusSupportingCollection.AddNew();
			productPermit.CSI_LineNo = 1;
			productPermit.CSI_ReferenceNumber = "";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var header = declaration.Invoices.AddNew();
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceline = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline.JI_PartNo = part.OP_PartNum;

			AssertEquals("PermitCusSupportingCollection Count", 0, invoiceline.PermitCusSupportingCollection.Count);

			productPermit.CSI_ReferenceNumber = "Reference1";
			Factory.Save();
			var invoiceline2 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline2.JI_PartNo = part.OP_PartNum;
			AssertEquals("PermitCusSupportingCollection Count", 1, invoiceline2.PermitCusSupportingCollection.Count);
			AssertEquals("CSI_ReferenceNumber", "Reference1", invoiceline2.PermitCusSupportingCollection[0].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(1), invoiceline2.PermitCusSupportingCollection[0].CSI_LineNo);

			var productPermit2 = pivot.ProductPermitCusSupportingCollection.AddNew();
			productPermit2.CSI_LineNo = 2;
			productPermit2.CSI_ReferenceNumber = "Reference2";
			Factory.Save();
			var invoiceline3 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceline3.JI_PartNo = part.OP_PartNum;
			AssertEquals("PermitCusSupportingCollection Count", 2, invoiceline3.PermitCusSupportingCollection.Count);
			AssertEquals("CSI_ReferenceNumber", "Reference1", invoiceline3.PermitCusSupportingCollection[0].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(1), invoiceline3.PermitCusSupportingCollection[0].CSI_LineNo);
			AssertEquals("CSI_ReferenceNumber", "Reference2", invoiceline3.PermitCusSupportingCollection[1].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(2), invoiceline3.PermitCusSupportingCollection[1].CSI_LineNo);

			var invoiceline4 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLineReference = invoiceline4.PermitCusSupportingCollection.AddNew();
			invoiceLineReference.CSI_LineNo = 2;
			invoiceLineReference.CSI_ReferenceNumber = "Reference2";
			Factory.Save();
			AssertEquals("PermitCusSupportingCollection Count", 1, invoiceline4.PermitCusSupportingCollection.Count);
			AssertEquals("CSI_ReferenceNumber", "Reference2", invoiceline4.PermitCusSupportingCollection[0].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(2), invoiceline4.PermitCusSupportingCollection[0].CSI_LineNo);
			invoiceline4.JI_PartNo = part.OP_PartNum;
			AssertEquals("PermitCusSupportingCollection Count", 2, invoiceline4.PermitCusSupportingCollection.Count);
			AssertEquals("CSI_ReferenceNumber", "Reference1", invoiceline4.PermitCusSupportingCollection[1].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(1), invoiceline4.PermitCusSupportingCollection[1].CSI_LineNo);
			AssertEquals("CSI_ReferenceNumber", "Reference2", invoiceline4.PermitCusSupportingCollection[0].CSI_ReferenceNumber);
			AssertEquals("CSI_LineNo", new ZShort(2), invoiceline4.PermitCusSupportingCollection[0].CSI_LineNo);
		}

		public void TestSetTariffEtcDataFromProductsPivotCore()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BAG";
			part.OP_PartNum = "NEWPROD1";
			part.OP_Desc = "PRODUCT1";
			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_OH = part.RelatedOrganisations.OfType<OrgPartRelation>().FirstOrDefault()?.OU_OH ?? ZGuid.Empty;
			pivot.CI_TariffNum = "8610101011";
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			pivot.CI_PrimaryPreference = "EUTRADE";
			pivot.CI_CustomsOwnerPartNo = "AAA123";
			pivot.CI_CustomsSupplierPartNo = "BBB456";
			pivot.CI_PartPivotUOM = "PKG";
			pivot.CI_CarType = "A";
			pivot.CI_Transmission = "B";
			pivot.CI_EngineType = "C";
			pivot.CI_LHD = "D";
			pivot.CI_HasCatalystConverter = "E";
			pivot.CI_EquipmentPrintMode = "F";
			pivot.CI_CarCondition = "G";
			pivot.CI_ModelYear = 99;
			pivot.CI_Displacement = "98";
			pivot.CI_NumberOfDoor = 9;
			pivot.CI_Seats = 96;
			pivot.CI_Cylinders = 95;
			pivot.CI_Gears = 94;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line1 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			declaration.JE_OH_Supplier = supplier.PK;

			Assert(line1.JI_CustomsOwnerPartNo.IsEmpty);
			Assert(line1.JI_CustomsSupplierPartNo.IsEmpty);
			Assert(line1.JI_InvoiceUQ.IsEmpty);

			line1.JI_PartNo = part.OP_PartNum;

			AssertEquals("AAA123", line1.JI_CustomsOwnerPartNo);
			AssertEquals("BBB456", line1.JI_CustomsSupplierPartNo);
			AssertEquals("PKG", line1.JI_InvoiceUQ);
			CombineAssertions("Set values from pivot", () =>
			{
				AssertEquals(true, line1.IsImport);
				AssertEquals(true, line1.IsCarRelatedTariff);
				AssertEquals("A", line1.JI_CarType);
				AssertEquals("B", line1.JI_Transmission);
				AssertEquals("C", line1.JI_EngineType);
				AssertEquals("D", line1.JI_LHD);
				AssertEquals("E", line1.JI_HasCatalystConverter);
				AssertEquals("F", line1.JI_EquipmentPrintMode);
				AssertEquals("G", line1.JI_CarCondition);
				AssertEquals(new ZShort(99), line1.JI_ModelYear);
				AssertEquals("98", line1.JI_Displacement);
				AssertEquals(new ZShort(9), line1.JI_NumberOfDoor);
				AssertEquals(new ZShort(96), line1.JI_Seats);
				AssertEquals(new ZShort(95), line1.JI_Cylinders);
				AssertEquals(new ZShort(94), line1.JI_Gears);
			});

			line1.JI_PartNo = ZString.Empty;
			pivot.CI_CarType = ZString.Empty;
			pivot.CI_Transmission = ZString.Empty;
			pivot.CI_EngineType = ZString.Empty;
			pivot.CI_LHD = ZString.Empty;
			pivot.CI_HasCatalystConverter = ZString.Empty;
			pivot.CI_EquipmentPrintMode = ZString.Empty;
			pivot.CI_CarCondition = ZString.Empty;
			pivot.CI_ModelYear = ZShort.Zero;
			pivot.CI_Displacement = ZString.Empty;
			pivot.CI_NumberOfDoor = ZShort.Zero;
			pivot.CI_Seats = ZShort.Zero;
			pivot.CI_Cylinders = ZShort.Zero;
			pivot.CI_Gears = ZShort.Zero;
			line1.JI_PartNo = part.OP_PartNum;
			CombineAssertions("Keep itself values", () =>
			{
				AssertEquals(true, line1.IsImport);
				AssertEquals(true, line1.IsCarRelatedTariff);
				AssertEquals("A", line1.JI_CarType);
				AssertEquals("B", line1.JI_Transmission);
				AssertEquals("C", line1.JI_EngineType);
				AssertEquals("D", line1.JI_LHD);
				AssertEquals("E", line1.JI_HasCatalystConverter);
				AssertEquals("F", line1.JI_EquipmentPrintMode);
				AssertEquals("G", line1.JI_CarCondition);
				AssertEquals(new ZShort(99), line1.JI_ModelYear);
				AssertEquals("98", line1.JI_Displacement);
				AssertEquals(new ZShort(9), line1.JI_NumberOfDoor);
				AssertEquals(new ZShort(96), line1.JI_Seats);
				AssertEquals(new ZShort(95), line1.JI_Cylinders);
				AssertEquals(new ZShort(94), line1.JI_Gears);
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			line1 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.JI_PartNo = part.OP_PartNum;
			CombineAssertions("No set values from pivot when non IsImport or non IsCarRelatedTariff", () =>
			{
				AssertEquals(false, line1.IsImport);
				AssertEquals(true, line1.IsCarRelatedTariff);
				AssertEquals(ZString.Empty, line1.JI_CarType);
				AssertEquals(ZString.Empty, line1.JI_Transmission);
				AssertEquals(ZString.Empty, line1.JI_EngineType);
				AssertEquals(ZString.Empty, line1.JI_LHD);
				AssertEquals(ZString.Empty, line1.JI_HasCatalystConverter);
				AssertEquals(ZString.Empty, line1.JI_EquipmentPrintMode);
				AssertEquals(ZString.Empty, line1.JI_CarCondition);
				AssertEquals(ZShort.Zero, line1.JI_ModelYear);
				AssertEquals(ZString.Empty, line1.JI_Displacement);
				AssertEquals(ZShort.Zero, line1.JI_NumberOfDoor);
				AssertEquals(ZShort.Zero, line1.JI_Seats);
				AssertEquals(ZShort.Zero, line1.JI_Cylinders);
				AssertEquals(ZShort.Zero, line1.JI_Gears);
			});
		}

		public void TestReadOnlyFields()
		{
			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = "IMP";
			var invoice1 = declaration4.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var entryInstruction = declaration4.CusEntryInstruction;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			entryInstruction.CEI_Style = "F1";
			Assert(!invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_Style = "F2";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_Style = "G2";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);

			entryInstruction.CEI_Style = ZString.Empty;
			entryInstruction.CEI_ReasonForDuty = "02";
			Assert(!invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_ReasonForDuty = "01";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_ReasonForDuty = "08";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);

			entryInstruction.CEI_ReasonForDuty = ZString.Empty;
			entryInstruction.CEI_CustomsOffice = "CA";
			Assert(!invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_CustomsOffice = "CB";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_CustomsOffice = "CS";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_CustomsOffice = "DB";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);
			entryInstruction.CEI_CustomsOffice = "BB";
			Assert(invoiceLine1.JI_CusValueConvRatio_ReadOnly);
		}

		public void TestIsForCAHeader()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCAHeader20);
			AssertEquals(false, line.IsForCAHeaderCI);
			AssertEquals(false, line.IsForCAHeader2Q);
			AssertEquals(false, line.IsForCAHeaderCD);
			AssertEquals(false, line.IsForCAHeaderIF);
			AssertEquals(false, line.IsForCAHeaderDH);
			AssertEquals(false, line.IsForCAHeaderDN);

			foreach (var code in new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" })
			{
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingAgency = code;
			}

			AssertEquals(false, line.IsForCAHeader20);
			AssertEquals(false, line.IsForCAHeaderCI);
			AssertEquals(false, line.IsForCAHeader2Q);
			AssertEquals(false, line.IsForCAHeaderCD);
			AssertEquals(false, line.IsForCAHeaderIF);
			AssertEquals(false, line.IsForCAHeaderDH);
			AssertEquals(false, line.IsForCAHeaderDN);

			foreach (InvoiceLineLinkControllingMsgHeader invoiceLineLinkControllingMsgHeader in line.InvoiceLineLinkControllingMsgHeaders)
			{
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
			}
			AssertEquals(true, line.IsForCAHeader20);
			AssertEquals(true, line.IsForCAHeaderCI);
			AssertEquals(true, line.IsForCAHeader2Q);
			AssertEquals(true, line.IsForCAHeaderCD);
			AssertEquals(true, line.IsForCAHeaderIF);
			AssertEquals(true, line.IsForCAHeaderDH);
			AssertEquals(true, line.IsForCAHeaderDN);
		}

		public void TestIsForCMHeaderMessageTypeNX101()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCMHeaderMessageTypeNX101);

			var messageTypesAndExpectResults = new (string, bool)[]
			{
				("NX301", false),
				("NX301_DN", false),
				("NX401", false),
				("NX601", false),
				("NX603", false),
				("X101", false),
				("NX101", true)
			};

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(false, line.IsForCMHeaderMessageTypeNX101);
			}

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item2, line.IsForCMHeaderMessageTypeNX101);
			}
		}

		public void TestIsForCMHeaderMessageTypeNX101CertificateType15()
		{
			var declartion = Factory.New<JobDeclaration>();
			var instruction = declartion.CusEntryInstruction;
			var controllingMessageHeader = instruction.ControllingMessageHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)declartion.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var invoiceLineLinkControllingMsgHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];

			AssertEquals(false, invoiceLine.IsForCMHeaderMessageTypeNX101CertificateType15);

			var testDatacollection = new ((bool IsLinkedCMHeader, string MessageType, string CertificateType) Settings, bool ExpectedResult)[]
			{
				((false, ControllingMessageTypeList.Codes.NX101, CertificateTypeList.Codes.Code15), false),
				((true, ControllingMessageTypeList.Codes.NX301, CertificateTypeList.Codes.Code15), false),
				((true, ControllingMessageTypeList.Codes.NX101, CertificateTypeList.Codes.Code10), false),
				((true, ControllingMessageTypeList.Codes.NX101, CertificateTypeList.Codes.Code15), true),
			};

			foreach (var testDataItem in testDatacollection)
			{
				var settings = testDataItem.Settings;
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = settings.IsLinkedCMHeader;
				controllingMessageHeader.TW1_ControllingMessageType = settings.MessageType;
				controllingMessageHeader.TW1_CertificateType = settings.CertificateType;
				AssertEquals(testDataItem.ExpectedResult, invoiceLine.IsForCMHeaderMessageTypeNX101CertificateType15);
			}
		}

		public void TestIsForCAHeaderMessageTypeNX301()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCMHeaderMessageTypeNX301);

			var messageTypesAndExpectResults = new (string, bool)[]
			{
				("NX301", true),
				("NX301_DN", false),
				("NX401", false),
				("NX601", false),
				("NX603", false),
				("X101", false)
			};

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(false, line.IsForCMHeaderMessageTypeNX301);
			}

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item2, line.IsForCMHeaderMessageTypeNX301);
			}
		}

		public void TestIsForCAHeaderMessageTypeNX301_DN()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCMHeaderMessageTypeNX301_DN);

			var messageTypesAndExpectResults = new (string, bool)[]
			{
				("NX301", false),
				("NX301_DN", true),
				("NX401", false),
				("NX601", false),
				("NX603", false),
				("X101", false)
			};

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(false, line.IsForCMHeaderMessageTypeNX301_DN);
			}

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item2, line.IsForCMHeaderMessageTypeNX301_DN);
			}
		}

		public void TestIsForCMHeaderMessageTypeNX401()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCMHeaderMessageTypeNX401);

			var messageTypesAndExpectResults = new (string, bool)[]
			{
				("NX301", false),
				("NX301_DN", false),
				("NX401", true),
				("NX601", false),
				("NX603", false),
				("X101", false)
			};

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item1, false, line.IsForCMHeaderMessageTypeNX401);
			}

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item1, typeAndResult.Item2, line.IsForCMHeaderMessageTypeNX401);
			}
		}

		public void TestIsForCAHeaderMessageTypeNX601()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCMHeaderMessageTypeNX601);

			var messageTypesAndExpectResults = new (string, bool)[]
			{
				("NX301", false),
				("NX301_DN", false),
				("NX401", false),
				("NX601", true),
				("NX603", false),
				("X101", false)
			};

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(false, line.IsForCMHeaderMessageTypeNX601);
			}

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item2, line.IsForCMHeaderMessageTypeNX601);
			}
		}

		public void TestIsForCAHeaderMessageTypeNX603()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line.IsForCMHeaderMessageTypeNX603);

			var messageTypesAndExpectResults = new (string, bool)[]
			{
				("NX301", false),
				("NX301_DN", false),
				("NX401", false),
				("NX601", false),
				("NX603", true),
				("X101", false)
			};

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(false, line.IsForCMHeaderMessageTypeNX603);
			}

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			foreach (var typeAndResult in messageTypesAndExpectResults)
			{
				controllingMessageHeader.TW1_ControllingMessageType = typeAndResult.Item1;
				AssertEquals(typeAndResult.Item2, line.IsForCMHeaderMessageTypeNX603);
			}
		}

		public void TestShouldDisplayLabelGroupBox()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			foreach (var code in new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" })
			{
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingAgency = code;
			}
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "20").IsLinkedCMHeader = true;
			AssertEquals(true, invoiceLine.IsForCAHeader20);
			Assert(invoiceLine.ShouldDisplayLabelGroupBox);

			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "20").IsLinkedCMHeader = false;
			AssertEquals(false, invoiceLine.IsForCAHeader20);
			Assert(!invoiceLine.ShouldDisplayLabelGroupBox);

			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "CI").IsLinkedCMHeader = true;
			AssertEquals(true, invoiceLine.IsForCAHeaderCI);
			Assert(invoiceLine.ShouldDisplayLabelGroupBox);

			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "CI").IsLinkedCMHeader = false;
			AssertEquals(false, invoiceLine.IsForCAHeaderCI);
			Assert(!invoiceLine.ShouldDisplayLabelGroupBox);

			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "2Q").IsLinkedCMHeader = true;
			AssertEquals(true, invoiceLine.IsForCAHeader2Q);
			Assert(invoiceLine.ShouldDisplayLabelGroupBox);

			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == "2Q").IsLinkedCMHeader = false;
			AssertEquals(false, invoiceLine.IsForCAHeader2Q);
			Assert(!invoiceLine.ShouldDisplayLabelGroupBox);
		}

		public void TestInvoiceLinePackageValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 100000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var collection = new BaseCusLinkPackageCollection(invoiceLine);
			var npbo = collection.AddNew();
			AssertType<InvoiceLinePackageValidation>(supporter.GetNewLinkPackValidation(npbo));
		}

		public void TestJI_CustomsUnitQty_ReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			Assert(invoiceLine.JI_CustomsUnitQty_ReadOnly);
		}

		public void TestJI_CustomsQuantity_ReadOnly()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			Assert(invoiceLine.JI_CustomsQuantity_ReadOnly);
		}

		public void TestJI_CustomsSecondUnitANdQuantity_ReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			Factory.Save();

			var tariff0000000021 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff0000000021.PK, "CU1", "A");

			var tariff1000000021 = helper.CreateTariff("TW", hsnTariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1000000021.PK, "CU2", "MTO");
			Factory.Save();

			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var customsSecondUnitQtyInfo = invoiceLine.JI_CustomsSecondUnitQtyInfo;
			var customsSecondQuantityInfo = invoiceLine.JI_CustomsSecondQuantityInfo;
			CombineAssertions(() =>
			{
				AssertEquals("When tariff is empty JI_CustomsSecondUnitQty is not read only", false, customsSecondUnitQtyInfo.ReadOnly);
				AssertEquals("When tariff is empty JI_CustomsSecondQuantity is not read only", false, customsSecondQuantityInfo.ReadOnly);

				invoiceLine.JI_Tariff = "0000000021";
				AssertEquals("JI_CustomsSecondUnitQty is not read only when tariff does not have CU2", false, customsSecondUnitQtyInfo.ReadOnly);
				AssertEquals("JI_CustomsSecondQuantity is not read only when tariff does not have CU2", false, customsSecondQuantityInfo.ReadOnly);

				invoiceLine.JI_Tariff = "1000000021";
				AssertEquals("JI_CustomsSecondUnitQty is read only when tariff have CU2", true, customsSecondUnitQtyInfo.ReadOnly);
				AssertEquals("JI_CustomsSecondQuantity is not read only when tariff have CU2", false, customsSecondQuantityInfo.ReadOnly);
			});
		}

		public void TestChassisNumbers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.ResetTotalsAndCachedValues();

			var invoice0 = declaration.Invoices.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInstruction.PK;
			line0.JI_Description = @"1";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			line0.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;

			AssertEquals(0, line0.ChassisNumbers.Count);

			var chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "123456";
			AssertEquals(1, line0.ChassisNumbers.Count);
			chassis.JG_ReferenceNumber = "";
			AssertEquals(0, line0.ChassisNumbers.Count);

			chassis.JG_ReferenceNumber = "123456";
			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "123456";
			AssertEquals(1, line0.ChassisNumbers.Count);

			chassis.JG_ReferenceNumber = "1234567";
			AssertEquals(2, line0.ChassisNumbers.Count);

			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "";

			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "422211";
			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "111111";

			var chassisNumbers = line0.ChassisNumbers;
			AssertEquals("123456", chassisNumbers[0]);
			AssertEquals("1234567", chassisNumbers[1]);
			AssertEquals("422211", chassisNumbers[2]);
			AssertEquals("111111", chassisNumbers[3]);
		}

		public void TestIsShippingFromFactoryToDutyLevyingArea()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
			Assert("When JI_Procedure equal 31 should be ", invoiceLine.IsShippingFromFactoryToDutyLevyingArea);
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
			Assert("When JI_Procedure equal 35 should be ", invoiceLine.IsShippingFromFactoryToDutyLevyingArea);
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._50;
			Assert("When JI_Procedure equal 50 should be ", invoiceLine.IsShippingFromFactoryToDutyLevyingArea);

			invoiceLine.JI_Procedure = "13";
			Assert("When JI_Procedure equal 13 should be ", !invoiceLine.IsShippingFromFactoryToDutyLevyingArea);
		}

		public void TestMaxLength()
		{
			AssertEquals(512, JobComInvoiceLine.Schema.JI_DescriptionMaxLength);
			AssertEquals(512, JobComInvoiceLine.Schema.JI_NDescriptionMaxLength);
		}

		public void TestInvoiceLineLinkControllingMsgHeaders()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var header = jobDeclaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";

			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);

			controllingMessageHeader.Delete();
			AssertEquals(2, invoiceLineLinkControllingMsgHeaders.Count);

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";

			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);

			line.JI_CEI = ZGuid.Empty;
			AssertEquals(0, line.InvoiceLineLinkControllingMsgHeaders.Count);
		}

		public void TestLinkControllingAgencyPKs()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var header = jobDeclaration.Invoices.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;

			var controllingMessageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader4 = entryInstruction.ControllingMessageHeaders.AddNew();

			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction.PK;

			AssertEquals(0, line1.GetLinkControllingAgencyPKs().Length);

			line1.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			line1.InvoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			AssertEquals(2, line1.GetLinkControllingAgencyPKs().Length);

			AssertEquals(true, line1.GetLinkControllingAgencyPKs().Contains(controllingMessageHeader1.PK));
			AssertEquals(true, line1.GetLinkControllingAgencyPKs().Contains(controllingMessageHeader2.PK));
			AssertEquals(true, !line1.GetLinkControllingAgencyPKs().Contains(controllingMessageHeader3.PK));
			AssertEquals(true, !line1.GetLinkControllingAgencyPKs().Contains(controllingMessageHeader4.PK));
		}

		public void TestIsLinkSameControllingAgencyAndSameData()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var header = jobDeclaration.Invoices.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;

			var controllingMessageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader3 = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeader4 = entryInstruction.ControllingMessageHeaders.AddNew();

			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction.PK;
			var line2 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line2.JI_CEI = entryInstruction.PK;

			AssertEquals(false, line1.IsLinkSameControllingAgencyAndSameData((l1, l2) => true));
			AssertEquals(false, line2.IsLinkSameControllingAgencyAndSameData((l1, l2) => true));

			line1.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			line1.InvoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			line2.InvoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			line2.InvoiceLineLinkControllingMsgHeaders[3].IsLinkedCMHeader = true;
			AssertEquals(false, line1.IsLinkSameControllingAgencyAndSameData((l1, l2) => true));
			AssertEquals(false, line2.IsLinkSameControllingAgencyAndSameData((l1, l2) => true));

			line2.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			AssertEquals(true, line1.IsLinkSameControllingAgencyAndSameData((l1, l2) => true));
			AssertEquals(true, line2.IsLinkSameControllingAgencyAndSameData((l1, l2) => true));
		}

		public void TestClearDataByControllingAgency()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "DH", "VP" });
			var header = jobDeclaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaderDN = entryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().FirstOrDefault(x => x.TW1_ControllingAgency == "DN");
			var controllingMessageHeaderIF = entryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().FirstOrDefault(x => x.TW1_ControllingAgency == "IF");
			var controllingMessageHeaderDH = entryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().FirstOrDefault(x => x.TW1_ControllingAgency == "DH");
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, "DN", true);
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, "IF", true);
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, "DH", true);
			line.JI_BottledDate = ZDateTime.Now;
			line.JI_ExpirationDate = ZDateTime.Now;
			line.JI_AlcoholEndOfShelfLife = ZDateTime.Now;
			line.JI_AlcoholAge = 9999;
			line.JI_AlcoholYear = 7;
			line.JI_AlteredLotNoAmt = 1m;
			line.JI_RemovedLotNoAmt = 1m;
			line.JI_NoOriginalLotNoAmt = 1m;
			line.JI_AlcoholCountryRegion = "X";

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, "DN", false);
			AssertEquals(ZDateTime.Empty, line.JI_BottledDate);
			AssertEquals(ZDateTime.Empty, line.JI_ExpirationDate);
			AssertEquals(ZDateTime.Empty, line.JI_AlcoholEndOfShelfLife);
			AssertEquals(ZInt.Zero, line.JI_AlcoholAge);
			AssertEquals(ZInt.Zero, line.JI_AlcoholYear);
			AssertEquals(ZDecimal.Zero, line.JI_AlteredLotNoAmt);
			AssertEquals(ZDecimal.Zero, line.JI_RemovedLotNoAmt);
			AssertEquals(ZDecimal.Zero, line.JI_NoOriginalLotNoAmt);
			AssertEquals(ZString.Empty, line.JI_AlcoholCountryRegion);

			var controllingMessageHeaderVP = entryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().FirstOrDefault(x => x.TW1_ControllingAgency == "VP");
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, "VP", true);
			line.JI_QuarantineFeatures = "A";
			line.JI_QuarantineTreatment = "B";
			line.JI_VaccinationTypeDate = "C";
			line.JI_MicrochipID = "D";
			line.JI_AnimalAgeYear = 1;
			line.JI_AnimalAgeMonth = 2;
			line.JI_AnimalMaleQty = 3;
			line.JI_AnimalFemaleQty = 4;
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, "VP", false);
			AssertEquals(ZString.Empty, line.JI_QuarantineFeatures);
			AssertEquals(ZString.Empty, line.JI_QuarantineTreatment);
			AssertEquals(ZString.Empty, line.JI_VaccinationTypeDate);
			AssertEquals(ZString.Empty, line.JI_MicrochipID);
			AssertEquals(ZInt.Zero, line.JI_AnimalAgeYear);
			AssertEquals(ZInt.Zero, line.JI_AnimalAgeMonth);
			AssertEquals(ZInt.Zero, line.JI_AnimalMaleQty);
			AssertEquals(ZInt.Zero, line.JI_AnimalFemaleQty);
		}

		public void TestSetInvoiceLineTaxDefaultQuantityWhenInvoiceChange()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			var tariffType = helper.CreateTariffType("TW", "TXX");
			Factory.Save();

			var tariff1 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff1.PK, "CU2", "B");

			var tariff_1 = helper.CreateTariff("TW", tariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_1.PK, "CU1", "A");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "A";
			invoiceLine.JI_CustomsSecondUnitQty = "B";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;
			var invoiceLineTax = invoiceLine.Taxes.AddNew();

			invoiceLineTax.JLT_BaseQuantityUQ = "C";
			invoiceLine.JI_CustomsUnitQty = "C";
			AssertEquals(500M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLine.JI_CustomsQuantity = 600M;
			AssertEquals(600M, invoiceLineTax.JLT_BaseQuantity);

			invoiceLineTax.JLT_BaseQuantityUQ = "A";
			invoiceLine.JI_CustomsSecondUnitQty = "A";
			AssertEquals(100M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLine.JI_CustomsSecondQuantity = 200M;
			AssertEquals(200M, invoiceLineTax.JLT_BaseQuantity);

			invoiceLineTax.JLT_BaseQuantityUQ = "TNE";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			AssertEquals(600M * 0.001M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLine.JI_CustomsQuantity = 1200M;
			AssertEquals(1200M * 0.001M, invoiceLineTax.JLT_BaseQuantity);

			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000021";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;

			invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000021";
			AssertEquals("A", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(500M, invoiceLineTax.JLT_BaseQuantity);

			invoiceLine.JI_Tariff = "11";
			AssertEquals(0, invoiceLine.Taxes.Count);
		}

		public void TestInvoiceLineRefsWhenDelete()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "87";
			var chassis1 = invoiceLine1.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "10";
			var assigned1 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			assigned1.JG_ReferenceNumber = "10";
			var lineRef1 = invoiceLine1.InvoiceLineRefs.AddNew();
			lineRef1.JG_ReferenceNumber = "10";
			lineRef1.JG_ReferenceType = "AA";
			AssertEquals(3, invoiceLine1.InvoiceLineRefs.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNotNull(newFactory.Load<ChassisJobComInvLineRefs>(chassis1.PK));
			AssertNotNull(newFactory.Load<AssignedJobComInvLineRefs>(assigned1.PK));
			AssertNotNull(newFactory.Load<JobComInvLineRefs>(lineRef1.PK));
			invoiceLine1.Delete();
			Factory.Save();
			AssertNull(newFactory.Load<ChassisJobComInvLineRefs>(chassis1.PK));
			AssertNull(newFactory.Load<AssignedJobComInvLineRefs>(assigned1.PK));
			AssertNull(newFactory.Load<JobComInvLineRefs>(lineRef1.PK));
		}

		public void TestDefaultEntryInstruction()
		{
			var jobDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader1 = jobDeclaration1.Invoices.AddNew();

			var jobDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader2 = jobDeclaration2.Invoices.AddNew();

			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoiceHeader1.PK;
			AssertEquals(jobDeclaration1.CusEntryInstruction.PK, invoiceLine.JI_CEI);
			invoiceLine.JI_JZ = invoiceHeader2.PK;
			AssertEquals(jobDeclaration2.CusEntryInstruction.PK, invoiceLine.JI_CEI);

			jobDeclaration1.MakeNonPersistent();
			invoiceLine.JI_JZ = invoiceHeader1.PK;
			AssertEquals(ZGuid.Empty, invoiceLine.JI_CEI);
		}

		public void TestSaveInvoiceWhenJobDeclarationIsNonPersistent()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, invoiceLine.JI_CEI);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNotNull(newFactory.Load<JobComInvoiceLine>(invoiceLine.PK));
			AssertNull(newFactory.Load<JobDeclaration>(declaration.PK));
		}

		public void TestHasTariffCustomsRequirementsAttribute()
		{
			SetUpTariff(Constants.UniversalReferenceConstants.CusTariffAttributeValue.T);
			InvoiceLine.JI_Tariff = ZString.Empty;
			Assert(!InvoiceLine.HasTariffCustomsRequirementsAttribute("T"));
			InvoiceLine.JI_Tariff = "2713200000";
			Assert(!InvoiceLine.HasTariffCustomsRequirementsAttribute("T"));
			InvoiceLine.JI_Tariff = "2713200001";
			Assert(InvoiceLine.HasTariffCustomsRequirementsAttribute("T"));
		}

		public void TestInvoiceLineRelatedControllingMsgHeadersGenPivots()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var header = jobDeclaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";

			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);

			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;

			AssertEquals(3, line.InvoiceLineRelatedControllingMsgHeadersGenPivots.Count);
			Factory.Save();
			AssertEquals(3, line.InvoiceLineRelatedControllingMsgHeadersGenPivots.Count);

			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = false;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = false;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = false;
			AssertEquals(0, line.InvoiceLineRelatedControllingMsgHeadersGenPivots.Count);
			Factory.Save();
			AssertEquals(0, line.InvoiceLineRelatedControllingMsgHeadersGenPivots.Count);

			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			Factory.Save();
			AssertEquals(1, line.InvoiceLineRelatedControllingMsgHeadersGenPivots.Count);
			var genPivotPK = line.InvoiceLineRelatedControllingMsgHeadersGenPivots[0].PK;

			var newFactory = new BusinessObjectFactory();
			AssertNotNull(newFactory.Load<GenPivot>(genPivotPK));

			line.Delete();
			Factory.Save();
			AssertNull(newFactory.Load<GenPivot>(genPivotPK));
		}

		public void TestGetRelatedControllingMsgHeaders()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var header = jobDeclaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";

			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";

			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);

			var relatedControllingMsgHeaders = line.GetRelatedControllingMsgHeaders();
			AssertEquals(0, relatedControllingMsgHeaders.Length);

			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			relatedControllingMsgHeaders = line.GetRelatedControllingMsgHeaders();
			AssertEquals(1, relatedControllingMsgHeaders.Length);
			Assert(relatedControllingMsgHeaders.Contains(invoiceLineLinkControllingMsgHeaders[0]));

			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			relatedControllingMsgHeaders = line.GetRelatedControllingMsgHeaders();
			AssertEquals(2, relatedControllingMsgHeaders.Length);
			Assert(relatedControllingMsgHeaders.Contains(invoiceLineLinkControllingMsgHeaders[1]));

			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			relatedControllingMsgHeaders = line.GetRelatedControllingMsgHeaders();
			AssertEquals(3, relatedControllingMsgHeaders.Length);
			Assert(relatedControllingMsgHeaders.Contains(invoiceLineLinkControllingMsgHeaders[2]));
		}

		void SetUpTariff(ZString type)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, type, tariff2);
			Factory.Save();
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new JobComInvoiceLineLightValidationTester(bizObjToTest);
		}

		class JobComInvoiceLineLightValidationTester : LightValidationTester
		{
			public JobComInvoiceLineLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != Common.AutoCusEntryNum.Schema.CE_Category
					&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryIsSystemGenerated
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryNum
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_EntryType
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_ParentID
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_ParentTable
						&& propertyName != Common.AutoCusEntryNum.Schema.CE_RN_NKCountryCode;
			}
		}

		public void IsContainerLinkMandatory()
		{
			AssertEquals("IsContainerLinkMandatory should be", false, Factory.New<JobComInvoiceLine>().IsContainerLinkMandatory);
		}

		public void TestJI_TariffDescription()
		{
			var declaration = InvoiceLine.Declaration;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "I02", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "I01", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "E04", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "E03", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C06", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C05", tariff1);

			InvoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceLine.JI_TariffDescription);
			InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("C05, C06, I01, I02", InvoiceLine.JI_TariffDescription);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("C05, C06, E03, E04", InvoiceLine.JI_TariffDescription);
		}

		public void TestIsEnvironmentalProtectionTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff, "TRUE", tariff2);
			Factory.Save();

			InvoiceLine.JI_Tariff = ZString.Empty;
			Assert(!InvoiceLine.IsEnvironmentalProtectionTariff);
			InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			Assert(!InvoiceLine.IsEnvironmentalProtectionTariff);
			InvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			Assert(InvoiceLine.IsEnvironmentalProtectionTariff);
		}

		public void TestShouldJI_EPTDigit1IsZ()
		{
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
			Assert(InvoiceLine.ShouldJI_EPTDigit1IsZ);
			InvoiceLine.JI_EPTDigit1 = "X";
			Assert(!InvoiceLine.ShouldJI_EPTDigit1IsZ);
		}

		public void TestJI_EPTDigit2_ReadOnly()
		{
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
			Assert(InvoiceLine.JI_EPTDigit2_ReadOnly);
			InvoiceLine.JI_EPTDigit1 = "X";
			Assert(!InvoiceLine.JI_EPTDigit2_ReadOnly);
		}

		public void TestJI_EPTDigit2()
		{
			InvoiceLine.JI_EPTDigit2 = "X";
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
			AssertEquals("0", InvoiceLine.JI_EPTDigit2);
			InvoiceLine.JI_EPTDigit1 = "X";
			AssertEquals(ZString.Empty, InvoiceLine.JI_EPTDigit2);
			InvoiceLine.JI_EPTDigit2 = "X";
			InvoiceLine.JI_EPTDigit1 = "X";
			AssertEquals("X", InvoiceLine.JI_EPTDigit2);
		}

		public void TestJI_EPTDigit3_ReadOnly()
		{
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
			Assert(InvoiceLine.JI_EPTDigit3_ReadOnly);
			InvoiceLine.JI_EPTDigit1 = "X";
			Assert(!InvoiceLine.JI_EPTDigit3_ReadOnly);
		}

		public void TestJI_EPTDigit3()
		{
			InvoiceLine.JI_EPTDigit3 = "X";
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
			AssertEquals("0", InvoiceLine.JI_EPTDigit3);
			InvoiceLine.JI_EPTDigit1 = "X";
			AssertEquals(ZString.Empty, InvoiceLine.JI_EPTDigit3);
			InvoiceLine.JI_EPTDigit3 = "X";
			InvoiceLine.JI_EPTDigit1 = "X";
			AssertEquals("X", InvoiceLine.JI_EPTDigit3);
		}

		public void TestJI_BottledDate()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_BottledDateInfo, "DN");
		}

		public void TestJI_ExpirationDate()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_ExpirationDateInfo, "DN");
		}

		public void TestJI_AlcoholEndOfShelfLife()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_AlcoholEndOfShelfLifeInfo, "DN");
		}

		public void TestCommonRelatedColumnsReadOnly()
		{
			AssertPropertyReadOnlyAndClearValue(x => x.JI_CustomsThirdQuantityInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_CustomsThirdUnitQtyInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_GoodsTypeInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.PreviousPermitNoInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_ProductThicknessInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_ProductGradeInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_TariffExtensionCodeInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_InnerPackDescriptionInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_InnerPackingMaterialInfo);
			AssertPropertyReadOnlyAndClearValue(x => x.JI_InnerPackTypeInfo);
			AssertBusinessObjectCollectionReadOnlyAndClearValue(x => nameof(x.ShippingIdentificationDataCollection));
		}

		public void TestIsForCMHeaderByMessageTypes()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.AssignCMHeaderToInvoices(controllingMessageHeader);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			CombineAssertions(() =>
			{
				AssertEquals("There is no NX301 or NX401 CM Header", false, line.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX401));
				AssertEquals("Is CM Header of NX101", true, line.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX101, ControllingMessageTypeList.Codes.NX301));
			});
		}

		public void TestClearOrSetDefaultConcessionOrder()
		{
			TariffDataForTestHelper.GenerateTariffAndRateIncludingConcessionOrderData(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2020, 02, 15, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98050000010";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_PrimaryPreference = "PRE";
			AssertEquals("QUOTA", invoiceLine.JI_ConcessionOrder);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Taiwan;
			AssertEquals(ZString.Empty, invoiceLine.JI_ConcessionOrder);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("QUOTA", invoiceLine.JI_ConcessionOrder);

			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals(ZString.Empty, invoiceLine.JI_ConcessionOrder);

			invoiceLine.JI_PrimaryPreference = "PRE";
			AssertEquals("QUOTA", invoiceLine.JI_ConcessionOrder);

			invoiceLine.JI_Tariff = "98050000009";
			AssertEquals(ZString.Empty, invoiceLine.JI_ConcessionOrder);

			invoiceLine.JI_Tariff = "98050000010";
			AssertEquals("QUOTA", invoiceLine.JI_ConcessionOrder);
		}

		public void TestJI_ConcessionOrderReadOnly()
		{
			TariffDataForTestHelper.GenerateTariffAndRateIncludingConcessionOrderData(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2020, 02, 15, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98050000009";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_PrimaryPreference = "PRE";
			Assert("JI_ConcessionOrder is read-only", invoiceLine.JI_ConcessionOrderInfo.ReadOnly);

			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			Assert("JI_ConcessionOrder is not read-only", !invoiceLine.JI_ConcessionOrderInfo.ReadOnly);
		}

		public void TestJI_Calc_EnvironmentalProtectionCode()
		{
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.A;
			InvoiceLine.JI_EPTDigit2 = ContainerCapacityList.Codes._1;
			InvoiceLine.JI_EPTDigit3 = ContainerMaterialNumberList.Codes._1;
			AssertEquals("A11", InvoiceLine.JI_Calc_EnvironmentalProtectionCode);
			InvoiceLine.JI_EPTDigit1 = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceLine.JI_Calc_EnvironmentalProtectionCode);
			InvoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.A;
			InvoiceLine.JI_EPTDigit2 = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceLine.JI_Calc_EnvironmentalProtectionCode);
			InvoiceLine.JI_EPTDigit2 = ContainerCapacityList.Codes._1;
			InvoiceLine.JI_EPTDigit3 = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceLine.JI_Calc_EnvironmentalProtectionCode);
		}

		public void TestSetDeclarationGoodsDescriptionToReportAirCraftParts()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_NDescription = "航空器油漆材料";
			InvoiceLine.JI_Description = "Aircraft Antenna Equipment & Parts";
			InvoiceLine.AddInfoChild.TWL_AircraftPartsCategory = "6";
			InvoiceLine.AddInfoChild.TWL_AircraftPartsCode = "2";
			InvoiceLine.AddInfoChild.TWL_AircraftIPC = "49-22-11";
			InvoiceLine.JI_TextileWidth = 125m;
			InvoiceLine.JI_TextileWidthUQ = "FT";
			AssertEquals("Aircraft Antenna Equipment & Parts航空器油漆材料[6|2|49-22-11||||]", InvoiceLine.JI_DeclarationGoodsDescription);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("航空器油漆材料\r\nAircraft Antenna Equipment & Parts\r\nWIDTH: 125 '", InvoiceLine.JI_DeclarationGoodsDescription);
		}

		public void TestJI_BrandNameMaxLength()
		{
			AssertEquals(50, InvoiceLine.JI_BrandNameInfo.MaxLength);
		}

		public void TestJI_DeclarationGoodsDescription()
		{
			var noteDescription = PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description;
			var expectedCarInfoString = "MODEL YEAR: 2020 CAR TYPE: VAN (PASSENGER CAR) DOOR: 5\r\nBRAND: VOLVO MODEL: V90 Cross Country D5 AWD\r\nDISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5\r\nLEFT SIDE STEERING: YES\r\nENGINE TYPE: DIESEL FUEL\r\nTRANSMISSION: AUTOMATIC\r\nSTANDARD EQUIPMENT WITH EGR. and Catalyst Converter (Non CFC Refrigerant System)\r\nCHASSIS NO: YVAIOWQ92108080383;YCVSIWO0208300380\r\n";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_NDescription = "XX1";
			InvoiceLine.JI_Description = "XX2";

			InvoiceLine.JI_Tariff = "86";
			InvoiceLine.JI_ModelYear = 2020;
			InvoiceLine.JI_NumberOfDoor = 5;
			InvoiceLine.JI_CarType = CarTypeCodeList.Codes.A2;
			InvoiceLine.JI_BrandName = "VOLVO";
			InvoiceLine.JI_Model = "V90 Cross Country D5 AWD";
			InvoiceLine.JI_Displacement = "1969 C.C.";
			InvoiceLine.JI_Cylinders = 4;
			InvoiceLine.JI_Seats = 5;
			InvoiceLine.JI_LHD = YesNoList.Codes.Yes;
			InvoiceLine.JI_EngineType = EngineTypeCodeList.Codes.DS;
			InvoiceLine.JI_Transmission = TransmissionCodeList.Codes.Auto;
			InvoiceLine.JI_EquipmentPrintMode = EquipmentPrintModeList.Codes.EGR;
			var chassis1 = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "YVAIOWQ92108080383";
			var chassis2 = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis2.JG_ReferenceNumber = "YCVSIWO0208300380";
			AssertEquals(20000, InvoiceLine.JI_DeclarationGoodsDescriptionInfo.MaxLength);
			AssertEquals("XX1\r\nXX2", InvoiceLine.JI_DeclarationGoodsDescription);
			Assert(!InvoiceLine.OverrideDeclarationGoodsDescription);
			AssertEquals(0, InvoiceLine.Notes.FindByDescription(noteDescription).Length);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var expected = "XX1\r\nXX2\r\n" + expectedCarInfoString;
			AssertEquals(expected, InvoiceLine.JI_DeclarationGoodsDescription);

			InvoiceLine.OverrideDeclarationGoodsDescription = true;
			InvoiceLine.JI_DeclarationGoodsDescription = "AAABBB11222";
			AssertEquals("AAABBB11222", InvoiceLine.Notes.GetNoteText(noteDescription));
			Assert(!InvoiceLine.JI_DeclarationGoodsDescriptionInfo.ReadOnly);

			InvoiceLine.OverrideDeclarationGoodsDescription = false;
			AssertEquals(0, InvoiceLine.Notes.FindByDescription(noteDescription).Length);
			Assert(InvoiceLine.JI_DeclarationGoodsDescriptionInfo.ReadOnly);

			var withDimensionExpected = expected + "\r\nWIDTH: 232.123 \"";
			InvoiceLine.JI_TextileWidth = 232.123m;
			InvoiceLine.JI_TextileWidthUQ = "IN";
			AssertEquals(withDimensionExpected, InvoiceLine.JI_DeclarationGoodsDescription);

			withDimensionExpected = expected + "\r\nWIDTH: 232.123 \'";
			InvoiceLine.JI_TextileWidth = 232.123m;
			InvoiceLine.JI_TextileWidthUQ = "FT";
			AssertEquals(withDimensionExpected, InvoiceLine.JI_DeclarationGoodsDescription);

			withDimensionExpected = expected + "\r\nWIDTH: 232.123 mm";
			InvoiceLine.JI_TextileWidth = 232.123m;
			InvoiceLine.JI_TextileWidthUQ = "MM";
			AssertEquals(withDimensionExpected, InvoiceLine.JI_DeclarationGoodsDescription);

			withDimensionExpected = expected + "\r\nWIDTH: 232.123 cm";
			InvoiceLine.JI_TextileWidth = 232.123m;
			InvoiceLine.JI_TextileWidthUQ = "CM";
			AssertEquals(withDimensionExpected, InvoiceLine.JI_DeclarationGoodsDescription);

			withDimensionExpected = expected + "\r\nWIDTH: 232.123 m";
			InvoiceLine.JI_TextileWidth = 232.123m;
			InvoiceLine.JI_TextileWidthUQ = "M";
			AssertEquals(withDimensionExpected, InvoiceLine.JI_DeclarationGoodsDescription);
		}

		public void TestJI_PermitGoodsDescription()
		{
			InvoiceLine.JI_PermitGoodsDescription = "Test Permit Goods Description";
			Factory.Save();

			var noteQuery = new ZQuery(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, InvoiceLine.PK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, "Permit Goods Description");
			noteQuery.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.Equal, "JobComInvoiceLine");
			var note = Factory.LoadTop1<StmNote>(noteQuery);
			AssertEquals("Test Permit Goods Description", note.ST_NoteText);

			note.ST_NoteText = "Modify Permit Goods Description";
			Factory.Save();

			var loadInvoiceLine = Factory.LoadTop1<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.PK, SQLComparisonOperator.Equal, InvoiceLine.PK));
			AssertEquals("Modify Permit Goods Description", loadInvoiceLine.JI_PermitGoodsDescription);
		}

		public void TestEmptyOverseasFreightExpressedInItsCurrency()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				var koreanCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
				var localCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "TWD");
				Declaration.JE_ExportDate = ZDateTime.Today;
				CurrencyConverterTestHelper.SetExchangeRate(Factory, koreanCurrency, 0.5m, Declaration.JE_ExportDate, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
				CurrencyConverterTestHelper.SetExchangeRate(Factory, localCurrency, 1.7m, Declaration.JE_ExportDate, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);

				InvoiceHeader.JZ_InvoiceAmount = 1000;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = koreanCurrency.RX_Code;
				InvoiceHeader.JZ_IncoTerm = "CIF";

				InvoiceHeader.Charges.RemoveAndDeleteAll();
				Declaration.TopGroupInvoice.Charges.RemoveAndDeleteAll();

				InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 0, koreanCurrency.RX_Code);
				InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 20, Declaration.LocalCurrencyCode);

				InvoiceLine.JI_LinePrice = 980m;
				Declaration.ResumeApportionment();
				AssertEquals("Line OFT amount", 0m, InvoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OFT currency", koreanCurrency.RX_Code, InvoiceLine.JI_OverseasFreight.Currency.Code);
				AssertEquals("Line ONS currency", Declaration.LocalCurrencyCode, InvoiceLine.JI_OverseasInsurance.Currency.Code);
				AssertEquals("JI_Calc_Insurance", 10m, InvoiceLine.JI_Calc_InsuranceInInvoiceCurr);
				AssertEquals("JI_Calc_Freight", 0m, InvoiceLine.JI_Calc_FreightInInvoiceCurr);
			}
		}

		public void TestDefaultCustomsQtyFromNetWeight()
		{
			var tariffTypeHSN = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			Factory.Save();

			var hsnCu1Tariff1 = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffUOM(hsnCu1Tariff1.PK, "CU1", "KGM");
			var hsnCu1Tariff2 = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.CreateTariffUOM(hsnCu1Tariff2.PK, "CU1", "TNE");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeightUQ = ZString.Empty;
			invoiceLine.JI_Tariff = "01012100003";

			invoiceLine.JI_NetWeight = 10m;
			AssertEquals("JI_CustomsQuantity", 0m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("JI_CustomsQuantity", 10m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_NetWeight = 1000m;
			AssertEquals("JI_CustomsQuantity", 1000m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("JI_CustomsQuantity", 1m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_NetWeightUQ = "X";
			AssertEquals("JI_CustomsQuantity", 0m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_Tariff = "01012100004";
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 1000m;
			AssertEquals("JI_CustomsQuantity", 1m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("JI_CustomsQuantity", 1000m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestShouldConverCustomsQuantity2()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			Assert(invoiceLine.ShouldConverCustomsQuantity2);
		}

		public void TestJI_CustomsValueDecimalPlaces()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_CustomsValue", true, attrib => attrib.DecimalPlaces == 0);
		}

		#region RepairAssemblyProcessing
		public void TestJI_RAPPrice()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;

			InvoiceLine.JI_InvoiceQuantity = 20;
			InvoiceLine.JI_Calc_RAPRORUnitPrice = 2m;
			AssertEquals(40m, InvoiceLine.JI_RAPPrice);
			InvoiceLine.JI_InvoiceQuantity = 10;
			AssertEquals(40m, InvoiceLine.JI_RAPPrice);
			InvoiceLine.JI_Calc_RAPRORUnitPrice = 2m;
			AssertEquals(20m, InvoiceLine.JI_RAPPrice);

			InvoiceLine.JI_RAPPrice = 10m;
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_RAPPrice);
			InvoiceLine.JI_RAPPrice = 10m;
			Declaration.JE_MessageType = ZString.Empty;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_RAPPrice);

			InvoiceLine.JI_RAPPrice = 10m;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_RAPPrice);
			InvoiceLine.JI_RAPPrice = 10m;
			InvoiceLine.JI_Procedure = ZString.Empty;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_RAPPrice);

			InvoiceLine.JI_RAPPrice = 10m;
			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			AssertEquals(10m, InvoiceLine.JI_RAPPrice);
			InvoiceLine.JI_RAPPrice = 10m;
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_RAPPrice);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_RAPPriceInfo);
			AssertEquals("Caption", "RAP/ROR Price", resourceStringData.Caption);
		}

		public void TestJI_Calc_RAPRORUnitPrice()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;

			InvoiceLine.JI_RAPPrice = 10m;
			InvoiceLine.JI_InvoiceQuantity = 10;
			AssertEquals(1m, InvoiceLine.JI_Calc_RAPRORUnitPrice);
			InvoiceLine.JI_RAPPrice = 20m;
			AssertEquals(2m, InvoiceLine.JI_Calc_RAPRORUnitPrice);
			InvoiceLine.JI_InvoiceQuantity = 20;
			AssertEquals(1m, InvoiceLine.JI_Calc_RAPRORUnitPrice);

			InvoiceLine.JI_Calc_RAPRORUnitPrice = 10m;
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_Calc_RAPRORUnitPrice);
			InvoiceLine.JI_Calc_RAPRORUnitPrice = 10m;
			Declaration.JE_MessageType = ZString.Empty;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_Calc_RAPRORUnitPrice);

			InvoiceLine.JI_Calc_RAPRORUnitPrice = 10m;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_Calc_RAPRORUnitPrice);
			InvoiceLine.JI_Calc_RAPRORUnitPrice = 10m;
			InvoiceLine.JI_Procedure = ZString.Empty;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_Calc_RAPRORUnitPrice);

			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			InvoiceLine.JI_RAPPrice = 10m;
			InvoiceLine.JI_InvoiceQuantity = 10;
			AssertEquals(1m, InvoiceLine.JI_Calc_RAPRORUnitPrice);
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			AssertEquals(ZDecimal.Zero, InvoiceLine.JI_Calc_RAPRORUnitPrice);

			BusinessObjectCaptionTestHelper.CombineAssertCaptions(InvoiceLine.JI_Calc_RAPRORUnitPriceInfo, "RAP/ROR Unit Price", "RAP/ROR U.P.");
		}

		public void TestJI_UseOneTenthCV()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;

			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCV);
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			Declaration.JE_MessageType = ZString.Empty;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCV);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCV);
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			InvoiceLine.JI_Procedure = ZString.Empty;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCV);
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCV);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCV);
		}

		public void TestJI_UseOneTenthCV_Attribute()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(line.JI_UseOneTenthCVInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Use 10% CV", resourceStringData.Caption);
				AssertEquals("Short Caption", "10% CV", resourceStringData.ShortCaption);
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(JobComInvoiceLine), "JI_UseOneTenthCV", true, attrib => attrib.Member == "JI_UseOneTenthCVReadOnly");
			});
		}

		public void TestJI_UseOneTenthCVReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
			AssertEquals(false, InvoiceLine.JI_UseOneTenthCVReadOnly);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._35;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
			AssertEquals(true, InvoiceLine.JI_UseOneTenthCVReadOnly);
		}

		public void TestJI_RAPPriceReadOnly()
		{
			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			AssertEquals(false, InvoiceLine.JI_RAPRORPriceReadOnly);

			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			AssertEquals(true, InvoiceLine.JI_RAPRORPriceReadOnly);
		}

		public void TestJI_Calc_RAPRORUnitPriceReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			AssertEquals(true, InvoiceLine.JI_Calc_RAPRORUnitPriceReadOnly);

			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			AssertEquals(false, InvoiceLine.JI_Calc_RAPRORUnitPriceReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._04;
			AssertEquals(true, InvoiceLine.JI_Calc_RAPRORUnitPriceReadOnly);
		}

		public void TestJI_RAPCurr()
		{
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertEquals("TWD", InvoiceLine.JI_RAPCurr);
		}

		public void TestJI_RAPCurrReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			AssertEquals(false, InvoiceLine.JI_RAPRORCurrReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			AssertEquals(false, InvoiceLine.JI_RAPRORCurrReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
			InvoiceLine.JI_UseOneTenthCV = ZBool.False;
			AssertEquals(true, InvoiceLine.JI_RAPRORCurrReadOnly);

			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			InvoiceLine.JI_UseOneTenthCV = ZBool.True;
			AssertEquals(true, InvoiceLine.JI_RAPRORCurrReadOnly);
		}

		public void TestRAPPriceRefCurrency()
		{
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertNotNull(InvoiceLine.RAPRORPriceRefCurrency);
		}

		public void TestJI_Calc_RAPPriceLocalAmount()
		{
			InvoiceLine.JI_RAPCurr = "TWD";
			InvoiceLine.JI_RAPPrice = 123456789012m;
			AssertEquals(123456789012m, InvoiceLine.JI_Calc_RAPRORPriceLocalAmount);
		}

		public void TestJI_Calc_RAPRORUnitCurr()
		{
			InvoiceLine.JI_RAPCurr = "TWD";
			AssertEquals("TWD", InvoiceLine.JI_Calc_RAPRORUnitCurr);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_Calc_RAPRORUnitCurrInfo);
			AssertEquals("Caption", "Curr.", resourceStringData.Caption);
		}

		public void TestIsRAPOrROR()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				Assert("IsRAPOrROR should be true when IsRAP is true", InvoiceLine.IsRAPOrROR);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				Assert("IsRAPOrROR should be true when IsROR is true", InvoiceLine.IsRAPOrROR);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._99;
				Assert("IsRAPOrROR should be false when IsROR is false and IsRAP is false", !InvoiceLine.IsRAPOrROR);
			});
		}

		public void TestIsRAP()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				Assert("37 is RAP", InvoiceLine.IsRAP);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
				Assert("39 is RAP", InvoiceLine.IsRAP);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3F;
				Assert("3F is RAP", InvoiceLine.IsRAP);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				Assert("38 is not RAP", !InvoiceLine.IsRAP);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
				Assert("3E is not RAP", !InvoiceLine.IsRAP);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
				Assert("31 is not RAP", !InvoiceLine.IsRAP);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3F;
				Assert("Export is not RAP", !InvoiceLine.IsRAP);
			});
		}

		public void TestIsROR()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				Assert("38 is ROR", InvoiceLine.IsROR);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
				Assert("3E is ROR", InvoiceLine.IsROR);

				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				Assert("37 is not ROR", !InvoiceLine.IsROR);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
				Assert("39 is not ROR", !InvoiceLine.IsROR);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3F;
				Assert("3F is not ROR", !InvoiceLine.IsROR);
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
				Assert("31 is not ROR", !InvoiceLine.IsROR);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
				Assert("Export is not ROR", !InvoiceLine.IsROR);
			});
		}

		public void TestShouldAllowOneTenthCVasRAPPrice()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			Assert(InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._39;
			Assert(InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._31;
			Assert(!InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			Assert(InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
			Assert(InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			Assert(!InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			Assert(!InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._3E;
			Assert(!InvoiceLine.ShouldAllowOneTenthCVasRAPRORPrice);
		}

		public void TestSetDefaultRAPValuesIfNeeded()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_RAPCurr = "TWD";
			InvoiceLine.JI_RAPPrice = 1000m;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			InvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertEquals("USD", InvoiceLine.JI_RAPCurr);
			AssertEquals(0m, InvoiceLine.JI_RAPPrice);
			InvoiceLine.JI_RAPPrice = 1000m;
			InvoiceLine.JI_Procedure = "XX";
			AssertEquals(ZString.Empty, InvoiceLine.JI_RAPCurr);
			AssertEquals(0m, InvoiceLine.JI_RAPPrice);
		}
		#endregion

		public void TestReservedFields()
		{
			var invoiceline = Factory.NewWithValidTestData<JobComInvoiceLine>();
			AssertType<JobComInvoiceLineReservedFieldCollection>(invoiceline.ReservedFields);
			var reservedField = invoiceline.ReservedFields.AddNew();
			AssertEquals(1, invoiceline.ReservedFields.Count);
			AssertType<JobComInvoiceLineReservedField>(reservedField);
			var supporter = invoiceline as IReservedFieldSupporter;
			AssertNotNull(supporter);
			AssertEquals(1, supporter.GetReservedFields().Count());
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "PreviousBondedEntryLineNumber")
			{
				base.TestBizObjectField(info);
			}
		}

		public void TestPreviousBondedEntryLineNumber()
		{
			InvoiceLine.PreviousBondedEntryLineNumber = 0;
			AssertEquals(new ZShort(0), InvoiceLine.PreviousBondedEntryLineNumber);
			InvoiceLine.PreviousBondedEntryLineNumber = 44;
			AssertEquals(new ZShort(44), InvoiceLine.PreviousBondedEntryLineNumber);
			AssertEquals(4, InvoiceLine.PreviousBondedEntryLineNumberInfo.MaxLength);
		}

		public void TestPreviousBondedEntryNumber()
		{
			AssertEquals(14, InvoiceLine.PreviousBondedEntryNumberInfo.MaxLength);
		}

		public void TestTypeApprovalCertificateNumbersReadOnly()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "20", "CI", "2Q", "CD", "IF", "DH", "VP", "DN" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			AssertTypeApprovalCertificateNumbersReadOnlyAndClearValueWhenSwitchToReadOnlyByControllingAgency(line1, "20");
			AssertTypeApprovalCertificateNumbersReadOnlyAndClearValueWhenSwitchToReadOnlyByControllingAgency(line1, "CI");
			AssertTypeApprovalCertificateNumbersReadOnlyAndClearValueWhenSwitchToReadOnlyByControllingAgency(line1, "2Q");
			AssertTypeApprovalCertificateNumbersNoReadOnlyByControllingAgency(line1, "CD");
			AssertTypeApprovalCertificateNumbersNoReadOnlyByControllingAgency(line1, "IF");
			AssertTypeApprovalCertificateNumbersNoReadOnlyByControllingAgency(line1, "DH");
			AssertTypeApprovalCertificateNumbersNoReadOnlyByControllingAgency(line1, "VP");
			AssertTypeApprovalCertificateNumbersNoReadOnlyByControllingAgency(line1, "DN");
		}

		public void TestAnimalAndPlantRelatedColumnsReadOnly()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.AssignCMHeaderToInvoices(controllingMessageHeader);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			controllingMessageHeader.TW1_BusinessType = ZString.Empty;
			CombineAssertions("Linked NX401, BusinessType is Empty", () =>
			{
				AssertEquals("JI_QuarantineTreatmentInfo ReadOnly", true, line.JI_QuarantineTreatmentInfo.ReadOnly);
				AssertEquals("JI_QuarantineFeaturesInfo ReadOnly", true, line.JI_QuarantineFeaturesInfo.ReadOnly);
				AssertEquals("JI_VaccinationTypeDateInfo ReadOnly", true, line.JI_VaccinationTypeDateInfo.ReadOnly);
				AssertEquals("PackingHouseCollection ReadOnly", true, line.PackingHouseCollection.ReadOnly);
				AssertEquals("PackingDateCollection ReadOnly", true, line.PackingDateCollection.ReadOnly);
				AssertEquals("JI_MicrochipIDInfo ReadOnly", true, line.JI_MicrochipIDInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeYearInfo ReadOnly", true, line.JI_AnimalAgeYearInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeMonthInfo ReadOnly", true, line.JI_AnimalAgeMonthInfo.ReadOnly);
				AssertEquals("JI_AnimalMaleQtyInfo ReadOnly", true, line.JI_AnimalMaleQtyInfo.ReadOnly);
				AssertEquals("JI_AnimalFemaleQtyInfo ReadOnly", true, line.JI_AnimalFemaleQtyInfo.ReadOnly);
				AssertEquals("SlaughterDateCollection ReadOnly", true, line.SlaughterDateCollection.ReadOnly);
			});

			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal;
			CombineAssertions("Linked NX401, BusinessType is 40", () =>
			{
				AssertEquals("JI_QuarantineTreatmentInfo ReadOnly", false, line.JI_QuarantineTreatmentInfo.ReadOnly);
				AssertEquals("JI_QuarantineFeaturesInfo ReadOnly", false, line.JI_QuarantineFeaturesInfo.ReadOnly);
				AssertEquals("JI_VaccinationTypeDateInfo ReadOnly", false, line.JI_VaccinationTypeDateInfo.ReadOnly);
				AssertEquals("PackingHouseCollection ReadOnly", true, line.PackingHouseCollection.ReadOnly);
				AssertEquals("PackingDateCollection ReadOnly", false, line.PackingDateCollection.ReadOnly);
				AssertEquals("JI_MicrochipIDInfo ReadOnly", false, line.JI_MicrochipIDInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeYearInfo ReadOnly", false, line.JI_AnimalAgeYearInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeMonthInfo ReadOnly", false, line.JI_AnimalAgeMonthInfo.ReadOnly);
				AssertEquals("JI_AnimalMaleQtyInfo ReadOnly", false, line.JI_AnimalMaleQtyInfo.ReadOnly);
				AssertEquals("JI_AnimalFemaleQtyInfo ReadOnly", false, line.JI_AnimalFemaleQtyInfo.ReadOnly);
				AssertEquals("SlaughterDateCollection ReadOnly", false, line.SlaughterDateCollection.ReadOnly);
			});

			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfImportPlant;
			CombineAssertions("Linked NX401, BusinessType is 60", () =>
			{
				AssertEquals("JI_QuarantineTreatmentInfo ReadOnly", false, line.JI_QuarantineTreatmentInfo.ReadOnly);
				AssertEquals("JI_QuarantineFeaturesInfo ReadOnly", false, line.JI_QuarantineFeaturesInfo.ReadOnly);
				AssertEquals("JI_VaccinationTypeDateInfo ReadOnly", true, line.JI_VaccinationTypeDateInfo.ReadOnly);
				AssertEquals("PackingHouseCollection ReadOnly", false, line.PackingHouseCollection.ReadOnly);
				AssertEquals("PackingDateCollection ReadOnly", true, line.PackingDateCollection.ReadOnly);
				AssertEquals("JI_MicrochipIDInfo ReadOnly", true, line.JI_MicrochipIDInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeYearInfo ReadOnly", true, line.JI_AnimalAgeYearInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeMonthInfo ReadOnly", true, line.JI_AnimalAgeMonthInfo.ReadOnly);
				AssertEquals("JI_AnimalMaleQtyInfo ReadOnly", true, line.JI_AnimalMaleQtyInfo.ReadOnly);
				AssertEquals("JI_AnimalFemaleQtyInfo ReadOnly", true, line.JI_AnimalFemaleQtyInfo.ReadOnly);
				AssertEquals("SlaughterDateCollection ReadOnly", true, line.SlaughterDateCollection.ReadOnly);
			});

			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfExportAnimal;
			CombineAssertions("Linked NX401, BusinessType is 30", () =>
			{
				AssertEquals("JI_QuarantineTreatmentInfo ReadOnly", false, line.JI_QuarantineTreatmentInfo.ReadOnly);
				AssertEquals("JI_QuarantineFeaturesInfo ReadOnly", false, line.JI_QuarantineFeaturesInfo.ReadOnly);
				AssertEquals("JI_VaccinationTypeDateInfo ReadOnly", false, line.JI_VaccinationTypeDateInfo.ReadOnly);
				AssertEquals("PackingHouseCollection ReadOnly", true, line.PackingHouseCollection.ReadOnly);
				AssertEquals("PackingDateCollection ReadOnly", true, line.PackingDateCollection.ReadOnly);
				AssertEquals("JI_MicrochipIDInfo ReadOnly", false, line.JI_MicrochipIDInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeYearInfo ReadOnly", false, line.JI_AnimalAgeYearInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeMonthInfo ReadOnly", false, line.JI_AnimalAgeMonthInfo.ReadOnly);
				AssertEquals("JI_AnimalMaleQtyInfo ReadOnly", false, line.JI_AnimalMaleQtyInfo.ReadOnly);
				AssertEquals("JI_AnimalFemaleQtyInfo ReadOnly", false, line.JI_AnimalFemaleQtyInfo.ReadOnly);
				AssertEquals("SlaughterDateCollection ReadOnly", true, line.SlaughterDateCollection.ReadOnly);
			});

			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfExportPlant;
			CombineAssertions("Linked NX401, BusinessType is 50", () =>
			{
				AssertEquals("JI_QuarantineTreatmentInfo ReadOnly", false, line.JI_QuarantineTreatmentInfo.ReadOnly);
				AssertEquals("JI_QuarantineFeaturesInfo ReadOnly", false, line.JI_QuarantineFeaturesInfo.ReadOnly);
				AssertEquals("JI_VaccinationTypeDateInfo ReadOnly", true, line.JI_VaccinationTypeDateInfo.ReadOnly);
				AssertEquals("PackingHouseCollection ReadOnly", true, line.PackingHouseCollection.ReadOnly);
				AssertEquals("PackingDateCollection ReadOnly", true, line.PackingDateCollection.ReadOnly);
				AssertEquals("JI_MicrochipIDInfo ReadOnly", true, line.JI_MicrochipIDInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeYearInfo ReadOnly", true, line.JI_AnimalAgeYearInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeMonthInfo ReadOnly", true, line.JI_AnimalAgeMonthInfo.ReadOnly);
				AssertEquals("JI_AnimalMaleQtyInfo ReadOnly", true, line.JI_AnimalMaleQtyInfo.ReadOnly);
				AssertEquals("JI_AnimalFemaleQtyInfo ReadOnly", true, line.JI_AnimalFemaleQtyInfo.ReadOnly);
				AssertEquals("SlaughterDateCollection ReadOnly", true, line.SlaughterDateCollection.ReadOnly);
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			CombineAssertions("Unlinked NX401", () =>
			{
				AssertEquals("JI_QuarantineTreatmentInfo ReadOnly", true, line.JI_QuarantineTreatmentInfo.ReadOnly);
				AssertEquals("JI_QuarantineFeaturesInfo ReadOnly", true, line.JI_QuarantineFeaturesInfo.ReadOnly);
				AssertEquals("JI_VaccinationTypeDateInfo ReadOnly", true, line.JI_VaccinationTypeDateInfo.ReadOnly);
				AssertEquals("PackingHouseCollection ReadOnly", true, line.PackingHouseCollection.ReadOnly);
				AssertEquals("PackingDateCollection ReadOnly", true, line.PackingDateCollection.ReadOnly);
				AssertEquals("JI_MicrochipIDInfo ReadOnly", true, line.JI_MicrochipIDInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeYearInfo ReadOnly", true, line.JI_AnimalAgeYearInfo.ReadOnly);
				AssertEquals("JI_AnimalAgeMonthInfo ReadOnly", true, line.JI_AnimalAgeMonthInfo.ReadOnly);
				AssertEquals("JI_AnimalMaleQtyInfo ReadOnly", true, line.JI_AnimalMaleQtyInfo.ReadOnly);
				AssertEquals("JI_AnimalFemaleQtyInfo ReadOnly", true, line.JI_AnimalFemaleQtyInfo.ReadOnly);
				AssertEquals("SlaughterDateCollection ReadOnly", true, line.SlaughterDateCollection.ReadOnly);
			});
		}

		void AssertTypeApprovalCertificateNumbersReadOnlyAndClearValueWhenSwitchToReadOnlyByControllingAgency(JobComInvoiceLine line, ZString controllingAgency)
		{
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
			Assert(!line.TypeApprovalCertificateNumbers.ReadOnly);
			line.TypeApprovalCertificateNumbers.CSI_ReferenceNumber = new ZString("1");
			line.TypeApprovalCertificateNumbers.CSI_ReferenceNumber2 = new ZString("1");
			line.TypeApprovalCertificateNumbers.CSI_Description = new ZString("1");
			line.TypeApprovalCertificateNumbers.CSI_Code = new ZString("1");
			Assert(!line.TypeApprovalCertificateNumbers.CSI_ReferenceNumber.IsEmpty);
			Assert(!line.TypeApprovalCertificateNumbers.CSI_ReferenceNumber2.IsEmpty);
			Assert(!line.TypeApprovalCertificateNumbers.CSI_Description.IsEmpty);
			Assert(!line.TypeApprovalCertificateNumbers.CSI_Code.IsEmpty);

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
			Assert(line.TypeApprovalCertificateNumbers.ReadOnly);
			Assert(line.TypeApprovalCertificateNumbers.CSI_ReferenceNumber.IsEmpty);
			Assert(line.TypeApprovalCertificateNumbers.CSI_ReferenceNumber2.IsEmpty);
			Assert(line.TypeApprovalCertificateNumbers.CSI_Description.IsEmpty);
			Assert(line.TypeApprovalCertificateNumbers.CSI_Code.IsEmpty);
		}

		void AssertTypeApprovalCertificateNumbersNoReadOnlyByControllingAgency(JobComInvoiceLine line, ZString controllingAgency)
		{
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
			Assert(line.TypeApprovalCertificateNumbers.ReadOnly);

			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
			Assert(line.TypeApprovalCertificateNumbers.ReadOnly);
		}

		public void TestJI_AlcoholAge()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_AlcoholAgeInfo, "DN");
		}

		public void TestJI_AlcoholYear()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_AlcoholYearInfo, "DN");
		}

		public void TestJI_AlteredLotNoAmt()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_AlteredLotNoAmtInfo, "DN");
		}

		public void TestJI_RemovedLotNoAmt()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_RemovedLotNoAmtInfo, "DN");
		}

		public void TestJI_NoOriginalLotNoAmt()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_NoOriginalLotNoAmtInfo, "DN");
		}

		public void TestJI_AlcoholCountryRegion()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CI" });
			var header = jobDeclaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.AssertReadOnlyByControllingAgency(line1, line1.JI_AlcoholCountryRegionInfo, "DN");
		}

		public void TestHasLinkedCMHeader()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals(false, invoiceLine.HasLinkedCMHeader);
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "XX";
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;
			AssertEquals(true, invoiceLine.HasLinkedCMHeader);
			link.IsLinkedCMHeader = false;
			AssertEquals(false, invoiceLine.HasLinkedCMHeader);
		}

		public void TestCertificateOfOriginSupported()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals(false, invoiceLine.CertificateOfOriginSupported);
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.X101);
			link.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.CertificateOfOriginSupported);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			AssertEquals(true, invoiceLine.CertificateOfOriginSupported);
		}

		public void TestAlcoholSupported()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals(false, invoiceLine.AlcoholSupported);
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.X101);
			link.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.AlcoholSupported);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			AssertEquals(true, invoiceLine.AlcoholSupported);
		}

		public void TestTypeApprovalSupported()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals(false, invoiceLine.TypeApprovalSupported);
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.X101);
			link.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.TypeApprovalSupported);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			AssertEquals(true, invoiceLine.TypeApprovalSupported);
		}

		public void TestAnimalAndPlantSupported()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals(false, invoiceLine.AnimalAndPlantSupported);
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.X101);
			link.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.AnimalAndPlantSupported);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			AssertEquals(true, invoiceLine.AnimalAndPlantSupported);
		}

		public void TestFoodAndDrugSupported()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals(false, invoiceLine.FoodAndDrugSupported);
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.X101);
			link.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLine.FoodAndDrugSupported);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertEquals(true, invoiceLine.FoodAndDrugSupported);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			AssertEquals(true, invoiceLine.FoodAndDrugSupported);
			link.IsLinkedCMHeader = false;
			AssertEquals(false, invoiceLine.FoodAndDrugSupported);
		}

		public void TestDefaultPrimaryPreference()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var preferencePR2 = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Preference2, "PR2", "TW");
			var preferencePR1 = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Preference1, "PR1", "TW");
			var preferenceSTD = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Standard, "STD", "TW");

			var preferencePT2 = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.ProvisionalPreference2, "PT2", "TW");
			var preferencePT1 = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.ProvisionalPreference1, "PT1", "TW");
			var preferencePT3 = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.ProvisionalPreference3, "PT3", "TW");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();

			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;
			var tradeGroup = universalReferenceTestDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "TEST1", date1, date2);
			universalReferenceTestDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, date1.Date, date2.Date);
			var hsnTariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType("TW", "HSN");
			Factory.Save();
			var rateType = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType("TW", Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			Factory.Save();
			var cusTariff = universalReferenceTestDataHelper.CreateTariff("TW", hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			var sTDRate = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceSTD.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(sTDRate, tradeGroup, date1, date2, "add11", "ord11");
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testInvLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, testInvLine.JI_PrimaryPreference);

			testInvLine.JI_Tariff = "123456789";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(Constants.PreferenceCodes.Standard, testInvLine.JI_PrimaryPreference);

			var pR1Rate = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePR1.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(pR1Rate, tradeGroup, date1, date2, "add21", "ord21");
			Factory.Save();

			testInvLine.JI_PrimaryPreference = ZString.Empty;
			testInvLine.JI_Tariff = ZString.Empty;
			testInvLine.JI_CountryOfOrigin = ZString.Empty;
			testInvLine.JI_Tariff = "123456789";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(Constants.PreferenceCodes.Preference1, testInvLine.JI_PrimaryPreference);

			var pR2Rate = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePR2.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(pR2Rate, tradeGroup, date1, date2, "add31", "ord31");
			Factory.Save();

			testInvLine.JI_PrimaryPreference = ZString.Empty;
			testInvLine.JI_Tariff = ZString.Empty;
			testInvLine.JI_CountryOfOrigin = ZString.Empty;
			testInvLine.JI_Tariff = "123456789";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(Constants.PreferenceCodes.Preference2, testInvLine.JI_PrimaryPreference);

			var pT3Rate = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePT3.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(pT3Rate, tradeGroup, date1, date2, "add41", "ord41");
			Factory.Save();

			testInvLine.JI_PrimaryPreference = ZString.Empty;
			testInvLine.JI_Tariff = ZString.Empty;
			testInvLine.JI_CountryOfOrigin = ZString.Empty;
			testInvLine.JI_Tariff = "123456789";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(Constants.PreferenceCodes.ProvisionalPreference3, testInvLine.JI_PrimaryPreference);

			var pT1Rate = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePT1.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(pT1Rate, tradeGroup, date1, date2, "add51", "ord51");
			Factory.Save();

			testInvLine.JI_PrimaryPreference = ZString.Empty;
			testInvLine.JI_Tariff = ZString.Empty;
			testInvLine.JI_CountryOfOrigin = ZString.Empty;
			testInvLine.JI_Tariff = "123456789";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(Constants.PreferenceCodes.ProvisionalPreference1, testInvLine.JI_PrimaryPreference);

			var pT2Rate = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferencePT2.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(pT2Rate, tradeGroup, date1, date2, "add61", "ord61");
			Factory.Save();

			testInvLine.JI_PrimaryPreference = ZString.Empty;
			testInvLine.JI_Tariff = ZString.Empty;
			testInvLine.JI_CountryOfOrigin = ZString.Empty;
			testInvLine.JI_Tariff = "123456789";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(Constants.PreferenceCodes.ProvisionalPreference2, testInvLine.JI_PrimaryPreference);

			universalReferenceTestDataHelper.CreateTariff("TW", hsnTariffType.PK, "200000001", date1, date2, "dummy Description 1");
			Factory.Save();

			testInvLine.JI_PrimaryPreference = ZString.Empty;
			testInvLine.JI_Tariff = ZString.Empty;
			testInvLine.JI_CountryOfOrigin = ZString.Empty;
			testInvLine.JI_Tariff = "200000001";
			testInvLine.JI_CountryOfOrigin = "AU";
			AssertEquals(ZString.Empty, testInvLine.JI_PrimaryPreference);
		}

		public void TestDefaultOrderNumber()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader1 = jobDeclaration.Invoices.AddNew();
			invoiceHeader1.JZ_OrderNumber = "1111";
			var invoiceHeader2 = jobDeclaration.Invoices.AddNew();
			invoiceHeader2.JZ_OrderNumber = "2222";
			var invoiceHeader3 = jobDeclaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader1.InvoiceLines.AddNew();
			AssertEquals("1111", invoiceLine.JI_OrderNumber);

			invoiceLine.JI_JZ = invoiceHeader2.PK;
			AssertEquals("2222", invoiceLine.JI_OrderNumber);

			invoiceLine.JI_JZ = invoiceHeader3.PK;
			AssertNullOrEmpty(invoiceLine.JI_OrderNumber);
		}

		public void TestDefaultOrderNumberWhenOrderLineIsNotNull()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader1 = jobDeclaration.Invoices.AddNew();
			var invoiceHeader2 = jobDeclaration.Invoices.AddNew();
			invoiceHeader2.JZ_OrderNumber = "2222";

			var invoiceLine = invoiceHeader1.InvoiceLines.AddNew();
			AssertNullOrEmpty(invoiceLine.JI_OrderNumber);

			var organization = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order = Factory.New<Order>();
			order.BuyerPK = organization.PK;
			order.SupplierPK = organization.PK;
			order.JD_OA_BuyerAddress = organization.MainAddress.PK;

			invoiceLine.JI_OrderNumber = ZString.Empty;
			var orderLine = order.OrderLines.AddNew();

			invoiceLine.JI_JO = orderLine.PK;
			invoiceLine.JI_JZ = invoiceHeader2.PK;
			Factory.Save();

			var filter = new ZQuery(JobComInvoiceLineSchema.PK, invoiceLine.PK).AddToFilter(JobComInvoiceLineSchema.JI_OrderNumber, ZString.Empty);
			var jobComInvoiceLines = Factory.Load<JobComInvoiceLine>(filter);
			Assert("jobComInvoiceLine should be loaded ", jobComInvoiceLines.Any());
		}

		public void TestCustomsQtyCalculated2WhenJI_CustomsSecondUnitQtyChanged()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				var refPacks = Factory.New<CusRefPacks>();
				refPacks.RP_ConversionFactor = 99m;
				refPacks.RP_CustomsPack = "MTK";
				refPacks.RP_CommercialPack = "MTR";

				Assert(InvoiceLine.ShouldConverCustomsQuantity2);

				InvoiceLine.JI_Tariff = "0001.01.01 1";
				InvoiceLine.JI_InvoiceUQ = "MTR";
				InvoiceLine.JI_CustomsSecondUnitQty = "MTR";
				AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsSecondQuantity);

				InvoiceLine.JI_InvoiceQuantity = 1m;
				AssertEquals(1m, InvoiceLine.JI_CustomsSecondQuantity);

				InvoiceLine.JI_CustomsSecondUnitQty = "MTK";
				AssertEquals(99m, InvoiceLine.JI_CustomsSecondQuantity);
			}
		}

		public void TestInvoiceQuantityAndUnitQtyResult()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "ADT";
			invoiceLine.JI_InvoiceQuantity = 1m;
			var collection = invoiceLine.InvoiceQuantityAndUnitQtyResultCollection;
			var adtInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "ADT");
			var amhInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "AMH");

			AssertEquals(1, collection.Count);
			AssertNotNull(adtInvoiceQuantityAndUnitQtyResult);
			AssertNull(amhInvoiceQuantityAndUnitQtyResult);
			AssertEquals(1m, adtInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "ADT";
			invoiceLine.JI_InvoiceQuantity = 2m;
			collection = invoiceLine.InvoiceQuantityAndUnitQtyResultCollection;
			adtInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "ADT");
			amhInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "AMH");

			AssertEquals(1, collection.Count);
			AssertNotNull(adtInvoiceQuantityAndUnitQtyResult);
			AssertNull(amhInvoiceQuantityAndUnitQtyResult);
			AssertEquals(3m, adtInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);

			invoiceLine.JI_InvoiceUQ = "AMH";
			adtInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "ADT");
			amhInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "AMH");

			AssertEquals(2, collection.Count);
			AssertNotNull(adtInvoiceQuantityAndUnitQtyResult);
			AssertNotNull(amhInvoiceQuantityAndUnitQtyResult);
			AssertEquals(1m, adtInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);
			AssertEquals(2m, amhInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "AMH";
			invoiceLine.JI_InvoiceQuantity = 2m;
			collection = invoiceLine.InvoiceQuantityAndUnitQtyResultCollection;
			adtInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "ADT");
			amhInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "AMH");

			AssertEquals(2, collection.Count);
			AssertNotNull(adtInvoiceQuantityAndUnitQtyResult);
			AssertNotNull(amhInvoiceQuantityAndUnitQtyResult);
			AssertEquals(1m, adtInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);
			AssertEquals(4m, amhInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			adtInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "ADT");
			amhInvoiceQuantityAndUnitQtyResult = collection.Cast<InvoiceQuantityAndUnitQtyResult>().FirstOrDefault(c => c.InvoiceUQ == "AMH");

			AssertEquals(1, collection.Count);
			AssertNull(adtInvoiceQuantityAndUnitQtyResult);
			AssertNotNull(amhInvoiceQuantityAndUnitQtyResult);
			AssertEquals(2m, amhInvoiceQuantityAndUnitQtyResult.InvoiceQuantity);

			Factory.Save();
			Assert(!declaration.HasChanges);
		}

		public void TestAssignCMHeaderToInvoices()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader1 = controllingMessageHeaders.AddNew();
			var controllingMessageHeader2 = controllingMessageHeaders.AddNew();

			var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine1.AssignCMHeaderToInvoices(controllingMessageHeader1);
			Assert(invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader1.PK).IsLinkedCMHeader);
			Assert(!invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader2.PK).IsLinkedCMHeader);

			invoiceLine1.AssignCMHeaderToInvoices(controllingMessageHeader2);
			Assert(invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader2.PK).IsLinkedCMHeader);
		}

		public void TestUnitPrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_EnteredUnitPrice", true, attrib => attrib.DecimalPlaces == 6);

			invoiceLine.JI_LinePrice = 10M;
			invoiceLine.JI_InvoiceQuantity = 3M;
			AssertEquals(3.333333M, invoiceLine.JI_EnteredUnitPrice);

			invoiceLine.JI_InvoiceQuantity = 4M;
			AssertEquals(3.333333M, invoiceLine.JI_EnteredUnitPrice);

			invoiceLine.JI_EnteredUnitPrice = 0;
			invoiceLine.JI_InvoiceQuantity = 5M;
			AssertEquals(2.666M, invoiceLine.JI_EnteredUnitPrice);
		}

		public void TestUpdateUnitPriceUnlessEnteredUnitPriceIsZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_EnteredUnitPrice", true, attrib => attrib.DecimalPlaces == 6);

			invoiceLine.JI_InvoiceQuantity = 550M;
			invoiceLine.JI_EnteredUnitPrice = 105.4545M;
			AssertEquals(57999.98M, invoiceLine.JI_LinePrice);

			invoiceLine.JI_LinePrice = 58000M;
			AssertEquals(105.4545M, invoiceLine.JI_EnteredUnitPrice);
		}

		public void TestCalculateLinePrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_EnteredUnitPrice = 10M;
			invoiceLine.JI_InvoiceQuantity = 3M;
			AssertEquals(30M, invoiceLine.JI_LinePrice);

			invoiceLine.JI_InvoiceQuantity = 4M;
			AssertEquals(40M, invoiceLine.JI_LinePrice);

			invoiceLine.JI_LinePrice = ZDecimal.Zero;
			invoiceLine.JI_InvoiceQuantity = 5M;
			AssertEquals(50M, invoiceLine.JI_LinePrice);

			invoiceLine.JI_LinePrice = ZDecimal.Zero;
			invoiceLine.JI_EnteredUnitPrice = 20M;
			AssertEquals(100M, invoiceLine.JI_LinePrice);

			invoiceLine.JI_EnteredUnitPrice = 30M;
			AssertEquals(150M, invoiceLine.JI_LinePrice);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 123.123m;
			invoiceLine2.JI_EnteredUnitPrice = 333.333m;
			AssertEquals(41040.96m, invoiceLine2.JI_LinePrice);

			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 123.123m;
			invoiceLine3.JI_EnteredUnitPrice = 333.333m;
			AssertEquals(41040.96m, invoiceLine3.JI_LinePrice);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 123.123m;
			invoiceLine4.JI_EnteredUnitPrice = 333.333m;
			AssertEquals(41040.96m, invoiceLine4.JI_LinePrice);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 123.123m;
			invoiceLine5.JI_EnteredUnitPrice = 333.333m;
			AssertEquals(41040.96m, invoiceLine5.JI_LinePrice);
		}

		public void TestCloneNAddInfoWhenAddInfoIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_AddInfo = "TariffAdditionalCode=1234";
			invoiceLine.JI_NAddInfo = "Compositions=群組*Group=組成";

			CombineAssertions(() =>
			{
				var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
				var clonedLine = clonedDec.InvoiceLines[0];
				AssertEquals("TemplateCopy JI_AddInfo", "TariffAdditionalCode=1234", clonedLine.JI_AddInfo);
				AssertEquals("TemplateCopy JI_NAddInfo", "Compositions=群組*Group=組成", clonedLine.JI_NAddInfo);

				var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy, Factory).Clone();
				AssertEquals("CountryToCountryCopy JI_AddInfo", "", clonedDec2.InvoiceLines[0].JI_AddInfo);
				AssertEquals("CountryToCountryCopy JI_NAddInfo", "", clonedDec2.InvoiceLines[0].JI_NAddInfo);
			});
		}

		public void TestCalculateCustomsSecondQuantityWhenClothMeterials()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			var tariffType = helper.CreateTariffType("TW", "TXX");
			Factory.Save();

			var tariff0000000021 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff0000000021.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff0000000021.PK, "CU2", "MTK");

			var tariff1000000021 = helper.CreateTariff("TW", tariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1000000021.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff1000000021.PK, "CU2", "MTO");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000021";
			AssertEquals("MTK", invoiceLine.JI_CustomsSecondUnitQty);

			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "MTR";
			invoiceLine.JI_TextileWidth = 1;
			invoiceLine.JI_TextileWidthUQ = "M";
			AssertEquals(1m, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_InvoiceQuantity = 2;
			AssertEquals(2m, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_TextileWidth = 300;
			AssertEquals(600m, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_TextileWidthUQ = "FT";
			AssertEquals(182.88m, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_InvoiceUQ = "CMT";
			AssertEquals(1.8288m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestUniversalCopyInTW()
		{
			var entityNode = new EntityCopyTemplateNode();
			foreach (var name in new string[] { "JI_JZ", "PreviousPermitNo", "ExemptionCode", "TypeApprovalPartyIdentifier", "TypeApprovalAuthorizedParty", "TypeApprovalCertificateNo", "PreviousBondedEntryNumber", "PreviousBondedEntryLineNumber", "PartyIdentifier", "AuthorizedPerson", "CertificateOfOriginNumber", "CertificateOfOriginNumberItemNumber", "CitesPermit", "HighTechLicense", "DeclarationGoodsDescription", "QuotaPermitNumber", "QuotaPermitNumberItemNumber", "TrademarkStorageDocsGuid" })
			{
				entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = name, CopyMethod = CopyMethod.Copy });
			}
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "InvoiceLineRelatedControllingMsgHeadersGenPivots", CopyMethod = CopyMethod.Copy, CustomCopyTemplateNode = "GetMsgHeadersGenPivotsCopyTemplateNode" });
			var innerNode = new EntityCopyTemplateNode { Name = "PermitCusSupportingCollection" };
			innerNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "CSI_ReferenceNumber", CopyMethod = CopyMethod.Copy });
			innerNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "CSI_LineNo", CopyMethod = CopyMethod.Copy });
			var collectionNode = new CollectionCopyTemplateNode { InnerNode = innerNode, Name = "PermitCusSupportingCollection", CopyMethod = CollectionCopyMethod.All, ItemPropertyName = "CSI_ParentID" };
			entityNode.Nodes.Add(collectionNode);

			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var declaration = controllingMsgHeaderHelper.New(new string[] { "DN", "IF", "DH" });
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().ForEach(x => x.IsLinkedCMHeader = true);
			line.TrademarkStorageDocsGuid = ZGuid.NewZGuid();
			line.PreviousPermitNo = "PPN";
			line.ExemptionCode = "E";
			line.TypeApprovalPartyIdentifier = "TP1";
			line.TypeApprovalAuthorizedParty = "TP2";
			line.TypeApprovalCertificateNo = "TACN";
			line.PreviousBondedEntryNumber = "PBEN";
			line.PreviousBondedEntryLineNumber = 1;
			line.PartyIdentifier = "PI";
			line.AuthorizedPerson = "AP";
			line.CertificateOfOriginNumber = "CON";
			line.CertificateOfOriginNumberItemNumber = 2;
			line.CitesPermit = "CP";
			line.HighTechLicense = "HTL";
			line.QuotaPermitNumber = "QPN";
			line.QuotaPermitNumberItemNumber = 3;
			line.JI_NDescription = "AA测试test123";
			AssertEquals("AA测试test123", line.JI_DeclarationGoodsDescription);
			AssertEquals("", line.DeclarationGoodsDescription);
			var permit = line.PermitCusSupportingCollection.AddNew();
			permit.CSI_ReferenceNumber = "PN1";
			permit.CSI_LineNo = 1;
			permit = line.PermitCusSupportingCollection.AddNew();
			permit.CSI_ReferenceNumber = "PN2";
			permit.CSI_LineNo = 2;

			var copyline = (JobComInvoiceLine)new BusinessObjectCopyManager().Copy(line, copyTree).Object;
			AssertEquals("PPN", copyline.PreviousPermitNo);
			AssertEquals("E", copyline.ExemptionCode);
			AssertEquals("TP1", copyline.TypeApprovalPartyIdentifier);
			AssertEquals("TP2", copyline.TypeApprovalAuthorizedParty);
			AssertEquals("TACN", copyline.TypeApprovalCertificateNo);
			AssertEquals("PBEN", copyline.PreviousBondedEntryNumber);
			AssertEquals(new ZShort(1), copyline.PreviousBondedEntryLineNumber);
			AssertEquals("PI", copyline.PartyIdentifier);
			AssertEquals("AP", copyline.AuthorizedPerson);
			AssertEquals("CON", copyline.CertificateOfOriginNumber);
			AssertEquals(new ZShort(2), copyline.CertificateOfOriginNumberItemNumber);
			AssertEquals("CP", copyline.CitesPermit);
			AssertEquals("HTL", copyline.HighTechLicense);
			AssertEquals("QPN", copyline.QuotaPermitNumber);
			AssertEquals(new ZShort(3), copyline.QuotaPermitNumberItemNumber);
			AssertEquals("", copyline.JI_DeclarationGoodsDescription);
			AssertEquals(line.TrademarkStorageDocsGuid, copyline.TrademarkStorageDocsGuid);

			var permitCusSupportingCollection = copyline.PermitCusSupportingCollection.Cast<PermitCusSupporting>();
			AssertEquals(2, permitCusSupportingCollection.Count());
			Assert(permitCusSupportingCollection.Any(x => x.CSI_ReferenceNumber == "PN1" && x.CSI_LineNo == new ZShort(1) && x.CSI_Type == CusSupportingInfoTypeList.Codes.PermitNumber && x.CSI_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix));
			Assert(permitCusSupportingCollection.Any(x => x.CSI_ReferenceNumber == "PN2" && x.CSI_LineNo == new ZShort(2) && x.CSI_Type == CusSupportingInfoTypeList.Codes.PermitNumber && x.CSI_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix));

			var invoiceLineLinkControllingMsgHeaders = copyline.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>();
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count());
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count(x => x.IsLinkedCMHeader));
			line.JI_NDescription = "";
			line.JI_DeclarationGoodsDescription = "AA测试test1234";
			copyline = (JobComInvoiceLine)new BusinessObjectCopyManager().Copy(line, copyTree).Object;
			AssertEquals("AA测试test1234", copyline.JI_DeclarationGoodsDescription);
			AssertEquals("AA测试test1234", copyline.DeclarationGoodsDescription);

			line.DeclarationGoodsDescription = "AA测试test123";
			copyline = (JobComInvoiceLine)new BusinessObjectCopyManager().Copy(line, copyTree).Object;
			AssertEquals("AA测试test123", copyline.JI_DeclarationGoodsDescription);
			AssertEquals("AA测试test123", copyline.DeclarationGoodsDescription);
		}

		public void TestFormattedAdValoremDutyRate()
		{
			var hsnTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = UniversalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var rateCodeDTS = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = UniversalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = UniversalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = UniversalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = UniversalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffBoth = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBothRateDTS = UniversalTestHelper.CreateRate(tariffBoth, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(tariffBothRateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffBothRateDTA = UniversalTestHelper.CreateRate(tariffBoth, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(tariffBothRateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "21039090200";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals("15%", invoiceLine.FormattedAdValoremDutyRate);

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.CashPayment;
			AssertEquals("15%", invoiceLine.FormattedAdValoremDutyRate);

			invoiceLine.JI_Tariff = "21039090201";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals(ZString.Empty, invoiceLine.FormattedAdValoremDutyRate);

			invoiceLine.JI_Tariff = "21039090202";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals("15%", invoiceLine.FormattedAdValoremDutyRate);

			invoiceLine.JI_CustomsUnitQty = "LTR";
			invoiceLine.JI_CustomsQuantity = 1m;
			AssertEquals("15%", invoiceLine.FormattedAdValoremDutyRate);

			invoiceLine.JI_DtyPymntMthd = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
			invoiceLine.JI_LinePrice = 1001m;
			AssertEquals("15%", invoiceLine.FormattedAdValoremDutyRate);

			invoiceLine.JI_DtyPymntMthd = ZString.Empty;
			AssertEquals("15%", invoiceLine.FormattedAdValoremDutyRate);
		}

		public void TestFormattedSpecificDutyRate()
		{
			var hsnTariffType = UniversalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = UniversalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var rateCodeDTS = UniversalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = UniversalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = UniversalTestHelper.CreateTradeGroup("TW", "US", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			UniversalTestHelper.AddCountry(tradeGroup, "US", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = UniversalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = UniversalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariff21039090202 = UniversalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate21039090202 = UniversalTestHelper.CreateRate(tariff21039090202, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.16 * VFD", preference.PK, "0.16", Core.Constants.CountryCodes.Taiwan);
			UniversalTestHelper.CreateCusApplicability(rate21039090202, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "21039090200";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals(ZString.Empty, invoiceLine.FormattedSpecificDutyRate);

			invoiceLine.JI_Tariff = "21039090202";
			AssertEquals("16%", invoiceLine.FormattedSpecificDutyRate);

			invoiceLine.JI_PrimaryPreference = "STD";
			AssertEquals(ZString.Empty, invoiceLine.FormattedSpecificDutyRate);

			invoiceLine.JI_Tariff = "21039090201";
			invoiceLine.JI_PrimaryPreference = "PR1";
			AssertEquals("150/LTR", invoiceLine.FormattedSpecificDutyRate);
		}

		public void TestSetDefaultValuesForNewPackableItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.JI_Description = "desc line 1";
			line1.AddInfoChild.TWL_DocumentaryQty = 2;
			line1.AddInfoChild.TWL_DocumentaryUQ = "BAG";
			line1.JI_DeclarationGoodsDescription = "測試1";
			line1.JI_Group = "group1";
			var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line2.JI_Description = "desc line 2";
			line2.AddInfoChild.TWL_DocumentaryQty = 3;
			line2.AddInfoChild.TWL_DocumentaryUQ = "BBG";
			line2.JI_DeclarationGoodsDescription = "ABC" + ZString.Replicate('Z', 4010);

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var package = packingList.PackageJob.Packages.AddNew();
			var packableItemRelataions = package.PackableItemRelataions;
			var items = packingList.PackableItems;
			CombineAssertions(() =>
			{
				AssertEquals(2, items.Count);
				var item = items.First();
				AssertEquals("測試1", item.CUI_GoodsDescription);
				AssertEquals(2m, item.CUI_PackableQty);
				AssertEquals("BAG", item.CUI_PackableUQ);
				AssertEquals("group1", item.Grouping);
				item = items.ElementAt(1);
				AssertEquals("ABC" + ZString.Replicate('Z', 3997), item.CUI_GoodsDescription);
				AssertEquals(3m, item.CUI_PackableQty);
				AssertEquals("BBG", item.CUI_PackableUQ);
				AssertEquals(ZString.Empty, item.Grouping);
			});
		}

		public void TestF5FTZDestinationCountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff_87120010109 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87120010109", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination, "AT;BE;BG;CY;CZ;DE;DK;EE;ES;FI;FR", tariff_87120010109);

			var tariff_87120010110 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87120010110", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination, "GR;HR;HU;HU;IE", tariff_87120010110);

			var tariff_87120010111 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87120010111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination, "AT;HR; HU;IE ", tariff_87120010111);

			var tariff_87120010112 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87120010112", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				InvoiceLine.JI_Tariff = tariff_87120010109.ZZ1_TariffCode;
				AssertContainsExactElementsInAnyOrder(new[] { "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "ES", "FI", "FR" }, InvoiceLine.F5FTZDestinationCountryCodes);

				InvoiceLine.JI_Tariff = tariff_87120010110.ZZ1_TariffCode;
				AssertContainsExactElementsInAnyOrder(new[] { "GR", "HR", "HU", "IE" }, InvoiceLine.F5FTZDestinationCountryCodes);

				InvoiceLine.JI_Tariff = tariff_87120010111.ZZ1_TariffCode;
				AssertContainsExactElementsInAnyOrder(new[] { "AT", "HR", "HU", "IE" }, InvoiceLine.F5FTZDestinationCountryCodes);

				InvoiceLine.JI_Tariff = tariff_87120010112.ZZ1_TariffCode;
				AssertEquals("Should be empty collection", 0, InvoiceLine.F5FTZDestinationCountryCodes.Count);
			});
		}

		public void TestImportExportRegulations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff_02089029204 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029204", minDate, maxDate);
			var tariff_02089029204_Attribute_IMP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_111.ZZ3_Value, "管制輸入。", minDate, maxDate);

			var tariff_02089029204_Attribute_EXP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_02089029204_Attribute_EXP_111.ZZ3_Value, "管制輸出。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_F01 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "F01", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_F01.ZZ3_Value, "輸入商品應依照「食品及相關產品輸入查驗辦法」規定，向衛生福利部食品藥物管理署申請辦理輸入查驗。【註：相關規定應洽衛生福利部食品藥物管理署】。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_B01 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "B01", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_B01.ZZ3_Value, "進口時，應依行政院農業委員會動植物防疫檢疫局編訂之「應施檢疫動植物品目表」及有關檢疫規定辦理。【註：相關規定請洽行政院農業委員會動植物防疫檢疫局或至該局網站http://www.baphiq.gov.tw查詢】。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_MW0 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "MW0", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_MW0.ZZ3_Value, "大陸物品不准輸入。", minDate, maxDate);

			var tariff_62032920006 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "62032920006", minDate, maxDate);
			var tariff_62032920006_Attribute_IMP_MP1 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "MP1", tariff_62032920006);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_62032920006_Attribute_IMP_MP1.ZZ3_Value, "（一）大陸物品有條件准許輸入，應符合「大陸物品有條件准許輸入項目、輸入管理法規彙總表」之規定。（二）「大陸物品有條件准許輸入項目、輸入管理法規彙總表」內列有特別規定「ＭＸＸ」代號者，應向國際貿易局辦理輸入許可證；未列有特別規定「ＭＸＸ」代號者，依一般簽證規定辦理。", minDate, maxDate);
			Factory.Save();

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = tariff_02089029204.ZZ1_TariffCode;
			AssertEquals(@"111
管制輸入。

B01
進口時，應依行政院農業委員會動植物防疫檢疫局編訂之「應施檢疫動植物品目表」及有關檢疫規定辦理。【註：相關規定請洽行政院農業委員會動植物防疫檢疫局或至該局網站http://www.baphiq.gov.tw查詢】。

F01
輸入商品應依照「食品及相關產品輸入查驗辦法」規定，向衛生福利部食品藥物管理署申請辦理輸入查驗。【註：相關規定應洽衛生福利部食品藥物管理署】。

MW0
大陸物品不准輸入。", InvoiceLine.ImportExportRegulations);

			InvoiceLine.JI_Tariff = tariff_62032920006.ZZ1_TariffCode;
			AssertEquals(@"MP1
（一）大陸物品有條件准許輸入，應符合「大陸物品有條件准許輸入項目、輸入管理法規彙總表」之規定。（二）「大陸物品有條件准許輸入項目、輸入管理法規彙總表」內列有特別規定「ＭＸＸ」代號者，應向國際貿易局辦理輸入許可證；未列有特別規定「ＭＸＸ」代號者，依一般簽證規定辦理。", InvoiceLine.ImportExportRegulations);

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = tariff_02089029204.ZZ1_TariffCode;
			AssertEquals(@"111
管制輸出。", InvoiceLine.ImportExportRegulations);
		}

		public void TestGetImportExportRegulationCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff_02089029204 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "02089029204", minDate, maxDate);
			var tariff_02089029204_Attribute_IMP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_111.ZZ3_Value, "管制輸入。", minDate, maxDate);

			var tariff_02089029204_Attribute_EXP_111 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations, "111", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations, tariff_02089029204_Attribute_EXP_111.ZZ3_Value, "管制輸出。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_F01 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "F01", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_F01.ZZ3_Value, "輸入商品應依照「食品及相關產品輸入查驗辦法」規定，向衛生福利部食品藥物管理署申請辦理輸入查驗。【註：相關規定應洽衛生福利部食品藥物管理署】。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_B01 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "B01", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_B01.ZZ3_Value, "進口時，應依行政院農業委員會動植物防疫檢疫局編訂之「應施檢疫動植物品目表」及有關檢疫規定辦理。【註：相關規定請洽行政院農業委員會動植物防疫檢疫局或至該局網站http://www.baphiq.gov.tw查詢】。", minDate, maxDate);

			var tariff_02089029204_Attribute_IMP_MW0 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "MW0", tariff_02089029204);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_02089029204_Attribute_IMP_MW0.ZZ3_Value, "大陸物品不准輸入。", minDate, maxDate);

			var tariff_62032920006 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "62032920006", minDate, maxDate);
			var tariff_62032920006_Attribute_IMP_MP1 = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations, "MP1", tariff_62032920006);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations, tariff_62032920006_Attribute_IMP_MP1.ZZ3_Value, "（一）大陸物品有條件准許輸入，應符合「大陸物品有條件准許輸入項目、輸入管理法規彙總表」之規定。（二）「大陸物品有條件准許輸入項目、輸入管理法規彙總表」內列有特別規定「ＭＸＸ」代號者，應向國際貿易局辦理輸入許可證；未列有特別規定「ＭＸＸ」代號者，依一般簽證規定辦理。", minDate, maxDate);
			Factory.Save();

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = tariff_02089029204.ZZ1_TariffCode;
			AssertContainsExactElementsInExactOrder(new[] { "111", "B01", "F01", "MW0" }, InvoiceLine.GetImportExportRegulationCodes());

			InvoiceLine.JI_Tariff = tariff_62032920006.ZZ1_TariffCode;
			AssertEquals("MP1", InvoiceLine.GetImportExportRegulationCodes().FirstOrDefault());

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = tariff_02089029204.ZZ1_TariffCode;
			AssertEquals("111", InvoiceLine.GetImportExportRegulationCodes().FirstOrDefault());
		}

		public void TestCustomsRegulations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff_05071011003 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "05071011003", minDate, maxDate);
			var tariff_05071011003_Attribute_LPartially = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, Constants.UniversalReferenceConstants.CusTariffAttributeValue.LPartially, tariff_05071011003);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWCustomsRequirements, tariff_05071011003_Attribute_LPartially.ZZ3_Value, "部份進口應課徵特種貨物及勞務稅", minDate, maxDate);

			var tariff_05071011003_Attribute_R = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "R", tariff_05071011003);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWCustomsRequirements, tariff_05071011003_Attribute_R.ZZ3_Value, "取消退稅", minDate, maxDate);

			var tariff_87162000005 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "87162000005", minDate, maxDate);
			var tariff_87162000005_Attribute_TPartially = helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, Constants.UniversalReferenceConstants.CusTariffAttributeValue.TPartially, tariff_87162000005);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWCustomsRequirements, tariff_87162000005_Attribute_TPartially.ZZ3_Value, "部份進口應課徵貨物稅", minDate, maxDate);
			Factory.Save();

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = tariff_05071011003.ZZ1_TariffCode;
			AssertEquals(@"L*
部份進口應課徵特種貨物及勞務稅

R
取消退稅", InvoiceLine.CustomsRegulations);

			InvoiceLine.JI_Tariff = tariff_87162000005.ZZ1_TariffCode;
			AssertEquals(@"T*
部份進口應課徵貨物稅", InvoiceLine.CustomsRegulations);
		}

		public void TestDeleteControllingMessageHeaderLinkInvoiceLineOnDelete()
		{
			var entryInstruction = Declaration.CusEntryInstruction;
			var line1 = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			var testCollection1 = messageHeader1.ControllingMessageHeaderLinkInvoiceLines;
			var testCollection2 = messageHeader2.ControllingMessageHeaderLinkInvoiceLines;
			AssertEquals(2, testCollection1.Count);
			AssertEquals(2, testCollection2.Count);

			line2.Delete();
			AssertEquals(1, testCollection1.Count);
			AssertEquals(1, testCollection2.Count);
		}

		public void TestShouldRefreshControllingMessageHeaderLinkInvoiceLinesWhenInvoiceLineDeleted()
		{
			var entryInstruction = Declaration.CusEntryInstruction;
			var line1 = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			InvoiceHeader.InvoiceLines.AddNew();
			entryInstruction.ControllingMessageHeaders.AddNew();
			entryInstruction.ControllingMessageHeaders.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDeclaration = newFactory.Load<JobDeclaration>(Declaration.PK);
			var newEntryInstruction = newDeclaration.CusEntryInstruction;
			var newMessageHeader1 = newEntryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().First();
			var newMessageHeader2 = newEntryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().ElementAt(1);
			var newCollection1 = newMessageHeader1.ControllingMessageHeaderLinkInvoiceLines;
			var newCollection2 = newMessageHeader2.ControllingMessageHeaderLinkInvoiceLines;
			newFactory.Save();
			AssertEquals(2, newCollection1.Count);
			AssertEquals(2, newCollection2.Count);

			line1.Delete();
			Factory.Save();
			AssertEquals(1, newCollection1.Count);
			AssertEquals(1, newCollection2.Count);
		}

		public void TestCurrencyConverter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertType<RefCurrencyCurrencyConverter>(invoiceLine.CurrencyConverter);
			invoiceLine.JI_JZ = invoiceHeader.PK;
			AssertType<CurrencyConverterWithFixedExchangeRatesDataProvider>(invoiceLine.CurrencyConverter);
		}

		public void TestInvoiceHeaderCurrencyConverterDataProviderWhenInvoiceDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			ICurrencyConverterDataProvider iCurrencyConverterDataProvider = invoiceLine;
			GetRateType();
			invoiceLine.JI_JZ = invoiceHeader2.PK;
			invoiceHeader1.Delete();
			AssertNoExceptionThrown(() => GetRateType());
			ExchangeRateType GetRateType() => iCurrencyConverterDataProvider.RateType;
		}

		public void TestJI_CustomsUnitQty_Caption()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(line.JI_CustomsUnitQtyInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Statistical Weight Unit", resourceStringData.Caption);
				AssertEquals("Short Caption", "UQ", resourceStringData.ShortCaption);
			});
		}

		public void TestJI_CustomsSecondUnitQty_Caption()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(line.JI_CustomsSecondUnitQtyInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Statistical Quantity Unit", resourceStringData.Caption);
				AssertEquals("Short Caption", "UQ", resourceStringData.ShortCaption);
				AssertEquals("Full Description", "Statistical Quantity Unit as indicated by the tariff item entered.", resourceStringData.FullDescription);
			});
		}

		public void TestRebuildInvoiceQuantityAndUnitQtyResultOnDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines = declaration.InvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			CombineAssertions(() =>
			{
				AssertEquals(100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});

			var invoiceLine2 = invoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 100;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", 200m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 1", "PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
				AssertEquals("Line 2", 200m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 2", "PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});

			invoiceLine2.Delete();
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", 100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 1", "PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});
		}

		public void TestRebuildInvoiceQuantityAndUnitQtyResultWhenJI_InvoiceQuantityChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines = declaration.InvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			CombineAssertions(() =>
			{
				AssertEquals(100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});

			var invoiceLine2 = invoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 100;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", 200m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 1", "PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
				AssertEquals("Line 2", 200m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 2", "PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});

			invoiceLine1.JI_InvoiceQuantity = 200;
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", 300m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 1", "PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
				AssertEquals("Line 2", 300m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 2", "PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});
		}

		public void TestRebuildInvoiceQuantityAndUnitQtyResultWhenJI_InvoiceUQChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines = declaration.InvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			CombineAssertions(() =>
			{
				AssertEquals(100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});

			var invoiceLine2 = invoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 100;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", 200m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 1", "PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
				AssertEquals("Line 2", 200m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 2", "PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});

			invoiceLine1.JI_InvoiceUQ = "PLT";
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", 2, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection.Count);
				AssertEquals("Line 1", 100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 1", "PLT", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
				AssertEquals("Line 1", 100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[1].InvoiceQuantity);
				AssertEquals("Line 1", "PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[1].InvoiceUQ);
				AssertEquals("Line 2", 2, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection.Count);
				AssertEquals("Line 2", 100m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("Line 2", "PLT", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
				AssertEquals("Line 2", 100m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[1].InvoiceQuantity);
				AssertEquals("Line 2", "PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[1].InvoiceUQ);
			});
		}

		public void TestRebuildInvoiceQuantityAndUnitQtyResultWhenJI_JZChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			AssertEquals(100m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
			AssertEquals("PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 200;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			AssertEquals(200m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
			AssertEquals("PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);

			invoiceLine1.JI_JZ = invoiceHeader2.PK;
			AssertEquals(300m, invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
			AssertEquals("PCE", invoiceLine1.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			AssertEquals(300m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
			AssertEquals("PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
		}

		public void TestSetDefaultProcedureByRelatedIndicatorIfRequired()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicatorNoEffect;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			AssertEquals(string.Empty, invoiceLine1.JI_Procedure);

			invoice.JZ_RelatedIndicator = RelationshipIndicatorList.Codes.RelationshipIndicator138;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals(Constants.ProcedureCodes._65, invoiceLine2.JI_Procedure);
		}

		public void TestPermitCusSupportingNo1()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.PermitCusSupportingNo1);
				line.PermitCusSupportingNo1 = "1";
				AssertEquals("1", line.PermitCusSupportingNo1);
			});
		}

		public void TestPermitCusSupportingLineNo1()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZInt.Zero, line.PermitCusSupportingLineNo1);
				line.PermitCusSupportingLineNo1 = 1;
				AssertEquals(1, line.PermitCusSupportingLineNo1);
			});
		}

		public void TestPermitCusSupportingNo2()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingNo2 = "1";

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.PermitCusSupportingNo2);
				line.PermitCusSupportingNo2 = "2";
				AssertEquals("2", line.PermitCusSupportingNo2);
			});
		}

		public void TestPermitCusSupportingLineNo2()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingLineNo2 = 1;

			CombineAssertions(() =>
			{
				AssertEquals(ZInt.Zero, line.PermitCusSupportingLineNo2);
				line.PermitCusSupportingLineNo2 = 2;
				AssertEquals(2, line.PermitCusSupportingLineNo2);
			});
		}

		public void TestPermitCusSupportingNo3()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingNo3 = "1";
			line.PermitCusSupportingNo3 = "2";

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.PermitCusSupportingNo3);
				line.PermitCusSupportingNo3 = "3";
				AssertEquals("3", line.PermitCusSupportingNo3);
			});
		}

		public void TestPermitCusSupportingLineNo3()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingLineNo3 = 1;
			line.PermitCusSupportingLineNo3 = 2;

			CombineAssertions(() =>
			{
				AssertEquals(ZInt.Zero, line.PermitCusSupportingLineNo3);
				line.PermitCusSupportingLineNo3 = 3;
				AssertEquals(3, line.PermitCusSupportingLineNo3);
			});
		}

		public void TestPermitCusSupportingNo4()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingNo4 = "1";
			line.PermitCusSupportingNo4 = "2";
			line.PermitCusSupportingNo4 = "3";

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.PermitCusSupportingNo4);
				line.PermitCusSupportingNo4 = "4";
				AssertEquals("4", line.PermitCusSupportingNo4);
			});
		}

		public void TestPermitCusSupportingLineNo4()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingLineNo4 = 1;
			line.PermitCusSupportingLineNo4 = 2;
			line.PermitCusSupportingLineNo4 = 3;

			CombineAssertions(() =>
			{
				AssertEquals(ZInt.Zero, line.PermitCusSupportingLineNo4);
				line.PermitCusSupportingLineNo4 = 4;
				AssertEquals(4, line.PermitCusSupportingLineNo4);
			});
		}

		public void TestPermitCusSupportingNo5()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingNo5 = "1";
			line.PermitCusSupportingNo5 = "2";
			line.PermitCusSupportingNo5 = "3";
			line.PermitCusSupportingNo5 = "4";

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.PermitCusSupportingNo5);
				line.PermitCusSupportingNo5 = "5";
				AssertEquals("5", line.PermitCusSupportingNo5);
			});
		}

		public void TestPermitCusSupportingLineNo5()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.PermitCusSupportingLineNo5 = 1;
			line.PermitCusSupportingLineNo5 = 2;
			line.PermitCusSupportingLineNo5 = 3;
			line.PermitCusSupportingLineNo5 = 4;

			CombineAssertions(() =>
			{
				AssertEquals(ZInt.Zero, line.PermitCusSupportingLineNo5);
				line.PermitCusSupportingLineNo5 = 5;
				AssertEquals(5, line.PermitCusSupportingLineNo5);
			});
		}

		public void TestSortedPermitCusSupportingCollection()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var permit1 = line.PermitCusSupportingCollection.AddNew();
			permit1.CSI_ItemNumber = 2;
			var permit2 = line.PermitCusSupportingCollection.AddNew();
			permit2.CSI_ItemNumber = 3;
			var permit3 = line.PermitCusSupportingCollection.AddNew();
			permit3.CSI_ItemNumber = 1;

			AssertContainsExactElementsInExactOrder(new PermitCusSupporting[] { permit3, permit1, permit2 }, line.SortedPermitCusSupportingCollection);
		}

		public void TestPermitCusSupportingMaxLength()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var expectMaxLength = 14;
			CombineAssertions(() =>
			{
				AssertEquals("PermitCusSupportingNo1", expectMaxLength, line.PermitCusSupportingNo1Info.MaxLength);
				AssertEquals("PermitCusSupportingNo2", expectMaxLength, line.PermitCusSupportingNo2Info.MaxLength);
				AssertEquals("PermitCusSupportingNo3", expectMaxLength, line.PermitCusSupportingNo3Info.MaxLength);
				AssertEquals("PermitCusSupportingNo4", expectMaxLength, line.PermitCusSupportingNo4Info.MaxLength);
				AssertEquals("PermitCusSupportingNo5", expectMaxLength, line.PermitCusSupportingNo5Info.MaxLength);
			});
		}

		public void TestAssignedNumber1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.AssignedNumber1Info, "Assigned Number 1", "Assigned No. 1", "AS NO. 1", string.Empty);
			AssertEquals(35, jobComInvoiceLine.AssignedNumber1Info.MaxLength);
			var assignedNumber = jobComInvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber.JG_ReferenceNumber = "XXX";
			CombineAssertions(() =>
			{
				AssertEquals("XXX", jobComInvoiceLine.AssignedNumber1);
				jobComInvoiceLine.AssignedNumber1 = "YYY";
				AssertEquals("YYY", assignedNumber.JG_ReferenceNumber);
			});
		}

		public void TestAssignedNumber2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.AssignedNumber2Info, "Assigned Number 2", "Assigned No. 2", "AS NO. 2", string.Empty);
			AssertEquals(35, jobComInvoiceLine.AssignedNumber2Info.MaxLength);
			_ = jobComInvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			var assignedNumber = jobComInvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber.JG_ReferenceNumber = "XXX";
			CombineAssertions(() =>
			{
				AssertEquals("XXX", jobComInvoiceLine.AssignedNumber2);
				jobComInvoiceLine.AssignedNumber2 = "YYY";
				AssertEquals("YYY", assignedNumber.JG_ReferenceNumber);
			});
		}

		public void TestPermitExemptionCode1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.PermitExemptionCode1Info, "Permit Exemption Code 1", "Exemption Code 1", "Exem. Code 1", string.Empty);
			AssertEquals(100, jobComInvoiceLine.PermitExemptionCode1Info.MaxLength);
			var exemptionOfControllingAgenciesCusSupportings = jobComInvoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			var exemptionOfControllingAgency = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency.CSI_ReferenceNumber = "XX123456789";
			CombineAssertions(() =>
			{
				AssertEquals("XX123456789", jobComInvoiceLine.PermitExemptionCode1);
				jobComInvoiceLine.PermitExemptionCode1 = "YY123456789";
				AssertEquals("YY123456789", exemptionOfControllingAgency.CSI_ReferenceNumber);
			});
		}

		public void TestPermitExemptionCode2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.PermitExemptionCode2Info, "Permit Exemption Code 2", "Exemption Code 2", "Exem. Code 2", string.Empty);
			AssertEquals(100, jobComInvoiceLine.PermitExemptionCode2Info.MaxLength);
			var exemptionOfControllingAgenciesCusSupportings = jobComInvoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			var exemptionOfControllingAgency = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency.CSI_ReferenceNumber = "XX123456789";
			CombineAssertions(() =>
			{
				AssertEquals("XX123456789", jobComInvoiceLine.PermitExemptionCode2);
				jobComInvoiceLine.PermitExemptionCode2 = "YY123456789";
				AssertEquals("YY123456789", exemptionOfControllingAgency.CSI_ReferenceNumber);
			});
		}

		public void TestPermitExemptionCode3()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.PermitExemptionCode3Info, "Permit Exemption Code 3", "Exemption Code 3", "Exem. Code 3", string.Empty);
			AssertEquals(100, jobComInvoiceLine.PermitExemptionCode3Info.MaxLength);
			var exemptionOfControllingAgenciesCusSupportings = jobComInvoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			var exemptionOfControllingAgency = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency.CSI_ReferenceNumber = "XX123456789";
			CombineAssertions(() =>
			{
				AssertEquals("XX123456789", jobComInvoiceLine.PermitExemptionCode3);
				jobComInvoiceLine.PermitExemptionCode3 = "YY123456789";
				AssertEquals("YY123456789", exemptionOfControllingAgency.CSI_ReferenceNumber);
			});
		}

		public void TestPermitExemptionCode4()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.PermitExemptionCode4Info, "Permit Exemption Code 4", "Exemption Code 4", "Exem. Code 4", string.Empty);
			AssertEquals(100, jobComInvoiceLine.PermitExemptionCode4Info.MaxLength);
			var exemptionOfControllingAgenciesCusSupportings = jobComInvoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			var exemptionOfControllingAgency = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency.CSI_ReferenceNumber = "XX123456789";
			CombineAssertions(() =>
			{
				AssertEquals("XX123456789", jobComInvoiceLine.PermitExemptionCode4);
				jobComInvoiceLine.PermitExemptionCode4 = "YY123456789";
				AssertEquals("YY123456789", exemptionOfControllingAgency.CSI_ReferenceNumber);
			});
		}

		public void TestPermitExemptionCode5()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(jobComInvoiceLine.PermitExemptionCode5Info, "Permit Exemption Code 5", "Exemption Code 5", "Exem. Code 5", string.Empty);
			AssertEquals(100, jobComInvoiceLine.PermitExemptionCode5Info.MaxLength);
			var exemptionOfControllingAgenciesCusSupportings = jobComInvoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			_ = exemptionOfControllingAgenciesCusSupportings.AddNew();
			var exemptionOfControllingAgency = exemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency.CSI_ReferenceNumber = "XX123456789";
			CombineAssertions(() =>
			{
				AssertEquals("XX123456789", jobComInvoiceLine.PermitExemptionCode5);
				jobComInvoiceLine.PermitExemptionCode5 = "YY123456789";
				AssertEquals("YY123456789", exemptionOfControllingAgency.CSI_ReferenceNumber);
			});
		}

		public void TestAlcoholTaxCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.AlcoholTaxCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Alcohol Tax (Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax1.JLT_Tariff = "TEST1001";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax2.JLT_Tariff = "TEST1005";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1001", line.AlcoholTaxCashTariffCode);
		}

		public void TestAlcoholTaxNonCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.AlcoholTaxNonCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Alcohol Tax (Non-Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax1.JLT_Tariff = "TEST1001";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
			invoiceLineTax2.JLT_Tariff = "TEST1005";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1005", line.AlcoholTaxNonCashTariffCode);
		}

		public void TestCommodityTaxCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.CommodityTaxCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Commodity Tax (Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			invoiceLineTax1.JLT_Tariff = "TEST1002";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			invoiceLineTax2.JLT_Tariff = "TEST1006";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1002", line.CommodityTaxCashTariffCode);
		}

		public void TestCommodityTaxNonCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.CommodityTaxNonCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Commodity Tax (Non-Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			invoiceLineTax1.JLT_Tariff = "TEST1002";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
			invoiceLineTax2.JLT_Tariff = "TEST1006";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1006", line.CommodityTaxNonCashTariffCode);
		}

		public void TestSpecialTaxCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.SpecialTaxCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Special Tax (Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			invoiceLineTax1.JLT_Tariff = "TEST1003";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			invoiceLineTax2.JLT_Tariff = "TEST1007";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1003", line.SpecialTaxCashTariffCode);
		}

		public void TestSpecialTaxNonCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.SpecialTaxNonCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Special Tax (Non-Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			invoiceLineTax1.JLT_Tariff = "TEST1003";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
			invoiceLineTax2.JLT_Tariff = "TEST1007";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1007", line.SpecialTaxNonCashTariffCode);
		}

		public void TestTobaccoTaxCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.TobaccoTaxCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Tobacco Tax (Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;
			invoiceLineTax1.JLT_Tariff = "TEST1004";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;
			invoiceLineTax2.JLT_Tariff = "TEST1008";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1004", line.TobaccoTaxCashTariffCode);
		}

		public void TestTobaccoTaxNonCashTariffCode()
		{
			CreateInvoiceLineTaxes();
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.TobaccoTaxNonCashTariffCodeInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertEquals("Caption", "Tobacco Tax (Non-Cash)", resStrings.Caption);

			line.JI_Tariff = "87031000002";
			line.Taxes.RemoveAndDeleteAll();
			var invoiceLineTax1 = line.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;
			invoiceLineTax1.JLT_Tariff = "TEST1004";
			invoiceLineTax1.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var invoiceLineTax2 = line.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;
			invoiceLineTax2.JLT_Tariff = "TEST1008";
			invoiceLineTax2.JLT_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			AssertEquals("TEST1008", line.TobaccoTaxNonCashTariffCode);
		}

		public void TestManufacturerDocAddress()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var processorAddress = line.ManufacturerDocAddress;
			processorAddress.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				AssertType<TWJobDocAddressDependentCollection>(line.DocAddresses);
				AssertType<TWJobDocAddress>(processorAddress);
				AssertType<ManufacturerAddressRequirement>(processorAddress.Requirement);
				AssertType<ManufacturerLocalAddressRequirement>(processorAddress.LocalAddress.Requirement);
			});
			processorAddress.OrganisationPK = Organization.PK;
			CombineAssertions(() =>
			{
				AssertEquals("Test Org", processorAddress.E2_CompanyName);
				AssertEquals("MAN", processorAddress.E2_AddressType);
				AssertEquals("JI", processorAddress.E2_ParentTableCode);
			});
		}

		public void TestManufacturerDocAddressOrgPK()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var processorAddress = line.ManufacturerDocAddress;
			processorAddress.OrganisationPK = Organization.PK;
			AssertEquals(Organization.PK, line.ManufacturerDocAddressOrgPK);

			var organization = Factory.New<OrgHeader>();
			organization.FillWithValidTestData();
			organization.OH_FullName = "Test Org 2";
			line.ManufacturerDocAddressOrgPK = organization.PK;
			AssertEquals(organization.PK, processorAddress.OrganisationPK);
		}

		public void TestJI_PermitUnitPriceCurrency()
		{
			CombineAssertions(() =>
			{
				InvoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
				AssertEquals("TWD", InvoiceLine.JI_PermitUnitPriceCurrency);

				InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				AssertEquals("USD", InvoiceLine.JI_PermitUnitPriceCurrency);
			});
		}

		public void TestJI_PermitUnitPriceCurrencyAttributes()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.JI_PermitUnitPriceCurrencyInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			AssertHasCustomAttribute<ListAttribute>(typeof(JobComInvoiceLine), property.Name, true, attribute => attribute.ListDataSourceMember == "Lookups.CurrencyList");
		}

		public void TestNX101ShippingMarks()
		{
			var noteQuery = new ZQuery(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, InvoiceLine.PK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, "NX101 Shipping Marks");
			noteQuery.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.Equal, "JobComInvoiceLine");
			Factory.Save();
			var note = Factory.LoadTop1<StmNote>(noteQuery);
			AssertNull("Should not save to StmNote when NX101ShippingMarks is empty", note);

			InvoiceLine.NX101ShippingMarks = "123123";
			Factory.Save();
			note = Factory.LoadTop1<StmNote>(noteQuery);
			AssertEquals("123123", note.ST_NoteText);
		}

		public void TestNX101ShippingMarksAttributes()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.NX101ShippingMarksInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Shipping Marks", resStrings.Caption);
				AssertEquals("Marks", resStrings.MediumCaption);
				AssertEquals("Marks", resStrings.ShortCaption);
				AssertEquals("Indicates the shipping marks of Package.", resStrings.FullDescription);
				AssertEquals(512, property.MaxLength);
			});
		}

		public void TestNX101PermitGoodsDescription()
		{
			CombineAssertions(() =>
			{
				InvoiceLine.DeclarationGoodsDescription = "121212";
				AssertEquals("121212", InvoiceLine.NX101PermitGoodsDescription);
				InvoiceLine.OverrideNX101PermitGoodsDescription = true;
				AssertEquals("121212", InvoiceLine.NX101PermitGoodsDescription);
				InvoiceLine.NX101PermitGoodsDescription = "123456";
				AssertEquals("123456", InvoiceLine.NX101PermitGoodsDescription);
				var note = InvoiceLine.Notes.FindByDescription("NX101 Permit Goods Description").First();
				Assert(!note.IsDeleted);
				AssertEquals("123456", note.ST_NoteText);
			});
		}

		public void TestNX101PermitGoodsDescriptionAttributes()
		{
			var line = Factory.New<JobComInvoiceLine>();
			var property = line.NX101PermitGoodsDescriptionInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Permit Goods Description", resStrings.Caption);
				AssertEquals("Goods Description", resStrings.MediumCaption);
				AssertEquals("Description", resStrings.ShortCaption);
				AssertEquals("Indicates the Goods Description for Certificate of Origin. Invoice Line Declaration Goods Description will trim to 512 characters and default to Permit Goods Description.", resStrings.FullDescription);
				AssertEquals(512, property.MaxLength);
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(JobComInvoiceLine), "NX101PermitGoodsDescription", true, attrib => attrib.Member == "NX101PermitGoodsDescriptionReadOnly");
			});
		}

		void CreateInvoiceLineTaxes()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var tariffTypeAT = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var tariffTypeCT = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "CT");
			var tariffTypeSS = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			var tariffTypeTT = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "TT");
			Factory.Save();
			universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var childTariff1 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeAT.PK, "TEST1001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff1.PK, hsnTariffType.PK, "87031000002");
			var childTariff2 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeCT.PK, "TEST1002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff2.PK, hsnTariffType.PK, "87031000002");
			var childTariff3 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeSS.PK, "TEST1003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff3.PK, hsnTariffType.PK, "87031000002");
			var childTariff4 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeTT.PK, "TEST1004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff4.PK, hsnTariffType.PK, "87031000002");
			var childTariff6 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeAT.PK, "TEST1005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff6.PK, hsnTariffType.PK, "87031000002");
			var childTariff7 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeCT.PK, "TEST1006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff7.PK, hsnTariffType.PK, "87031000002");
			var childTariff8 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeSS.PK, "TEST1007", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff8.PK, hsnTariffType.PK, "87031000002");
			var childTariff9 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeTT.PK, "TEST1008", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff9.PK, hsnTariffType.PK, "87031000002");

			Factory.Save();
		}

		public void TestPackingHouseCollection()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var collection = line.PackingHouseCollection;
			AssertType<PackingHouseCollection>(collection);

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX401";
			controllingMessageHeader.TW1_BusinessType = "60";
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			collection.AddNew();
			AssertEquals(1, collection.Count);

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			AssertEquals(0, collection.Count);
		}

		public void TestPackingDateCollection()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var coll = line.PackingDateCollection;
			AssertType<PackingDateCollection>(coll);

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX401";
			controllingMessageHeader.TW1_BusinessType = "40";
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			coll.AddNew();
			AssertEquals(1, coll.Count);

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			AssertEquals(0, coll.Count);
		}

		public void TestSlaughterDateCollection()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var coll = line.SlaughterDateCollection;
			AssertType<SlaughterDateCollection>(coll);

			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX401";
			controllingMessageHeader.TW1_BusinessType = "40";

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			coll.AddNew();
			AssertEquals(1, coll.Count);

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			AssertEquals(0, coll.Count);
		}

		public void TestReservedFieldCode1()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.ReservedFieldCode1);
				line.ReservedFields.AddNew().CY_Code = "1";
				AssertEquals("1", line.ReservedFieldCode1);
			});
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(line.ReservedFieldCode1Info, "Reserved Field 1 Code", "Reserved Code 1", "RS Code 1", string.Empty);
		}

		public void TestReservedFieldCode2()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.ReservedFieldCode2);
				line.ReservedFields.AddNew().CY_Code = "1";
				line.ReservedFields.AddNew().CY_Code = "2";
				AssertEquals("2", line.ReservedFieldCode2);
			});
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(line.ReservedFieldCode2Info, "Reserved Field 2 Code", "Reserved Code 2", "RS Code 2", string.Empty);
		}

		public void TestReservedFieldValue1()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.ReservedFieldValue1);
				line.ReservedFields.AddNew().CY_Data = "A";
				AssertEquals("A", line.ReservedFieldValue1);
			});
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(line.ReservedFieldValue1Info, "Reserved Field 1 Value", "Reserved Value 1", "RS Value 1", string.Empty);
		}

		public void TestReservedFieldValue2()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, line.ReservedFieldValue2);
				var field = line.ReservedFields.AddNew();
				field.CY_Data = "A";
				field.CY_Code = "1";
				field = line.ReservedFields.AddNew();
				field.CY_Data = "B";
				field.CY_Code = "2";
				AssertEquals("B", line.ReservedFieldValue2);
			});
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(line.ReservedFieldValue2Info, "Reserved Field 2 Value", "Reserved Value 2", "RS Value 2", string.Empty);
		}

		public void TestCustomsUnitDefaultingStrategyNotClearCustomsThirdUnitQty()
		{
			CreateTariffForTest();
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Tariff = "";
			line.JI_CustomsThirdUnitQty = "PAC";
			line.JI_Tariff = "01012100003";
			AssertEquals("CustomsUnitQty should not change by CustomsUnitDefaultingStrategy", "PAC", line.JI_CustomsThirdUnitQty);
		}

		public void TestDefaultUseOneTenthCVWhenJI_ProcedureChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Procedure = ProcedureCodes._37;
			AssertEquals(false, line.JI_UseOneTenthCV);

			line.JI_Procedure = ProcedureCodes._38;
			AssertEquals(true, line.JI_UseOneTenthCV);
		}

		public void TestJI_RAPPriceCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(line.JI_RAPPriceInfo, string.Empty);
			AssertEquals("Caption", "RAP/ROR Price", resData.Caption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(line.JI_RAPPriceInfo, JobComInvoiceLine.RapCaptionKey);
			AssertEquals("Caption", "RAP Price", resData.Caption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(line.JI_RAPPriceInfo, JobComInvoiceLine.RorCaptionKey);
			AssertEquals("Caption", "ROR Price", resData.Caption);
		}

		public void TestJI_Calc_RAPRORUnitPriceCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(line.JI_Calc_RAPRORUnitPriceInfo, string.Empty);
			AssertEquals("Caption", "RAP/ROR Unit Price", resData.Caption);
			AssertEquals("ShortCaption", "RAP/ROR U.P.", resData.ShortCaption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(line.JI_Calc_RAPRORUnitPriceInfo, JobComInvoiceLine.RapCaptionKey);
			AssertEquals("Caption", "RAP Unit Price", resData.Caption);
			AssertEquals("ShortCaption", "RAP U.P.", resData.ShortCaption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(line.JI_Calc_RAPRORUnitPriceInfo, JobComInvoiceLine.RorCaptionKey);
			AssertEquals("Caption", "ROR Unit Price", resData.Caption);
			AssertEquals("ShortCaption", "ROR U.P.", resData.ShortCaption);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			UniversalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			OwnerOrgHeader = Factory.New<OrgHeader>();
			OwnerOrgHeader.OH_Code = "XXX1";
			OwnerOrgHeader.OH_FullName = "Owner XXX1";
			Part1 = Factory.New<OrgSupplierPart>();
			Part1.OP_PartNum = "Part No 1";
			Part1.OP_Desc = "PART 1";
			var relatedOrganisation = Part1.RelatedOrganisations.AddOwner(OwnerOrgHeader);
			relatedOrganisation.OU_UsePartAttrib1 = true;

			Part2 = Factory.New<OrgSupplierPart>();
			Part2.OP_PartNum = "Part No 2";
			Part2.OP_Desc = "PART 2";
			Part2.RelatedOrganisations.AddOwner(OwnerOrgHeader);

			Factory.Save();
		}

		public OrgSupplierPart Part1;
		public OrgSupplierPart Part2;
		public OrgHeader OwnerOrgHeader;
		public UniversalReferenceTestDataHelper UniversalTestHelper;

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		}

		#endregion

		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.New<OrgHeader>();
					organization.FillWithValidTestData();
					organization.OH_FullName = "Test Org";
					var address1 = organization.Addresses.AddNew();
					address1.OA_Address1 = "Test 1";
					var address2 = organization.Addresses.AddNew();
					address2.OA_Address1 = "Test 2";
				}

				return organization;
			}
		}

		OrgHeader organization;

		#region Image Base64
		readonly string imageBase64ForTesting = "Qk0yJQAAAAAAADIBAAAoAAAAYAAAAGAAAAABAAgAAAAAAAAkAAASCwAAEgsAAD8AAAA/AAAAACmlAAg5pQAYQpwACDmtABhClAAAOa0AKVKMAAApnAAYKYwAGEKlABA5hAAhUowAECmMABAxhAAhQpwAIUqMAClSlAAhMXsAIUKUABgxjAAYSqUAGDmUAAg5nAApWpwAIVqcACE5hAAhUpwAIUqcABg5pQAYOZwAGEKtABBCpQAYMXsACDGEABA5nAAQOZQAGDmtAAAxrQAIKYwAGEqcABBCnAAAMaUAIVqlACFSlAApWqUAECGEABhChAAAOaUAIVKlABAxnAAIKYQAGEqMACExhAAYSpQAGFKcABA5pQApWpQAGCl7ABAhjAAQQpQACCGEABhKhAAYUpQAEhISCgogCg0gCgoKCgoNCgoNCgoNCgoNDQYPCwsGBisGBgYQKwsQEREZIBkgEREZEREZERERERELEBAGKxALEA8GBgYGEAYLBAIVHR0SAicONTUOGw4SGw4bEhsSDSAuEhISCg0KDQoKIAogCg0KDQ0KDSEKDQoKDwYGEAsPCwsLCwYGBgYGEBERERERIBERGREZERkRGRAGBgYQBgYLCxAGDxALCzMGHR0EFRUCFQ4SDhsSDicSDjUSGwQSIA0KDQ0gCg0uDQ0uDQ0KDQ0KCgoNCg0KCgsGEAYGEAsQBhAGEAsQECsGCxcXNBEREREZERkRGTQRFxcPBgY4CwYLBgsGEBALBhAQMwIVHR0CFQ4nEhsSBDUOEgQSEhIEEhIbCgoKIAoKDQoNDQoNCgoNCgoKDQohDQYQBisLBgYGEAsPCwYLBgYLFxgXGhkRGRkREREZEREXFxcYEAsQBhAGEAYGCwsLDwYGCwYEIwQVFSMOBAQCEgQOBBIbEhIbEhISDSAKCg0NCgoKCg0KDQ0KDSENCg0NCwsPDwYLDysLBhAQBhAGOAsXGhcYGhEREREZERkRERgYGhcXFxAGEA8LDwsQBgYGBgsGBgsQAgICAgQCFRIbDhsSGxsSDhsSGxsSCg0KDQoNCg0KDQ0KDSEKCg0KCgYPBgYGCwsQBgsQDw8LBjMGFxcaFxgaGBoYERkZERERKhoqFywXGhcXBg8GEBAPCxAGEBAGEBAGDxUCAgQdFQQCDicSGxIbEhIbEhISDQoKDQoKDQoNCgoNCgoNDQoNDRAGBgYGCxAPCwsLBgYGCxAGFxcXGhcXGCoYKhERERkaGiwYGhgsFxoXEBAGBgYGCw8rBg8LCzMGBjMjFR0EAhUCBAQOEgISGwISGxsECg0KCg0KDQ0KDQ0KDQ0KIQohKwYLBhAPEAYrCwsLBisGCxcXGBcYLBgYMBoaMCAZGSoaKhoqGBcYFxcXFwsQDwsPEAYGCxAPDwYGDxAGIxUiFR0VAhsEDhICEhICEhIODQoKDQ0KDQ0mISEmISYhJgIEDgQCBCMCAgQCBAIVBBUVBAkUCAwIDC0TCAwTEwgIDBMMCAgIEwgMExMIFBQVAgQCBAISAgQEAhUjBAICBCIxIjEiMSIdHQISDicSDg4SCg0KCgoNCgomISEmISEmJgICBAIEDhIEBAIEAgQCBAQJFAkCCQgMEwgIDAwIDAwIEwgTDAgMCBMTCAgnCQkwAgIEFQIEAgICEg4CBBUEFQ4iMSIxHTEiHRIONQQbEgQSDQoNCg0KDQ0hJiEmISYyDgQODgIEBAIEAgIEFQIEFQQJAhQUFAgIDAwICAwICBMTDAgMEwgTCAwIEwIUFA4UFAQCBAQVAgQEAgQCFQICBCMOIiIdIh0iMQ4EDhICEhICDQoNCg0KDQ0hJiYhMgIEAgQCBAIEAiMOAhUjFQQOCQkJFAkCFAkJEwwIDAwIExMIEwwTCAgMDBMICQkJFAkJFAIUBAIEBAIEAgQCBAIVAg4EBDEiHTExHRUCEgQCEhInCgoNCgoNCgomITImJgQCBAISBBICAhICEgICDhIUAhQnAhQJCRQJHxMICDkMDAwIDAgMCAwTDAwJFB8UAgIUAicJFAQCBAQVBAICBAIEBAQCAgQiMSIiMR0EHRsSDg4ODQoNCg0KIQ0mJiYyAgIEHQQCAgQOBAICBBICBBQCFAkJCQkUCQkfCQgTCAgMCAwTCAwMCAgICB8JCQkJCQkCCQkUAgQCBCMEAgQCBAQCBAQEAhUCIh0iIiMdBAIONQQSDQoNDSEhDQ0mMgQEAg4VBAQCBAIEAgIEAgQEFBQCCQknCRQtDAgIDBMMEwgMEwgMCBMICAgMCAgTCBMMEwkUCQkJJwknAhUOAh0EAgIEAgI7BAQEAiIiHQQVIwQdEgISCg0KCg0KDQomMgIVBAQjIwIODgISAgQCFRIUAgkJMBQJFAkUCAgTCAgIExMIDAwICAgTDAgIDBMMCAgICQkJAgIUCScJJwQEBAQCBAQCBBUSAgIVBAQxIh0CFR0VHRIODQohCgoNCg0mAgQCAgICAgQCBAQCBAIEAhQCFCcJFAkJKAkJHwwMDAgTDAgTDAwMDAwICAwIDAgIDAgUCR8JCQkUAgkCCQ4CAhUEAgQEDgQCAgQCBAIEIiMVAiMEIycODQoNDSENIQsVBB0EBAIEDgQCAQMvAQEBAwUDAwMFBQUDAwUFAwUHAQEAAAAHARYAAAcAAQABAAAHAAMFBQUFJQMDBQMFAwUDAQMvASUBBBUVBAQCBAQVAhUCFRUCFRUODQ0KCg0hMhAEFQQVKAQCAhUVAS8DAQMvBQUDBQUDAwMDBQMDAwMDAAcBBwcBBwABBwAWAAEHAAABAyUDAwMDAwMFAwUDBS8DBSkBBS8BAgICBAIVAgIEAhAVHR0EHRUjISEhDQ0hDwYCBAQOBAIEFQQCAwEvAQMDAwMDAwMFAwUDAAAAAQEAAQEHAQAHAQEHAQAAAQABBwEABwEHAAApAwMFAwMFBQMDAwEBAQMBBAQEAgIEFQQEFRAGFRUVAgQdIQ0hIQsGBg8CEgQCBB0CFQIEAwEBBQUFBQUDAwMFBQUDJQEAAAEHAQEHARYAFhYAFgEABwcAFgAHAQcBByUDAwMFAwMFBQUDBQUDLykBBBUCBAQCBAQCBBAPCx0jHRUVDSENMg8PDxAEAgIVBAQEAhUCAQUDAwMDAwMDBQUDAwMDBQUBAAAAAAAABwEAAAABAAcBBwEAAAEABwEAAAUFBQUDBQUDAwMDAwMDAwEFFQQEBAQCAgQCBAYGBgsVFR0VDTIyCy4PBgYVBAIEAiMEFQQEAQMFAwMFAwMFAwMFBQUFAwMlAQEAFgEHAQABAAcAAAAAAQAWAAcBAAABBQMDBQMFAwMFAwMFAwMDAwMBBAIOAgIEBAQVAg8LCwYLHQQiMg8LBgYGDy4EBAQOAgQEAgICAwMvBQUDAwMFAwUDBQMFAwUDBQAAFgEABwcBAAEHABYAAQAHAAcBAAMDAwMDBQMDBQUDAwUDBQMFAwUDJxUEAgICBBUCAgsPBg8PDxUdBgYGDwsPCw8EIwQCBAIEFQQUAwMDAwUDAwUDBQMFAwABAAEHAQcBAAAAAQEHAQAAAQAABwEAFgEAAAAHAQEAAAMFAwMFBQMDAwMDBQMFCQ4EAgQOBAQEAjMzEAsPDwYPCwsGBgYLBg8VOyMjFQQdBAIJBQMFBQMFBQMFAwUDBQUAAQAABwAAFhYAAAcAAAEBAAABAAAWAAAWBxYABwcBAAUFAwUFAwUFAwUDAwMDFAkEBAQCBBUCFQYGDw8PCw8GMwYQBgYPCwYjBAQVIyMVFBQCAwMDAwUFBQUFAwUFBQUDBQAAAQEABxYAAAABAAcBAAAAAAcAFgcBAAEAAQAFAwUFAyUFAwMFAwMFAwMDCQIUEgIVAgICFQYGCxAPCw8LBgYLBgsPDw8VIwQjBCgiKCIJLwUFBSkFJQMFBQMFAwMlBRYABwAWAAAAARYABwEHAQEHFgABAAAHAAAHAAMDBQUDBQUDAwUDBQUDBQMFFBQnCQQEAhUEBAsQDwsGEAYQCysGBg8PBg87FRUEFQkCHx8oAwMDKQMDAwUDAwMFBQUFAwABAAcAAQEABwABAAAABwABAAAAAQEAAQEAASUFBQUFJQMFBQMFAwMFAwUDCQkUFCcCFQIEAg8GEA8LBgsQCwsPDwYLDw8VIyMCCQkoIigfJQMFAwMFJQUFBQMAAQEWAAAAAAAHAQAABwABAAAAAAAAAQEAFgEAAAcAAQAAAAcAAQUDBQUFAwMFAwMDCSgJJxQJJwQCBAsQBgYQCwYPEAsQDxAGEA8jAhQJAgIfKBYoBSUFLyUDBQMlJQUBAAAAAAEHAQABBwEBAAEHBxYAFgABAAAHAAAHAQEABwEHAQEHAAUFAwMDBQUDBQMFAigJCQICCQQCBBAGCwsGBg8QDwYLBg8GBhAEIwIUCQkoCQk3BQUlBQUDAAUDAwMHARYHAQcBBwcAAQcABwABAAAAAAAHAQEBAAEABwcBAAAABwABAAMFJQUAAwMFAwUDHxQCKAIUAhQVBA8LCw8LDwszBgsGCw8GCxcoAgkJJwIJHwkUBQUlBSkFAQcFAwMWAAEAFgAWAQcAFgEHAQABBwAAAAAHAQABAAAHAQEAFgAWABYBAAUDBRYAAwMFAwADCR8JCSIoCQkCFAYLDw8LCwsGDw8GCxAGCysUCSgJCR8UIgkJAwcFJQUlKQEpJQUAAAAAAAAABwEABwcAABYAAQAWBwEAABYAAAABAAAAAAAAAQAHAAMlAQAABQUDJQADHx8CHwIUCQIJAg8LEA8PDw8GBhAGEA8GFxcJAgIUCS0JHx8JBSkHBSUFBwcHAQMWBxYAARYAAQcBAAAWBwAAAAAAAQEAFgABBwcAARYAAQEHAAABAAMBAAABAwMDAQEFHwkfCQkUFAIUCRoXDwYGBgYPKwYLBgYXFywIFAIUFAgIOh8fAwABBwUlBwcHBykWAAABBwEAAAcBFgcBAAAABwAAFhYAAAABBwcAFhYABwEAAAcAAAAAAAAAAwEAFgEFCQkIDAkJHwkCCBgXFwYGCwYGDw8LBhcYGBgMCAkJIgw8EwkfBQAHAAcFAAApBwAABwcAAAAAFgAAAAEHAAEHAQcBAAAAAQEHAAEAAAABAAAWAAEHAQcBAQcBAwcBAAclHwwMCBQJKAkTLRcaGAYLDwYLBg8GFxcXFxoTCBQJFBMMCBMJJQABAAcpBwcHKQcHAAEHAAEHAAAWAAcBAAABAAAAAQEWAAABBwcBAAcAAQAAAQcBAAAABwAAFgEAAAEFHxMMEwkJCQwMCBcXGBcXDwYQBgsXGBoaGhgICAgIFAwMEwwIKQcBAAEHACkHAAAHAAAHAAABBwcBAQABABYAAQEHAAEHAQEHAQcBAAAAAQAAAQAAAAABAAAHAQAHAQcAExMICCgUDAgMExoaGBcXFw8LIBcaFywXGBgIEwgTCAgIDAgIAAEABwcABwcABwcpBxYpBwAHAAAABwEAAQABAAABAAABAAABAAEAABYABwcBABYAFgAHAAcBBxYAAAEHCC0MDAktCAwICBgaGhoYGBcRERgYGBoXFxcTDAgTDBMTCBMIAQABAAEAAAAWAAAHAAcABwcBABYAAR4cDgkkHBwcHBwcHBweDhwBBwABAAEAFgABAAcBAAAHAQABAAABDAgMCAwMEwgIDCwXGhgYFxEREREZFywwGBgIEwwIEwgICAwIAAAAAQABBwAWAAABAAEHBwcABwEABxwcCQ4eHDccCQ43JB4cCRwBABYAAQABAAAAAQcBAAAAAQABAAcBOhMMDAwIDAgMExgsGBoaIBEgGRkRGRoYKiwIDBMIEwgTDBMIAQAWAAAHABYABwcpBwcBAAABAAAWADcJHB4cCR4cCSQOCQkJHh4HAAAAABYAAAcBBwEAAAAWAAcAAQEACBMIDAgVCBMMCBoaGBcZERkRERkRGRcYGCoICAwIDBMIEwgTAAAAAQEAAQAHAQAHACkHBwEHBwcAASQJHhweHgkkHB4kHBwkDhwBAQEHAQABAAAAAQAWAAEABwEABwcBExMICAw5DAgIDBoqGBERGREZGRERGRkRLBoTDBMMLQgICBMIARYAARYAAAEAAAcABwcAFikABwcAAAkcCR4JHCQeHBweHBwJHBwAARYAAAABAAcABwcAAQAHAAcAAAABCAgIDAwTCAgTCCoaEREZEREZERkZEREZETAIEwwIDAwTDAgTAAAAAQAHAQcWBwEABwcAKQcBBwEAFhweCQ4kHAkJHAkcHiQJCSQBAAAHAQEABwEHAQABAAABAAABAQEHCAwtDAgMCAgIExoZGTQREREREREZERkRETYMCAgMEwgMCAgMAAEABwcABwAAAQcAAQEHBwAHKQcAABwcHiQ3DhwJHhweJAkkHA4AFgEAAAcBAAAABwcHAAEHAQAABwABDAgICAwTDBMMDBkRGRERGRkZERkRERERERkICAgTCDkIDAwIBwEHAAAABwcBAAEBBwEAFgcWAAcBBw4eDgkcHiQeDhwOJBweHDcAARYABwAWAAABBwEABwEAAQEAAQABCAwICAgTLQgMExERERkRGRERGREZEREZGSoTCAgMCAwMCAgIAQABBwcBAAAHAQAAAAAHAAApBwAHAB4JJCQJJA4cHh4kDgkcHgkAAAAAAQEAAQAAAQcBAAAHAQAAAAEACAgIDAwIDAgTCBEgERERERkRERERGRkRERoTCAwICAgMEwwMBwcAAQAHARYAAAABAAcBAAcAAAAAAR4kDg4kDh4JJAkcCQkcCRwWAAEAAAAABwcBAAAAAQEAAAAWAAABExMMLQwIEwwIDDYgGREZGRkRIBERGRkXMBoMCAwmCAwTDAwMAAAAFhYAAQEABwABAAABBwAABwcpAR4eJBwkHg4eJB4eDgkeHBwAAQEAAQEAAQAHAAcAAQAHAAcAAAABCAgMExMMExMIDBgsMBkRGRkREREZERg2NiwIEwgICDItDAgMARYBAAAAAQAAAQAHAQcBAAEAAQAHABwJCR4OJAkkDg4cHiQOJBwBAAEAAAAAAQEAAQEHAAABAAEAFgEACBMMCAgIDBMICDAYGBEZGREZGRkZERcsGioTCAwMLS0TCBMIAAAAAAEHABYBAAcBAAAAAQABBwABAAkkHiQeDh4eHB4eHAkcHgkAAQAWAAEHAAABAAABABYABwABAAABEwgMCAwIDAwICCw2GhcRIBkRERkYFxoaNhgMDAwIPAwMCAgIAAABAAEAAAcBAAAAAAAAAQEBAAcAAR4JHiQeHg4eJB4cCR4JCQ4AABYAARYAAAcBBwAABwABAAABBwAADAgTEwwICBMIExcXNhgaGBERGRcXGhgsGCwTCAwtCAwIExMIARYAAAAWAAEAFhYAAQEHAAAAAQEAACQcHgkkDh4kDg4eHBwkHCQBAAAAFgABBwEAABYAAQAWAAEHAAcBOQwtDBQTCAgICBoqGhcXFxk0ERcYFxcaGhoIEwgMKBMICAwTBwABBwEAFgABAAABAAEAAQEABwEHAQ4cHB4OHBwJHBweDh4OHA4AFgEAAAAAAQABBwABBwcAAQABBwApDAgMCAkJExMTExgXGBcYFxcREBAGFxg+GhcMCCgUCQgIDAgUBQcBAAAAAQAAFgABAAABAAEAAQcAAQEAAQABAAAAAQABBwAWAAEHAQAWAAEABwEHAAABAAcAAQABBwADHwgMCBQJCRMIExoXGBoYBgYGECsGGBc2FyoIDB8fHxMICB8JAwEHAAEDAAEBAAEAAAcABwcAAAABAAEHAAcHBxYAABYAAQAAAAABAAEAFgABAAAAARYAFgABBQEAAQEDHy0IDAkJFBQICBoXFxcaCwYQEAYQBhcXGBoIBCgUAgwMCBQfAwABAAUDAQAHAQABBwEAAQEABwEAAQABBwEBAAABAAAABwABAAAAAQAABwcAAQEABwAAAAApAwAHAAcFHwkIDAkJCQkEDBgYFwYLEAYQBhAGCxAPGBoJFAkoCQgUCQkUBQcABQMFAQAAAQMBABYAAQEHAAEHAQcBBwcBBwABBwABAAABAAAAAQAAAAAHAAEHAQMAAAAHAwUFAAAFHx8fCQIoCQkJAhoYBhA9LhAGEAYrBgY4FxcUDicJAhQJCQkUAwEFAwUDAAEpAwUHAQAHAAABAAABAAEAAQEAAAEAAQEAAQEAAAAWAAABAQABAAAAAAUDARYABQUFAwEDCQkJHwkJCQIUCRcXCwY4DwYQCwYGBhAPEBcUAgkwFAkUCQkJBQMFJQUFAQADBQMBAAABBQAAAQcABwcABwABAAAHAAAHAAAWAAAABwcABwMDAQEAAQMFAwABAwUDBQcFHwkUCQkUAgkCFAsGBgYGDw8LCxAGDwYQBgsCFAkUCQkUCQkUAwMFAyUFAQUDBQMBACUDAQEAAQAHAAAWAAABBwcBAAABAAABBwAAAAABAAEAAwEAAAUFBQUAAwMFBQUFCQkUFCcJJwICIwsLBhAGBjMPCwYLBgsGEAYVAg4JFBQOFAkJBQUDBQUDBQUDBQUAAQMHAAEHAAEAFgAAFgEAAQAAAQEHAAEHAAABAQAAAQAHAQUAAQMFAwMFBQMFAwMFFAkfCQkCCQkEHQ8LBgYQCwYLDxAGCwYLDwsOBDAJFBQCFBQJAwMFAwMFAwMFBQMABRYAAQABAAABAAABAAcAAAEWAAABAAABAAAHAAcBBwEAAAMFAAUDAwUDBQUDBQUDCQkCHxQJMAQVBA8PBhAGBgsPDw8GCwYLDwsCDgICFAknCRQCBQUDBQUFAwMFBQMFAyUFBQABAAEAAQAWAAABAAcBAAABAAABAAABAAEAAAMFAwMFAwMFAwUFAwMFAwMFFAkUDgIUBAQCAgYQBgYLBgYLBgsGDzgPCwYCFQQEDgIJFBQUAwMFBQMFAwUFBQUFBQMFAwUDAAABAAEAARYAAAAAFgEHAAEHAAcAAQAWAwUDBQUDBQUDBQUDAwUDBQUDCRQCFAkEAgISDgYLBhAGEBAGDw8GEDMGEAYEAgIOBBQOFBQJBQMDAwMDBQMFBQMFAwUDBQMBAAEAFgAHBwABBxYAAAABBwABBwABAAAAAAUDBQMFAwMFAwMFBQUDAwUDCQIJAgIVBAQCBAsGCwYGEAsLEAYPBgYGEAsCAgIOBAQVCScJAwMFAwUDBQUDBQMFBQUDJQABBwABFgAAAAEAAQEAAAAHAQcBAAABAAAHAAEDBQMDBQUDBQUDBQUDAwMFCQIUBAQOBBUEFQYQBhAGEAYGEAsGCwsGCwYCBAIEAgIEBAkUAQUDBQMFAwMFAwUDAwUDFhYAABYAAAABBwAHAQcBBwcBBwAABwEAAAEAAQAAAwMFAwMFAwMFAwMFAwUDFBQCBAQVAg4EIw8QBgYQBhALDwYGDwYLEAYEAgIEBCMOAhUCBQUDAwMFAwUFAwMFBQMAKQABAAABAAEAAQEAAAAAAQAHAAEHAQcWAAABAAABAAMFAwUDBQUDBQUvAwUDAgQVDgQCDgIEAgYGBgYGCwY4Dw8GDxALBgYCEgIEBAIEDgQCBQMFAQMDBQUDBQMFAwUDBQMFAwABAAEAAQEABwAHAQAABwcHAAABACUDAwMFAwUDBQUDAwMFAwUDBQMDFQQCAgIEAgQCBA8PBhAGEAYQCg8PDwsLEBAEAgQCFQQCBAIVAQUDAwMFAwMFAwUFBQMFBQUFAwEHAAABAAAWABYAAAAWAAABBwAHABYlJQMFBQMFAwUDBQUDBQMDAwUBAgIEBBICEiMEAgYPKwsQBhUCDQ0PDxAQBhACEhUEAiMEAgQCAQMFAwMFAwMFAwMFAwUDBQUDAAABABYAARYAAQABBwEABwcAAAEAFgABAwUDBQUDBQMFAwMDAwMDBQEvFQQEDgIEAhICBAYQBgYGBg4CDSEKCg8GEAYCAg4SAhUVAg4EAQElAwMFAwMFAwMFBQUDAwAABwAWAAEHAQcBAQEBAAABAAcAAAAAAQcBBwUDBQUDBQMDBQUDAwMDKQMvFQQEAgQCBAQCEgYGEBAEAhsOCg0KDA8PBgYEDgICAg4EFQICAS8BKQUDAwMDBQMDBQMFBQEAAQEAAQABAAEABwAABxYABwAWAAcWKQEAAQAFAwMFAwUDBQMDBS8BAwEBAgQCBAIEBAICBAsGKxUOEhICCg0KCg0PDxAEAgIVEgQCBAQEKQElAQEFAwUFAwUDAwUDAwMFAwUDBykHARYAARYAAQABAAAAFgAABQMFAwMFAwUDBQMFAwMDAwMBLykBBB0EAgICFQQEAhAGBgICDgIOCg0KDQ0NCg8CAgIEAg4EAgQEAhUCBAIEDgIJFCgJCQkJFCgfCQkIDAwMEwwTDAgMDAwIEwwMDAwMLR8iHzcJCQkJFAkUFAICDg4OBAIEFRICBAIEDgIEBAYdBA4EGxIbDQoKCgoKDQ8SAg4EDgQEEgQjBA4EAhUEBDAOFAkUAhQfCQkJCQwMCAgIExMICAgIEwgMCAgTCAgMDCIfHygWKBQUCRQJAgIVEgQEDgIEDgQCFQQCBBUEAh0EDhsOAgIODQ0NCgoNCg0EAgICBAICAgICDgICBAIEDgIUAgkCFCgJCQkUHxMIDAwTCAwTExMMCBMIExMMCAgILQgWKBYoHwkOFAkUFAQCBAICBAQEAgQEFQQVAgIEBAQOAhICGxsEDQoNCg0KDQ0mIQQSBAISAhUEAgQVBA4CBAQCBAkJCQkCFCgMCAw5EwwTDBMIOggTCAgTCBMICAwMDAgMOigfCQkJCRQEAgQCBAQCAgQEAgQEAgISBAIiIgISAg4SAg4ECg0KDQoNCgomJgICBBICFQQjBAIEDgQEAhUEAhQUAhQJAgkCHx8JHwgMCAgIEwgMEwgIExMIDAkJKCgfAiIiKAkUAgQCEgISAgQCBAICBAIVAgQCAjEdHQ4CGycODgQOCgoNCg0KDQ0hJiYEAgIEAgQOAgQCBAIEFQQOBBUCCQkwCRQJCQkJCQgTCAgMCC0IDBMMCAgTExQJCR8CHwIJCQIJDg4EAgQCAgISAgQCBAQCBAQCBCICHQ4OBBInEhIODS4NCg0KCg0mISEmISMEAgQEAgQEBAQEAgQCBAQOFQkCFBQJCQIJLQgIEwgIEwgMCAgTDAgIDBMTCQICCQIUHyMEBAIEAg4CFQIVAgIEAgIEDgIxHQIiHQIOEhsEDhICCgoNDQ0KDQoyJiYyJg4CBAIVBAQCBBUCFRUVAgIEBAQJAgkUCQkICAwIDAgMCAwTCAwIEwwICAgIDBQJCQkUDhUCBBUVAhUEAgQCDgQCBAQCDiIiHR0CHRsSJwQbGxsOCgoKCgoNCgoNJiEmISYVAhUEAgQVBAQVBAQEFQQCBAQUFBQCCQgMDDkMCBMIDAgTCAgTCBMTCAwTCAkUAiIJFCMVFQQCBAIEBAIEBAIEEgQEFSIdHR0dHQQOEhIOBAQSDS4NIC4NCg0KDQoNCgoKDQ8PDwsGEBAGCwsPEBAQBg8QDxgXFxcaGBoYGzYaKioZETAaLBoYFxcsGBcXGAYLEAsGEAYQBgYQBgYGCwsGBgYdAg4ODg4CEhsOEhIbEhI1DQ0uCg0KDQ0NCg0NDQ0NCg8PDw8GCwYGBg8GCwYGBgsLBhoXGioXGBoXKiowGhERERgaKiwYGBcaFxoXKwsPDw8LBgsGCxAPBgsGEBALEAQOBAIbBAISGxIEJxsSGxsSCg0KDQ0NCgoKDQoKCgoKDQ0PDwYGEAsPDwsPDwsPBgYGCxAPFxcaFxcYGBoYKhEZESoaFywaGhgqFxcYEAYGEBAPMzgPBgYrBhALEAYLBgIODg4EDg4EAhsbEg4SAhIOCg0KICAKCgoKCgoNCg0KCgoNCg8PMwYGEBAPBgYGCwYGBhAGMxcXGhoaFyoREREZERERGBgsGBoXGAYLCwsGBgsQBhAGDxAPBgYGDwYdEg4OAhsODg4OBBIbBAQOGxIbCgoNDQ0NCgoNCg0KDQoKDQoNCg8PDwsPCwYQCxArBgYGCw8GBgsrGBgXGBEgESARICARIBcYFxcXFwsQBgsLEBALCwsGCwYQBisLBh0VJw4CGxInEhsSGxsSDhsEEhIbLiAuDS4NDQ0KDQ0KCg0KDSEKDQ0PDw8LBg8GBgYQBhAPDwYLBgYGGhcXGBERGRERERE0ERoYFxcXBgYGCwYGCxAGBhAGBgYLBgYGBgQCEhIOBBsSAgIbBBISGxIbEgQSCiAuCg0gCgoKDQ0KCg0KDQ0KDQ0MCg8PBgYGCwYPBgYLBgsGBhAGCxcRERERLhERGRERICARBgYGBgsQBhAGBgsLDw8GCwsQBhAEBAIbAhIOEg41DhsSGxsSGxsSEhsSCgoKIAouDQ0gCgoKDQoNCgoNCgoKDQ8PDw8PDwsPEBAGCw8GDwsGBhERGSARERkRESAZEREREQ8QBgYLEAYLCwsQBgYQBgYPCxUCDg4SDgInDhIOEgQOEhIbEhISGxISEhISCiAKIC4KDQ0KCg0KDQ0KDQ0KIQoPDwsGKwsGBgsGCw8QDwsLBhkZERE0EREZIBERICAgERALCxAGCwYQEBAGEAYLCxALBg4EDg4CJw4OEg41DhsSGxsSJxIbIC4KEhISCgoKDQ0KDQ0KDQ0KDQ0KIQoNCgoNCg8PDw8LBgYLDwYPBgY0ETQRESARICA0ESARNBkRERk0BhAGEAYLEAsGCw8QBgsLHQ4CAgIOAgIODg4OBA4SDgQOEhIOCgoN";
		#endregion
	}
}
