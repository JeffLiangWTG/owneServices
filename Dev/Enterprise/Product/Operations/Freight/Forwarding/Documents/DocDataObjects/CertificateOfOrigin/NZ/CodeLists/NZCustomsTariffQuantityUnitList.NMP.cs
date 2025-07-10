using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	public partial class NZCustomsTariffQuantityUnitList
	{
		public static IEnumerable<ZString> NumberOfPacksCodes { get; } = new ZString[]
		{
			"BAG",
			"BLC",
			"BSK",
			"BOT",
			"BOX",
			"BBK",
			"BBG",
			"BND",
			"CTN",
			"CAS",
			"COI",
			"CNT",
			"CRD",
			"CRT",
			"CYL",
			"DOZ",
			"ENV",
			"PKG",
			"PAI",
			"PLT",
			"REL",
			"RLL",
			"SHT",
			"SKD",
			"SPL",
			"TOT",
			"TUB"
		};
	}
}
