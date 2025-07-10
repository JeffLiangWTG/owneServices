using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefNMFCFilterBusinessObject))]
	sealed class RefNMFCFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestDescriptionFilter()
		{
			RefNMFC nmfc1 = Factory.NewWithValidTestData<RefNMFC>();
			RefNMFC nmfc2 = Factory.NewWithValidTestData<RefNMFC>();
			nmfc1.FN_ItemNo = "892";
			nmfc1.FN_Class = "123";
			nmfc1.FN_Description = "SEA";

			nmfc2.FN_ItemNo = "459";
			nmfc2.FN_Class = "323";
			nmfc2.FN_Description = "AIR";

			Factory.Save();

			RefNMFCFilterBusinessObject filter = new RefNMFCFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "SEA";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			RefNMFCCollection nmfcCollection = new RefNMFCCollection(Factory);
			nmfcCollection.AdditionalFilter = filter.Filter;

			AssertCollectionContains(nmfc1, nmfcCollection);
			AssertCollectionNotContains(nmfc2, nmfcCollection);
		}

		public void TestItemNoFilter()
		{
			RefNMFC nmfc1 = Factory.NewWithValidTestData<RefNMFC>();
			RefNMFC nmfc2 = Factory.NewWithValidTestData<RefNMFC>();
			nmfc1.FN_ItemNo = "123";
			nmfc1.FN_Class = "890";

			nmfc2.FN_ItemNo = "321";
			nmfc2.FN_Class = "879";

			Factory.Save();

			RefNMFCFilterBusinessObject filter = new RefNMFCFilterBusinessObject();
			((ModuleTextFilter)filter["Item No"]).Property = "123";
			((ModuleTextFilter)filter["Item No"]).IsActive = true;

			RefNMFCCollection nmfcCollection = new RefNMFCCollection(Factory);
			nmfcCollection.AdditionalFilter = filter.Filter;

			AssertCollectionContains(nmfc1, nmfcCollection);
			AssertCollectionNotContains(nmfc2, nmfcCollection);
		}

		public void TestClassFilter()
		{
			RefNMFC nmfc1 = Factory.NewWithValidTestData<RefNMFC>();
			RefNMFC nmfc2 = Factory.NewWithValidTestData<RefNMFC>();
			nmfc1.FN_ItemNo = "123";
			nmfc1.FN_Class = "673";

			nmfc2.FN_ItemNo = "123";
			nmfc2.FN_Class = "184";

			Factory.Save();

			RefNMFCFilterBusinessObject filter = new RefNMFCFilterBusinessObject();
			((ModuleTextFilter)filter["Class"]).Property = "673";
			((ModuleTextFilter)filter["Class"]).IsActive = true;

			RefNMFCCollection nmfcCollection = new RefNMFCCollection(Factory);
			nmfcCollection.AdditionalFilter = filter.Filter;

			AssertCollectionContains(nmfc1, nmfcCollection);
			AssertCollectionNotContains(nmfc2, nmfcCollection);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefNMFCFilterBusinessObject();
		}

		#endregion
	}
}
