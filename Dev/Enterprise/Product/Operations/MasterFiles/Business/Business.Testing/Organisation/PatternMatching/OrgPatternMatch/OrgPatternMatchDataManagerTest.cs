using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching.Testing
{
	sealed class OrgPatternMatchDataManagerTest : TestCaseWithFactory
	{
		public void TestConstructorChecksForNulls()
		{
			var collection = new OrgPatternMatchCollection(Factory);
			var orgHeader = Factory.New<OrgHeader>();

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new OrgPatternMatchDataManager(null, collection); });
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new OrgPatternMatchDataManager(orgHeader, null); });
			AssertNoExceptionThrown(delegate
			{ new OrgPatternMatchDataManager(orgHeader, collection); });
		}

		public void TestOrganisation()
		{
			var collection = new OrgPatternMatchCollection(Factory);
			var orgHeader = Factory.New<OrgHeader>();

			var manager = new OrgPatternMatchDataManager(orgHeader, collection);

			AssertEquals("manager.Organisation", orgHeader, manager.Organisation);
		}

		public void TestCollectionAndDataManagement()
		{
			var collection = new OrgPatternMatchCollection(Factory);
			var orgHeader = Factory.New<OrgHeader>();

			var manager = new OrgPatternMatchDataManager(orgHeader, collection);

			var patternMatch1 = manager.CreateNewPatternMatch();
			AssertNotNull("manager.CreateNewPatternMatch()", patternMatch1);
			AssertEquals("collection.Contains(patternMatch1.PK)", true, collection.Contains(patternMatch1.PK));

			var matchesLoaded = manager.GetPatternMatchesAlreadyLoaded();
			AssertEquals("matchesLoaded.Count()", 1, matchesLoaded.Count());
			AssertEquals("matchesLoaded.First()", patternMatch1, matchesLoaded.First());

			var patternMatch2 = manager.CreateNewPatternMatch();
			AssertNotNull("manager.CreateNewPatternMatch()", patternMatch2);
			AssertEquals("collection.Contains(patternMatch2.PK)", true, collection.Contains(patternMatch2.PK));

			manager.DeletePatternMatches(new ZQuery(OrgPatternMatchSchema.PK, patternMatch1.PK));

			AssertEquals("patternMatch1.IsDeleted", true, patternMatch1.IsDeleted);
			AssertEquals("patternMatch2.IsDeleted", false, patternMatch2.IsDeleted);
		}
	}
}
