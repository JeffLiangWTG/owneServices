namespace Enterprise.Customs.NO.GUI
{
	partial class TransportDetailsVoyageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VoyageNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// NationalityCodeFindBox
			// 
			this.NationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NationalityCodeFindBox, "JE_RN_NKTransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_RN_NKTransportNationality)));
			this.NationalityCodeFindBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("BA542EAA-A8A6-4625-919B-52C2500F148B", "Nationality");
			this.NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 0, true);
			this.NationalityCodeFindBox.Name = "NationalityCodeFindBox";
			this.NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NationalityCodeFindBox.ParentType = null;
			this.NationalityCodeFindBox.PreBoundMaxLength = 2;
			this.NationalityCodeFindBox.ShowDescriptionBox = false;
			this.NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.NationalityCodeFindBox.TabIndex = 1;
			// 
			// VoyageNumTextBox
			// 
			this.VoyageNumTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VoyageNumTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_VoyageFlightNo)));
			this.VoyageNumTextBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("B5F0A6D1-729A-4329-B87D-A07BBAD95D9B", "Voyage");
			this.VoyageNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VoyageNumTextBox.Name = "VoyageNumTextBox";
			this.VoyageNumTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.VoyageNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.VoyageNumTextBox.TabIndex = 0;
			// 
			// TransportDetailsVoyageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VoyageNumTextBox);
			this.Controls.Add(this.NationalityCodeFindBox);
			this.Name = "TransportDetailsVoyageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NationalityCodeFindBox.ResumeLayout(true);
			this.NationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox NationalityCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox VoyageNumTextBox;
	}
}
