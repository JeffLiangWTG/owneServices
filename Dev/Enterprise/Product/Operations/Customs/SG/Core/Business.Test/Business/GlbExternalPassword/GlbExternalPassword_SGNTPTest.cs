using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_SGNTP))]
	class GlbExternalPassword_SGNTPTest : GlbExternalPassword_SGTest<GlbExternalPassword_SGNTP>
	{
		public override void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertEquals(PasswordTypesList.Codes.NTP, GlbExternalPassword.GP_PasswordType);
		}

		public void TestSGNTPAddToStaffCredential_ValPwd()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.CurrentDecryptedPassword = "TEST";
			GlbExternalPassword.NextDecryptedPassword = "NXT";
			GlbExternalPassword.GP_UserID = "ZAC";
			GlbExternalPassword.GP_PasswordType = "NTP";
			Factory.Save();
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				AssertConfigurationValues(reader.DeserializeToConfiguration(), "VAL", true);
			}
		}

		public void TestSGNTPAddToStaffCredential_InvPwd()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.CurrentDecryptedPassword = "TEST";
			GlbExternalPassword.NextDecryptedPassword = "NXT";
			GlbExternalPassword.GP_UserID = "ZAC";
			GlbExternalPassword.GP_PasswordType = "NTP";
			GlbExternalPassword.GP_PasswordStatus = "INV";
			Factory.Save();
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				AssertConfigurationValues(reader.DeserializeToConfiguration(), "INV", true);
			}
		}

		public override void TestOnlySendCredentialIfNeeded()
		{
			base.TestOnlySendCredentialIfNeeded();
			var credential = CreateNewGlbExternalPassword(Factory);
			credential.CurrentDecryptedPassword = "TEST";
			Factory.Save();
			var interchange1 = Factory.GetLatestEHubConfigurationInterchange();
			credential.NextDecryptedPassword = "HI";
			Factory.Save();
			var interchange2 = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotEquals(interchange1, interchange2);
			credential.NextDecryptedPassword = ZString.Empty;
			Factory.Save();
			AssertEquals(interchange2, Factory.GetLatestEHubConfigurationInterchange());
		}

		public void TestSGNTPAddToStaffCredential_EmptyNextPassword()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.CurrentDecryptedPassword = "TEST";
			GlbExternalPassword.GP_UserID = "ZAC";
			GlbExternalPassword.GP_PasswordType = "NTP";
			GlbExternalPassword.GP_PasswordStatus = "INV";
			Factory.Save();
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				AssertConfigurationValues(reader.DeserializeToConfiguration(), "INV", false);
			}
		}

		void AssertConfigurationValues(Configuration configuration, ZString status, ZBool hasNextSegment)
		{
			AssertEquals("Configuration name should be set", "SGCustomsNTP", configuration.Name);
			AssertEquals("There should be one group", 1, configuration.Group.Count);
			var systemGroup = configuration.Group[0];
			AssertEquals("Group type should be set", "System", systemGroup.Type);
			AssertEquals("Group reference should be set", "EDIDAT", systemGroup.Reference);
			AssertEquals("There should be one subgroup", 1, systemGroup.Items.Length);
			var companyGroup = (Group)systemGroup.Items[0];
			AssertEquals("Group type should be set", "Company", companyGroup.Type);
			AssertEquals("Group reference should be set", "EDI", companyGroup.Reference);
			AssertEquals("There should be one subgroup", 1, companyGroup.Items.Length);
			var group = (Group)companyGroup.Items[0];
			AssertEquals("Group type should be set", "Staff", group.Type);
			AssertEquals("Group reference should be set", "ZAC", group.Reference);
			AssertEquals("There should be one subgroup", 1, group.Items.Length);
			var subgroup = (Group)group.Items[0];
			AssertEquals("Group type should be set", "NTP", subgroup.Type);
			AssertEquals("Group status should be set", status, subgroup.Status);
			AssertEquals("There should be two subgroups", hasNextSegment ? 2 : 1, subgroup.Items.Length);
			var currentCredential = (Credential)subgroup.Items[0];
			AssertEquals("Credential name should be set", "Current", currentCredential.Name);
			AssertEquals("Credential user name should be set", "ZAC", currentCredential.UserName);
			AssertNotNull("Credential password should be set", currentCredential.Password);
			if (hasNextSegment)
			{
				var nextCredential = (Credential)subgroup.Items[1];
				AssertEquals("Credential name should be set", "Next", nextCredential.Name);
				AssertEquals("Credential user name should be set", "ZAC", nextCredential.UserName);
				AssertNotNull("Credential password should be set", nextCredential.Password);
			}
		}
	}
}
