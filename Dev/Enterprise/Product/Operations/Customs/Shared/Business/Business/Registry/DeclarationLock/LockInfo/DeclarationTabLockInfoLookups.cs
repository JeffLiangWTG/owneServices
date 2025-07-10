using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataRegistry.Business
{
	public sealed class DeclarationTabLockInfoLookups : ZLookups
	{
		public DeclarationTabLockInfoLookups(DeclarationTabLockInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList TabPageList
		{
			get
			{
				if (tabPageList == null)
				{
					var parent = (DeclarationTabLockInfo)Parent;

					tabPageList = new CodeDescriptionPairList();

					switch (parent.LockConfig?.DeclarationType)
					{
						case "ARN":
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.All, Constants.Customs.DeclarationTabPages.Descriptions.All);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsArrivalNotification, Constants.Customs.DeclarationTabPages.Descriptions.NctsArrivalNotification);
							if (parent.GetCompanyCountry() == Core.Constants.CountryCodes.Switzerland)
							{
								tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsArrivalAdditionalGoodsInformation, Constants.Customs.DeclarationTabPages.Descriptions.NctsArrivalAdditionalGoodsInformation);
							}
							break;
						case "DEP":
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.All, Constants.Customs.DeclarationTabPages.Descriptions.All);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHeader, Constants.Customs.DeclarationTabPages.Descriptions.NctsDepartureHeader);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsDepartureTransportAndEquipment, Constants.Customs.DeclarationTabPages.Descriptions.NctsDepartureTransportAndEquipment);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHouse, Constants.Customs.DeclarationTabPages.Descriptions.NctsDepartureHouse);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsDepartureGoodsItem, Constants.Customs.DeclarationTabPages.Descriptions.NctsDepartureGoodsItem);
							break;
						case "ULR":
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.NctsArrivalUnloadingRemarks, Constants.Customs.DeclarationTabPages.Descriptions.NctsArrivalUnloadingRemarks);
							break;
						default:
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.All, Constants.Customs.DeclarationTabPages.Descriptions.All);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.Declaration, Constants.Customs.DeclarationTabPages.Descriptions.Declaration);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.DeclarationCustom, Constants.Customs.DeclarationTabPages.Descriptions.DeclarationCustom);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers, Constants.Customs.DeclarationTabPages.Descriptions.DeclarationNumbers);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.DeclarationOrders, Constants.Customs.DeclarationTabPages.Descriptions.DeclarationOrders);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.DeclarationOrganizations, Constants.Customs.DeclarationTabPages.Descriptions.DeclarationOrganizations);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.DeclarationPickupOrDelivery, Constants.Customs.DeclarationTabPages.Descriptions.DeclarationPickupOrDelivery);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.DeclarationServices, Constants.Customs.DeclarationTabPages.Descriptions.DeclarationServices);

							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.Routing, Constants.Customs.DeclarationTabPages.Descriptions.Routing);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.Containers, Constants.Customs.DeclarationTabPages.Descriptions.Containers);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.Packing, Constants.Customs.DeclarationTabPages.Descriptions.Packing);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.InvoiceGroups, Constants.Customs.DeclarationTabPages.Descriptions.InvoiceGroups);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.InvoiceHeaders, Constants.Customs.DeclarationTabPages.Descriptions.InvoiceHeaders);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.InvoiceLines, Constants.Customs.DeclarationTabPages.Descriptions.InvoiceLines);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.Misc, Constants.Customs.DeclarationTabPages.Descriptions.Misc);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.MessageOrEntries, Constants.Customs.DeclarationTabPages.Descriptions.MessageOrEntries);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.EntryInstructions, Constants.Customs.DeclarationTabPages.Descriptions.EntryInstructions);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.EntryDetails, Constants.Customs.DeclarationTabPages.Descriptions.EntryDetails);
							tabPageList.AddPair(Constants.Customs.DeclarationTabPages.Codes.BondedDetails, Constants.Customs.DeclarationTabPages.Descriptions.BondedDetails);
							break;
					}
				}

				return tabPageList;
			}
		}
		CodeDescriptionPairList tabPageList;
	}
}
