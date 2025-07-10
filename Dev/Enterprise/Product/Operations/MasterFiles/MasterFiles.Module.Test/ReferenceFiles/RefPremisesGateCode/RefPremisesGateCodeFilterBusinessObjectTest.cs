using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPremisesGateCodeFilterBusinessObject))]
	sealed class RefPremisesGateCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text Filters

		public void TestPremisesCodeFilter()
		{
			RefPremisesGateCode premisesCode1 = Factory.NewWithValidTestData<RefPremisesGateCode>();
			RefPremisesGateCode premisesCode2 = Factory.NewWithValidTestData<RefPremisesGateCode>();
			premisesCode1.R5_PremisesGateCode = "Indescr";
			premisesCode2.R5_PremisesGateCode = "SomeDescr";

			Factory.Save();

			RefPremisesGateCodeFilterBusinessObject filter = new RefPremisesGateCodeFilterBusinessObject();
			((ModuleTextFilter)filter["Premises Code"]).Property = "Indescr";
			((ModuleTextFilter)filter["Premises Code"]).IsActive = true;

			RefPremisesGateCodeCollection pGateCodes = new RefPremisesGateCodeCollection(Factory, filter.Filter);

			AssertCollectionContains(premisesCode1, pGateCodes);
			AssertCollectionNotContains(premisesCode2, pGateCodes);
		}

		public void TestDataProviderFilter()
		{
			RefPremisesGateCode dataProvider1 = Factory.NewWithValidTestData<RefPremisesGateCode>();
			RefPremisesGateCode dataProvider2 = Factory.NewWithValidTestData<RefPremisesGateCode>();
			dataProvider1.R5_DataProvider = PremiseGateCodeDataProviderList.Codes.OneStop;
			dataProvider2.R5_DataProvider = "";

			Factory.Save();

			RefPremisesGateCodeFilterBusinessObject filter = new RefPremisesGateCodeFilterBusinessObject();
			((ModuleTextFilter)filter["Data Provider"]).Property = PremiseGateCodeDataProviderList.Codes.OneStop;
			((ModuleTextFilter)filter["Data Provider"]).IsActive = true;

			RefPremisesGateCodeCollection pGateCodes = new RefPremisesGateCodeCollection(Factory);
			pGateCodes.AdditionalFilter = filter.Filter;

			AssertCollectionContains(dataProvider1, pGateCodes);
			AssertCollectionNotContains(dataProvider2, pGateCodes);
		}

		#endregion

		#region CheckboxFiltersTests

		public void TestIsWharfFilter()
		{
			RefPremisesGateCode wharfCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
			RefPremisesGateCode notWharfCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
			wharfCode.R5_IsWharf = true;
			notWharfCode.R5_IsWharf = false;

			RefPremisesGateCodeFilterBusinessObject filter = new RefPremisesGateCodeFilterBusinessObject();
			((ModuleTextFilter)filter["Is Wharf"]).IsActive = true;
			AssertEquals("Default value", "Is Wharf", ((ModuleTextFilter)filter["Is Wharf"]).Property);

			RefPremisesGateCodeCollection pGateCodes = new RefPremisesGateCodeCollection(Factory);
			pGateCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(wharfCode, pGateCodes);
			AssertCollectionNotContains(notWharfCode, pGateCodes);

			((ModuleTextFilter)filter["Is Wharf"]).Property = "Is Not Wharf";
			((ModuleTextFilter)filter["Is Wharf"]).IsActive = true;

			pGateCodes.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains(wharfCode, pGateCodes);
			AssertCollectionContains(notWharfCode, pGateCodes);

			((ModuleTextFilter)filter["Is Wharf"]).Property = "All";
			((ModuleTextFilter)filter["Is Wharf"]).IsActive = true;

			pGateCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(wharfCode, pGateCodes);
			AssertCollectionContains(notWharfCode, pGateCodes);
		}

		public void TestIsSystemFilter()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.RefPremisesGateCode))
			{
				RefPremisesGateCode systemCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
				RefPremisesGateCode notSystemCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
				systemCode.R5_IsSystem = true;
				notSystemCode.R5_IsSystem = false;

				var filter = module.FilterBusinessObject;
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;
				AssertEquals("Default value", "System", ((ModuleTextFilter)filter["Is System Defined"]).Property);

				RefPremisesGateCodeCollection pGateCodes = new RefPremisesGateCodeCollection(Factory);
				pGateCodes.AdditionalFilter = filter.Filter;
				AssertCollectionContains(systemCode, pGateCodes);
				AssertCollectionNotContains(notSystemCode, pGateCodes);

				((ModuleTextFilter)filter["Is System Defined"]).Property = "Not System";
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;

				pGateCodes.AdditionalFilter = filter.Filter;
				AssertCollectionNotContains(systemCode, pGateCodes);
				AssertCollectionContains(notSystemCode, pGateCodes);

				((ModuleTextFilter)filter["Is System Defined"]).Property = "All";
				((ModuleTextFilter)filter["Is System Defined"]).IsActive = true;

				pGateCodes.AdditionalFilter = filter.Filter;
				AssertCollectionContains(systemCode, pGateCodes);
				AssertCollectionContains(notSystemCode, pGateCodes);
			}
		}

		public void TestIsCotainerYardFilter()
		{
			RefPremisesGateCode containerYardCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
			RefPremisesGateCode notContainerYardCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
			containerYardCode.R5_IsContainerYard = true;
			notContainerYardCode.R5_IsContainerYard = false;

			RefPremisesGateCodeFilterBusinessObject filter = new RefPremisesGateCodeFilterBusinessObject();
			((ModuleTextFilter)filter["Is Container Yard"]).IsActive = true;
			AssertEquals("Default value", "Is Container Yard", ((ModuleTextFilter)filter["Is Container Yard"]).Property);

			RefPremisesGateCodeCollection pGateCodes = new RefPremisesGateCodeCollection(Factory);
			pGateCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(containerYardCode, pGateCodes);
			AssertCollectionNotContains(notContainerYardCode, pGateCodes);

			((ModuleTextFilter)filter["Is Container Yard"]).Property = "Is Not Container Yard";
			((ModuleTextFilter)filter["Is Container Yard"]).IsActive = true;

			pGateCodes.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains(containerYardCode, pGateCodes);
			AssertCollectionContains(notContainerYardCode, pGateCodes);

			((ModuleTextFilter)filter["Is Container Yard"]).Property = "All";
			((ModuleTextFilter)filter["Is Container Yard"]).IsActive = true;

			pGateCodes.AdditionalFilter = filter.Filter;
			AssertCollectionContains(containerYardCode, pGateCodes);
			AssertCollectionContains(notContainerYardCode, pGateCodes);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefPremisesGateCodeFilterBusinessObject();
		}

		#endregion
	}
}
