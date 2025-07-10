using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESTransportDocumentType.Business
{
	public class AESTransportDocumentTypeXMLProducer : CommonXMLProducer
	{
		public AESTransportDocumentTypeXMLProducer()
			: base(
			"EUAES_TransportDocumentType.xml",
			"EU AES - TransportDocumentType",
			XmlWriterHelper.GetRefCusCodeListConfigurationForAES("TD44E")
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new AESTransportDocumentTypeDataParser();
			}
		}
	}
}
