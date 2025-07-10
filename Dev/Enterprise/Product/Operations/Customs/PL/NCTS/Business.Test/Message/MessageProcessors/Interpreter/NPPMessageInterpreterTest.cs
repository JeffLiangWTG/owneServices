using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NPPMessageInterpreter<NctsCommonMovementHeader>))]
sealed class NPPMessageInterpreterTest : PL.Business.Testing.NPPMessageInterpreterTest
{
	protected override IMessageInterpreter<IConfirmation> CreateMessageInterpreter()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return new NPPMessageInterpreter<NctsCommonMovementHeader>(nctsHeader.MovementHeader);
	}
}
