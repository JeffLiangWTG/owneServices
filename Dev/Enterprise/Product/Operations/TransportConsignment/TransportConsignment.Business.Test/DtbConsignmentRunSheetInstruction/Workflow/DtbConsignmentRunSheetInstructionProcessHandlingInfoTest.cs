using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetInstructionProcessHandlingInfoTest : TestCaseWithFactory
	{
		#region TestDtbConsignmentRunSheetInstructionLineTriggers

		public void TestDtbConsignmentRunSheetInstructionLineTriggers()
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction1 = runSheet.RunSheetInstructions.AddNew();
			var instruction2 = runSheet.RunSheetInstructions.AddNew();
			var trigger = runSheet.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.RunSheetInstruction;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());
			Factory.Save();

			instruction1.GetLogs().AddNew(Events.CustomisableEvent00);
			instruction2.GetLogs().AddNew(Events.CustomisableEvent00);
			AssertEquals(2, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
		}

		static void SetupEmailNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "bung@bung.bung";
		}

		#endregion

		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction = runSheet.RunSheetInstructions.AddNew();

			var handlingInfo = new DtbConsignmentRunSheetInstructionProcessHandlingInfo(instruction);

			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ArrivalCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

	}
}
