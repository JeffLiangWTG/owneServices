using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IForwardingConsolDataObjectsCollection : IReadOnlyCollection<IForwardingConsolDataObject>
	{
		IForwardingConsolDataObject Departure { get; }
		IForwardingConsolDataObject Arrival { get; }
	}
}
