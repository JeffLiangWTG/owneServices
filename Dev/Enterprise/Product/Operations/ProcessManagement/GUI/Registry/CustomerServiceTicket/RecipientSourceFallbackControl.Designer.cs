namespace Enterprise.ProcessManagement.GUI
{
	partial class RecipientSourceFallbackControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FallbackSourcesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FallbackSourcesGrid)).BeginInit();
			this.FallbackSourcesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.RecipientSourceFallbackHeader);
			// 
			// FallbackSourcesGrid
			// 
			this.FallbackSourcesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FallbackSourcesGrid, "SourceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.RecipientSourceFallbackHeader)(null)).SourceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ProcessManagement.Business.RecipientSource)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.RecipientSourceFallbackHeader)(null)).SourceCollection)).SyncRoot)).FallbackSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.RecipientSource)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.RecipientSourceFallbackHeader)(null)).SourceCollection)).SyncRoot)).SourceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.RecipientSource)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.RecipientSourceFallbackHeader)(null)).SourceCollection)).SyncRoot)).SourceTypeDescription)));
			this.FallbackSourcesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "FallbackSequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "SourceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "SourceTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.FallbackSourcesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FallbackSourcesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FallbackSourcesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FallbackSourcesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FallbackSourcesGrid.GridId = "fc95dab3-a29f-4065-bcf6-556abd6b7c3e";
			this.FallbackSourcesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FallbackSourcesGrid.LayoutKey = "FallbackSourcesGrid";
			this.FallbackSourcesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FallbackSourcesGrid.Name = "FallbackSourcesGrid";
			this.FallbackSourcesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			this.FallbackSourcesGrid.TabIndex = 0;
			// 
			// RecipientSourceFallbackControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FallbackSourcesGrid);
			this.Name = "RecipientSourceFallbackControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FallbackSourcesGrid)).EndInit();
			this.FallbackSourcesGrid.ResumeLayout(false);
			this.FallbackSourcesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid FallbackSourcesGrid;
	}
}
