using Enterprise.Semaphores.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Web
{
	[Immutable]
	public class RFLoginSemaphoreType : ISemaphoreType
	{
		string ISemaphoreType.Category
		{
			get { return "LGN"; }
		}

		string ISemaphoreType.LockInfo
		{
			get { return "RFWebLogin"; }
		}

		int ISemaphoreType.MaxConcurrentHandles
		{
			get { return 0; }
		}
	}
}
