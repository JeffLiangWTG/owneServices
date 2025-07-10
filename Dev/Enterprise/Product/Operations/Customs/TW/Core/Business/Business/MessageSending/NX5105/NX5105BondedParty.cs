using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105BondedPartyWrapper : IBondedParty
	{
		readonly OrgAddress orgAddress;
		readonly ZString customsControlIDCusCode;

		public NX5105BondedPartyWrapper(OrgAddress orgAddress, ZString customsControlIDCusCode)
		{
			this.orgAddress = Argument.NotNull(orgAddress, "orgAddress");
			this.customsControlIDCusCode = customsControlIDCusCode;
		}

		ZString IBondedParty.ID
		{
			get
			{
				string[] codesToLookFor = new string[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.CodeTypes.WarehouseControlledPremisesID };
				return orgAddress.GetCustomsRegNo(codesToLookFor);
			}
		}

		ZString IBondedParty.BondedID
		{
			get
			{
				var result = ZString.Empty;
				if (!((IBondedParty)this).ID.IsEmpty)
				{
					result = orgAddress.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode);
				}
				return result;
			}
		}

		ZString IBondedParty.TypeCode => PartyIdentifierCodeList.Codes._58;

		ZString IBondedParty.CustomsControlID => orgAddress?.GetCustomsRegNo(customsControlIDCusCode) ?? ZString.Empty;
	}
}
