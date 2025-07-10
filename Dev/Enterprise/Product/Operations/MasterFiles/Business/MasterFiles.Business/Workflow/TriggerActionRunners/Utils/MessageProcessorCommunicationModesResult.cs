using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public sealed class MessageProcessorCommunicationModesResult : IMessageProcessorCommunicationModesResult
	{
		public MessageProcessorCommunicationModesResult(IList<IEDICommunicationsMode> communicationModes, MultilingualString logging)
		{
			CommunicationModes = communicationModes;
			ConfigurationLogging = logging;
			Destinations = CommunicationModes.ToArray();
		}

		public IList<IEDICommunicationsMode> CommunicationModes { get; }

		public MultilingualString ConfigurationLogging { get; }

		public IList<IMessageDestinationSource> Destinations { get; }
	}
}
