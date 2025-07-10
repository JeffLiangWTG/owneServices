using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Security;

namespace Enterprise.Customs.US.LVS.Business
{
	public class MessageSendingValidation : Customs.Business.MessageSendingValidation
	{
		protected MessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, bool refreshValidation = true) : base(topLevelBusinessObjectForValidation, messageErrors, refreshValidation)
		{
		}

		protected internal MessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true) : base(topLevelBusinessObjectForValidation, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
		}

		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation)
		{
			return new MessageSendingValidation(topLevelBusinessObjectForValidation, null);
		}

		protected override void MarkAsNeedingValidationIncludingChildren()
		{
			base.MarkAsNeedingValidationIncludingChildren();
			TopLevelBusinessObjectForValidation.MarkAsNeedingValidationIncludingChildren();
		}
	}
}
