
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI
{
	partial class QueryQuotaVisaOptionForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.US_QueryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.VisaTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffCodeFindBox = new Enterprise.Customs.Common.GUI.TariffFindBox();
			this.SecondTariffFindBox = new Enterprise.Customs.Common.GUI.TariffFindBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.US_QueryTypeDropEdit.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.TariffCodeFindBox.SuspendLayout();
			this.SecondTariffFindBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 243, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.QueryQuotaVisaOption);
			// 
			// US_QueryTypeDropEdit
			// 
			this.US_QueryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_QueryTypeDropEdit, "US_QueryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_QueryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_QueryTypeList)));
			this.US_QueryTypeDropEdit.BindToList = "US_QueryTypeList";
			this.US_QueryTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e4e51620-6bfd-4ea3-a60a-16ec13ea52b5", "Query Type");
			this.US_QueryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 29, true);
			this.US_QueryTypeDropEdit.Name = "US_QueryTypeDropEdit";
			this.US_QueryTypeDropEdit.PreBoundMaxLength = 1;
			this.US_QueryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.US_QueryTypeDropEdit.TabIndex = 0;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "US_UC_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_UC_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).CountryOfOriginList)));
			this.CountryCodeFindBox.BindToList = "CountryOfOriginList";
			this.CountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5fe14402-19ac-46a1-8745-96c2c103412f", "Country Of Origin");
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 61, true);
			this.CountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.PreBoundMaxLength = 2;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.CountryCodeFindBox.TabIndex = 1;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 214, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 214, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "US_CategoryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_CategoryNumber)));
			this.zTextBox2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4656bbec-42d8-4699-9597-31cb20ad202a", "Category Number");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 157, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.zTextBox2.TabIndex = 4;
			// 
			// VisaTextBox
			// 
			this.BindingSource.SetBindingMember(this.VisaTextBox, "US_VisaNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_VisaNumber)));
			this.VisaTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d9e8e4d5-e956-427e-86c2-cbcee8b0fe22", "Visa Number");
			this.VisaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 157, true);
			this.VisaTextBox.Name = "VisaTextBox";
			this.VisaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.VisaTextBox.TabIndex = 5;
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "US_FormattedTariffNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_FormattedTariffNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).Tariffs)));
			this.TariffCodeFindBox.BindToList = "Tariffs";
			this.TariffCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f389a678-ba2b-4b92-9a19-0d419c82e9e8", "Tariff Number");
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 93, true);
			this.TariffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.TariffCodeFindBox.TabIndex = 2;
			// 
			// SecondTariffFindBox
			// 
			this.SecondTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecondTariffFindBox, "US_SecondTariffFormattedNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).US_SecondTariffFormattedNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryQuotaVisaOption)(null)).Tariffs)));
			this.SecondTariffFindBox.BindToList = "Tariffs";
			this.SecondTariffFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f3ff5105-e3af-49d9-8104-688aac7b83bf", "Second Tariff Number");
			this.SecondTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 125, true);
			this.SecondTariffFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			this.SecondTariffFindBox.Name = "SecondTariffFindBox";
			this.SecondTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.SecondTariffFindBox.TabIndex = 3;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0db0b274-bdf4-42e5-9f5d-f98c37432100", "Query Options");
			this.zGroupBox1.Controls.Add(this.SecondTariffFindBox);
			this.zGroupBox1.Controls.Add(this.TariffCodeFindBox);
			this.zGroupBox1.Controls.Add(this.VisaTextBox);
			this.zGroupBox1.Controls.Add(this.US_QueryTypeDropEdit);
			this.zGroupBox1.Controls.Add(this.CountryCodeFindBox);
			this.zGroupBox1.Controls.Add(this.zTextBox2);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 9, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 197, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// QueryQuotaVisaOptionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 267, true);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SendButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.QueryQuotaVisaOption);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.QueryQuotaVisaOption";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 293, true);
			this.Name = "QueryQuotaVisaOptionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Query Quota/Visa";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.US_QueryTypeDropEdit.ResumeLayout(true);
			this.US_QueryTypeDropEdit.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
			this.SecondTariffFindBox.ResumeLayout(true);
			this.SecondTariffFindBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit US_QueryTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private Enterprise.ZArchitecture.ZTextBox zTextBox2;
		private Enterprise.ZArchitecture.ZTextBox VisaTextBox;
		protected Enterprise.Customs.Common.GUI.TariffFindBox TariffCodeFindBox;
		protected Enterprise.Customs.Common.GUI.TariffFindBox SecondTariffFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
	}
}