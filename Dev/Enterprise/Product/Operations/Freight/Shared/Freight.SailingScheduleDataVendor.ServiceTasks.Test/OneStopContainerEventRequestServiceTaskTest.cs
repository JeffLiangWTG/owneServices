using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks.Test
{
	[TestedType(typeof(OneStopContainerEventRequestServiceTask))]
	sealed class OneStopContainerEventRequestServiceTaskTest : ServiceTaskTestCase<OneStopContainerEventRequestServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "CTR", hostedServiceAttribute.Code);
				AssertEquals("Description", "ComTrac Container Request Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "FRT", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "5minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestTaskRunsForDifferentCompanies()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "111";
			var orgProxy1 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy1.OH_FullName = "Proxy One Incorporated";
			company1.GC_OH_OrgProxy = orgProxy1.PK;
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "aaa";
			branch1.GB_RL_NKHomePort = "AUSYD";

			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "222";
			var orgProxy2 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy2.OH_FullName = "Proxy Two Incorporated";
			company2.GC_OH_OrgProxy = orgProxy2.PK;
			GlbBranch branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "bbb";
			branch2.GB_RL_NKHomePort = "AUBNE";

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
				FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Helper.Container.JC_ContainerNum = "AABB11";
				Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;

				Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
				Helper.Consol.JK_RL_NKDischargePort = "NZAKL";

				OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
				vendor.NotifyContainerCreated(Helper.Container);
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
				FreightDataRegistry.Instance.OneStopContainerEventsEnabled.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				FreightDataRegistry.Instance.OneStopContainerEventsEnabledNZ.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				string containerNum = "AABB12";

				Helper.Container.JC_ContainerNum = containerNum;
				Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;

				Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
				Helper.Consol.JK_RL_NKDischargePort = "NZAKL";

				OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
				vendor.NotifyContainerCreated(Helper.Container);
				Factory.Save();
			}

			OneStopContainerEventRequestServiceTask task = new OneStopContainerEventRequestServiceTask();
			InitialiseAndRunTaskSchedule(task);

			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			Func<string, string[]> split = x => x.Split(System.Environment.NewLine.ToCharArray());

			bool email1validated = false;
			bool email2validated = false;

			foreach (var email in Env.OutgoingMailManager.EmailsCreated)
			{
				AssertEquals("Recipient", "alerts@edi.1-stop.biz", email.Recipients[0]);
				Assert(email.Attachments[0].DisplayName.StartsWith("ALERT_20"));
				Assert(email.Attachments[0].DisplayName.EndsWith(".csv"));

				string expectedSubject1 = string.Format("{0}(EDI License='EDI111DAT' ABN='' Name='{1}')", ServiceTaskConstants.ContainerEventSubscriptionEmailSubjectPrefix, orgProxy1.OH_FullName);
				string expectedSubject2 = string.Format("{0}(EDI License='EDI222DAT' ABN='' Name='{1}')", ServiceTaskConstants.ContainerEventSubscriptionEmailSubjectPrefix, orgProxy2.OH_FullName);

				if (email.Subject == expectedSubject1)
				{
					string expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
").Replace("'", "\"");
					AssertContainsExactElementsInAnyOrder(split(expected), split(Encoding.UTF8.GetString(email.Attachments[0].Data)));
					email1validated = true;
				}
				else if (email.Subject == expectedSubject2)
				{
					string expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB12','" + containerPK + @"','AU'
").Replace("'", "\"");
					AssertContainsExactElementsInAnyOrder(split(expected), split(Encoding.UTF8.GetString(email.Attachments[0].Data)));
					email2validated = true;
				}
			}

			if (!email1validated || !email2validated)
			{
				Fail("Expected subject has not been found.");
			}
		}

		public void TestEmailsAreNotSendIfNotFactoryEnabled()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;

			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);
			string expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
").Replace("'", "\"");

			AssertEmailCreationForSpecificPorts("AUSYD", "NZAKL", expected, 11, true, false);
			AssertEmailCreationForSpecificPorts("AUSYD", "NZAKL", string.Empty, 12, false, false);

			expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB12','" + containerPK + @"','NZ'
").Replace("'", "\"");

			AssertEmailCreationForSpecificPorts("AUSYD", "NZAKL", expected, 12, false, true);
		}

		public void TestMismatchEmailContainsSubscriptionsAttached()
		{
			var oneStopNotificationGroup = Factory.New<GlbGroup>();
			oneStopNotificationGroup.GG_Code = "1SN";

			GlbStaff recipient = oneStopNotificationGroup.Staff.AddNew();
			recipient.GS_EmailAddress = "1stop_notifications@edi.com.au";
			recipient.GS_Code = "ZAC";

			Factory.Save();
			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oneStopNotificationGroup.PK.ToGuid());

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);

			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			string containerNum = "AABB" + 11;

			Helper.Container.JC_ContainerNum = containerNum;
			Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;

			Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
			Helper.Consol.JK_RL_NKDischargePort = "NZAKL";

			OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
			vendor.NotifyContainerCreated(Helper.Container);
			Factory.Save();

			OneStopContainerEventRequestServiceTask task = new OneStopContainerEventRequestServiceTask();
			InitialiseAndRunTaskSchedule(task);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			ZString eventType = Enterprise.Freight.SailingDataVendor.Business.OneStopConstants.ContainerEventTypes.ExportPreAdvised;
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*" + (ZString)"XXXXX" + "*GATU0859042*" + eventType + "*" + Helper.Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613*ETA 2006/09/15 07:00 AM";
			email.Body = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: " + (ZString)"XXXXX" + @"
Vessel/Container 	: GATU0859042
Event Type 		: " + eventType + @"
Event Date 		: 2006/09/06
Event Time 		: 10:31 AM
Information 	: " + Helper.Container.PK.ToString().Replace("-", "") + @"
Vessel Name 	: ARIAKE
Lloyds No 		: 9294159
Voyage Number 	: 0613
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE
";
			Env.OutgoingMailManager.Create(Factory, email);

			ZQuery query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, email.FromAddress);
			query.AddToFilter(MailDBItemsSchema.MI_Subject, email.Subject);
			MailItem mailItem = Factory.LoadTop1<MailItem>(query);
			mailItem.MI_Direction = MailDirection.Receive;

			Helper.Container.JC_ContainerNum = "GATU0859042";
			Helper.Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Helper.Consol.Transports.MostInterestingTransport.JW_Vessel = "NYK PROVIDER";
			Helper.Consol.Transports.MostInterestingTransport.JW_VoyageFlight = "1234";

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			new OneStopContainerInformationProcessor().Process(new NotificationBuffer());
			AssertEquals("Email should contain 3 attachments including subscription info.", 3, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);

			bool subscriptionAsserted = false;

			foreach (AttachmentDef attachment in Env.OutgoingMailManager.EmailsCreated[0].Attachments)
			{
				if (attachment.DisplayName.StartsWith("Subscription from"))
				{
					subscriptionAsserted = true;
					string expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
").Replace("'", "\"");

					AssertEquals(expected, Encoding.UTF8.GetString(attachment.Data));
				}
			}

			Assert(subscriptionAsserted);
		}

		public void TestEmailSentToSubscribedRecipient_DifferentVessel()
		{
			GlbGroup oneStopNotificationGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			oneStopNotificationGroup1.GG_Code = "1SN1";
			GlbStaff recipient1 = oneStopNotificationGroup1.Staff.AddNew();
			recipient1.GS_EmailAddress = "1stop_notifications1@edi.com.au";
			recipient1.GS_Code = "ZAC";
			recipient1.GS_LoginName = "recipient1";
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany company1 = Factory.New<GlbCompany>();
			branch1.GB_GC = company1.PK;

			GlbGroup oneStopNotificationGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			oneStopNotificationGroup2.GG_Code = "1SN2";
			GlbStaff recipient2 = oneStopNotificationGroup2.Staff.AddNew();
			recipient2.GS_EmailAddress = "1stop_notifications2@edi.com.au";
			recipient2.GS_Code = "ZAB";
			recipient2.GS_LoginName = "recipient2";
			GlbCompany company2 = GlbCompany.CurrentCompany;

			Factory.Save();

			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, oneStopNotificationGroup1.PK.ToGuid());
			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, oneStopNotificationGroup2.PK.ToGuid());

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);

			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			string containerNum = "AABB" + 11;

			Helper.Container.JC_ContainerNum = containerNum;
			Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
			Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
			Helper.Consol.JK_RL_NKDischargePort = "NZAKL";

			OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
			vendor.NotifyContainerCreated(Helper.Container);
			Factory.Save();

			OneStopContainerEventRequestServiceTask task = new OneStopContainerEventRequestServiceTask();
			InitialiseAndRunTaskSchedule(task);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			ZString eventType = Enterprise.Freight.SailingDataVendor.Business.OneStopConstants.ContainerEventTypes.ExportPreAdvised;
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*" + (ZString)"XXXXX" + "*GATU0859042*" + eventType + "*" + Helper.Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613*ETA 2006/09/15 07:00 AM";
			email.Body = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: " + (ZString)"XXXXX" + @"
Vessel/Container 	: GATU0859042
Event Type 		: " + eventType + @"
Event Date 		: 2006/09/06
Event Time 		: 10:31 AM
Information 	: " + Helper.Container.PK.ToString().Replace("-", "") + @"
Vessel Name 	: ARIAKE
Lloyds No 		: 9294159
Voyage Number 	: 0613
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE
";
			Env.OutgoingMailManager.Create(Factory, email);

			ZQuery query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, email.FromAddress);
			query.AddToFilter(MailDBItemsSchema.MI_Subject, email.Subject);
			MailItem mailItem = Factory.LoadTop1<MailItem>(query);
			mailItem.MI_Direction = MailDirection.Receive;

			Helper.Container.JC_ContainerNum = "GATU0859042";
			Helper.Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Helper.Consol.Transports.MostInterestingTransport.JW_Vessel = "NYK PROVIDER";

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Env.SetTemporaryUserContext(recipient1.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert(GlbCompany.CurrentCompany.PK != company2.PK);
				new OneStopContainerInformationProcessor().Process(new NotificationBuffer());
			}

			AssertEquals("Different Vessel email should contain 3 attachments including subscription info.", 3, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			AssertEquals("Different Vessel email should be sent to recipient2 that belongs to company2.", recipient2.GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("Different Vessel email should have an appropriate subject line.", "1-Stop Vessel Difference", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestEmailSentToSubscribedRecipient_OneStopCodeNotRegistered()
		{
			GlbGroup oneStopNotificationGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			oneStopNotificationGroup1.GG_Code = "1SN1";
			GlbStaff recipient1 = oneStopNotificationGroup1.Staff.AddNew();
			recipient1.GS_EmailAddress = "1stop_notifications1@edi.com.au";
			recipient1.GS_Code = "ZAC";
			recipient1.GS_LoginName = "recipient1";
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany company1 = Factory.New<GlbCompany>();
			branch1.GB_GC = company1.PK;

			GlbGroup oneStopNotificationGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			oneStopNotificationGroup2.GG_Code = "1SN2";
			GlbStaff recipient2 = oneStopNotificationGroup2.Staff.AddNew();
			recipient2.GS_EmailAddress = "1stop_notifications2@edi.com.au";
			recipient2.GS_Code = "ZAB";
			recipient2.GS_LoginName = "recipient2";
			GlbCompany company2 = GlbCompany.CurrentCompany;

			Factory.Save();

			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, oneStopNotificationGroup1.PK.ToGuid());
			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, oneStopNotificationGroup2.PK.ToGuid());

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);

			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			string containerNum = "AABB" + 11;

			Helper.Container.JC_ContainerNum = containerNum;
			Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
			Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
			Helper.Consol.JK_RL_NKDischargePort = "NZAKL";

			OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
			vendor.NotifyContainerCreated(Helper.Container);
			Factory.Save();

			OneStopContainerEventRequestServiceTask task = new OneStopContainerEventRequestServiceTask();
			InitialiseAndRunTaskSchedule(task);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			ZString eventType = Enterprise.Freight.SailingDataVendor.Business.OneStopConstants.ContainerEventTypes.GateIn;
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*" + (ZString)"XXXXX" + "*GATU0859042*" + eventType + "*" + Helper.Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613*ETA 2006/09/15 07:00 AM";
			email.Body = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: " + (ZString)"XXXXX" + @"
Vessel/Container 	: GATU0859042
Event Type 		: " + eventType + @"
Event Date 		: 2006/09/06
Event Time 		: 10:31 AM
Information 	: " + Helper.Container.PK.ToString().Replace("-", "") + @"
Vessel Name 	: 
Lloyds No 		: 
Voyage Number 	: 
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE
";
			Env.OutgoingMailManager.Create(Factory, email);

			ZQuery query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, email.FromAddress);
			query.AddToFilter(MailDBItemsSchema.MI_Subject, email.Subject);
			MailItem mailItem = Factory.LoadTop1<MailItem>(query);
			mailItem.MI_Direction = MailDirection.Receive;

			Helper.Container.JC_ContainerNum = "GATU0859042";
			Helper.Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Env.SetTemporaryUserContext(recipient1.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert(GlbCompany.CurrentCompany.PK != company2.PK);
				new OneStopContainerInformationProcessor().Process(new NotificationBuffer());
			}

			AssertEquals("Unregistered 1-Stop Location Notification email should contain 3 attachments including subscription info.", 3, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			AssertEquals("Unregistered 1-Stop Location Notification email should be sent to recipient2 that belongs to company2.", recipient2.GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("Unregistered 1-Stop Location Notification email should have an appropriate subject line.", string.Format("{0} Unregistered 1-Stop Location Notification", eventType), Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestEmailSentToSubscribedRecipient_OneStopContainerEventAlertServiceConfigurationProblem()
		{
			GlbGroup oneStopNotificationGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			oneStopNotificationGroup1.GG_Code = "1SN1";
			GlbStaff recipient1 = oneStopNotificationGroup1.Staff.AddNew();
			recipient1.GS_EmailAddress = "1stop_notifications1@edi.com.au";
			recipient1.GS_Code = "ZAC";
			recipient1.GS_LoginName = "recipient1";
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany company1 = Factory.New<GlbCompany>();
			branch1.GB_GC = company1.PK;

			GlbGroup oneStopNotificationGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			oneStopNotificationGroup2.GG_Code = "1SN2";
			GlbStaff recipient2 = oneStopNotificationGroup2.Staff.AddNew();
			recipient2.GS_EmailAddress = "1stop_notifications2@edi.com.au";
			recipient2.GS_Code = "ZAB";
			recipient2.GS_LoginName = "recipient2";
			GlbCompany company2 = GlbCompany.CurrentCompany;

			Factory.Save();

			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, oneStopNotificationGroup1.PK.ToGuid());
			FreightDataRegistry.Instance.OneStopNotificationGroup.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, oneStopNotificationGroup2.PK.ToGuid());

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);

			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			string containerNum = "AABB11";

			Helper.Container.JC_ContainerNum = containerNum;
			Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
			Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
			Helper.Consol.JK_RL_NKDischargePort = "NZAKL";

			OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
			vendor.NotifyContainerCreated(Helper.Container);
			Factory.Save();

			OneStopContainerEventRequestServiceTask task = new OneStopContainerEventRequestServiceTask();
			InitialiseAndRunTaskSchedule(task);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			ZString eventType = Enterprise.Freight.SailingDataVendor.Business.OneStopConstants.ContainerEventTypes.ExportPreAdvised;
			EmailDef email = new EmailDef();
			email.FromAddress = "helpdesk@1-stop.biz";
			email.AddRecipientForUserCommunication("clinton@edi.com.au");
			email.Subject = "1-STOP NOTIFY*376530*" + (ZString)"XXXXX" + "*GATU0859042*" + eventType + "*" + Helper.Container.PK.ToString().Replace("-", "") + "*2006/09/06 10:31 AM*ARIAKE*9294159*0613*ETA 2006/09/15 07:00 AM";
			email.Body = @"
Type 			: 1-STOP NOTIFY 
Message ID 		: 376530
Event Location 	: " + (ZString)"XXXXXXXXXXXXX" + @"
Vessel/Container 	: AABB11
Event Type 		: " + eventType + @"
Event Date 		: 2012/06/02
Event Time 		: 10:31 AM
Information 	: " + Helper.Container.PK.ToString().Replace("-", "") + @"
Vessel Name 	: 
Lloyds No 		: 
Voyage Number 	: 
ETD from Load Port: 2006/09/10 18:52 PM from NZAKL
ETA at Discharge Port: 2006/09/15 07:00 AM at AUBNE
";
			Env.OutgoingMailManager.Create(Factory, email);

			ZQuery query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, email.FromAddress);
			query.AddToFilter(MailDBItemsSchema.MI_Subject, email.Subject);
			MailItem mailItem = Factory.LoadTop1<MailItem>(query);
			mailItem.MI_Direction = MailDirection.Receive;

			Helper.Container.JC_ContainerNum = containerNum;
			Helper.Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Env.SetTemporaryUserContext(recipient1.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert(GlbCompany.CurrentCompany.PK != company2.PK);
				new OneStopContainerInformationProcessor().Process(new NotificationBuffer());
			}

			AssertEquals("1-Stop Container Event Alert Service configuration problem email should contain 4 attachments including subscription info.", 3, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			AssertEquals("1-Stop Container Event Alert Service configuration problem email should be sent to recipient2 that belongs to company2.", recipient2.GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("1-Stop Container Event Alert Service configuration problem email should have an appropriate subject line.", "1-Stop Container Event Alert Service configuration problem", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestCreateEmail()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);

			string expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB11','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB11','" + containerPK + @"','NZ'
").Replace("'", "\"");

			AssertEmailCreationForSpecificPorts("AUSYD", "NZAKL", expected, 11);

			expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB12','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB12','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB12','" + containerPK + @"','AU'
").Replace("'", "\"");

			AssertEmailCreationForSpecificPorts("NZAKL", "AUSYD", expected, 12);

			expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB13','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB13','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB13','" + containerPK + @"','NZ'
").Replace("'", "\"");

			AssertEmailCreationForSpecificPorts("NZAKL", "USLAX", expected, 13);

			expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','AABB14','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','AABB14','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','AABB14','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','AABB14','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','AABB14','" + containerPK + @"','AU'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','AABB14','" + containerPK + @"','AU'
").Replace("'", "\"");

			AssertEmailCreationForSpecificPorts("USLAX", "AUSYD", expected, 14);

			expected = @"";

			AssertEmailCreationForSpecificPorts("USNYC", "NLAMS", expected, 15);
		}

		public void TestNullOrgProxyDoesntThrowException()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			string containerPK = OneStopContainerEventRequest.GetPKString(Helper.Container);

			string expected = (@"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','AABB13','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','AABB13','" + containerPK + @"','NZ'
'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','AABB13','" + containerPK + @"','NZ'
").Replace("'", "\"");

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			AssertEmailCreationForSpecificPorts("NZAKL", "USLAX", expected, 13);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
						new TaskNudgeInformationForTest(
							EDIMessageSchema.Constants.TableName,
							"ComTrac",
							EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
							EDIMessageSchema.Constants.EM_IsActive + "=Y",
							EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
							EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ComTrac),
				};
			}
		}

		void AssertEmailCreationForSpecificPorts(string loadPort, string dischargePort, string expected, int containerNumFountain, bool hasOneStopAU = true, bool hasOneStopNZ = true)
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			string containerNum = "AABB" + containerNumFountain;

			Helper.Container.JC_ContainerNum = containerNum;
			Helper.Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;

			Helper.Consol.JK_RL_NKLoadPort = loadPort;
			Helper.Consol.JK_RL_NKDischargePort = dischargePort;

			OneStopContainerEventDataVendor vendor = new OneStopContainerEventDataVendor();
			vendor.NotifyContainerCreated(Helper.Container);
			Factory.Save();

			FreightDataRegistry.HasOneStopAU = hasOneStopAU;
			FreightDataRegistry.HasOneStopNZ = hasOneStopNZ;

			OneStopContainerEventRequestServiceTask task = new OneStopContainerEventRequestServiceTask();
			InitialiseTaskSchedule(task, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(task);

			if (string.IsNullOrEmpty(expected))
			{
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				return;
			}

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Recipient", "alerts@edi.1-stop.biz", email.Recipients[0]);
			Assert(email.Attachments[0].DisplayName.StartsWith("ALERT_20"));
			Assert(email.Attachments[0].DisplayName.EndsWith(".csv"));
			Func<string, string[]> split = x => x.Split(System.Environment.NewLine.ToCharArray());
			AssertContainsExactElementsInAnyOrder(split(expected), split(Encoding.UTF8.GetString(email.Attachments[0].Data)));

			string expectedSubject = GlbCompany.CurrentCompany.OrgProxy != null
																? $"{ServiceTaskConstants.ContainerEventSubscriptionEmailSubjectPrefix}(EDI License='{GlbCompany.CurrentCompany.LicenceKeyIdentifier}' ABN='{GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number}' Name='{GlbCompany.CurrentCompany.OrgProxy.OH_FullName}')"
																: string.Empty;

			AssertEquals("Subject", expectedSubject, email.Subject);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;
	}
}
