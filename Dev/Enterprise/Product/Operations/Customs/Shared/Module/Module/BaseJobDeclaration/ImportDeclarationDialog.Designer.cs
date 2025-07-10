namespace Enterprise.Customs.Module
{
	partial class ImportDeclarationDialog
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportDeclarationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeclarationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel5.TabIndex = 0;
			this.zLabel5.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("35F3474E-B5AE-4F79-80B6-4EA89938D68B", "Country:");
			// 
			// CancelButton
			// 
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 60, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.TabIndex = 3;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("E4761F64-442B-4908-BE19-1C01D624046A", "Cancel");
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ImportDeclarationButton
			// 
			this.ImportDeclarationButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportDeclarationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 60, true);
			this.ImportDeclarationButton.Name = "ImportDeclarationButton";
			this.ImportDeclarationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.ImportDeclarationButton.TabIndex = 2;
			this.ImportDeclarationButton.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("B020FA1E-B951-49FF-B01E-9A68B705529E", "Import Declaration");
			this.ImportDeclarationButton.Click += new System.EventHandler(this.ImportDeclarationButtonButton_Click);
			// 
			// DeclarationGuidFindBox
			// 
			this.DeclarationGuidFindBox.BindTo = "DeclarationPK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.ImportJobDeclaration)(null)).DeclarationPK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.ImportJobDeclaration)(null)).DeclarationPKInfo)));
			this.DeclarationGuidFindBox.BindToList = "Lookups+DeclarationList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.ImportJobDeclaration)(null)).Lookups.DeclarationList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeclarationGuidFindBox, false);
			this.DeclarationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 32, true);
			this.DeclarationGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.JobDeclaration;
			this.DeclarationGuidFindBox.Name = "DeclarationGuidFindBox";
			this.DeclarationGuidFindBox.PopupCaption = Enterprise.Customs.Module.Res.GetString("CD65EA28-7D32-4F15-9700-93A5E928CA49", "Customs Declaration");
			this.DeclarationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.DeclarationGuidFindBox.TabIndex = 1;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.BindTo = "CountryCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.ImportJobDeclaration)(null)).CountryCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ImportJobDeclaration)(null)).CountryCode)));
			this.CountryCodeFindBox.BindToList = "Lookups+CountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.ImportJobDeclaration)(null)).Lookups.CountryList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CountryCodeFindBox, false);
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 8, true);
			this.CountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20, true);
			this.CountryCodeFindBox.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 31, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = Enterprise.Customs.Module.Res.GetString("F102CBFC-38A0-413C-8740-A637E106D961", "Declaration:");
			// 
			// ImportDeclarationDialog
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 112, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CountryCodeFindBox);
			this.Controls.Add(this.DeclarationGuidFindBox);
			this.Controls.Add(this.ImportDeclarationButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.zLabel5);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.ImportJobDeclaration";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ImportDeclarationDialog";
			this.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("F504505E-17A4-4E4F-9D04-7EBD62AB80E6", "Select Declaration To Import");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.ImportDeclarationButton, 0);
			this.Controls.SetChildIndex(this.DeclarationGuidFindBox, 0);
			this.Controls.SetChildIndex(this.CountryCodeFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabel5;
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		protected internal Enterprise.ZArchitecture.GUI.ZGuidFindBox DeclarationGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton ImportDeclarationButton;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
	}
}
