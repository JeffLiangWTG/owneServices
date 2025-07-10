namespace Enterprise.Customs.US.GUI
{
	partial class MO3UserControl
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
			this.MO3GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MO3Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MO3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO3Grid)).BeginInit();
			this.MO3Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// MO3GroupBox
			// 
			this.MO3GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("53137dea-2028-4a01-8d68-7b0f2b8f4b31", "MO3 Details");
			this.MO3GroupBox.Controls.Add(this.MO3Grid);
			this.MO3GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MO3GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MO3GroupBox.Name = "MO3GroupBox";
			this.MO3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			this.MO3GroupBox.TabIndex = 0;
			this.MO3GroupBox.TabStop = false;
			this.MO3GroupBox.Text = "MO3 Details";
			// 
			// MO3Grid
			// 
			this.MO3Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MO3Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_AuthorizationNumber)));
			this.MO3Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2ed297dc-ee43-45b8-82a6-a4d60d58685e", "Exemption Authorization Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_AuthorizationNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.MO3Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MO3Grid.CopySelectedRowsAllowed = true;
			this.MO3Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MO3Grid.GridId = "151c2a9e-4ccf-4eb1-8846-ad4892e1d954";
			this.MO3Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MO3Grid.LayoutKey = "MO3Grid";
			this.MO3Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MO3Grid.Name = "MO3Grid";
			this.MO3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 267, true);
			this.MO3Grid.TabIndex = 0;
			// 
			// MO3UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MO3GroupBox);
			this.Name = "MO3UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MO3GroupBox.ResumeLayout(false);
			this.MO3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO3Grid)).EndInit();
			this.MO3Grid.ResumeLayout(false);
			this.MO3Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MO3GroupBox;
		public ZArchitecture.ZGrid MO3Grid;
	}
}
