using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class SettingOrderDeterminerTest : TestCaseWithFactory
	{
		public void TestGetImportSettingOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var settingOrder = SettingOrderDeterminer.GetSettingOrder(invoiceLine).ToArray();
			AssertEquals(75, settingOrder.Length);
			AssertContains("US_JI_ParentProduct", settingOrder[0]);
			AssertContains("US_FlavorContentCreditInd", settingOrder[1]);
			AssertContains("JI_PartNo", settingOrder[2]);
			AssertContains("JI_CC", settingOrder[3]);
			AssertContains("US_SupTariff", settingOrder[4]);
			AssertContains("JI_OA_ManufacturerAddress", settingOrder[5]);
			AssertContains("US_UC_NKCountryOfExport", settingOrder[6]);
			AssertContains("US_UC_NKCountryOfOrigin", settingOrder[7]);
			AssertContains("JI_Tariff", settingOrder[8]);
			AssertContains("US_SPI", settingOrder[9]);
			AssertContains("US_SecondarySPI", settingOrder[10]);
			AssertContains("US_DestinationState", settingOrder[11]);
			AssertContains("JI_OA_ShipToPartyAddress", settingOrder[12]);
			AssertContains("US_TaxApply", settingOrder[13]);
			AssertContains("US_TaxCode", settingOrder[14]);
			AssertContains("US_TaxRateT", settingOrder[15]);
			AssertContains("US_TaxRateS", settingOrder[16]);
			AssertContains("US_TransactionsRelated", settingOrder[17]);
			AssertContains("JI_OA_Seller", settingOrder[18]);
			AssertContains("JI_Description", settingOrder[19]);
			AssertContains("JI_InvoiceQuantity", settingOrder[20]);
			AssertContains("JI_InvoiceUQ", settingOrder[21]);
			AssertContains("JI_LinePrice", settingOrder[22]);
			AssertContains("JI_OA_ConsigneeAddress", settingOrder[23]);
			AssertContains("JI_RH_NKCommodity_Code", settingOrder[24]);
			AssertContains("US_ManifestQty", settingOrder[25]);
			AssertContains("JI_Weight", settingOrder[26]);
			AssertContains("JI_WeightUQ", settingOrder[27]);
			AssertContains("JI_NetWeight", settingOrder[28]);
			AssertContains("JI_NetWeightUQ", settingOrder[29]);
			AssertContains("JI_Volume", settingOrder[30]);
			AssertContains("JI_VolumeUQ", settingOrder[31]);
			AssertContains("JI_CustomsQuantity", settingOrder[32]);
			AssertContains("JI_CustomsSecondQuantity", settingOrder[33]);
			AssertContains("JI_CustomsThirdQuantity", settingOrder[34]);
			AssertContains("US_PIRPRulingType", settingOrder[35]);
			AssertContains("US_AgricultureLicNo", settingOrder[36]);
			AssertContains("US_CAExportCertificate", settingOrder[37]);
			AssertContains("US_PIRPRulingNo", settingOrder[38]);
			AssertContains("US_WoolLicenceNo", settingOrder[39]);
			AssertContains("US_CBTPACertificateNo", settingOrder[40]);
			AssertContains("US_CottonFeeExempt", settingOrder[41]);
			AssertContains("US_CottonCertificateNo", settingOrder[42]);
			AssertContains("US_MiscPermitNo", settingOrder[43]);
			AssertContains("US_SWPMIndicator", settingOrder[44]);
			AssertContains("US_IsNAFTANet", settingOrder[45]);
			AssertContains("US_LumberExportPrice", settingOrder[46]);
			AssertContains("US_LumberExportCharges", settingOrder[47]);
			AssertContains("US_LumberImporterDeclaration", settingOrder[48]);
			AssertContains("US_VisaNo", settingOrder[49]);
			AssertContains("US_VisaQty", settingOrder[50]);
			AssertContains("US_VisaUQ", settingOrder[51]);
			AssertContains("US_TextileCategoryNo", settingOrder[52]);
			AssertContains("US_DateOfExportFromCountryOfOrigin", settingOrder[53]);
			AssertContains("US_ADD_NA", settingOrder[54]);
			AssertContains("US_ADDCaseNo", settingOrder[55]);
			AssertContains("US_ADDDepositValue", settingOrder[56]);
			AssertContains("US_IsBondedADD", settingOrder[57]);
			AssertContains("US_ADDDepositRateIndicator", settingOrder[58]);
			AssertContains("US_ADDuty", settingOrder[59]);
			AssertContains("US_ADDDecID", settingOrder[60]);
			AssertContains("US_CVD_NA", settingOrder[61]);
			AssertContains("US_CVDCaseNo", settingOrder[62]);
			AssertContains("US_CVDDepositValue", settingOrder[63]);
			AssertContains("US_IsBondedCVD", settingOrder[64]);
			AssertContains("US_CVDDepositRateIndicator", settingOrder[65]);
			AssertContains("US_CVDuty", settingOrder[66]);
			AssertContains("US_OverrideDuty", settingOrder[67]);
			AssertContains("US_Duty", settingOrder[68]);
			AssertContains("US_OverrideSupDuty", settingOrder[69]);
			AssertContains("US_SupDuty", settingOrder[70]);
			AssertContains("JI_HazMatCode", settingOrder[71]);
			AssertContains("JI_HazMatCodeQualifier", settingOrder[72]);
			AssertContains("US_HazMatDesc", settingOrder[73]);
			AssertContains("US_HazMatClassDesc", settingOrder[74]);
		}

		public void TestGetImportSettingOrderForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var settingOrder = SettingOrderDeterminer.GetSettingOrder(invoiceLine).ToArray();
			AssertEquals(33, settingOrder.Length);
			AssertContains("US_JI_ParentProduct", settingOrder[0]);
			AssertContains("US_FlavorContentCreditInd", settingOrder[1]);
			AssertContains("JI_PartNo", settingOrder[2]);
			AssertContains("JI_CC", settingOrder[3]);
			AssertContains("US_SupTariff", settingOrder[4]);
			AssertContains("JI_OA_ManufacturerAddress", settingOrder[5]);
			AssertContains("US_UC_NKCountryOfExport", settingOrder[6]);
			AssertContains("US_UC_NKCountryOfOrigin", settingOrder[7]);
			AssertContains("JI_Tariff", settingOrder[8]);
			AssertContains("US_SPI", settingOrder[9]);
			AssertContains("US_SecondarySPI", settingOrder[10]);
			AssertContains("US_DestinationState", settingOrder[11]);
			AssertContains("JI_OA_ShipToPartyAddress", settingOrder[12]);
			AssertContains("US_TextileCategoryNo", settingOrder[13]);
			AssertContains("US_ZoneStatus", settingOrder[14]);
			AssertContains("US_F_PNDisclaimer", settingOrder[15]);
			AssertContains("JI_OA_Seller", settingOrder[16]);
			AssertContains("JI_Description", settingOrder[17]);
			AssertContains("JI_InvoiceQuantity", settingOrder[18]);
			AssertContains("JI_InvoiceUQ", settingOrder[19]);
			AssertContains("JI_LinePrice", settingOrder[20]);
			AssertContains("JI_OA_ConsigneeAddress", settingOrder[21]);
			AssertContains("JI_RH_NKCommodity_Code", settingOrder[22]);
			AssertContains("US_ManifestQty", settingOrder[23]);
			AssertContains("JI_Weight", settingOrder[24]);
			AssertContains("JI_WeightUQ", settingOrder[25]);
			AssertContains("JI_NetWeight", settingOrder[26]);
			AssertContains("JI_NetWeightUQ", settingOrder[27]);
			AssertContains("JI_Volume", settingOrder[28]);
			AssertContains("JI_VolumeUQ", settingOrder[29]);
			AssertContains("JI_CustomsQuantity", settingOrder[30]);
			AssertContains("JI_CustomsSecondQuantity", settingOrder[31]);
			AssertContains("JI_CustomsThirdQuantity", settingOrder[32]);
		}

		public void TestGetSettingOrderForStandalone_InvoiceHeader_Import()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var settingOrder = SettingOrderDeterminer.GetSettingOrderForStandalone(invoice).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(34, settingOrder.Length);
				AssertContains("JZ_OH_Supplier", settingOrder[0]);
				AssertContains("JZ_OH_Buyer", settingOrder[1]);
				AssertContains("JZ_StandAloneInvoiceDirection", settingOrder[2]);
				AssertContains("JZ_InvoiceNumber", settingOrder[3]);
				AssertContains("JZ_InvoiceDate", settingOrder[4]);
				AssertContains("JZ_GB", settingOrder[5]);
				AssertContains("JZ_InvoiceAmount", settingOrder[6]);
				AssertContains("JZ_RX_NKInvoice_Currency", settingOrder[7]);
				AssertContains("JZ_InvoiceCurrExRateType", settingOrder[8]);
				AssertContains("JZ_InvoiceCurrExRate", settingOrder[9]);
				AssertContains("JZ_IncoTerm", settingOrder[10]);
				AssertContains("JZ_Weight", settingOrder[11]);
				AssertContains("JZ_WeightUQ", settingOrder[12]);
				AssertContains("US_InvoiceType", settingOrder[13]);
				AssertContains("US_PaymentTerms", settingOrder[14]);
				AssertContains("US_PaymentTermsDesc", settingOrder[15]);
				AssertContains("US_DES", settingOrder[16]);
				AssertContains("US_ValueForDiscount", settingOrder[17]);
				AssertContains("US_ValueForForeignTax", settingOrder[18]);
				AssertContains("US_TermsOfDeliveryLocationQualifier", settingOrder[19]);
				AssertContains("US_TermsOfDeliveryLocationIndicator", settingOrder[20]);
				AssertContains("US_TermsOfDeliveryLocation", settingOrder[21]);
				AssertContains("JZ_OA_SupplierAddress", settingOrder[22]);
				AssertContains("JZ_OA_ManufacturerAddress", settingOrder[23]);
				AssertContains("JZ_OA_ExporterAddress", settingOrder[24]);
				AssertContains("JZ_OA_InvoicerDocAddress", settingOrder[25]);
				AssertContains("JZ_OA_SellerAddress", settingOrder[26]);
				AssertContains("JZ_OH_SellingAgent", settingOrder[27]);
				AssertContains("JZ_OH_Buyer", settingOrder[28]);
				AssertContains("JZ_OA_BuyerAddress", settingOrder[29]);
				AssertContains("JZ_OH_BuyerAgent", settingOrder[30]);
				AssertContains("JZ_OA_ConsigneeAddress", settingOrder[31]);
				AssertContains("JZ_OA_SoldToPartyAddress", settingOrder[32]);
				AssertContains("JZ_OA_ShipToPartyAddress", settingOrder[33]);
			});
		}

		public void TestGetSettingOrderForNormal_InvoiceHeader_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var settingOrder = SettingOrderDeterminer.GetSettingOrderForNormal(invoice).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(32, settingOrder.Length);
				AssertContains("JZ_OA_SupplierAddress", settingOrder[0]);
				AssertContains("JZ_OA_ManufacturerAddress", settingOrder[1]);
				AssertContains("JZ_OA_ExporterAddress", settingOrder[2]);
				AssertContains("JZ_OA_InvoicerDocAddress", settingOrder[3]);
				AssertContains("JZ_OA_SellerAddress", settingOrder[4]);
				AssertContains("JZ_OH_SellingAgent", settingOrder[5]);
				AssertContains("JZ_OH_Buyer", settingOrder[6]);
				AssertContains("JZ_OA_BuyerAddress", settingOrder[7]);
				AssertContains("JZ_OH_BuyerAgent", settingOrder[8]);
				AssertContains("JZ_OA_ConsigneeAddress", settingOrder[9]);
				AssertContains("JZ_OA_SoldToPartyAddress", settingOrder[10]);
				AssertContains("JZ_InvoiceNumber", settingOrder[11]);
				AssertContains("JZ_InvoiceAmount", settingOrder[12]);
				AssertContains("JZ_RX_NKInvoice_Currency", settingOrder[13]);
				AssertContains("JZ_InvoiceCurrExRateType", settingOrder[14]);
				AssertContains("JZ_InvoiceCurrExRate", settingOrder[15]);
				AssertContains("JZ_IncoTerm", settingOrder[16]);
				AssertContains("JZ_InvoiceDate", settingOrder[17]);
				AssertContains("JZ_Weight", settingOrder[18]);
				AssertContains("JZ_WeightUQ", settingOrder[19]);
				AssertContains("US_UC_NKCountryOfExport", settingOrder[20]);
				AssertContains("US_DateOfExport", settingOrder[21]);
				AssertContains("US_UC_NKCountryOfOrigin", settingOrder[22]);
				AssertContains("US_TransactionsRelated", settingOrder[23]);
				AssertContains("US_FirstSale", settingOrder[24]);
				AssertContains("JZ_NoOfPacks", settingOrder[25]);
				AssertContains("US_IsLineGrouping", settingOrder[26]);
				AssertContains("US_FDAContactName", settingOrder[27]);
				AssertContains("US_FDAContactPhoneNo", settingOrder[28]);
				AssertContains("US_FDAContactEmail", settingOrder[29]);
				AssertContains("JZ_OA_FDAShipperAddress", settingOrder[30]);
				AssertContains("JZ_InvoiceCurrLandedCostExRate", settingOrder[31]);
			});
		}
	}
}
