using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanySignatureCredential))]
	sealed class GlbCompanySignatureCredentialTest : GlbExternalPasswordTest<GlbCompanySignatureCredential>
	{
		public void TestSetDefaultValues()
		{
			var credential = CreateNewGlbExternalPassword(Factory);
			AssertEquals(GlbCompany.CurrentCompany.PK, credential.GP_GC);
			AssertEquals(ZGuid.Empty, credential.GP_GS);
			AssertEquals(PasswordTypesList.Codes.TRU, credential.GP_PasswordType);
			AssertEquals(ZString.Empty, credential.GP_CurrentPassword);
			AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(ZString.Empty, credential.CurrentDecryptedPassword);
			AssertEquals(ZString.Empty, credential.GP_UserID);
			AssertEquals(20, GlbCompanySignatureCredential.Schema.CurrentDecryptedPasswordMaxLengthTR);
		}

		public void TestPasswordStatus()
		{
			AssertEquals(PasswordStatusList.Codes.Invalid, GlbExternalPassword.GP_PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = ZString.Empty;
			AssertEquals(ZString.Empty, GlbExternalPassword.GP_PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			AssertEquals(PasswordStatusList.Codes.Valid, GlbExternalPassword.GP_PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			AssertEquals(PasswordStatusList.Codes.Invalid, GlbExternalPassword.GP_PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			AssertEquals(Core.Constants.PasswordOK, GlbExternalPassword.GP_PasswordStatus);
		}

		public void TestPassword()
		{
			AssertEquals(ZString.Empty, GlbExternalPassword.GP_CurrentPassword);

			GlbExternalPassword.GP_CurrentPassword = "222";
			AssertEquals("222", GlbExternalPassword.GP_CurrentPassword);

			GlbExternalPassword.GP_CurrentPassword = string.Empty;
			AssertEquals(string.Empty, GlbExternalPassword.GP_CurrentPassword);
		}

		public void TestDecryptedPassword()
		{
			AssertEquals(ZString.Empty, GlbExternalPassword.CurrentDecryptedPassword);

			GlbExternalPassword.CurrentDecryptedPassword = "111";
			AssertEquals("111", GlbExternalPassword.CurrentDecryptedPassword);
			AssertEquals(Core.Constants.PasswordOK, GlbExternalPassword.GP_PasswordStatus);
			AssertNoErrors(GlbExternalPassword.CurrentDecryptedPasswordInfo);

			GlbExternalPassword.CurrentDecryptedPassword = string.Empty;
			AssertEquals(string.Empty, GlbExternalPassword.GP_CurrentPassword);
			AssertEquals(string.Empty, GlbExternalPassword.GP_PasswordStatus);
			AssertHasErrors(GlbExternalPassword.CurrentDecryptedPasswordInfo);
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestLocationString_MaxLength()
		{
			try
			{
				GlbExternalPassword.CurrentDecryptedPassword = "111111111111111111111";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestUserID()
		{
			AssertEquals(ZString.Empty, GlbExternalPassword.GP_UserID);

			GlbExternalPassword.GP_UserID = "XXX";
			AssertEquals("XXX", GlbExternalPassword.GP_UserID);
			AssertNoErrors(GlbExternalPassword.GP_UserIDInfo);

			GlbExternalPassword.GP_UserID = string.Empty;
			AssertEquals(string.Empty, GlbExternalPassword.GP_UserID);
			AssertHasErrors(GlbExternalPassword.GP_UserIDInfo);
		}

		public void TestValidation()
		{
			Assert(GlbExternalPassword.Validation is GlbCompanySignatureCredentialValidation);
		}

		public void TestSignatureCredentialsChangeLogging()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Core.Constants.CountryCodes.Turkey);
				AssertEquals(false, company1.SignatureCredentials.IsEditAllowed);
				AssertEquals(false, company1.SignatureCredentials.IsLoaded);
				AssertEquals(false, company1.SignatureCredentials.AllowNew);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.SignatureCredentials));
				var signatureCredential = company1.SignatureCredentials.AddNew();

				var logs = company1.Logs.GetAllLogs().Cast<StmALog>();
				AssertEquals(0, logs.Count(log => log.SL_Reference.Contains("Signature Credential edited,")));
				AssertEquals(0, logs.Count(log => log.SL_Reference.Contains("Signature Credential added,")));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
				AssertEquals(true, company2.SignatureCredentials.IsEditAllowed);
				AssertEquals(true, company2.SignatureCredentials.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.SignatureCredentials));

				var signatureCredentialAdded = company2.SignatureCredentials.AddNew();
				AssertEquals(nameof(signatureCredentialAdded.GP_GC), company2.PK, signatureCredentialAdded.GP_GC);
				signatureCredentialAdded.GP_UserID = "123";
				signatureCredentialAdded.GP_CurrentPassword = "123";
				AssertEquals(false, company2.SignatureCredentials.AllowNew);
				Factory.Save();

				var logs = company2.Logs.GetAllLogs().Cast<StmALog>();
				AssertEquals(1, logs.Count(log => log.SL_Reference.Contains("Signature Credential " + signatureCredentialAdded.GP_UserID + " is added,")));

				company2.SignatureCredentials[0].GP_UserID = "1234";
				company2.SignatureCredentials[0].GP_CurrentPassword = "1234";
				var oldValue = company2.SignatureCredentials[0].GP_UserIDInfo.OriginalValue;
				Factory.Save();

				logs = company2.Logs.GetAllLogs().Cast<StmALog>();
				AssertEquals(1, logs.Count(log => log.SL_Reference.Contains("Signature Credential is edited, " + oldValue + " -> " + company2.SignatureCredentials[0].GP_UserID)));
				AssertEquals(2, logs.Count(log => log.SL_Reference.Contains("Signature Credential")));

				var signatureCredentialDeleted = company2.SignatureCredentials[0];
				var deletedValue = signatureCredentialDeleted.GP_UserID;
				signatureCredentialDeleted.Delete();
				Factory.Save();

				logs = company2.Logs.GetAllLogs().Cast<StmALog>();
				AssertEquals(1, logs.Count(log => log.SL_Reference.Contains("Signature Credential " + deletedValue + " is deleted,")));
				AssertEquals(3, logs.Count(log => log.SL_Reference.Contains("Signature Credential")));
			}
		}

		protected override GlbCompanySignatureCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.New<GlbCompanySignatureCredential>();
		}
	}
}
