namespace Enterprise.Customs.TW.GUI
{
	partial class ContainerUserControl
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
			this.containersUserControl1.SuspendLayout();
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			this.CusContainersBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CusContainersBoundGrid
			// 
			// 
			// 
			// 
			this.CusContainersBoundGrid.InnerGrid.AllowNavigation = false;
			this.CusContainersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CusContainersBoundGrid.InnerGrid.CaptionVisible = false;
			this.CusContainersBoundGrid.InnerGrid.GridId = null;
			this.CusContainersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainersBoundGrid.InnerGrid.LayoutKey = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CusContainersBoundGrid.InnerGrid.Name = "Grid";
			this.CusContainersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 83, true);
			this.CusContainersBoundGrid.InnerGrid.TabIndex = 0;

			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusContainer);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.CusContainer)(null)).JC_IsEmptyContainer)));

			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo isPartColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.CusContainer)(null)).CO_IsPart)));
			isPartColumnStyleInfo.ColumnName = "CO_IsPart";
			isPartColumnStyleInfo.IsMandatory = true;
			isPartColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.CusContainersBoundGrid.ColumnStyles.Add(isPartColumnStyleInfo);
			// 
			// ContainerUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ContainerUserControl";
			this.containersUserControl1.ResumeLayout(true);
			this.containersUserControl1.PerformLayout();
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseAllPanel.PerformLayout();
			this.BaseContainerPanel.ResumeLayout(false);
			this.BaseContainerPanel.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			this.CusContainersBoundGrid.ResumeLayout(true);
			this.CusContainersBoundGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
