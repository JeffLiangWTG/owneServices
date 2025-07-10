using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CashAdvanceChargeCodeMultiSelectUserControl : ZUserControl
	{
		public CashAdvanceChargeCodeMultiSelectUserControl()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				ChargeCodeModuleButtonGrid.ShowEditButton = false;
				ChargeCodeModuleButtonGrid.ShowNewButton = false;
				ChargeCodeModuleButtonGrid.InnerGrid.ReadOnly = true;
				ChargeCodeModuleButtonGrid.Attached += ChargeCodeModuleButtonGrid_Attached;
				ChargeCodeModuleButtonGrid.Detached += ChargeCodeModuleButtonGrid_Detached;
			}
		}

		AccCashAdvanceDefaultingConfiguration Config;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Do Nothing. Handling manually.
		}

		public void Bind(AccCashAdvanceDefaultingConfiguration config)
		{
			Config = config;
			base.SetDataBinding(Config, "");
		}

		protected void ChargeCodeModuleButtonGrid_Attached(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			Config.SyncChargeCodes(e.AttachedBusinessObjects.OfType<IAccChargeCode>().Select(x => x.PK));
		}

		protected void ChargeCodeModuleButtonGrid_Detached(object sender, ModuleButtonGridOnDetachedEventArgs e)
		{
			Config.UnsyncChargeCodes(e.DetachedBusinessObjects.OfType<IAccChargeCode>().Select(x => x.PK));
		}
	}
}
