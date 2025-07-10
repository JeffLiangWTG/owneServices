namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	#region Acas Shipment Report

	public enum AcasState
	{
		None,
		OriginalSent,
		AmendmentSent,
		AcknowledgementSent,
		AcknowledgementRequired,
		AmendmentRequired,
		AssessmentOngoing,
		AssessmentComplete,
		HoldInPlace,
		HoldRemoved
	}

	#endregion

	#region Acas House Checklist

	public enum AcasHouseChecklistMessageState
	{
		None,
		OriginalSent,
		OriginalForwarded,
		RejectedByEHub,
		RejectedByCBP,
		MessagePendingProcessing,
		ClearanceCompleted,
		HoldInPlace,
		HoldRemoved
	}

	#endregion
}
