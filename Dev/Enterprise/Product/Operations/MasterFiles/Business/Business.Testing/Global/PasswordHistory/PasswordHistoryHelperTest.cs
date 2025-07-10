using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PasswordHistoryHelperTest : TestCaseWithFactory
	{
		public void TestAddPasswordHistory()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 4;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			PasswordHistoryHelper.AddPasswordHistory(staff, "One");
			PasswordHistoryHelper.AddPasswordHistory(staff, "Two");
			PasswordHistoryHelper.AddPasswordHistory(staff, "Three");
			PasswordHistoryHelper.AddPasswordHistory(staff, "Four");
			Factory.Save();

			AssertEquals(3, PasswordHistoryHelper.GetPasswordHistories(staff).Count);
			AssertEquals("One", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "One"));
			AssertEquals("Two", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Two"));
			AssertEquals("Three", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Three"));
			AssertEquals("Four", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Four"));

			EnvProxy.Instance.Registry.PasswordHistoryCount = 3;
			PasswordHistoryHelper.AddPasswordHistory(staff, "Five");
			Factory.Save();
			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(staff).Count);
			AssertEquals("One", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "One"));
			AssertEquals("Two", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Two"));
			AssertEquals("Three", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Three"));
			AssertEquals("Four", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Four"));
			AssertEquals("Five", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Five"));
		}

		public void TestAddPasswordHistory_OldestRecordDeletesFirst()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 3;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			//Faking password histories to be in future date, so they are in the order of: password2, password1
			var pw1 = PasswordHistoryHelper.NewPasswordHistory(staff, "password1");
			pw1.PWH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(1);
			var pw2 = PasswordHistoryHelper.NewPasswordHistory(staff, "password2");
			pw2.PWH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);
			Factory.Save();

			//Add a new password, but it is going to be older than the two in the list, so the list remains: password2, password1
			PasswordHistoryHelper.AddPasswordHistory(staff, "password3");
			Factory.Save();
			var collection = PasswordHistoryHelper.GetPasswordHistories(staff);
			AssertEquals(true, collection[0].IsMatched("password2"));
			AssertEquals(true, collection[1].IsMatched("password1"));

			//Change the histroy cound to 4, adding 2 new passwords, but only the last one remains, and the order should be: password5, password2, password1
			EnvProxy.Instance.Registry.PasswordHistoryCount = 4;
			PasswordHistoryHelper.AddPasswordHistory(staff, "password4");
			PasswordHistoryHelper.AddPasswordHistory(staff, "password5");
			Factory.Save();

			collection = PasswordHistoryHelper.GetPasswordHistories(staff);
			AssertEquals(true, collection[0].IsMatched("password5"));
			AssertEquals(true, collection[1].IsMatched("password2"));
			AssertEquals(true, collection[2].IsMatched("password1"));
		}

		public void TestPasswordHistoryCount()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			DataRegistry.Instance.PasswordHistoryCount = 4;

			// New password is added to GlbPasswordHistory
			staff.ResetPassword("password1");
			Factory.Save();

			var staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			var newPasswordHistory = PasswordHistoryHelper.GetPasswordHistories(staffReloaded);
			AssertEquals("New password history count", 1, newPasswordHistory.Count);

			// Now we add another password, we should have 2 in GlbPasswordHistory 
			staff.ResetPassword("password2");
			Factory.Save();

			staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			newPasswordHistory = PasswordHistoryHelper.GetPasswordHistories(staffReloaded);
			AssertEquals("New password history count", 2, newPasswordHistory.Count);

			// We add another password, we should have 3 in GlbPasswordHistory 
			staff.ResetPassword("password3");
			Factory.Save();

			staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			newPasswordHistory = PasswordHistoryHelper.GetPasswordHistories(staffReloaded);
			AssertEquals("New password history count", 3, newPasswordHistory.Count);

			// We add another password, we should have 3 in GlbPasswordHistory 
			staff.ResetPassword("password4");
			Factory.Save();

			staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			newPasswordHistory = PasswordHistoryHelper.GetPasswordHistories(staffReloaded);
			AssertEquals("New password history count", 3, newPasswordHistory.Count);
		}

		public void TestHasPasswordBeenUsed()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 2;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.AddPasswordToHistory("One");
			staff.StaffPlainTextPassword = "Two";
			Factory.Save();

			Assert("Password 'One' should be previously used", staff.HasPasswordBeenUsed("One"));
			Assert("Password 'Two' should be previously used", staff.HasPasswordBeenUsed("Two"));

			staff.ResetPassword("Three");
			Factory.Save();

			Assert("Password 'One' should not be previously used", !staff.HasPasswordBeenUsed("One"));
			Assert("Password 'Two' should be previously used", staff.HasPasswordBeenUsed("Two"));
			Assert("Password 'Three' should be previously used", staff.HasPasswordBeenUsed("Three"));
		}

		public void TestCanHandleAndReportCorruptedHistory()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 3;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			PasswordHistoryHelper.AddPasswordHistory(staff, "One");
			PasswordHistoryHelper.AddPasswordHistory(staff, "Two");
			PasswordHistoryHelper.AddPasswordHistory(staff, "Three");
			Factory.Save();

			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(staff).Count);
			AssertEquals("One", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "One"));
			AssertEquals("Two", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Two"));
			AssertEquals("Three", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Three"));

			// Corrupt the history
			var histories = PasswordHistoryHelper.GetPasswordHistories(staff);
			histories[0].PWH_Salt = null;
			Factory.Save();

			AssertEquals(2, PasswordHistoryHelper.GetPasswordHistories(staff).Count);
			AssertEquals("One", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "One"));
			AssertEquals("'Two' - Corrupted history shouldn't be matched ", false, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Two"));
			AssertEquals("Three", true, PasswordHistoryHelper.HasPasswordBeenUsed(staff, "Three"));

			AssertStartsWith("Last Report Start With", "The password history is corrupted, please investigate why this happen", ErrorReporter.LastMessageReported);
			AssertContains("PWH_Salt:", ErrorReporter.LastMessageReported);
			AssertContains($"PK: {histories[0].PK}", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.LastExceptionReported is ArgumentException);
			AssertContains("Salt is not at least eight bytes.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestGetPasswordHistories_LoadedInTheRightOrder()
		{
			var staffA = Factory.NewWithValidTestData<GlbStaff>();
			var staffB = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			PasswordHistoryHelper.NewPasswordHistory(staffA, "passwordA1");
			PasswordHistoryHelper.NewPasswordHistory(staffA, "passwordA2");
			PasswordHistoryHelper.NewPasswordHistory(staffA, "passwordA3");
			Factory.Save();
			PasswordHistoryHelper.NewPasswordHistory(staffB, "passwordB1");
			Factory.Save();
			var phB2 = PasswordHistoryHelper.NewPasswordHistory(staffB, "passwordB2");
			phB2.PWH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);// to fake a newer history
			Factory.Save();
			PasswordHistoryHelper.NewPasswordHistory(staffB, "passwordB3");
			Factory.Save();
			var phB4 = PasswordHistoryHelper.NewPasswordHistory(staffB, "passwordB4");
			phB4.PWH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1); // to fake an older history
			Factory.Save();

			var reloadedStaffA = new BusinessObjectFactory().Load<GlbStaff>(staffA.PK);
			var reloadedStaffB = new BusinessObjectFactory().Load<GlbStaff>(staffB.PK);
			var reloadedPHA = PasswordHistoryHelper.GetPasswordHistories(reloadedStaffA);
			var reloadedPHB = PasswordHistoryHelper.GetPasswordHistories(reloadedStaffB);

			AssertEquals("Staff A PasswordHistories Count", 3, reloadedPHA.Count);
			Assert("Staff A PasswordHistories[0] matched", reloadedPHA[0].IsMatched("passwordA1"));
			Assert("Staff A PasswordHistories[1] matched", reloadedPHA[1].IsMatched("passwordA2"));
			Assert("Staff A PasswordHistories[2] matched", reloadedPHA[2].IsMatched("passwordA3"));

			AssertEquals("Staff B PasswordHistories Count", 4, reloadedPHB.Count);
			Assert("Staff B PasswordHistories[0] matched", reloadedPHB[0].IsMatched("passwordB4"));
			Assert("Staff B PasswordHistories[1] matched", reloadedPHB[1].IsMatched("passwordB1"));
			Assert("Staff B PasswordHistories[2] matched", reloadedPHB[2].IsMatched("passwordB3"));
			Assert("Staff B PasswordHistories[3] matched", reloadedPHB[3].IsMatched("passwordB2"));
		}

		public void TestHashPasswordForLookup()
		{
			var staffPK = new Guid("BE35EFB1-739C-4670-AA66-3360352F2035");
			var password = "rocketScience";

			var expectedHash = new byte[] { 71, 192, 109, 138 };
			var hash = PasswordHistoryHelper.HashPasswordForLookup(staffPK, password);
			AssertEquals(expectedHash, hash);
		}

		public void TestMovePasswordHistories()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
			var destinationPerson = Factory.NewWithValidTestData<GlbPerson>();
			var sourceContact = Factory.NewWithValidTestData<OrgContact>();

			foreach (var idx in Enumerable.Range(1, 5))
			{
				sourceContact.SetHashedPassword($"c{idx}");
				Factory.Save();
			}

			foreach (var idx in Enumerable.Range(1, 5))
			{
				destinationPerson.SetHashedPassword($"p{idx}");
				Factory.Save();
			}

			foreach (var idx in Enumerable.Range(1, 5))
			{
				AssertEquals(true, sourceContact.HasPasswordBeenUsed($"c{idx}"));
				AssertEquals(true, destinationPerson.HasPasswordBeenUsed($"p{idx}"));
			}

			// before
			// destinationPerson/sourceContact has 4 password histories (PWH) and 1 current password (OC/PER)
			// sourceContact      c1[PHW] c2[PHW] c3[PHW] c4[PHW] c5[OC]
			// destinationPerson  p1[PHW] p2[PHW] p3[PHW] p4[PHW] p5[PER]
			AssertEquals(true, sourceContact.VerifyPassword("c5"));
			AssertEquals(true, destinationPerson.VerifyPassword("p5"));
			AssertEquals(4, PasswordHistoryHelper.GetPasswordHistories(sourceContact).Count);
			AssertEquals(4, PasswordHistoryHelper.GetPasswordHistories(destinationPerson).Count);

			//moves sourceContact's password history to sourceContact.Person
			sourceContact.RemovePasswordAndHash();
			Factory.Save();
			// sourceContact [nothing]
			// sourceContact.Person c1[PHW] c2[PHW] c3[PHW] c4[PHW] c5[PHW]
			AssertEquals(true, sourceContact.OC_PasswordHash.IsEmpty);
			AssertEquals(0, PasswordHistoryHelper.GetPasswordHistories(sourceContact).Count);
			AssertEquals(5, PasswordHistoryHelper.GetPasswordHistories(sourceContact.Person).Count);

			PasswordHistoryHelper.MovePasswordHistories(sourceContact.Person, destinationPerson);
			Factory.Save();

			// after
			// sourceContact.Person has 0 PWH and 0 current password
			// destinationPerson    has 6 PWH and 1 current password
			// destinationPerson.Person [nothing]
			// destinationPerson           c1[PHW][deleted] c2[PHW][deleted] c3[PHW][deleted]
			//                             c4[PHW] p1[PHW] p2[PHW] p3[PHW] p4[PHW] c5[PHW] p5[PER]
			// because the c5 password history is created with utcNow, it's on the bottom of the list.
			AssertEquals(true, sourceContact.OC_PasswordHash.IsEmpty);
			AssertEquals(true, sourceContact.Person.PER_PasswordHash.IsEmpty);
			AssertEquals(0, PasswordHistoryHelper.GetPasswordHistories(sourceContact).Count);
			AssertEquals(0, PasswordHistoryHelper.GetPasswordHistories(sourceContact.Person).Count);
			var personPasswordHistories = PasswordHistoryHelper.GetPasswordHistories(destinationPerson);
			AssertEquals(6, personPasswordHistories.Count);
			AssertEquals(true, destinationPerson.VerifyPassword("p5"));
			AssertEquals(true, personPasswordHistories[0].VerifyPassword("c4"));
			AssertEquals(true, personPasswordHistories[1].VerifyPassword("p1"));
			AssertEquals(true, personPasswordHistories[2].VerifyPassword("p2"));
			AssertEquals(true, personPasswordHistories[3].VerifyPassword("p3"));
			AssertEquals(true, personPasswordHistories[4].VerifyPassword("p4"));
			AssertEquals(true, personPasswordHistories[5].VerifyPassword("c5"));
		}
	}
}
