using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRelatedPartyDependentCollection))]
	sealed class OrgRelatedPartyDependantCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgRelatedPartyDependentCollection(GetOrganisation("John"), Factory);
		}

		public void TestCreateDetachedLogWhenDeletingRelatedParty()
		{
			var org = GetOrganisation("Test");
			var relatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = org.PK;

			Factory.Save();

			var collection = new OrgRelatedPartyDependentCollection(org, Factory);
			collection.Load();
			collection.RemoveAndDelete(relatedParty);
			var detachedLogs = org.Logs.Find(l => l.SL_Reference.Contains("Detached"));
			AssertEquals(1, detachedLogs.Count());
		}

		OrgHeader GetOrganisation(ZString name)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = name;
			org.OH_RL_NKClosestPort = "AUSYD";

			return org;
		}
	}
}
