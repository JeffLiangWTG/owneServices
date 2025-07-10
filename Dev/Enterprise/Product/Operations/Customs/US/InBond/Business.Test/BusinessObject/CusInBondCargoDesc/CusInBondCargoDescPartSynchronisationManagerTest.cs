using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondCargoDescPartSynchronisationManagerTest : TestCaseWithFactory
	{
		public void TestCalculatePart()
		{
			Factory.RefreshEnabled = true;
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
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
			var commodity1 = Createcommodity(Factory, "TestProduct", Importer.PK, Supplier.PK);
			var syncManager1 = commodity1.PartSyncManager;
			AssertEquals("syncManager.TotalMatchCount", 0, syncManager1.TotalMatchCount);
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var commodity2 = Createcommodity(Factory, "TestProduct", Importer.PK, Supplier.PK);
			var syncManager2 = commodity2.PartSyncManager;
			AssertEquals("There are two products found", 2, syncManager2.TotalMatchCount);
			var product3 = SaveNewPart(Factory, "TestProduct", Importer, Supplier, "TestDescription");
			syncManager2.CalculatePart();
			AssertEquals("SyncManager.Supplier", product3.PK, syncManager2.Part.PK);
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
			var commodity = Createcommodity(Factory, "TestProduct", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
			syncManager.CalculatePart();
			AssertEquals("syncManager.TotalNumberOfPartsCount", 3, syncManager.TotalNumberOfPartsCount);
			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			syncManager.CalculatePart();
			AssertEquals("syncManager.TotalNumberOfPartsCount", 3, syncManager.TotalNumberOfPartsCount);
		}

		public void TestPartReloadsIfTheImporterChanges()
		{
			SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");
			var commodity = Createcommodity(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			var header = commodity.Header;
			header.BH_OA_Importer = Importer.MainAddress.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfTheSupplierChanges()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");
			var commodity = Createcommodity(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			commodity.BY_OH_Supplier = Supplier.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfTheContainerChanges()
		{
			SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");
			var commodity = Createcommodity(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			var container = commodity.Container;
			commodity.BY_ParentID = ZGuid.Empty;
			container.Header.BH_OA_Importer = Importer.MainAddress.PK;
			AssertNull(syncManager.Part);
			commodity.BY_ParentID = container.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartDoesntReloadsIfTheSupplierChangesOnADifferentCommodity()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");
			var commodity = Createcommodity(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var container = commodity.Container;
			var commodity1 = container.Commodities.AddNew();
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			commodity1.BY_OH_Supplier = Supplier.PK;
			AssertNull(syncManager.Part);
			commodity.BY_OH_Supplier = Supplier.PK;
			AssertNotNull(syncManager.Part);
		}

		public void TestCollectionReloadsIfTheSupplierChangesAfterTheCommodityHasChanged()
		{
			SaveNewPart(Factory, "PARTNUM", null, Supplier, "DESC");
			var commodity = Createcommodity(Factory, "PARTNUM", ZGuid.Empty, ZGuid.Empty);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			var container = commodity.Container;
			container.Commodities.RemoveFromRelationship(commodity);
			commodity.BY_ParentID = container.PK;
			container.Commodities.Add(commodity);
			AssertNull(syncManager.Part);
			container.Header.BH_OH_Supplier = Supplier.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPart()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC");
			AssertNotNull("Part", syncManager.Part);
		}

		public void TestPartSynchronisesIfChangedInAnotherFactory()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			var part = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC");
			AssertEquals("Part Description", "DESC", syncManager.Part.OP_Desc);
			ChangePartDescription(part.PK, "NEWDESC");
			AssertEquals("Part Description", "NEWDESC", syncManager.Part.OP_Desc);
		}

		public void TestCommoditySyncsIfPartCreatedInAnotherFactory()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC");
			AssertEquals("Commodity Description", "DESC", commodity.BY_Description);
		}

		public void TestCommoditySyncsIfPartChangedInAnotherFactory()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			AssertEquals(ZGuid.Empty, commodity.BY_OP_Part);
			var part = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", Importer, null, "DESC");
			var part2 = SaveNewPart(new BusinessObjectFactory(), "PARTNUM2", Importer, null, "DESC2");
			AssertEquals(part.PK, commodity.BY_OP_Part);
			AssertEquals("Commodity Description", "DESC", commodity.BY_Description);
			ChangePartDescription(part.PK, "NEWDESC");
			AssertEquals("Commodity Description", "NEWDESC", commodity.BY_Description);
			commodity.BY_PartNumber = "PARTNUM2";
			AssertEquals("Commodity Description", "DESC2", commodity.BY_Description);
			ChangePartDescription(part2.PK, "NEWDESC2");
			AssertEquals("Commodity Description", "NEWDESC2", commodity.BY_Description);
			commodity.BY_PartNumber = "PARTNUM";
			AssertEquals("Commodity Description", "NEWDESC", commodity.BY_Description);
			ChangePartDescription(part2.PK, "NEWNEWDESC2");
			AssertEquals("Commodity Description", "NEWDESC", commodity.BY_Description);
		}

		public void TestSynchroniseWorksForLoadedcommodity()
		{
			var part = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			Factory.Save();
			var secondFactory = new BusinessObjectFactory();
			var secondFactoryHeader = secondFactory.Load<CusInBondHeader>(commodity.Header.PK);
			var secondFactoryCommodity = secondFactoryHeader.MovementHeaders[0].MovementDetails[0].Containers[0].Commodities[0];
			ChangePartDescription(part.PK, "NEWDESC");
			AssertEquals("commodity Description", "NEWDESC", secondFactoryCommodity.BY_Description);
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfHeaderDeleted()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			commodity.Header.Delete();
			commodity.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfContainerDeleted()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			commodity.Container.Delete();
			commodity.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfCommodityDeleted()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			commodity.Delete();
			commodity.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions()]
		public void TestPartsChangeAfterCommodityIsDeleted()
		{
			var commodity = Createcommodity(Factory, "PARTNUM", Importer.PK, Supplier.PK);
			var syncManager = commodity.PartSyncManager;
			AssertNull(syncManager.Part);
			commodity.Delete();
			SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC");
		}

		OrgHeader importer;
		OrgHeader Importer => importer ?? (importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS"));

		OrgHeader supplier;
		OrgHeader Supplier => supplier ?? (supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABABEU"));

		CusInBondCargoDesc Createcommodity(BusinessObjectFactory factory, ZString partNum, ZGuid importerPK, ZGuid supplierPK)
			=> Createcommodity(factory.New<CusInBondHeader>(), partNum, importerPK, supplierPK);

		CusInBondCargoDesc Createcommodity(CusInBondHeader header, ZString partNum, ZGuid importerPK, ZGuid supplierPK)
		{
			header.ImporterOrgPK = importerPK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_OH_Supplier = supplierPK;
			commodity.BY_PartNumber = partNum;
			return commodity;
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
			var pivot = newPart.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = US.Business.USCTariff.CottonFeeApplicable;
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
	}
}
