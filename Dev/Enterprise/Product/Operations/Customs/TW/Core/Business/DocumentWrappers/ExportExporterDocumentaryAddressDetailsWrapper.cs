using Enterprise.Customs.TW.Business.N5203;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ExportExporterDocumentaryAddressDetailsWrapper : DocumentaryAddressDetailsWrapper
	{
		public ExportExporterDocumentaryAddressDetailsWrapper(OrgHeader header, JobDocAddress documentaryAddress)
			: base(header, documentaryAddress)
		{
		}

		protected override AddressData GetEnglishAddress()
		{
			return new ExporterAddressData(orgHeader, documentaryAddress, SharedHelper.GetEnglishLanguageCodes());
		}
	}
}
