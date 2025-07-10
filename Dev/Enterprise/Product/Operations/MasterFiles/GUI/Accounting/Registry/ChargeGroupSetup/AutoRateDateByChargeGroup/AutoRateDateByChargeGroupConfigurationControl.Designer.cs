namespace Enterprise.MasterFiles.GUI
{
	partial class AutoRateDateByChargeGroupConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.filterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.autoRateDateByChargeGroupControl = new Enterprise.MasterFiles.GUI.AutoRateDateByChargeGroupControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AutoRateDateByChargeGroupConfiguration);
			// 
			// filterTypeDropEdit
			// 
			this.filterTypeDropEdit.AllowDrop = true;
			this.filterTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.filterTypeDropEdit, "FilterType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AutoRateDateByChargeGroupConfiguration)(null)).FilterType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.filterTypeDropEdit, false);
			this.filterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.filterTypeDropEdit.Name = "filterTypeDropEdit";
			this.filterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 20, true);
			this.filterTypeDropEdit.TabIndex = 1;
			// 
			// autoRateDateByChargeGroupControl
			// 
			this.autoRateDateByChargeGroupControl.AllowDrop = true;
			this.autoRateDateByChargeGroupControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.autoRateDateByChargeGroupControl, "AutoRateDateByChargeGroups");
			this.autoRateDateByChargeGroupControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.autoRateDateByChargeGroupControl.Name = "autoRateDateByChargeGroupControl";
			this.autoRateDateByChargeGroupControl.ReadOnly = false;
			this.autoRateDateByChargeGroupControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			this.autoRateDateByChargeGroupControl.TabIndex = 0;
			this.autoRateDateByChargeGroupControl.Visible = false;
			// 
			// AutoRateDateByChargeGroupConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.filterTypeDropEdit);
			this.Controls.Add(this.autoRateDateByChargeGroupControl);
			this.Name = "AutoRateDateByChargeGroupConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 354, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		AutoRateDateByChargeGroupControl autoRateDateByChargeGroupControl;
		internal ZArchitecture.GUI.ZDropEdit filterTypeDropEdit;
	}
}
