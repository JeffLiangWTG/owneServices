using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffManagerCollection))]
	sealed class GlbStaffManagerCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbStaffManagerCollection>
	{
		public void TestConstructors()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var collectionA = new GlbStaffManagerCollection(staff, new ZQuery());
			var newStaffManagerA = collectionA.AddNew();
			AssertEquals(ZString.Empty, newStaffManagerA.GSM_ManagerType);

			var collectionB = new GlbStaffManagerCollection(staff, "DRM");
			AssertContains("and (GSM_ManagerType = 'DRM')", collectionB.CompleteFilter.LiteralTextADO);
			var newStaffManagerB = collectionB.AddNew();
			AssertEquals("DRM", newStaffManagerB.GSM_ManagerType);
		}

		#region Implementation

		protected override GlbStaffManagerCollection GetCollectionToTest()
		{
			return new GlbStaffManagerCollection(Factory);
		}

		#endregion
	}
}
