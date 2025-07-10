using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class ScheduleChooserTest : BaseFreightTest
	{
		#region Autofill event

		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		public void TestAutoFill_Cancel()
		{
			AutofillItemsMethodHelper(ZDialogResult.Cancel, false);
		}

		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		public void TestAutoFill_No()
		{
			AutofillItemsMethodHelper(ZDialogResult.No, false);
		}

		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		public void TestAutoFill_Yes()
		{
			AutofillItemsMethodHelper(ZDialogResult.Yes, true);
		}

		public void TestDoesNotHookEventIfBookingIsForwardRegistered()
		{
			SeaVoyage.GenerateSailings();
			Factory.Save();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_A_BKD = DateTime.Now;
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_IsForwardRegistered = true;
			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var parent = (ISailingChooserParent)quickBooking;
			((ISupportDataImporting)parent.Booking).IsImportingData = false;
			parent.Booking.JS_RL_NKLoadPort = "AUBNE";
			parent.Booking.JS_RL_NKDischargePort = "SGSIN";
			parent.SailingJX = Guid.Empty;
			parent.Booking.JS_E_DEP = DateTime.Now.AddDays(2);
			AssertEquals("SailingJX was not reset", Guid.Empty, parent.SailingJX);
		}

		public void TestShouldNotShowDialogForm_WhenFactoryInTransaction()
		{
			BuildSailing();

			var parent = GetNewQuotedBooking();
			var booking = (ISupportDataImporting)parent.Booking;
			booking.IsImportingData = false;
			parent.Booking.JS_A_BKD = ZDateTime.Now.AddMinutes(-10);
			parent.Booking.JS_RL_NKLoadPort = "AUBNE";
			parent.Booking.JS_RL_NKDischargePort = "SGSIN";

			AssertNull("PRE: Should not have been autofilled yet as we havent entered all the required information", parent.ScheduleChooser.Sailing);
			Assert("PRE: Should not have been asked yet since we cannot autofill anyway", UnitTestUserNotification.Instance.LastMessage.WasNone);

			Factory.Saving += factory =>
			{
				parent.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			};

			Factory.Save();

			AssertNull("Autofilled without popup form as it was in transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotNull("Sailing has been defaulted", parent.ScheduleChooser.Sailing);
		}

		public void AutofillItemsMethodHelper(ZDialogResult answerToReturnFromDialog, bool sailingShouldBeFilled)
		{
			BuildSailing();
			ISailingChooserParent parent = GetNewQuotedBooking();
			var booking = (ISupportDataImporting)parent.Booking;
			booking.IsImportingData = false;
			parent.Booking.JS_A_BKD = ZDateTime.Now.AddMinutes(-10);
			parent.Booking.JS_RL_NKLoadPort = "AUBNE";
			parent.Booking.JS_RL_NKDischargePort = "SGSIN";
			AssertNull("PRE: Should not have been autofilled yet as we havent entered all the required information", parent.ScheduleChooser.Sailing);
			Assert("PRE: Should not have been asked yet since we cannot autofill anyway", UnitTestUserNotification.Instance.LastMessage.WasNone);
			UnitTestUserNotification.Instance.AddAnswer(answerToReturnFromDialog);
			parent.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Would you like to populate the latest Sailing schedule?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
			Assert("Sailing", sailingShouldBeFilled == (parent.ScheduleChooser.Sailing != null));
		}

		#endregion
		#region TestJS_IsDirectBooking
		public void TestJS_IsDirectBooking()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.Booking.JS_IsDirectBooking = true;
			JobSailing sailing = Helper.SydLaxSailing;
			sailing.Voyage.JV_VoyageFlight = "QF123";
			quotedBooking.Booking.JS_JX = sailing.PK;
			AssertEquals("Should have defaulted", "081", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			quotedBooking.Booking.JS_IsDirectBooking = false;
			AssertEquals("Should have defaulted", "", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			AssertEquals("Neutral should have been cleared", false, quotedBooking.Booking.JS_IsNeutralMaster);
			quotedBooking.Booking.JS_IsDirectBooking = true;
			AssertEquals("Should have defaulted", "081", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
		}

		#endregion
		#region TestMasterBillAirlinePrefix
		public void TestMasterBillAirlinePrefix()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			ScheduleChooser scheduleChooser = quotedBooking.ScheduleChooser;
			JobMawb jobMawb = GetTestJobMawb(quotedBooking.Booking);
			Factory.Save();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			SetBookingAirLoadingMasterBill(quotedBooking);
			Factory.Save();
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "083";
			AssertEquals("083", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("083", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "176";
			// It should be "Pending Allocation..." because the prefix changed from 176 to 083 which has triggered deallocation and allocation
			AssertEquals("Pending Allocation...", quotedBooking.ScheduleChooser.MasterBillNeutralMAWB);
			Factory.Save();
			AssertEquals("10000001", quotedBooking.ScheduleChooser.MasterBillNeutralMAWB);
			AssertEquals("176", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			quotedBooking.Booking.JS_IsNeutralMaster = false;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "618";
			AssertEquals("618", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("618", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
		}

		public void TestMasterBillAirlinePrefix_ReadOnly()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			AssertEquals(false, quotedBooking.ScheduleChooser.MasterBillMAWBInfo.ReadOnly);
			quotedBooking.ScheduleChooser.MasterBillMAWB_ReadOnly = true;
			AssertEquals(true, quotedBooking.ScheduleChooser.MasterBillMAWBInfo.ReadOnly);
		}

		#endregion
		#region TestMasterBillAirlinePrefix_NotDefaultWhenNotDirect
		public void TestMasterBillAirlinePrefix_NotDefaultWhenNotDirect()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			ScheduleChooser scheduleChooser = quotedBooking.ScheduleChooser;
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.Booking.JS_IsDirectBooking = true;
			JobSailing sailing = Helper.SydLaxSailing;
			sailing.Voyage.JV_VoyageFlight = "QF123";
			AssertEquals("Should NOT have defaulted yet", "", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			quotedBooking.Booking.JS_JX = sailing.PK;
			AssertEquals("Should have defaulted", "081", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			QuotedBooking quotedBookingNonDirect = GetNewQuotedBooking();
			quotedBookingNonDirect.LoadPort = AUSYDLoco;
			quotedBookingNonDirect.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBookingNonDirect.Booking.JS_IsNeutralMaster = true;
			quotedBookingNonDirect.Booking.JS_IsDirectBooking = false;
			JobSailing sailing2 = Helper.SydLaxSailing;
			sailing2.Voyage.JV_VoyageFlight = "QF123";
			quotedBookingNonDirect.Booking.JS_JX = sailing.PK;
			AssertEquals("Should NOT have defaulted", "", quotedBookingNonDirect.ScheduleChooser.MasterBillAirlinePrefix);
		}

		#endregion
		#region TestMasterBillMAWB
		public void TestMasterBillMAWB()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			JobMawb jobMawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			SetBookingAirLoadingMasterBill(quotedBooking);
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "083";
			AssertExceptionThrown(typeof(MAWBAllocationException), () => Factory.Save());
			AssertEquals("083", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("", quotedBooking.ScheduleChooser.MasterBillMAWB);
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "176";
			quotedBooking.ScheduleChooser.Factory.Save();
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("10000001", quotedBooking.ScheduleChooser.MasterBillMAWB);
			quotedBooking.Booking.JS_IsNeutralMaster = false;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "618";
			AssertEquals("618", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("", quotedBooking.ScheduleChooser.MasterBillMAWB);
		}

		public void TestMasterBillMAWB_ReadOnly()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			AssertEquals(false, quotedBooking.ScheduleChooser.MasterBillMAWBInfo.ReadOnly);
			quotedBooking.ScheduleChooser.MasterBillMAWB_ReadOnly = true;
			AssertEquals(true, quotedBooking.ScheduleChooser.MasterBillMAWBInfo.ReadOnly);
		}

		#endregion
		#region TestJS_MasterBillNum
		public void TestJS_MasterBillNum()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			JobMawb jobMawb = GetTestJobMawb(quotedBooking.Booking);
			SetBookingAirLoadingMasterBill(quotedBooking);
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.Booking.JS_HouseBill = "17610000001";
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("176", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			AssertEquals("10000001", quotedBooking.ScheduleChooser.MasterBillMAWB);
			AssertNotNull("JobMawb object should be loaded.", jobMawb);
		}

		#endregion
		#region TestDuplicateMasterBills
		public void TestDuplicateMasterBills()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>(); // we need a ForwardingJobMawb
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			Transport transport = consol.Transports[0];
			transport.JW_JX = ExportSailing.PK;
			consol.JK_MasterBillNum = TestMasterBillNum;
			Factory.Save();
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			quotedBooking.Booking.JS_IsDirectBooking = true;
			quotedBooking.Booking.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			quotedBooking.Booking.JS_JX = Helper.SydLaxSailing.PK;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = TestMasterBillNum.Substring(0, 3);
			quotedBooking.ScheduleChooser.MasterBillMAWB = TestMasterBillNum.Substring(3);
			AssertEquals("Expecting Duplicate Master Bill Error", true, quotedBooking.ScheduleChooser.MasterBillMAWBInfo.HasErrors());
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "535";
			quotedBooking.ScheduleChooser.MasterBillMAWB = "35879654";
			quotedBooking.Booking.JS_IsNeutralMaster = false;
			AssertNoErrors("Expecting No error on Master Bill", quotedBooking.ScheduleChooser.MasterBillMAWBInfo);
			quotedBooking.ScheduleChooser.MasterBillMAWB = "35879654";
			Factory.Save();
			QuotedBooking quotedBooking2 = GetNewQuotedBooking();
			quotedBooking2.LoadPort = AUSYDLoco;
			quotedBooking2.DischargePort = USLAXLoco;
			quotedBooking2.Booking.JS_IsDirectBooking = true;
			quotedBooking2.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking2.ScheduleChooser.MasterBillAirlinePrefix = "535";
			quotedBooking2.ScheduleChooser.MasterBillMAWB = "35879654";
			AssertEquals("Expecting Duplicate Master Bill Error", true, quotedBooking2.ScheduleChooser.MasterBillMAWBInfo.HasErrors());
			quotedBooking2.ScheduleChooser.MasterBillAirlinePrefix = "213";
			quotedBooking2.ScheduleChooser.MasterBillMAWB = "21312344";
			AssertNoErrors("Expecting No error on Master Bill", quotedBooking2.ScheduleChooser.MasterBillMAWBInfo);
		}

		#endregion
		#region TestJS_TransportMode
		public void TestJS_TransportMode()
		{
			JobSailing sailing = Helper.SydLaxSailing;
			sailing.Voyage.JV_VoyageFlight = "QF123";
			Factory.Save();
			//setup JobMawb
			JobMawb jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_Airline3DigitPrefix = "081";
			jobMawb.JM_MAWB = "10000001";
			jobMawb.JM_ServiceLevel = "STD";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.Booking.JS_IsDirectBooking = true;
			AssertEquals("Should NOT have defaulted yet", "", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			quotedBooking.Booking.JS_JX = sailing.PK;
			AssertEquals("Should have defaulted", "081", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			Factory.Save();
			AssertEquals("Should have defaulted", "10000001", quotedBooking.ScheduleChooser.MasterBillMAWB);
			AssertEquals("Should have a JobMawb Attached", jobMawb.PK, quotedBooking.ScheduleChooser.MAWBAllocation.AllocatedMawb.PK);
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Should still be the same value", "081", quotedBooking.ScheduleChooser.MasterBillAirlinePrefix);
			AssertEquals("Should still be the same value", "10000001", quotedBooking.ScheduleChooser.MasterBillMAWB);
			AssertEquals(true, quotedBooking.ScheduleChooser.MAWBAllocation.GetShouldDeallocate());
			AssertEquals("Is Direct should still be True", true, quotedBooking.Booking.JS_IsDirectBooking);
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Is Direct should still be True", true, quotedBooking.Booking.JS_IsDirectBooking);
		}

		#endregion
		public void TestIMAWBAllocationParentNotesParent()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			IMAWBAllocationParent mawbAllocationParent = quotedBooking.ScheduleChooser;
			AssertEquals(quotedBooking.Booking, mawbAllocationParent.NotesParent);
		}

		#region TestJS_IsNeutralMaster
		public void TestJS_IsNeutralMaster()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			JobMawb jobMawb = GetTestJobMawb(quotedBooking.Booking);
			SetBookingAirLoadingMasterBill(quotedBooking);
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			quotedBooking.Booking.JS_HouseBill = "17610000001";
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.Booking.JS_IsNeutralMaster = false;
			AssertEquals("176", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.Booking.JS_IsNeutralMaster = true;
		}

		public void TestJS_IsNeutralMaster_MAWBNotCleared()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			JobMawb mawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.ScheduleChooser.Parent.IsDirect = true;
			quotedBooking.LoadPort = AUSYDLoco;
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "176";
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.ScheduleChooser.Parent.IsDirect = false;
			quotedBooking.ScheduleChooser.Parent.IsDirect = true;
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "176";
			quotedBooking.ScheduleChooser.Parent.IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.ScheduleChooser.Parent.IsNeutralMaster = false;
			AssertEquals("176", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("", quotedBooking.ScheduleChooser.MasterBillMAWB);
			quotedBooking.ScheduleChooser.Parent.IsNeutralMaster = true;
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
			AssertEquals("10000001", quotedBooking.ScheduleChooser.MasterBillMAWB);
		}

		#endregion
		#region TestJS_NKLoadPort
		public void TestJS_NKLoadPort()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			JobMawb jobMawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			SetBookingAirLoadingMasterBill(quotedBooking);
			quotedBooking.LoadPort = "ZACPT";
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "180";
			AssertEquals("180", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			AssertEquals(true, quotedBooking.Booking.JS_IsNeutralMaster);
			AssertEquals(true, ImportExportHelper.IsBranchCountry(quotedBooking.LoadPort));
			AssertEquals(true, quotedBooking.Booking.IsAir);
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "176";
			Factory.Save();
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
		}

		#endregion
		#region TestOnLoaded
		public void TestOnLoaded()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			JobMawb jobMawb = GetTestJobMawb(quotedBooking.Booking);
			SetBookingAirLoadingMasterBill(quotedBooking);
			AssertEquals(jobMawb, quotedBooking.ScheduleChooser.MAWBAllocation.AllocatedMawb);
		}

		#endregion
		#region TestValidateMasterBillAirlinePrefix
		public void TestValidateMasterBillAirlinePrefix()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true; //Consol defaults this to true, from registry
			SetBookingAirLoadingMasterBill(quotedBooking);
			JobSailing sailing = Helper.SydLaxSailing;
			sailing.Voyage.JV_VoyageFlight = "QF123";
			quotedBooking.Booking.JS_JX = sailing.PK;
			quotedBooking.ScheduleChooser.ValidateMasterBillAirlinePrefix();
			Assert(!quotedBooking.ScheduleChooser.MasterBillAirlinePrefixInfo.HasWarnings());
			quotedBooking.ScheduleChooser.MasterBillAirlinePrefix = "083";
			quotedBooking.ScheduleChooser.ValidateMasterBillAirlinePrefix();
			AssertEquals("The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.", quotedBooking.ScheduleChooser.MasterBillAirlinePrefixInfo.GetWarnings().GetFirstMessage());
		}

		#endregion
		#region TestValidateMasterBillNum
		public void TestValidateMasterBillNum()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			JobMawb jobMawb = GetTestJobMawb(quotedBooking.Booking);
			Factory.Save();
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			SetBookingAirLoadingMasterBill(quotedBooking);
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			JobSailing sailing = Helper.SydLaxSailing;
			sailing.Voyage.JV_VoyageFlight = "SA111";
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.Booking.JS_JX = sailing.PK;
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			Quote quote = QuotedBooking.CreateNewQuote(factory1, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(factory1);
			QuotedBooking newQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, factory1);
			ZInt savedRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			try
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
				newQuotedBooking.Booking.JS_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value + 1);
				newQuotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
				newQuotedBooking.Booking.JS_IsNeutralMaster = false;
				newQuotedBooking.Booking.JS_HouseBill = "17610000001";
				factory1.Save();
				quotedBooking.Booking.JS_HouseBill = "17610000001";
				AssertEquals(true, quotedBooking.Booking.JS_IsNeutralMaster);
				quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
				AssertEquals("This Master Bill Number already exists on another Consol, Shipment or Booking.\r\nPlease select another number.", quotedBooking.ScheduleChooser.MasterBillMAWBInfo.GetErrors().GetFirstMessage());
				newQuotedBooking.Booking.Delete();
				factory1.Save();
				quote = QuotedBooking.CreateNewQuote(factory1, QuotedBooking.QuoteState.ApprovedAndAccepted);
				booking = QuotedBooking.CreateNewBooking(factory1);
				newQuotedBooking = QuotedBooking.New(quote.PK, booking.PK, factory1);
				newQuotedBooking.Booking.JS_HouseBill = "17610000001";
				newQuotedBooking.Booking.JS_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value - 1);
				factory1.Save();
				quotedBooking.Booking.JS_HouseBill = "17610000001";
				AssertEquals(true, quotedBooking.Booking.JS_IsNeutralMaster);
				quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
				AssertNoErrors(quotedBooking.ScheduleChooser.MasterBillMAWBInfo);
				quotedBooking.Factory.Save();
			}
			finally
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, savedRecyclePeriod);
			}

			newQuotedBooking.Booking.Delete();
			factory1.Save();
			quotedBooking.Booking.JS_IsNeutralMaster = false;
			quotedBooking.Booking.JS_HouseBill = "083101010";
			quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
			Assert(quotedBooking.ScheduleChooser.MasterBillMAWBInfo.HasWarnings());
			quotedBooking.Booking.JS_HouseBill = "08310000000";
			quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
			Assert(quotedBooking.ScheduleChooser.MasterBillMAWBInfo.HasWarnings());
			quotedBooking.Booking.JS_HouseBill = "17610000001";
			quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
			AssertEquals("This Master Bill Number is already in stock.\r\nIt is flagged as a Neutral Number.\r\nPlease enter another number.", quotedBooking.ScheduleChooser.MasterBillMAWBInfo.GetErrors().GetFirstMessage());
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			jobMawb.JM_OH_AllocatedTo = ZGuid.NewZGuid();
			quotedBooking.Booking.JS_HouseBill = "17610000001";
			quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
			AssertEquals("MAWB has been \'borrowed out\' to a customer. Please enter another number.", quotedBooking.ScheduleChooser.MasterBillMAWBInfo.GetErrors().GetFirstMessage());
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			jobMawb.JM_OH_AllocatedTo = ZGuid.Empty;
			quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
			AssertEquals("There are no MAWBs left for this Airline. Please add more numbers to your stock.", quotedBooking.ScheduleChooser.MasterBillNeutralMAWBInfo.GetWarnings().GetFirstMessage());
		}

		#endregion
		#region TestValidateMasterBillNumIgnoredOldConsols
		public void TestValidateMasterBillNumIgnoredOldConsols()
		{
			const string Error = "This Master Bill Number already exists on another Consol, Shipment or Booking.\r\nPlease select another number.";
			const string BillNumber1 = "zzzsnth";
			const string BillNumber2 = "zzzaoeu";
			ZInt savedRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			try
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24);
				CommonConsol oldConsol1 = Factory.New<CommonConsol>();
				oldConsol1.JK_TransportMode = Constants.TransportModes.Air;
				oldConsol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-23);
				oldConsol1.JK_MasterBillNum = BillNumber1;
				CommonConsol oldConsol2 = Factory.New<CommonConsol>();
				oldConsol2.JK_TransportMode = Constants.TransportModes.Sea;
				oldConsol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-25);
				oldConsol2.JK_MasterBillNum = BillNumber2;
				Factory.Save();
				QuotedBooking quotedBooking = GetNewQuotedBooking();
				quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
				quotedBooking.Booking.JS_HouseBill = BillNumber1;
				quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
				AssertHasError(quotedBooking.ScheduleChooser.MasterBillMAWBInfo, Error);
				quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
				quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
				AssertNoError(quotedBooking.ScheduleChooser.MasterBillMAWBInfo, Error);
				quotedBooking.Booking.JS_HouseBill = BillNumber2;
				quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
				AssertNoError(quotedBooking.ScheduleChooser.MasterBillMAWBInfo, Error);
				quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
				quotedBooking.ScheduleChooser.ValidateMasterBillMAWB();
				AssertNoError(quotedBooking.ScheduleChooser.MasterBillMAWBInfo, Error);
			}
			finally
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, savedRecyclePeriod);
			}
		}

		#endregion
		#region Carrier Service Levels
		public void TestCarrierServiceLevels_NeutralAirWaybillServiceLevelList()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_PackingMode = Constants.ContainerModes.FCL;
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			OrgCarrierServiceLevel lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "XXX";
			lvl.PL_CarrierServiceLevelDescription = "XXX";
			lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";
			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			lvl = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "MON";
			lvl.PL_CarrierServiceLevelDescription = "Monday tuesday";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			Factory.Save();
			AssertEquals("STD Only", 1, quotedBooking.ScheduleChooser.NeutralAirWaybillServiceLevelList.Count);
			quotedBooking.Booking.BookedShippingLinePK = carrier.PK;
			AssertEquals("Two defined service levels + STD", 3, quotedBooking.ScheduleChooser.NeutralAirWaybillServiceLevelList.Count);
			quotedBooking.Booking.BookedShippingLinePK = carrier2.PK;
			AssertEquals("One defined service level + STD", 2, quotedBooking.ScheduleChooser.NeutralAirWaybillServiceLevelList.Count);
			quotedBooking.Booking.BookedShippingLinePK = ZGuid.Empty;
			AssertEquals("STD Only", 1, quotedBooking.ScheduleChooser.NeutralAirWaybillServiceLevelList.Count);
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			quotedBooking.Booking.JS_JX = sailing.PK;
			AssertEquals("Two defined service levels + STD", 3, quotedBooking.ScheduleChooser.NeutralAirWaybillServiceLevelList.Count);
		}

		#endregion
		#region TestDefaultTheCarrierFromTheSailingForFCLAndLCLBookings
		public void TestDefaultTheCarrierFromTheSailingForFCLAndLCLBookings()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingProvider = true;
			carrier1.OH_IsShippingLine = true;
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingProvider = true;
			carrier2.OH_IsShippingLine = true;
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_OH_Line = carrier1.PK;
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage1.GenerateSailings();
			JobSailing sailing1 = voyage1.Sailings[0];
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_OH_Line = carrier2.PK;
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage2.GenerateSailings();
			JobSailing sailing2 = voyage2.Sailings[0];
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			quotedBooking.Booking.JS_OA_BookedShippingLineAddress = ZGuid.Empty;
			quotedBooking.Booking.JS_JX = sailing1.PK;
			AssertEquals("Should have defaulted the carrier as the booking is FCL", carrier1.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
			quotedBooking.Booking.JS_JX = sailing2.PK;
			AssertEquals("Should not have changed an existing carrier", carrier1.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.OnBoardCourier;
			quotedBooking.Booking.JS_OA_BookedShippingLineAddress = ZGuid.Empty;
			quotedBooking.Booking.JS_JX = sailing1.PK;
			AssertEquals("Should not have defaulted the carrier as the booking is NOT FCL or LCL", ZGuid.Empty, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
			quotedBooking.Booking.JS_JX = ZGuid.Empty;
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			quotedBooking.Booking.BookedShippingLinePK = ZGuid.Empty;
			quotedBooking.Booking.JS_JX = sailing1.PK;
			AssertEquals("Should have defaulted the carrier as the booking is LCL", carrier1.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
			quotedBooking.Booking.JS_JX = sailing2.PK;
			AssertEquals("Should not have changed an existing carrier", carrier1.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
		}

		#endregion
		#region TestJS_AWBServiceLevel
		public void TestJS_AWBServiceLevel()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			JobMawb jobMawb = GetTestJobMawb(quotedBooking.Booking);
			SetBookingAirLoadingMasterBill(quotedBooking);
			OrgCarrierServiceLevel svcLevel = quotedBooking.ScheduleChooser.NeutralAirWaybillServiceLevelList.AddNew();
			svcLevel.PL_Code = "OTH";
			svcLevel.PL_CarrierServiceLevelDescription = "Other";
			jobMawb.JM_ServiceLevel = "OTH";
			AssertEquals("17610000001", quotedBooking.Booking.JS_HouseBill);
		}

		#endregion
		#region TestValidateJS_AWBServiceLevel
		public void TestValidateJS_AWBServiceLevel()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_IsNeutralMaster = true;
			SetBookingAirLoadingMasterBill(quotedBooking);
			quotedBooking.ScheduleChooser.AWBServiceLevel = "STD";
			AssertEquals(false, quotedBooking.ScheduleChooser.AWBServiceLevelInfo.HasErrors());
			quotedBooking.ScheduleChooser.AWBServiceLevel = "";
			AssertEquals(false, quotedBooking.ScheduleChooser.AWBServiceLevelInfo.HasErrors());
			quotedBooking.ScheduleChooser.AWBServiceLevel = "ABC";
			AssertEquals(true, quotedBooking.ScheduleChooser.AWBServiceLevelInfo.HasErrors());
		}

		#endregion
		#region TestGetSailingFromConsolIfConsolidated
		public void TestGetSailingFromConsolIfConsolidated()
		{
			BuildSailing();
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_JX = sailingA.PK;
			AssertEquals("Should be sailing A", sailingA.PK, quotedBooking.ScheduleChooser.Sailing.PK);
			ForwardingConsol consol = quotedBooking.Booking.Consols.AddNew();
			quotedBooking.Booking.JS_JX = ZGuid.Empty;
			Transport transport = consol.Transports[0];
			transport.JW_JX = sailingB.PK;
			AssertEquals("Should be sailing from the consol", sailingB.PK, quotedBooking.ScheduleChooser.Sailing.PK);
		}

		#endregion
		#region TestGetVoyageNoLabelDependingOnTransportMode
		public void TestGetVoyageNoLabelDependingOnTransportMode()
		{
			BuildSailing();
			var quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_JX = sailingA.PK;
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			Assert(quotedBooking.Booking.IsAir);
			AssertEquals("Flight No", quotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode());
			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			Assert(quotedBooking.Booking.IsSea);
			AssertEquals("Voyage No", quotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode());
			quotedBooking.Mode = Core.Constants.RateMode.LRA;
			Assert(quotedBooking.Booking.IsRail);
			AssertEquals("Journey", quotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode());
			quotedBooking.Mode = Core.Constants.RateMode.LRO;
			Assert(quotedBooking.Booking.IsRoad);
			AssertEquals("Truck Ref.", quotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode());
		}

		#endregion
		#region TestSearchForSailingIsSuppressedWhenBookingIsImportingData
		public void TestSearchForSailingIsSuppressedWhenBookingIsImportingData()
		{
			BuildSailing();
			ISailingChooserParent parent = GetNewQuotedBooking();
			var booking = (ISupportDataImporting)parent.Booking;
			booking.IsImportingData = true;
			parent.Booking.JS_RL_NKLoadPort = "AUBNE";
			parent.Booking.JS_RL_NKDischargePort = "SGSIN";
			parent.Booking.JS_A_BKD = ZDateTime.Now.AddMinutes(-10);
			parent.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNull(parent.ScheduleChooser.Sailing);
			booking.IsImportingData = false;
			parent.Booking.JS_A_BKD = ZDateTime.Now;
			AssertNotNull(parent.ScheduleChooser.Sailing);
		}

		#endregion
		#region TestMawbNumberWillNotChangeOnTransportModeChangingToAir
		public void TestMawbNumberWillNotChangeOnTransportModeChangingToAir()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_HouseBill = "HELLO";
			booking.JS_TransportMode = Core.Constants.TransportModes.Road;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals("Precondition: house bill is not empty", "HELLO", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.Booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("House bill has not been set to empty when changing transport mode to AIR", "HELLO", quotedBooking.Booking.JS_HouseBill);
			quotedBooking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("House bill has not been set to empty when changing transport mode to SEA", "HELLO", quotedBooking.Booking.JS_HouseBill);
		}

		#endregion
		#region Implementation
		QuotedBooking GetNewQuotedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			return QuotedBooking.New(quote.PK, booking.PK, Factory);
		}

		const string TestMasterBillNum = "08187443521";
		protected ZString AUSYDLoco = "AUSYD";
		protected ZString USLAXLoco = "USLAX";
		protected SailingsForTestClasses Helper;
		protected override void SetUp()
		{
			base.SetUp();
			Helper = new SailingsForTestClasses(Factory);
		}

		JobMawb GetTestJobMawb(ForwardingShipment booking)
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			JobMawb mawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			mawb.JM_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			mawb.JM_ParentID = booking.PK;
			return mawb;
		}

		JobMawb AddMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;
			return mawb;
		}

		void SetBookingAirLoadingMasterBill(QuotedBooking quotedBooking)
		{
			quotedBooking.Booking.JS_IsDirectBooking = true;
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.Booking.JS_RL_NKLoadPort = "AUSYD";
			quotedBooking.Booking.JS_HouseBill = "17610000001";
		}

		JobSailing sailingA;
		JobSailing sailingB;
		void BuildSailing()
		{
			var vessel1 = RefVessel.LookupVesselByName("PACKING", Factory).First();
			var vessel2 = RefVessel.LookupVesselByName("ANADYR", Factory).First();

			JobVoyage voyageA = Factory.New<JobVoyage>();
			voyageA.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyageA.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyageA.JV_RV_NKVessel = vessel1.RV_FK;
			voyageA.JV_VoyageFlight = "TEST01";
			voyageA.Origins[0].JA_E_DEP = ZDateTime.Now.AddDays(1);
			voyageA.GenerateSailings();
			sailingA = voyageA.Sailings[0];
			sailingA.JX_IsPublished = true;
			JobVoyage voyageB = Factory.New<JobVoyage>();
			voyageB.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyageB.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyageB.JV_RV_NKVessel = vessel2.RV_FK;
			voyageB.JV_VoyageFlight = "TEST02";
			voyageB.Origins[0].JA_E_DEP = ZDateTime.Now.AddDays(1);
			voyageB.GenerateSailings();
			sailingB = voyageB.Sailings[0];
			sailingB.JX_IsPublished = true;
			Factory.Save();
		}
		#endregion
	}
}
