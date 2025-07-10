namespace Enterprise.MarketingManager.GUI
{
	partial class TradeLaneWithDetailsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.topSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.associatedEntitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.associatedEntitiesGrid = new Enterprise.MarketingManager.GUI.SalesAssociatedEntitiesGridControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).BeginInit();
			this.topSplitContainer.Panel2.SuspendLayout();
			this.topSplitContainer.SuspendLayout();
			this.associatedEntitiesGroupBox.SuspendLayout();
			this.associatedEntitiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesHeader);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.topSplitContainer);
			this.mainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 200, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			this.mainSplitContainer.TabIndex = 0;
			// 
			// topSplitContainer
			// 
			this.topSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topSplitContainer.Name = "topSplitContainer";
			// 
			// topSplitContainer.Panel2
			// 
			this.topSplitContainer.Panel2.Controls.Add(this.associatedEntitiesGroupBox);
			this.topSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 75, true);
			this.topSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(600);
			this.topSplitContainer.TabIndex = 0;
			// 
			// associatedEntitiesGroupBox
			// 
			this.associatedEntitiesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b69572d1-e378-4c30-bd3f-327ef1a3fb2c", "Associations");
			this.associatedEntitiesGroupBox.Controls.Add(this.associatedEntitiesGrid);
			this.associatedEntitiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.associatedEntitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.associatedEntitiesGroupBox.Name = "associatedEntitiesGroupBox";
			this.associatedEntitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 75, true);
			this.associatedEntitiesGroupBox.TabIndex = 1;
			this.associatedEntitiesGroupBox.TabStop = false;
			// 
			// associatedEntitiesGrid
			// 
			this.associatedEntitiesGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.associatedEntitiesGrid, "FilterableEntitySalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISalesValue)(((Enterprise.MarketingManager.Business.EntitySalesWrapper)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).FilterableEntitySalesCollection)).SyncRoot)))));
			this.associatedEntitiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.associatedEntitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.associatedEntitiesGrid.Name = "associatedEntitiesGrid";
			this.associatedEntitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 58, true);
			this.associatedEntitiesGrid.TabIndex = 0;
			// 
			// TradeLaneWithDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "TradeLaneWithDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer topSplitContainer;
		private Enterprise.MarketingManager.GUI.SalesAssociatedEntitiesGridControl associatedEntitiesGrid;
		private ZArchitecture.GUI.ZGroupBox associatedEntitiesGroupBox;
	}
}
