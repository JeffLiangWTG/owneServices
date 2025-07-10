using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestsSubclassesOf(typeof(ScheduleChangeEmailSender))]
	internal abstract class ScheduleChangeEmailSenderTest : ScheduleChangeEmailSenderTestBase
	{
		public abstract void TestSendEmailIfRequired();

		public abstract void TestSendEmailIfRequired_NoCarrier();

		public void TestSendEmailIfRequired_NoEmailIfScheduleAdded()
		{
			JobVoyage voyage = NewJobVoyage(TransportMode, "Vessel", "Voyage", ShippingLine.PK);
			JobSailing portPair = NewJobSailing(
				voyage, "AUSYD", "AUMEL",
				new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2), new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
			AssertEmailNotSent("Expected no emails to be sent when a sailing schedule is registered");
		}

		public void TestSendEmailIfRequired_NoEmailIfNoDatesHaveChanged()
		{
			JobVoyage voyage = NewJobVoyage(TransportMode, "Vessel", "Voyage", ShippingLine.PK);
			JobSailing portPair = NewJobSailing(
				voyage, "AUSYD", "AUMEL",
				new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2), new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
			Factory.Save();

			portPair.JX_ContainerReleaseNumber = "X";
			portPair.Origin.JA_Berth = "X";
			portPair.Destination.JB_Berth = "X";
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
			AssertEmailNotSent("Expected no emails to be sent when no dates have changed on a sailing schedule");
		}

		public void TestSendEmailIfRequired_Notifications()
		{
			JobVoyage voyage = NewJobVoyage(TransportMode, "Vessel", "Voyage", ShippingLine.PK);
			JobSailing portPair = NewJobSailing(
				voyage, "AUSYD", "AUMEL",
				new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2), new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
			Factory.Save();

			portPair.Origin.JA_A_DEP = ZDateTime.Now;
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
			AssertEquals("Notifications after sending the email", "Sent " + TransportModeDescription + " schedule change notification email.\r\n", Notifications.AsString);
			Notifications.Clear();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Now.AddMinutes(1), ZDateTime.Now.AddMinutes(1), Notifications);
			AssertEquals("Notifications when there is no email to be sent", "", Notifications.AsString);
		}

		#region Implementation

		protected void TestSendEmailIfRequired(ZString transportModeDescription, ZString expectedVesselVoyageHeader1, ZString expectedVesselVoyageHeader2, bool withCarrier)
		{
			CreateNewScheduleAndMakeChanges(withCarrier);
			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 2, 2), Notifications);

			ZString expectedSubject = transportModeDescription + " Schedules have changed";
			ZString expectedBody =
transportModeDescription + @" Schedule changes (first time run) to 02-Feb-13 00:00 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: CargoWise Service
" + expectedVesselVoyageHeader2 + @"
---
MYPKG               ETD                01-Jan-13 00:00 11-Jan-18 00:00

Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
" + expectedVesselVoyageHeader1 + @"
---
MYPKG               ETD                01-Jan-13 00:00 11-Jan-18 00:00
MYPKG               ATD                02-Jan-13 00:00 12-Jan-18 00:00
         AUSYD      ETA                03-Jan-13 00:00 13-Jan-18 00:00
         AUMEL      ETA                13-Jan-13 00:00 13-Jan-18 00:00
         AUSYD      ATA                04-Jan-13 00:00 14-Jan-18 00:00
         AUMEL      ATA                14-Jan-13 00:00 14-Jan-18 00:00
         AUSYD      Storage            (empty)         21-Jan-18 00:00
         AUMEL      Storage            (empty)         21-Jan-18 00:00
         AUSYD      Availability       (empty)         22-Jan-18 00:00
         AUMEL      Availability       (empty)         22-Jan-18 00:00
MYPKG               Cargo Cut Off      (empty)         23-Jan-18 00:00
MYPKG               Receival Commences (empty)         24-Jan-18 00:00

Schedule change notifications";

			AssertEmailSent("Expected an email when one or more dates on a " + transportModeDescription + " schedule are amended", expectedSubject, expectedBody);
		}

		protected void TestSendEmailIfRequired(ZString transportModeDescription, ZString expectedVesselVoyageHeader1, ZString expectedVesselVoyageHeader2, bool withCarrier, ZString dataProvider)
		{
			CreateNewScheduleAndMakeChanges(withCarrier, dataProvider);
			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 2, 2), Notifications);

			ZString dataProviderDescription = SailingScheduleHelper.GetDataSourceList().GetDescriptionFromCode(dataProvider);
			ZString expectedSubject = transportModeDescription + " Schedules have changed";
			ZString expectedBody =
transportModeDescription + @" Schedule changes (first time run) to 02-Feb-13 00:00 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: Automated " + transportModeDescription + @" Schedule System
" + expectedVesselVoyageHeader1 + @"
---
<!--StartSection DataProviderHeading-->
(*DataProviderHeading*)
<!--EndSection DataProviderHeading-->
MYPKG               ETD                01-Jan-13 00:00 11-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
MYPKG               ATD                02-Jan-13 00:00 12-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUSYD      ETA                03-Jan-13 00:00 13-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUMEL      ETA                13-Jan-13 00:00 13-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUSYD      ATA                04-Jan-13 00:00 14-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUMEL      ATA                14-Jan-13 00:00 14-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUSYD      Storage            (empty)         21-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUMEL      Storage            (empty)         21-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUSYD      Availability       (empty)         22-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
         AUMEL      Availability       (empty)         22-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
MYPKG               Cargo Cut Off      (empty)         23-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->
MYPKG               Receival Commences (empty)         24-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->

Changed By: CargoWise Service
" + expectedVesselVoyageHeader2 + @"
---
MYPKG               ETD                01-Jan-13 00:00 11-Jan-18 00:00

Changed By: Automated Sailing Schedule System
Vessel: Vessel2
Voyage: Voyage2
Carrier: Carrier
---
<!--StartSection DataProviderHeading-->
(*DataProviderHeading*)
<!--EndSection DataProviderHeading-->
         AUSYD      ETA                03-Jan-13 00:00 20-Jan-18 00:00
<!--StartSection DataProviderDetails-->
1-STOP
<!--EndSection DataProviderDetails-->

Schedule change notifications";

			AssertEmailSent("Expected an email when one or more dates on a " + transportModeDescription + " schedule are amended", expectedSubject, expectedBody);
		}
		protected abstract string TransportModeDescription { get; }

		#endregion
	}
}
