using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class SerialNumberSelectorUserControl
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			SelectSerialNumberCheckBox = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			this.SerialNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SerialNumberGrid)).BeginInit();
			this.SerialNumberGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SerialNumberGrid
			// 
			this.SerialNumberGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SerialNumberGrid, ".");
			this.SerialNumberGrid.CaptionVisible = false;
			SelectSerialNumberCheckBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			SelectSerialNumberCheckBox.ColumnName = "Selected";
			zTextBoxColumnStyleInfo.ColumnName = "SerialNumberValue";
			zTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.SerialNumberGrid.ColumnStyles.Add(SelectSerialNumberCheckBox);
			this.SerialNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			this.SerialNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SerialNumberGrid.GridId = "5683499A-9FC4-4926-B19C-5D33AF6E1768";
			this.SerialNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SerialNumberGrid.LayoutKey = "SerialNumberGrid";
			this.SerialNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SerialNumberGrid.Name = "SerialNumberGrid";
			this.SerialNumberGrid.TabIndex = 0;
			// 
			// SerialNumberSelectorUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SerialNumberGrid);
			this.Name = "SerialNumberSelectorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SerialNumberGrid)).EndInit();
			this.SerialNumberGrid.ResumeLayout(false);
			this.SerialNumberGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZCheckBoxColumnStyleInfo SelectSerialNumberCheckBox;
		public ZArchitecture.ZGrid SerialNumberGrid;
	}
}
