using System.Windows.Forms.Design;

namespace Enterprise.Warehouse.Transit.GUI
{
	#if !WINZOR
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
	#endif
	partial class CFSDetailsToolStripButton
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// CFSDetailsToolStripButton
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 10, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 64, true);
		}

		#endregion
	}
}
