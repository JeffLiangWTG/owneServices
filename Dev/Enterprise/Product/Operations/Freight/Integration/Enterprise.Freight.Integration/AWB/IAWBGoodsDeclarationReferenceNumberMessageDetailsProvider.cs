using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider
	{
		List<ZString> Numbers { get; }
		ZString CountryOfIssue { get; }
		ZString MovementCode { get; }
	}
}
