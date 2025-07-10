using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IApportionmentMethodOverride
	{
		ZString Module { get; }

		ZString ConsolType { get; }

		ZString ApportionmentMethod { get; }

		ZString ContainerMode { get; }

		ZString TransportMode { get; }

		ZString Direction { get; }
	}
}
