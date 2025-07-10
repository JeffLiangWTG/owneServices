namespace Enterprise.Freight.CFS
{
	public interface ILabelDocumentSupporter
	{
		bool SupportImportLabels { get; }
		bool SupportOnForwardingLabels { get; }
		bool SupportTranshipmentLabels { get; }

		string ImportLabelErrorMessage { get; }
		string OnForwardingErrorMessage { get; }
		string TranshipmentErrorMessage { get; }
	}
}
