using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAutoRateDateByChargeGroupConfiguration
	{
		ZString FilterType { get; }
		IEnumerable<IAutoRateDate> GetAutoRateDates(string chargeGroup);
	}
}
