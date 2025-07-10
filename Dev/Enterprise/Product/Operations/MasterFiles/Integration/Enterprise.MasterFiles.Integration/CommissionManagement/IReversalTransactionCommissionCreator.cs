namespace Enterprise.MasterFiles.Integration
{
	public interface IReversalTransactionCommissionCreator : ICommissionCreator
	{
		void CreateReversalCommissions(IAccCommissionHeader originalCommissionHeader);
	}
}
