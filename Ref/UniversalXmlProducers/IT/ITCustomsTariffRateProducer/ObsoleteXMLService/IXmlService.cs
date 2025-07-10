using System;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.XmlService
{
	public interface IXmlService
	{
		void CreateUniversalXml();
		XmlDocument GetUniversalXml();
		void SaveXml(string filePath);
		void SerializeThenAppend(RefCusTariff refCusTariff);
		void UpdatePublicationDate(DateTime publicationDate);
	}
}
