using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ClientIntelligenceFormForTest : ZClientIntelligenceForm
	{
		public ClientIntelligenceFormForTest() : base()
		{
		}

		public ClientIntelligenceFormForTest(OrgHeader organization) : base(organization)
		{
		}

		public new ZArchitecture.GUI.ZTemplateTabControl OrganisationsTabControl
		{
			get { return base.OrganisationsTabControl; }
		}
	}
}
