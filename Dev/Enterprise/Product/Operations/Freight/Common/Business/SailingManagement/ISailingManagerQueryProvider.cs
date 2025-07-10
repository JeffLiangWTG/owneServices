namespace Enterprise.Freight.Common.Business
{
	public interface ISailingManagerQueryProvider
	{
		SailingManagerUpdateMode QueryFreshMatchBehaviour(QueryFreshMatchBehaviourArgs queryArgs);

		bool SelectedByUser { get; }
	}
}
