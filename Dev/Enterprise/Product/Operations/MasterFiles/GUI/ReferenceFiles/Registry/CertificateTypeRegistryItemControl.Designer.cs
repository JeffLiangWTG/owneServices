namespace Enterprise.MasterFiles.GUI
{
	partial class CertificateTypeRegistryItemControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.elementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CertificateTypesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.elementsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CertificateTypesGrid)).BeginInit();
            this.CertificateTypesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CertificateTypeCollection);
            // 
            // elementsGroupBox
            // 
            this.elementsGroupBox.Controls.Add(this.CertificateTypesGrid);
            this.elementsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.elementsGroupBox, false);
            this.elementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.elementsGroupBox.Name = "elementsGroupBox";
            this.elementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 274, true);
            this.elementsGroupBox.TabIndex = 0;
            this.elementsGroupBox.TabStop = false;
            // 
            // CertificateTypesGrid
            // 
            this.CertificateTypesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.CertificateTypesGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CertificateType)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CertificateType)(null)).Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.CertificateType)(null)).DescriptionMultilingual)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CertificateType)(null)).IsMandatory)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CertificateType)(null)).IsUnique)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CertificateType)(null)).AlertType)));
            this.CertificateTypesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CertificateTypeRegistryItemControl|1f4bcea0-636f-4727-8f5b-818bb88cd034", "Code");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "Code";
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CertificateTypeRegistryItemControl|6d1cc157-5b91-49a4-812f-3151febac4ee", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "DescriptionMultilingual";
            zTextBoxColumnStyleInfo2.IsMandatory = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CertificateTypeRegistryItemControl|d0229830-8aeb-4733-967c-6c74013c6175", "Mandatory");
            zCheckBoxColumnStyleInfo1.ColumnName = "IsMandatory";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CertificateTypeRegistryItemControl|96b6e888-1f68-421e-880d-e14523aaa07a", "Unique");
            zCheckBoxColumnStyleInfo2.ColumnName = "IsUnique";
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d96d607e-6b12-4bb5-93be-48eb73123518", "Alert Type");
            zDropEditColumnStyleInfo1.ColumnName = "AlertType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.CertificateTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.CertificateTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.CertificateTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.CertificateTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.CertificateTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.CertificateTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CertificateTypesGrid.GridId = "667c65d0-875e-4ea6-b61b-182d86dad448";
            this.CertificateTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CertificateTypesGrid.LayoutKey = "CertificateTypesGrid";
            this.CertificateTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.CertificateTypesGrid.Name = "CertificateTypesGrid";
            this.CertificateTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 255, true);
            this.CertificateTypesGrid.TabIndex = 0;
            // 
            // CertificateTypeRegistryItemControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.elementsGroupBox);
            this.Name = "CertificateTypeRegistryItemControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 274, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.elementsGroupBox.ResumeLayout(false);
            this.elementsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CertificateTypesGrid)).EndInit();
            this.CertificateTypesGrid.ResumeLayout(false);
            this.CertificateTypesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox elementsGroupBox;
		private Enterprise.ZArchitecture.ZGrid CertificateTypesGrid;
	}
}
