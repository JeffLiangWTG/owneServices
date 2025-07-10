namespace Enterprise.eTail.GUI
{
	partial class UpdateHVLVItemStatusUserControl
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
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.STUEventDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
            this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.StatusCodeDropEdit.SuspendLayout();
            this.STUEventDateTimeOffsetEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.eTail.GUI.UpdateHVLVItemStatusApplicator);
            // 
            // StatusCodeDropEdit
            // 
            this.StatusCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatusCodeDropEdit, "StatusCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.GUI.UpdateHVLVItemStatusApplicator)(null)).StatusCode)));
            this.StatusCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 2, true);
            this.StatusCodeDropEdit.Name = "StatusCodeDropEdit";
            this.StatusCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
            this.StatusCodeDropEdit.TabIndex = 0;
            // 
            // zLabel1
            // 
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
            this.zLabel1.TabIndex = 1;
            this.zLabel1.Text = "HVLV Item Status";
            // 
            // zLabel2
            // 
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
            this.zLabel2.TabIndex = 3;
            this.zLabel2.Text = "STU Event Time";
            // 
            // STUEventDateTimeOffsetEdit
            // 
            this.STUEventDateTimeOffsetEdit.AllowDrop = true;
            this.STUEventDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.STUEventDateTimeOffsetEdit, "STUEventTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.GUI.UpdateHVLVItemStatusApplicator)(null)).STUEventTime)));
            this.STUEventDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.STUEventDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 29, true);
            this.STUEventDateTimeOffsetEdit.Name = "STUEventDateTimeOffsetEdit";
            this.STUEventDateTimeOffsetEdit.TabIndex = 6;
            // 
            // zLabel3
            // 
            this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
            this.zLabel3.Name = "zLabel3";
            this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 40, true);
            this.zLabel3.TabIndex = 7;
            this.zLabel3.Text = "Warning: when more than 100 consignments are selected this action can take long" +
								" and freeze your application.";
            // 
            // UpdateHVLVItemStatusUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.zLabel3);
            this.Controls.Add(this.STUEventDateTimeOffsetEdit);
            this.Controls.Add(this.zLabel2);
            this.Controls.Add(this.zLabel1);
            this.Controls.Add(this.StatusCodeDropEdit);
            this.Name = "UpdateHVLVItemStatusUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.StatusCodeDropEdit.ResumeLayout(true);
            this.StatusCodeDropEdit.PerformLayout();
            this.STUEventDateTimeOffsetEdit.ResumeLayout(true);
            this.STUEventDateTimeOffsetEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit StatusCodeDropEdit;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.GUI.ZDateTimeOffsetEdit STUEventDateTimeOffsetEdit;
		private ZArchitecture.ZLabel zLabel3;
	}
}
