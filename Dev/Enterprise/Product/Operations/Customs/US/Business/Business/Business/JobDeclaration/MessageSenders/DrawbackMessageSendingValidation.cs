using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackMessageSendingValidation : Customs.Business.MessageSendingValidation
	{
		public DrawbackMessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors)
			: base(topLevelBusinessObjectForValidation, messageErrors, Env.Security.USDrawbackSendWithMessageErrors)
		{
		}
	}
}
