using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks
{
	public interface IEHubMessageSender
	{
		void Send(BusinessObjectFactory factory, IEnumerable<string> messages, IEnumerable<string> recipients);
	}
}
