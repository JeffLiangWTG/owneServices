using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class ExporterSchemePluginUS : ExporterSchemePlugin
	{
		public ExporterSchemePluginUS(OrgHeader hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		protected override Control GetNewUserControlCore()
		{
			ExporterSchemeControlUS control = new ExporterSchemeControlUS();
			control.Dock = DockStyle.Fill;
			return control;
		}

		public override string Name
		{
			get { return Res.GetString("25ce81af-9592-446c-aae4-41692c2a1f25", "TSA Known Shipper"); }
		}
	}
}
