using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class OrganisationFormWhsPickingPlugIn : ZPlugIn
	{
		public OrganisationFormWhsPickingPlugIn(OrgHeader client)
			: base(client)
		{
		}

		OrgHeader Client => (OrgHeader)HostBusinessEntity;

		#region Business Entity

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Client != null ? WhsClientPickingParams.GetClientPickingParams(Client) : null;
		}

		#endregion

		#region LicenceCheckPoint

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		#endregion

		#region Name

		public override string Name => (NoResString)"Picking"; // Hard-coded name

		#endregion

		#region User Control

		protected override Control GetNewUserControl()
		{
			return new WhsPickingUserControl();
		}

		protected override ZBool HasUserControl => true;

		#endregion
	}
}
