using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderCopyBO))]
	class JobComInvoiceHeaderCopyBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var source = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			var destination = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
			return new JobComInvoiceHeaderCopyBO(source, destination);
		}

		public void TestCopyInvoiceHeader_SerialNumber()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();

			var declaration1 = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.JZ_OH_Supplier = supplier.PK;
			invoice1.JZ_OH_Buyer = buyer.PK;
			invoice1.JZ_InvoiceDate = new ZDate(2020, 8, 6);
			invoice1.JZ_InvoiceNumber = "Inv1";
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_IncoTerm = "ABC";
			invoice1.JZ_Volume = 20;
			invoice1.JZ_VolumeUQ = "PC";
			invoice1.JZ_Weight = 123;
			invoice1.JZ_NetWeightUQ = "KG";
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_NoOfPacks = 3;
			invoice1.JZ_IncoTermPlace = "Sydney";
			invoice1.JZ_OH_Consignee = consignee.PK;

			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.JI_PartNo = "1";
			line1.JI_PartAttrib1 = "part1";
			line1.JI_PartAttrib2 = "part2";
			line1.JI_PartAttrib3 = "part3";
			line1.JI_SerialNumber = "serialNumber";
			line1.JI_Description = "desc";
			line1.JI_InvoiceQuantity = 10;
			line1.JI_InvoiceUQ = "KG";
			line1.JI_LinePrice = 100;
			line1.JI_Volume = 10;
			line1.JI_VolumeUQ = "PC";
			line1.JI_WeightUQ = "KG";
			line1.JI_NetWeight = 10;
			line1.JI_NetWeightUQ = "KG";
			line1.JI_CustomsQuantity = 12;
			line1.JI_CustomsUnitQty = "QTY";
			line1.JI_Tariff = "1234";
			line1.JI_CountryOfOrigin = "AU";
			line1.JI_RH_NKCommodity_Code = "com";
			line1.JI_OP = product.PK;
			line1.JI_BrandName = "brand";
			line1.JI_Model = "model";

			var line2 = invoice1.InvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.JI_BrandName = "test brand";
			line2.JI_SecondaryPreference = "second";

			var declaration2 = Factory.New<BaseJobDeclaration>();
			var copiedInvoice = declaration2.Invoices.AddNew();
			var copyBo = new JobComInvoiceHeaderCopyBO(invoice1, copiedInvoice);
			copyBo.CopyInvoice();

			AssertEquals("Supplier should be copied", invoice1.JZ_OH_Supplier, copiedInvoice.JZ_OH_Supplier);
			AssertEquals("Buyer should be copied", invoice1.JZ_OH_Buyer, copiedInvoice.JZ_OH_Buyer);
			AssertEquals("Invoice Date should be copied", invoice1.JZ_InvoiceDate, copiedInvoice.JZ_InvoiceDate);
			AssertEquals("Invoice number should be copied", invoice1.JZ_InvoiceNumber, copiedInvoice.JZ_InvoiceNumber);
			AssertEquals("Invoice amount should be copied", invoice1.JZ_InvoiceAmount, copiedInvoice.JZ_InvoiceAmount);
			AssertEquals("IncoTerm should be copied", invoice1.JZ_IncoTerm, copiedInvoice.JZ_IncoTerm);
			AssertEquals("Volume should be copied", invoice1.JZ_Volume, copiedInvoice.JZ_Volume);
			AssertEquals("VolumeUQ should be copied", invoice1.JZ_VolumeUQ, copiedInvoice.JZ_VolumeUQ);
			AssertEquals("Weight should be copied", invoice1.JZ_Weight, copiedInvoice.JZ_Weight);
			AssertEquals("NetWeightUQ should be copied", invoice1.JZ_NetWeightUQ, copiedInvoice.JZ_NetWeightUQ);
			AssertEquals("Invoice currency should be copied", invoice1.JZ_RX_NKInvoice_Currency, copiedInvoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("No. of Packs should be copied", invoice1.JZ_NoOfPacks, copiedInvoice.JZ_NoOfPacks);
			AssertEquals("Inco Term Place should be copied", invoice1.JZ_IncoTermPlace, copiedInvoice.JZ_IncoTermPlace);
			AssertEquals("Consignee should not be copied", ZGuid.Empty, copiedInvoice.JZ_OH_Consignee);

			AssertEquals("2 invoice lines should be copied", 2, copiedInvoice.InvoiceLines.Count);
			var copiedInvoiceLine1 = copiedInvoice.InvoiceLines[0];

			AssertEquals("Line 1- Invoice line number should be copied", line1.JI_LineNo, copiedInvoiceLine1.JI_LineNo);
			AssertEquals("Line 1- Part number should be copied", line1.JI_PartNo, copiedInvoiceLine1.JI_PartNo);
			AssertEquals("Line 1- PartAttrib1 should be copied", line1.JI_PartAttrib1, copiedInvoiceLine1.JI_PartAttrib1);
			AssertEquals("Line 1- PartAttrib2 should be copied", line1.JI_PartAttrib2, copiedInvoiceLine1.JI_PartAttrib2);
			AssertEquals("Line 1- PartAttrib3 should be copied", line1.JI_PartAttrib3, copiedInvoiceLine1.JI_PartAttrib3);
			AssertEquals("Line 1- SerialNumber", line1.JI_SerialNumber, copiedInvoiceLine1.JI_SerialNumber);
			AssertEquals("Line 1- Description should be copied", line1.JI_Description, copiedInvoiceLine1.JI_Description);
			AssertEquals("Line 1- Invoice Quantity should be copied", line1.JI_InvoiceQuantity, copiedInvoiceLine1.JI_InvoiceQuantity);
			AssertEquals("Line 1- Invoice UQ should be copied", line1.JI_InvoiceUQ, copiedInvoiceLine1.JI_InvoiceUQ);
			AssertEquals("Line 1- Line price should be copied", line1.JI_LinePrice, copiedInvoiceLine1.JI_LinePrice);
			AssertEquals("Line 1- Volume should be copied", line1.JI_Volume, copiedInvoiceLine1.JI_Volume);
			AssertEquals("Line 1- VolumeUQ should be copied", line1.JI_VolumeUQ, copiedInvoiceLine1.JI_VolumeUQ);
			AssertEquals("Line 1- Weight UQ should be copied", line1.JI_WeightUQ, copiedInvoiceLine1.JI_WeightUQ);
			AssertEquals("Line 1- Net Weight should be copied", line1.JI_NetWeight, copiedInvoiceLine1.JI_NetWeight);
			AssertEquals("Line 1- Net Weight UQ should be copied", line1.JI_NetWeightUQ, copiedInvoiceLine1.JI_NetWeightUQ);
			AssertEquals("Line 1- Customs Quantity should be copied", line1.JI_CustomsQuantity, copiedInvoiceLine1.JI_CustomsQuantity);
			AssertEquals("Line 1- Customs Unit Qty should be copied", line1.JI_CustomsUnitQty, copiedInvoiceLine1.JI_CustomsUnitQty);
			AssertEquals("Line 1- Tariff should be copied", line1.JI_Tariff, copiedInvoiceLine1.JI_Tariff);
			AssertEquals("Line 1- Country Of Origin should be copied", line1.JI_CountryOfOrigin, copiedInvoiceLine1.JI_CountryOfOrigin);
			AssertEquals("Line 1- Commodity Code should be copied", line1.JI_RH_NKCommodity_Code, copiedInvoiceLine1.JI_RH_NKCommodity_Code);
			AssertEquals("Line 1- Invoice line product should be copied", line1.JI_OP, copiedInvoiceLine1.JI_OP);
			AssertEquals("Line 1- Brand name should be copied", line1.JI_BrandName, copiedInvoiceLine1.JI_BrandName);
			AssertEquals("Line 1- Model should be copied", line1.JI_Model, copiedInvoiceLine1.JI_Model);

			var copiedInvoiceLine2 = copiedInvoice.InvoiceLines[1];
			AssertEquals("Line 2- Invoice line number should be copied", line2.JI_LineNo, copiedInvoiceLine2.JI_LineNo);
			AssertEquals("Line 2- Invoice line price should be copied", line2.JI_BrandName, copiedInvoiceLine2.JI_BrandName);
			AssertEquals("Second Preference should not be copied", string.Empty, copiedInvoiceLine2.JI_SecondaryPreference);
		}
	}
}
