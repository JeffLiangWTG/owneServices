using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class ExporterSchemePluginUK : ExporterSchemePlugin
	{
		public ExporterSchemePluginUK(OrgHeader parentOrganisation) : base(parentOrganisation)
		{
		}

		protected override Control GetNewUserControlCore()
		{
			return new ExporterSchemeControlUK();
		}

		public override string Name => Res.GetString("77cd283d-e94d-4ec3-ba4e-c4bce08ff395", "Supply Chain Security (UK)");
	}
}
