namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SPTSContainerTabUserControl
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
			this.SPTSHeaderContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SPTSHeaderContainersGrid)).BeginInit();
			this.SPTSHeaderContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSContainer);
			// 
			// SPTSHeaderContainersGrid
			// 
			this.SPTSHeaderContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SPTSHeaderContainersGrid, "HeaderContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).HeaderContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSContainer)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).HeaderContainers)).SyncRoot)).BC_ContainerNum)));
			this.SPTSHeaderContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "BC_ContainerNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.SPTSHeaderContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SPTSHeaderContainersGrid.CopySelectedRowsAllowed = true;
			this.SPTSHeaderContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SPTSHeaderContainersGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.SPTSHeaderContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SPTSHeaderContainersGrid.LayoutKey = "zGrid1";
			this.SPTSHeaderContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SPTSHeaderContainersGrid.Name = "SPTSHeaderContainersGrid";
			this.SPTSHeaderContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 200, true);
			this.SPTSHeaderContainersGrid.TabIndex = 1;
			// 
			// SPTSContainerTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SPTSHeaderContainersGrid);
			this.Name = "SPTSContainerTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SPTSHeaderContainersGrid)).EndInit();
			this.SPTSHeaderContainersGrid.ResumeLayout(false);
			this.SPTSHeaderContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid SPTSHeaderContainersGrid;
	}
}
