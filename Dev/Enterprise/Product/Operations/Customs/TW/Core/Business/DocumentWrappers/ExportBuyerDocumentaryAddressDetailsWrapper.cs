using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ExportBuyerDocumentaryAddressDetailsWrapper : DocumentaryAddressDetailsWrapper
	{
		public ExportBuyerDocumentaryAddressDetailsWrapper(OrgHeader header, JobDocAddress documentaryAddress) : base(header, documentaryAddress)
		{
		}

		protected override AddressData GetEnglishAddress()
		{
			return new BuyerAddressData(orgHeader, documentaryAddress, SharedHelper.GetEnglishLanguageCodes());
		}
	}
}
