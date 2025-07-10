using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class AutoratingDateFilteringUserControl
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
			this.filterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl = new AutoratingDateFilteringChargeGroupAndCustomizedUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrgHeader);
			// 
			// filterTypeDropEdit
			// 
			this.filterTypeDropEdit.AllowDrop = true;
			this.filterTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.filterTypeDropEdit, "MiscServ+OM_AutoratingDateFiltering");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeader)(null)).MiscServ.OM_AutoratingDateFiltering)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.filterTypeDropEdit, false);
			this.filterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.filterTypeDropEdit.Name = "filterTypeDropEdit";
			this.filterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 20, true);
			this.filterTypeDropEdit.TabIndex = 1;
			// 
			// autoRateDateByChargeGroupControl
			// 
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.AllowDrop = true;
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.autoratingDateFilteringChargeGroupAndCustomizedUserControl, "MiscServ+RatingDateConfigByChargeGroupConfiguration+RatingDateConfigByChargeGroups");
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.Name = "autoRateDateByChargeGroupControl";
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.TabIndex = 0;
			this.autoratingDateFilteringChargeGroupAndCustomizedUserControl.Visible = false;
			// 
			// AutoRateDateByChargeGroupConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.filterTypeDropEdit);
			this.Controls.Add(this.autoratingDateFilteringChargeGroupAndCustomizedUserControl);
			this.Name = "AutoRateDateByChargeGroupConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 354, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private AutoratingDateFilteringChargeGroupAndCustomizedUserControl autoratingDateFilteringChargeGroupAndCustomizedUserControl;
		private ZArchitecture.GUI.ZDropEdit filterTypeDropEdit;
	}
}
