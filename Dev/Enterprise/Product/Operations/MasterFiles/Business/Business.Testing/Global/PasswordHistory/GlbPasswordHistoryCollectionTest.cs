using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPasswordHistoryCollection))]
	sealed class GlbPasswordHistoryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			return new GlbPasswordHistoryCollection(staff);
		}
	}
}
