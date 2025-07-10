namespace Enterprise.DeniedPartyScreening.GUI
{
	static class DeniedPartyScreeningEntities
	{
		internal static void AddMenuItem(DeniedPartyScreeningActionsProvider provider)
		{
			provider.AddinSeparatorActionsMenuItemIfNeeded();

			DpsCountryWideSanctions.AddActionsMenuItem(provider);

			DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
		}
	}
}
