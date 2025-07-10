using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IRequiredTemperature
	{
		ZBool RequiresTemperatureControl { get; set; }
		ZPropertyInfo RequiresTemperatureControlInfo { get; }
		ZDecimal RequiredTemperatureMaximum { get; set; }
		ZPropertyInfo RequiredTemperatureMaximumInfo { get; }
		ZDecimal RequiredTemperatureMinimum { get; set; }
		ZPropertyInfo RequiredTemperatureMinimumInfo { get; }
		ZString RequiredTemperatureUnit { get; set; }
		ZPropertyInfo RequiredTemperatureUnitInfo { get; }
	}
}
