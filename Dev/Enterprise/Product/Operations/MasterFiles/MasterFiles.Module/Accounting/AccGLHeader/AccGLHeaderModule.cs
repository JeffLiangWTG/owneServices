using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccGLHeaderModule : ZFilterGridModule
	{
		public AccGLHeaderModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccGLHeader; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccGLHeader);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccGLHeaderFilterControl(GridCollection, (AccGLHeaderFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccGLHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccGLHeaderFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewActionMenuItems());
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("MasterFiles.AccGLHeader.DefineGeneralLedgerSections", "Define General Ledger Sections"), ShowDefineFrameworkForm));
			return menu.ToArray();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.GLAccounts;
			}
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		void ShowDefineFrameworkForm(object sender, EventArgs e)
		{
			if (!Env.Security.GLAccountsModify.IsAllowed)
			{
				Env.Security.GLAccountsModify.ShowError();
			}
			else
			{
				AccGLHeaderBulkUpdater updater = new AccGLHeaderBulkUpdater(new BusinessObjectFactory());
				ZFormModaliser.ShowDialogAndDispose(new DefineGLSectionsForm(updater));
			}
		}
	}
}
