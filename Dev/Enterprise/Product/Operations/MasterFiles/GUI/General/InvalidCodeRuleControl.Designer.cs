namespace Enterprise.MasterFiles.GUI.General
{
	partial class InvalidCodeRuleControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.codeDescriptionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.codeDescriptionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// codeDescriptionGrid
			// 
			this.codeDescriptionGrid.AllowNavigation = false;
			this.codeDescriptionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvalidCodeRuleControl|289c82d9-41a8-4c3c-94a8-c749972095ef", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvalidCodeRuleControl|ae375928-c3b6-4b42-bce7-320aad553c6f", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.codeDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.codeDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.codeDescriptionGrid.GridId = "c17bd133-5547-4fe5-be25-db66e8f7b855";
			this.codeDescriptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codeDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.codeDescriptionGrid.LayoutKey = "CodeDescriptionRuleGrid";
			this.codeDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.codeDescriptionGrid.Name = "codeDescriptionGrid";
			this.codeDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 239, true);
			this.codeDescriptionGrid.TabIndex = 0;
			// 
			// InvalidCodeRuleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.codeDescriptionGrid);
			this.Name = "InvalidCodeRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 239, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.codeDescriptionGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid codeDescriptionGrid;
	}
}
