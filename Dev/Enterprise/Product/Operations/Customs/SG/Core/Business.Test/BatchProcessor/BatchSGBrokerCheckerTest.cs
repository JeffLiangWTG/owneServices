using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSGBrokerCheckerTest : TestCaseWithFactory
	{
		[TestDate(2007, 1, 1)]
		public void TestCheckStaff()
		{
			GlbStaff broker1 = CreateBroker("broker1", "B1", "Broker 1", "Test@test1.com");
			GlbStaff broker2 = CreateBroker("broker2", "B2", "Broker 2", "");
			GlbStaff broker3 = CreateBroker("broker3", "B3", "Broker 3", "");
			GlbExternalPassword_SGv4 password1 = CreatePassword(broker1.PK, "", "", "");
			GlbExternalPassword_SGv4 password2 = CreatePassword(broker2.PK, "USER2", "", Core.Constants.PasswordOK);
			GlbExternalPassword_SGv4 password3 = CreatePassword(broker3.PK, "USER3", "PWD3", "NOK");
			GlbGroup emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "TPR";
			GlbGroupLink groupLink = Factory.New<GlbGroupLink>();
			groupLink.GK_GG = emailGroup.PK;
			groupLink.GK_GS = broker1.PK;
			logger = new LoggingInformation();
			Factory.Save();
			SGCustomsDataRegistry.Instance.SendErrorsToGroup.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, emailGroup.PK.ToGuid());
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var mock = new Mock<BatchSGBrokerChecker>(new object[] { Logger, Helper });
			BatchSGBrokerChecker brokerChecker = mock.Object;
			AssertEquals("Registry", DateTime.MinValue, SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.Value);
			brokerChecker.CheckStaff();
			AssertEquals("email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("subject", "Errors exist in the following Broker (s) TradeNet registration details", email.Subject);
			AssertContains("body", "Broker: " + broker3.GS_Code + " - " + broker3.GS_FullName + " has invalid TradeNet password details; Password Status = (NOK)", email.Body);
			AssertNotContains("body", broker1.GS_FullName, email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "Test@test1.com", email.Recipients[0].Email);
			AssertEquals("Registry", new ZDateTime(2007, 1, 1), SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.Value.Date);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			brokerChecker.CheckStaff();
			AssertEquals("email created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime());
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			brokerChecker.CheckStaff();
			AssertEquals("email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		#region Implementation
		GlbStaff CreateBroker(ZString loginName, ZString code, ZString fullName, ZString eMail)
		{
			GlbStaff result = Factory.New<GlbStaff>();
			result.GS_LoginName = loginName;
			result.GS_Code = code;
			result.GS_FullName = fullName;
			result.GS_EmailAddress = eMail;
			return result;
		}

		GlbExternalPassword_SGv4 CreatePassword(ZGuid staffPK, ZString mailBoxID, ZString currentPassword, ZString passwordStatus)
		{
			GlbExternalPassword_SGv4 result = Factory.New<GlbExternalPassword_SGv4>();
			result.GP_GS = staffPK;
			result.GP_UserID = mailBoxID;
			result.GP_CurrentPassword = currentPassword;
			result.GP_PasswordStatus = passwordStatus;
			return result;
		}

		LoggingInformation Logger
		{
			get
			{
				return logger ?? (logger = new LoggingInformation());
			}
		}

		LoggingInformation logger;

		BatchSGInterchangeHelper Helper
		{
			get => helper ?? (helper = new BatchSGInterchangeHelperForTesting(Logger));
		}

		BatchSGInterchangeHelper helper;
		#endregion
	}
}
