using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceLinePartSynchronisationManagerTest : TestCaseWithFactory
	{
		public void TestLoadingCountrySpecificPartFromDifferentCountry_WI00324523()
		{
			var companyLV = Factory.New<GlbCompany>();
			companyLV.GC_Code = "CL1";
			companyLV.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var companyLVBranch = companyLV.Branches.AddNew();
			companyLVBranch.GB_Code = "BL1";

			var client = Importer;
			var partPK = SaveNewPart(Factory, "PARTNUM", client, null, "DESC").PK;
			BaseJobDeclaration declaration = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.EU.IJobDeclaration>();
				declaration.JE_GB = companyLVBranch.PK;
				declaration.JE_OH_Importer = client.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "PARTNUM";
				var part = invoiceLine.Part;
				AssertEquals(partPK, part.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.IOrgSupplierPart>(), part.GetType());
			}
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var newFactory = new BusinessObjectFactory();
				var partCN = newFactory.Load<OrgSupplierPart>(partPK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.CN.IOrgSupplierPart>(), partCN.GetType());
				declaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.IJobDeclaration>(), declaration.GetType());
				var invoice = declaration.Invoices[0];
				var invoiceLine = invoice.JobComInvoiceLines[0];
				var part = invoiceLine.Part;
				AssertEquals(partPK, part.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.EU.IOrgSupplierPart>(), part.GetType());
			}
		}

		public void TestCalculatePart()
		{
			Factory.RefreshEnabled = true;
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			var factory2 = new BusinessObjectFactory();
			SaveNewPart(factory2, "PARTNUM", Importer, null, "DESC");

			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestTotalMatchCount()
		{
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var importer3 = Factory.NewWithValidTestData<OrgHeader>();

			SaveNewPart(Factory, "TestProduct", importer2, Supplier, "TestDescription");
			SaveNewPart(Factory, "TestProduct", importer3, Supplier, "TestDescription2");

			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var invLine1 = CreateInvoiceLine(Factory, "TestProduct", Importer.PK, Supplier.PK);
			var syncManager1 = invLine1.PartSyncManager;
			AssertEquals("syncManager.TotalMatchCount", 0, syncManager1.TotalMatchCount);

			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var invLine2 = CreateInvoiceLine(Factory, "TestProduct", Importer.PK, Supplier.PK);
			var syncManager2 = invLine2.PartSyncManager;
			AssertEquals("There are two products found", 2, syncManager2.TotalMatchCount);

			var product3PK = SaveNewPart(Factory, "TestProduct", Importer, Supplier, "TestDescription").PK;
			syncManager2.CalculatePart();
			AssertEquals("SyncManager.Supplier", product3PK, syncManager2.Part.PK);
			AssertEquals("There should be only one count for this", 1, syncManager2.TotalMatchCount);
		}

		public void TestTotalNumberOfPartsCount()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var importer3 = Factory.NewWithValidTestData<OrgHeader>();

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.FillWithValidTestData();

			var supplier3 = Factory.New<OrgHeader>();
			supplier3.FillWithValidTestData();

			SaveNewPart(Factory, "TestProduct", importer2, supplier2, "TestDescription");
			SaveNewPart(Factory, "TestProduct", importer3, supplier3, "TestDescription2");
			SaveNewPart(Factory, "TestProduct", Importer, Supplier, "TestDescription");

			var invLine = CreateInvoiceLine(Factory, "TestProduct", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			syncManager.CalculatePart();
			AssertEquals("syncManager.TotalNumberOfPartsCount", 3, syncManager.TotalNumberOfPartsCount);

			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			syncManager.CalculatePart();
			AssertEquals("syncManager.TotalNumberOfPartsCount", 3, syncManager.TotalNumberOfPartsCount);
		}

		public void TestPartReloadsIfTheImporterChanges()
		{
			SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");

			var invLine = CreateInvoiceLine(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.Declaration.JE_OH_Importer = Importer.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPreferentialOrder_Import()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var part1PK = SaveNewPart(Factory, "PART", Importer, null, "DESC1").PK;
			_ = SaveNewPart(Factory, "PART", null, Supplier, "DESC2").PK;

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(part1PK, syncManager.PartPK);
		}

		public void TestEnableExactMatchForProduct_Import()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var differentSupplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			_ = SaveNewPart(Factory, "PART", Importer, Supplier, "DESC1");

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, differentSupplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(ZGuid.Empty, syncManager.PartPK);
		}

		public void TestPreferentialOrder_DualImport()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			ZGuid part3PK;
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				SaveNewPart(Factory, "PART", Importer, null, "DESC1");
				SaveNewPart(Factory, "PART", null, Supplier, "DESC2");
				part3PK = SaveNewPart(Factory, "PART", Importer, Supplier, "DESC3").PK;
			}

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(part3PK, syncManager.PartPK);
		}

		public void TestPreferentialOrder_DualImportWithExactMatch()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			ZGuid part3PK;
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				SaveNewPart(Factory, "PART", Importer, null, "DESC1");
				SaveNewPart(Factory, "PART", null, Supplier, "DESC2");
				part3PK = SaveNewPart(Factory, "PART", Importer, Supplier, "DESC3").PK;
			}

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(part3PK, syncManager.PartPK);
		}

		public void TestPreferentialOrder_Export()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			SaveNewPart(Factory, "PART", Importer, null, "DESC1");
			var part2PK = SaveNewPart(Factory, "PART", null, Supplier, "DESC2").PK;

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(part2PK, syncManager.PartPK);
		}

		public void TestEnableExactMatchForProduct_Export()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var differentImporter = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			SaveNewPart(Factory, "PART", Importer, null, "DESC1");

			var invLine = CreateInvoiceLine(Factory, "PART", differentImporter.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(ZGuid.Empty, syncManager.PartPK);
		}

		public void TestPreferentialOrder_DualExport()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			ZGuid part3PK;
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				SaveNewPart(Factory, "PART", Importer, null, "DESC1");
				SaveNewPart(Factory, "PART", null, Supplier, "DESC2");
				part3PK = SaveNewPart(Factory, "PART", Importer, Supplier, "DESC3").PK;
			}

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(part3PK, syncManager.PartPK);
		}

		public void TestPreferentialOrder_DualExportWithExactMatch()
		{
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			ZGuid part3PK;
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				SaveNewPart(Factory, "PART", Importer, null, "DESC1");
				SaveNewPart(Factory, "PART", null, Supplier, "DESC2");
				part3PK = SaveNewPart(Factory, "PART", Importer, Supplier, "DESC3").PK;
			}

			var invLine = CreateInvoiceLine(Factory, "PART", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertEquals(part3PK, syncManager.PartPK);
		}

		public void TestPartReloadsIfTheSupplierChanges()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");

			var invLine = CreateInvoiceLine(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.InvoiceHeader.JZ_OH_Supplier = Supplier.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfThePartNumberChanges()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");

			var invLine = CreateInvoiceLine(Factory, ZString.Empty, ZGuid.Empty, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.JI_PartNo = "PARTNUM";
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfTheInvoiceHeaderChanges()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");

			var invLine = CreateInvoiceLine(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.RemoveAll();
			invLine.JI_JZ = invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartDoesntReloadsIfTheSupplierChangesOnADifferentInvoice()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");

			var invLine = CreateInvoiceLine(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.RemoveAll();
			invLine.JI_JZ = invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].PK;
			AssertNull(syncManager.Part);
			invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier = Supplier.PK;

			AssertNull(syncManager.Part);
		}

		public void TestCollectionReloadsIfTheSupplierChangesAfterTheInvoiceHasChanged()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");

			var invLine = CreateInvoiceLine(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.RemoveAll();
			invLine.JI_JZ = invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].PK;
			invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Add(invLine);
			AssertNull(syncManager.Part);
			invLine.Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JZ_OH_Supplier = Supplier.PK;

			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPart()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC");
			AssertNotNull("Part", syncManager.Part);

			syncManager.ClearPart();
			AssertNull("Part", syncManager.Part);
		}

		public void TestOnlySetJI_OP()
		{
			var invoiceLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invoiceLine.PartSyncManager;
			syncManager.Enabled = false;
			var newPartPK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC").PK;
			AssertEquals("Precondition:", ZGuid.Empty, invoiceLine.JI_OP);

			syncManager.OnlySetPartPK();
			AssertEquals("JI_OP should not be set when PartSyncManager is disabled", ZGuid.Empty, invoiceLine.JI_OP);

			syncManager.Enabled = true;
			syncManager.OnlySetPartPK();
			AssertEquals("JI_OP should be set when PartSyncManager is enabled", newPartPK, invoiceLine.JI_OP);
		}

		public void TestEnabled()
		{
			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			invoice1.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			Assert("PartSyncManager.Enabled", invoiceLine1.PartSyncManager.Enabled);

			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			Assert("PartSyncManager.Enabled", !invoiceLine2.PartSyncManager.Enabled);

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var invoice = anotherFactory.Load<BaseJobComInvoiceHeader>(invoice1.PK);
			var invoiceLine = invoice.BusinessObjectsWithRelatedEvents[0] as BaseJobComInvoiceLine;
			Assert("invoiceLine's PartSyncManager.Enable (load from BusinessObjectsWithRelatedEvents) should be same to invoiceLine1's", invoiceLine.PartSyncManager.Enabled);

			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			Assert("invoiceLine's PartSyncManager.Enable should be false", !invoiceLine.PartSyncManager.Enabled);
		}

		public void TestPartSynchronisesIfChangedInAnotherFactory()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			var partPK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC").PK;
			AssertEquals("Part Description", "DESC", syncManager.Part.OP_Desc);
			ChangePartDescription(partPK, "NEWDESC");
			AssertEquals("Part Description", "NEWDESC", syncManager.Part.OP_Desc);
		}

		public void TestInvoiceSyncsIfPartCreatedInAnotherFactory()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC");
			AssertEquals("Line Description", "DESC", invLine.JI_Description);
		}

		public void TestInvoiceSyncsIfPartChangedInAnotherFactory()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			AssertEquals(ZGuid.Empty, invLine.JI_OP);
			var partPK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC").PK;

			AssertEquals(partPK, invLine.JI_OP);
			AssertEquals("Line Description", "DESC", invLine.JI_Description);
			ChangePartDescription(partPK, "NEWDESC");
			AssertEquals("Line Description", "NEWDESC", invLine.JI_Description);
		}

		public void TestInvoiceSyncsIfPartChangedInAnotherFactory2()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			var partPK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC").PK;
			var part2PK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM2", Importer, null, "DESC2").PK;

			AssertEquals("Line Description", "DESC", invLine.JI_Description);
			ChangePartDescription(partPK, "NEWDESC");
			AssertEquals("Line Description", "NEWDESC", invLine.JI_Description);
			invLine.JI_PartNo = "PARTNUM2";
			AssertEquals("Line Description", "DESC2", invLine.JI_Description);
			ChangePartDescription(part2PK, "NEWDESC2");
			AssertEquals("Line Description", "NEWDESC2", invLine.JI_Description);
			invLine.JI_PartNo = "PARTNUM";
			AssertEquals("Line Description", "NEWDESC", invLine.JI_Description);
			ChangePartDescription(part2PK, "NEWNEWDESC2");
			AssertEquals("Line Description", "NEWDESC", invLine.JI_Description);
		}

		public void TestSynchroniseDoesNotUpdateWhenSamePartForLoadedInvoiceLineWhenAutoRefreshTurnedOff()
		{
			using (CustomsDataRegistry.Instance.EnableAutoRefreshProductData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
				var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
				Factory.Save();
				var secondFactory = new BusinessObjectFactory();
				var secondFactoryDec = secondFactory.Load<BaseJobDeclaration>(invLine.Declaration.PK);
				var secondFactoryLine = secondFactoryDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
				JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(secondFactory, secondFactoryLine.PartSyncManagerActiveDeciderPK);

				ChangePartDescription(partPK, "NEWDESC");
				AssertEquals("Line Description", "DESC", secondFactoryLine.JI_Description);
			}
		}

		public void TestSynchroniseDoesUpdateWhenSamePartForLoadedInvoiceLineWhenForceRefresh()
		{
			using (CustomsDataRegistry.Instance.EnableAutoRefreshProductData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
				var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
				Factory.Save();
				var secondFactory = new BusinessObjectFactory();
				var secondFactoryDec = secondFactory.Load<BaseJobDeclaration>(invLine.Declaration.PK);
				var secondFactoryLine = secondFactoryDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
				JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(secondFactory, secondFactoryLine.PartSyncManagerActiveDeciderPK);

				secondFactoryLine.Declaration.ForcePartRefreshFromUI = true;
				ChangePartDescription(partPK, "NEWDESC");
				AssertEquals("Line Description", "NEWDESC", secondFactoryLine.JI_Description);
			}
		}

		public void TestSynchroniseDoesUpdateWhenSamePartForLoadedInvoiceLineWhenAutoRefreshTurnedOn()
		{
			using (CustomsDataRegistry.Instance.EnableAutoRefreshProductData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
				var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
				Factory.Save();
				var secondFactory = new BusinessObjectFactory();
				var secondFactoryDec = secondFactory.Load<BaseJobDeclaration>(invLine.Declaration.PK);
				var secondFactoryLine = secondFactoryDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
				JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(secondFactory, secondFactoryLine.PartSyncManagerActiveDeciderPK);

				declaration.ForcePartRefreshFromUI = true;
				ChangePartDescription(partPK, "NEWDESC");
				AssertEquals("Line Description", "NEWDESC", secondFactoryLine.JI_Description);
			}
		}

		public void TestSynchroniseDoesUpdateWhenPartIsOriginallyNullWhenForceRefresh()
		{
			using (CustomsDataRegistry.Instance.EnableAutoRefreshProductData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
				var invLine = CreateInvoiceLine(Factory, null, Importer.PK, Supplier.PK);
				Factory.Save();
				var secondFactory = new BusinessObjectFactory();
				var secondFactoryDec = secondFactory.Load<BaseJobDeclaration>(invLine.Declaration.PK);
				var secondFactoryLine = secondFactoryDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
				JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(secondFactory, secondFactoryLine.PartSyncManagerActiveDeciderPK);

				secondFactoryLine.Declaration.ForcePartRefreshFromUI = true;
				ChangePartDescription(partPK, "NEWDESC");
				AssertEquals("Line Description", "", secondFactoryLine.JI_Description);
			}
		}

		public void TestSynchroniseDoesUpdateWhenWhenPartSetOnNewInvoiceLineWhenAutoRefreshTurnedOff()
		{
			using (CustomsDataRegistry.Instance.EnableAutoRefreshProductData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
				var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
				invLine.JI_PartNo = "PARTNUM";
				AssertEquals("Line Description", "DESC", invLine.JI_Description);
			}
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfDecDeleted()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			invLine.Declaration.Delete();
			invLine.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfHeaderDeleted()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			invLine.InvoiceHeader.Delete();
			invLine.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfLineDeleted()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			invLine.Delete();
			invLine.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions()]
		public void TestPartsChangeAfterLineIsDeleted()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			invLine.Delete();
			SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");
		}

		public void TestRefreshPartSyncManagerActiveDeciderPKDictionary()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var dictionary = JobComInvoiceLinePartSynchronisationManager.GetPartSyncManagerActiveDeciderPKDictionary(Factory);
			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(ZGuid.Empty));

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoiceLine.JI_JZ = invoice.PK;
			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(invoice.PK));

			var dec = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = dec.PK;
			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(dec.PK));

			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(invoice.PK));
		}

		public void TestRefreshPartSyncManagerActiveDeciderPKDictionary_AlreadyHasOneLine()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var dictionary = JobComInvoiceLinePartSynchronisationManager.GetPartSyncManagerActiveDeciderPKDictionary(Factory);
			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(dec.PK));
			AssertEquals(1, dictionary[dec.PK].Count);
			Assert(dictionary[dec.PK].Contains(invoiceLine1.PartSyncManager));

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(dec.PK));
			AssertEquals(2, dictionary[dec.PK].Count);
			Assert(dictionary[dec.PK].Contains(invoiceLine2.PartSyncManager));
		}

		public void TestStopManagingWhenActiveDeciderPKWasDisposed()
		{
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);

			var partPK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC").PK;
			AssertEquals("Line Description", "DESC", invLine.JI_Description);

			ChangePartDescription(partPK, "NEWDESC");
			AssertEquals("Should be synchronized", "NEWDESC", invLine.JI_Description);

			JobComInvoiceLinePartSynchronisationManager.StopManagingWhenActiveDeciderPKWasDisposed(invLine.Factory, invLine.PartSyncManagerActiveDeciderPK);
			ChangePartDescription(partPK, "NEWDESC1");
			AssertEquals("Should not be synchronized", "NEWDESC", invLine.JI_Description);
		}

		public void TestBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared_OneCountryHasCountrySpecificPartAndTheOtherEither()
		{
			AssertBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared(CountryCodes.India, CountryCodes.UnitedStates);
		}

		public void TestBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared_NeitherOfCountriesHasCountrySpecificPart_DefaultTypeForUnsupportedCountry()
		{
			AssertBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared(CountryCodes.India, CountryCodes.HongKong);
		}

		public void TestBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared_NeitherOfCountriesHasCountrySpecificPart_DefaultTypeForEuCountry()
		{
			AssertBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared(CountryCodes.Croatia, CountryCodes.Latvia);
		}

		public void TestBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared_BothCountriesHaveCountrySpecificPart()
		{
			AssertBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared(CountryCodes.Australia, CountryCodes.China);
		}

		public void TestBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared_BothCountriesHaveCountrySpecificPart2()
		{
			AssertBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared(CountryCodes.Australia, CountryCodes.UnitedStates);
		}

		void AssertBusinessObjectsWithRelatedEvents_AvoidSamePartInstanceShared(string firstCountryCode, string secondCountryCode)
		{
			var firstBranch = GetBranch(firstCountryCode);
			var secondBranch = GetBranch(secondCountryCode);
			var afghanistanBranch = GetBranch(CountryCodes.Afghanistan);

			const string partNo = "PARTNUM";
			var part = SaveNewPart(Factory, partNo, Importer, Supplier, "DESC");
			var shipment = Factory.New<ForwardingShipment>();
			_ = AddBrokerage(firstBranch, partNo, shipment);
			_ = AddBrokerage(secondBranch, partNo, shipment);
			_ = AddClassification(firstBranch, part);
			_ = AddClassification(secondBranch, part);
			Factory.Save();

			using (DisposableEnvironment.ForBranch(afghanistanBranch.PK.ToGuid()))
			{
				AssertNoExceptionThrown(() => _ = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK).BusinessObjectsWithRelatedEvents);
			}

			using (DisposableEnvironment.ForBranch(firstBranch.PK.ToGuid()))
			{
				AssertNoExceptionThrown(() => _ = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK).BusinessObjectsWithRelatedEvents);
			}

			using (DisposableEnvironment.ForBranch(secondBranch.PK.ToGuid()))
			{
				AssertNoExceptionThrown(() => _ = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK).BusinessObjectsWithRelatedEvents);
			}
		}

		public void TestFirstPivotsAndClassification_OneSpecificed_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.UnitedStates, CountryCodes.Bahrain, CountryCodes.Bahrain);
		}

		public void TestSecondPivotsAndClassification_OneSpecificed_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.UnitedStates, CountryCodes.Bahrain, CountryCodes.UnitedStates, true);
		}

		public void TestFirstPivotsAndClassification_NeitherSpecificed_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.HongKong, CountryCodes.Bahrain, CountryCodes.Bahrain);
		}

		public void TestSecondPivotsAndClassification_NeitherSpecificed_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.HongKong, CountryCodes.Bahrain, CountryCodes.HongKong);
		}

		public void TestFirstPivotsAndClassification_NeitherSpecificed2_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Croatia, CountryCodes.Latvia, CountryCodes.Croatia, CountryCodes.Croatia);
		}

		public void TestSecondPivotsAndClassification_NeitherSpecificed2_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Croatia, CountryCodes.Latvia, CountryCodes.Croatia, CountryCodes.Latvia);
		}

		public void TestFirstPivotsAndClassification_BothSpecificed_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Australia, CountryCodes.China, CountryCodes.Australia, CountryCodes.Australia, true);
		}

		public void TestSecondPivotsAndClassification_BothSpecificed_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Australia, CountryCodes.China, CountryCodes.Australia, CountryCodes.China, true);
		}

		public void TestFirstPivotsAndClassification_BothSpecificed2_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.SouthAfrica, CountryCodes.Italy, CountryCodes.SouthAfrica, CountryCodes.SouthAfrica, true);
		}

		public void TestSecondPivotsAndClassification_BothSpecificed2_LoginAsFirstCountry()
		{
			AssertPivotsAndClassification(CountryCodes.SouthAfrica, CountryCodes.Italy, CountryCodes.SouthAfrica, CountryCodes.Italy, true);
		}

		public void TestFirstPivotsAndClassification_OneSpecificed_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.UnitedStates, CountryCodes.UnitedStates, CountryCodes.Bahrain);
		}

		public void TestSecondPivotsAndClassification_OneSpecificed_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.UnitedStates, CountryCodes.UnitedStates, CountryCodes.UnitedStates, true);
		}

		public void TestFirstPivotsAndClassification_NeitherSpecificed_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.HongKong, CountryCodes.HongKong, CountryCodes.Bahrain);
		}

		public void TestSecondPivotsAndClassification_NeitherSpecificed_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Afghanistan, CountryCodes.HongKong, CountryCodes.HongKong, CountryCodes.HongKong);
		}

		public void TestFirstPivotsAndClassification_NeitherSpecificed2_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Croatia, CountryCodes.Latvia, CountryCodes.Latvia, CountryCodes.Croatia);
		}

		public void TestSecondPivotsAndClassification_NeitherSpecificed2_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Croatia, CountryCodes.Latvia, CountryCodes.Latvia, CountryCodes.Latvia);
		}

		public void TestFirstPivotsAndClassification_BothSpecificed_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Australia, CountryCodes.China, CountryCodes.China, CountryCodes.Australia, true);
		}

		public void TestSecondPivotsAndClassification_BothSpecificed_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Australia, CountryCodes.China, CountryCodes.China, CountryCodes.China, true);
		}

		public void TestFirstPivotsAndClassification_BothSpecificed2_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.SouthAfrica, CountryCodes.Italy, CountryCodes.Italy, CountryCodes.SouthAfrica, true);
		}

		public void TestSecondPivotsAndClassification_BothSpecificed2_LoginAsSecondCountry()
		{
			AssertPivotsAndClassification(CountryCodes.SouthAfrica, CountryCodes.Italy, CountryCodes.Italy, CountryCodes.Italy, true);
		}

		public void TestFirstPivotsAndClassification_OneSpecificed_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.UnitedStates, CountryCodes.Afghanistan, CountryCodes.Bahrain);
		}

		public void TestSecondPivotsAndClassification_OneSpecificed_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.UnitedStates, CountryCodes.Afghanistan, CountryCodes.UnitedStates, true);
		}

		public void TestFirstPivotsAndClassification_NeitherSpecificed_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.HongKong, CountryCodes.Afghanistan, CountryCodes.Bahrain);
		}

		public void TestSecondPivotsAndClassification_NeitherSpecificed_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Bahrain, CountryCodes.HongKong, CountryCodes.Afghanistan, CountryCodes.HongKong);
		}

		public void TestFirstPivotsAndClassification_NeitherSpecificed2_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Croatia, CountryCodes.Latvia, CountryCodes.Afghanistan, CountryCodes.Croatia);
		}

		public void TestSecondPivotsAndClassification_NeitherSpecificed2_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Croatia, CountryCodes.Latvia, CountryCodes.Afghanistan, CountryCodes.Latvia);
		}

		public void TestFirstPivotsAndClassification_BothSpecificed_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Australia, CountryCodes.China, CountryCodes.Afghanistan, CountryCodes.Australia, true);
		}

		public void TestSecondPivotsAndClassification_BothSpecificed_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.Australia, CountryCodes.China, CountryCodes.Afghanistan, CountryCodes.China, true);
		}

		public void TestFirstPivotsAndClassification_BothSpecificed2_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.SouthAfrica, CountryCodes.Italy, CountryCodes.Afghanistan, CountryCodes.SouthAfrica, true);
		}

		public void TestSecondPivotsAndClassification_BothSpecificed2_LoginAsThirdCountry()
		{
			AssertPivotsAndClassification(CountryCodes.SouthAfrica, CountryCodes.Italy, CountryCodes.Afghanistan, CountryCodes.Italy, true);
		}

		public void TestPartIsLoadedOnCopiedDeclarationInvoiceLine()
		{
			var companyLV = Factory.New<GlbCompany>();
			companyLV.GC_Code = "CL1";
			companyLV.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var companyLVBranch = companyLV.Branches.AddNew();
			companyLVBranch.GB_Code = "BL1";

			var client = Importer;
			SaveNewPart(Factory, "PARTNUM", client, null, "DESC");
			using (DisposableEnvironment.ForBranch(companyLVBranch.PK.ToGuid()))
			{
				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.EU.IJobDeclaration>();
				declaration.JE_GB = companyLVBranch.PK;
				declaration.JE_OH_Importer = client.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "PARTNUM";

				var copiedDeclaration = (BaseJobDeclaration)declaration.TemplateCopy();
				var copiedInvoiceLine = (BaseJobComInvoiceLine)copiedDeclaration.InvoiceLines.Single();

				CombineAssertions(() =>
				{
					AssertEquals("PartNo should be the same on copied invoice line", "PARTNUM", copiedInvoiceLine.JI_PartNo);
					AssertNotNull("The part should be loaded", copiedInvoiceLine.Part);
				});
			}
		}

		public void TestNullPartReturnedWhenPartPKCleared()
		{
			Factory.RefreshEnabled = true;
			SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");
			var invLine = CreateInvoiceLine(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = invLine.PartSyncManager;

			CombineAssertions(() =>
			{
				AssertNotNull("Part set", syncManager.Part);

				invLine.JI_OP = ZGuid.Empty;

				AssertNull("Part PK cleared, null part should be returned", syncManager.Part);
			});
		}

		public void AssertPivotsAndClassification(string firstCountryCode, string secondCountryCode, string loginCountryCode, string testCountry, bool countryOverrided = false)
		{
			var firstBranch = GetBranch(firstCountryCode);
			var secondBranch = GetBranch(secondCountryCode);
			GlbBranch loginBranch;
			if (firstCountryCode == loginCountryCode)
			{
				loginBranch = firstBranch;
			}
			else if (secondCountryCode == loginCountryCode)
			{
				loginBranch = secondBranch;
			}
			else
			{
				loginBranch = GetBranch(loginCountryCode);
			}

			var ((declaration1PK, invoiceLine1PK), (declaration2PK, invoiceLine2PK), (classification1, pivot1), (classification2, pivot2)) = AddDataForPivotsAndClassificaiontTest(firstBranch, secondBranch);
			int expectedClassificationCount = 0, expectedPivotCount = 0;
			var (expectedDeclarationPK, expectedInvoiceLinePK) = testCountry == firstCountryCode ? (declaration1PK, invoiceLine1PK) : (declaration2PK, invoiceLine2PK);
			ZGuid expectedClassificationPK = ZGuid.Empty, expectedPivotPK = ZGuid.Empty;
			bool expectedFromFirst = false, expectedFromSecond = false;
			if (countryOverrided || loginCountryCode == testCountry)
			{
				expectedFromFirst = testCountry == firstCountryCode;
				expectedFromSecond = testCountry == secondCountryCode;
			}
			else
			{
				expectedFromFirst = loginCountryCode == firstCountryCode;
				expectedFromSecond = loginCountryCode == secondCountryCode;
			}
			if (expectedFromFirst)
			{
				(expectedClassificationCount, expectedClassificationPK, expectedPivotCount, expectedPivotPK) = (1, classification1.PK, 1, pivot1.PK);
			}
			else if (expectedFromSecond)
			{
				(expectedClassificationCount, expectedClassificationPK, expectedPivotCount, expectedPivotPK) = (1, classification2.PK, 1, pivot2.PK);
			}

			using (DisposableEnvironment.ForBranch(loginBranch.PK.ToGuid()))
			{
				var dec = new BusinessObjectFactory().Load<BaseJobDeclaration>(expectedDeclarationPK);
				var invoiceLineCount = dec == null ? 0 : 1;
				var invoiceLinePK = (dec == null || dec.InvoiceLines.Count == 0) ? ZGuid.Empty : dec.InvoiceLines[0].PK;
				var curPart = dec?.InvoiceLines[0].Part;
				var classificationCount = curPart == null ? 0 : curPart.ClassificationsForBinding.Count;
				var classificationPK = classificationCount == 0 ? ZGuid.Empty : curPart.ClassificationsForBinding.First().PK;
				var pivotCount = curPart == null ? 0 : curPart.PivotsForBinding.Count;
				var pivotPK = pivotCount == 0 ? ZGuid.Empty : curPart.PivotsForBinding.First().PK;
				CombineAssertions(() =>
				{
					AssertEquals("Declaration InvoiceLine.Count", 1, invoiceLineCount);
					if (invoiceLineCount > 0)
					{
						AssertEquals("Declaration InvoiceLine[0].PK", expectedInvoiceLinePK, invoiceLinePK);
					}
					AssertEquals("Declaration InvoiceLine[0].Part.Classification.Count", expectedClassificationCount, classificationCount);
					if (classificationCount > 0)
					{
						AssertEquals("Declaration InvoiceLine[0].Part.Classification[0] should", expectedClassificationPK, classificationPK);
					}
					AssertEquals("Declaration InvoiceLine[0].Part.Pivot.Count", expectedPivotCount, pivotCount);
					if (pivotCount > 0)
					{
						AssertEquals("Declaration InvoiceLine[0].Part.Pivot[0] & should", expectedPivotPK, pivotPK);
					}
				});
			}
		}

		((ZGuid, ZGuid), (ZGuid, ZGuid), (BaseCusClassification, BaseCusClassPartPivot), (BaseCusClassification, BaseCusClassPartPivot)) AddDataForPivotsAndClassificaiontTest(GlbBranch firstBranch, GlbBranch secondBranch)
		{
			const string partNo = "PARTNUM";
			var part = SaveNewPart(Factory, partNo, Importer, Supplier, "DESC");
			var shipment = Factory.New<ForwardingShipment>();
			var pk1 = AddBrokerage(firstBranch, partNo, shipment);
			var pk2 = AddBrokerage(secondBranch, partNo, shipment);
			var cpPair1 = AddClassification(firstBranch, part);
			var cpPair2 = AddClassification(secondBranch, part);
			Factory.Save();

			return (pk1, pk2, cpPair1, cpPair2);
		}

		(ZGuid DeclarationPK, ZGuid InvoiceLinePK) AddBrokerage(GlbBranch branch, string partNo, ForwardingShipment shipment)
		{
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_OH_Importer = Importer.PK;
				declaration.JE_OH_Supplier = Supplier.PK;
				var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = partNo;
				declaration.JE_JS = shipment.PK;
				declaration.JE_GB = branch.PK;
				return (declaration.PK, invoiceLine.PK);
			}
		}

		(BaseCusClassification, BaseCusClassPartPivot) AddClassification(GlbBranch branch, OrgSupplierPart orgSupplierPart)
		{
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var countryCode = branch.Company.GC_RN_NKCountryCode;
				var classification = Factory.New<BaseCusClassification>();
				classification.CC_TariffNum = "0105.92.00 01";
				classification.CC_ClassificationType = Common.ClassificationType.IMP;
				classification.CC_LookupCode = "L1";
				classification.CC_Description = $"Classficiation for {countryCode}";
				var pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = orgSupplierPart.PK;
				pivot.CI_RN_NKCountry = countryCode;
				return (classification, pivot);
			}
		}

		GlbBranch GetBranch(string countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.FillWithValidTestData();
			company.GC_RN_NKCountryCode = countryCode;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			return branch;
		}

		#region Implementation

		OrgHeader Importer
		{
			get { return importer ?? (importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS")); }
		}
		OrgHeader importer;

		OrgHeader Supplier
		{
			get { return supplier ?? (supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABABEU")); }
		}
		OrgHeader supplier;

		BaseJobComInvoiceLine CreateInvoiceLine(BusinessObjectFactory factory, ZString partNum, ZGuid importerPK, params ZGuid[] supplierPKs)
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
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(factory, result.PartSyncManagerActiveDeciderPK);
			result.JI_PartNo = partNum;
			return result;
		}

		OrgSupplierPart SaveNewPart(BusinessObjectFactory factory, ZString partNum, OrgHeader importer, OrgHeader supplier, ZString description)
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
			return newPart;
		}

		void ChangePartDescription(ZGuid partPK, ZString newDescription)
		{
			var factory = new BusinessObjectFactory();
			var part = factory.Load<OrgSupplierPart>(partPK);
			part.OP_Desc = newDescription;
			factory.Save();
		}

		protected override void SetUp()
		{
			declaration = Factory.New<BaseJobDeclaration>();
			base.SetUp();
		}

		BaseJobDeclaration declaration;

		#endregion
	}
}
