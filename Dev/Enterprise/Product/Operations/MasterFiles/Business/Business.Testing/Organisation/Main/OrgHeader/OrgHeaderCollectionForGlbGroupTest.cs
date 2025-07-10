using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderCollectionForGlbGroup))]
	sealed class OrgHeaderCollectionForGlbGroupTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			return new OrgHeaderCollectionForGlbGroup(group);
		}

		public void TestCollectionLoad()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_GG_OrgSecurityGroup = group.PK;
			Factory.Save();

			var collection = new OrgHeaderCollectionForGlbGroup(group);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(org, collection[0]);
		}

		public void TestAddRemoveOrg()
		{
			var groupA = Factory.NewWithValidTestData<GlbGroup>();
			var groupB = Factory.NewWithValidTestData<GlbGroup>();
			var collectionA = new OrgHeaderCollectionForGlbGroup(groupA);
			var collectionB = new OrgHeaderCollectionForGlbGroup(groupB);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			collectionA.Add(org);
			AssertEquals(groupA.PK, org.MiscServ.OM_GG_OrgSecurityGroup);

			collectionB.Add(org);
			AssertEquals(groupB.PK, org.MiscServ.OM_GG_OrgSecurityGroup);

			collectionB.Remove(org);
			AssertEquals(ZGuid.Empty, org.MiscServ.OM_GG_OrgSecurityGroup);
		}
	}
}
