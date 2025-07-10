using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.GUI
{
	public class OrganisationPlugIn : ZAlwaysLoadPlugIn
	{
		public OrganisationPlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = OrgHeaderWrapper.New(organisation);
		}

		readonly OrgHeaderWrapper organisation;

		public override string Name
		{
			get { return "Customs Messaging"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new OrganisationPlugInUserControl();
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new OrganisationPlugInMenu(organisation, delegate
				{
					Env.Licence.ImportBroker.Login(this);
				});
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return organisation;
		}
	}
}
