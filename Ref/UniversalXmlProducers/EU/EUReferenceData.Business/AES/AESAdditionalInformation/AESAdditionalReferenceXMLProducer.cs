using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESAdditionalReference.Business
{
	public class AESAdditionalReferenceXMLProducer : CommonXMLProducer
	{
		public AESAdditionalReferenceXMLProducer()
			: base(
			"EUAES_AdditionalReference.xml",
			"EU AES - AdditionalReference",
			XmlWriterHelper.GetRefCusCodeListConfigurationForAES("AR44E")
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new AESAdditionalReferenceDataParser();
			}
		}
	}
}
