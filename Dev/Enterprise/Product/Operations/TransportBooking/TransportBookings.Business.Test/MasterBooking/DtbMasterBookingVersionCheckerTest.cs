using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportBookings.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbMasterBookingVersionChecker))]
	class DtbMasterBookingVersionCheckerTest : TestCaseWithFactory
	{
		public void TestSubIsInSyncWithMaster_NoInstructions()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithNoInstructions();
			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: true, "Checker.IsSubInSyncWithMaster() should return true for subBooking as it is in sync with masterBooking");
		}

		public void TestSubIsInSyncWithMaster_WithInstructions()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: true, "Checker.IsSubInSyncWithMaster() should return true for subBooking as it is in sync with masterBooking");
		}

		public void TestSubIsNotInSyncWithMaster_BookingNotLinkedToMaster()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			subBooking.KM_KM_MasterBooking = ZGuid.Empty;
			Factory.Save();
			subBooking.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking DtbBooking record is not linked with masterBooking");
		}

		public void TestSubIsNotInSyncWithMaster_BookingVersionMismatch()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			masterBooking.KM_MasterBookingVersion = 2;
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking has a different MasterBookingVersion");
		}

		public void TestSubIsNotInSyncWithMaster_BookingConsolidationVersionMismatch()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion = 2;
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking booking consolidation parent has a different MasterBookingVersion to masterBooking booking consolidation");
		}

		public void TestSubIsNotInSyncWithMaster_BookingConsolidationNotLinkedToMaster()
		{
			(var masterBooking, var _) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			var otherBookingConsolidation = Helper.CreateConsolidation();
			var otherBooking = Helper.CreateBooking(otherBookingConsolidation);
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, otherBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for otherBooking as otherBookingConsolidation is not linked with masterBookingConsolidation");
		}

		public void TestSubIsNotInSyncWithMaster_BookingInstructionMissingFromSub()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			subBooking.Instructions.Last().Delete();
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking is missing an instruction that is on the masterBooking");
		}

		public void TestSubIsNotInSyncWithMaster_BookingInstructionVersionMismatch()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			masterBooking.Instructions.Last().KN_MasterBookingVersion = 2;
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking has an instruction with a different MasterBookingVersion from the linked masterBooking instruction");
		}

		public void TestSubIsNotInSyncWithMaster_BookingConfirmationMissingFromSub()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			subBooking.Instructions.Last().Confirmations.Last().Delete();
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking is missing a confirmation that is on the masterBooking");
		}

		public void TestSubIsNotInSyncWithMaster_BookingConfirmationVersionMismatch()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();
			masterBooking.Instructions.Last().Confirmations.Last().KK_MasterBookingVersion = 2;
			Factory.Save();

			AssertCheckSubInSyncWithMaster(masterBooking, subBooking, expectedInSync: false, "Checker.IsSubInSyncWithMaster() should return false for subBooking as subBooking has an confirmation with a different MasterBookingVersion from the linked masterBooking confirmation");
		}

		(DtbBooking masterBooking, DtbBooking subBooking) CreateInSyncMasterAndSubBookingWithNoInstructions()
		{
			var subBookingConsolidation = Helper.CreateConsolidation();
			subBookingConsolidation.KB_JobType = "BKG";
			subBookingConsolidation.KB_JobDirection = "PIC";
			var subBooking = Helper.CreateBooking(subBookingConsolidation);
			subBooking.KM_Direction = "ORG";
			var masterBookingConsolidation = Helper.CreateConsolidation();
			masterBookingConsolidation.KB_JobType = "BKG";
			masterBookingConsolidation.KB_JobDirection = "PIC";
			var masterBooking = Helper.CreateBooking(masterBookingConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_Direction = "ORG";
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBookingConsolidation.KB_KB_MasterBookingConsolidation = masterBookingConsolidation.PK;
			Factory.Save();

			subBooking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBookingConsolidation.KB_MasterBookingVersion = masterBookingConsolidation.KB_MasterBookingVersion;
			Factory.Save();

			return (masterBooking, subBooking);
		}

		(DtbBooking masterBooking, DtbBooking subBooking) CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithNoInstructions();

			var masterBookingPicInstruction = masterBooking.Instructions.AddNew("PIC");
			masterBookingPicInstruction.KN_IsMaster = true;
			masterBookingPicInstruction.KN_MasterBookingVersion = 1;
			var masterBookingDlvInstruction = masterBooking.Instructions.AddNew("DLV");
			masterBookingDlvInstruction.KN_IsMaster = true;
			masterBookingDlvInstruction.KN_MasterBookingVersion = 1;
			var subBookingPicInstruction = subBooking.Instructions.AddNew("PIC");
			subBookingPicInstruction.KN_KN_MasterBookingInstruction = masterBookingPicInstruction.PK;
			subBookingPicInstruction.KN_MasterBookingVersion = 1;
			var subBookingDlvInstruction = subBooking.Instructions.AddNew("DLV");
			subBookingDlvInstruction.KN_KN_MasterBookingInstruction = masterBookingDlvInstruction.PK;
			subBookingDlvInstruction.KN_MasterBookingVersion = 1;

			var masterBookingPicConfirmation = masterBookingPicInstruction.Confirmations.AddNew("PIC");
			masterBookingPicConfirmation.KK_IsMaster = true;
			masterBookingPicConfirmation.KK_MasterBookingVersion = 1;
			var masterBookingDlvConfirmation = masterBookingDlvInstruction.Confirmations.AddNew("DLV");
			masterBookingDlvConfirmation.KK_IsMaster = true;
			masterBookingDlvConfirmation.KK_MasterBookingVersion = 1;
			var subBookingPicConfirmation = subBookingPicInstruction.Confirmations.AddNew("PIC");
			subBookingPicConfirmation.KK_KK_MasterBookingConfirmation = masterBookingPicConfirmation.PK;
			subBookingPicConfirmation.KK_MasterBookingVersion = 1;
			var subBookingDlvConfirmation = subBookingDlvInstruction.Confirmations.AddNew("DLV");
			subBookingDlvConfirmation.KK_KK_MasterBookingConfirmation = masterBookingDlvConfirmation.PK;
			subBookingDlvConfirmation.KK_MasterBookingVersion = 1;
			Factory.Save();

			return (masterBooking, subBooking);
		}

		void AssertCheckSubInSyncWithMaster(DtbBooking masterBooking, DtbBooking subBooking, bool expectedInSync, string assertMessage)
		{
			var checker = new DtbMasterBookingVersionChecker(masterBooking);
			var result = checker.IsSubInSyncWithMaster(subBooking);
			AssertEquals(assertMessage, expectedInSync, result);
		}

		TransportBookingTestHelper Helper => helper ?? new TransportBookingTestHelper(Factory);
		readonly TransportBookingTestHelper helper;
	}
}
