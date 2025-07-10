using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IRegistrationNumberProvider
	{
		ZString GetTaxNumber(ZString countryCode, ZString typeCode);

		IReadOnlyCollection<(ZString taxNumber, ZString countryOfIssue)> GetTaxNumbers(ZString typeCode);
	}
}
