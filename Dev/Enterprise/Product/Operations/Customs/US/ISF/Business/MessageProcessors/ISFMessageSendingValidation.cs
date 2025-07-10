using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFMessageSendingValidation : MessageSendingValidation
	{
		protected ISFMessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors)
			: base(topLevelBusinessObjectForValidation, messageErrors, Env.Security.ImporterSecurityFilingSendWithMessageErrors)
		{
		}

		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors)
		{
			MessageSendingValidation result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(topLevelBusinessObjectForValidation, messageErrors);
			}
			else
			{
				result = new ISFMessageSendingValidation(topLevelBusinessObjectForValidation, messageErrors);
			}
			return result;
		}
	}
}
