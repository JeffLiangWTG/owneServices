using System.Collections.Generic;
using System.Threading.Tasks;

namespace OcmPoc.Mapping.Interface
{
	public interface IMapping
    {
		void Initialise();

		Task<MapForSendResult> MapAsync(MapForSendCommand command);
		Task<MapReceivedResult> MapAsync(MapReceivedCommand command);
    }
}
