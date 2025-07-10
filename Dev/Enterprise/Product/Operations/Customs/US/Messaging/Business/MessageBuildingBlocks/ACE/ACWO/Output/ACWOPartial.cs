namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse)]
	partial class ACWOCW03 : MessageBlock, I7501Errors
	{
		public bool Accepted
		{
			get { return this.ConditionCode == "C01"; }
		}

		ZString I7501Errors.LineNumber
		{
			get { return this.EntrySummaryLineItemIdentifier; }
		}

		ZString I7501Errors.Code
		{
			get { return this.ConditionCode; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return this.NarrativeText; }
		}

		bool I7501Errors.IsError
		{
			get { return !Accepted; }
		}
	}
}
