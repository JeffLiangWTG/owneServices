using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.GUI
{
	public partial class CommercialInvoiceForm : ZForm, IPreviousNextControlOverrideProvider, IDataGridLayoutIdentifierRoot
	{
		public CommercialInvoiceForm()
		{
		}

		public CommercialInvoiceForm(BaseJobComInvoiceHeader invoice)
		{
			fInvoice = invoice;
			CommercialInvoiceWorkflowTabPage.Initialize(invoice);
			SetDataBinding(new FakeDeclarationCreatorForInvoice(invoice).HeaderData, "");

			LinesTabPage.LazyCreateControls += LinesTabPage_LazyCreateControls;
			HookMessageTypeChangeEvent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			SetupHeaders();
			SetUpDataTransferMenu();

			invoice.RegisterEditableChildObject(invoice.JobComInvoiceLines); // for this form only, as dec is not validated

			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, RoutingTabIndex);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			AddBrokerageMenuItems();
			lockManager = new CommercialInvoiceCustomsControlLockManager(invoice, this);
			lockManager.Register(CommercialInvoiceCustomsControlLockManager.TabPages.Codes.Lines, LinesTabPage);
		}
		readonly CommercialInvoiceCustomsControlLockManager lockManager;

		public virtual int RoutingTabIndex
		{
			get { return 1; }
		}

		public override bool AllowExportNativeXml
		{
			get
			{
				return false;
			}
		}

		void LinesTabPage_LazyCreateControls(object sender, EventArgs e)
		{
			LoadLinesTabPage();
		}

		void SetUpDataTransferMenu()
		{
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItem);
		}

		List<MenuItem> ExportToXmlMenuItem
		{
			get
			{
				return new ExportToXmlMenuItemSet<BaseJobComInvoiceHeader>(() => Exporter, Invoice);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				return new InvoiceDataTransferExporter(StandAloneInvoiceValueObjectDataAdapter.New(), true);
			}
		}

		public void LoadLinesTabPage()
		{
			if (LinesTabPage.Controls.Count == 0)
			{
				fInvoiceLineUserControl = GetNewInvoiceLineUserControl();
				var declarationControl = fInvoiceLineUserControl as DeclarationInvoiceLineUserControl;
				if (declarationControl != null)
				{
					declarationControl.JobDeclaration = JobDeclaration;
				}

				fInvoiceLineUserControl.Dock = DockStyle.Fill;
				LinesTabPage.Controls.Add(fInvoiceLineUserControl);

				if (declarationControl != null)
				{
					declarationControl.InitializeGridLayout();
				}

				SetupForUseWithNoDeclaration();
				fInvoiceLineUserControl.SetDataBinding(JobDeclaration, "");

				if (Invoice != null && !string.IsNullOrEmpty(Invoice.JZ_InvoiceNumber))
				{
					BorderWiseAsyncBatchTariffProcessorProvider.ClassifyDeclarationJobInBorderWise(this, JobDeclaration);
				}
			}
		}

		void HookMessageTypeChangeEvent()
		{
			Invoice.JZ_MessageTypeInfo.ValueChanged -= JZ_MessageTypeInfo_ValueChanged;
			Invoice.JZ_MessageTypeInfo.ValueChanged += JZ_MessageTypeInfo_ValueChanged;
		}

		void JZ_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ManageControlVisibilityWhenJZ_MessageTypeIsChanged();
		}

		protected virtual void ManageControlVisibilityWhenJZ_MessageTypeIsChanged()
		{
			RemoveLinesTabPageControls();
		}

		public void RemoveLinesTabPageControls()
		{
			if (LinesTabPage != null)
			{
				foreach (Control currentControl in LinesTabPage.Controls)
				{
					currentControl.Dispose();
				}
				LinesTabPage.Controls.Clear();
				(LinesTabPage as ZBindingTabPage)?.ResetBinding();
			}
		}

		void AddBrokerageMenuItems()
		{
			TopLevelMenu = GetNewTopLevelMenu();
			((CommercialInvoiceEDIMenu)TopLevelMenu).Declaration = JobDeclaration;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), TopLevelMenu);
		}

		protected CommercialInvoiceEDIMenu GetNewTopLevelMenu()
		{
			if (fMenu == null)
			{
				fMenu = GetNewTopLevelMenuCore();
			}

			return fMenu;
		}
		CommercialInvoiceEDIMenu fMenu;

		protected virtual CommercialInvoiceEDIMenu GetNewTopLevelMenuCore()
		{
			return new CommercialInvoiceEDIMenu();
		}

		protected override IBusiness GetTopLevelBusinessEntityForPlugIn()
		{
			return Invoice;
		}

		class InvoicePreviousNextControl : ZPreviousNextControl
		{
			public InvoicePreviousNextControl(ModuleResultsBusinessObject bizO, ZController controller)
				: base(bizO, controller)
			{
			}

			protected override BusinessObject GetBusinessObjectToEdit()
			{
				return Controller.Factory.Load<BaseJobComInvoiceHeader>(BusinessEntity.CurrentPK);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			CommercialInvoiceStmNoteTabPage.SetDataBinding(Invoice, "");
			CommercialInvoiceEventTabPage.SetDataBinding(Invoice, "");
			CommercialInvoiceWorkflowTabPage.SetDataBinding(Invoice, "");
			base.SetDataBinding(dataSource, dataMember);
		}

		public override IBusiness BusinessEntity
		{
			get { return Invoice; }
		}

		protected override IBusiness BusinessEntityForValidation
		{
			get { return Invoice; }
		}

		public override IBusiness BusinessEntityForHasChanges
		{
			get { return Invoice; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("8e3f0bb1-ee50-4bb0-83a3-c8ae1fa1cf68", "Commercial Invoice"); }
		}

		public BaseJobComInvoiceHeader Invoice
		{
			get { return fInvoice == null && JobDeclaration is ICommonInvoiceDataProvider provider ? provider.Invoices.First() : fInvoice; }
		}

		public ICommonInvoiceDataProvider JobDeclaration
		{
			get { return (ICommonInvoiceDataProvider)DataSource; }
		}

		/// <summary>
		/// Called once on form initialisation
		/// </summary>
		protected virtual CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

		void SetupHeaders()
		{
			var headerControl = GetHeaderUserControl();
			headerControl.Invoice = Invoice;
			headerControl.Dock = DockStyle.Fill;
			HeaderTabPage.Controls.Add(headerControl);
		}

		/// <summary>
		/// This is called every time the tab page changes to the lines tab.
		/// </summary>

		[Browsable(false)]
		public BaseInvoiceLineUserControl InvoiceLineUserControl
		{
			get { return fInvoiceLineUserControl; }
		}

		#region IPreviousNextControlOverrideProvider Members

		bool IPreviousNextControlOverrideProvider.OverridesSetPreviousNextControlParentAndPosition
		{
			get { return false; }
		}

		bool IPreviousNextControlOverrideProvider.ShouldDoBaseSetPreviousNextControlParentAndPosition
		{
			get { return true; }
		}

		bool IPreviousNextControlOverrideProvider.OverridesGetPreviousNextControl
		{
			get { return true; }
		}

		void IPreviousNextControlOverrideProvider.SetPreviousNextControlParentAndPosition(ZPreviousNextControl control)
		{
			throw new NotImplementedException();
		}

		ZPreviousNextControl IPreviousNextControlOverrideProvider.GetPreviousNextControl(ModuleResultsBusinessObject bizO, ZController controller)
		{
			return new InvoicePreviousNextControl(bizO, controller);
		}

		#endregion

		protected BaseInvoiceLineUserControl fInvoiceLineUserControl;

		protected virtual BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			return new GeneralCountryInvoiceLineUserControl();
		}

		protected virtual void SetupForUseWithNoDeclaration()
		{
			RemoveColumn(BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
		}

		protected void RemoveColumn(string columnName)
		{
			var jI_Calc_InvoiceColumnInfo = InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(columnName);
			if (jI_Calc_InvoiceColumnInfo != null)
			{
				InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(jI_Calc_InvoiceColumnInfo);
			}
		}
		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && fInvoiceLineUserControl != null)
			{
				BorderWiseAsyncBatchTariffProcessorProvider.SendMessageAndDisposeConnectionIfNeeded(JobDeclaration, null, true);
			}
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (lockManager != null)
				{
					lockManager.Dispose();
				}

				if (fInvoiceLineUserControl != null)
				{
					BorderWiseAsyncBatchTariffProcessorProvider.SendMessageAndDisposeConnectionIfNeeded(JobDeclaration, null);
					fInvoiceLineUserControl.Dispose();
				}

				fInvoice.JZ_MessageTypeInfo.ValueChanged -= JZ_MessageTypeInfo_ValueChanged;

				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}
				}
			}
			base.Dispose(disposing);
		}

		private IContainer components;

		private void MainTabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab == LinesTabPage)
			{
				LoadLinesTabPage();
			}
		}

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				var branch = fInvoice != null && fInvoice.Branch != null ? fInvoice.Branch : GlbBranch.CurrentBranch;

				return branch.Country != null ? branch.Country.RN_Code : branch.GB_RL_NKHomePort.Left(2);
			}
		}

		#endregion
	}
}
