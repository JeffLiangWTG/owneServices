using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC141CSenderTest : NCTSMessageSenderTest<CC141CSender, ICC141C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC141CSender(null));
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;
}
