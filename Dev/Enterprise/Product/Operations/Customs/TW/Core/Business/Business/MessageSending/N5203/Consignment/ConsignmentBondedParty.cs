using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class BondedParty : IBondedParty
	{
		public BondedParty(OrgAddress address)
		{
			this.orgAddress = address;
		}

		readonly OrgAddress orgAddress;

		public ZString ID
		{
			get
			{
				string[] codesToLookFor = new string[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.CodeTypes.WarehouseControlledPremisesID };
				return orgAddress.GetCustomsRegNo(codesToLookFor);
			}
		}

		public ZString BondedID
		{
			get
			{
				var result = ZString.Empty;
				if (!ID.IsEmpty)
				{
					result = orgAddress.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode);
				}
				return result;
			}
		}

		public ZString TypeCode => PartyIdentifierCodeList.Codes._58;

		public ZString CustomsControlID => ZString.Empty;
	}
}
