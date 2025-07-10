using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class BondedGoodsBondedFactories : IBondedParty
	{
		public BondedGoodsBondedFactories(BondedFactory bondedFactory, ZString customsControlID)
		{
			this.bondedFactory = bondedFactory;
			CustomsControlID = customsControlID;
		}

		readonly BondedFactory bondedFactory;

		public ZString ID => bondedFactory.Address.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode);

		public ZString BondedID => ZString.Empty;

		public ZString TypeCode => PartyIdentifierCodeList.Codes._58;

		public ZString CustomsControlID { private set; get; }
	}
}
