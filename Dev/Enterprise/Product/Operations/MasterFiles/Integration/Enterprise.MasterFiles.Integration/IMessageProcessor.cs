using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IMessageProcessor : IProcessor
	{
		IMessageProcessorCommunicationModesResult GetDestinations();
	}

	public interface IMessageProcessorCommunicationModesResult
	{
		IList<IMessageDestinationSource> Destinations { get; }
		MultilingualString ConfigurationLogging { get; }
	}
}
