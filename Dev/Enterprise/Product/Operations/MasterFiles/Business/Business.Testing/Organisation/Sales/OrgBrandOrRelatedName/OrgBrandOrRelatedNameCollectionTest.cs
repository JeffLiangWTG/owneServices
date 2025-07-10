using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgBrandOrRelatedNameCollection))]
	sealed class OrgBrandOrRelatedNameCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			return new OrgBrandOrRelatedNameCollection(testHeader);
		}

		public void TestAddNewWithBrandName()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();

			AssertEquals("Brand Count 0", 0, testHeader.BrandsOrRelatedNames.Count);
			testHeader.BrandsOrRelatedNames.AddNew(BrandName);
			Assert("New Brand Addes", testHeader.BrandsOrRelatedNames.Contains(BrandName));
		}

		public void TestStringIndex()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.BrandsOrRelatedNames.AddNew(BrandName);

			AssertNotNull(testHeader.BrandsOrRelatedNames[BrandName]);
		}

		public void TestContains()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.BrandsOrRelatedNames.AddNew(BrandName);

			Assert(testHeader.BrandsOrRelatedNames.Contains(BrandName));
		}

		public void TestRemoveAndDelete()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.OH_Code = "Dodgy";
			testHeader.Factory.Save();
			var isFindingDuplicates = false;
			testHeader.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
			((IDeduplicatable)testHeader).ShouldRunDeduplication = true;

			CreateBrandOrRelatedNameAndTryDeleteAndAssertCollectionCount(testHeader, "Test1", 0);
			Assert(isFindingDuplicates);
			isFindingDuplicates = false;

			testHeader.BrandsOrRelatedNames.AddNew("Test2");
			CreateBrandOrRelatedNameAndTryDeleteAndAssertCollectionCount(testHeader, "Test3", 1);
			Assert(isFindingDuplicates);
			isFindingDuplicates = false;

			testHeader.BrandsOrRelatedNames.AddNew("Test4");
			CreateBrandOrRelatedNameAndTryDeleteAndAssertCollectionCount(testHeader, "Test5", 2);
			Assert(isFindingDuplicates);
			isFindingDuplicates = false;

			testHeader.BrandsOrRelatedNames.AddNew("Test6");
			CreateBrandOrRelatedNameAndTryDeleteAndAssertCollectionCount(testHeader, "Test7", 3);
			Assert(isFindingDuplicates);
			isFindingDuplicates = false;
		}

		void CreateBrandOrRelatedNameAndTryDeleteAndAssertCollectionCount(OrgHeader master, string brand, int expectedCountAfterDelete)
		{
			var brandBizo = master.BrandsOrRelatedNames.AddNew();
			brandBizo.P1_RelatedName = brand;
			Factory.Save();
			master.BrandsOrRelatedNames.RemoveAndDelete(brandBizo);
			AssertEquals(string.Format("Should have [{0}] BrandsOrRelatedNames after attempting to delete", expectedCountAfterDelete),
				expectedCountAfterDelete, master.BrandsOrRelatedNames.Count);
		}

		public void TestSettingPatternRequiresRegen()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgBrandOrRelatedNameCollection collection = new OrgBrandOrRelatedNameCollection(org);
			OrgBrandOrRelatedName bo = collection.AddNew();
			collection.Master.PatternMatchRequiresRegen = false;
			Assert(!collection.Master.PatternMatchRequiresRegen);
			collection.Remove(bo);
			Assert(collection.Master.PatternMatchRequiresRegen);

			bo = collection.AddNew();
			collection.Master.PatternMatchRequiresRegen = false;
			Assert(!collection.Master.PatternMatchRequiresRegen);
			collection.RemoveAndDelete(bo);
			Assert(collection.Master.PatternMatchRequiresRegen);
		}

		const string BrandName = "Timmy O' Toole Industries Pty Ltd";
	}
}
