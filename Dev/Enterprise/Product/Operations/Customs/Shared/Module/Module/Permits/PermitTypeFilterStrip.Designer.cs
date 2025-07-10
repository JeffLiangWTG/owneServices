namespace Enterprise.Customs.Module
{
	partial class PermitTypeFilterStrip
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
			this.CountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryDropEdit.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Module.PermitTypeModuleFilter);
			// 
			// CountryDropEdit
			// 
			this.CountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryDropEdit, "Property0");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Module.PermitTypeModuleFilter)(null)).Property0)));
			this.CountryDropEdit.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("049EF9D0-3C81-4D83-88F2-E862513DD464", "Country/Region");
			this.CountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 1, true);
			this.CountryDropEdit.Name = "CountryDropEdit";
			this.CountryDropEdit.PreBoundMaxLength = 2;
			this.CountryDropEdit.ShowDescriptionBox = false;
			this.CountryDropEdit.ShowHorizontalScrollBar = false;
			this.CountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.CountryDropEdit.TabIndex = 0;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "Property1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Module.PermitTypeModuleFilter)(null)).Property1)));
			this.TypeDropEdit.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("09813AE1-E835-45BF-817B-6C4C99D857AE", "Type");
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 1, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.PreBoundMaxLength = 3;
			this.TypeDropEdit.ShowDescriptionBox = false;
			this.TypeDropEdit.ShowHorizontalScrollBar = false;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TypeDropEdit.TabIndex = 1;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Module.PermitTypeModuleFilter)(null)).Property2)));
			this.SubTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SubTypeDropEdit.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("ACB652EB-E15A-40CE-9E34-432217A480F5", "Sub Type");
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 1, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.PreBoundMaxLength = 3;
			this.SubTypeDropEdit.ShowHorizontalScrollBar = false;
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.SubTypeDropEdit.TabIndex = 2;
			// 
			// PermitTypeFilterStrip
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubTypeDropEdit);
			this.Controls.Add(this.TypeDropEdit);
			this.Controls.Add(this.CountryDropEdit);
			this.Name = "PermitTypeFilterStrip";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryDropEdit.ResumeLayout(true);
			this.CountryDropEdit.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZDropEdit CountryDropEdit;
		protected ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit SubTypeDropEdit;
	}
}
