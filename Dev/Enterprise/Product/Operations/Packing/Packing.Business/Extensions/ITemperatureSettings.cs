using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface ITemperatureSettings
	{
		bool IsTemperatureControlled { get; set; }
		ZDecimal TemperatureMin { get; set; }
		ZDecimal TemperatureMax { get; set; }
		ZString TemperatureUnit { get; set; }
	}
}
