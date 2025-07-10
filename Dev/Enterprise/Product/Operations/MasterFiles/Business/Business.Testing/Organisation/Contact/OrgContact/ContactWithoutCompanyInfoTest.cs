using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContactWithoutCompanyInfo))]
	sealed class ContactWithoutCompanyInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyBizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			return new ContactWithoutCompanyInfo("", "", "", "", "", "", "", dummyBizO, false);
		}

		public void TestGeneratePasswordUrl()
		{
			var dummyBizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			var contactWithoutCompanyInfo = new ContactWithoutCompanyInfo(Factory, "testContact", "test@wisetechglobal.com", "http://webtracker/resetpassword.aspx?ResetKey=",
				"My Salutation", "My ExtraInstruction", "My OrgCode", "My OrgCode, My OrgCode2", dummyBizO, false);
			var url = contactWithoutCompanyInfo.GeneratePasswordInstructionUrl("token", PasswordInstructionType.Reset);
			AssertEquals("http://webtracker/resetpassword.aspx?ResetKey=token", url);
			AssertEquals("http://webtracker/resetpassword.aspx?ResetKey=", contactWithoutCompanyInfo.Url);
			AssertEquals("testContact", contactWithoutCompanyInfo.Name);
			AssertEquals("test@wisetechglobal.com", contactWithoutCompanyInfo.Email);
			AssertEquals("My Salutation", contactWithoutCompanyInfo.Salutation);
			AssertEquals("My ExtraInstruction", contactWithoutCompanyInfo.ExtraInstruction);
			AssertEquals("My OrgCode", contactWithoutCompanyInfo.OrgCode);
			AssertEquals("My OrgCode, My OrgCode2", contactWithoutCompanyInfo.OrgCodes);
			AssertEquals(dummyBizO.PK, contactWithoutCompanyInfo.SenderForLogs.PK);

			Env.Registry.MailboxDisplayName = "Wise Tech";
			EnvProxy.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress = "noreply@wisetechglobal.com";
			AssertEquals("Wise Tech", contactWithoutCompanyInfo.FromDisplayName);
			AssertEquals("EN", contactWithoutCompanyInfo.Language);
			AssertEquals("noreply@wisetechglobal.com", contactWithoutCompanyInfo.FromAddress);

			AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
			{
				contactWithoutCompanyInfo.GeneratePasswordInstructionUrl("token", PasswordInstructionType.Set);
			});
		}
	}
}
