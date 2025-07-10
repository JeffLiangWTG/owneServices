using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationFilterBusinessObject))]
	public class GlbAccreditationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_Code = "AC1";
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred2.HAC_Code = "AC2";
			var filter = GetNewFilterStripBusinessObject()["Code"] as ModuleTextFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "AC1";
			var collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			filter.Property = "AC";
			collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			AssertCollectionContains(accred2, collection);
		}

		public void TestDescriptionFilter()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_Code = "AC1";
			accred1.HAC_Description = "description 1";
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred2.HAC_Code = "AC2";
			accred2.HAC_Description = "description 2";
			var filter = GetNewFilterStripBusinessObject()["Description"] as ModuleTextFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "description 1";
			var collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			filter.Property = "description";
			collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			AssertCollectionContains(accred2, collection);
		}

		public void TestCertificateCodeFilter()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_Code = "AC1";
			accred1.HAC_CertificateCode = "CC1";
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred2.HAC_Code = "AC2";
			accred2.HAC_CertificateCode = "CC2";
			var filter = GetNewFilterStripBusinessObject()["Certificate Code"] as ModuleTextFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "CC1";
			var collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			filter.Property = "CC";
			collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			AssertCollectionContains(accred2, collection);
		}

		public void TestWebPublishedFilter()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_IsWebPublished = true;
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred2.HAC_IsWebPublished = false;
			var filter = GetNewFilterStripBusinessObject()["Web Published"] as ModuleFlagsFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property0 = true;
			var collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			filter.Property0 = false;
			collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred2, collection);
		}

		public void TestIsRefresherFilter()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_IsRefresher = true;
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred2.HAC_IsRefresher = false;
			var filter = GetNewFilterStripBusinessObject()["Is Refresher"] as ModuleFlagsFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property0 = true;
			var collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred1, collection);
			filter.Property0 = false;
			collection = new GlbAccreditationCollection(Factory, filter.Query);
			AssertCollectionContains(accred2, collection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbAccreditationFilterBusinessObject();
		}
	}
}
