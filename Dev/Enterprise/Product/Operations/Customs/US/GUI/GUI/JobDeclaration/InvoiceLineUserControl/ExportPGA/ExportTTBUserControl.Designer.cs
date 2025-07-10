namespace Enterprise.Customs.US.GUI
{
	partial class ExportTTBUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TTBGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TTBGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TTBGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TTBGrid)).BeginInit();
			this.TTBGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.TTBLineCollection);
			// 
			// TTBGroupBox
			// 
			this.TTBGroupBox.Controls.Add(this.TTBGrid);
			this.TTBGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TTBGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TTBGroupBox.Name = "TTBGroupBox";
			this.TTBGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 430, true);
			this.TTBGroupBox.TabIndex = 1;
			this.TTBGroupBox.TabStop = false;
			this.TTBGroupBox.Text = "TTB - Alcohol and Tobacco Tax and Trade Bureau";
			// 
			// TTBGrid
			// 
			this.TTBGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TTBGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.TTBLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.TTBLine)(null)).US_NumberForIRC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.TTBLine)(null)).US_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.TTBLine)(null)).US_SerialNumber)));
			this.TTBGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_NumberForIRC";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "US_Date";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_SerialNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.TTBGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TTBGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TTBGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TTBGrid.CopySelectedRowsAllowed = true;
			this.TTBGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TTBGrid.GridId = "10131025-2352-43d9-9f8a-1dddfe9951db";
			this.TTBGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TTBGrid.LayoutKey = "TTBGrid";
			this.TTBGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TTBGrid.Name = "TTBGrid";
			this.TTBGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 411, true);
			this.TTBGrid.TabIndex = 0;
			// 
			// ExportTTBUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TTBGroupBox);
			this.Name = "ExportTTBUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TTBGroupBox.ResumeLayout(false);
			this.TTBGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TTBGrid)).EndInit();
			this.TTBGrid.ResumeLayout(false);
			this.TTBGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox TTBGroupBox;
		private ZArchitecture.ZGrid TTBGrid;
	}
}
