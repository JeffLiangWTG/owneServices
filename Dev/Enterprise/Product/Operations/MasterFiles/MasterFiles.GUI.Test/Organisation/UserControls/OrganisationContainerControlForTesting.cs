namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrganisationContainerControlForTesting : OrganisationContainerControl
	{
		protected override void ToggleClientIntelligenceForm()
		{
			ClientIntelligenceFormToggled = true;
		}
		public bool ClientIntelligenceFormToggled;

		protected override void ToggleCompetitorIntelligenceForm()
		{
			CompetitorIntelligenceFormToggled = true;
		}
		public bool CompetitorIntelligenceFormToggled;
	}
}
