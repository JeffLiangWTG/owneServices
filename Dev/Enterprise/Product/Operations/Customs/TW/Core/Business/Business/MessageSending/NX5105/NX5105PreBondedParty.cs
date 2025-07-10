using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105PreBondedPartyWrapper : IBondedParty
	{
		readonly OrgAddress orgAddress;
		readonly ZString idCusCode;
		readonly ZString customsControlIDCusCode;

		public NX5105PreBondedPartyWrapper(OrgAddress orgAddress, ZString idCusCode, ZString customsControlIDCusCode)
		{
			this.orgAddress = Argument.NotNull(orgAddress, "orgAddress");
			this.idCusCode = idCusCode;
			this.customsControlIDCusCode = customsControlIDCusCode;
		}

		ZString IBondedParty.ID => orgAddress.GetCustomsRegNo(idCusCode);

		ZString IBondedParty.BondedID => null;

		ZString IBondedParty.TypeCode => PartyIdentifierCodeList.Codes._58;

		ZString IBondedParty.CustomsControlID => customsControlIDCusCode;
	}
}
