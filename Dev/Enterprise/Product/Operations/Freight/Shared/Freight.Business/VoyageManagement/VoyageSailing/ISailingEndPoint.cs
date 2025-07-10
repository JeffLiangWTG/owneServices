using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Business
{
	public interface ISailingEndPoint : IEDIMessageCollectionProvider
	{
		ZGuid PK { get; }
		ZString Port { get; }
		ZDateTime EstimatedDate { get; }
		ZString Direction { get; }
		ZString Vessel { get; }
		ZString Voyage { get; }
	}
}
