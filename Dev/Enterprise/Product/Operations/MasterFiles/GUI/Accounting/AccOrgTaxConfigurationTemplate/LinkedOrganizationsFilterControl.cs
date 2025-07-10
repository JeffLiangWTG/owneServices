using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class LinkedOrganizationsFilterControl : ZFilterStripControl
	{
		public LinkedOrganizationsFilterControl(FilterStripBusinessObject filterBusinessObject) : base(null, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(200);

		public void UpdateRecordsFoundLabel(string newMessage, bool isHighlighted)
		{
			ToolStripRecordsFoundLabel.ForeColor = isHighlighted ? Color.Red : Color.Blue;
			ToolStripRecordsFoundLabel.Text = newMessage;
		}
	}
}
