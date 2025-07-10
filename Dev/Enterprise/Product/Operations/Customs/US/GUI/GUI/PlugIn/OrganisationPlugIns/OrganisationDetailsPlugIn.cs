using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.GUI
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
			get { return "USA"; }
		}

		protected override CargoWise.Types.ZBool HasUserControl
		{
			get { return true; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return wrapper;
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new OrganisationDetailPlugInUserControl();
		}
	}
}
