using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ExternalPasswordConfigurationToSenderTest : TestCaseWithFactory
	{
		public void TestSendingRegistrations()
		{
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, null, null, null, null);
			var senders = ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess;
			AssertEquals(0, senders.Count);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "A", null, null, null);
			AssertEquals(1, senders.Count);
			AssertEquals(1, senders[Factory].ConfigurationsToBeSend.Count);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "A", null, null, null);
			AssertEquals(1, senders.Count);
			AssertEquals(2, senders[Factory].ConfigurationsToBeSend.Count);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "B", null, null, null);
			AssertEquals(1, senders.Count);
			AssertEquals(3, senders[Factory].ConfigurationsToBeSend.Count);

			var configurationsToBeSend = senders[Factory].ConfigurationsToBeSend;
			Assert(configurationsToBeSend.Count(x => x.ConfigurationName == "B") == 1);
			Assert(configurationsToBeSend.Count(x => x.ConfigurationName == "A") == 2);
			Factory.Save();
			senders = ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess;
			AssertEquals(0, senders.Count);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "A", null, null, null);

			var newFactory = new BusinessObjectFactory();
			ExternalPasswordConfigurationToSender.RegisterForSending(newFactory, "A", null, null, null);

			senders = ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess;
			AssertEquals(2, senders.Count);

			Factory.Save();

			newFactory.Save();
			senders = ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess;
			AssertEquals(0, senders.Count);
		}

		public void TestRegisterForSending()
		{
			var glbcompany = Factory.NewWithValidTestData<GlbCompany>();
			glbcompany.GC_Code = "CMP";
			var glbgroup = Factory.NewWithValidTestData<GlbGroup>();
			glbgroup.GG_Code = "GG";
			var glbstaff = Factory.NewWithValidTestData<GlbStaff>();
			glbstaff.GS_Code = "XXX";

			var passwordA1 = Factory.New<GlbExternalPasswordForATest>();
			passwordA1.GP_PasswordStatus = "VAL";
			passwordA1.GP_PasswordType = "A1";
			passwordA1.GP_UserID = "A1";
			passwordA1.GP_GC = glbcompany.PK;
			passwordA1.GP_GG = glbgroup.PK;
			passwordA1.GP_GS = glbstaff.PK;

			var passwordA2 = Factory.New<GlbExternalPasswordForATest>();
			passwordA2.GP_PasswordStatus = "VAL";
			passwordA2.GP_PasswordType = "A2";
			passwordA2.GP_UserID = "A2";
			passwordA2.GP_GC = glbcompany.PK;
			passwordA2.GP_GG = glbgroup.PK;
			passwordA2.GP_GS = glbstaff.PK;

			var passwordA3 = Factory.New<GlbExternalPasswordForATest>();
			passwordA3.GP_PasswordStatus = "VAL";
			passwordA3.GP_PasswordType = "A3";
			passwordA3.GP_UserID = "A3";
			passwordA3.GP_GC = glbcompany.PK;
			passwordA3.GP_GG = glbgroup.PK;
			passwordA3.GP_GS = glbstaff.PK;

			var passwordB1 = Factory.New<GlbExternalPasswordForBTest>();
			passwordB1.GP_PasswordStatus = "VAL";
			passwordB1.GP_PasswordType = "B";
			passwordB1.GP_UserID = "B";
			passwordB1.GP_GC = glbcompany.PK;
			passwordB1.GP_GG = glbgroup.PK;
			passwordB1.GP_GS = glbstaff.PK;

			var passwordC1 = Factory.New<GlbExternalPasswordForCTest>();
			passwordC1.GP_PasswordStatus = "VAL";
			passwordC1.GP_PasswordType = "C";
			passwordC1.GP_UserID = "C";
			passwordC1.GP_GC = glbcompany.PK;
			passwordC1.GP_GG = glbgroup.PK;
			passwordC1.GP_GS = glbstaff.PK;
			Factory.Save();

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "", glbcompany, glbgroup, glbstaff);
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNull(interchange);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "BB", null, null, null);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess[Factory].ConfigurationsToBeSend.Count);
			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "BB", null, null, null);

			ClearInterchange(interchange);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "BB", null, null, glbstaff);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess[Factory].ConfigurationsToBeSend.Count);
			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "BB", null, null, "XXX");

			ClearInterchange(interchange);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "BB", glbcompany, null, glbstaff);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess[Factory].ConfigurationsToBeSend.Count);
			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "BB", "CMP", null, "XXX");

			ClearInterchange(interchange);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "BB", glbcompany, glbgroup, glbstaff);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess[Factory].ConfigurationsToBeSend.Count);
			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "BB", "CMP", "GG", "XXX");

			ClearInterchange(interchange);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST1", glbcompany, glbgroup, glbstaff);
			Factory.Save();
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", "CMP", "GG", "XXX", 3);

			ClearInterchange(interchange);

			passwordA1.Delete();
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST1", glbcompany, glbgroup, glbstaff);
			Factory.Save();
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", "CMP", "GG", "XXX", 2);

			ClearInterchange(interchange);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST1", glbcompany, glbgroup, glbstaff, EDIInterchangeTypeList.Codes.CustomsWare, CredentialRecipient.DirectxT);

			passwordA2.CredentialRecipientExpose = CredentialRecipient.DirectxT;
			passwordA3.CredentialRecipientExpose = CredentialRecipient.DirectxT;

			passwordA2.Delete();
			passwordA3.Delete();

			Factory.Save();

			interchange = Factory.GetLatestDxTConfigurationInterchange(EDIInterchangeTypeList.Codes.CustomsWare);
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", "CMP", "GG", "XXX", 0);

			ClearInterchange(interchange);

			AssertSendConfigurationOrder(glbcompany, glbgroup, glbstaff);
		}

		public void TestActionOnSuccessfulSendingInvoked()
		{
			var glbcompany = Factory.NewWithValidTestData<GlbCompany>();
			glbcompany.GC_Code = "CMP";
			var glbgroup = Factory.NewWithValidTestData<GlbGroup>();
			glbgroup.GG_Code = "GG";
			var glbstaff = Factory.NewWithValidTestData<GlbStaff>();
			glbstaff.GS_Code = "XXX";

			var passwordA1 = Factory.New<GlbExternalPasswordForATest>();
			passwordA1.GP_PasswordStatus = "VAL";
			passwordA1.GP_PasswordType = "A1";
			passwordA1.GP_UserID = "A1";
			passwordA1.GP_GC = glbcompany.PK;
			passwordA1.GP_GG = glbgroup.PK;
			passwordA1.GP_GS = glbstaff.PK;

			var invocationsCount = 0;
			Action action = new Action(() =>
			{
				invocationsCount++;
			});

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "", glbcompany, glbgroup, glbstaff);
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNull("PREREQ: interchange is not created", interchange);
			AssertEquals("No Action should be invoked", 0, invocationsCount);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST1", glbcompany, glbgroup, glbstaff, actionOnSuccessfulSending: action);
			Factory.Save();
			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull("PREREQ: interchange must be created", interchange);
			AssertConfiguration(interchange, "TEST1", "CMP", "GG", "XXX", 1);

			AssertEquals("Action should be invoked", 1, invocationsCount);
		}

		void AssertSendConfigurationOrder(GlbCompany glbcompany, GlbGroup glbgroup, GlbStaff glbstaff)
		{
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST1", glbcompany, glbgroup, glbstaff);
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST2", glbcompany, glbgroup, glbstaff);
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST3", glbcompany, glbgroup, glbstaff);
			AssertEquals(1, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);
			AssertEquals(3, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess[Factory].ConfigurationsToBeSend.Count);
			var configurationsToBeSend = ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess[Factory].ConfigurationsToBeSend;
			Assert(configurationsToBeSend.Count(x => x.ConfigurationName == "TEST1" && x.CompanyCode == "CMP" && x.GroupCode == "GG" && x.StaffCode == "XXX") == 1);
			Assert(configurationsToBeSend.Count(x => x.ConfigurationName == "TEST2" && x.CompanyCode == "CMP" && x.GroupCode == "GG" && x.StaffCode == "XXX") == 1);
			Assert(configurationsToBeSend.Count(x => x.ConfigurationName == "TEST3" && x.CompanyCode == "CMP" && x.GroupCode == "GG" && x.StaffCode == "XXX") == 1);

			Factory.Save();
			AssertEquals(0, ExternalPasswordConfigurationToSenderForTest.SenderForTestingAccess.Count);

			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST3", "CMP", "GG", "XXX", 1);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST2", "CMP", "GG", "XXX", 1);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", "CMP", "GG", "XXX", 0);

			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST3", glbcompany, glbgroup, glbstaff);
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST2", glbcompany, glbgroup, glbstaff);
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, "TEST1", glbcompany, glbgroup, glbstaff);
			Factory.Save();

			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", "CMP", "GG", "XXX", 0);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST2", "CMP", "GG", "XXX", 1);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST3", "CMP", "GG", "XXX", 1);

			ClearInterchange(interchange);
		}

		void ClearInterchange(IEDIInterchange interchange)
		{
			interchange.Delete();
			Factory.Save();
		}

		static void AssertConfiguration(IEDIInterchange interchange, ZString configurationName, ZString companyCode, ZString groupCode, ZString staffCode, int passwordLength = 0)
		{
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var configuration = reader.DeserializeToConfiguration();
				AssertEquals("Configuration name should be set", configurationName, configuration.Name);

				var systemGroup = configuration.Group[0];
				var group = AssertConfigurationGroup(companyCode, groupCode, staffCode, systemGroup);

				AssertEquals("There should be password", passwordLength, group.Items?.Length ?? 0);
			}
		}

		static Group AssertConfigurationGroup(ZString companyCode, ZString groupCode, ZString staffCode, Group systemGroup)
		{
			var group = systemGroup;

			var groupList = new List<ZString>();
			if (!companyCode.IsEmpty)
			{
				groupList.Add("Company");
			}
			if (!groupCode.IsEmpty)
			{
				groupList.Add("Group");
			}
			if (!staffCode.IsEmpty)
			{
				groupList.Add("Staff");
			}

			AssertEquals("Group type should be set", "System", systemGroup.Type);
			AssertEquals("There should be one subgroup", groupList.Count > 0 ? 1 : 0, systemGroup.Items?.Length ?? 0);

			foreach (var groupName in groupList)
			{
				group = (Group)group.Items[0];
				AssertEquals("Group type should be set", groupName, group.Type);
				switch (groupName)
				{
					case "Company":
						AssertEquals(groupName, companyCode, group.Reference);
						break;
					case "Group":
						AssertEquals(groupName, groupCode, group.Reference);
						break;
					case "Staff":
						AssertEquals(groupName, staffCode, group.Reference);
						break;
				}
			}

			return group;
		}

		class ExternalPasswordConfigurationToSenderForTest : ExternalPasswordConfigurationToSender
		{
			protected ExternalPasswordConfigurationToSenderForTest(ZString interchangeType)
				: base(interchangeType)
			{
			}

			internal static Dictionary<BusinessObjectFactory, ExternalPasswordConfigurationToSender> SenderForTestingAccess => Senders;
		}
	}
}
