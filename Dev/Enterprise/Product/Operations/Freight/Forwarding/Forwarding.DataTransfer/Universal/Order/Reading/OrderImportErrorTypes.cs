namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public enum OrderImportErrorTypes
	{
		None,
		AlreadyAttachedToShipment,
		AlreadyAttachedToDeclaration,
		CutOffDateHasPassed,
		CutOffDateNotSet,
		DeclarationHasCommencedEvent
	}
}
