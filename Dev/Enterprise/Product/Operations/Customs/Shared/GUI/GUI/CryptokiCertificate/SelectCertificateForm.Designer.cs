namespace Enterprise.Customs.GUI.Certificates
{
	partial class SelectCertificateForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ChooseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CanceledButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			this.CertificatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CryptokiCertificateCollection);
			// 
			// ChooseButton
			// 
			this.ChooseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ChooseButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C91AAF77-9436-4052-A405-AD123B93673E", "Choose");
			this.ChooseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(742, 412, true);
			this.ChooseButton.Name = "ChooseButton";
			this.ChooseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.ChooseButton.TabIndex = 1;
			this.ChooseButton.ToolTipCaption = null;
			this.ChooseButton.Click += new System.EventHandler(this.ChooseButton_Click);
			// 
			// CanceledButton
			// 
			this.CanceledButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CanceledButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4E0587F3-EE81-47C1-8712-4E1AB5D5A9C6", "Cancel");
			this.CanceledButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CanceledButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(806, 412, true);
			this.CanceledButton.Name = "CanceledButton";
			this.CanceledButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.CanceledButton.TabIndex = 2;
			this.CanceledButton.ToolTipCaption = null;
			this.CanceledButton.Click += new System.EventHandler(this.CanceledButton_Click);
			// 
			// CertificatesGrid
			// 
			this.CertificatesGrid.AllowNavigation = false;
			this.CertificatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CertificatesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CryptokiCertificate)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).NotBefore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).NotAfter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).Issuer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).Thumbprint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).SerialNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).TokenManufacturerId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).TokenChipset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CryptokiCertificate)(null)).TokenModel)));
			this.CertificatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Owner";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			zDateEditColumnStyleInfo1.ColumnName = "NotBefore";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "NotAfter";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "Issuer";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			zTextBoxColumnStyleInfo3.ColumnName = "Thumbprint";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo4.ColumnName = "SerialNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo5.ColumnName = "TokenManufacturerId";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.ColumnName = "TokenChipset";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "TokenModel";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CertificatesGrid.GridId = "9c903eb3-88d1-4f6e-b6bc-64c0c1c832f8";
			this.CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificatesGrid.LayoutKey = "CertificatesGrid";
			this.CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.CertificatesGrid.Name = "CertificatesGrid";
			this.CertificatesGrid.ReadOnly = true;
			this.CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 398, true);
			this.CertificatesGrid.TabIndex = 0;
			this.CertificatesGrid.DoubleClick += new System.EventHandler(this.CertificatesGrid_DoubleClick);
			// 
			// SelectCertificateForm
			// 
			this.AcceptButton = this.ChooseButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CanceledButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("92DB3803-23B5-4C3A-A26F-8252A0E902A9", "Select Certificate");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 464, true);
			this.Controls.Add(this.CertificatesGrid);
			this.Controls.Add(this.CanceledButton);
			this.Controls.Add(this.ChooseButton);
			this.DataSourceType = typeof(Enterprise.Customs.Business.CryptokiCertificateCollection);
			this.Name = "SelectCertificateForm";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.ChooseButton, 0);
			this.Controls.SetChildIndex(this.CanceledButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CertificatesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			this.CertificatesGrid.ResumeLayout(false);
			this.CertificatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton ChooseButton;
		private ZArchitecture.GUI.ZButton CanceledButton;
		private ZArchitecture.ZGrid CertificatesGrid;
	}
}
