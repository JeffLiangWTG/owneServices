namespace Enterprise.MasterFiles.Business
{
	[ImportRelatedActivityPromptUserDecider("Enterprise.MarketingManager.GUI.ImportRelatedActivityPromptUserTradeDetailDecider, Enterprise.MarketingManager.GUI")]
	public interface IImportRelatedActivityTradeDetailDecider : IImportRelatedActivityDecider
	{
		ImportRelatedActivityTradeDetailDecision GetDecision(ISalesValueAssociatedEntity entity);
	}

	public sealed class ImportRelatedActivityTradeDetailDecision
	{
		public ImportRelatedActivityTradeDetailDecision(bool cancelled, OrgTradeDetail selectedTradeDetail)
		{
			Cancelled = cancelled;
			SelectedTradeDetail = selectedTradeDetail;
		}

		public bool Cancelled;
		public OrgTradeDetail SelectedTradeDetail;
	}
}
