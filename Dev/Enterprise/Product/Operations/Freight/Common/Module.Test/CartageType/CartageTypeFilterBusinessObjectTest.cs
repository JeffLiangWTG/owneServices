using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Module.Testing
{
	[TestedType(typeof(CartageTypeFilterBusinessObject))]
	sealed class CartageTypeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TextFiltersTests

		public void TestDefaultFilter()
		{
			CommonCartageType description1 = Factory.NewWithValidTestData<CommonCartageType>();
			CommonCartageType description2 = Factory.NewWithValidTestData<CommonCartageType>();
			description1.E3_JobType = "Indi";
			description2.E3_JobType = "Some";

			Factory.Save();

			CartageTypeFilterBusinessObject filter = new CartageTypeFilterBusinessObject();
			CommonCartageTypeCollection jobTypes = new CommonCartageTypeCollection(Factory, filter.Filter);
			AssertCollectionContains(description1, jobTypes);
			AssertCollectionContains(description2, jobTypes);
		}

		public void TestCodeFilter()
		{
			CommonCartageType description1 = Factory.NewWithValidTestData<CommonCartageType>();
			CommonCartageType description2 = Factory.NewWithValidTestData<CommonCartageType>();
			description1.E3_JobType = "Indi";
			description2.E3_JobType = "Some";

			Factory.Save();

			CartageTypeFilterBusinessObject filter = new CartageTypeFilterBusinessObject();
			((ModuleTextFilter)filter["Code"]).Property = "Indi";
			((ModuleTextFilter)filter["Code"]).IsActive = true;

			CommonCartageTypeCollection localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);

			AssertCollectionContains(description1, localCJTypes);
			AssertCollectionNotContains(description2, localCJTypes);
		}

		public void TestDescriptionFilter()
		{
			CommonCartageType description1 = Factory.NewWithValidTestData<CommonCartageType>();
			CommonCartageType description2 = Factory.NewWithValidTestData<CommonCartageType>();
			description1.E3_Description = "Indescriptive";
			description1.E3_JobType = "X1";
			description2.E3_Description = "SomeDescription";
			description2.E3_JobType = "X2";

			Factory.Save();

			CartageTypeFilterBusinessObject filter = new CartageTypeFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Indescriptive";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			CommonCartageTypeCollection localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);

			AssertCollectionContains(description1, localCJTypes);
			AssertCollectionNotContains(description2, localCJTypes);
		}

		public void TestTransportModeFilter()
		{
			CommonCartageType transportMode1 = Factory.NewWithValidTestData<CommonCartageType>();
			CommonCartageType transportMode2 = Factory.NewWithValidTestData<CommonCartageType>();
			transportMode1.E3_JobType = "X1";
			transportMode1.E3_ShippingTransportMode = "Ind";
			transportMode2.E3_JobType = "X2";
			transportMode2.E3_ShippingTransportMode = "Som";

			Factory.Save();

			CartageTypeFilterBusinessObject filter = new CartageTypeFilterBusinessObject();
			((ModuleTextFilter)filter["Transport Mode"]).Property = "Ind";
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;

			CommonCartageTypeCollection localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);

			AssertCollectionContains(transportMode1, localCJTypes);
			AssertCollectionNotContains(transportMode2, localCJTypes);
		}

		public void TestOrganisationFilter()
		{
			CommonCartageOrg localCJOrg1 = Factory.NewWithValidTestData<CommonCartageOrg>();
			CommonCartageOrg localCJOrg2 = Factory.NewWithValidTestData<CommonCartageOrg>();
			CommonCartageType localCJType1 = localCJOrg1.CommonCartageType;
			CommonCartageType localCJType2 = localCJOrg2.CommonCartageType;
			localCJOrg1.E5_OrgType = "Org";
			localCJType1.E3_JobType = "X1";
			localCJOrg2.E5_OrgType = "!!!";
			localCJType2.E3_JobType = "X2";

			Assert(localCJOrg1.E5_OrgType != localCJOrg2.E5_OrgType);

			localCJOrg1.E5_E3 = localCJType1.PK;
			localCJOrg2.E5_E3 = localCJType2.PK;

			Factory.Save();

			CartageTypeFilterBusinessObject filter = new CartageTypeFilterBusinessObject();
			((ModuleTextFilter)filter["Organisation"]).Property = localCJOrg1.E5_OrgType;
			((ModuleTextFilter)filter["Organisation"]).IsActive = true;

			CommonCartageTypeCollection localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);

			AssertCollectionContains(localCJType1, localCJTypes);
			AssertCollectionNotContains(localCJType2, localCJTypes);
		}

		#endregion

		#region CheckboxFiltersTests

		public void TestStatusHiddenFilter()
		{
			CommonCartageType hiddenType = Factory.NewWithValidTestData<CommonCartageType>();
			CommonCartageType nonHiddenType = Factory.NewWithValidTestData<CommonCartageType>();
			hiddenType.E3_IsHidden = true;
			hiddenType.E3_JobType = "X1";
			nonHiddenType.E3_IsHidden = false;
			nonHiddenType.E3_JobType = "X2";

			CartageTypeFilterBusinessObject filter = new CartageTypeFilterBusinessObject();
			((ModuleTextFilter)filter["Hidden Status"]).IsActive = true;
			AssertEquals("Default value", "Is Hidden", ((ModuleTextFilter)filter["Hidden Status"]).Property);

			CommonCartageTypeCollection localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);
			AssertCollectionContains(hiddenType, localCJTypes);
			AssertCollectionNotContains(nonHiddenType, localCJTypes);

			((ModuleTextFilter)filter["Hidden Status"]).Property = "Is Not Hidden";
			((ModuleTextFilter)filter["Hidden Status"]).IsActive = true;

			localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);

			AssertCollectionNotContains(hiddenType, localCJTypes);
			AssertCollectionContains(nonHiddenType, localCJTypes);

			((ModuleTextFilter)filter["Hidden Status"]).Property = "All";
			((ModuleTextFilter)filter["Hidden Status"]).IsActive = true;

			localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);

			AssertCollectionContains(hiddenType, localCJTypes);
			AssertCollectionContains(nonHiddenType, localCJTypes);
		}

		public void TestStatusSystemFilter()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.CartageType))
			{
				CommonCartageType systemType = Factory.NewWithValidTestData<CommonCartageType>();
				CommonCartageType nonSystemType = Factory.NewWithValidTestData<CommonCartageType>();
				systemType.E3_IsSystem = true;
				systemType.E3_JobType = "X1";
				nonSystemType.E3_IsSystem = false;
				nonSystemType.E3_JobType = "X2";

				var filter = module.FilterBusinessObject;
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;
				AssertEquals("Default value", "System", ((ModuleTextFilter)filter["Is System Defined"]).Property);

				CommonCartageTypeCollection localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);
				AssertCollectionContains(systemType, localCJTypes);
				AssertCollectionNotContains(nonSystemType, localCJTypes);

				((ModuleTextFilter)filter["Is System Defined"]).Property = "Not System";
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;

				localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);
				AssertCollectionNotContains(systemType, localCJTypes);
				AssertCollectionContains(nonSystemType, localCJTypes);

				((ModuleTextFilter)filter["Is System Defined"]).Property = "All";
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;

				localCJTypes = new CommonCartageTypeCollection(Factory, filter.Filter);
				AssertCollectionContains(systemType, localCJTypes);
				AssertCollectionContains(nonSystemType, localCJTypes);
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CartageTypeFilterBusinessObject();
		}

		#endregion
	}
}
