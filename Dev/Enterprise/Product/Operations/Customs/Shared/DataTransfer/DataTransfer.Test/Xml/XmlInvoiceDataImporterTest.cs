using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class XmlInvoiceDataImporterTest : XmlDeclarationDataImporterTest
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoices()
		{
			OrgHeader org = GetNewSupplierOrg();
			Factory.Save();

			BaseJobDeclaration toJobDec = Factory.New<BaseJobDeclaration>();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Legacy\TestFiles\CommercialInvoice.xml");
			XmlInvoiceDataImporter importer = new XmlInvoiceDataImporter(xmlDoc, toJobDec);
			importer.Import();
			BaseJobComInvoiceHeader invHead = toJobDec.Invoices[0];
			AssertEquals("INV NO 1", invHead.JZ_InvoiceNumber);
			AssertEquals(23.44M, invHead.JZ_InvoiceAmount);
			AssertEquals("ZAR", invHead.Invoice_Currency.RX_Code);
			AssertEquals(new ZDateTime(2004, 2, 15), invHead.JZ_InvoiceDate);
			AssertEquals(new ZDateTime(2004, 2, 16), invHead.JZ_ValuationDateOverride);
			AssertEquals(false, invHead.JZ_GroupInvoice);

			AssertEquals("DDU", invHead.JZ_IncoTerm);
			AssertEquals(org.PK, invHead.JZ_OH_Supplier);
			AssertEquals(99.88M, invHead.JZ_Volume);
			AssertEquals("M3", invHead.JZ_VolumeUQ);
			AssertEquals(11.11M, invHead.JZ_Weight);
			AssertEquals("KG", invHead.JZ_WeightUQ);

			JobComInvCharge[] charges = invHead.Charges.GetCharge("COM");
			AssertEquals(1, charges.Length);
			JobComInvCharge charge = charges[0];
			AssertEquals("COM", charge.J7_ChargeType);
			AssertEquals("AUD", charge.J7_RX_NKCurrency);
			AssertEquals(7.89M, charge.J7_Amount);

			charges = invHead.Charges.GetCharge("ADD");
			AssertEquals(1, charges.Length);
			charge = charges[0];
			AssertEquals("ADD", charge.J7_ChargeType);
			AssertEquals("USD", charge.J7_RX_NKCurrency);
			AssertEquals(9.65M, charge.J7_Amount);
			AssertEquals(true, charge.J7_IsGSTApplicable);
			AssertEquals(true, charge.J7_IsDutiable);
			AssertEquals(false, charge.J7_IsIncludedInITOT);

			AssertEquals(2, toJobDec.InvoiceLines.Count);
			ZQuery filter = new ZQuery(JobComInvoiceLineSchema.JI_PartNo, "Prod No 1");
			BusinessObject[] invLines = toJobDec.InvoiceLines.Find(filter);
			AssertEquals(1, invLines.Length);
			BaseJobComInvoiceLine invLine = (BaseJobComInvoiceLine)invLines[0];
			AssertEquals("Prod No 1", invLine.JI_PartNo);
			AssertEquals("Desc 1", invLine.JI_Description);
			AssertEquals(44.25M, invLine.JI_InvoiceQuantity);
			AssertEquals("KG", invLine.JI_InvoiceUQ);
			AssertEquals(25.33M, invLine.JI_CustomsQuantity);
			AssertEquals("KG", invLine.JI_CustomsUnitQty);
			AssertEquals(875.35M, invLine.JI_LinePrice);
			AssertEquals("ZAR", invLine.LinePriceRefCurrency.RX_Code);
			AssertEquals("Ord No 1", invLine.JI_OrderNumber);
			AssertEquals(12.21M, invLine.JI_Volume);
			AssertEquals("M3", invLine.JI_VolumeUQ);
			AssertEquals(354.25M, invLine.JI_Weight);
			AssertEquals("KG", invLine.JI_WeightUQ);
			AssertEquals("KR", invLine.JI_CountryOfOrigin);
			AssertEquals("Concession", invLine.JI_ConcessionOrder);
			AssertEquals("CustomText1_1", invLine.JI_CustomAttrib1);
			AssertEquals("CustomText2_1", invLine.JI_CustomAttrib2);
			AssertEquals("CustomText3_1", invLine.JI_CustomAttrib3);

			filter = new ZQuery(JobComInvoiceLineSchema.JI_PartNo, "Prod No 2");
			invLines = toJobDec.InvoiceLines.Find(filter);
			AssertEquals(1, invLines.Length);
			invLine = (BaseJobComInvoiceLine)invLines[0];
			AssertEquals("Prod No 2", invLine.JI_PartNo);
			AssertEquals("Desc 2", invLine.JI_Description);
			AssertEquals(42.25M, invLine.JI_InvoiceQuantity);
			AssertEquals("LB", invLine.JI_InvoiceUQ);
			AssertEquals(25M, invLine.JI_CustomsQuantity);
			AssertEquals("NO", invLine.JI_CustomsUnitQty);
			AssertEquals(75.45M, invLine.JI_LinePrice);
			AssertEquals("ZAR", invLine.LinePriceRefCurrency.RX_Code);
			AssertEquals("Ord No 2", invLine.JI_OrderNumber);
			AssertEquals(34.43M, invLine.JI_Volume);
			AssertEquals(85.58M, invLine.JI_Weight);
			AssertEquals("M3", invLine.JI_VolumeUQ);
			AssertEquals("KG", invLine.JI_WeightUQ);
			AssertEquals("CN", invLine.JI_CountryOfOrigin);
			AssertEquals("Concession 2", invLine.JI_ConcessionOrder);
			AssertEquals("CustomText1_2", invLine.JI_CustomAttrib1);
			AssertEquals("CustomText2_2", invLine.JI_CustomAttrib2);
			AssertEquals("CustomText3_2", invLine.JI_CustomAttrib3);
		}

		public void TestDescriptionIsDefaultedFromProductIfNoneSpecified()
		{
			OrgHeader org = GetNewSupplierOrg();
			MasterFiles.Business.OrgSupplierPart part = GetNewPart("PART", "PARTDESC", org);
			Factory.Save();

			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_OH_Supplier = org.PK;

			string xmlForImport =
				"<Invoices>" +
				"<InvoiceHeader>" +
				"<InvoiceNumber>5008446</InvoiceNumber>" +
				"<InvoiceLines>" +
				"<InvoiceLine>" +
				"<ProductNumber>PART</ProductNumber>" +
				"</InvoiceLine>" +
				"</InvoiceLines>" +
				"</InvoiceHeader>" +
				"</Invoices>";

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlForImport);
			XmlInvoiceDataImporter importer = new XmlInvoiceDataImporter(xmlDoc, jobDec);
			importer.Import();
			BaseJobComInvoiceLine invLine = jobDec.Invoices[0].JobComInvoiceLines[0];
			AssertEquals("PART", invLine.JI_PartNo);
			AssertEquals("PARTDESC", invLine.JI_Description);
		}

		OrgHeader GetNewSupplierOrg()
		{
			OrgHeader result = OrgHeader.New(Factory);
			result.OH_Code = "TESTORG";
			result.OH_IsConsignor = true;
			return result;
		}

		MasterFiles.Business.OrgSupplierPart GetNewPart(string partNum, string description, OrgHeader supplier)
		{
			MasterFiles.Business.OrgSupplierPart result = MasterFiles.Business.OrgSupplierPart.New(Factory);
			result.OP_PartNum = partNum;
			result.OP_Desc = description;
			result.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			return result;
		}
	}
}
