using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IRequiredTaxNumberHelper
	{
		ZString ExpandTaxTypeCodeIfNecessary(ZString taxTypeCode);

		IEnumerable<string> GetTaxCodeTypes(ZString docType, ZString countryCode);
	}
}
