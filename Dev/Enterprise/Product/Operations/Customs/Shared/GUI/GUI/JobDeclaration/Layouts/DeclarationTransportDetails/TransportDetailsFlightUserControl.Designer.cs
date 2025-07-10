namespace Enterprise.Customs.GUI
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// VoyageFlightNumberTextBox
			// 
			this.VoyageFlightNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VoyageFlightNumberTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));
			this.VoyageFlightNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C6E0B524-5E32-4CA7-89B6-F877E520B174", "Flight/Folio");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_Folio)));
			this.FolioNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FolioNumberTextBox, false);
			this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 0, true);
			this.FolioNumberTextBox.Name = "FolioNumberTextBox";
			this.FolioNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.FolioNumberTextBox.TabIndex = 1;
			// 
			// TransportDetailsFlightUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VoyageFlightNumberTextBox);
			this.Controls.Add(this.FolioNumberTextBox);
			this.Name = "TransportDetailsFlightUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.ZTextBox VoyageFlightNumberTextBox;
		internal Enterprise.ZArchitecture.ZTextBox FolioNumberTextBox;
	}
}
