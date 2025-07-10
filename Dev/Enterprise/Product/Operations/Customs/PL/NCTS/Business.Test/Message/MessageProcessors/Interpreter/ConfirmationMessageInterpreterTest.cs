using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(ConfirmationMessageInterpreter<NctsCommonMovementHeader>))]
sealed class ConfirmationMessageInterpreterTest : ConfirmationMessageInterpreterTestBase<NctsCommonMovementHeader>
{
	protected override NctsCommonMovementHeader CreateAttachedObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader.MovementHeader;
	}
}

