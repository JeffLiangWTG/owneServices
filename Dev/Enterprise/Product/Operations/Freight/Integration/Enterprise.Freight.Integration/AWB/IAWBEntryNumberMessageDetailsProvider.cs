using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBEntryNumberMessageDetailsProvider
	{
		ZString Type { get; }
		ZString Number { get; }
	}
}
