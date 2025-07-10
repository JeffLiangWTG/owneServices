namespace Enterprise.Customs.GUI
{
	partial class TransportInlandOwnPropulsionUserControl
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
			this.TransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeOfIDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportNationalityCodeFindBox.SuspendLayout();
			this.TypeOfIDDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// TransportNationalityCodeFindBox
			// 
			this.TransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityCodeFindBox, "JE_RN_NKTransportNationalityInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RN_NKTransportNationalityInland)));
			this.TransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 24, true);
			this.TransportNationalityCodeFindBox.Name = "TransportNationalityCodeFindBox";
			this.TransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityCodeFindBox.ParentType = null;
			this.TransportNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityCodeFindBox.TabIndex = 2;
			// 
			// TransportIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportIDTextBox, "JE_TransportIDInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportIDInland)));
			this.TransportIDTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7aabb1cd-3c70-4bfb-b385-38bf7aee7861", "ID", "Transport ID", "");
			this.TransportIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.TransportIDTextBox.Name = "TransportIDTextBox";
			this.TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.TransportIDTextBox.TabIndex = 1;
			// 
			// TypeOfIDDropEdit
			// 
			this.TypeOfIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeOfIDDropEdit, "JE_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportMeans)));
			this.TypeOfIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TypeOfIDDropEdit.Name = "TypeOfIDDropEdit";
			this.TypeOfIDDropEdit.PreBoundMaxLength = 2;
			this.TypeOfIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.TypeOfIDDropEdit.TabIndex = 0;
			// 
			// TransportInlandOwnPropulsionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TypeOfIDDropEdit);
			this.Controls.Add(this.TransportIDTextBox);
			this.Controls.Add(this.TransportNationalityCodeFindBox);
			this.Name = "TransportInlandOwnPropulsionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportNationalityCodeFindBox.ResumeLayout(true);
			this.TransportNationalityCodeFindBox.PerformLayout();
			this.TypeOfIDDropEdit.ResumeLayout(true);
			this.TypeOfIDDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox TransportNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox TransportIDTextBox;
		internal ZArchitecture.GUI.ZDropEdit TypeOfIDDropEdit;
	}
}
