using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESAdditionalInformation.Business
{
	public class AESAdditionalInformationXMLProducer : CommonXMLProducer
	{
		public AESAdditionalInformationXMLProducer()
			: base(
			"EUAES_AdditionalInformation.xml",
			"EU AES - AdditionalInformation",
			XmlWriterHelper.GetRefCusCodeListConfigurationForAES("AI44E")
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new AESAdditionalInformationDataParser();
			}
		}
	}
}
