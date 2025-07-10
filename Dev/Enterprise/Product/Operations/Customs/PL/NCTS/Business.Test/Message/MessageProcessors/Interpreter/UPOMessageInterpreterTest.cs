using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(UPOMessageInterpreter))]
public class UPOInterpreterTest : PL.Business.Testing.UPOInterpreterTest
{
	protected override IMessageInterpreter<IUpo> CreateMessageInterpreter()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return new UPOMessageInterpreter(nctsHeader.MovementHeader);
	}
}
