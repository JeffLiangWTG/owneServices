namespace Enterprise.Customs.US.LVS.GUI
{
	partial class DisclaimApplicablePGAsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.gridPGARequirements = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridPGARequirements)).BeginInit();
			this.gridPGARequirements.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.GUI.DisclaimApplicablePGAsApplicator);
			// 
			// gridPGARequirements
			// 
			this.gridPGARequirements.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridPGARequirements, "DisclaimOptions");
			this.gridPGARequirements.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ba7a9df1-49b9-48f5-9b33-6866d5261d58", "Agency");
			zTextBoxColumnStyleInfo1.ColumnName = "AgencyCodeWithDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(447);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("5e5535f4-b180-44d0-9338-f656517f16a7", "Disclaim Reason");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "DisclaimReason";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			this.gridPGARequirements.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridPGARequirements.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.gridPGARequirements.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridPGARequirements.GridId = "551d87bb-1c26-4590-88fd-4738552efd2b";
			this.gridPGARequirements.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridPGARequirements.LayoutKey = "gridPGARequirements";
			this.gridPGARequirements.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gridPGARequirements.Name = "gridPGARequirements";
			this.gridPGARequirements.TabIndex = 0;
			// 
			// DisclaimApplicablePGAsOperationActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.gridPGARequirements);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 395, true);
			this.Name = "DisclaimAllPGAsOperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 395, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridPGARequirements)).EndInit();
			this.gridPGARequirements.ResumeLayout(false);
			this.gridPGARequirements.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid gridPGARequirements;
	}
}
