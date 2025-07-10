using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportCommon.GUI
{
	public class TransportModuleButtonGrid : ZModuleButtonGridWithBlankDetachMessage
	{
		public TransportModuleButtonGrid()
		{
			DisableButtons();
		}

		#region AllowDoubleClick

		protected override bool AllowDoubleClick
		{
			get { return true; }
		}

		#endregion

		#region DisableButtons

		void DisableButtons()
		{
			ShowAttachButton = false;
			ShowDetachButton = false;
			ShowEditButton = false;
			ShowNewButton = false;
			toolStrip.Visible = false;
			mainLayoutPanel.RowCount = 1; // This removes space added by ToolStrip control
		}

		#endregion
	}
}
