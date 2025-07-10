namespace Enterprise.Customs.PL.GUI
{
	partial class TransportInlandRailUserControl
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
			this.TrainNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TrainNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TrainNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// TrainNationalityCodeFindBox
			// 
			this.TrainNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TrainNationalityCodeFindBox, "JE_RN_NKTransportNationalityInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RN_NKTransportNationalityInland)));
			this.TrainNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TrainNationalityCodeFindBox.Name = "TrainNationalityCodeFindBox";
			this.TrainNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TrainNationalityCodeFindBox.ParentType = null;
			this.TrainNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TrainNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TrainNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
			this.TrainNationalityCodeFindBox.TabIndex = 1;
			// 
			// TrainNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TrainNumberTextBox, "JE_TransportIDInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportIDInland)));
			this.TrainNumberTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("69E0EB95-13B6-4D60-9A4D-9C7879E37988", "Train Num.", "Train Number", "");
			this.TrainNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TrainNumberTextBox.Name = "TrainNumberTextBox";
			this.TrainNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 18, true);
			this.TrainNumberTextBox.TabIndex = 0;
			// 
			// TransportInlandRailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TrainNumberTextBox);
			this.Controls.Add(this.TrainNationalityCodeFindBox);
			this.Name = "TransportInlandRailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TrainNationalityCodeFindBox.ResumeLayout(true);
			this.TrainNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox TrainNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox TrainNumberTextBox;
	}
}
