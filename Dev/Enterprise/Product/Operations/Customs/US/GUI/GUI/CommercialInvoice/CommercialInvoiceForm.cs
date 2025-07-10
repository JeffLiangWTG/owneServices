using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public enum ViewMode { ACS, ACECertifiedACS, ACECertifiedACE }

	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm()
		{
		}

		public CommercialInvoiceForm(JobComInvoiceHeader invoice)
			: base(invoice)
		{
			ACSViewModeMenuItem = new ZMenuItem("ACS View Mode", new EventHandler(ACSViewModeMenuItem_Click));
			ZFormMenuStrategy.AddActionsMenuItem(this, ACSViewModeMenuItem);

			ACECertifiedACSViewModeMenuItem = new ZMenuItem("ACE Certified in ACS View Mode", new EventHandler(ACEViewModeMenuItem_Click));
			ZFormMenuStrategy.AddActionsMenuItem(this, ACECertifiedACSViewModeMenuItem);

			ACECertifiedACEViewModeMenuItem = new ZMenuItem("ACE Certified in ACE View Mode", new EventHandler(ACECertifiedViewModeMenuItem_Click));
			ZFormMenuStrategy.AddActionsMenuItem(this, ACECertifiedACEViewModeMenuItem);

			if (invoice.IsExport)
			{
				ACSViewModeMenuItem.Visible = false;
				ACECertifiedACSViewModeMenuItem.Visible = false;
				ACECertifiedACEViewModeMenuItem.Visible = false;
			}
			else
			{
				SetDefultViewMode();
			}
		}

		internal ZMenuItem ACECertifiedACSViewModeMenuItem;
		internal ZMenuItem ACSViewModeMenuItem;
		internal ZMenuItem ACECertifiedACEViewModeMenuItem;
		internal ViewMode CurrentViewMode = ViewMode.ACECertifiedACE;

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			invoiceHeaderAIIUserControl1.ManageDeclarationRelatedControlsVisibility(false);
			invoiceHeaderAIIUserControl1.AIIInvoiceDateDateEdit.Visible = false;
			invoiceHeaderOrganisationsUserControl1.UpdateControlVisibilityAndCaptions(false);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				invoiceHeaderAIIUserControl1.SetDataBinding(dataSource, dataMember);
				invoiceHeaderOrganisationsUserControl1.SetDataBinding(dataSource, dataMember);

				ManageAIIRelatedTabsVisibility();
			}
		}

		void ACSViewModeMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentViewMode != ViewMode.ACS)
			{
				CurrentViewMode = ViewMode.ACS;
				SetMenuItemChecked();
				var declaration = Invoice.JobDeclaration;
				if (declaration != null)
				{
					using (declaration.SuspendSettingHasChanges())
					using (declaration.SuspendDataChangeByFakeDeclaration())
					{
						declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
						declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
						declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
					}
				}

				ReLoadLinesTabPage();
			}
		}

		void ACEViewModeMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentViewMode != ViewMode.ACECertifiedACS)
			{
				CurrentViewMode = ViewMode.ACECertifiedACS;
				SetMenuItemChecked();
				var declaration = Invoice.JobDeclaration;
				if (declaration != null)
				{
					using (declaration.SuspendSettingHasChanges())
					using (declaration.SuspendDataChangeByFakeDeclaration())
					{
						declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
						declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
						declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
					}
				}

				ReLoadLinesTabPage();
			}
		}

		void ACECertifiedViewModeMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentViewMode != ViewMode.ACECertifiedACE)
			{
				CurrentViewMode = ViewMode.ACECertifiedACE;
				SetDefultViewMode();
				ReLoadLinesTabPage();
			}
		}

		void SetDefultViewMode()
		{
			SetMenuItemChecked();
			var declaration = Invoice.JobDeclaration;
			if (declaration != null && CurrentViewMode == ViewMode.ACECertifiedACE)
			{
				using (declaration.SuspendSettingHasChanges())
				using (declaration.SuspendDataChangeByFakeDeclaration())
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
			}
		}

		void SetMenuItemChecked()
		{
			ACSViewModeMenuItem.Checked = CurrentViewMode == ViewMode.ACS;
			ACECertifiedACSViewModeMenuItem.Checked = CurrentViewMode == ViewMode.ACECertifiedACS;
			ACECertifiedACEViewModeMenuItem.Checked = CurrentViewMode == ViewMode.ACECertifiedACE;
		}

		void ReLoadLinesTabPage()
		{
			RemoveLinesTabPageControls();
			LoadLinesTabPage();
		}

		protected override void ManageControlVisibilityWhenJZ_MessageTypeIsChanged()
		{
			base.ManageControlVisibilityWhenJZ_MessageTypeIsChanged();
			ManageAIIRelatedTabsVisibility();
			ACECertifiedACSViewModeMenuItem.Visible = !Invoice.IsExport;
			ACSViewModeMenuItem.Visible = !Invoice.IsExport;
			ACECertifiedACEViewModeMenuItem.Visible = !Invoice.IsExport;
		}

		void ManageAIIRelatedTabsVisibility()
		{
			bool shouldBeVisible = !Invoice.IsExport;

			AIITabPage.TabVisible = shouldBeVisible;
			OrganisationTabPage.TabVisible = shouldBeVisible;

			if (shouldBeVisible)
			{
				this.MainTabControl.TabPages.Remove(this.AIITabPage);
				this.MainTabControl.TabPages.Remove(this.OrganisationTabPage);

				this.MainTabControl.TabPages.Insert(this.AIITabPage, 2);
				this.MainTabControl.TabPages.Insert(this.OrganisationTabPage, 3);
			}
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result = null;

			if (JobDeclaration.IsExport)
			{
				result = new USExportInvoiceLineUserControl();
			}
			else if (CurrentViewMode == ViewMode.ACS)
			{
				result = new USACSImportInvoiceLineUserControl();
			}
			else if (CurrentViewMode == ViewMode.ACECertifiedACS || CurrentViewMode == ViewMode.ACECertifiedACE)
			{
				result = new USACEImportInvoiceLineUserControl();
				SetDefultViewMode();
			}

			return result;
		}

		protected override void SetupForUseWithNoDeclaration()
		{
			base.SetupForUseWithNoDeclaration();
			RemoveColumn(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber);
			RemoveColumn(JobComInvoiceLine.Schema.JI_Calc_EntryNumber);
			RemoveColumn(JobComInvoiceLine.Schema.JI_Calc_XTN);
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl()
		{
			return new InvoiceHeaderUserControl();
		}
	}
}
