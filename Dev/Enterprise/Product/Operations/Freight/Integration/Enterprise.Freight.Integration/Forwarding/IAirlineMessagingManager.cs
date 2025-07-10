
using System.Collections.Generic;

namespace Enterprise.Freight.Integration
{
	public interface IAirlineMessagingManager
	{
		bool Send<T>(T documentObject, KeyValuePair<string, string>[] additionalInfoCollection, out string failureReason);
	}
}
