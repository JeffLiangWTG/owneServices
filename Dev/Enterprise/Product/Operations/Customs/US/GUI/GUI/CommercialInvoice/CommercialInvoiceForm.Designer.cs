
namespace Enterprise.Customs.US.GUI
{
	partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
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
		new void InitializeComponent()
		{
            this.AIITabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.invoiceHeaderAIIUserControl1 = new Enterprise.Customs.US.GUI.InvoiceHeaderAIIUserControl();
            this.OrganisationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.invoiceHeaderOrganisationsUserControl1 = new Enterprise.Customs.US.GUI.InvoiceHeaderOrganisationsUserControl();
            this.MainTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AIITabPage.SuspendLayout();
            this.OrganisationTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // PostingButtonsUserControl
            // 
            this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(767, 630, true);
            this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 24, true);
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.AIITabPage);
            this.MainTabControl.Controls.Add(this.OrganisationTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 626, true);
            this.MainTabControl.Controls.SetChildIndex(this.LinesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.OrganisationTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.AIITabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.HeaderTabPage, 0);
            // 
            // HeaderTabPage
            // 
            this.HeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 599, true);
            // 
            // LinesTabPage
            // 
            this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 471, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 22, true);
            // 
            // AIITabPage
            // 
            this.AIITabPage.Controls.Add(this.invoiceHeaderAIIUserControl1);
            this.AIITabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.AIITabPage.Name = "AIITabPage";
            this.AIITabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.AIITabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 471, true);
            this.AIITabPage.TabIndex = 4;
            this.AIITabPage.Text = "Electronic Invoice";
            // 
            // invoiceHeaderAIIUserControl1
            // 
            this.BindingSource.SetBindingMember(this.invoiceHeaderAIIUserControl1, ".");
            this.invoiceHeaderAIIUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.invoiceHeaderAIIUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.invoiceHeaderAIIUserControl1.Name = "invoiceHeaderAIIUserControl1";
            this.invoiceHeaderAIIUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 465, true);
            this.invoiceHeaderAIIUserControl1.TabIndex = 0;
            // 
            // OrganisationTabPage
            // 
            this.OrganisationTabPage.Controls.Add(this.invoiceHeaderOrganisationsUserControl1);
            this.OrganisationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.OrganisationTabPage.Name = "OrganisationTabPage";
            this.OrganisationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.OrganisationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 471, true);
            this.OrganisationTabPage.TabIndex = 5;
            this.OrganisationTabPage.Text = "Organizations";
            // 
            // invoiceHeaderOrganisationsUserControl1
            // 
            this.BindingSource.SetBindingMember(this.invoiceHeaderOrganisationsUserControl1, ".");
            this.invoiceHeaderOrganisationsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.invoiceHeaderOrganisationsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.invoiceHeaderOrganisationsUserControl1.Name = "invoiceHeaderOrganisationsUserControl1";
            this.invoiceHeaderOrganisationsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 465, true);
            this.invoiceHeaderOrganisationsUserControl1.TabIndex = 0;
            // 
            // CommercialInvoiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 680, true);
            this.Name = "CommercialInvoiceForm";
            this.ShowIcon = false;
            this.Text = "CommercialInvoiceForm";
            this.MainTabControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AIITabPage.ResumeLayout(false);
            this.OrganisationTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabPage AIITabPage;
		internal InvoiceHeaderAIIUserControl invoiceHeaderAIIUserControl1;
		internal Enterprise.ZArchitecture.GUI.ZTabPage OrganisationTabPage;
		internal InvoiceHeaderOrganisationsUserControl invoiceHeaderOrganisationsUserControl1;
	}
}
