using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class HourlyFreightNotificationEmailSenderTest : TestCaseWithFactory
	{
		[TestDate(2010, 8, 20)]
		public void TestSaveConcurrency()
		{
			var sailing1 = GetSailing("USLAX", "AUSYD");
			var sailing2 = GetSailing("AUSYD", "AUBNE");
			var sailing3 = GetSailing("AUBNE", "AUADL");
			var sailing4 = GetSailing("AUADL", "NZAKL");
			Factory.Save();

			sailing1.Origin.JA_E_DEP = new ZDateTime(2010, 10, 1);
			sailing2.Origin.JA_E_DEP = new ZDateTime(2010, 10, 2);
			sailing3.Origin.JA_E_DEP = new ZDateTime(2010, 10, 3);
			Factory.Save();

			IncrementTime(new TimeSpan(0, 1, 0));

			int conflictCounter = 2;
			var concurrencies = new JobSailing[] { sailing1, sailing2 };
			var emailSender = new HourlyFreightNotificationEmailSender();
			emailSender.PreSaveHookForTesting = delegate(BusinessObjectFactory factory)
			{
				if (conflictCounter > 0)
				{
					var otherFactory = new BusinessObjectFactory();
					var concurreny = otherFactory.Load<JobSailing>(concurrencies[concurrencies.Length - conflictCounter].PK);
					concurreny.Origin.JA_E_DEP = new ZDateTime(2010, 11, 1);
					otherFactory.Save();

					conflictCounter--;
				}
			};

			AssertNoExceptionThrown(delegate
			{ emailSender.SendEmailsIfRequired(Notifications); });
			AssertEquals(0, conflictCounter);

			var newFactory = new BusinessObjectFactory();
			sailing1 = newFactory.Load<JobSailing>(sailing1.PK);
			sailing2 = newFactory.Load<JobSailing>(sailing2.PK);
			sailing3 = newFactory.Load<JobSailing>(sailing3.PK);
			sailing4 = newFactory.Load<JobSailing>(sailing4.PK);

			sailing1.Origin.JA_E_DEP = new ZDateTime(2010, 12, 1);
			sailing2.Origin.JA_E_DEP = new ZDateTime(2010, 12, 2);
			sailing3.Origin.JA_E_DEP = new ZDateTime(2010, 12, 3);
			sailing4.Origin.JA_E_DEP = new ZDateTime(2010, 12, 4);
			newFactory.Save();

			IncrementTime(new TimeSpan(0, 1, 0));

			conflictCounter = 3;
			concurrencies = new JobSailing[] { sailing1, sailing2, sailing3 };
			AssertNoExceptionThrown(delegate
			{ emailSender.SendEmailsIfRequired(Notifications); });
			AssertEquals(0, conflictCounter);
			AssertContains("Concurrency error during sending schedule change email.", Notifications.AsString);
		}

		[TestDate(2005, 1, 2)]
		public void TestSendEmailsIfRequired()
		{
			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("Should set registry item HourlyFreightNotificationEmailsLastSent", ZDateTime.Now, FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value);

			AssertEquals("ScheduleChangeEmailSender (Sea) called", true, EmailSender.SeaScheduleChangeEmailSender.SendEmailIfRequiredCalled);
			AssertEquals("ScheduleChangeEmailSender (Air) called", true, EmailSender.AirScheduleChangeEmailSender.SendEmailIfRequiredCalled);
			AssertEquals("ScheduleChangeEmailSender (Road) called", true, EmailSender.RoadScheduleChangeEmailSender.SendEmailIfRequiredCalled);
			AssertEquals("ScheduleChangeEmailSender (Rail) called", true, EmailSender.RailScheduleChangeEmailSender.SendEmailIfRequiredCalled);
		}

		[TestDate(2001, 1, 1)]
		public void TestSendEmailsIfRequired_ForScheduleChangeNotifications()
		{
			Sailing.Origin.JA_E_DEP = new ZDateTime(2005, 1, 1);
			Factory.Save();

			Sailing.Origin.JA_E_DEP = new ZDateTime(2005, 2, 2);
			Factory.Save();
			IncrementTime(new TimeSpan(0, 1, 0));

			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("First change 'previous value'", true, Env.OutgoingMailManager.EmailsCreated[0].Body.IndexOf("01-Jan-05") != -1);
			AssertEquals("First change 'updated value'", true, Env.OutgoingMailManager.EmailsCreated[0].Body.IndexOf("02-Feb-05") != -1);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var newFactory = new BusinessObjectFactory();
			var loadedSailing = newFactory.Load<JobSailing>(Sailing.PK);
			loadedSailing.Origin.JA_E_DEP = new ZDateTime(2005, 3, 3);
			newFactory.Save();
			IncrementTime(new TimeSpan(0, 1, 0));
			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("Second change 'previous value'", true, Env.OutgoingMailManager.EmailsCreated[0].Body.IndexOf("02-Feb-05") != -1);
			AssertEquals("Second change 'updated value'", true, Env.OutgoingMailManager.EmailsCreated[0].Body.IndexOf("03-Mar-05") != -1);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[TestDate(2000, 1, 1, 1, 1, 1, 450)]
		public void TestSendEmailsIfRequired_ForScheduleChangeNotifications_CorrectFromToDates()
		{
			AssertEquals("Registry HourlyFreightNotificationEmailsLastSent empty initially", ZDateTime.Invalid, FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value);
			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("'From' date corrected to maximum allowed backlog", new ZDateTime(1999, 12, 1, 1, 1, 1), EmailSender.SeaScheduleChangeEmailSender.LastFromDate);
			AssertEquals("To date of first run", new ZDateTime(2000, 1, 1, 1, 1, 1), EmailSender.SeaScheduleChangeEmailSender.LastToDate);
			AssertEquals("Registry HourlyFreightNotificationEmailsLastSent set to 'to' date", new ZDateTime(2000, 1, 1, 1, 1, 1), FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value);

			IncrementTime(new TimeSpan(0, 0, 1));

			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("From date of second run", new ZDateTime(2000, 1, 1, 1, 1, 1), EmailSender.SeaScheduleChangeEmailSender.LastFromDate);
			AssertEquals("To date of second run", new ZDateTime(2000, 1, 1, 1, 1, 2), EmailSender.SeaScheduleChangeEmailSender.LastToDate);
			AssertEquals("Registry HourlyFreightNotificationEmailsLastSent set to 'to' date", new ZDateTime(2000, 1, 1, 1, 1, 2), FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value);

			TestDateAttribute.Date = new DateTime(2012, 03, 01);
			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("'From' date corrected to maximum allowed backlog", new ZDateTime(2012, 02, 01), EmailSender.SeaScheduleChangeEmailSender.LastFromDate);
			AssertEquals("'To' date", new ZDateTime(2012, 03, 01), EmailSender.SeaScheduleChangeEmailSender.LastToDate);
			AssertEquals("Registry set to 'To' date", new ZDateTime(2012, 03, 01), FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value);
		}

		[TestDate]
		public void TestSendEmailsIfRequired_PurgesJobScheduleChangeTable()
		{
			Sailing.Origin.JA_E_DEP = ZDateTime.Empty;
			Factory.Save();
			Sailing.Origin.JA_E_DEP = ZDateTime.Now;
			Factory.Save();

			ZQuery scheduleChangeQuery = new ZQuery(JobScheduleChangeSchema.E7_ParentID, Sailing.Origin.PK);
			AssertEquals("1 JobScheduleChange record before", 1, Factory.GetDatabaseCount(typeof(JobScheduleChange), scheduleChangeQuery));

			IncrementTime(new TimeSpan(0, 1, 0));

			EmailSender.SendEmailsIfRequired(Notifications);
			AssertEquals("0 JobScheduleChange records after purge", 0, Factory.GetDatabaseCount(typeof(JobScheduleChange), scheduleChangeQuery));
		}

		#region Test Classes

		class TestHourlyFreightNotificationEmailSender : HourlyFreightNotificationEmailSender
		{
			public readonly TestScheduleChangeEmailSender SeaScheduleChangeEmailSender = new TestScheduleChangeEmailSender();
			public readonly TestScheduleChangeEmailSender AirScheduleChangeEmailSender = new TestScheduleChangeEmailSender();
			public readonly TestScheduleChangeEmailSender RoadScheduleChangeEmailSender = new TestScheduleChangeEmailSender();
			public readonly TestScheduleChangeEmailSender RailScheduleChangeEmailSender = new TestScheduleChangeEmailSender();

			internal override ScheduleChangeEmailSender GetScheduleChangeEmailSender(string transportMode)
			{
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Sea:
						return SeaScheduleChangeEmailSender;
					case Core.Constants.TransportModes.Air:
						return AirScheduleChangeEmailSender;
					case Core.Constants.TransportModes.Road:
						return RoadScheduleChangeEmailSender;
					case Core.Constants.TransportModes.Rail:
						return RailScheduleChangeEmailSender;
				}
				return null;
			}
		}

		[Core.NonSerializedClass]
		class TestScheduleChangeEmailSender : SeaScheduleChangeEmailSender
		{
			public bool SendEmailIfRequiredCalled;
			public ZDateTime LastFromDate;
			public ZDateTime LastToDate;

			public override JobScheduleChange[] SendEmailIfRequired(BusinessObjectFactory factory, ZDateTime fromDate, ZDateTime toDate, INotifications notifications, int batchSize = 100)
			{
				JobScheduleChange[] result = base.SendEmailIfRequired(factory, fromDate, toDate, notifications);
				SendEmailIfRequiredCalled = true;
				this.LastFromDate = fromDate;
				this.LastToDate = toDate;
				return result;
			}
		}

		#endregion

		#region Implementation

		TestHourlyFreightNotificationEmailSender EmailSender
		{
			get
			{
				if (fEmailSender == null)
				{
					fEmailSender = new TestHourlyFreightNotificationEmailSender();
				}
				return fEmailSender;
			}
		}
		TestHourlyFreightNotificationEmailSender fEmailSender;

		NotificationBuffer Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBuffer();
				}
				return fNotifications;
			}
		}
		NotificationBuffer fNotifications;

		GlbGroup NotificationGroup
		{
			get
			{
				if (fNotificationGroup == null)
				{
					fNotificationGroup = Factory.New<GlbGroup>();
					fNotificationGroup.GG_Code = "TST";
					GlbStaff emailRecipient = fNotificationGroup.Staff.AddNew();

					emailRecipient.GS_EmailAddress = "clinton@edi.com.au";
					emailRecipient.GS_Code = "ZAC";
					Factory.Save();
				}
				return fNotificationGroup;
			}
		}
		GlbGroup fNotificationGroup;

		JobVoyage Voyage
		{
			get
			{
				if (fVoyage == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "VesselName";

					fVoyage = Factory.New<JobVoyage>();
					fVoyage.JV_RV_NKVessel = vessel.RV_FK;
					fVoyage.JV_VoyageFlight = "Voyage";
					fVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
				}
				return fVoyage;
			}
		}
		JobVoyage fVoyage;

		JobSailing Sailing
		{
			get { return sailing ?? (sailing = GetSailing("MYPKG", "AUSYD")); }
		}
		JobSailing sailing;

		JobSailing GetSailing(ZString load, ZString discharge)
		{
			if (Voyage.Origins.GetOriginFromLoading(load) == null)
			{
				Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			}

			if (Voyage.Destinations.GetDestinationFromDischarge(discharge) == null)
			{
				Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			}

			return Voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);
		}

		void IncrementTime(TimeSpan span)
		{
			if (TestDateAttribute.IsActive)
			{
				TestDateAttribute.Date += span;
			}

			if (TestUtcOffsetAttribute.IsActive)
			{
				TestUtcOffsetAttribute.Time += span;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			FreightDataRegistry.Instance.SeaScheduleChangeNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.PK.ToGuid());
		}

		#endregion
	}
}
