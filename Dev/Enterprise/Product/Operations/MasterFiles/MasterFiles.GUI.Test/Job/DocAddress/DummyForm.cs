using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public partial class DummyForm : ZForm
	{
		public DummyForm(JobDocAddressParentForTesting host)
			: base(host)
		{
			this.Host = host;
			PlugIns.Add(ControllerIDs.DocAddresses);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			Size = new System.Drawing.Size(ControlDpiScalingHelper.ScaleToCurrentDpiX(900), ControlDpiScalingHelper.ScaleToCurrentDpiY(600));
		}

		internal protected override ZTabControl TopLevelTabControl
		{
			get { return TabControl; }
		}

		protected JobDocAddressParentForTesting Host;
	}
}
