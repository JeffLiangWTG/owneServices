using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public interface IDataLoader
	{
		bool Load(string baseUrl);
	}

	public abstract class BaseDataLoader : IDataLoader
	{
		protected IHttpHandler Handler { get; }
		protected ILogger Logger { get; }

		protected BaseDataLoader(ILogger logger, IHttpHandler handler)
		{
			Handler = Argument.NotNull(handler, nameof(handler));
			Logger = Argument.NotNull(logger, nameof(logger));
		}

		public abstract bool Load(string baseUrl);
	}
}
