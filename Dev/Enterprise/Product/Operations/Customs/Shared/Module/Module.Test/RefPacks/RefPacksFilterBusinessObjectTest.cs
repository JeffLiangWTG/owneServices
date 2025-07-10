using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Shared.Module.Testing
{
	[TestedType(typeof(RefPacksFilterBusinessObject))]
	sealed class RefPacksFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCommercialPackFilter()
		{
			refPacks1.RP_CommercialPack = "AAA";
			refPacks2.RP_CommercialPack = "AAB";
			Factory.Save();
			var packsFilter = (ModuleTextFilter)filterBO["Commercial Packs"];
			packsFilter.Property = "AA";
			packsFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain RefPacks1", !filterCollection.Contains(refPacks1));
			Assert("Expect collection not to contain RefPacks2", !filterCollection.Contains(refPacks2));
			packsFilter.Property = "AAB";
			packsFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain RefPacks1", !filterCollection.Contains(refPacks1));
			Assert("Expect collection to contain RefPacks2", filterCollection.Contains(refPacks2));
		}

		public void TestIsSystemFilter()
		{
			refPacks1.RP_IsSystem = true;
			refPacks2.RP_IsSystem = false;
			Factory.Save();
			var packsFilter = (ModuleTextFilter)filterBO["Is System Defined"];
			packsFilter.Property = "System";
			packsFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain RefPacks1", filterCollection.Contains(refPacks1));
			Assert("Expect collection not to contain RefPacks2", !filterCollection.Contains(refPacks2));
			packsFilter.Property = "Not System";
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain RefPacks1", !filterCollection.Contains(refPacks1));
			Assert("Expect collection to contain RefPacks2", filterCollection.Contains(refPacks2));
			packsFilter.Property = "All";
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain RefPacks1", filterCollection.Contains(refPacks1));
			Assert("Expect collection to contain RefPacks2", filterCollection.Contains(refPacks2));
		}

		public void TestCustomsPackListProperty()
		{
			AssertNotNull(filterBO.CustomsPackList);
			AssertEquals(21, filterBO.CustomsPackList.Count);
		}

		public void TestCustomsPackFilter()
		{
			refPacks1.RP_CustomsPack = "AAA";
			refPacks2.RP_CustomsPack = "AAB";
			Factory.Save();
			var packsFilter = (ModuleTextFilter)filterBO["Customs Pack"];
			packsFilter.Property = "AA";
			packsFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain RefPacks1", !filterCollection.Contains(refPacks1));
			Assert("Expect collection not to contain RefPacks2", !filterCollection.Contains(refPacks2));
			packsFilter.Property = "AAB";
			packsFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain RefPacks1", !filterCollection.Contains(refPacks1));
			Assert("Expect collection to contain RefPacks2", filterCollection.Contains(refPacks2));
		}

		public void TestSupplierListProperty()
		{
			AssertNotNull(filterBO.SupplierList);
		}

		public void TestSupplierFilter()
		{
			organisation1.OH_IsConsignor = true;
			organisation2.OH_IsConsignor = true;
			refPacks1.RP_OH_Supplier = organisation1.PK;
			refPacks2.RP_OH_Supplier = organisation2.PK;
			Factory.Save();
			var consignorFilter = (ModuleGuidFilter)filterBO["Supplier"];
			consignorFilter.Property = organisation1.PK;
			consignorFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain RefPacks1", filterCollection.Contains(refPacks1));
			Assert("Expect collection not to contain RefPacks2", !filterCollection.Contains(refPacks2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new RefPacksFilterBusinessObject();

		CusRefPacks refPacks1;
		CusRefPacks refPacks2;
		OrgHeader organisation1;
		OrgHeader organisation2;
		RefPacksFilterBusinessObject filterBO;
		CusRefPacksCollection filterCollection;
		protected override void SetUp()
		{
			base.SetUp();
			refPacks1 = Factory.NewWithValidTestData<CusRefPacks>();
			refPacks1.RP_Type = RPTypeList.Codes.CommercialInvoice;
			refPacks2 = Factory.NewWithValidTestData<CusRefPacks>();
			refPacks2.RP_Type = RPTypeList.Codes.CommercialInvoice;
			organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			filterBO = (RefPacksFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterBO.QueryObjectType = typeof(CusRefPacks);
			filterCollection = new CusRefPacksCollection(Factory);
		}
	}
}
