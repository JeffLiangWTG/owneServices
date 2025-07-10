namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	partial class OrderMilestonesUserControl
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
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			this.ProcessTasksGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProcessTasksGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.Order);
			// 
			// ProcessTasksGrid
			// 
			this.ProcessTasksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProcessTasksGrid, "MilestoneView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).MilestoneView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).MilestoneView)).SyncRoot)).P9_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).MilestoneView)).SyncRoot)).P9_ScheduledDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).MilestoneView)).SyncRoot)).P9_ActualDateForBinding)));
			this.ProcessTasksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P9_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.ColumnName = "P9_ScheduledDateForBinding";
			zDateEditColumnStyleInfo2.ColumnName = "P9_ActualDateForBinding";
			this.ProcessTasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProcessTasksGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ProcessTasksGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ProcessTasksGrid.GridId = "81d4b27c-edd7-45ad-a0f0-cef83e393fa0";
			this.ProcessTasksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessTasksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProcessTasksGrid.LayoutKey = "ProcessTasksGrid";
			this.ProcessTasksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcessTasksGrid.Name = "ProcessTasksGrid";
			this.ProcessTasksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 298, true);
			this.ProcessTasksGrid.TabIndex = 0;
			// 
			// OrderMilestonesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProcessTasksGrid);
			this.Name = "OrderMilestonesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 298, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProcessTasksGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ProcessTasksGrid;
	}
}
