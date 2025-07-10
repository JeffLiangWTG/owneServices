namespace Enterprise.eTail.GUI
{
	partial class UpdateHVLVStatusUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.StatusCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.GUI.UpdateHVLVStatusApplicator);
			// 
			// StatusCodeDropEdit
			// 
			this.StatusCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusCodeDropEdit, "StatusCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.GUI.UpdateHVLVStatusApplicator)(null)).StatusCode)));
			this.StatusCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 2, true);
			this.StatusCodeDropEdit.Name = "StatusCodeDropEdit";
			this.StatusCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.StatusCodeDropEdit.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "HVLV Status";
			// 
			// UpdateHVLVStatusUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.StatusCodeDropEdit);
			this.Name = "UpdateHVLVStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit StatusCodeDropEdit;
		private ZArchitecture.ZLabel zLabel1;
	}
}
