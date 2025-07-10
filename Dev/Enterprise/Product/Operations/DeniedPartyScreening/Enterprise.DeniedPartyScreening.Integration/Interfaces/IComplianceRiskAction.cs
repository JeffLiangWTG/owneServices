namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IComplianceRiskAction
	{
		void SynchronizeAndSaveIfNeeded();
		void ShowMessageIfNeeded();
		void UpdateVisibilityIfNeeded();
	}
}
