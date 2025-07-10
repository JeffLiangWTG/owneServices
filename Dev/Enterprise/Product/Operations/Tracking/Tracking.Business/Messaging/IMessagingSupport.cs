using System.Collections;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public interface IMessagingSupport
	{
		IEnumerable EDIMessages { get; }
		ZString ParentReferenceNumber { get; }
	}
}
