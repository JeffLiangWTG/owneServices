namespace Enterprise.Customs.US.GUI
{
	partial class ExportDEAUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DEAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DEAGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DEAGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DEAGrid)).BeginInit();
			this.DEAGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.DEAHeaderCollection);
			// 
			// DEAGroupBox
			// 
			this.DEAGroupBox.Controls.Add(this.DEAGrid);
			this.DEAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DEAGroupBox.Name = "DEAGroupBox";
			this.DEAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 400, true);
			this.DEAGroupBox.TabIndex = 1;
			this.DEAGroupBox.TabStop = false;
			this.DEAGroupBox.Text = "DEA - Drug Enforcement Administration";
			// 
			// DEAGrid
			// 
			this.DEAGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DEAGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DEAHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_DrugCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_UnitOfMeasure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_PermitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_RegistrationNumber)));
			this.DEAGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_DrugCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "US_Weight";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_UnitOfMeasure";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_PermitNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_RegistrationNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.DEAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DEAGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DEAGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DEAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DEAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DEAGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAGrid.GridId = "efefd520-ee97-47ea-9cdd-cb83a645b89b";
			this.DEAGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DEAGrid.LayoutKey = "DEAGrid";
			this.DEAGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DEAGrid.Name = "DEAGrid";
			this.DEAGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 381, true);
			this.DEAGrid.TabIndex = 0;
			// 
			// ExportDEAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DEAGroupBox);
			this.Name = "ExportDEAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DEAGroupBox.ResumeLayout(false);
			this.DEAGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DEAGrid)).EndInit();
			this.DEAGrid.ResumeLayout(false);
			this.DEAGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DEAGroupBox;
		private ZArchitecture.ZGrid DEAGrid;
	}
}
