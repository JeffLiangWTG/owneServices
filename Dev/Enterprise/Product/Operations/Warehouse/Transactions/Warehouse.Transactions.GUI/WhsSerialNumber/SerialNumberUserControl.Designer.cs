namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class SerialNumberUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SerialNumberValue";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.SerialNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SerialNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SerialNumberGrid.GridId = "4AEAAE97-002C-4FDC-A690-7898A9712CC3";
			this.SerialNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SerialNumberGrid.LayoutKey = "SerialNumberGrid";
			this.SerialNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SerialNumberGrid.Name = "SerialNumberGrid";
			this.SerialNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 510, true);
			this.SerialNumberGrid.TabIndex = 0;
			// 
			// SerialNumberUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SerialNumberGrid);
			this.Name = "SerialNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 510, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SerialNumberGrid)).EndInit();
			this.SerialNumberGrid.ResumeLayout(false);
			this.SerialNumberGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.ZGrid SerialNumberGrid;
	}
}
