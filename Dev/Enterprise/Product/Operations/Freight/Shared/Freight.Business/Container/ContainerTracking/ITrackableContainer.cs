using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public interface ITrackableContainer
	{
		ZString ContainerNumber { get; }
		bool ContainerNumberHasChanges { get; }

		Logs Logs { get; }
	}
}
