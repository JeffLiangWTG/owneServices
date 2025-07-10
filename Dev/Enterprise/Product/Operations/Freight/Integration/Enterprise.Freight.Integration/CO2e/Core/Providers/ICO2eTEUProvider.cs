using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface ICO2eTEUProvider : ICO2eProvider
	{
		bool IncludeTEU { get; }
		ZDecimal NumberOfTEU { get; }
		ZDecimal TonnesPerTEU { get; }
		ZDecimal ContainerEmptyWeightPerTEU { get; }
		ZString ContainerEmptyWeightPerTEUUnit { get; }
	}
}
