using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	public partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupportingInformationUserControl = new Enterprise.Customs.PL.GUI.SupportingInformationControl();
			this.ExciseZDropEdit = new ZDropEdit();
			this.VATZDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingInformationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// SupportingInformationUserControl
			// 
			this.SupportingInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
			this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
			this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 484, true);
			this.SupportingInformationUserControl.TabIndex = 0;
			// 
			// VATZDropEdit
			// 
			this.VATZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATZDropEdit, "ZG_VATDeferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).ZG_VATDeferType)));
			this.VATZDropEdit.CaptionResourceString = null;
			this.VATZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 42, true);
			this.VATZDropEdit.Name = "VATZDropEdit";
			this.VATZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.VATZDropEdit.TabIndex = 1;
			// 
			// ExciseZDropEdit
			//
			this.ExciseZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExciseZDropEdit, "ZG_ExciseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).ZG_ExciseCode)));
			this.ExciseZDropEdit.CaptionResourceString = null;
			this.ExciseZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			this.ExciseZDropEdit.Name = "ExciseZDropEdit";
			this.ExciseZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.ExciseZDropEdit.TabIndex = 2;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.Controls.Add(this.SupportingInformationUserControl);
			this.Controls.Add(this.ExciseZDropEdit);
			this.Controls.Add(this.VATZDropEdit);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 698, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingInformationUserControl.ResumeLayout(true);
			this.SupportingInformationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal SupportingInformationControl SupportingInformationUserControl;
		internal ZDropEdit ExciseZDropEdit;
		internal ZDropEdit VATZDropEdit;
	}
}
