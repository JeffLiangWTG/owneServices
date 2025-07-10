using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSG4InterchangeHelperTest : TestCaseWithFactory
	{
		[TestDate(2017, 07, 20)]
		public void TestTestConnectionCommand()
		{
			var helper = new BatchSG4InterchangeHelper(null);
			SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNotNull(helper.TestConnectionCommand);
			AssertEquals("The command string for production is correct", "https://www.tradexchange.gov.sg/txmhbweb/mhb/TestConnectionServlet", helper.TestConnectionCommand.CommandStringForTesting);
			helper = new BatchSG4InterchangeHelper(null);
			SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("The command string for test is correct", "https://trial.tradexchange.gov.sg/txmhbweb/mhb/TestConnectionServlet", helper.TestConnectionCommand.CommandStringForTesting);
		}

		public void TestShowVerboseLogging()
		{
			var helper = new BatchSG4InterchangeHelper(null);
			AssertEquals("ShowVerboseLogging", false, helper.ShowVerboseLogging);
			SGCustomsDataRegistry.Instance.VerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ShowVerboseLogging registry turned on", true, helper.ShowVerboseLogging);
		}

		public void TestVerboseLog()
		{
			var helper = new BatchSG4InterchangeHelper(null);
			var loggingInformation = new LoggingInformation();
			helper.VerboseLog(loggingInformation, "Test log message");
			AssertEquals("Logging information output", 0, loggingInformation.UserLogStrings.Count);
			SGCustomsDataRegistry.Instance.VerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			helper.VerboseLog(loggingInformation, "Test log message");
			AssertEquals("Logging information output", 1, loggingInformation.UserLogStrings.Count);
			AssertEquals("Logging information output", true, loggingInformation.UserLogStrings[0].Contains("Test log message"));
		}

		public void TestPasswordType()
		{
			var helper = new BatchSG4InterchangeHelper(new LoggingInformation());
			AssertEquals("SG4", helper.PasswordType);
		}

		public void TestApplicationDescription()
		{
			var helper = new BatchSG4InterchangeHelper(new LoggingInformation());
			AssertEquals("TradeNet", helper.ApplicationDescription);
		}

		public void TestPassword()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_LoginName = "lkwepoid";
			broker.GS_Code = "WER";
			broker.GS_IsActive = true;
			var wrapper = SGGlbStaffWrapper.Get(broker);
			wrapper.Tradenetv4Password.GP_UserID = "BLAH";
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
			wrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
			var helper = new BatchSG4InterchangeHelper(new LoggingInformation());
			AssertEquals(wrapper.Tradenetv4Password.PK, helper.GetGlbExternalPassword(wrapper).PK);
		}
	}
}
