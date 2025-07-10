using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class FaultInterchangeUnpackingStrategyTestPLN : PL.Business.Testing.FaultInterchangeUnpackingStrategyTest
{
	protected override string ApplicationCode => ApplicationCodes.PLCustomsNCTS;

	protected override BusinessObject CreateTransmitMessageLinkedObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader.MovementHeader;
	}
}
