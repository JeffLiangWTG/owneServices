using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPasswordHistory))]
	sealed class GlbPasswordHistoryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocumentMacroIgnore_Password()
		{
			var passwordHashInfo = typeof(GlbPasswordHistory).GetProperty("PWH_Hash");
			var passwordSaltInfo = typeof(GlbPasswordHistory).GetProperty("PWH_Salt");

			Assert("PWH_Hash should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordHashInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("PWH_Salt should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordSaltInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = Factory.NewWithValidTestData<GlbPasswordHistory>();
			bizo.PWH_ParentTableCode = "OC";
			bizo.PWH_Algorithm = "PHS1";
			bizo.PWH_IterationCount = 1;
			return bizo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			DataRegistry.Instance.PasswordHistoryCount = 3; // to ensure the new bizo is not deleted because of no history count
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public void TestIsMatched()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var passwordText = "mysecretpassword";
			var passwordHistory = PasswordHistoryHelper.NewPasswordHistory(staff, passwordText);
			var lookupHash = PasswordHistoryHelper.HashPasswordForLookup(staff.PK.ToGuid(), passwordText);
			var wrongLookupHash1 = PasswordHistoryHelper.HashPasswordForLookup(Guid.NewGuid(), passwordText);
			var wrongLookupHash2 = PasswordHistoryHelper.HashPasswordForLookup(staff.PK.ToGuid(), "xyz");

			AssertEquals("Match", true, passwordHistory.IsMatched("mysecretpassword"));
			AssertEquals("Match", true, passwordHistory.IsMatched("mysecretpassword", lookupHash));
			AssertEquals("Not Match, wrong lookup hash 1", false, passwordHistory.IsMatched("mysecretpassword", wrongLookupHash1));
			AssertEquals("Not Match, wrong lookup hash 2", false, passwordHistory.IsMatched("mysecretpassword", wrongLookupHash2));
			AssertEquals("Not Match", false, passwordHistory.IsMatched("crap"));
			AssertEquals("Not Match", false, passwordHistory.IsMatched("crap", lookupHash));
			AssertEquals("Not Match", false, passwordHistory.IsMatched("mysecretpassword1"));
			AssertEquals("Not Match", false, passwordHistory.IsMatched("mysecretpasswor"));
			AssertEquals("Not Match", false, passwordHistory.IsMatched(""));

			// empty lookup hash
			passwordHistory.PWH_TruncatedHash = null;
			AssertEquals("Match - empty lookup hash", true, passwordHistory.IsMatched("mysecretpassword"));
			AssertEquals("Match - empty lookup hash", true, passwordHistory.IsMatched("mysecretpassword", lookupHash));
			AssertEquals("Match - empty lookup hash, ignore wrong lookup hash 1", true, passwordHistory.IsMatched("mysecretpassword", wrongLookupHash1));
			AssertEquals("Match - empty lookup hash, ignore wrong lookup hash 2", true, passwordHistory.IsMatched("mysecretpassword", wrongLookupHash2));
			AssertEquals("Not Match - empty lookup hash", false, passwordHistory.IsMatched("crap"));
			AssertEquals("Not Match - empty lookup hash", false, passwordHistory.IsMatched("crap", lookupHash));
			AssertEquals("Not Match - empty lookup hash", false, passwordHistory.IsMatched("mysecretpassword1"));
			AssertEquals("Not Match - empty lookup hash", false, passwordHistory.IsMatched("mysecretpasswor"));
			AssertEquals("Not Match - empty lookup hash", false, passwordHistory.IsMatched(""));

			// invalid algorithm
			passwordHistory.PWH_Algorithm = "Invalid";
			AssertEquals("Invalid algorithm", false, passwordHistory.IsMatched("mysecretpassword"));
			AssertContains("PWH_Algorithm: Invalid", ErrorReporter.LastMessageReported);
			AssertContains($"PWH_PK: {passwordHistory.PK}", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.LastExceptionReported is ArgumentException);
			AssertContains("Value 'Invalid' is not a valid string representation of a hash kind.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestShouldNotAddCorruptedPasswordToHistory()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 2;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.AddPasswordToHistory("One");
			staff.StaffPlainTextPassword = "Two";
			Factory.Save();

			AssertEquals("Password 'One' should be previously used", true, staff.HasPasswordBeenUsed("One"));
			AssertEquals("Password 'Two' should be previously used", true, staff.HasPasswordBeenUsed("Two"));

			// Corrupt current password
			staff.GS_PasswordHashIterations = 0;
			staff.GS_PasswordSalt = null;
			Factory.Save();

			AssertEquals("Password 'One' should be previously used", true, staff.HasPasswordBeenUsed("One"));
			AssertEquals("Password 'Two' has been corrupted and should not be matched", false, staff.HasPasswordBeenUsed("Two"));

			staff.ResetPassword("Three");
			AssertNoExceptionThrown("Should not throw when attempt to save corrupted password to history.", () => Factory.Save());

			AssertEquals("Corrupted password 'Two' should not be added to history", 1, PasswordHistoryHelper.GetPasswordHistories(staff).Count);
			AssertEquals("Corrupted password 'Two' should not be added to history", false, staff.HasPasswordBeenUsed("Two"));
			AssertEquals("Password 'One' should be previously used", true, staff.HasPasswordBeenUsed("One"));
			AssertEquals("Password 'Three' should be previously used", true, staff.HasPasswordBeenUsed("Three"));

			AssertStartsWith("Last Report Start With", "The staff password is corrupted, please investigate why this happen", ErrorReporter.LastMessageReported);
			AssertContains("GS_PasswordSalt:", ErrorReporter.LastMessageReported);
			AssertContains("GS_PasswordHashIterations: 0", ErrorReporter.LastMessageReported);
			AssertContains($"GS_PK: {staff.PK}", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.LastExceptionReported is ArgumentException);
			AssertContains("Salt is not at least eight bytes.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestLoadByParentIDAndCode()
		{
			EnvProxy.Instance.Registry.PasswordHistoryCount = 5;
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.SetHashedPassword("c123");
			contact.SetHashedPassword("c456");

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.SetHashedPassword("p123");
			person.SetHashedPassword("p456");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.AddPasswordToHistory("s123");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var passwordHistory = newFactory.Load<GlbPasswordHistory>(new ZQuery(GlbPasswordHistorySchema.PWH_ParentID, contact.PK)
										.AddToFilter(GlbPasswordHistorySchema.PWH_ParentTableCode, contact.TablePrefix)).Single();
			AssertEquals(false, passwordHistory.VerifyPassword("c456"));
			AssertEquals(true, passwordHistory.VerifyPassword("c123"));

			passwordHistory = newFactory.Load<GlbPasswordHistory>(new ZQuery(GlbPasswordHistorySchema.PWH_ParentID, person.PK)
										.AddToFilter(GlbPasswordHistorySchema.PWH_ParentTableCode, person.TablePrefix)).Single();
			AssertEquals(false, passwordHistory.VerifyPassword("p456"));
			AssertEquals(true, passwordHistory.VerifyPassword("p123"));

			passwordHistory = newFactory.Load<GlbPasswordHistory>(new ZQuery(GlbPasswordHistorySchema.PWH_ParentID, staff.PK)
										.AddToFilter(GlbPasswordHistorySchema.PWH_ParentTableCode, staff.TablePrefix)).Single();
			AssertEquals(false, passwordHistory.VerifyPassword("s456"));
			AssertEquals(true, passwordHistory.VerifyPassword("s123"));
		}
	}
}
