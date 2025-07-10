using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using Regex = System.Text.RegularExpressions.Regex;
using RegexOptions = System.Text.RegularExpressions.RegexOptions;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPasswordBase_TW))]
	public abstract class GlbExternalPasswordBase_TWTest<T> : GlbExternalPasswordWithCertificateTest<T> where T : GlbExternalPasswordBase_TW
	{
		protected virtual string ValidMailBox => "CBK0123-A";
		protected virtual string ValidPasswordType => PasswordTypesList.Codes.TVA;
		protected virtual ZString InterchangeType => EDIInterchangeTypeList.Codes.ForwarderConfiguration;

		[ExpectNoExceptions]
		public void TestClone()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "TC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_IsActive = true;
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			var password1 = Factory.NewWithValidTestData<T>();
			password1.GP_GC = company1.PK;
			password1.GP_GS = staff1.PK;
			password1.GP_UserID = "1";
			password1.GP_MailBoxID = ValidMailBox;

			var certificateData = new byte[] { 120, 200 };
			password1.GP_Certificate = certificateData;
			password1.CurrentDecryptedCertificatePassphrase = "ABC123";

			NUnit.Framework.Assert.That(password1.SupportsClone(), NUnit.Framework.Is.True, "SupportsClone");
			NUnit.Framework.Assert.That(password1 is ITemplateCopyable, NUnit.Framework.Is.True, "Is ITemplateCopyable");
			var copy = (password1 as ITemplateCopyable).TemplateCopy();
			NUnit.Framework.Assert.That(copy, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.EntityFramework.IBusiness)), "TemplateCopy - should not be [null]");
			var passwordCopy = copy as GlbExternalPasswordBase_TW;
			NUnit.Framework.Assert.That(passwordCopy, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.GlbExternalPasswordBase_TW)), "as GlbExternalPasswordBase_TW - should not be [null]");

			CombineAssertions("Clone", () =>
			{
				NUnit.Framework.Assert.That(passwordCopy.GP_GC, NUnit.Framework.Is.EqualTo(company1.PK), "company");
				NUnit.Framework.Assert.That(passwordCopy.GP_GS, NUnit.Framework.Is.EqualTo(staff1.PK), "staff");
				NUnit.Framework.Assert.That(passwordCopy.CurrentDecryptedCertificatePassphrase, NUnit.Framework.Is.EqualTo("ABC123").Using(CustomComparers.TypeComparison), "Certificate Passphrase");
				NUnit.Framework.Assert.That(passwordCopy.GP_Certificate.Equals(new byte[] { 120, 200 }), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Certificate Data");
				NUnit.Framework.Assert.That(passwordCopy.PK, NUnit.Framework.Is.Not.EqualTo(password1.PK), "Certificate Data");
				NUnit.Framework.Assert.That(passwordCopy.GP_UserID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "UserID");
				NUnit.Framework.Assert.That(passwordCopy.GP_MailBoxID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "MailBoxID");
				NUnit.Framework.Assert.That(passwordCopy.GP_CurrentPassword, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "MailBox Password");
			});
		}

		[ExpectNoExceptions]
		public void TestPasswordDataCorrectInCredentialMessage()
		{
			string DecodePassword(string password)
			{
				using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
				{
					var keyXml = "<RSAKeyValue><Modulus>1XmSsYj8fup2QDmo2Yp0nKVzNsonnECcN2OcZXjF+eIEgQyLydnwngBcdW7UOwqQ0E+ffFcdoW44IaTVZ/A7yNC/pRJQy42BBcXteSo7SqgHt7yvPUph+oFpEWLIe0WnPJDmZCRFNoZnMUoOzK89oA/v0I3neveYHjM0bRgtVBE=</Modulus><Exponent>AQAB</Exponent><P>5sXEF2kBcvgWJ1FSo1rahov1KiqogxqBOJio3FmsLsjafwV9O1xKGmjuL1gdQbgd7rPI7x8b42PTfGctlAIPdQ==</P><Q>7M+6YC9HcPeNvr6/S+jX7tP+OV6Xljqe/U9/j7fisEG/9cfFz9lh+H+oItB1MpOD/dKB3RNW5/ODA5TSsHsarQ==</Q><DP>PQtja63jLD5j3dKtQXjvBVhQae8O1F9Wf1oikOdHnLiU07ToA6POFl5bYzqzwoappFL6fAaGogfuEaJZdCV3YQ==</DP><DQ>5iFYtXA8tQNdtCgaLuKwNV++hnHuTgfZycEf7cJ9gVvj+C2ThlFya9NiybJasjO46UlQ+k54/iAfCbPuq6J2YQ==</DQ><InverseQ>k3hB5HGpUKFMYBO+Dh7dkJJ6z7TGKevKWHw3Rhv6aYFOqVvDBKQyW52pbh4aHv5eX15QrBJNPnXYmovSYj47oQ==</InverseQ><D>A/CjvA+bcMCP5f+6cFNtc2OxWe+cELaiO3p6QpGFU+dE7oMmWazM/lmNhfmsRJpdZwmFLR9oKQMsBDZIMwwnH5/WD+gso3VmPRsg5BIdoOs/F0ZkR1qeUlwbjmaD+fPV4b8Tg/3JVeSWT7K8xtKGMLWDcdLMA3vaRo/1/g+KfD0=</D></RSAKeyValue>";
					var text = Convert.FromBase64String(password);
					rsa.FromXmlString(keyXml);
					return new string(System.Text.Encoding.UTF8.GetChars(rsa.Decrypt(text, true)));
				}
			}
			var credential = CreateNewGlbExternalPassword(Factory);
			credential.GP_PasswordType = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "No credential Send - should be [null]");
			credential.Delete();
			Factory.Save();
			NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "No credential Send - should be [null]");

			credential = CreateNewGlbExternalPassword(Factory);
			credential.GP_PasswordType = PasswordTypesList.Codes.TVA;
			credential.GP_MailBoxID = "MBID";
			credential.GP_UserID = "USERID";
			credential.GP_Certificate = new ZBlob(System.Text.Encoding.UTF8.GetBytes("CERT"));
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.CurrentDecryptedCertificatePassphrase = "PASSPHRASE";
			Factory.Save();
			var interchange1 = Factory.GetLatestEHubConfigurationInterchange(InterchangeType);
			NUnit.Framework.Assert.That(interchange1, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "Credential Send - should not be [null]");

			var m = Regex.Match(interchange1.EI_BodyText, @"<Password>([^<]+)</Password", RegexOptions.IgnoreCase);
			NUnit.Framework.Assert.That(m.Success, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(DecodePassword(m.Groups[1].Value), NUnit.Framework.Is.EqualTo("PASSWORD"));

			m = Regex.Match(interchange1.EI_BodyText, @"<Passphrase>([^<]+)</Passphrase", RegexOptions.IgnoreCase);
			NUnit.Framework.Assert.That(m.Success, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(DecodePassword(m.Groups[1].Value), NUnit.Framework.Is.EqualTo("PASSPHRASE"));
		}

		[ExpectNoExceptions]
		public override void TestOnlySendCredentialIfNeeded()
		{
			var credential = CreateNewGlbExternalPassword(Factory);
			credential.GP_PasswordType = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "No credential Send - should be [null]");
			credential.Delete();
			Factory.Save();
			NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "No credential Send - should be [null]");
			credential = CreateNewGlbExternalPassword(Factory);
			credential.GP_PasswordType = PasswordTypesList.Codes.TVA;
			credential.GP_MailBoxID = "MBID";
			credential.GP_UserID = "USERID";
			credential.GP_Certificate = new ZBlob(System.Text.Encoding.UTF8.GetBytes("CERT"));
			credential.CurrentDecryptedCertificatePassphrase = "PASSWORD";
			Factory.Save();
			var interchange1 = Factory.GetLatestEHubConfigurationInterchange(InterchangeType);
			NUnit.Framework.Assert.That(interchange1, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "Credential Send - should not be [null]");
			credential.Delete();
			Factory.Save();
			var interchange2 = Factory.GetLatestEHubConfigurationInterchange(InterchangeType);
			NUnit.Framework.Assert.That(interchange2, NUnit.Framework.Is.Not.EqualTo(interchange1), "Should send credential on delete");
			interchange1.Delete();
			interchange2.Delete();
			Factory.Save();
			foreach (var data in new[] { new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.GP_MailBoxID, (ZString)"MBID"), new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.GP_UserID, (ZString)"USERID"), new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.GP_Certificate, new ZBlob(System.Text.Encoding.UTF8.GetBytes("CERT"))), new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.CurrentDecryptedCertificatePassphrase, (ZString)"PASSWORD"), new Tuple<string, IZType>(MasterFiles.Business.GlbExternalPassword.Schema.GP_PasswordType, (ZString)"TVA") })
			{
				credential = CreateNewGlbExternalPassword(Factory);
				credential.GP_PasswordType = ZString.Empty;
				credential.GP_MailBoxID = ZString.Empty;
				credential.GP_UserID = ZString.Empty;
				credential.GP_Certificate = ZBlob.Empty;
				credential.GP_CertificatePassPhrase = ZString.Empty;
				Factory.Save();
				NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "No credential Send - should be [null]");
				var info = credential.ZPropertyInfoHash.GetPropertySafe(data.Item1);
				info.Value = data.Item2;
				Factory.Save();
				interchange1 = Factory.GetLatestEHubConfigurationInterchange(InterchangeType);
				NUnit.Framework.Assert.That(interchange1, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Messaging.Integration.IEDIInterchange)), "Credential Send - should not be [null]");
				info.Value = info.Value.Default;
				System.Threading.Thread.Sleep(1);
				Factory.Save();
				interchange2 = Factory.GetLatestEHubConfigurationInterchange(InterchangeType);
				NUnit.Framework.Assert.That(interchange2, NUnit.Framework.Is.Not.EqualTo(interchange1), "Credential Send");
				info.Value = data.Item2;
				info.Value = info.Value.Default;
				System.Threading.Thread.Sleep(1);
				Factory.Save();
				NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(interchange2), "No new send as data hasn't changed seen saving");
				credential.Delete();
				Factory.Save();
				NUnit.Framework.Assert.That(Factory.GetLatestEHubConfigurationInterchange(InterchangeType), NUnit.Framework.Is.EqualTo(interchange2), "No new send as data hasn't changed seen saving");
				interchange1.Delete();
				interchange2.Delete();
				Factory.Save();
			}
		}

		[ExpectNoExceptions]
		public void TestCodeDescriptionProperty()
		{
			var type = Factory.NewWithValidTestData<T>().GetType();
			NUnit.Framework.Assert.That(DescriptionPropertyAttribute.DescriptionPropertyNameFromType(type), NUnit.Framework.Is.EqualTo(AutoGlbExternalPassword.Schema.GP_MailBoxID));
			NUnit.Framework.Assert.That(CodePropertyAttribute.CodePropertyNameFromType(type), NUnit.Framework.Is.EqualTo(AutoGlbExternalPassword.Schema.GP_MailBoxID));
		}

		[ExpectNoExceptions]
		public override void TestDataDefaultFromCertificate()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var issueDate = new ZDateTime(2017, 7, 21, 18, 14, 15);
			var expiryDate = new ZDateTime(2020, 7, 21, 18, 24, 00);
			var userID = "BOB";
			GlbExternalPassword.GP_IssueDate = issueDate;
			GlbExternalPassword.GP_ExpiryDate = expiryDate;
			GlbExternalPassword.GP_UserID = userID;
			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_IssueDate, NUnit.Framework.Is.EqualTo(issueDate), "GP_IssueDate");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_ExpiryDate, NUnit.Framework.Is.EqualTo(expiryDate), "GP_ExpiryDate");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_UserID, NUnit.Framework.Is.EqualTo(userID).Using(CustomComparers.TypeComparison), "GP_UserID");
			GlbExternalPassword.GP_Certificate = new byte[] { 241, 40 };
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_IssueDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty), "GP_IssueDate");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_ExpiryDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty), "GP_ExpiryDate");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_UserID, NUnit.Framework.Is.EqualTo(userID).Using(CustomComparers.TypeComparison), "GP_UserID");
			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_IssueDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty), "GP_IssueDate");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_ExpiryDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty), "GP_ExpiryDate");
		}

		[ExpectNoExceptions]
		public override void TestReadOnly()
		{
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_IssueDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "GP_IssueDateInfo.ReadOnly");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "GP_ExpiryDateInfo.ReadOnly");
			NUnit.Framework.Assert.That(GlbExternalPassword.GP_UserIDInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "GP_UserIDInfo.ReadOnly");
		}

		[ExpectNoExceptions]
		public void TestCreateCredentialXml()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "TWC";
			var wrapper = TWGlbStaffWrapper.Get(glbStaff);

			var password1 = wrapper.TWPasswordCollection.AddNew();
			password1.GP_PasswordType = PasswordTypesList.Codes.AUB;
			password1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password1.GP_MailBoxID = "CBK0123-0";
			password1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			password1.GP_GS = glbStaff.PK;
			password1.GP_UserID = "1";

			var password2 = wrapper.TWPasswordCollection.AddNew();
			password2.GP_PasswordType = PasswordTypesList.Codes.TVA;
			password2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password2.GP_MailBoxID = "CBK0124-0";
			password2.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			password2.GP_GS = glbStaff.PK;
			password2.GP_UserID = "2";

			var password3 = wrapper.TWPasswordCollection.AddNew();
			password3.GP_PasswordType = PasswordTypesList.Codes.UVC;
			password3.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password3.CurrentDecryptedCertificatePassphrase = "CaRgOw1sE";
			password3.GP_MailBoxID = "CBK0125-0";
			password3.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			password3.GP_GS = glbStaff.PK;
			password3.GP_UserID = "3";

			Factory.Save();

			var interchanges = GlbExternalPasswordTestHelper.GetLatestEHubConfigurationInterchanges(Factory, 3, EDIInterchangeTypeList.Codes.Configuration).ToArray();
			NUnit.Framework.Assert.That(interchanges.Length, NUnit.Framework.Is.EqualTo(1));

			var interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"TWCustomsSubscribers\""));
			AssertConfigurationValues(interchange, "TWCustomsSubscribers", 3);
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Contain("<UserName>1</UserName>"));
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Contain("<UserName>2</UserName>"));
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Contain("<UserName>3</UserName>"));

			interchanges.ForEach(x => x.Delete());
			Factory.Save();

			password1.Delete();
			Factory.Save();
			interchanges = GlbExternalPasswordTestHelper.GetLatestEHubConfigurationInterchanges(Factory, 3, EDIInterchangeTypeList.Codes.Configuration).ToArray();
			NUnit.Framework.Assert.That(interchanges.Length, NUnit.Framework.Is.EqualTo(1));
			interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"TWCustomsSubscribers\""));
			AssertConfigurationValues(interchange, "TWCustomsSubscribers", 2);
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Not.Contain("<UserName>1</UserName>"));
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Contain("<UserName>2</UserName>"));
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Contain("<UserName>3</UserName>"));

			interchanges.ForEach(x => x.Delete());
			Factory.Save();

			password2.Delete();
			password3.Delete();
			Factory.Save();
			interchanges = GlbExternalPasswordTestHelper.GetLatestEHubConfigurationInterchanges(Factory, 3, EDIInterchangeTypeList.Codes.Configuration).ToArray();
			NUnit.Framework.Assert.That(interchanges.Length, NUnit.Framework.Is.EqualTo(1));
			interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"TWCustomsSubscribers\""));
			AssertConfigurationValues(interchange, "TWCustomsSubscribers", 0);
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Not.Contain("<UserName>1</UserName>"));
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Not.Contain("<UserName>2</UserName>"));
			NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), NUnit.Framework.Does.Not.Contain("<UserName>3</UserName>"));
		}

		[ExpectNoExceptions]
		void AssertConfigurationValues(IEDIInterchange interchange, ZString configurationName, int certificateCount)
		{
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var configuration = reader.DeserializeToConfiguration();
				NUnit.Framework.Assert.That(configuration.Name, NUnit.Framework.Is.EqualTo(configurationName), "Configuration name should be set");
				var systemGroup = configuration.Group[0];
				NUnit.Framework.Assert.That(systemGroup.Type, NUnit.Framework.Is.EqualTo("System").Using(CustomComparers.TypeComparison), "System type should be set");
				var company = (Group)systemGroup.Items[0];
				NUnit.Framework.Assert.That(company.Type, NUnit.Framework.Is.EqualTo("Company").Using(CustomComparers.TypeComparison), "Company type should be set");
				var group = (Group)company.Items[0];
				NUnit.Framework.Assert.That(group.Type, NUnit.Framework.Is.EqualTo("Staff").Using(CustomComparers.TypeComparison), "Staff type should be set");
				NUnit.Framework.Assert.That(group.Reference, NUnit.Framework.Is.EqualTo("TWC").Using(CustomComparers.TypeComparison), "Staff should be TWC");
				var subGroupCount = group.Items?.Length ?? 0;
				NUnit.Framework.Assert.That(subGroupCount, NUnit.Framework.Is.EqualTo(certificateCount), "There should be one subgroup");
			}
		}

		[ExpectNoExceptions]
		public void TestGP_PasswordStatus()
		{
			var password = Factory.NewWithValidTestData<T>();
			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_UserID = "1";
			password.CurrentDecryptedPassword = "222";
			password.GP_MailBoxID = ValidMailBox;
			password.Validation.ValidateAll();
			Factory.Save();

			password = Factory.Load<T>(password.PK);
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");
		}

		public void TestGP_MailBoxID()
		{
			var password = Factory.NewWithValidTestData<T>();
			NUnit.Framework.Assert.That(password.GP_MailBoxIDInfo.MaxLength, NUnit.Framework.Is.EqualTo(16));
			AssertExceptionThrown<MaxLengthExceededException>(() => password.GP_MailBoxID = new ZString('X', 17));
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestChangeGP_PasswordStatus()
		{
			var password = Factory.NewWithValidTestData<T>();
			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.CurrentDecryptedPassword = "222";
			password.GP_UserID = "1";
			password.GP_MailBoxID = ValidMailBox;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");

			password.GP_PasswordType = ZString.Empty;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is INV");
			password.GP_PasswordType = ValidPasswordType;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");

			password.GP_Certificate = ZBlob.Empty;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is INV");
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");

			password.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is INV");
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");

			password.CurrentDecryptedPassword = ZString.Empty;
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is INV");
			password.CurrentDecryptedPassword = "222";
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");
		}

		[TestDate(2018, 12, 18)]
		[ExpectNoExceptions]
		public void TestCredentialItems()
		{
			var userId = "TWUSER";
			var password = Factory.NewWithValidTestData<T>();
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_UserID = userId;
			password.GP_ReceiveAutomatically = true;
			password.GP_PasswordType = PasswordTypesList.Codes.TVA;
			password.GP_MailBoxID = "CBK0123-0";
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			password.DisableConfigurationToSender = false;

			var userId1 = "TWUSER1";
			var password1 = Factory.NewWithValidTestData<T>();
			password1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password1.GP_UserID = userId1;
			password1.GP_ReceiveAutomatically = false;
			password1.GP_PasswordType = PasswordTypesList.Codes.CDS;
			password1.GP_MailBoxID = "CBK0124-0";
			password1.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			password1.DisableConfigurationToSender = false;

			Factory.Save();

			var interchange = GetLatestEHubConfigurationInterchanges(Factory, 3).FirstOrDefault();
			var xml = interchange.EI_BodyText;
			AssertXMLContains("<UserName>" + userId + "</UserName>", xml);
			AssertXMLContains("<Item Name=\"ReceiveAutomatically\">1</Item>", xml);
			AssertXMLContains("<Item Name=\"Platform\">TVA</Item>", xml);
			NUnit.Framework.Assert.That(xml.ToString(), NUnit.Framework.Does.Contain("<Group Type=\"MailBoxID\" Reference=\"" + password.GP_MailBoxID + "\" Status=\"" + password.GP_PasswordStatus + "\">"));
			NUnit.Framework.Assert.That(xml.ToString(), NUnit.Framework.Does.Contain("</Certificate>"));

			AssertXMLContains("<UserName>" + userId1 + "</UserName>", xml);
			AssertXMLContains("<Item Name=\"ReceiveAutomatically\">0</Item>", xml);
			AssertXMLContains("<Item Name=\"Platform\">CDS</Item>", xml);
			NUnit.Framework.Assert.That(xml.ToString(), NUnit.Framework.Does.Contain("<Group Type=\"MailBoxID\" Reference=\"" + password1.GP_MailBoxID + "\" Status=\"" + password1.GP_PasswordStatus + "\">"));
		}

		[ExpectNoExceptions]
		public void TestClearStatusReasonWhenStatusIsValOrEmpty()
		{
			var credential = Factory.NewWithValidTestData<T>();
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_UserID = "1";
			credential.CurrentDecryptedPassword = "222";
			credential.GP_MailBoxID = ValidMailBox;
			credential.Validation.ValidateAll();
			Factory.Save();

			var statusReasonMessage = "recv file by tymcomm+J fail! return code :6, please refer to tymcomm''s log";
			var loadedCredential = Factory.Load<T>(credential.PK);
			NUnit.Framework.Assert.That(loadedCredential.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Valid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus is VAL");

			loadedCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			loadedCredential.GP_StatusReason = statusReasonMessage;
			NUnit.Framework.Assert.That(loadedCredential.GP_StatusReason, NUnit.Framework.Is.EqualTo(statusReasonMessage).Using(CustomComparers.TypeComparison));

			loadedCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			NUnit.Framework.Assert.That(loadedCredential.GP_StatusReason, NUnit.Framework.Is.EqualTo(ZString.Empty), "GP_StatusReason value should be cleared");

			loadedCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			loadedCredential.GP_StatusReason = statusReasonMessage;
			NUnit.Framework.Assert.That(loadedCredential.GP_StatusReason, NUnit.Framework.Is.EqualTo(statusReasonMessage).Using(CustomComparers.TypeComparison));

			loadedCredential.GP_PasswordStatus = ZString.Empty;
			NUnit.Framework.Assert.That(loadedCredential.GP_StatusReason, NUnit.Framework.Is.EqualTo(ZString.Empty), "GP_StatusReason value should be cleared");
		}

		[ExpectNoExceptions]
		public void TestCredentialInterchangeType()
		{
			var credential = Factory.NewWithValidTestData<T>();
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_UserID = "TVCBBKTWTPE00123-TEST";
			credential.CurrentDecryptedPassword = "222";
			credential.GP_MailBoxID = "TFD0291";
			credential.Validation.ValidateAll();
			Factory.Save();

			var ediInterchange = Factory.Load<EDIInterchange>(new ZQuery()).FirstOrDefault();
			NUnit.Framework.Assert.That(ediInterchange.EI_InterchangeType, NUnit.Framework.Is.EqualTo(InterchangeType));
		}

		protected IEnumerable<EDIInterchange> GetLatestEHubConfigurationInterchanges(BusinessObjectFactory factory, int count) => GlbExternalPasswordTestHelper.GetLatestEHubConfigurationInterchanges(factory, count, InterchangeType);
	}

	public static class GlbExternalPasswordTestHelper
	{
		internal static IEnumerable<EDIInterchange> GetLatestEHubConfigurationInterchanges(BusinessObjectFactory factory, int count, ZString interchangeType)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(IEDIInterchange)) { MaximumRows = count };
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, interchangeType);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_From, ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_To, MasterFiles.Business.Customs.XmlCredential.Constants.Configuration.EHubRecipient);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.eHub);
			dbQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			return factory.Load<EDIInterchange>(dbQuery).Take(count);
		}
	}
}
