using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI
{
	partial class ArrivalAdditionalDetailsUserControl
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
		private void InitializeComponent()
		{
			this.TirPageNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TirUnloadingNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TirPageNumberDropEdit.SuspendLayout();
			this.TirUnloadingNumberDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.NCTS.Business.MessageSendingObject);
			// 
			// TirPageNumberDropEdit
			// 
			this.TirPageNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TirPageNumberDropEdit, "TirPageNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).TirPageNumber)));
			this.TirPageNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 14, true);
			this.TirPageNumberDropEdit.Name = "TirPageNumberDropEdit";
			this.TirPageNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.TirPageNumberDropEdit.TabIndex = 0;
			// 
			// TirUnloadingNumberDropEdit
			// 
			this.TirUnloadingNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TirUnloadingNumberDropEdit, "TirUnloadingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).TirUnloadingNumber)));
			this.TirUnloadingNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 36, true);
			this.TirUnloadingNumberDropEdit.Name = "TirUnloadingNumberDropEdit";
			this.TirUnloadingNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.TirUnloadingNumberDropEdit.TabIndex = 1;
			this.TirUnloadingNumberDropEdit.ShowDescriptionBox = false;
			// 
			// ArrivalAdditionalDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TirPageNumberDropEdit);
			this.Controls.Add(this.TirUnloadingNumberDropEdit);
			this.Name = "ArrivalAdditionalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 213, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TirPageNumberDropEdit.ResumeLayout(true);
			this.TirPageNumberDropEdit.PerformLayout();
			this.TirUnloadingNumberDropEdit.ResumeLayout(true);
			this.TirUnloadingNumberDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit TirPageNumberDropEdit;
		internal ZDropEdit TirUnloadingNumberDropEdit;
	}
}
