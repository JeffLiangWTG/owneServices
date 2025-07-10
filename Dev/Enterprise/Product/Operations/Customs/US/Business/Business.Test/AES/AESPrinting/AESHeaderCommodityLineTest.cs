using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AES.Testing
{
	[TestedType(typeof(AESHeaderCommodityLine))]
	sealed class AESHeaderCommodityLineTest : AESPrintCommodityLineTest
	{
		public override void TestLicenseTypeDescription()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Entry.Factory, new ZString[] { USAESLicenseCode.Codes.C36 });

			Entry.MergedLines.AddNew().RandomLine.US_LicenseType = USAESLicenseCode.Codes.C36;
			var headerForDocumentPrint = new AESHeaderPrint(Entry);

			var line = (AESHeaderCommodityLine)headerForDocumentPrint.Commodities[0];
			AssertEquals("License Type description", "GBS  C36 (C36)", line.LicenseTypeDescription);
		}

		public void TestProperties()
		{
			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.JI_Description = "DESCRIPTION 1";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.US_LicenseValue = 2390m;
			invoiceLine1.JI_Weight = 145m;
			invoiceLine1.JI_CustomsQuantity = 24m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsSecondQuantity = 46m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 1500m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2020202020";
			invoiceLine2.JI_Description = "DESCRIPTION 2";
			invoiceLine2.US_ExportCode = ExportInformationCodeList.Codes.CH;
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine2.US_LicenseNo = "LCN5678";
			invoiceLine2.JI_Weight = 250m;
			invoiceLine2.JI_CustomsQuantity = 30m;
			invoiceLine2.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Packs;
			invoiceLine2.JI_CustomsSecondQuantity = 60m;
			invoiceLine2.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pieces;
			invoiceLine2.US_ECCN = "EC34";
			invoiceLine2.JI_LinePrice = 2600m;
			invoiceLine2.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;

			invoiceLine2.US_DDTCITARExemptionNo = "123.11B";
			invoiceLine2.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine2.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine2.US_DDTCRegistrationNo = "REG123";
			invoiceLine2.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.Ammunition;
			invoiceLine2.US_DDTCQuantity = 120m;

			invoiceLine2.US_IsUsedVehicle = true;
			invoiceLine2.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			invoiceLine2.US_VehicleID = "VID12";
			invoiceLine2.US_VehicleTitleNo = "VTITLENO";
			invoiceLine2.US_VehicleTitleState = "MA";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, Declaration.ActiveEntryHeaders.Count);
			var entry = Declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);

			var headerForDocumentPrint = new AESHeaderPrint(entry);

			var commodityLineForDocumentPrint = (AESHeaderCommodityLine)headerForDocumentPrint.Commodities[0];
			AssertCommodityLine(invoiceLine1, commodityLineForDocumentPrint);
			AssertEquals("DDTC ITAR Exemption Number", ZString.Empty, commodityLineForDocumentPrint.ITARExemptionNo);
			AssertEquals("USML Category Code Code", ZString.Empty, commodityLineForDocumentPrint.USMLCategoryCode);
			AssertEquals("USML Category Code Description", ZString.Empty, commodityLineForDocumentPrint.USMLCategoryDescription);
			AssertEquals("Vehicle ID Type Description", ZString.Empty, commodityLineForDocumentPrint.VehicleIDTypeDescription);
			AssertEquals("Vehicle Title State Description", ZString.Empty, commodityLineForDocumentPrint.VehicleTitleStateDescription);

			commodityLineForDocumentPrint = (AESHeaderCommodityLine)headerForDocumentPrint.Commodities[1];
			AssertCommodityLine(invoiceLine2, commodityLineForDocumentPrint);
			AssertEquals("DDTC ITAR Exemption Number", "123.11B", commodityLineForDocumentPrint.ITARExemptionNo);
			AssertEquals("USML Category Code Description", USMLCategoryCodes.Codes.Ammunition, commodityLineForDocumentPrint.USMLCategoryCode);
			AssertEquals("USML Category Code Description", USMLCategoryCodes.Descriptions.Ammunition, commodityLineForDocumentPrint.USMLCategoryDescription);
			AssertEquals("Vehicle ID Type Description", VehicleIDTypeList.Descriptions.VIN, commodityLineForDocumentPrint.VehicleIDTypeDescription);
			AssertEquals("Vehicle Title State Description", "Massachusetts (MA)", commodityLineForDocumentPrint.VehicleTitleStateDescription);
		}

		protected override AESPrintCommodityLine GetLineForTest()
		{
			var declaration = Entry.Declaration;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var entryLine = Entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C36;
			var headerForDocumentPrint = new AESHeaderPrint(Entry);
			return (AESHeaderCommodityLine)headerForDocumentPrint.Commodities[0];
		}

		protected override BusinessObject GetNewBusinessObject() => new AESHeaderCommodityLine(Factory.New<CusEntryLine>());

		void AssertCommodityLine(JobComInvoiceLine invoiceLine, AESHeaderCommodityLine commodityLineForDocumentPrint)
		{
			AssertEquals("Export Information Code", invoiceLine.US_ExportCode, commodityLineForDocumentPrint.ExportCode);
			AssertEquals("Line Number", invoiceLine.JI_LineNo.ToString(), commodityLineForDocumentPrint.LineNo);
			AssertEquals("Commodity Description", invoiceLine.JI_Description, commodityLineForDocumentPrint.TariffDescription);
			AssertEquals("License Code/License Exemption Code", invoiceLine.US_LicenseType, commodityLineForDocumentPrint.LicenseType);
			AssertEquals("Foreign/Domestic Origin Indicator", invoiceLine.US_AESOriginIndicator, commodityLineForDocumentPrint.OriginIndicator);
			AssertEquals("Schedule B HTS Number", invoiceLine.JI_Tariff, commodityLineForDocumentPrint.TariffNo);
			AssertEquals("UnitOfMeasure", invoiceLine.JI_CustomsUnitQty, commodityLineForDocumentPrint.UQ);
			AssertEquals("Quantity", invoiceLine.JI_CustomsQuantity, commodityLineForDocumentPrint.Qty);
			AssertEquals("ValueOfGoods", invoiceLine.JI_LinePriceInLocalCurrency, commodityLineForDocumentPrint.Value);
			AssertEquals("ShippingWeight", invoiceLine.JI_Weight, commodityLineForDocumentPrint.GrossWt);
			AssertEquals("ShippingWeight UQ", invoiceLine.JI_WeightUQ, commodityLineForDocumentPrint.GrossWtUQ);
			AssertEquals("ECCN", invoiceLine.US_ECCN, commodityLineForDocumentPrint.ECCN);
			AssertEquals("Export License Number", invoiceLine.US_LicenseNo, commodityLineForDocumentPrint.ExportLicenseNo);
			AssertEquals("Export License Value", invoiceLine.US_LicenseValue.ToString(), commodityLineForDocumentPrint.LicenseValue);

			AssertEquals("ITAR Exemption Number", invoiceLine.US_DDTCITARExemptionNo, commodityLineForDocumentPrint.ITARExemptionNo);
			AssertEquals("Military Equipment Indicator", invoiceLine.US_DDTCMilitaryEquipmentIndicator, commodityLineForDocumentPrint.MilitaryEquipmentIndicator);
			AssertEquals("Party Certification Indicator", invoiceLine.US_DDTCPartyCertificationIndicator, commodityLineForDocumentPrint.PartyCertificationIndicator);
			AssertEquals("Registration Number", invoiceLine.US_DDTCRegistrationNo, commodityLineForDocumentPrint.RegistrationNo);
			AssertEquals("DDTC Qty", invoiceLine.US_DDTCQuantity.ToString(), commodityLineForDocumentPrint.DDTCQuantityWithUnitOfMeasure.Trim());
			AssertEquals("Vehicle ID", invoiceLine.US_VehicleID, commodityLineForDocumentPrint.VehicleID);
			AssertEquals("Vehicle Title Number", invoiceLine.US_VehicleTitleNo, commodityLineForDocumentPrint.VehicleTitleNumber);
		}
	}
}
