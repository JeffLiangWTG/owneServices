using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WebSecurityExtensionsTest : TestCaseWithFactory
	{
		public void TestIsRightGrantedWithoutCacheInNewModelOnlyForDocsAndReports_ForReports()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var stmMenuItem = Factory.New<StmMenuItem>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code~";
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Gender = "M";
			Factory.Save();
			var code = stmMenuItem.PK.ToString();
			var description = "desc";
			var rightCodeAndDescription = new CodeDescriptionPair(code, description);
			var webSecurityRight = new WebSecurityRight(rightCodeAndDescription.Code, (NoResString)rightCodeAndDescription.Description, WebSecurityApplication.EdiWebTracker, false);

			var orgSecurityRight = contact.Header.SecurityRights.AddNew();
			orgSecurityRight.OX_SU = webSecurityRight.SecurityGuid;
			orgSecurityRight.OX_Granted = true;
			Factory.Save();

			AssertEquals("pre-condition", false, GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value);
			AssertEquals("Granted", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

			orgSecurityRight.OX_Granted = false;
			Factory.Save();
			AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

			using (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("pre-condition", true, GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value);

				orgSecurityRight.OX_Granted = true;
				Factory.Save();
				AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				orgSecurityRight.OX_Granted = false;
				Factory.Save();
				AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				var glbSecurity = Factory.New<GlbSecurity>();
				var glbGroup = Factory.New<GlbGroup>();
				glbSecurity.GU_ItemGUID = webSecurityRight.SecurityGuid;
				glbSecurity.GU_GG = glbGroup.PK;
				Factory.Save();
				AssertEquals("Not Granted before adding contact to security group", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				// add contact to security group
				var glbGroupOrgContactLink = Factory.New<GlbGroupOrgContactLink>();
				glbGroupOrgContactLink.GCK_GG_Group = glbGroup.PK;
				glbGroupOrgContactLink.GCK_OC_Contact = contact.PK;
				Factory.Save();
				AssertEquals("Granted after adding contact to security group", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				orgSecurityRight.OX_Granted = true;
				Factory.Save();
				AssertEquals("Granted", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				orgSecurityRight.OX_Granted = false;
				Factory.Save();
				AssertEquals("Granted", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				glbSecurity.GU_ItemGUID = Guid.NewGuid();
				Factory.Save();
				AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));
			}
		}

		public void TestIsRightGrantedWithoutCacheInNewModelOnlyForDocsAndReports_ForDocs()
		{
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var code = "DocCode";
			var description = "Doc: desc";
			var rightCodeAndDescription = new CodeDescriptionPair(code, description);
			var webSecurityRight = new WebSecurityRight(rightCodeAndDescription.Code, (NoResString)rightCodeAndDescription.Description, WebSecurityApplication.EdiWebTracker, true);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code~";
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Gender = "M";
			Factory.Save();
			var orgSecurityRight = contact.Header.SecurityRights.AddNew();
			orgSecurityRight.OX_SecurityItemName = webSecurityRight.SecurityItemName;
			orgSecurityRight.OX_Granted = true;
			Factory.Save();

			AssertEquals("pre-condition", false, GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value);
			AssertEquals("Granted", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

			orgSecurityRight.OX_Granted = false;
			Factory.Save();
			AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

			using (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("pre-condition", true, GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value);

				orgSecurityRight.OX_Granted = true;
				Factory.Save();
				AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				orgSecurityRight.OX_Granted = false;
				Factory.Save();
				AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				var glbSecurity = Factory.New<GlbSecurity>();
				var glbGroup = Factory.New<GlbGroup>();
				glbSecurity.GU_SecurityRight = webSecurityRight.SecurityItemName;
				glbSecurity.GU_GG = glbGroup.PK;
				Factory.Save();
				AssertEquals("Not Granted before adding contact to the security group", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				// add contact to security group
				var glbGroupOrgContactLink = Factory.New<GlbGroupOrgContactLink>();
				glbGroupOrgContactLink.GCK_GG_Group = glbGroup.PK;
				glbGroupOrgContactLink.GCK_OC_Contact = contact.PK;
				Factory.Save();
				AssertEquals("Granted after adding contact to the security group", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				orgSecurityRight.OX_Granted = true;
				Factory.Save();
				AssertEquals("Granted", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				orgSecurityRight.OX_Granted = false;
				Factory.Save();
				AssertEquals("Granted", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));

				glbSecurity.GU_SecurityRight = webSecurityRight + "Not";
				Factory.Save();
				AssertEquals("Not Granted", false, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));
			}
		}
	}
}
