using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationLookups : SGAddInfoLookups
	{
		public AddInfoJobDeclarationLookups(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		public new AddInfoJobDeclaration Parent => (AddInfoJobDeclaration)base.Parent;

		public OrgHeaderCollection Organisations => organisations ?? (organisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisations;

		public ForwarderCollection Forwarders => forwarders ?? (forwarders = new ForwarderCollection(Factory));
		ForwarderCollection forwarders;
	}
}
