using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public class ConditionData
	{
		public string ConditionType { get; set; }
		public Applicability applicability { get; set; }
		public CertificateData certificateData { get; set; }
	}

	public class Applicability
	{
		public List<string> ExcludedCountries { get; set; }
		public string AdditionalCode { get; set; }

		public string TradeGroup { get; set; }
		public DateTime StartDate { get; set; }
	}
}
