using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPassword))]
	public abstract class GlbExternalPasswordTest<T> : EnterpriseBusinessObjectTestCase
			where T : GlbExternalPassword
	{
		public virtual void TestCredentialRecipient()
		{
			var credential = (T)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals(CredentialRecipient.eHub, credential.CredentialRecipient);
		}

		public virtual void TestIsCorrectlySetupForTypeDecider()
		{
			var credential = CreateNewGlbExternalPassword(Factory);
			Factory.Save();

			var reloadedCredential = new BusinessObjectFactory().Load<GlbExternalPassword>(credential.PK);
			AssertType<T>("Ensure that GlbExternalPasswordProviders contain the right setting to load this correctly via TypeDecider", reloadedCredential);
		}

		public void TestMaximumDecryptedLength()
		{
			Assert("CurrentDecryptedCertificatePassphrase Maximum Length cannot be longer than what GP_CertificatePassPhrase supports after TwoWayEncoder Encryption", GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo.MaxLength <= Enterprise.MasterFiles.Business.GlbExternalPassword.Schema.CurrentDecryptedCertificatePassphraseMaxLength);
			Assert("CurrentDecryptedPassword Maximum Length cannot be longer than what GP_CurrentPassword supports after TwoWayEncoder Encryption", GlbExternalPassword.CurrentDecryptedPasswordInfo.MaxLength <= Enterprise.MasterFiles.Business.GlbExternalPassword.Schema.CurrentDecryptedPasswordMaxLength);
			Assert("NextDecryptedPassword Maximum Length cannot be longer than what GP_NextPassword supports after TwoWayEncoder Encryption", GlbExternalPassword.NextDecryptedPasswordInfo.MaxLength <= Enterprise.MasterFiles.Business.GlbExternalPassword.Schema.NextDecryptedPasswordMaxLength);
		}

		public virtual void TestOnlySendCredentialIfNeeded()
		{
			var credential = CreateNewGlbExternalPassword(Factory);
			if (credential.ConfigurationName.IsEmpty)
			{
				Assert("No need to send", true);
			}
			else
			{
				Factory.Save();
				AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());
				credential.Delete();
				Factory.Save();
				AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());

				credential = CreateNewGlbExternalPassword(Factory);
				SetCredentialData(credential);
				Factory.Save();
				var interchange1 = Factory.GetLatestEHubConfigurationInterchange();
				AssertNotNull("Credential Send", interchange1);
				credential.Delete();
				Factory.Save();
				var interchange2 = Factory.GetLatestEHubConfigurationInterchange();
				AssertNotEquals("Should send credential on delete", interchange1, interchange2);

				interchange1 = Factory.GetLatestEHubConfigurationInterchange();
				credential = CreateNewGlbExternalPassword(Factory);
				ClearInitialCredentialData(credential);
				Factory.Save();
				AssertEquals("No credential Send", interchange1, Factory.GetLatestEHubConfigurationInterchange());

				SetCredentialData(credential);
				Factory.Save();
				interchange2 = Factory.GetLatestEHubConfigurationInterchange();
				AssertNotEquals("Credential Send", interchange1, interchange2);

				ClearCredentialData(credential);
				Factory.Save();
				interchange1 = Factory.GetLatestEHubConfigurationInterchange();
				AssertNotEquals("Credential Send", interchange1, interchange2);

				SetCredentialData(credential);
				ClearCredentialData(credential);
				Factory.Save();
				AssertEquals("No new send as data hasn't changed seen saving", interchange1, Factory.GetLatestEHubConfigurationInterchange());

				credential.Delete();
				Factory.Save();
				AssertEquals("No new send as data hasn't changed seen saving", interchange1, Factory.GetLatestEHubConfigurationInterchange());
			}
		}

		protected virtual void SetCredentialData(T credential)
		{
			credential.CurrentDecryptedPassword = "HELLO";
		}

		protected virtual void ClearCredentialData(T credential)
		{
			credential.CurrentDecryptedPassword = ZString.Empty;
		}

		protected virtual void ClearInitialCredentialData(T credential)
		{
			credential.CurrentDecryptedPassword = ZString.Empty;
			credential.GP_PasswordStatus = ZString.Empty;
		}

		#region Implementation

		protected T GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = CreateNewGlbExternalPassword(Factory);
				}
				return glbExternalPassword;
			}
		}
		T glbExternalPassword;

		protected virtual T CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var result = factory.New<T>();
			result.GP_GS = Staff.PK;
			return result;
		}

		protected GlbStaff Staff
		{
			get
			{
				if (glbStaff == null)
				{
					glbStaff = Factory.New<GlbStaff>();
					glbStaff.GS_Code = "ZAC";
				}
				return glbStaff;
			}
		}
		GlbStaff glbStaff;

		protected override BusinessObject GetNewBusinessObject()
		{
			return GlbExternalPassword;
		}

		#endregion
	}
}
