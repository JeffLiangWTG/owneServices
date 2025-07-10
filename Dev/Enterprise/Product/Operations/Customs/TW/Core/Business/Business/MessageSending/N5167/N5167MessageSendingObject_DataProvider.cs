using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5167
{
	public partial class N5167MessageSendingObject : MessageSendingObject, IN5167Declaration
	{
		public ZString DeclarationOfficeID => Header.EntryInstruction?.CEI_CustomsOffice ?? ZString.Empty;

		public ZString FunctionalReferenceID => MessageConstants.FunctionalReferenceIDPlaceHolder;

		public IPartyDetails Agent
		{
			get
			{
				var id = Header.EntryInstruction?.CEI_BoxNumber ?? ZString.Empty;
				var customsProfile = Header.Declaration?.JE_CustomsProfile ?? ZString.Empty;
				return new PartyDetails(id: id, subBoxID: SharedHelper.ExtractSubBoxID(customsProfile), roleCode: "CB");
			}
		}

		public IConsignment Consignment => null;

		public IGoodsShipment GoodsShipment => new GoodsShipment(Header);

		public IPartyDetails Importer
		{
			get
			{
				IPartyDetails importer = null;
				var orgHeader = Header?.Declaration?.Importer;
				if (orgHeader != null && orgHeader.CountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					var customsControlID = ZString.Empty;
					var address = orgHeader.MainAddress;
					if (Header.EntryInstruction.CEI_Style != Constants.DeclarationTypes.Import.D7
						&& address.GetCustomsRegNo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID).IsEmpty)
					{
						customsControlID = address.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID);
					}
					importer = new PartyDetails(customsControlID: customsControlID);
				}
				return importer;
			}
		}

		ZString GetCustomsRegNo(OrgHeader orgHeader, string codeType) => orgHeader.GetCustomsRegNo(codeType);
	}
}
