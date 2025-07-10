using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	internal abstract class ScheduleChangeEmailSenderTestBase : TestCaseWithFactory
	{
		#region Implementation

		protected abstract string TransportMode { get; }
		protected abstract GuidRegistryItem NotificationGroupRegistryItem { get; }

		protected void ResetEmailTemplate()
		{
			if (emailTemplateSwitcher != null)
			{
				emailTemplateSwitcher.Dispose();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalNotificationGroup = NotificationGroupRegistryItem.Value;
			NotificationGroupRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.PK.ToGuid());

			emailTemplateSwitcher = new ScheduleChangeEmailTextFormatTemplateSwitcher();
			FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);

			originalBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			originalUserFullName = GlbStaff.CurrentUser.GS_FullName;
		}

		protected override void TearDown()
		{
			base.TearDown();
			ResetEmailTemplate();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalBranchHomePort;
			NotificationGroupRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalNotificationGroup);
			GlbStaff.CurrentUser.GS_FullName = originalUserFullName;
		}

		protected void CreateNewScheduleAndMakeChanges(bool withCarrier)
		{
			CreateNewScheduleAndMakeChanges(withCarrier, "");
		}

		protected void CreateNewScheduleAndMakeChanges(bool withCarrier, string dataProvider)
		{
			if (!withCarrier)
			{
				Voyage1.JV_OH_Line = ZGuid.Empty;
				Voyage2.JV_OH_Line = ZGuid.Empty;
			}

			Assert("Test requires a [TestDate] attribute", TestDateAttribute.IsActive);
			TestDateAttribute.Date = new DateTime(2013, 1, 1);

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);

			JobSailing voyage1PortPair1 = Voyage1.Sailings[0];
			JobSailing voyage1PortPair2 = Voyage1.Sailings[1];
			JobSailing voyage1PortPair3 = Voyage1.Sailings[2];
			Factory.Save();

			JobSailing voyage2PortPair = NewJobSailing(
				Voyage2, "MYPKG", "AUSYD",
				new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2), new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
			AssertEmailNotSent("No email sent until changes are made to the schedule");

			voyage1PortPair1.Origin.JA_E_DEP = new ZDateTime(2018, 1, 11);
			voyage1PortPair1.Origin.JA_A_DEP = new ZDateTime(2018, 1, 12);
			voyage1PortPair1.Destination.JB_E_ARV = new ZDateTime(2018, 1, 13);
			voyage1PortPair1.Destination.JB_A_ARV = new ZDateTime(2018, 1, 14);
			voyage1PortPair1.Destination.JB_StorageDate = new ZDateTime(2018, 1, 21);
			voyage1PortPair1.Destination.JB_AvailabilityDate = new ZDateTime(2018, 1, 22);
			voyage1PortPair1.Origin.JA_CutOff = new ZDateTime(2018, 1, 23);
			voyage1PortPair1.Origin.JA_ReceivalCommences = new ZDateTime(2018, 1, 24);

			voyage1PortPair3.Origin.JA_E_DEP = new ZDateTime(2018, 1, 11);
			voyage1PortPair3.Origin.JA_A_DEP = new ZDateTime(2018, 1, 12);
			voyage1PortPair3.Destination.JB_E_ARV = new ZDateTime(2018, 1, 13);
			voyage1PortPair3.Destination.JB_A_ARV = new ZDateTime(2018, 1, 14);
			voyage1PortPair3.Destination.JB_StorageDate = new ZDateTime(2018, 1, 21);
			voyage1PortPair3.Destination.JB_AvailabilityDate = new ZDateTime(2018, 1, 22);
			voyage1PortPair3.Origin.JA_CutOff = new ZDateTime(2018, 1, 23);
			voyage1PortPair3.Origin.JA_ReceivalCommences = new ZDateTime(2018, 1, 24);

			if (dataProvider.Length > 0)
			{
				var origin1 = voyage1PortPair1.Origin as IScheduleChangeEmailSupporter;
				if (origin1 != null)
				{
					origin1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ETD, dataProvider);
					origin1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ATD, dataProvider);
					origin1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLCutOff, dataProvider);
					origin1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLReceivalCommences, dataProvider);
				}

				var origin2 = voyage1PortPair3.Origin as IScheduleChangeEmailSupporter;
				if (origin2 != null)
				{
					origin2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ETD, dataProvider);
					origin2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ATD, dataProvider);
					origin2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLCutOff, dataProvider);
					origin2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLReceivalCommences, dataProvider);
				}

				var destination1 = voyage1PortPair1.Destination as IScheduleChangeEmailSupporter;
				if (destination1 != null)
				{
					destination1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ETA, dataProvider);
					destination1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ATA, dataProvider);
					destination1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLStorage, dataProvider);
					destination1.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLAvailable, dataProvider);
				}

				var destination2 = voyage1PortPair3.Destination as IScheduleChangeEmailSupporter;
				if (destination2 != null)
				{
					destination2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ETA, dataProvider);
					destination2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ATA, dataProvider);
					destination2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLStorage, dataProvider);
					destination2.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.FCLAvailable, dataProvider);
				}

				voyage2PortPair.Destination.JB_E_ARV = new ZDateTime(2018, 1, 20);
				var destination3 = voyage2PortPair.Destination as IScheduleChangeEmailSupporter;
				if (destination3 != null)
				{
					destination3.AddOrUpdateJobScheduleChange(ScheduleDateTypes.Codes.ETA, dataProvider);
				}

				using (CurrentUserChanger.SwitchToNewUserTemporarily(BatchProcessorUser.GS_LoginName))
				{
					Factory.Save();
				}
			}

			Factory.Save();

			voyage2PortPair.Origin.JA_E_DEP = new ZDateTime(2018, 1, 11);
			using (CurrentUserChanger.SwitchToNewUserTemporarily(BatchProcessorUser.GS_LoginName))
			{
				Factory.Save();
			}

			TestDateAttribute.Date = new DateTime(2013, 2, 1);

			Factory.Save();
		}

		protected void AssertEmailNotSent(ZString message)
		{
			AssertEquals(message, 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected void AssertEmailSent(ZString message, ZString expectedSubject, ZString expectedBody)
		{
			AssertEquals(message, 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Expected to send an email in HTML format", EmailContentTypes.HTML, Env.OutgoingMailManager.EmailsCreated[0].ContentType);
			AssertEquals("Expected to send to the correct recipient", NotificationGroup.Staff[0].GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);

			AssertEquals("Expected 3 attachments", 3, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			AssertEquals("Expected attachment with name Banner.jpg that will be referenced by the html", "Banner.jpg", Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].DisplayName);
			AssertEquals("Expected attachment with name Footer.jpg that will be referenced by the html", "Footer.jpg", Env.OutgoingMailManager.EmailsCreated[0].Attachments[1].DisplayName);
			AssertEquals("Expected attachment with name HowToUnsubscribe.txt that will be referenced by the html", "HowToUnsubscribe.txt", Env.OutgoingMailManager.EmailsCreated[0].Attachments[2].DisplayName);

			AssertMultilineASCIIEquals(message + "; subject", expectedSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertMultilineASCIIEquals(message + "; body", expectedBody, Env.OutgoingMailManager.EmailsCreated[0].Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		protected OrgHeader ShippingLine
		{
			get { return shippingLine ?? (shippingLine = new VoyageTestHelper(Factory).CreateCarrier("Carrier")); }
		}
		OrgHeader shippingLine;

		protected JobVoyage NewJobVoyage(ZString transportMode, ZString vesselCode, ZString voyage, ZGuid carrierPK)
		{
			RefVessel vessel = RefVessel.LookupVesselByCode(vesselCode, Factory);
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = vesselCode;
			}

			JobVoyage result = Factory.New<JobVoyage>();
			result.JV_AirSeaRoad = transportMode;
			result.JV_RV_NKVessel = vesselCode;
			result.JV_VoyageFlight = voyage;
			result.JV_OH_Line = carrierPK;

			return result;
		}

		protected JobSailing NewJobSailing(JobVoyage voyage, ZString loadPort, ZString dischargePort, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			VoyageOrigin origin = NewVoyageOrigin(voyage, loadPort, eTD, aTD);
			VoyageDestination destination = NewVoyageDestination(voyage, dischargePort, eTA, aTA);
			return voyage.Sailings[0];
		}

		protected ScheduleChangeEmailSender ChangeEmailSender
		{
			get
			{
				if (fChangeEmailSender == null)
				{
					fChangeEmailSender = ScheduleChangeEmailSender.New(TransportMode);
				}
				return fChangeEmailSender;
			}
		}
		ScheduleChangeEmailSender fChangeEmailSender;

		protected NotificationBuffer Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = new NotificationBuffer();
				}
				return notifications;
			}
		}
		NotificationBuffer notifications;

		protected JobVoyage Voyage1
		{
			get
			{
				if (voyage1 == null)
				{
					voyage1 = NewJobVoyage(TransportMode, "Vessel1", "Voyage1", ShippingLine.PK);

					NewVoyageOrigin(voyage1, "MYPKG", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2));
					NewVoyageDestination(voyage1, "AUSYD", new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
					NewVoyageOrigin(voyage1, "AUSYD", new ZDateTime(2013, 1, 11), new ZDateTime(2013, 1, 12));
					NewVoyageDestination(voyage1, "AUMEL", new ZDateTime(2013, 1, 13), new ZDateTime(2013, 1, 14));
				}
				return voyage1;
			}
		}
		JobVoyage voyage1;

		protected JobVoyage Voyage2
		{
			get
			{
				if (voyage2 == null)
				{
					voyage2 = NewJobVoyage(TransportMode, "Vessel2", "Voyage2", ShippingLine.PK);
				}
				return voyage2;
			}
		}
		JobVoyage voyage2;

		VoyageOrigin NewVoyageOrigin(JobVoyage voyage, ZString loadPort, ZDateTime eTD, ZDateTime aTD)
		{
			VoyageOrigin result = voyage.Origins.AddNew();
			result.JA_RL_NKPortOfLoading = loadPort;
			result.JA_E_DEP = eTD;
			result.JA_A_DEP = aTD;
			return result;
		}

		VoyageDestination NewVoyageDestination(JobVoyage voyage, ZString dischargePort, ZDateTime eTA, ZDateTime aTA)
		{
			VoyageDestination result = voyage.Destinations.AddNew();
			result.JB_RL_NKPortOfDischarge = dischargePort;
			result.JB_E_ARV = eTA;
			result.JB_A_ARV = aTA;
			return result;
		}

		Guid originalNotificationGroup;
		ZString originalBranchHomePort;
		ScheduleChangeEmailTextFormatTemplateSwitcher emailTemplateSwitcher;
		ZString originalUserFullName;
		GlbStaff BatchProcessorUser
		{
			get { return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, User.ServiceUserCode)); }
		}
		GlbGroup NotificationGroup
		{
			get
			{
				if (notificationGroup == null)
				{
					notificationGroup = Factory.New<GlbGroup>();
					GlbStaff emailRecipient = notificationGroup.Staff.AddNew();

					emailRecipient.GS_EmailAddress = "clinton@edi.com.au";
					emailRecipient.GS_Code = "ZAC";
					Factory.Save();
				}
				return notificationGroup;
			}
		}
		GlbGroup notificationGroup;

		#endregion
	}
}
