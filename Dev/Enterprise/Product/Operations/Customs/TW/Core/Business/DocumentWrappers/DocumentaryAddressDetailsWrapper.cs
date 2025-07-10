using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class DocumentaryAddressDetailsWrapper : PartyDetailsWrapper
	{
		public DocumentaryAddressDetailsWrapper(OrgHeader header, JobDocAddress documentaryAddress)
			: base(documentaryAddress.Address, header)
		{
			this.documentaryAddress = documentaryAddress;
		}

		protected readonly JobDocAddress documentaryAddress;

		public ZString Address => AddressCore.Line;

		public ZString CompanyName => NameCore;

		protected override AddressData GetEnglishAddress()
		{
			return new AddressData(orgHeader, documentaryAddress, SharedHelper.GetEnglishLanguageCodes());
		}
	}
}
