using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.Testing
{
	public class CurrentUsersPinTest : TestCaseWithFactory
	{
		public void TestLiveModeFunctionality()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var usersPin = new CurrentUsersPin(Factory, false);
			var wrapper = staff.GetNZWrapper();
			var savePin = wrapper.NZBPassword.GP_CurrentPassword;
			wrapper.NZBPassword.GP_UserID = "THISISNOTADRILL";
			usersPin.DecryptedPinCode = "NeverUseThisPIN";
			Assert("PIN should have changed.", savePin != wrapper.NZBPassword.GP_CurrentPassword);
			AssertEquals("UsersPin.BrokerID", "THISISNOTADRILL", usersPin.BrokerID);
			AssertEquals("UsersPin.DecryptedPinCode", "NeverUseThisPIN".ToUpper(), usersPin.DecryptedPinCode);
		}

		public void TestTestModeFunctionality()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var usersPin = new CurrentUsersPin(Factory, true);
			var wrapper = staff.GetNZWrapper();
			var savePin = wrapper.NZBPassword.GP_CurrentPassword;
			wrapper.NZBPassword.GP_UserID = "THISISNOTADRILL";
			usersPin.DecryptedPinCode = "NeverUseThisPIN";
			Assert("PIN should have changed.", savePin != wrapper.NZBPassword.GP_CurrentPassword);
			AssertEquals("UsersPin.BrokerID", wrapper.NZBPassword.GP_UserID, usersPin.BrokerID);
			AssertEquals("UsersPin.DecryptedPinCode", usersPin.DecryptedPinCode, usersPin.DecryptedPinCode);

			wrapper.NZBPassword.GP_UserID = "";
			usersPin.DecryptedPinCode = "";
			AssertEquals("UsersPin.BrokerID - in this case now should use test value", CurrentUsersPin.TestSystemBrokerID, usersPin.BrokerID);
			AssertEquals("UsersPin.DecryptedPinCode - in this case now should use test value", CurrentUsersPin.TestSystemPinCode, usersPin.DecryptedPinCode);
		}
	}
}
