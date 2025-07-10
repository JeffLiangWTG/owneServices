using System.Windows.Forms;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class PortCallRequestFilterControl : ZFilterStripControl
	{
		public PortCallRequestFilterControl(PortCallResponseCollection responses, PortCallRequestFilterStripBusinessObject filterBizo)
			: base(responses, filterBizo)
		{
			InitializeComponent();
			AddStripButton.Visible = false;
		}

		protected override bool ShouldAddEmptyFilterStripOnReset => false;

		protected override void BindCore()
		{
			base.BindCore();

			FilteredGrid.DoubleClick += delegate
			{
				if (FilteredGrid.SelectedElements.Length > 0)
				{
					var parentForm = FindForm();
					if (parentForm != null)
					{
						parentForm.DialogResult = DialogResult.OK;
						parentForm.Close();
					}
				}
			};
		}
	}
}
