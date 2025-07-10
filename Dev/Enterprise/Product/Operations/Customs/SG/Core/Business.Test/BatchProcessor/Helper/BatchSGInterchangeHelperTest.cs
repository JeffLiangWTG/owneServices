using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	public class BatchSGInterchangeHelperTest : TestCaseWithFactory
	{
		public void TestMailboxChecker()
		{
			BatchSGInterchangeHelper helper = new BatchSGInterchangeHelperForTesting(null);
			AssertNotNull(helper.MailboxChecker);
		}

		public void TestShowVerboseLogging()
		{
			var helper = new BatchSG4InterchangeHelper(null);
			AssertEquals("ShowVerboseLogging", false, helper.ShowVerboseLogging);
		}

		public void TestVerboseLog()
		{
			var helper = new BatchSGInterchangeHelperForTesting(null);
			var loggingInformation = new LoggingInformation();
			helper.VerboseLog(loggingInformation, "Test log message");
			AssertEquals("Logging information output", 1, loggingInformation.UserLogStrings.Count);
			helper.VerboseLog(loggingInformation, "Test log message");
			AssertEquals("Logging information output", 2, loggingInformation.UserLogStrings.Count);
			AssertEquals("Logging information output", true, loggingInformation.UserLogStrings[1].Contains("Test log message"));
		}

		public void TestGetValidBrokerMailboxes()
		{
			var helper = new BatchSGInterchangeHelperForTesting(new LoggingInformation());
			var brokerStaff = helper.GetValidBrokerMailboxes();
			AssertEquals("Broker Staff is emtpy", 0, brokerStaff.Count);
			CreateBroker("lkwepoid", "WER", false, "Stuff", "Current", Core.Constants.PasswordOK);
			CreateBroker("fmndfsfg", "LKF", true, "Stuff", "Current", "JK");
			CreateBroker("slkjdflksd", "HDF", true, "", "Current", Core.Constants.PasswordOK);
			CreateBroker("lkhjdskfhn", "MND", true, "BLAH", "", Core.Constants.PasswordOK);
			var broker = CreateBroker("skhfakjfiopu", "JDC", true, "BLAH", "Current", Core.Constants.PasswordOK);
			var wrapper = SGGlbStaffWrapper.Get(broker);
			Factory.Save();
			brokerStaff = helper.GetValidBrokerMailboxes();
			AssertEquals("Only one broker is returned", 1, brokerStaff.Count);
			var result = brokerStaff[0];
			var wrapper2 = SGGlbStaffWrapper.Get(result);
			AssertEquals("Correct Login Name", broker.GS_LoginName, result.GS_LoginName);
			AssertEquals("Correct Login Code", broker.GS_Code, result.GS_Code);
			AssertEquals("Correct Is Active", broker.GS_IsActive, result.GS_IsActive);
			AssertEquals("Correct Personal User ID", wrapper.Tradenetv4Password.GP_UserID, wrapper2.Tradenetv4Password.GP_UserID);
			AssertEquals("Correct Broker Working Password", wrapper.Tradenetv4Password.CurrentDecryptedPassword, wrapper2.Tradenetv4Password.CurrentDecryptedPassword);
			AssertEquals("Correct Broker Password Status", wrapper.Tradenetv4Password.GP_PasswordStatus, wrapper2.Tradenetv4Password.GP_PasswordStatus);
		}

		GlbStaff CreateBroker(string loginName, string code, bool isActive, string personalUserID, string brokerWorkingPassword, string brokerPasswordStatus)
		{
			GlbStaff result = Factory.New<GlbStaff>();
			result.GS_LoginName = loginName;
			result.GS_Code = code;
			result.GS_IsActive = isActive;
			var wrapper = SGGlbStaffWrapper.Get(result);
			wrapper.Tradenetv4Password.GP_UserID = personalUserID;
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = brokerWorkingPassword;
			wrapper.Tradenetv4Password.GP_PasswordStatus = brokerPasswordStatus;
			return result;
		}
	}
}
