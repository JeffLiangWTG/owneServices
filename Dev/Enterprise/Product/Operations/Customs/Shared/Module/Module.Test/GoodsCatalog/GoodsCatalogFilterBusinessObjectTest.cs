using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GoodsCatalogFilterBusinessObject))]
	public class GoodsCatalogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLookupCodeFilter()
		{
			var lookupCodeFilter = (ModuleTextFilter)filterBO["Lookup Code"];
			lookupCodeFilter.IsActive = true;

			lookupCodeFilter.Property = "XXX";
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("CGC_CatalogCode", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog1 Result", goodsCatalog1, goodsCatalogResult[0]);
			});

			lookupCodeFilter.Property = "XYZ";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("CGC_CatalogCode", 0, goodsCatalogResult.Length);
		}

		public void TestDescriptionFilter()
		{
			var descriptionFilter = (ModuleTextFilter)filterBO["Description"];
			descriptionFilter.IsActive = true;

			descriptionFilter.Property = "DESC 2";
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Description", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog2 Result", goodsCatalog2, goodsCatalogResult[0]);
			});

			descriptionFilter.Property = "DESC";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Description", 3, goodsCatalogResult.Length);

			descriptionFilter.Property = "XXX";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Description", 0, goodsCatalogResult.Length);
		}

		public void TestTariffFilter()
		{
			var tariffFilter = (ModuleTextFilter)filterBO["Tariff"];
			tariffFilter.IsActive = true;

			tariffFilter.Property = "001122";
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Tariff", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog1 Result", goodsCatalog1, goodsCatalogResult[0]);
			});

			tariffFilter.Property = "0011";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Tariff", 3, goodsCatalogResult.Length);

			tariffFilter.Property = "112233";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Tariff", 0, goodsCatalogResult.Length);
		}

		public void TestMessageStatusFilter()
		{
			var lookupCodeFilter = (ModuleTextFilter)filterBO["Message Status"];
			lookupCodeFilter.IsActive = true;
			lookupCodeFilter.Property = "AWA";

			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Message Status", 1, goodsCatalogResult.Length);

			lookupCodeFilter.Property = "ACC";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Message Status", 1, goodsCatalogResult.Length);

			goodsCatalog1.CGC_MessageStatus = "AWA";
			lookupCodeFilter.Property = "AWA";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Message Status", 2, goodsCatalogResult.Length);

			lookupCodeFilter.Property = "WWW";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Message Status", 0, goodsCatalogResult.Length);

			lookupCodeFilter.Property = string.Empty;
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Result Length", 3, goodsCatalogResult.Length);

			lookupCodeFilter.Property = "NOT";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Message Status", 1, goodsCatalogResult.Length);
		}

		public virtual void TestLookup()
		{
			AssertType<GoodsCatalogFilterLookups>(new GoodsCatalogFilterBusinessObject().Lookups);
		}

		public void TestTypeFilter()
		{
			var typeFilter = (ModuleTextFilter)filterBO["Type"];
			typeFilter.IsActive = true;

			typeFilter.Property = GoodsCatalogTypeList.Codes.Export;
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Result Length", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog1 Result", goodsCatalog1, goodsCatalogResult[0]);
			});

			typeFilter.Property = string.Empty;
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Result Length", 3, goodsCatalogResult.Length);
		}

		public void TestAuthorityIdentifierFilter()
		{
			var authorityIdentifierFilter = (ModuleTextFilter)filterBO["Authority Identifier"];
			authorityIdentifierFilter.IsActive = true;

			authorityIdentifierFilter.Property = "1";
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Authority Identifier", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog1 Result", goodsCatalog1, goodsCatalogResult[0]);
			});

			authorityIdentifierFilter.Property = "9";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Authority Identifier", 0, goodsCatalogResult.Length);
		}

		public void TestStatusFilter()
		{
			var statusFilter = (ModuleTextFilter)filterBO["Status"];
			statusFilter.IsActive = true;

			statusFilter.Property = "1";
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Tariff", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog1 Result", goodsCatalog1, goodsCatalogResult[0]);
			});

			statusFilter.Property = "9";
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Tariff", 0, goodsCatalogResult.Length);
		}

		public void TestConsigneeFilter()
		{
			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "2";
			consignee2.OH_FullName = "TEST CONSIGNEE 2";

			goodsCatalog2.CGC_OH_Owner = consignee2.PK;

			var statusFilter = (ModuleGuidFilter)filterBO["Consignee"];
			statusFilter.IsActive = true;

			statusFilter.Property = consignee2.PK;
			var goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Consignee", 1, goodsCatalogResult.Length);
				AssertEquals("GoodsCatalog2 Result", goodsCatalog2, goodsCatalogResult[0]);
			});

			statusFilter.Property = ZGuid.Empty;
			goodsCatalogResult = Factory.Load<BaseCusGoodsCatalog>(filterBO.Filter);
			AssertEquals("Consignee", 3, goodsCatalogResult.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = GetNewFilterStripBusinessObject();
			filterCollection = new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "1";
			consignee.OH_FullName = "TEST CONSIGNEE";

			goodsCatalog1 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			goodsCatalog1.CGC_OH_Owner = consignee.PK;
			goodsCatalog1.CGC_CatalogCode = "XXX";
			goodsCatalog1.CGC_Description = "DESC 1";
			goodsCatalog1.CGC_Tariff = "001122";
			goodsCatalog1.CGC_MessageStatus = "ACC";
			goodsCatalog1.CGC_AuthorityStatus = "1";
			goodsCatalog1.CGC_AuthorityIdentifier = "1";
			goodsCatalog1.CGC_Type = GoodsCatalogTypeList.Codes.Export;

			goodsCatalog2 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			goodsCatalog2.CGC_OH_Owner = consignee.PK;
			goodsCatalog2.CGC_CatalogCode = "YYY";
			goodsCatalog2.CGC_Description = "DESC 2";
			goodsCatalog2.CGC_Tariff = "001133";
			goodsCatalog2.CGC_MessageStatus = "AWA";
			goodsCatalog2.CGC_AuthorityStatus = "2";
			goodsCatalog2.CGC_AuthorityIdentifier = "2";
			goodsCatalog2.CGC_Type = GoodsCatalogTypeList.Codes.Import;

			goodsCatalog3 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			goodsCatalog3.CGC_OH_Owner = consignee.PK;
			goodsCatalog3.CGC_CatalogCode = "ZZZ";
			goodsCatalog3.CGC_Description = "DESC 3";
			goodsCatalog3.CGC_Tariff = "001144";
			goodsCatalog3.CGC_MessageStatus = "";
			goodsCatalog3.CGC_AuthorityStatus = "3";
			goodsCatalog3.CGC_AuthorityIdentifier = "3";
			goodsCatalog3.CGC_Type = GoodsCatalogTypeList.Codes.Import;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GoodsCatalogFilterBusinessObject();
		}

		protected FilterStripBusinessObject filterBO;
		protected BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog> filterCollection;
		protected BaseCusGoodsCatalog goodsCatalog1;
		protected BaseCusGoodsCatalog goodsCatalog2;
		protected BaseCusGoodsCatalog goodsCatalog3;
	}
}
