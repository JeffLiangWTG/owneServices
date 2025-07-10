using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationProcessTaskCollection))]
	sealed class HRJobApplicationProcessTaskCollectionTest : ProcessTaskCollectionTest<HRJobApplicationProcessTaskCollection>
	{
		protected override HRJobApplicationProcessTaskCollection GetCollectionToTestCore()
		{
			return new HRJobApplicationProcessTaskCollection(Application);
		}

		HRJobApplication Application
		{
			get { return application ?? (application = Factory.New<HRJobApplication>()); }
		}
		HRJobApplication application;

		public void TestRelationFilterSet()
		{
			var collection = new HRJobApplicationProcessTaskCollection(Application, ZQuery.NoResultQuery);
			AssertEquals(ZQuery.NoResultQuery, collection.CompleteFilter);
		}
	}
}
