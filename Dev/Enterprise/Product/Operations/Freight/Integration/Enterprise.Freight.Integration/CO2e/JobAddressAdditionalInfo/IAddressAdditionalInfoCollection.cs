using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobAddressAdditionalInfoCollection : IEnumerable<IJobAddressAdditionalInfo>, ICollection
	{
		IJobAddressAdditionalInfo Get(ZString type);
		IJobAddressAdditionalInfo GetOrCreate(ZString type);
	}
}
