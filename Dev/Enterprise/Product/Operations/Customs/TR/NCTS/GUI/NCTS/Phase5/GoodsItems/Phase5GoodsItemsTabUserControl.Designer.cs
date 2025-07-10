namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class Phase5GoodsItemsTabUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components;

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
			this.components = new System.ComponentModel.Container();
			this.GoodsItemExportToOpenTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemExportToOpenTabUserControl = new Enterprise.Customs.TR.NCTS.GUI.Phase5GoodsItemExportToOpenTabUserControl();
			this.GoodsItemTabControl.SuspendLayout();
			this.GoodsItemExportToOpenTabUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsDepartureCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc>);
			// 
			// GoodsItemTabControl
			// 
			this.GoodsItemTabControl.Controls.Remove(this.GoodsItemSupplyChainActorsTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemExportToOpenTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemSupplyChainActorsTabPage);
			// 
			// GoodsItemExportToOpenTabPage
			//
			this.GoodsItemExportToOpenTabPage.CaptionResourceString = Res.GetData("B42F30A8-E144-4C95-8AA0-0BF0C1569FC9", "Export To Open");
			this.GoodsItemExportToOpenTabPage.Controls.Add(this.GoodsItemExportToOpenTabUserControl);
            this.GoodsItemExportToOpenTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.GoodsItemExportToOpenTabPage.Name = "GoodsItemExportToOpenTabPage";
            this.GoodsItemExportToOpenTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.GoodsItemExportToOpenTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 303, true);
            this.GoodsItemExportToOpenTabPage.TabIndex = 2;
            this.GoodsItemExportToOpenTabPage.UseVisualStyleBackColor = true;
			// 
			// GoodsItemExportToOpenTabUserControl
			// 
			this.GoodsItemExportToOpenTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemExportToOpenTabUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TR.NCTS.Business.NctsExportToOpenCollection)(((TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)));
			this.GoodsItemExportToOpenTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemExportToOpenTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemExportToOpenTabUserControl.Name = "GoodsItemExportToOpenTabUserControl";
			this.GoodsItemExportToOpenTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 299, true);
			// 
			// Phase5GoodsItemsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "Phase5GoodsItemsTabUserControl";
            this.ShouldSerializeTabPageMethods = false;
            this.GoodsItemTabControl.ResumeLayout(false);
            this.GoodsItemTabControl.PerformLayout();
			this.GoodsItemExportToOpenTabUserControl.ResumeLayout(true);
			this.GoodsItemExportToOpenTabUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		#endregion

		public ZArchitecture.GUI.ZTabPage GoodsItemExportToOpenTabPage;
		public Phase5GoodsItemExportToOpenTabUserControl GoodsItemExportToOpenTabUserControl;
	}
}
