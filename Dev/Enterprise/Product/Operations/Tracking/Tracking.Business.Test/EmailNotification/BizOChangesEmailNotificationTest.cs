using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestsSubclassesOf(typeof(IBizOChangesEmailNotification), ExcludeClientDlls = true)]
	public abstract class BizOChangesEmailNotificationTest<T> : TestCaseWithFactory where T : IBizOChangesEmailNotification
	{
		public void TestPropertyRecordingIsMultilingual()
		{
			using (var languageTestHelper = new BusinessObjectChangesEmailNotifierTest.LanguageTestHelper())
			{
				var bizO = GetNewBizOForNotification();
				var notifier = new BusinessObjectChangesEmailNotifier(bizO);
				bizO.Factory.Save();
				languageTestHelper.AssertEmailLanguage(Core.SharedConstants.Languages.EnglishAmerican);
			}
		}

		protected abstract T GetNewBizOForNotification();

		protected override void SetUp()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@cargowise.com";
			Factory.Save();

			base.SetUp();
		}
	}
}
