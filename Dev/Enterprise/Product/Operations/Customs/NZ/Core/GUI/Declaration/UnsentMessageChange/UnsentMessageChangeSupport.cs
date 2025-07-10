namespace Enterprise.Customs.NZ.GUI.Declaration
{
	using System.Windows.Forms;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.ZArchitecture.GUI;

	public class UnsentMessageChangeSupport
	{
		public ContinueWithSave CheckForHeldMessageChangesAndPerformUserAction(JobDeclaration declaration, ZForm parentForm)
		{
			var continueSave = ContinueWithSave.Yes;
			var heldMessageInfo = new HeldMessageSyncInfo(declaration.CusEntryHeader);

			if (heldMessageInfo.SetHasChangesToHeldMessages())
			{
				var popupForm = new UnsentMessageChangeForm(heldMessageInfo);

				if (ZFormModaliser.ShowDialogAndDispose(popupForm) == DialogResult.Cancel)
				{
					continueSave = ContinueWithSave.No;
				}
				else
				{
					continueSave =
						(PerformApplicableHeldMessageActions(heldMessageInfo, parentForm)) ?
						ContinueWithSave.Yes : ContinueWithSave.No;
				}
			}

			return continueSave;
		}

		bool PerformApplicableHeldMessageActions(HeldMessageSyncInfo heldMessageInfo, ZForm parentForm)
		{
			var result = true;
			var cancelledMessages = heldMessageInfo.PerformHeldMessageActions();

			if (heldMessageInfo.ShouldRecreateMessage)
			{
				var messageManager = new MessagingFunctionalityManager(heldMessageInfo.EntryHeader.Declaration);
				result = messageManager.ShowSubmitToCustomsForm(Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, parentForm, false, false);

				if (!result)
				{
					heldMessageInfo.RevertCancelHeldMessage(cancelledMessages);
				}
			}

			return result;
		}
	}
}
