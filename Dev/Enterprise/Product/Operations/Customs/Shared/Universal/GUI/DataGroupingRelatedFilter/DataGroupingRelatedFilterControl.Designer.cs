namespace Enterprise.Customs.Universal.GUI
{
	partial class DataGroupingRelatedFilterControl
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
			this.DataGroupingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Property2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Property2FindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DataGroupingCodeFindBox.SuspendLayout();
			this.Property2DropEdit.SuspendLayout();
			this.Property2FindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.GUI.DataGroupingRelatedFilter);
			// 
			// DataGroupingCodeFindBox
			// 
			this.DataGroupingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DataGroupingCodeFindBox, "Property1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.GUI.DataGroupingRelatedFilter)(null)).Property1)));
			this.DataGroupingCodeFindBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("DD0CBD8B-7D48-4D2C-8B49-D7B8756B067F", "Ctry./Rgn./Grouping");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DataGroupingCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.DataGroupingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 1, true);
			this.DataGroupingCodeFindBox.Name = "DataGroupingCodeFindBox";
			this.DataGroupingCodeFindBox.PreBoundMaxLength = 3;
			this.DataGroupingCodeFindBox.ShouldResize = true;
			this.DataGroupingCodeFindBox.ShowDescriptionBox = false;
			this.DataGroupingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.DataGroupingCodeFindBox.TabIndex = 0;
			// 
			// Property2DropEdit
			// 
			this.Property2DropEdit.AllowDrop = true;
			this.Property2DropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Property2DropEdit, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.GUI.DataGroupingRelatedFilter)(null)).Property2)));
			this.Property2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 1, true);
			this.Property2DropEdit.Name = "Property2DropEdit";
			this.Property2DropEdit.ShowDescriptionBox = false;
			this.Property2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.Property2DropEdit.TabIndex = 1;
			this.Property2DropEdit.Visible = false;
			// 
			// Property2FindBox
			// 
			this.Property2FindBox.AllowDrop = true;
			this.Property2FindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Property2FindBox, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.GUI.DataGroupingRelatedFilter)(null)).Property2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.Property2FindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.Property2FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 1, true);
			this.Property2FindBox.Name = "Property2FindBox";
			this.Property2FindBox.ShouldResize = true;
			this.Property2FindBox.ShowDescriptionBox = false;
			this.Property2FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Property2FindBox.TabIndex = 2;
			this.Property2FindBox.Visible = false;
			// 
			// DataGroupingRelatedFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Property2FindBox);
			this.Controls.Add(this.Property2DropEdit);
			this.Controls.Add(this.DataGroupingCodeFindBox);
			this.Name = "DataGroupingRelatedFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DataGroupingCodeFindBox.ResumeLayout(true);
			this.DataGroupingCodeFindBox.PerformLayout();
			this.Property2DropEdit.ResumeLayout(true);
			this.Property2DropEdit.PerformLayout();
			this.Property2FindBox.ResumeLayout(true);
			this.Property2FindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox DataGroupingCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit Property2DropEdit;
		private ZArchitecture.GUI.ZCodeFindBox Property2FindBox;
	}
}
