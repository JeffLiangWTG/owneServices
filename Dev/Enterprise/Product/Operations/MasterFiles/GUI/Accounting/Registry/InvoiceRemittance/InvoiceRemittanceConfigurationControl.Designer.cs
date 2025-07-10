using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class InvoiceRemittanceConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.ChildGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(InvoiceRemittanceConfigurationCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AB33D614-991D-4AEF-9779-729D4CE717D4", "Code");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 128, true);
			this.ParentGroupBox.TabIndex = 2;
			this.ParentGroupBox.TabStop = false;
			// 
			// ParentGrid
			// 
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceConfiguration)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceConfiguration)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceConfiguration)(null)).BillerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceConfiguration)(null)).BillerAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((InvoiceRemittanceConfiguration)(null)).MaxPossibleLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceConfiguration)(null)).Message)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceConfiguration)(null)).DebtorLocation)));
			this.ParentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("79D31439-C5BA-439E-A4BA-AB99605E04B9", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ED08864B-ACFD-41B5-B7B9-A956C3C8F79C", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("899C7E0A-F8E1-4958-9EB9-6278E90C76B4", "Biller Code");
			zTextBoxColumnStyleInfo3.ColumnName = "BillerCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9BC8DB6A-4DD4-4C0F-B68E-058901236921", "Biller Account Number");
			zTextBoxColumnStyleInfo8.ColumnName = "BillerAccountNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("201495FB-BD2A-4050-8813-FF8668B3B167", "Max Possible Length");
			zCalcEditColumnStyleInfo1.ColumnName = "MaxPossibleLength";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.AllowNegative = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9C55F269-76F4-4A6C-8167-CBFCD7BD9228", "Message");
			zTextBoxColumnStyleInfo4.ColumnName = "Message";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E43DE21B-EC7B-4A08-B7EE-6361D0909E05", "Debtor Location");
			zDropEditColumnStyleInfo1.ColumnName = "DebtorLocation";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ParentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "1AB74FAB-8FB4-4F8F-9630-D05FB1979D77";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 111, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.GridSplitter.MinSize = 120;
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 2, true);
			this.GridSplitter.TabIndex = 4;
			this.GridSplitter.TabStop = false;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("69213EA9-369C-4260-B8ED-EFE69069CAAF", "Elements");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 244, true);
			this.ChildGroupBox.TabIndex = 5;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "Elements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((InvoiceRemittanceCustomisationElement)(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)).SyncRoot)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceCustomisationElement)(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)).SyncRoot)).ElementName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((InvoiceRemittanceCustomisationElement)(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceCustomisationElement)(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)).SyncRoot)).DigitCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceCustomisationElement)(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)).SyncRoot)).CheckDigit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceRemittanceCustomisationElement)(((System.Collections.IList)(((InvoiceRemittanceConfiguration)(null)).Elements)).SyncRoot)).CheckDigitAlgorithm)));
			this.ChildGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5B3BC0A1-94BD-4801-8F7D-565E44071289", "Order");
			zCalcEditColumnStyleInfo2.ColumnName = "Order";
			zCalcEditColumnStyleInfo2.MaxValue = 100;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("32AED030-6780-4EBC-8433-66436FD47080", "Element Name");
			zTextBoxColumnStyleInfo5.ColumnName = "ElementName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4650366B-136A-4886-8FFE-8B241E767927", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "Include";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6F201B0B-9FB4-491A-A4C3-7E8E8E039EEF", "Digit/Code");
			zTextBoxColumnStyleInfo6.ColumnName = "DigitCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("34DC626B-1A9B-4688-BDFC-693B2DA333BD", "Check Digit");
			zDropEditColumnStyleInfo3.ColumnName = "CheckDigit";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("FA61B1D9-4807-4857-9D47-8269EAB6594D", "Check Digit Algorithm");
			zDropEditColumnStyleInfo2.ColumnName = "CheckDigitAlgorithm";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ChildGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "09387452-11A7-40C4-8333-DE0FE08AD97E";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 227, true);
			this.ChildGrid.TabIndex = 1;
			this.ChildGrid.DisableImportDataMenuItem = true;
			this.ChildGrid.ShowMassUpdateMenuItem = false;
			// 
			// InvoiceRemittanceConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "InvoiceRemittanceConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 374, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ParentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ChildGrid.ResumeLayout(false);
			this.ChildGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox ParentGroupBox;
		internal ZArchitecture.ZGrid ParentGrid;
		CargoWise.Windows.UI.KSplitter GridSplitter;
		ZArchitecture.GUI.ZGroupBox ChildGroupBox;
		ZArchitecture.ZGrid ChildGrid;

	}
}
