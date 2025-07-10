using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ICustomsMessagingSupporter
	{
		BusinessObject TopLevelBusinessObject { get; }

		IReadOnlyCollection<ICustomsMessenger> Messengers { get; }

		ICustomsMessagingProvider Provider { get; }
	}
}
