using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESNationality.Business
{
	public class AESNationalityXMLProducer : CommonXMLProducer
	{
		public AESNationalityXMLProducer()
			: base(Constants.AESNationality.OutputFileName, Constants.AESNationality.OutputFileDataSource, XmlWriterHelper.GetRefCusCodeListConfigurationForAES(Constants.ZZRefCusCodeList.AESNationality, defaultStartDateWithMinimumDate: true)
			)
		{
		}

		protected override CommonDataParser DataParser => new AESNationalityDataParser();
	}
}
