namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TestJobDeclarationValidation : JobDeclarationValidation
	{
		public TestJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			new OrganisationValidation(Parent.Factory).ValidateSCACCodeForOrganisation(Parent.JE_OH_ForwarderInfo, Core.Constants.TransportModes.Sea);
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			new OrganisationValidation(Parent.Factory).ValidateCustomsRegNoForOrganisation(Parent.JE_OH_ImporterInfo, "AAA", "LOL lol LOL lol");
		}
	}
}
