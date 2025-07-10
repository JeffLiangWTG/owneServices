using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFInvoiceImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.OH_Code = "TSTMAN1";
			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.OH_Code = "TSTMAN2";
			var manufacturer3 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer3.OH_Code = "TSTMAN3";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TSTIMP";

			manufacturer1.MainAddress.Address1 = "Address 1";
			manufacturer2.MainAddress.Address1 = "Address 1";
			manufacturer3.MainAddress.Address1 = "Address 1";

			var manufacturer1SecondAddress = manufacturer1.Addresses.AddNew();
			manufacturer1SecondAddress.Address1 = "Address 2";

			var part1 = Factory.New<US.Business.OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			part1.OP_Desc = "PART1";
			part1.RelatedOrganisations.AddOwner(importer);

			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;

			var invoice1 = Factory.New<JobComInvoiceHeader>();
			{
				invoice1.InvoiceHeaderRefs.AddNew("CN", "10001");
				invoice1.InvoiceHeaderRefs.AddNew("CN", "10002A");
				invoice1.InvoiceHeaderRefs.AddNew("CN", "10002A");
				invoice1.InvoiceHeaderRefs.AddNew("CN", "10003B");
				invoice1.InvoiceHeaderRefs.AddNew("HB", "20001");
				invoice1.InvoiceHeaderRefs.AddNew("MB", "30001");

				var invoice1Line1 = invoice1.InvoiceLines.AddNew();
				invoice1Line1.JI_PartNo = "PART1";
				invoice1Line1.JI_PartAttrib1 = "AT1-1";
				invoice1Line1.JI_PartAttrib2 = "AT2-1";
				invoice1Line1.JI_PartAttrib3 = "AT3-1";
				invoice1Line1.US_UC_NKCountryOfOrigin = "AU";
				invoice1Line1.JI_Tariff = "123456789";
				invoice1Line1.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice1Line1.JI_LinePrice = 1.0m;

				var invoice1Line2 = invoice1.InvoiceLines.AddNew();
				invoice1Line2.JI_PartNo = "PART1";
				invoice1Line2.JI_PartAttrib1 = "AT1-1";
				invoice1Line2.JI_PartAttrib2 = "AT2-1";
				invoice1Line2.JI_PartAttrib3 = "AT3-1";
				invoice1Line2.US_UC_NKCountryOfOrigin = "AU";
				invoice1Line2.JI_Tariff = "123456789";
				invoice1Line2.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice1Line2.JI_LinePrice = 2.0m;

				var invoice1Line3 = invoice1.InvoiceLines.AddNew();
				invoice1Line3.JI_PartNo = "PART2";
				invoice1Line3.JI_PartAttrib1 = "AT1-3";
				invoice1Line3.JI_PartAttrib2 = "AT2-3";
				invoice1Line3.JI_PartAttrib3 = "AT3-3";
				invoice1Line3.US_UC_NKCountryOfOrigin = "SG";
				invoice1Line3.JI_Tariff = "234567890";
				invoice1Line3.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.EntityPK;
				invoice1Line3.JI_LinePrice = 3.0m;
			}

			// running import twice, second import should not add any new data
			for (int i = 1; i <= 2; i++)
			{
				new ISFInvoiceImporter().Import(header, invoice1);

				AssertEquals("imported invoice should not be attached to declaration", ZGuid.Empty, invoice1.JZ_JE);
				AssertEquals("lines should not be removed from original invoice", 3, invoice1.InvoiceLines.Count);
				AssertEquals("references should not be removed from original invoice", 6, invoice1.InvoiceHeaderRefs.Count);

				AssertMultilineASCIIEquals(
					$"run #{i}",
					string.Join("\r\n", new string[]
					{
						"PART1 (PART1), AT1-1, AT2-1, AT3-1, AU, 1234.56.789, TSTMAN1 Address 1",
						"PART2 (-----), AT1-3, AT2-3, AT3-3, SG, 2345.67.890, TSTMAN2 Address 1"
					}.OrderBy(t => t)),
					string.Join("\r\n", header.Lines.Select(l =>
						$"{l.BL_TextProductCode} ({l.SupplierPart?.OP_PartNum ?? "-----"}), {l.BL_PartAttrib1}, {l.BL_PartAttrib2}, {l.BL_PartAttrib3}, {l.BL_RN_NKGoodsOrigin}, {l.BL_FormattedHarmonisedNum}, {l.ManufacturerDocAddress?.Organisation.OH_Code} {l.ManufacturerDocAddress?.Address1}"
					).OrderBy(t => t))
				);

				AssertMultilineASCIIEquals(
					$"run #{i}",
					string.Join("\r\n", new string[]
					{
						"CN, 10001",
						"CN, 10002A",
						"CN, 10003B"
					}.OrderBy(t => t)),
					string.Join("\r\n", header.Equipments.Select(l =>
						$"{l.BE_EquipCode}, {l.BE_ContainerNum}"
					).OrderBy(t => t))
				);

				AssertMultilineASCIIEquals(
					$"run #{i}",
					string.Join("\r\n", new string[]
					{
						"TSTMAN1 Address 1",
						"TSTMAN2 Address 1"
					}.OrderBy(t => t)),
					string.Join("\r\n", header.DocAddresses.OfType<ISFDocAddress>().Where(l => l.DocAddressType == DocAddressType.Manufacturer).Select(l =>
						$"{l.Organisation?.OH_Code} {l.Address1}"
					).OrderBy(t => t))
				);
			}

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			{
				invoice2.InvoiceHeaderRefs.AddNew("CN", "10002A");
				invoice2.InvoiceHeaderRefs.AddNew("CN", "10003b");
				invoice2.InvoiceHeaderRefs.AddNew("CN", "10004C");
				invoice2.InvoiceHeaderRefs.AddNew("HB", "20002");
				invoice2.InvoiceHeaderRefs.AddNew("MB", "30002");

				// one matching line and several lines that differ in one parameter only

				var invoice2Line1 = invoice2.InvoiceLines.AddNew();
				invoice2Line1.JI_PartNo = "PART1";
				invoice2Line1.JI_PartAttrib1 = "AT1-1";
				invoice2Line1.JI_PartAttrib2 = "AT2-1";
				invoice2Line1.JI_PartAttrib3 = "AT3-1";
				invoice2Line1.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line1.JI_Tariff = "123456789";
				invoice2Line1.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line1.JI_LinePrice = 1.0m;

				var invoice2Line2 = invoice2.InvoiceLines.AddNew();
				invoice2Line2.JI_PartNo = "PART3";
				invoice2Line2.JI_PartAttrib1 = "AT1-1";
				invoice2Line2.JI_PartAttrib2 = "AT2-1";
				invoice2Line2.JI_PartAttrib3 = "AT3-1";
				invoice2Line2.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line2.JI_Tariff = "123456789";
				invoice2Line2.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line2.JI_LinePrice = 1.0m;

				var invoice2Line3 = invoice2.InvoiceLines.AddNew();
				invoice2Line3.JI_PartNo = "PART1";
				invoice2Line3.JI_PartAttrib1 = "AT1-2";
				invoice2Line3.JI_PartAttrib2 = "AT2-1";
				invoice2Line3.JI_PartAttrib3 = "AT3-1";
				invoice2Line3.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line3.JI_Tariff = "123456789";
				invoice2Line3.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line3.JI_LinePrice = 1.0m;

				var invoice2Line4 = invoice2.InvoiceLines.AddNew();
				invoice2Line4.JI_PartNo = "PART1";
				invoice2Line4.JI_PartAttrib1 = "AT1-1";
				invoice2Line4.JI_PartAttrib2 = "AT2-2";
				invoice2Line4.JI_PartAttrib3 = "AT3-1";
				invoice2Line4.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line4.JI_Tariff = "123456789";
				invoice2Line4.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line4.JI_LinePrice = 1.0m;

				var invoice2Line5 = invoice2.InvoiceLines.AddNew();
				invoice2Line5.JI_PartNo = "PART1";
				invoice2Line5.JI_PartAttrib1 = "AT1-1";
				invoice2Line5.JI_PartAttrib2 = "AT2-1";
				invoice2Line5.JI_PartAttrib3 = "AT3-2";
				invoice2Line5.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line5.JI_Tariff = "123456789";
				invoice2Line5.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line5.JI_LinePrice = 1.0m;

				var invoice2Line6 = invoice2.InvoiceLines.AddNew();
				invoice2Line6.JI_PartNo = "PART1";
				invoice2Line6.JI_PartAttrib1 = "AT1-1";
				invoice2Line6.JI_PartAttrib2 = "AT2-1";
				invoice2Line6.JI_PartAttrib3 = "AT3-1";
				invoice2Line6.US_UC_NKCountryOfOrigin = "JP";
				invoice2Line6.JI_Tariff = "123456789";
				invoice2Line6.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line6.JI_LinePrice = 1.0m;

				var invoice2Line7 = invoice2.InvoiceLines.AddNew();
				invoice2Line7.JI_PartNo = "PART1";
				invoice2Line7.JI_PartAttrib1 = "AT1-1";
				invoice2Line7.JI_PartAttrib2 = "AT2-1";
				invoice2Line7.JI_PartAttrib3 = "AT3-1";
				invoice2Line7.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line7.JI_Tariff = "345678901";
				invoice2Line7.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.EntityPK;
				invoice2Line7.JI_LinePrice = 1.0m;

				var invoice2Line8 = invoice2.InvoiceLines.AddNew();
				invoice2Line8.JI_PartNo = "PART1";
				invoice2Line8.JI_PartAttrib1 = "AT1-1";
				invoice2Line8.JI_PartAttrib2 = "AT2-1";
				invoice2Line8.JI_PartAttrib3 = "AT3-1";
				invoice2Line8.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line8.JI_Tariff = "123456789";
				invoice2Line8.JI_OA_ManufacturerAddress = manufacturer3.MainAddress.EntityPK;
				invoice2Line8.JI_LinePrice = 1.0m;

				var invoice2Line9 = invoice2.InvoiceLines.AddNew();
				invoice2Line9.JI_PartNo = "PART1";
				invoice2Line9.JI_PartAttrib1 = "AT1-1";
				invoice2Line9.JI_PartAttrib2 = "AT2-1";
				invoice2Line9.JI_PartAttrib3 = "AT3-1";
				invoice2Line9.US_UC_NKCountryOfOrigin = "AU";
				invoice2Line9.JI_Tariff = "123456789";
				invoice2Line9.JI_OA_ManufacturerAddress = manufacturer1SecondAddress.EntityPK;
				invoice2Line9.JI_LinePrice = 1.0m;
			}

			new ISFInvoiceImporter().Import(header, invoice2);

			AssertEquals("imported invoice should not be attached to declaration", ZGuid.Empty, invoice2.JZ_JE);
			AssertEquals("lines should not be removed from original invoice", 9, invoice2.InvoiceLines.Count);
			AssertEquals("references should not be removed from original invoice", 5, invoice2.InvoiceHeaderRefs.Count);

			AssertMultilineASCIIEquals(
				string.Join("\r\n", new string[]
				{
					"PART1 (PART1), AT1-1, AT2-1, AT3-1, AU, 1234.56.789, TSTMAN1 Address 1",
					"PART2 (-----), AT1-3, AT2-3, AT3-3, SG, 2345.67.890, TSTMAN2 Address 1",
					"PART3 (-----), AT1-1, AT2-1, AT3-1, AU, 1234.56.789, TSTMAN1 Address 1",
					"PART1 (PART1), AT1-2, AT2-1, AT3-1, AU, 1234.56.789, TSTMAN1 Address 1",
					"PART1 (PART1), AT1-1, AT2-2, AT3-1, AU, 1234.56.789, TSTMAN1 Address 1",
					"PART1 (PART1), AT1-1, AT2-1, AT3-2, AU, 1234.56.789, TSTMAN1 Address 1",
					"PART1 (PART1), AT1-1, AT2-1, AT3-1, JP, 1234.56.789, TSTMAN1 Address 1",
					"PART1 (PART1), AT1-1, AT2-1, AT3-1, AU, 3456.78.901, TSTMAN1 Address 1",
					"PART1 (PART1), AT1-1, AT2-1, AT3-1, AU, 1234.56.789, TSTMAN3 Address 1",
					"PART1 (PART1), AT1-1, AT2-1, AT3-1, AU, 1234.56.789, TSTMAN1 Address 2"
				}.OrderBy(t => t)),
				string.Join("\r\n", header.Lines.Select(l =>
					$"{l.BL_TextProductCode} ({l.SupplierPart?.OP_PartNum ?? "-----"}), {l.BL_PartAttrib1}, {l.BL_PartAttrib2}, {l.BL_PartAttrib3}, {l.BL_RN_NKGoodsOrigin}, {l.BL_FormattedHarmonisedNum}, {l.ManufacturerDocAddress?.Organisation.OH_Code} {l.ManufacturerDocAddress?.Address1}"
				).OrderBy(t => t))
			);

			AssertMultilineASCIIEquals(
				string.Join("\r\n", new string[]
				{
					"CN, 10001",
					"CN, 10002A",
					"CN, 10003B",
					"CN, 10004C"
				}.OrderBy(t => t)),
				string.Join("\r\n", header.Equipments.Select(l =>
					$"{l.BE_EquipCode}, {l.BE_ContainerNum}"
				).OrderBy(t => t))
			);

			AssertMultilineASCIIEquals(
				string.Join("\r\n", new string[]
				{
					"TSTMAN1 Address 1",
					"TSTMAN1 Address 2",
					"TSTMAN2 Address 1",
					"TSTMAN3 Address 1"
				}.OrderBy(t => t)),
				string.Join("\r\n", header.DocAddresses.OfType<ISFDocAddress>().Where(l => l.DocAddressType == DocAddressType.Manufacturer).Select(l =>
					$"{l.Organisation?.OH_Code} {l.Address1}"
				).OrderBy(t => t))
			);
		}
	}
}
