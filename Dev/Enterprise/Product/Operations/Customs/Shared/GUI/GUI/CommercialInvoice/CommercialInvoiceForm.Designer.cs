using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class CommercialInvoiceForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected ZPostingButtonsUserControl PostingButtonsUserControl;
		public ZTemplateTabControl MainTabControl;
		public BaseDeclarationTabPage HeaderTabPage;
		private ZTabPage ReferenceTabPage;
		public BaseDeclarationTabPage LinesTabPage;
		private ZStmNoteTabPage CommercialInvoiceStmNoteTabPage;
		private ZLogsTabPage CommercialInvoiceEventTabPage;
		private ReferenceUserControl ReferenceUserControl;
		private ZWorkflowTabPage CommercialInvoiceWorkflowTabPage;
		private readonly BaseJobComInvoiceHeader fInvoice;
		protected ZMenuItem TopLevelMenu;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.HeaderTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.ReferenceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReferenceUserControl = new Enterprise.Customs.GUI.ReferenceUserControl();
			this.LinesTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.CommercialInvoiceWorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.CommercialInvoiceStmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.CommercialInvoiceEventTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.ReferenceTabPage.SuspendLayout();
			this.CommercialInvoiceWorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 657, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1109, 22, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(501);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 632, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.RequiresHacks = true;
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1109, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.HeaderTabPage);
			this.MainTabControl.Controls.Add(this.ReferenceTabPage);
			this.MainTabControl.Controls.Add(this.LinesTabPage);
			this.MainTabControl.Controls.Add(this.CommercialInvoiceWorkflowTabPage);
			this.MainTabControl.Controls.Add(this.CommercialInvoiceStmNoteTabPage);
			this.MainTabControl.Controls.Add(this.CommercialInvoiceEventTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1107, 601, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.SelectedIndexChanging += new System.EventHandler(this.MainTabControl_SelectedIndexChanging);
			// 
			// HeaderTabPage
			// 
			this.HeaderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HeaderTabPage.Name = "HeaderTabPage";
			this.HeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 574, true);
			this.HeaderTabPage.TabIndex = 0;
			this.HeaderTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7715121B-F8EC-4375-937F-B6AD3641EB36", "Header");
			// 
			// ReferenceTabPage
			// 
			this.ReferenceTabPage.Controls.Add(this.ReferenceUserControl);
			this.ReferenceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReferenceTabPage.Name = "ReferenceTabPage";
			this.ReferenceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 576, true);
			this.ReferenceTabPage.TabIndex = 4;
			this.ReferenceTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7852A9CC-DC4E-48CB-964E-1C158976D8BE", "Reference");
			// 
			// ReferenceUserControl
			// 
			this.BindingSource.SetBindingMember(this.ReferenceUserControl, ".");
			this.ReferenceUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceUserControl.Name = "ReferenceUserControl";
			this.ReferenceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 576, true);
			this.ReferenceUserControl.TabIndex = 0;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 576, true);
			this.LinesTabPage.TabIndex = 1;
			this.LinesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("FE378AFF-8EA7-4FC8-A620-C5E6D08D59D2", "Lines");
			// 
			// CommercialInvoiceWorkflowTabPage
			//
			this.CommercialInvoiceWorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommercialInvoiceWorkflowTabPage.Name = "CommercialInvoiceWorkflowTabPage";
			this.CommercialInvoiceWorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CommercialInvoiceWorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 576, true);
			this.CommercialInvoiceWorkflowTabPage.TabIndex = 5;
			this.CommercialInvoiceWorkflowTabPage.UseVisualStyleBackColor = true;
			// 
			// CommercialInvoiceStmNoteTabPage
			// 
			this.CommercialInvoiceStmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommercialInvoiceStmNoteTabPage.Name = "CommercialInvoiceStmNoteTabPage";
			this.CommercialInvoiceStmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 576, true);
			this.CommercialInvoiceStmNoteTabPage.TabIndex = 2;
			// 
			// CommercialInvoiceEventTabPage
			// 
			this.CommercialInvoiceEventTabPage.ExcludeFromBindingOnSave = true;
			this.CommercialInvoiceEventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommercialInvoiceEventTabPage.Name = "CommercialInvoiceEventTabPage";
			this.CommercialInvoiceEventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 576, true);
			this.CommercialInvoiceEventTabPage.TabIndex = 3;
			// 
			// CommercialInvoiceForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1109, 679, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1124, 718, true);
			this.Name = "CommercialInvoiceForm";
			this.ShouldSerializeTabPageMethods = false;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6E3E57DE-FDEB-4768-B871-0CA93A32700C", "Commercial Invoice");
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.ReferenceTabPage.ResumeLayout(false);
			this.CommercialInvoiceWorkflowTabPage.ResumeLayout(false);
			this.CommercialInvoiceWorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
