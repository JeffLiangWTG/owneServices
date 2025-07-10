using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFMessageSendingValidationTesing : TestCaseWithFactory
	{
		public void TestSendMessageWithErrorsSecurityCheckpoint()
		{
			var oldAllowed = Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = false;
				iSFHeader.BF_EntryType = "X";
				var messageResults = ISFMessageSendingValidation.New(iSFHeader, null).CheckBusinessObjectLevelValidation();
				AssertEquals(true, messageResults.ContainsError($"{MessageSendingValidation.MessageErrorsExistWithNoSecurityRight} {string.Format(MessageSendingValidation.InformationForGetSecurityRight, Env.Security.ImporterSecurityFilingSendWithMessageErrors.DisplayTextPathToSecurityRight)}"));
			}
			finally
			{
				Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = oldAllowed;
			}

			oldAllowed = Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = true;
				iSFHeader.BF_EntryType = "X";
				var messageResults = ISFMessageSendingValidation.New(iSFHeader, null).CheckBusinessObjectLevelValidation();
				AssertEquals(false, messageResults.ContainsError(MessageSendingValidation.MessageErrorsExistWithNoSecurityRight));
			}
			finally
			{
				Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			iSFHeader = Factory.New<CusISFHeader>();
		}

		CusISFHeader iSFHeader;
	}
}
