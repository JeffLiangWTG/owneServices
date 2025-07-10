using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PatternMatchingRemoverTest : TestCaseWithFactory
	{
		public void TestDeleteAllPatterns()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();

			var patternMatchingDomain = Factory.NewWithValidTestData<PatternMatchingDomain>();
			patternMatchingDomain.ParentId = orgContact.PK;
			patternMatchingDomain.ParentTableCode = orgContact.TablePrefix;

			var patternMatchingEmail = Factory.NewWithValidTestData<PatternMatchingEmail>();
			patternMatchingEmail.ParentId = orgContact.PK;
			patternMatchingEmail.ParentTableCode = orgContact.TablePrefix;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.ParentId = orgContact.PK;
			patternMatchingName.ParentTableCode = orgContact.TablePrefix;

			var patternMatchingPhone = Factory.NewWithValidTestData<PatternMatchingPhone>();
			patternMatchingPhone.ParentId = orgContact.PK;
			patternMatchingPhone.ParentTableCode = orgContact.TablePrefix;

			var patternMatchingRegCode = Factory.NewWithValidTestData<PatternMatchingRegCode>();
			patternMatchingRegCode.ParentId = orgContact.PK;
			patternMatchingRegCode.ParentTableCode = orgContact.TablePrefix;
			Factory.Save();

			PatternMatchingRemover.DeleteAll(orgContact);

			Assert(patternMatchingDomain.IsDeleted);
			Assert(patternMatchingEmail.IsDeleted);
			Assert(patternMatchingName.IsDeleted);
			Assert(patternMatchingPhone.IsDeleted);
			Assert(patternMatchingRegCode.IsDeleted);
		}
	}
}
