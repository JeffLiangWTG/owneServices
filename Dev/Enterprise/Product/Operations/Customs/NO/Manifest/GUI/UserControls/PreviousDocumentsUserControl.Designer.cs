namespace Enterprise.Customs.NO.Manifest.GUI
{
	partial class PreviousDocumentsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.PreviousDocumentsGrid.CaptionVisible = false;
			zCodeDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeDropEditColumnStyleInfo.ColumnName = "CSI_Code";
			zCodeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			this.PreviousDocumentsGrid.ColumnStyles.Add(zCodeDropEditColumnStyleInfo);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsGrid.GridId = "B0B93892-5D46-4D9C-92BF-8FAEBA092C96";
			this.PreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousDocumentsGrid.LayoutKey = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsGrid.Name = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			this.PreviousDocumentsGrid.TabIndex = 0;
			// 
			// PreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousDocumentsGrid);
			this.Name = "PreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid PreviousDocumentsGrid;
	}
}
