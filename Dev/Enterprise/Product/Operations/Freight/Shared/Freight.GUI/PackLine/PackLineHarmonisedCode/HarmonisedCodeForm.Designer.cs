namespace Enterprise.Freight.GUI
{
	partial class HarmonisedCodeForm
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

		Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo countryCodeColumnStyleInfo;
		Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			countryCodeColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			tariffColumnStyleInfo = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HSCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HSCodesGrid)).BeginInit();
			this.HSCodesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 216, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.PackLine);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("2e805a4d-48e8-44ec-9c42-8c3f70d6eb7a", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 188, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// HSCodesGrid
			// 
			this.HSCodesGrid.AllowNavigation = false;
			this.HSCodesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HSCodesGrid, "HarmonisedCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).HarmonisedCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobPackLineHarmonisedCode)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).HarmonisedCodes)).SyncRoot)).JLH_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobPackLineHarmonisedCode)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).HarmonisedCodes)).SyncRoot)).JLH_Code)));
			this.HSCodesGrid.CaptionVisible = false;
			countryCodeColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("85d62325-f456-43d9-877a-35285e84f2e9", "Country/Region");
			countryCodeColumnStyleInfo.ColumnName = "JLH_RN_NKCountry";
			countryCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("7b4379dd-e71a-451d-8567-88db2feae71a", "Code");
			tariffColumnStyleInfo.ColumnName = "JLH_Code";
			tariffColumnStyleInfo.PartialDescriptionMinLengthForSearch = 0;
			tariffColumnStyleInfo.TariffType = "HSN";
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HSCodesGrid.ColumnStyles.Add(countryCodeColumnStyleInfo);
			this.HSCodesGrid.ColumnStyles.Add(tariffColumnStyleInfo);
			this.HSCodesGrid.GridId = "2e39c9e4-460f-479e-bba3-25d9f253e32b";
			this.HSCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HSCodesGrid.LayoutKey = "HSCodesGrid";
			this.HSCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.HSCodesGrid.Name = "HSCodesGrid";
			this.HSCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 174, true);
			this.HSCodesGrid.TabIndex = 2;
			// 
			// HarmonisedCodeForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9acffcb9-6738-421f-b839-ebe767c0e1bc", "Harmonized Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 240, true);
			this.Controls.Add(this.HSCodesGrid);
			this.Controls.Add(this.OKButton);
			this.DataSourceType = typeof(Enterprise.Freight.Business.PackLine);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "HarmonisedCodeForm";
			this.Text = "HarmonisedCodeForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.HSCodesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HSCodesGrid)).EndInit();
			this.HSCodesGrid.ResumeLayout(false);
			this.HSCodesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.ZGrid HSCodesGrid;
	}
}
