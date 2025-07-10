namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class AutomatedTariffDescriptionPopulationControl
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
			this.EnableCommercialInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutomatedTariffDescriptionPopulationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EnableCustomsDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AutomatedTariffDescriptionPopulationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.AutomatedTariffDescriptionPopulation);
			// 
			// EnableCommercialInvoiceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableCommercialInvoiceCheckBox, "EnableCommercialInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AutomatedTariffDescriptionPopulation)(null)).EnableCommercialInvoice)));
			this.EnableCommercialInvoiceCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableCommercialInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 44, true);
			this.EnableCommercialInvoiceCheckBox.Name = "EnableCommercialInvoiceCheckBox";
			this.EnableCommercialInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 18, true);
			this.EnableCommercialInvoiceCheckBox.TabIndex = 0;
			this.EnableCommercialInvoiceCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutomatedTariffDescriptionPopulationGroupBox
			// 
			this.AutomatedTariffDescriptionPopulationGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("88BC94CF-DD37-4E84-A157-6479CBCAB2A4", "Enable auto population of goods description");
			this.AutomatedTariffDescriptionPopulationGroupBox.Controls.Add(this.EnableCustomsDeclarationCheckBox);
			this.AutomatedTariffDescriptionPopulationGroupBox.Controls.Add(this.EnableCommercialInvoiceCheckBox);
			this.AutomatedTariffDescriptionPopulationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutomatedTariffDescriptionPopulationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AutomatedTariffDescriptionPopulationGroupBox.Name = "AutomatedTariffDescriptionPopulationGroupBox";
			this.AutomatedTariffDescriptionPopulationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.AutomatedTariffDescriptionPopulationGroupBox.TabIndex = 1;
			this.AutomatedTariffDescriptionPopulationGroupBox.TabStop = false;
			// 
			// EnableCustomsDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableCustomsDeclarationCheckBox, "EnableCustomsDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AutomatedTariffDescriptionPopulation)(null)).EnableCustomsDeclaration)));
			this.EnableCustomsDeclarationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableCustomsDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 66, true);
			this.EnableCustomsDeclarationCheckBox.Name = "EnableCustomsDeclarationCheckBox";
			this.EnableCustomsDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 18, true);
			this.EnableCustomsDeclarationCheckBox.TabIndex = 1;
			this.EnableCustomsDeclarationCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutomatedTariffDescriptionPopulationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AutomatedTariffDescriptionPopulationGroupBox);
			this.Name = "AutomatedTariffDescriptionPopulationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AutomatedTariffDescriptionPopulationGroupBox.ResumeLayout(false);
			this.AutomatedTariffDescriptionPopulationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox EnableCommercialInvoiceCheckBox;
		private ZArchitecture.GUI.ZGroupBox AutomatedTariffDescriptionPopulationGroupBox;
		private ZArchitecture.GUI.ZCheckBox EnableCustomsDeclarationCheckBox;
	}
}
