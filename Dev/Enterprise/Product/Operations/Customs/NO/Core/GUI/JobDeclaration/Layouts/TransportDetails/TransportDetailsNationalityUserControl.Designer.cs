namespace Enterprise.Customs.NO.GUI
{
	partial class TransportDetailsNationalityUserControl
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
            this.TransportDetailsNationality = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
			// 
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// TransportNationalityFindBox
			// 
			this.TransportDetailsNationality.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportDetailsNationality, "JE_RN_NKTransportNationality");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_RN_NKTransportNationality)));
            this.TransportDetailsNationality.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("F7F367A4-32D9-40A7-88D9-4083EE9AFA27", "Nationality of Transport Means");
            this.TransportDetailsNationality.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
            this.TransportDetailsNationality.Name = "TransportNationalityFindBox";
            this.TransportDetailsNationality.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransportDetailsNationality.ParentType = null;
            this.TransportDetailsNationality.PreBoundMaxLength = 2;
            this.TransportDetailsNationality.ShowDescriptionBox = false;
            this.TransportDetailsNationality.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
            this.TransportDetailsNationality.TabIndex = 1;
            // 
            // TransportIDAndNationalityUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TransportDetailsNationality);
            this.Name = "TransportIDAndNationalityUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportDetailsNationality;
	}
}
