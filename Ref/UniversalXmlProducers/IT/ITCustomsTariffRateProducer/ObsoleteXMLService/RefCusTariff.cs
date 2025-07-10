using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusTariff
	{
		[XmlElement(ElementName = "ZZ1_TariffCode")]
		public string TariffCode { get; set; }

		[XmlElement(ElementName = "RefCusVATApplicability")]
		public List<RefCusVatApplicability> VatApplicabilities { get; }

		[XmlElement(ElementName = "RefCusRate")]
		public List<RefCusRate> CusRates { get; }

		[XmlElement(ElementName = "RefCusCondition")]
		public List<RefCusCondition> CusConditions { get; }

		RefCusTariff()
		{
		}

		public RefCusTariff(string tariffCode)
		{
			TariffCode = tariffCode;

			VatApplicabilities = new List<RefCusVatApplicability>();
			CusConditions = new List<RefCusCondition>();
			CusRates = new List<RefCusRate>();
		}
	}
}
