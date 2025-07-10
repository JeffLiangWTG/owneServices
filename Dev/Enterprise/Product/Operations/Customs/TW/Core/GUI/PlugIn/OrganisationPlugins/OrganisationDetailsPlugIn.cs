using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.TW.GUI
{
	public class OrganisationDetailsPlugIn : ZPlugIn
	{
		public OrganisationDetailsPlugIn(OrgHeader orgHeader) : base(orgHeader)
		{
			this.wrapper = OrgHeaderWrapper.New(orgHeader);
		}

		readonly OrgHeaderWrapper wrapper;

		protected override CargoWise.Types.ZBool HasUserControl => true;

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new OrganisationDetailPlugInUserControl();
		}

		protected override CargoWise.EntityFramework.IBusiness GetBusinessEntityForPlugIn()
		{
			return wrapper;
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		public override string Name => (NoResString)"Taiwan";
	}
}
