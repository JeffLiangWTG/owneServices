using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ZA.GUI
{
	public class OrganisationDetailsPlugIn : ZPlugIn
	{
		public OrganisationDetailsPlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.wrapper = OrgHeaderWrapper.New(organisation);
		}

		readonly OrgHeaderWrapper wrapper;

		public override string Name
		{
			get { return "South Africa"; }
		}

		protected override CargoWise.Types.ZBool HasUserControl
		{
			get { return true; }
		}

		protected override CargoWise.EntityFramework.IBusiness GetBusinessEntityForPlugIn()
		{
			return wrapper;
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new OrganisationDetailPlugInUserControl();
		}
	}
}
