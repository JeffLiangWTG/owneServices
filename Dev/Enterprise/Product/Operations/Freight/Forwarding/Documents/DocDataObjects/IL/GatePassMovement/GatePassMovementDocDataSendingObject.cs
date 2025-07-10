using CargoWise.Common;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class GatePassMovementDocDataSendingObject
	{
		public GatePassMovementDocDataSendingObject(GatePassMovementDocDataObject gatePassMovementDocDataObject, bool isCancelActionTypeCode)
		{
			GatePassMovementDocDataObject = Argument.NotNull(gatePassMovementDocDataObject, nameof(gatePassMovementDocDataObject));
			IsCancelActionTypeCode = isCancelActionTypeCode;
		}

		public readonly GatePassMovementDocDataObject GatePassMovementDocDataObject;

		public bool IsCancelActionTypeCode { get; }
	}
}
