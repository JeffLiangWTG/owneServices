using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Integration;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	public class GlobalCommercialInvoicePlugin : ZPlugIn
	{
		public GlobalCommercialInvoicePlugin(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			pluginBizO ??= new GlobalCommercialInvoicePluginBusinessObject(HostBusinessEntity);
		}

		public override string Name => Constants.PluginName;

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		readonly GlobalCommercialInvoicePluginBusinessObject pluginBizO;

		protected override Control GetNewUserControl()
		{
			return pluginUserControl ??= new GlobalCommercialInvoicePluginUserControl(pluginBizO) { Dock = DockStyle.Fill };
		}
		GlobalCommercialInvoicePluginUserControl pluginUserControl;
	}
}
