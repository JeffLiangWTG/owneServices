using Enterprise.ComplianceRisk.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	class DeniedPartyScreeningJobs
	{
		public static void AddMenuItem(DeniedPartyScreeningActionsProvider provider)
		{
			if (provider.ComplianceRiskEnabled)
			{
				if (provider.IsModuleActionsMenuItem)
				{
					provider.AddinSeparatorActionsMenuItemIfNeeded();
					ComplianceRiskStatusFormSynchronizer.AddModuleActionsMenuItem(provider);
					ComplianceRiskStatusFormViewPartyRisk.AddModuleActionsMenuItem(provider);
				}
			}
			else
			{
				if (provider.IsFormActionsMenuItem)
				{
					provider.AddinSeparatorActionsMenuItemIfNeeded();
					new DpsMarkJobScreeningStatusClear().AddActionsMenuItem(provider);
				}
			}
		}
	}
}
