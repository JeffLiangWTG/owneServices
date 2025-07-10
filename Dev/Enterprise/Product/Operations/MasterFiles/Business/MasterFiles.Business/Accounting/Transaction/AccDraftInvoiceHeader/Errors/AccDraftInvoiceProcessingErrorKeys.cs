namespace Enterprise.MasterFiles.Business
{
	public static partial class AccDraftInvoiceProcessingErrors
	{
		/// <summary>
		/// Codes representing errors that may occur during AP Reconciliation and posting process.
		/// </summary>
		public static class Keys
		{
			public const string NoAccrual = "NAF";
			public const string NoSuitableCombinationOfAccrualsFound = "NAM";
			public const string MultipleCombinationsOfAccrualsFound = "MAC";
			public const string ReconciliationTimeout = "TMO";
			public const string ForeignCurrency = "FCR";
			public const string DisabledNegativeAccrualBehaviour = "NAD";
		}

		/// <summary>
		/// Codes representing contexts. A context explains at which stage of AP Reconciliation and posting process an error occurred.
		/// </summary>
		public static class Context
		{
			public const string AutoAPReconciliation = "ARC";
			public const string ManualAPReconciliation = "MRC";
			public const string PostingAfterAutoAPReconciliation = "PAR";
			public const string PostingAfterManualAPReconciliation = "PMR";
			public const string SavingDraftInvoiceAfterParsing = "SDI";
		}
	}
}
