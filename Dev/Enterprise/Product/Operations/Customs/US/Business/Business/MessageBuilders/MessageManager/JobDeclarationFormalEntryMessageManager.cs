using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationImportMessageManager : JobDeclarationMessageManager
		, Customs.Business.IMessageManager
	{
		public JobDeclarationImportMessageManager(JobDeclaration declaration, ImportMessageSendingActionCollection actions)
			: base(declaration)
		{
			this.actions = actions;
		}
		public readonly ImportMessageSendingActionCollection actions;

		/// <summary>
		/// This is what base uses to send a message
		/// </summary>
		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			return actions.GetSingleMessageManagersSelected();
		}

		protected override GetAllSingleMessageManagersDelegate GetAllSingleMessageManagersDelegateForAmendmentDetection
		{
			get { return delegate { return actions.GetAllSingleMessageManagers(); }; }
		}

		protected override bool ShouldWaitUntilResponded
		{
			get { return false; }
		}

		protected override ZString AdditionalMessageOnOriginalAndAmendment
		{
			get { return "Please exit this job until a response has been received."; }
		}

		protected override bool RunPreSaveValidationWhenSendingAMessage => false;

		#region IMessageManager Members

		Customs.Business.MessageSendingNotificationCollection Customs.Business.IMessageManager.CheckBusinessObjectLevelValidationIfRequired()
		{
			return Customs.Business.MessageSendingValidation.New(declaration, null).CheckBusinessObjectLevelValidation();
		}

		Customs.Business.IDeferredAmendmentSavingOptions Customs.Business.IMessageManager.GetDeferredAmendmentSavingOptions()
		{
			return null;
		}

		#endregion
	}
}
