using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using static Enterprise.Environment.Testing.UserContextTest;

namespace Enterprise.Customs.ZA.Business.MessagingProcess.Testing
{
	sealed class MessagingPOCHelperTest : TestCaseWithFactory
	{
		public void TestIsPOCActive()
		{
			var user = new UserForTest();
			var context = new UserContextForTest(user, Env.CurrentCompany);

			using (Env.SetTemporaryUserContext(context))
			{
				using (MessagingPOCHelper.TemporarilyEnablePOC(enabled: false))
				{
					user.IsDeveloper = false;
					AssertEquals("Non-developer, no FUNCS", false, MessagingPOCHelper.IsPOCActive);
					user.IsDeveloper = true;
					AssertEquals("IsDeveloper, no FUNCS", true, MessagingPOCHelper.IsPOCActive);
				}

				using (MessagingPOCHelper.TemporarilyEnablePOC(enabled: true))
				{
					user.IsDeveloper = false;
					AssertEquals("Non-developer, with FUNCS", true, MessagingPOCHelper.IsPOCActive);
					user.IsDeveloper = true;
					AssertEquals("IsDeveloper, with FUNCS", true, MessagingPOCHelper.IsPOCActive);
				}
			}
		}
	}
}
