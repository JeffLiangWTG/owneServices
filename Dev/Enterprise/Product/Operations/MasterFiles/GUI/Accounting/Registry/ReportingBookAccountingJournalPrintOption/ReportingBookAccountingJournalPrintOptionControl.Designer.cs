namespace Enterprise.MasterFiles.GUI
{
	public partial class ReportingBookAccountingJournalPrintOptionControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ReportingBookAccountingJournalPrintOptionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportingBookAccountingJournalPrintOptionGrid)).BeginInit();
			this.ReportingBookAccountingJournalPrintOptionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOptionCollection);
			// 
			// ReportingBookAccountingJournalPrintOptionGrid
			// 
			this.ReportingBookAccountingJournalPrintOptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportingBookAccountingJournalPrintOptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOption)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOption)(null)).ReportingBook)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOption)(null)).ReportingBookDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOption)(null)).DisplayParentAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOption)(null)).DisplayAttribute)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ReportingBookAccountingJournalPrintOption)(null)).Default)));
			this.ReportingBookAccountingJournalPrintOptionGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("379f2256-1a69-47ef-9f2c-ceb475c6e99b", "Reporting Book");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ReportingBook";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c61310bd-e687-4389-9faf-d0e4ed89a61d", "Reporting Book Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ReportingBookDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("94baaf2f-1a22-43f0-aaa9-03539e91c532", "Display Parent Account");
			zCheckBoxColumnStyleInfo1.ColumnName = "DisplayParentAccount";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7477af08-3ca2-4b4a-968b-9b8f2e1a9ea8", "Display Attribute");
			zCheckBoxColumnStyleInfo2.ColumnName = "DisplayAttribute";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9036a58b-4e33-44d4-86d1-c22471aae559", "Default");
			zCheckBoxColumnStyleInfo3.ColumnName = "Default";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReportingBookAccountingJournalPrintOptionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ReportingBookAccountingJournalPrintOptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportingBookAccountingJournalPrintOptionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReportingBookAccountingJournalPrintOptionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ReportingBookAccountingJournalPrintOptionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ReportingBookAccountingJournalPrintOptionGrid.GridId = "596c0814-c596-4096-871f-30af651bcee1";
			this.ReportingBookAccountingJournalPrintOptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportingBookAccountingJournalPrintOptionGrid.LayoutKey = "ReportingBookAccountingJournalPrintOptionGrid";
			this.ReportingBookAccountingJournalPrintOptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportingBookAccountingJournalPrintOptionGrid.Name = "ReportingBookAccountingJournalPrintOptionGrid";
			this.ReportingBookAccountingJournalPrintOptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 222, true);
			this.ReportingBookAccountingJournalPrintOptionGrid.TabIndex = 0;
			this.ReportingBookAccountingJournalPrintOptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ReportingBookAccountingJournalPrintOptionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportingBookAccountingJournalPrintOptionGrid);
			this.Name = "ReportingBookAccountingJournalPrintOptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportingBookAccountingJournalPrintOptionGrid)).EndInit();
			this.ReportingBookAccountingJournalPrintOptionGrid.ResumeLayout(false);
			this.ReportingBookAccountingJournalPrintOptionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.ZGrid ReportingBookAccountingJournalPrintOptionGrid;
	}
}
