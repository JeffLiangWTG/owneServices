using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class DeclarationMessageOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public DeclarationMessageOperationalActionMethodApplicator()
			: base(OperationalActionLogAndUserNotificationWrapper.Constants.SubmitOriginalEntryToTaiwanCustoms)
		{
			ReMergeAndCalculate = false;
			SuppressNotificationPopout = true;
			IgnoreMessageWarnings = false;
			UseDaysOfDelayed = true;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, CargoWise.EntityFramework.BusinessObject[] targets)
		{
			var notificationCollector = new MessageNotificationCollector();
			var sendsMessagesToCustomsGUI = new OperationalActionMessageNotificationCollector(log) { SuppressUserInteraction = SuppressNotificationPopout };
			foreach (var declaration in targets)
			{
				if (declaration is JobDeclaration twDeclaration)
				{
					twDeclaration.MessageInitiator = sendsMessagesToCustomsGUI;
					var messageType = twDeclaration.CustomsMessageType;
					var wrapper = new JobDeclarationMessageSendingObjectParent(twDeclaration, messageType);
					wrapper.MenuCaption = OperationalActionLogAndUserNotificationWrapper.Constants.SubmitOriginalEntryToTaiwanCustoms;
					var messageManagerWrapper = new BatchDeclarationMessageManagerWrapper(twDeclaration, wrapper, log, notificationCollector, sendsMessagesToCustomsGUI, messageType, IgnoreMessageWarnings, SuppressNotificationPopout, ReMergeAndCalculate);
					messageManagerWrapper.PerformFunctionOperationalAction(UseDaysOfDelayed);
				}
			}
		}

		#region User Action Configuration Section
		public bool UseDaysOfDelayed { get; set; }
		public bool ReMergeAndCalculate { get; set; }
		public bool SuppressNotificationPopout { get; set; }
		public bool IgnoreMessageWarnings { get; set; }
		#endregion
	}
}
