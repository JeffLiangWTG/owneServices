using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSGoodsItemsProvider : INCTSGoodsItems
	{
		public NCTSGoodsItemsProvider(NctsDepartureCargoDesc nctsDepartureCargoDesc)
		{
			goodsItem = nctsDepartureCargoDesc;
			nctsHeader = (NctsHeader)goodsItem.Header;
		}
		readonly NctsDepartureCargoDesc goodsItem;
		readonly NctsHeader nctsHeader;

		public int LineNumber => goodsItem.BY_LineNo;
		public string TariffCode => goodsItem.BY_FormattedHarmonisedTariff.Replace(".", "");
		public string Description => goodsItem.BY_Description;
		public string DescriptionLNG => TRMessageConstants.LanguageCode;
		public decimal GrossWeight => goodsItem.BY_GrossWeight.RoundWeight();
		public decimal NetWeight => goodsItem.BY_NetWeight.RoundWeight();
		public string CountyOfDispatchCode => goodsItem.BY_RN_NKCountryOfDispatch;
		public string CountyOfDestinationCode => goodsItem.BY_RN_NKCountryOfDestination;
		public decimal MonetaryValue => goodsItem.BY_MonetaryValue.RoundAmount();
		public string MonetaryValueCurrencyCode => goodsItem.BY_RX_NKCurrency;

		public IReadOnlyCollection<INCTSAdditionalDocuments> SupportingDocuments => fSupportingDocuments = fSupportingDocuments ?? goodsItem.SupportingDocuments.Select(document => new NCTSDocumentsProvider(document)).ToArray();
		IReadOnlyCollection<INCTSAdditionalDocuments> fSupportingDocuments;

		public IReadOnlyCollection<INCTSAdditionalDocuments> PreviousDocuments => fPreviousDocuments = fPreviousDocuments ?? goodsItem.PreviousDocuments.Where(x => x.CSI_Code != NCTSMessageProviderConstants.PreviousDocuments.WarehouseCode).Select(document => new NCTSDocumentsProvider(document)).ToArray();
		IReadOnlyCollection<INCTSAdditionalDocuments> fPreviousDocuments;

		public IReadOnlyCollection<INCTSSpecialMentionsDocuments> SpecialMentionsDocuments => fSpecialMentionsDocuments = fSpecialMentionsDocuments ?? goodsItem.AdditionalInfos.Select(document => new NCTSDocumentsProvider(document)).ToArray();
		IReadOnlyCollection<INCTSSpecialMentionsDocuments> fSpecialMentionsDocuments;

		#region Company

		public INCTSOrganization Consignor => nctsHeader.Consignor.Address == null && goodsItem.Consignor.Address != null ? new NCTSOrganizationProvider(goodsItem.Consignor) : null;

		public INCTSOrganization Consignee => nctsHeader.Consignee.Address == null && goodsItem.Consignee.Address != null ? new NCTSOrganizationProvider(goodsItem.Consignee) : null;

		#endregion

		public IReadOnlyCollection<INCTSContainers> Containers => fContainers = fContainers ?? goodsItem.ContainersPivots.Where(x => x.ContainerSelected).Select(containers => new NCTSContainerProvider(containers)).ToArray();
		IReadOnlyCollection<INCTSContainers> fContainers;

		public IReadOnlyCollection<INCTSPacks> Packs => fPacks = fPacks ?? goodsItem.Packages.Select(packs => new NCTSPacksProvider(packs)).ToArray();
		IReadOnlyCollection<INCTSPacks> fPacks;
	}
}
