using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class OrgSupplierPartBaseOnlyTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestTariffs()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_TariffNum = "000";
			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = "111";
			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_TariffNum = "222";

			AssertEquals("HTB:000, HTB:111, HTB:222", product.Tariffs);

			var pivot4 = product.PivotsForBinding.AddNew();
			pivot4.CI_TariffNum = "333";

			Assert(product.Tariffs.Contains("..."));
		}

		public void TestLoad_IsExportJob()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var buyer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABABEU");

			declaration.JE_OH_Importer = buyer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var part1PK = SaveNewPart(Factory, "PART", buyer, null, "DESC1");
			var part2PK = SaveNewPart(Factory, "PART", null, supplier, "DESC2");

			_ = CreateInvoiceLineForPart(Factory, declaration, "PART", buyer.PK, supplier.PK);

			var loader = new OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart));
			AssertEquals(part2PK, loader.Load("PART", buyer, supplier, false, false, true).PK);
			AssertEquals(part1PK, loader.Load("PART", buyer, supplier, false, false, false).PK);
		}

		public void TestClearDataAndDeleteChildren()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SDK232KSD";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "10TEST32";
			var relatedOrg = product.RelatedOrganisations.AddOwner(org);
			Factory.Save();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1";
			var partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "1";
			var location = product.Locations.AddNew();
			location.OR_Warehouse = "1";
			var billOfMaterial = product.BillOfMaterials.AddNew();
			billOfMaterial.OE_F3_NKPackType = "A";
			billOfMaterial.OE_OP_Component = product.PK;
			var partBarcode = product.PartBarcodes.AddNew();
			partBarcode.PH_Barcode = "AProduct";
			partBarcode.PH_F3_NKPackType = "A";
			var workflow = product.WorkflowItems.AddNew();
			workflow.P9_Description = "A";
			Factory.Save();
			var systemCreateTimeUtc = product.OP_SystemCreateTimeUtc;
			var systemCreateUser = product.OP_SystemCreateUser;
			var systemLastEditTimeUtc = product.OP_SystemLastEditTimeUtc;
			var systemLastEditUser = product.OP_SystemLastEditUser;
			product.ClearDataAndDeleteChildren();
			AssertEquals("product.OP_PartNum", "10TEST32", product.OP_PartNum);
			AssertEquals("product.OP_SystemCreateUser", systemCreateUser, product.OP_SystemCreateUser);
			AssertEquals("product.OP_SystemCreateTimeUtc", systemCreateTimeUtc, product.OP_SystemCreateTimeUtc);
			AssertEquals("product.OP_SystemLastEditTimeUtc", systemLastEditTimeUtc, product.OP_SystemLastEditTimeUtc);
			AssertEquals("product.OP_SystemLastEditUser", systemLastEditUser, product.OP_SystemLastEditUser);
			AssertEquals("product.OP_IsActive", ZBool.True, product.OP_IsActive);
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
			AssertEquals("partUnit.IsDeleted", true, partUnit.IsDeleted);
			AssertEquals("location.IsDeleted", true, location.IsDeleted);
			AssertEquals("billOfMaterial.IsDeleted", true, billOfMaterial.IsDeleted);
			AssertEquals("relatedOrg.IsDeleted", true, relatedOrg.IsDeleted);
			AssertEquals("partBarcode.IsDeleted", true, partBarcode.IsDeleted);
			AssertEquals("workflow.IsDeleted", true, workflow.IsDeleted);
		}

		public void TestDoesPartMatchPivotForInactiveCheck()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var part = Factory.New<OrgSupplierPart>();
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "";
			AssertEquals(false, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_ChildType = "HTE";
			AssertEquals(false, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_ChildType = "HTI";
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_ChildType = "HTB";
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_RN_NKCountry = "!@";
			AssertEquals(false, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_ChildType = "#@1";
			AssertEquals(false, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			pivot.CI_ChildType = invoiceLine.GetPartPivotType();
			AssertEquals(true, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));

			_ = part.PivotsForBinding.AddNew();
			AssertEquals(false, part.DoesPartMatchPivotForInactiveCheck(invoiceLine));
		}

		#region Implementation

		#region ProductMatchingSetup
		BaseJobComInvoiceLine CreateInvoiceLineForPart(BusinessObjectFactory factory, BaseJobDeclaration declaration, ZString partNum, ZGuid importerPK, params ZGuid[] supplierPKs)
		{
			declaration.JE_OH_Importer = importerPK;

			BaseJobComInvoiceLine result = null;
			var firstHeader = true;
			foreach (ZGuid supplierPK in supplierPKs)
			{
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				if (firstHeader)
				{
					result = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				invoiceHeader.JZ_OH_Supplier = supplierPK;
				firstHeader = false;
			}
			result.JI_PartNo = partNum;
			return result;
		}

		ZGuid SaveNewPart(BusinessObjectFactory factory, ZString partNum, OrgHeader importer, OrgHeader supplier, ZString description)
		{
			var newPart = factory.New<OrgSupplierPart>();
			if (importer != null)
			{
				newPart.RelatedOrganisations.AddOwner(importer);
			}
			if (supplier != null)
			{
				newPart.RelatedOrganisations.AddSupplier(supplier);
			}
			newPart.OP_PartNum = partNum;
			newPart.OP_Desc = description;
			factory.Save();
			return newPart.PK;
		}
		#endregion

		#endregion
	}
}
