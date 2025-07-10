using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgFormForTest : ZOrganisationsForm
	{
		public OrgFormForTest(OrgHeader organisation) : base(organisation)
		{
		}

		public ZTemplateTabControl OrgTabControl
		{
			get { return OrganisationsTabControl; }
		}
	}
}
