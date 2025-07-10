using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword))]
	sealed class GlbExternalPasswordBaseOnlyTest : GlbExternalPasswordTest<GlbExternalPassword>
	{
		public void TestCorrectIntefaceSetup()
		{
			var externalPassword = Factory.New<Integration.IGlbExternalPassword>();
			AssertEquals(typeof(GlbExternalPassword), externalPassword.GetType());
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
		}

		public void TestDisableConfigurationToSender()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "#@4";
			staff.GS_FullName = "STAFF 3";
			staff.GS_LoginName = "#@4";
			staff.GS_EmailAddress = "staff3@where.com";
			Factory.Save();

			var staffPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			staffPassword.DisableConfigurationToSender = true;
			staffPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			staffPassword.GP_GG = ZGuid.Empty;
			staffPassword.GP_GS = staff.PK;
			staffPassword.GP_UserID = "ABC123";
			staffPassword.GP_PasswordType = "NTP";
			staffPassword.CurrentDecryptedPassword = "KDS323";
			Factory.Save();
			AssertNull(Factory.GetLatestEHubConfigurationInterchange());
			staffPassword.Delete();
			Factory.Save();
			AssertNull(Factory.GetLatestEHubConfigurationInterchange());

			staffPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			staffPassword.DisableConfigurationToSender = false;
			staffPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			staffPassword.GP_GG = ZGuid.Empty;
			staffPassword.GP_GS = staff.PK;
			staffPassword.GP_UserID = "ABC123";
			staffPassword.GP_PasswordType = "NTP";
			staffPassword.CurrentDecryptedPassword = "KDS323";
			Factory.Save();
			AssertNotNull(Factory.GetLatestEHubConfigurationInterchange());
		}

		class GlbExternalPasswordForConfigurationTest : GlbExternalPassword
		{
			public GlbExternalPasswordForConfigurationTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString ConfigurationName => "Configuration";
		}

		public void TestGP_PasswordStatusDescription()
		{
			var externalPasswordMock = Factory.NewMoq<GlbExternalPassword>();
			var externalPassword = externalPasswordMock.Object;
			var lookupsMock = new Mock<GlbExternalPasswordLookups>(externalPassword);
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "2DEF DESC");
			list.AddPair("DEF", "ABC1 DESC");
			lookupsMock.Setup(m => m.PasswordStatusList).Returns(list);
			externalPasswordMock.Protected().Setup<GlbExternalPasswordLookups>("GetNewLookups").Returns(lookupsMock.Object);
			AssertEquals(ZString.Empty, externalPassword.GP_PasswordStatusDescription);
			externalPassword.GP_PasswordStatus = "ABC";
			AssertEquals("2DEF DESC (ABC)", externalPassword.GP_PasswordStatusDescription);
			externalPassword.GP_PasswordStatus = "KDS";
			AssertEquals(" (KDS)", externalPassword.GP_PasswordStatusDescription);
			externalPassword.GP_PasswordStatus = "DEF";
			AssertEquals("ABC1 DESC (DEF)", externalPassword.GP_PasswordStatusDescription);

			lookupsMock.VerifyAll();
			externalPasswordMock.VerifyAll();
		}

		public void TestGP_PasswordTypeDescription()
		{
			var externalPasswordMock = Factory.NewMoq<GlbExternalPassword>();
			var externalPassword = externalPasswordMock.Object;
			var lookupsMock = new Mock<GlbExternalPasswordLookups>(externalPassword);
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "2DEF DESC");
			list.AddPair("DEF", "ABC1 DESC");
			lookupsMock.Setup(m => m.PasswordTypeList).Returns(list);
			externalPasswordMock.Protected().Setup<GlbExternalPasswordLookups>("GetNewLookups").Returns(lookupsMock.Object);
			AssertEquals(ZString.Empty, externalPassword.GP_PasswordTypeDescription);
			externalPassword.GP_PasswordType = "ABC";
			AssertEquals("2DEF DESC", externalPassword.GP_PasswordTypeDescription);
			externalPassword.GP_PasswordType = "KDS";
			AssertEquals("KDS", externalPassword.GP_PasswordTypeDescription);
			externalPassword.GP_PasswordType = "DEF";
			AssertEquals("ABC1 DESC", externalPassword.GP_PasswordTypeDescription);

			lookupsMock.VerifyAll();
			externalPasswordMock.VerifyAll();
		}

		public void TestGP_CurrentPassword()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.GP_CurrentPassword = "TEST";
			AssertEquals(Core.Constants.PasswordOK, GlbExternalPassword.GP_PasswordStatus);
		}

		public void TestGP_NextPassword()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.GP_NextPassword = "TEST";
			AssertEquals(Core.Constants.PasswordOK, GlbExternalPassword.GP_PasswordStatus);
		}

		public void TestCurrentDecryptedPassword()
		{
			GlbExternalPassword.CurrentDecryptedPassword = "CurrentDecryptedPassword";
			AssertEquals("CurrentDecryptedPassword", GlbExternalPassword.CurrentDecryptedPassword);
			var encoder = new TwoWayEncoder(Staff.PK.ToGuid());
			AssertEquals(encoder.Encrypt("CurrentDecryptedPassword"), GlbExternalPassword.GP_CurrentPassword);
			GlbExternalPassword.GP_CurrentPassword = encoder.Encrypt("1HeLLo2");
			AssertEquals("1HeLLo2", GlbExternalPassword.CurrentDecryptedPassword);
		}

		public void TestNextDecryptedPassword()
		{
			GlbExternalPassword.NextDecryptedPassword = "NewNext";
			AssertEquals("NewNext", GlbExternalPassword.NextDecryptedPassword);
			var encoder = new TwoWayEncoder(Staff.PK.ToGuid());
			AssertEquals(encoder.Encrypt("NewNext"), GlbExternalPassword.GP_NextPassword);
			GlbExternalPassword.GP_NextPassword = encoder.Encrypt("1HeLLo2");
			AssertEquals("1HeLLo2", GlbExternalPassword.NextDecryptedPassword);
		}

		public void TestCurrentDecryptedCertificatePassphrase()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "Phrase";
			AssertEquals("Phrase", GlbExternalPassword.CurrentDecryptedCertificatePassphrase);
			var encoder = new TwoWayEncoder(Staff.PK.ToGuid());
			AssertEquals(encoder.Encrypt("Phrase"), GlbExternalPassword.GP_CertificatePassPhrase);
			GlbExternalPassword.GP_CertificatePassPhrase = encoder.Encrypt("1HeLLo2");
			AssertEquals("1HeLLo2", GlbExternalPassword.CurrentDecryptedCertificatePassphrase);
		}

		public void TestWrappedDecryptedPropertyMaxLength()
		{
			AssertEquals("The current maxium length of CurrentDecryptedCertificatePassphrase is 47 which when encrypted will result in 128 characters; if GP_CertificatePassPhrase maximum is changed then we should change the maximum length of CurrentDecryptedCertificatePassphrase to reflect it", 128, GlbExternalPasswordSchema.GP_CertificatePassPhrase.MaxLength);
			AssertEquals("The current maxium length of CurrentDecryptedPassword is 47 which when encrypted will result in 128 characters; if GP_CurrentPassword maximum is changed then we should change the maximum length of CurrentDecryptedPassword to reflect it", 128, GlbExternalPasswordSchema.GP_CurrentPassword.MaxLength);
			AssertEquals("The current maxium length of NextDecryptedPassword is 47 which when encrypted will result in 128 characters; if GP_NextPassword maximum is changed then we should change the maximum length of NextDecryptedPassword to reflect it", 128, GlbExternalPasswordSchema.GP_NextPassword.MaxLength);
		}

		public void TestWithDeactivatedStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "TEST USER";
			staff.GS_LoginName = "testuser";
			staff.GS_EmailAddress = "test@test.test";
			Factory.Save();

			staff.GS_IsActive = false;

			var password = Factory.New<GlbExternalPassword>();
			password.GP_GS = staff.PK;

			staff.RunPreSaveValidation();
			AssertNoErrors(password.GP_GSInfo);
		}

		public void TestOriginalStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "TEST USER";
			staff.GS_LoginName = "testuser";
			staff.GS_EmailAddress = "test@test.test";
			Factory.Save();

			staff.GS_IsActive = false;
			var password = Factory.NewWithValidTestData<GlbExternalPassword>();
			password.GP_GS = staff.PK;
			password.GP_UserID = "A1";

			var password1 = Factory.NewWithValidTestData<GlbExternalPassword>();
			password1.GP_GS = staff.PK;
			password1.GP_UserID = "A1";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			password = newFactory.Load<GlbExternalPassword>(password.PK);
			password.GP_GS = ZGuid.Empty;
			AssertNull(password.Staff);
			AssertEquals(staff.PK, password.OriginalStaff.PK);

			password1 = newFactory.Load<GlbExternalPassword>(password.PK);
			password1.GP_GS = ZGuid.Empty;
			password1.Delete();
			AssertEquals(staff.PK, password1.OriginalStaff.PK);

			staff.Delete();
			AssertEquals(staff.PK, password.OriginalStaff.PK);
			AssertEquals(staff.PK, password1.OriginalStaff.PK);

			staff = newFactory.Load<GlbStaff>(staff.PK);
			AssertEquals(staff.PK, password.OriginalStaff.PK);
			AssertEquals(staff.PK, password1.OriginalStaff.PK);

			staff.Delete();
			AssertNull(password.OriginalStaff);
			AssertNull(password1.OriginalStaff);
			Factory.Save();

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "77";
			staff.GS_FullName = "TEST USER";
			staff.GS_LoginName = "testuser";
			staff.GS_EmailAddress = "test@test.test";
			Factory.Save();

			staff.GS_IsActive = false;
			password = Factory.NewWithValidTestData<GlbExternalPassword>();
			password.GP_GS = staff.PK;
			password.GP_UserID = "A3";
			Factory.Save();

			password = newFactory.Load<GlbExternalPassword>(password.PK);
			password.GP_GS = ZGuid.Empty;
			staff.Delete();
			AssertEquals(staff.PK, password.OriginalStaff.PK);

			Factory.Save();
			AssertNull(password.OriginalStaff);
		}

		public void TestSendConfigurationWhenDelete()
		{
			var staff = Factory.New<GlbStaff>();
			var newFactory = new BusinessObjectFactory();
			staff.GS_Code = "77";
			staff.GS_FullName = "TEST USER";
			staff.GS_LoginName = "testuser";
			staff.GS_EmailAddress = "test@test.test";
			Factory.Save();

			staff.GS_IsActive = false;
			var password = Factory.NewWithValidTestData<GlbExternalPasswordForATest>();
			password.GP_GS = staff.PK;
			password.GP_UserID = "A3";
			password.GP_PasswordStatus = "VAL";
			password.GP_PasswordType = "C";
			Factory.Save();

			password = newFactory.Load<GlbExternalPasswordForATest>(password.PK);
			AssertEquals(staff.PK, password.Staff.PK);

			password.GP_GS = ZGuid.Empty;
			AssertNull(password.Staff);
			AssertEquals(staff.PK, password.OriginalStaff.PK);

			password.Delete();
			AssertEquals(staff.PK, password.OriginalStaff.PK);
			staff.Delete();

			AssertEquals(staff.PK, password.OriginalStaff.PK);

			newFactory.Save();
			var interchange = newFactory.GetLatestEHubConfigurationInterchange();
			AssertConfiguration(interchange, "TEST1", "77", 0);
		}

		static void AssertConfiguration(Messaging.Integration.IEDIInterchange interchange, ZString configurationName, ZString staffCode, int passwordLength = 0)
		{
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var configuration = reader.DeserializeToConfiguration();
				AssertEquals("Configuration name should be set", configurationName, configuration.Name);

				var systemGroup = configuration.Group[0];
				AssertEquals("Group type should be set", "System", systemGroup.Type);
				AssertEquals("There should be one subgroup", 1, systemGroup.Items.Length);

				var group = (Group)systemGroup.Items[0];
				AssertEquals("Group type should be set", "Company", group.Type);

				group = (Group)group.Items[0];
				AssertEquals("Group type should be set", "Staff", group.Type);
				AssertEquals("Staff", staffCode, group.Reference);

				AssertEquals("There should be password", passwordLength, group.Items?.Length ?? 0);
			}
		}

		public void TestOnlySendCertificateCredentialIfNeeded()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "77";
			staff.GS_FullName = "TEST USER";
			staff.GS_LoginName = "testuser";
			staff.GS_EmailAddress = "test@test.test";
			Factory.Save();

			staff.GS_IsActive = false;
			var credential = Factory.NewWithValidTestData<GlbExternalPasswordForCertificateTest>();
			credential.GP_GS = staff.PK;
			credential.GP_UserID = "A3";
			credential.GP_PasswordStatus = "VAL";
			credential.GP_PasswordType = "C";
			Factory.Save();
			AssertNull("No credential Send and a new interchange created during update as no change on GP_Certificate", Factory.GetLatestDxTConfigurationInterchange());

			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			Factory.Save();
			var interchange1 = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull("Credential Send and a new interchange created during update as GP_Certificate changed", interchange1);

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "Change GP_PasswordStatus only" };
			var credentialInNewFactory = newFactory.Load<GlbExternalPasswordForCertificateTest>(credential.PK);
			credentialInNewFactory.GP_PasswordStatus = "INV";
			newFactory.Save();
			AssertEquals("No credential Send and no new interchange created during update other property as no change on GP_Certificate", interchange1.PK, newFactory.GetLatestDxTConfigurationInterchange().PK);

			var newFactory2 = new BusinessObjectFactory() { NameForDebugging = "GP_Certificate no change" };
			var credentialInNewFactory2 = newFactory2.Load<GlbExternalPasswordForCertificateTest>(credential.PK);
			credentialInNewFactory2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credentialInNewFactory2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			newFactory2.Save();
			AssertEquals("No credential Send and no new interchange created during update as no change on GP_Certificate", interchange1.PK, newFactory2.GetLatestDxTConfigurationInterchange().PK);

			credential.Delete();
			Factory.Save();
			var interchange2 = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotEquals("Should send credential and a new interchange created on delete as GP_Certificate changed", interchange1.PK, interchange2.PK);

			var credential2 = Factory.NewWithValidTestData<GlbExternalPasswordForCertificateTest>();
			credential.GP_GS = staff.PK;
			credential.GP_PasswordStatus = "VAL";
			credential.GP_PasswordType = "C";
			Factory.Save();
			AssertEquals("No credential Send and no new interchange created during update as no change on GP_Certificate", interchange2.PK, Factory.GetLatestDxTConfigurationInterchange().PK);
			credential2.Delete();
			Factory.Save();
			AssertEquals("No credential Send and no new interchange created during delete as no change on GP_Certificate", interchange2.PK, Factory.GetLatestDxTConfigurationInterchange().PK);
		}

		class GlbExternalPasswordForATest : GlbExternalPassword
		{
			public GlbExternalPasswordForATest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString ConfigurationName => "TEST1";
		}

		class GlbExternalPasswordForCertificateTest : GlbExternalPassword
		{
			public GlbExternalPasswordForCertificateTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString ConfigurationName => "CertificateConfigurationTEST";

			public override CredentialRecipient CredentialRecipient => CredentialRecipient.DirectxT;

			protected override ZPropertyInfo[] CredentialApplicableInfos()
			{
				return new ZPropertyInfo[] { GP_CertificateInfo };
			}

			protected override bool ShouldSendDeleteCredential()
			{
				return !GP_CertificateInfo.OriginalValue.IsEmpty;
			}
		}
	}
}
