using System;

namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageWorkSheetForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 635, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 608, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonWorkSheet);
			// 
			// CartageWorkSheetForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 691, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonWorkSheet);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1060, 311, true);
			this.Name = "CartageWorkSheetForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "JobCartageRunSheetForm";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.jobCartageRunSheetMainControl1 = new Enterprise.Freight.LocalCartage.GUI.JobCartageRunSheetMainControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.jobCartageRunSheetMainControl1);
			// 
			// jobCartageRunSheetMainControl1
			// 
			this.BindingSource.SetBindingMember(this.jobCartageRunSheetMainControl1, ".");
			this.jobCartageRunSheetMainControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobCartageRunSheetMainControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.jobCartageRunSheetMainControl1.Name = "jobCartageRunSheetMainControl1";
			this.jobCartageRunSheetMainControl1.ShowCartageLegDetails = false;
			this.jobCartageRunSheetMainControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 608);
			this.jobCartageRunSheetMainControl1.TabIndex = 0;
			this.MainTabPage.ResumeLayout(true);

		}
		#endregion

		public JobCartageRunSheetMainControl jobCartageRunSheetMainControl1;
	}
}
