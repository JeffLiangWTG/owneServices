using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ActiveUsersModule : ZEmbeddedModule
	{
		public ActiveUsersModule()
			: base()
		{
		}

		protected override Control GetNewEmbeddedControl()
		{
			Control result = new ActiveUsersUserControl(new ActiveUsersModuleBusinessObject(new BusinessObjectFactory()));
			result.Dock = DockStyle.Fill;
			return result;
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ActiveUsers; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ActiveUser; }
		}
	}
}
