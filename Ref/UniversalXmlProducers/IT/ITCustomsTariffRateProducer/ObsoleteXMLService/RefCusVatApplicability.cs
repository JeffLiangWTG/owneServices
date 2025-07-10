using System;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusVatApplicability
	{
		[XmlElement(ElementName = "ZX5_ZZF_NKTaxOrFeeCode")]
		public string TaxOrFeeCode { get; set; }

		[XmlElement(ElementName = "ZX5_StartDate")]
		public string StartDate { get; set; }

		[XmlElement(ElementName = "ZX5_AdditionalCode")]
		public string AdditionalCode { get; set; }

		internal RefCusVatApplicability()
		{ }

		public RefCusVatApplicability(string taxOrFeeCode, string additionalCode, DateTime startDate)
		{
			StartDate = startDate.ToString("s");
			TaxOrFeeCode = taxOrFeeCode;
			AdditionalCode = additionalCode;
		}
	}
}
