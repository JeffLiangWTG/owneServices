using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMessageSendingMovementObject : InBondMessageSendingHeaderObject
	{
		public InBondMessageSendingMovementObject(USInBondMoveHeader movementHeader, InBondMessageType messageType, ISendsMessagesToCustoms messageInitiator)
			: base(movementHeader.Header, messageType, messageInitiator)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		readonly USInBondMoveHeader movementHeader;

		public override IEnumerable<CusInBondMoveHeader> GetMovementHeadersForSending()
		{
			yield return movementHeader.MoveHeader;
		}
	}
}
