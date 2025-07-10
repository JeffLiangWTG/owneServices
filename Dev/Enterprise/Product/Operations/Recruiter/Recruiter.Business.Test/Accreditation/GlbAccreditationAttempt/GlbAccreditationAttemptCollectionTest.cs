using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptCollection))]
	sealed class GlbAccreditationAttemptCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbAccreditationAttemptCollection(Factory);
		}

		public void TestAllowNew()
		{
			var collection = new GlbAccreditationAttemptCollection(Factory);
			var attempt = collection.AddNew();
			AssertNotNull(attempt);
			AssertEquals(false, collection.AllowNew);
		}
	}
}
