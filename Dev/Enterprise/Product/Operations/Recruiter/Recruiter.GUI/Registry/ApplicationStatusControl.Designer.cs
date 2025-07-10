namespace Enterprise.Recruiter.GUI
{
	partial class ApplicationStatusControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StatusesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.notificationEmailTemplateControl1 = new Enterprise.Registry.GUI.NotificationEmailTemplateControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusesGrid)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.ApplicationStatusCollection);
			// 
			// StatusesGrid
			// 
			this.StatusesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StatusesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.ApplicationStatus)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ApplicationStatus)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ApplicationStatus)(null)).Description)));
			this.StatusesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.StatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StatusesGrid.GridId = "277926e7-58aa-4a95-97e1-be6fa98fe83c";
			this.StatusesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StatusesGrid.LayoutKey = "zGrid1";
			this.StatusesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusesGrid.Name = "StatusesGrid";
			this.StatusesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 100, true);
			this.StatusesGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.StatusesGrid);
			this.splitContainer1.Panel1MinSize = 100;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.notificationEmailTemplateControl1);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 430, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.splitContainer1.TabIndex = 1;
			// 
			// notificationEmailTemplateControl1
			// 
			this.BindingSource.SetBindingMember(this.notificationEmailTemplateControl1, "EmailTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.NotificationEmailTemplate)(((Enterprise.Recruiter.Business.ApplicationStatus)(null)).EmailTemplate)));
			this.notificationEmailTemplateControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notificationEmailTemplateControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.notificationEmailTemplateControl1.Name = "notificationEmailTemplateControl1";
			this.notificationEmailTemplateControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 326, true);
			this.notificationEmailTemplateControl1.TabIndex = 0;
			// 
			// ApplicationStatusControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ApplicationStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusesGrid)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid StatusesGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal Enterprise.Registry.GUI.NotificationEmailTemplateControl notificationEmailTemplateControl1;
	}
}
