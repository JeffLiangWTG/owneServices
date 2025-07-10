namespace Enterprise.Freight.LocalCartage.Business
{
	public interface IRunSheetSecurityQueryProvider
	{
		void TryAuthorise(CommonCartageLeg leg);
	}
}
