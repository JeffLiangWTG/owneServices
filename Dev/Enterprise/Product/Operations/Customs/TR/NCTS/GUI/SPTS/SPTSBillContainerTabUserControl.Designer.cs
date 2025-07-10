namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SPTSBillContainerTabUserControl
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
			this.SPTSBillContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SPTSBillContainersGrid)).BeginInit();
			this.SPTSBillContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSContainer);
			// 
			// SPTSBillContainersGrid
			// 
			this.SPTSBillContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SPTSBillContainersGrid, "Bills.SPTSBillContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSBill)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)).SyncRoot)).SPTSBillContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSContainer)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSBill)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)).SyncRoot)).SPTSBillContainers)).SyncRoot)).BC_ContainerNum)));
			this.SPTSBillContainersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "BC_ContainerNum";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.SPTSBillContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SPTSBillContainersGrid.CopySelectedRowsAllowed = true;
			this.SPTSBillContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SPTSBillContainersGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.SPTSBillContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SPTSBillContainersGrid.LayoutKey = "zGrid1";
			this.SPTSBillContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SPTSBillContainersGrid.Name = "SPTSBillContainersGrid";
			this.SPTSBillContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 200, true);
			this.SPTSBillContainersGrid.TabIndex = 1;
			// 
			// SPTSContainerTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SPTSBillContainersGrid);
			this.Name = "SPTSContainerTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SPTSBillContainersGrid)).EndInit();
			this.SPTSBillContainersGrid.ResumeLayout(false);
			this.SPTSBillContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid SPTSBillContainersGrid;
	}
}
