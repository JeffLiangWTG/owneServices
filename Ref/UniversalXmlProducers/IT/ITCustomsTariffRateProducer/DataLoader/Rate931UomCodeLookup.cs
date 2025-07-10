using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	sealed class Rate931UomCodeLookup : UomCodeLookup
	{
		protected override (string, string)[] GetLookupList()
		{
			return base.GetLookupList()
				.Select(item => item.Code == KgmCode ? (KgmoCode, item.Abbreviation) : item)
				.ToArray();
		}

		const string KgmCode = "KGM";
		const string KgmoCode = "KGMO";
	}
}
