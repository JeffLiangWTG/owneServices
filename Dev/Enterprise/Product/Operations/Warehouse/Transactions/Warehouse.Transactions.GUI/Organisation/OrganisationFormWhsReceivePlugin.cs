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
	public partial class OrganisationFormWhsReceivePlugin : ZPlugIn
	{
		public OrganisationFormWhsReceivePlugin(OrgHeader org)
			: base(org)
		{
			this.org = org;
		}

		public override string Name => (NoResString)"Receive"; // Hard-coded name

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override Control GetNewUserControl()
		{
			return new WhsReceiveUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (clientParams == null)
			{
				clientParams = WhsClientParams.GetClientParams(org);
				org.RegisterEditableChildObject(clientParams);
			}
			return clientParams;
		}

		protected override ZBool HasUserControl => true;

		readonly OrgHeader org;
		WhsClientParams clientParams;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrganisationFormWhsReceivePlugin
	{
		public OrgHeader OrgForTest => org;
		public LicenceCheckpoint LicenceCheckPointForTest => LicenceCheckPoint;
		public bool HasUserControlForTest => HasUserControl;
		public Control GetNewUserControlForTest() => GetNewUserControl();
		public WhsClientParams GetBusinessEntityForPlugInForTest() => (WhsClientParams)GetBusinessEntityForPlugIn();
	}
}

#endif
#endregion
