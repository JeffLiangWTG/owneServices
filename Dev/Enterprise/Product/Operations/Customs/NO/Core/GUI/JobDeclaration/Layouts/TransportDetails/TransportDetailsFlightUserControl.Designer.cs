namespace Enterprise.Customs.NO.GUI
{
	partial class TransportDetailsFlightUserControl
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
            this.VoyageFlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.FolioNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransportNationalityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
            // 
            // VoyageFlightNumberTextBox
            // 
            this.VoyageFlightNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.VoyageFlightNumberTextBox, "JE_VoyageFlightNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_VoyageFlightNo)));
            this.VoyageFlightNumberTextBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("F520D9EF-BAA0-4F6F-B91B-3271E0B85FC4", "Flight/Folio");
            this.VoyageFlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.VoyageFlightNumberTextBox.Name = "VoyageFlightNumberTextBox";
            this.VoyageFlightNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.VoyageFlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
            this.VoyageFlightNumberTextBox.TabIndex = 0;
            // 
            // FolioNumberTextBox
            // 
            this.FolioNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.FolioNumberTextBox, "JE_Folio");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_Folio)));
            this.FolioNumberTextBox.CaptionResourceString = null;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FolioNumberTextBox, false);
            this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 0, true);
            this.FolioNumberTextBox.Name = "FolioNumberTextBox";
            this.FolioNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
            this.FolioNumberTextBox.TabIndex = 1;
            // 
            // TransportNationalityFindBox
            // 
            this.TransportNationalityFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportNationalityFindBox, "JE_RN_NKTransportNationality");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_RN_NKTransportNationality)));
			this.TransportNationalityFindBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("756B5C06-B21D-481A-8FB3-EED68AC386BC", "Nationality");
			this.TransportNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
            this.TransportNationalityFindBox.Name = "TransportNationalityFindBox";
            this.TransportNationalityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransportNationalityFindBox.ParentType = null;
            this.TransportNationalityFindBox.PreBoundMaxLength = 2;
            this.TransportNationalityFindBox.ShowDescriptionBox = false;
            this.TransportNationalityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
            this.TransportNationalityFindBox.TabIndex = 2;
            // 
            // TransportDetailsFlightUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.VoyageFlightNumberTextBox);
            this.Controls.Add(this.FolioNumberTextBox);
			this.Controls.Add(this.TransportNationalityFindBox);
			this.Name = "TransportDetailsFlightUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.ZTextBox VoyageFlightNumberTextBox;
		internal Enterprise.ZArchitecture.ZTextBox FolioNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportNationalityFindBox;
	}
}
