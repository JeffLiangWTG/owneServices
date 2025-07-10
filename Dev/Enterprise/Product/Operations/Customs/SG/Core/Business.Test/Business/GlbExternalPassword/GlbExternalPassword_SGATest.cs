using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_SGA))]
	class GlbExternalPassword_SGATest : GlbExternalPassword_SGTest<GlbExternalPassword_SGA>
	{
		public void TestDefaultPasswordIfNeed()
		{
			var staff1 = CreateNewStaff("ST1");
			var staff2 = CreateNewStaff("ST2");
			var staff3 = CreateNewStaff("ST3");
			var staff4 = CreateNewStaff("ST4");
			var userID = "TESTUSER00";
			var password1 = CreateNewPassword(userID, staff1.PK);
			password1.GP_MailBoxID = "USER1@MAIL.COM";
			var password2 = CreateNewPassword(userID, staff2.PK);
			password2.GP_MailBoxID = "USER2@MAIL.COM";
			password1.CurrentDecryptedPassword = "CPW001";
			password1.NextDecryptedPassword = "NPW001";
			password1.GP_PasswordStatus = Core.Constants.PasswordOK;
			password2.CurrentDecryptedPassword = "CPW002";
			password2.NextDecryptedPassword = "NPW002";
			password2.GP_PasswordStatus = Core.Constants.PasswordOK;
			Factory.Save();
			var password = CreateNewPassword(userID, staff4.PK);
			password.ShouldDefaultPasswordsEvent += () => true;
			AssertEquals("Should not default CurrentDecryptedPassword as there are two valid linked staff records which are using different current passwords.", string.Empty, password.CurrentDecryptedPassword);
			AssertEquals("Should not default NextDecryptedPassword as there are two valid linked staff records which are using different next passwords.", string.Empty, password.NextDecryptedPassword);
			AssertEquals("Should not default GP_PasswordStatus as there are two valid linked staff records which are using different passwords.", string.Empty, password.GP_PasswordStatus);
			password2.GP_PasswordStatus = "INV";
			password.GP_UserID = string.Empty;
			password.GP_UserID = userID;
			AssertEquals("Should default CurrentDecryptedPassword as there is only one valid linked staff record.", password1.CurrentDecryptedPassword, password.CurrentDecryptedPassword);
			AssertEquals("Should default NextDecryptedPassword as there is only one valid linked staff record.", password1.NextDecryptedPassword, password.NextDecryptedPassword);
			AssertEquals("Should default GP_PasswordStatus as there is only one valid linked staff record.", Core.Constants.PasswordOK, password.GP_PasswordStatus);
			password.CurrentDecryptedPassword = string.Empty;
			password.NextDecryptedPassword = string.Empty;
			password.GP_PasswordStatus = string.Empty;
			password2.CurrentDecryptedPassword = "CPW001";
			password2.GP_PasswordStatus = Core.Constants.PasswordOK;
			password.GP_UserID = string.Empty;
			password.GP_UserID = userID;
			AssertEquals("Should default CurrentDecryptedPassword as there are two valid linked staff records which are using same current password.", password1.CurrentDecryptedPassword, password.CurrentDecryptedPassword);
			AssertEquals("Should not default NextDecryptedPassword as there are two valid linked staff records which are using different next passwords.", string.Empty, password.NextDecryptedPassword);
			AssertEquals("Should default GP_PasswordStatus as there are two valid linked staff records which are using same current password.", Core.Constants.PasswordOK, password.GP_PasswordStatus);

			staff1.GS_IsActive = false;
			staff2.GS_IsDevice = true;
			staff3.GS_CanLogin = false;
			var password3 = CreateNewPassword(userID, staff3.PK);
			password3.CurrentDecryptedPassword = "CPW001";
			password3.NextDecryptedPassword = "NPW001";
			password3.GP_PasswordStatus = Core.Constants.PasswordOK;
			password1.CurrentDecryptedPassword = "CPW001";
			password1.NextDecryptedPassword = "NPW001";
			password1.GP_PasswordStatus = Core.Constants.PasswordOK;
			password2.CurrentDecryptedPassword = "CPW001";
			password2.NextDecryptedPassword = "NPW001";
			password2.GP_PasswordStatus = Core.Constants.PasswordOK;
			password.CurrentDecryptedPassword = string.Empty;
			password.NextDecryptedPassword = string.Empty;
			password.GP_PasswordStatus = string.Empty;

			password.GP_UserID = string.Empty;
			password.GP_UserID = userID;
			AssertEquals("Should not default CurrentDecryptedPassword as all passwords with same user ID are not linked with a staff which is active, non-device and can login.", string.Empty, password.CurrentDecryptedPassword);
			AssertEquals("Should not default NextDecryptedPassword as all passwords with same user ID are not linked with a staff which is active, non-device and can login.", string.Empty, password.NextDecryptedPassword);
			AssertEquals("Should not default GP_PasswordStatus as all passwords with same user ID are not linked with a staff which is active, non-device and can login.", string.Empty, password.GP_PasswordStatus);
		}

		public void TestSyncPasswordIfNeed()
		{
			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;
			var staffCode = 0;
			var currentCompanyPk = GlbCompany.CurrentCompany.PK;
			var otherCompanyPk = newFactory.NewWithValidTestData<GlbCompany>().PK;
			var glbGroup = newFactory.New<GlbGroup>();
			glbGroup.GG_Code = "ZAC";
			GlbExternalPassword_SGA CreatePassword(BusinessObjectFactory factory, ZString statusReason, ZString userId, ZString currentPassword, ZString nextPassword, ZString status, ZGuid companyPk)
			{
				var id = Guid.NewGuid().ToString();
				var staff = factory.New<GlbStaff>();
				staff.FillWithValidTestData();
				staff.GS_IsActive = true;
				staff.GS_IsDevice = false;
				staff.GS_CanLogin = true;
				staff.GS_Code = $"X{staffCode:00}";
				staff.GS_LoginName = id;
				var result = factory.New<GlbExternalPassword_SGA>();
				result.GP_GG = glbGroup.PK;
				result.GP_GC = companyPk;
				result.GP_GS = staff.PK;
				result.GP_StatusReason = statusReason;
				result.CurrentDecryptedPassword = currentPassword;
				result.NextDecryptedPassword = nextPassword;
				result.GP_PasswordStatus = status;
				result.GP_MailBoxID = id;
				result.GP_UserID = userId;
				staffCode++;
				return result;
			}

			var passwordsForSync = new[] { CreatePassword(newFactory, "Different Current Decrypted Password", "TEST000", "CPW001", "NPW000", Core.Constants.PasswordOK, currentCompanyPk), CreatePassword(newFactory, "Different Next Decrypted Password", "TEST000", "CPW000", "NPW001", Core.Constants.PasswordOK, currentCompanyPk), CreatePassword(newFactory, "Different Password Status", "TEST000", "CPW000", "NPW000", "XYZ", currentCompanyPk), };
			var passwordsForKeep = new[] { CreatePassword(newFactory, "Same Password Info", "TEST000", "CPW000", "NPW000", Core.Constants.PasswordOK, currentCompanyPk), CreatePassword(newFactory, "Different Company PK", "TEST000", "CPW000", "NPW000", Core.Constants.PasswordOK, otherCompanyPk), CreatePassword(newFactory, "Different User ID", "TEST001", "CPW000", "NPW000", Core.Constants.PasswordOK, currentCompanyPk) };
			newFactory.Save();
			var password = CreatePassword(Factory, "Changed Password", "TEST000", "CPW000", "NPW000", Core.Constants.PasswordOK, currentCompanyPk);
			password.SyncPassword(password.GetPasswordsForSync());
			Factory.Save();
			CombineAssertions(() =>
			{
				newFactory = NewFactory();
				newFactory.RefreshEnabled = false;
				foreach (var passwordForSync in passwordsForSync)
				{
					var reason = passwordForSync.GP_StatusReason.ToLower();
					var passwordInNewFactory = newFactory.Load<GlbExternalPassword_SGA>(passwordForSync.PK);
					AssertEquals($"Should sync CurrentDecryptedPassword as this password has {reason}.", password.CurrentDecryptedPassword, passwordInNewFactory.CurrentDecryptedPassword);
					AssertEquals($"Should sync NextDecryptedPassword as this password has {reason}.", password.NextDecryptedPassword, passwordInNewFactory.NextDecryptedPassword);
					AssertEquals($"Should sync GP_PasswordStatus as this password has {reason}.", password.GP_PasswordStatus, passwordInNewFactory.GP_PasswordStatus);
				}

				foreach (var passwordForKeep in passwordsForKeep)
				{
					var reason = passwordForKeep.GP_StatusReason.ToLower();
					var passwordInNewFactory = newFactory.Load<GlbExternalPassword_SGA>(passwordForKeep.PK);
					AssertEquals($"Should not sync CurrentDecryptedPassword as this password has {reason}.", passwordForKeep.CurrentDecryptedPassword, passwordInNewFactory.CurrentDecryptedPassword);
					AssertEquals($"Should not sync NextDecryptedPassword as this password has {reason}.", passwordForKeep.NextDecryptedPassword, passwordInNewFactory.NextDecryptedPassword);
					AssertEquals($"Should not sync GP_PasswordStatus as this password has {reason}.", passwordForKeep.GP_PasswordStatus, passwordInNewFactory.GP_PasswordStatus);
				}

				var interchange = Factory.GetLatestEHubConfigurationInterchange();
				using (var reader = interchange.GetEI_BodyTextReader())
				{
					var configuration = reader.DeserializeToConfiguration();
					var group = AssertTopGroups(configuration, 3);
					void AssertStaffGroup(int index, string reference)
					{
						var staffGroup = group.Items[index] as Group;
						AssertNotNull("Other staff records are included in the Credential XML output.", staffGroup);
						AssertEquals("Reference", reference, staffGroup.Reference);
						var accountGroup = (Group)staffGroup.Items[0];
						AssertEquals("Sub Group Type should be SGCustomsAccount.", "SGCustomsAccount", accountGroup.Type);
						var currentCredential = (Credential)accountGroup.Items[0];
						AssertEquals("Current credential name should be set.", "Current", currentCredential.Name);
						AssertEquals("Current credential user name should be set.", "TEST000", currentCredential.UserName);
						AssertNotNull("Current credential password should be set.", currentCredential.Password);
						var nextCredential = (Credential)accountGroup.Items[1];
						AssertEquals("Next credential name should be set.", "Next", nextCredential.Name);
						AssertEquals("Next credential user name should be set.", "TEST000", nextCredential.UserName);
						AssertNotNull("Next credential password should be set.", nextCredential.Password);
					}

					AssertStaffGroup(0, "X00");
					AssertStaffGroup(1, "X01");
					AssertStaffGroup(2, "X06");
				}
			}

			);
		}

		public void TestSyncPasswordWhenLinkedWithInvalidStaff()
		{
			var staff1 = CreateNewStaff("ST1");
			var staff2 = CreateNewStaff("ST2");
			var staff3 = CreateNewStaff("ST3");
			var staff4 = CreateNewStaff("ST4");
			var userID = "TESTUSER00";
			var password1 = CreateNewPassword(userID, staff1.PK);
			var password2 = CreateNewPassword(userID, staff2.PK);
			var password3 = CreateNewPassword(userID, staff3.PK);
			var passwordsForSync = new[] { password1, password2, password3 };
			var password = CreateNewPassword(userID, staff4.PK);
			password1.CurrentDecryptedPassword = string.Empty;
			password1.NextDecryptedPassword = string.Empty;
			password1.GP_PasswordStatus = string.Empty;
			password2.CurrentDecryptedPassword = string.Empty;
			password2.NextDecryptedPassword = string.Empty;
			password2.GP_PasswordStatus = string.Empty;
			password3.CurrentDecryptedPassword = string.Empty;
			password3.NextDecryptedPassword = string.Empty;
			password3.GP_PasswordStatus = string.Empty;
			password.CurrentDecryptedPassword = "CPW004";
			password.NextDecryptedPassword = "NPW004";
			password.GP_PasswordStatus = Core.Constants.PasswordOK;
			staff1.GS_IsActive = false;
			staff2.GS_IsDevice = true;
			staff3.GS_CanLogin = false;
			password.SyncPassword(password.GetPasswordsForSync());
			foreach (var passwordForSync in passwordsForSync)
			{
				AssertEquals("Should not sync CurrentDecryptedPassword as the password is not linked with a staff which is active, non-device and can login.", string.Empty, passwordForSync.CurrentDecryptedPassword);
				AssertEquals("Should not sync NextDecryptedPassword as the password is not linked with a staff which is active, non-device and can login.", string.Empty, passwordForSync.NextDecryptedPassword);
				AssertEquals("Should not sync GP_PasswordStatus as the password is not linked with a staff which is active, non-device and can login.", string.Empty, passwordForSync.GP_PasswordStatus);
			}

			staff1.GS_IsActive = true;
			staff2.GS_IsDevice = false;
			staff3.GS_CanLogin = true;
			password.SyncPassword(password.GetPasswordsForSync());
			foreach (var passwordForSync in passwordsForSync)
			{
				AssertEquals("Should sync CurrentDecryptedPassword as the password is linked with a staff which is active, non-device and can login.", "CPW004", passwordForSync.CurrentDecryptedPassword);
				AssertEquals("Should sync NextDecryptedPassword as the password is linked with a staff which is active, non-device and can login.", "NPW004", passwordForSync.NextDecryptedPassword);
				AssertEquals("Should sync GP_PasswordStatus as the password is linked with a staff which is active, non-device and can login.", Core.Constants.PasswordOK, passwordForSync.GP_PasswordStatus);
			}
		}

		public override void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertEquals(PasswordTypesList.Codes.SGA, GlbExternalPassword.GP_PasswordType);
		}

		public override void TestValidation()
		{
			AssertType<GlbExternalPasswordValidation_SGA>(GlbExternalPassword.Validation);
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

		public void TestSGAAddToStaffCredential_ValPwd()
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

		public void TestSGAAddToStaffCredential_InvPwd()
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

		public void TestSGAAddToStaffCredential_EmptyCurrentPassword()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.CurrentDecryptedPassword = string.Empty;
			GlbExternalPassword.NextDecryptedPassword = "TEST";
			GlbExternalPassword.GP_UserID = "ZAC";
			GlbExternalPassword.GP_PasswordType = "NTP";
			GlbExternalPassword.GP_PasswordStatus = "INV";
			Factory.Save();
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				AssertConfigurationValues(reader.DeserializeToConfiguration(), "INV", true, false);
			}
		}

		public void TestSGAAddToStaffCredential_EmptyNextPassword()
		{
			GlbExternalPassword.GP_PasswordStatus = "XYZ";
			GlbExternalPassword.CurrentDecryptedPassword = "TEST";
			GlbExternalPassword.NextDecryptedPassword = string.Empty;
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

		void AssertConfigurationValues(Configuration configuration, ZString status, bool hasNextSegment, bool hasCurrentSegment = true)
		{
			var group = AssertTopGroups(configuration, 1);
			var subgroup = (Group)group.Items[0];
			AssertEquals("Group type should be set", "NTP", subgroup.Type);
			AssertEquals("Group status should be set", status, subgroup.Status);
			var expectedCount = hasCurrentSegment && hasNextSegment ? 2 : hasCurrentSegment || hasNextSegment ? 1 : 0;
			AssertEquals($"There should be {expectedCount} subgroups", expectedCount, subgroup.Items.Length);
			var currentCredential = subgroup.Items.Cast<Credential>().FirstOrDefault(c => c.Name == "Current");
			var nextCredential = subgroup.Items.Cast<Credential>().FirstOrDefault(c => c.Name == "Next");
			AssertEquals(hasCurrentSegment, currentCredential != null);
			AssertEquals(hasNextSegment, nextCredential != null);
			if (hasCurrentSegment)
			{
				AssertEquals("Credential user name should be set", "ZAC", currentCredential.UserName);
				AssertNotNull("Credential password should be set", currentCredential.Password);
			}

			if (hasNextSegment)
			{
				AssertEquals("Credential user name should be set", "ZAC", nextCredential.UserName);
				AssertNotNull("Credential password should be set", nextCredential.Password);
			}
		}

		Group AssertTopGroups(Configuration configuration, int subGroupCount)
		{
			AssertEquals("Configuration name should be set", "SGCustomsConfiguration", configuration.Name);
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
			AssertEquals("Group type should be set", "Group", group.Type);
			AssertEquals("Group reference should be set", "ZAC", group.Reference);
			AssertEquals($"There should be {subGroupCount} subgroup", subGroupCount, group.Items.Length);
			return group;
		}

		protected override GlbExternalPassword_SGA CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var result = factory.New<GlbExternalPassword_SGA>();
			result.GP_GG = Group.PK;
			return result;
		}

		GlbExternalPassword_SGA CreateNewPassword(string userID, ZGuid staffPK)
		{
			var password = Factory.New<GlbExternalPassword_SGA>();
			password.GP_GG = Group.PK;
			password.GP_GS = staffPK;
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_UserID = userID;
			return password;
		}

		GlbStaff CreateNewStaff(string code)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = Guid.NewGuid().ToString();
			staff.GS_IsActive = true;
			staff.GS_IsDevice = false;
			staff.GS_CanLogin = true;
			return staff;
		}

		GlbGroup Group
		{
			get
			{
				if (glbGroup == null)
				{
					glbGroup = Factory.New<GlbGroup>();
					glbGroup.GG_Code = "ZAC";
				}

				return glbGroup;
			}
		}

		GlbGroup glbGroup;
	}
}
