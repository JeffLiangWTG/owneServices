using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class ServiceTypeDateFilterControl
	{
		public ServiceTypeDateFilterControl()
		{
			InitializeComponent();
		}

		protected void InitializeControl()
		{
			ControlDpiScalingHelper.SetTop(ref jobServiceTypeEdit, ParentStrip.FilterControlTop + ControlDpiScalingHelper.ScaleToCurrentDpiY(26), false);
		}

		public ServiceTypeDateFilterControl(ZFilterStrip parentStrip) : base(parentStrip)
		{
			InitializeComponent();
			InitializeControl();
		}
	}
}
