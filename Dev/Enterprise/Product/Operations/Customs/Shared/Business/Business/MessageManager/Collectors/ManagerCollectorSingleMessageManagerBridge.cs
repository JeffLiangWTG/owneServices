using System.Collections.Generic;

namespace Enterprise.Customs.Business
{
	public class ManagerCollectorSingleMessageManagerBridge
	{
		public ManagerCollectorSingleMessageManagerBridge(ManagerCollector collector, IEnumerable<SingleMessageManager> managers)
		{
			ManagerCollector = collector;
			Managers = managers;
		}

		public readonly ManagerCollector ManagerCollector;
		public readonly IEnumerable<SingleMessageManager> Managers;
	}
}
