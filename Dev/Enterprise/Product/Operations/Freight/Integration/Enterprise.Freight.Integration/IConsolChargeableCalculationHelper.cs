using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IConsolChargeableCalculationHelper
	{
		IReadOnlyDictionary<ZString, ZDecimal> CalculateChargeables(ISet<ZString> uniqueRefs, ZString weightUnit, ZString volumeUnit);
	}
}
