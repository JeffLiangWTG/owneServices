namespace Enterprise.MarketingManager.GUI
{
	partial class WarehouseTradeDetailsControl
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
		void InitializeComponent()
		{
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.topSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.associatedEntitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.associatedEntitiesGrid = new Enterprise.MarketingManager.GUI.SalesAssociatedEntitiesGridControl();
			this.jobCommonTradeDetailFieldsControl = new Enterprise.MarketingManager.GUI.JobCommonTradeDetailFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).BeginInit();
			this.topSplitContainer.Panel2.SuspendLayout();
			this.topSplitContainer.SuspendLayout();
			this.associatedEntitiesGroupBox.SuspendLayout();
			this.associatedEntitiesGrid.SuspendLayout();
			this.jobCommonTradeDetailFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("26834108-3472-40f1-a382-fb02f55259d7", "Details");
			this.detailsGroupBox.Controls.Add(this.mainSplitContainer);
			this.detailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 260, true);
			this.detailsGroupBox.TabIndex = 10;
			this.detailsGroupBox.TabStop = false;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.topSplitContainer);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.AutoScroll = true;
			this.mainSplitContainer.Panel2.Controls.Add(this.associatedEntitiesGroupBox);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 243, true);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(547);
			this.mainSplitContainer.TabIndex = 7;
			// 
			// topSplitContainer
			// 
			this.topSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.topSplitContainer.IsSplitterFixed = true;
			this.topSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topSplitContainer.Name = "topSplitContainer";
			this.topSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// topSplitContainer.Panel2
			// 
			this.topSplitContainer.Panel2.AutoScroll = true;
			this.topSplitContainer.Panel2.Controls.Add(this.jobCommonTradeDetailFieldsControl);
			this.topSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(125);
			this.topSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 243, true);
			this.topSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(107);
			this.topSplitContainer.TabIndex = 8;
			// 
			// associatedEntitiesGroupBox
			// 
			this.associatedEntitiesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("037df920-8abe-4576-9dc5-51a844bf47a6", "Associations");
			this.associatedEntitiesGroupBox.Controls.Add(this.associatedEntitiesGrid);
			this.associatedEntitiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.associatedEntitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.associatedEntitiesGroupBox.Name = "associatedEntitiesGroupBox";
			this.associatedEntitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 243, true);
			this.associatedEntitiesGroupBox.TabIndex = 0;
			this.associatedEntitiesGroupBox.TabStop = false;
			// 
			// associatedEntitiesGrid
			// 
			this.associatedEntitiesGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.associatedEntitiesGrid, "EntityTradeDetailsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISalesValue)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)))));
			this.associatedEntitiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.associatedEntitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.associatedEntitiesGrid.Name = "associatedEntitiesGrid";
			this.associatedEntitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 227, true);
			this.associatedEntitiesGrid.TabIndex = 1;
			// 
			// jobCommonTradeDetailFieldsControl
			// 
			this.jobCommonTradeDetailFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jobCommonTradeDetailFieldsControl, "EntityTradeDetailsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgTradeDetail)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(null)).EntityTradeDetailsCollection)).SyncRoot)))));
			this.jobCommonTradeDetailFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobCommonTradeDetailFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jobCommonTradeDetailFieldsControl.Name = "jobCommonTradeDetailFieldsControl";
			this.jobCommonTradeDetailFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 134, true);
			this.jobCommonTradeDetailFieldsControl.TabIndex = 0;
			// 
			// WarehouseTradeDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.detailsGroupBox);
			this.Name = "WarehouseTradeDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 260, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.topSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).EndInit();
			this.topSplitContainer.ResumeLayout(false);
			this.topSplitContainer.PerformLayout();
			this.associatedEntitiesGroupBox.ResumeLayout(false);
			this.associatedEntitiesGroupBox.PerformLayout();
			this.associatedEntitiesGrid.ResumeLayout(true);
			this.associatedEntitiesGrid.PerformLayout();
			this.jobCommonTradeDetailFieldsControl.ResumeLayout(true);
			this.jobCommonTradeDetailFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;
		private JobCommonTradeDetailFieldsControl jobCommonTradeDetailFieldsControl;
		internal CargoWise.Windows.UI.KSplitContainer topSplitContainer;
		private Enterprise.MarketingManager.GUI.SalesAssociatedEntitiesGridControl associatedEntitiesGrid;
		private ZArchitecture.GUI.ZGroupBox associatedEntitiesGroupBox;
	}
}
