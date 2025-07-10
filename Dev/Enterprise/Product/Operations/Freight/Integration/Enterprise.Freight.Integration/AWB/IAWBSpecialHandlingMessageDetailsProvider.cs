using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBSpecialHandlingMessageDetailsProvider
	{
		ZString SpecialHandling { get; }
	}
}
