using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.AMS.Messaging
{
	public class AMSMessageSendingValidation : MessageSendingValidation
	{
		protected AMSMessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, bool canSendErrorsAsWarnings, IEnumerable<INotification> messageErrors)
			: base(topLevelBusinessObjectForValidation, messageErrors, Env.Security.USAMSSendWithMessageErrors)
		{
			canSendMessageErrorsAsWarnings = canSendErrorsAsWarnings;
		}

		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, bool canSendErrorsAsWarnings, IEnumerable<INotification> messageErrors)
		{
			MessageSendingValidation result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(topLevelBusinessObjectForValidation, messageErrors);
			}
			else
			{
				result = new AMSMessageSendingValidation(topLevelBusinessObjectForValidation, canSendErrorsAsWarnings, messageErrors);
			}

			return result;
		}

		protected override bool CanSendWithMessageErrors => canSendMessageErrorsAsWarnings && base.CanSendWithMessageErrors;

		readonly bool canSendMessageErrorsAsWarnings;
	}
}
