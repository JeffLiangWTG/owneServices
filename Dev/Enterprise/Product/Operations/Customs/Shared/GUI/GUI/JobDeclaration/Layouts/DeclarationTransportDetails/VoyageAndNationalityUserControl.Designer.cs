namespace Enterprise.Customs.GUI
{
	partial class VoyageAndNationalityUserControl
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
			this.TransportNationalityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VoyageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportNationalityFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// TransportNationalityFindBox
			// 
			this.TransportNationalityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityFindBox, "JE_RN_NKTransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RN_NKTransportNationality)));
			this.TransportNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TransportNationalityFindBox.Name = "TransportNationalityFindBox";
			this.TransportNationalityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityFindBox.ParentType = null;
			this.TransportNationalityFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityFindBox.ShowDescriptionBox = false;
			this.TransportNationalityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityFindBox.TabIndex = 1;
			// 
			// VoyageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageNumberTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));
			this.VoyageNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3EE4F5FC-6EF9-433C-BA2A-73D08F3756D0", "Voy.", "Voyage", "");
			this.VoyageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VoyageNumberTextBox.Name = "VoyageNumberTextBox";
			this.VoyageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.VoyageNumberTextBox.TabIndex = 0;
			// 
			// VoyageAndNationalityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VoyageNumberTextBox);
			this.Controls.Add(this.TransportNationalityFindBox);
			this.Name = "VoyageAndNationalityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportNationalityFindBox.ResumeLayout(true);
			this.TransportNationalityFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox VoyageNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportNationalityFindBox;
	}
}
