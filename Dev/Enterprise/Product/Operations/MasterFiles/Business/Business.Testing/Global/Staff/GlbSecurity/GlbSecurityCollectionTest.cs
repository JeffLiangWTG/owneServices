using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbSecurityCollection))]
	sealed class GlbSecurityCollectionTest : BusinessObjectCollectionTestCase
	{
		new GlbSecurityCollection Collection
		{
			get { return (GlbSecurityCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbSecurityCollection(Factory);
		}

		public void TestGlbSecurityCollectionForGroup()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbSecurityCollection collection = new GlbSecurityCollection(group, Factory);
			collection.AddNew();
			AssertEquals("Group PK is set by default.", group.PK, collection[0].GU_GG);
		}

		public void TestGlbSecurityCollectionForStaff()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			GlbSecurityCollection collection = new GlbSecurityCollection(staff, Factory);
			collection.AddNew();
			AssertEquals("Staff PK is set by default.", staff.PK, collection[0].GU_GS);
		}

		public void TestFilter()
		{
			GlbStaff staffAlice = Factory.New<GlbStaff>();
			staffAlice.GS_Code = "AL";
			staffAlice.GS_LoginName = "Alice";

			GlbStaff staffBob = Factory.New<GlbStaff>();
			staffBob.GS_Code = "BO";
			staffBob.GS_LoginName = "Bob";

			GlbGroup groupYankee = Factory.New<GlbGroup>();
			groupYankee.GG_Code = "YA";

			GlbGroup groupZulu = Factory.New<GlbGroup>();
			groupZulu.GG_Code = "ZU";

			staffAlice.Groups.Add(groupYankee);

			// GlbGroup's SetDefaultValues creates GlbSecurity objects, so we have to clean them up down here.	
			foreach (BusinessObject bizO in Factory.Load<GlbSecurity>(new ZQuery()))
			{
				bizO.Delete();
			}

			// Tell the factory that it shouldn't hit the DB for GlbSecurity queries.
			Factory.SeedQueryCache(GlbSecuritySchema.Constants.TableName, new ZQuery());

			GlbSecurity securityAlice = Factory.New<GlbSecurity>();
			securityAlice.GU_GS = staffAlice.PK;

			GlbSecurity securityBob = Factory.New<GlbSecurity>();
			securityBob.GU_GS = staffBob.PK;

			GlbSecurity securityYankee = Factory.New<GlbSecurity>();
			securityYankee.GU_GG = groupYankee.PK;

			GlbSecurity securityZulu = Factory.New<GlbSecurity>();
			securityZulu.GU_GG = groupZulu.PK;

			Collection.Load();
			AssertEquals("Should load all GlbSecurities.", 4, Collection.Count);

			var collectionA = new GlbSecurityCollection(staffAlice, Factory);
			collectionA.Load();
			AssertEquals(2, collectionA.Count);
			AssertCollectionContains(securityAlice, Collection);
			AssertCollectionContains(securityYankee, Collection);

			var collectionB = new GlbSecurityCollection(staffBob, Factory);
			collectionB.Load();
			AssertEquals(1, collectionB.Count);
			AssertCollectionContains(securityBob, Collection);

			var collectionY = new GlbSecurityCollection(groupYankee, Factory);
			collectionY.Load();
			AssertEquals(1, collectionB.Count);
			AssertCollectionContains(securityYankee, Collection);

			var collectionZ = new GlbSecurityCollection(groupZulu, Factory);
			collectionZ.Load();
			AssertEquals(1, collectionB.Count);
			AssertCollectionContains(securityZulu, Collection);
		}

		public void TestOnRemoving()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbSecurityCollection collection = new GlbSecurityCollection(group, Factory);
			AssertEquals("Collection should NOT have changes.", false, collection.HasChanges);

			GlbSecurity item1 = collection.AddNew();
			item1.GU_SecurityRight = "Blah";
			AssertEquals("Item added, collection should have changes.", true, collection.HasChanges);

			Factory.Save();
			AssertEquals("Collection saved should NOT have changes.", false, collection.HasChanges);

			collection.Remove(item1);
			AssertEquals("Item removed, collection should have changes.", true, collection.HasChanges);
		}

		public void TestSecurity()
		{
			AssertNull("Security", Collection.Security);
			SecurityCore security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			Collection.Security = security;
			AssertEquals("Security", security, Collection.Security);
		}

		public void TestResetToGroupsDefaults()
		{
			GlbStaff alex = Factory.New<GlbStaff>();
			alex.GS_Code = "A.K";
			alex.GS_LoginName = "Alex";

			GlbStaff shmalex = Factory.New<GlbStaff>();
			shmalex.GS_Code = "SH";
			shmalex.GS_LoginName = "Shmalex";

			GlbGroup boys = Factory.New<GlbGroup>();
			boys.GG_Code = "BOY";

			alex.Groups.Add(boys);
			shmalex.Groups.Add(boys);

			GlbSecurity securityForAlex = Factory.New<GlbSecurity>();
			securityForAlex.GU_GS = alex.PK;

			GlbSecurity securityForShmalex = Factory.New<GlbSecurity>();
			securityForShmalex.GU_GS = shmalex.PK;

			GlbSecurityCollection collectionForAlex = new GlbSecurityCollection(alex, Factory);
			GlbSecurityCollection collectionForShmalex = new GlbSecurityCollection(shmalex, Factory);
			collectionForAlex.Load();
			collectionForShmalex.Load();

			Assert("Precondition: there is staff's permission 'manually' set", collectionForAlex.Contains(securityForAlex));
			Assert("Precondition: there is staff's permission 'manually' set", collectionForShmalex.Contains(securityForShmalex));

			collectionForAlex.ResetToGroupsDefaults();

			Assert("staff's permission for Alex should be reset to default groups' and not contain 'manually' set permissions", !collectionForAlex.Contains(securityForAlex));
			Assert("staff's permission for Shmalex should NOT be reset to default groups' permissions and should contain 'manually' set permissions", collectionForShmalex.Contains(securityForShmalex));

			GlbSecurityCollection securityForBoys = new GlbSecurityCollection(boys, Factory);
			securityForBoys.Load();
			int groupsPermissionCount = securityForBoys.Count;

			securityForBoys.ResetToGroupsDefaults();

			AssertEquals("After reset group still has to have all its security permissions", securityForBoys.Count, groupsPermissionCount);
		}

		#region ShouldRemoveLookupKeyWhenGlbSecurityWasDeleted

		public void TestShouldRemoveLookupKeyWhenGlbSecurityWasDeleted_GroupSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var securityCollection = new GlbSecurityCollection(group, Factory);

			var securityItem = new SecurityCore(securityCollection, group, Env.CurrentBranch.PK, Env.CurrentCompany.PK, Env.CurrentDepartment.PK);
			var securityRight = securityItem.CompanyTariffRates.Code;

			var security = AddNewSecurity(false, group.PK, securityRight);

			securityCollection.Add(security);

			var lookupKey = GlbSecurityRightsLookupKey.ForLookup(securityItem.CompanyTariffRates, group.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			var securitys = securityCollection.Find(lookupKey, true);
			AssertEquals(security, securitys[0]);

			security.Delete();
			AssertNoExceptionThrown(() => securityCollection.Find(lookupKey, true));

			security = AddNewSecurity(false, group.PK, securityRight);
			securityCollection.Add(security);

			securitys = securityCollection.Find(lookupKey, false);
			AssertEquals(security, securitys[0]);

			security.Delete();
			AssertNoExceptionThrown(() => securityCollection.Find(lookupKey, false));
		}

		public void TestShouldRemoveLookupKeyWhenGlbSecurityWasDeleted_StaffSecurity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var securityCollection = new GlbSecurityCollection(staff, Factory);

			var securityItem = new SecurityCore(securityCollection, staff, Env.CurrentBranch.PK, Env.CurrentCompany.PK, Env.CurrentDepartment.PK);
			var securityRight = securityItem.CompanyTariffRates.Code;

			var security = AddNewSecurity(true, staff.PK, securityRight);

			securityCollection.Add(security);

			var lookupKey = GlbSecurityRightsLookupKey.ForLookup(securityItem.CompanyTariffRates, staff.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			var securitys = securityCollection.Find(lookupKey, false);
			AssertEquals(security, securitys[0]);

			security.Delete();
			AssertNoExceptionThrown(() => securityCollection.Find(lookupKey, false));

			security = AddNewSecurity(true, staff.PK, securityRight);
			securityCollection.Add(security);

			securitys = securityCollection.Find(lookupKey, true);
			AssertEquals(security, securitys[0]);

			security.Delete();
			AssertNoExceptionThrown(() => securityCollection.Find(lookupKey, true));
		}

		GlbSecurity AddNewSecurity(bool isForStaff, ZGuid pk, string securityRight)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityItemIsAllowed = false;
			security.GU_SecurityRight = securityRight;

			if (isForStaff)
			{
				security.GU_GS = pk;
			}
			else
			{
				security.GU_GG = pk;
			}

			return security;
		}

		#endregion
	}
}
