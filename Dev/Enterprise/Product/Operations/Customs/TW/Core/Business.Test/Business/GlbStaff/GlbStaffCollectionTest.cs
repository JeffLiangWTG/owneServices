using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWGlbStaffCollection))]
	sealed class GlbStaffCollectionTest : ActiveBusinessObjectCollectionTestCase<TWGlbStaffCollection>
	{
		[ExpectNoExceptions]
		public void TestRelationshipFilter()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;
			var cantLoginStaff = Factory.NewWithValidTestData<GlbStaff>();
			cantLoginStaff.GS_CanLogin = false;
			Factory.Save();
			var coll = new TWGlbStaffCollection(Factory);
			NUnit.Framework.Assert.That(coll, NUnit.Framework.Has.Some.EqualTo(cantLoginStaff).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(coll, NUnit.Framework.Has.None.EqualTo(resource).Using(CustomComparers.TypeComparison));
		}

		protected override TWGlbStaffCollection GetCollectionToTest()
		{
			return new TWGlbStaffCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			return staff;
		}
	}
}
