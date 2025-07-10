using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	[ZArchitecture.GUI.Testing.TestExcludeZWinFormsAllHaveFormBashers]
	public partial class BaseJobDeclarationForm : ZForm, ITabVisibilityDeciderPersistence, IDevToolMessageBuilderMappingPathConfigurator, ICustomerServiceMenuSectionCodeOverridable
	{
		public BaseJobDeclarationForm()
		{
		}

		public BaseJobDeclarationForm(BaseJobDeclaration declaration)
			: base(declaration)
		{
			ConstructMe(declaration);
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItem);
			if (declaration.SupportScreeningPresentation)
			{
				var deniedPartyScreeningPresentationManager = new DeniedPartyScreeningPresentationManager();
				deniedPartyScreeningPresentationManager.CreateMenusForJob(this);
				new DeniedPartyScreeningActionsProvider(this, declaration).AddJobsMenuItem();
			}

			AddPlugins();

			ServicesSelectionGuiProvider.Register(declaration.Factory);

			if (declaration.SupportSelectingLinesToPrint)
			{
				declaration.OnGetLinesToPrint += Declaration_OnGetLinesToPrint;
			}
			declaration.OnGetTransportToPrint += Declaration_OnGetTransportToPrint;

			AddScreeningLogsTabPage();

			AddComplianceRiskMessageBannerIfNeeded();
		}

		protected virtual void AddPlugins()
		{
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, RoutingTabIndex);
			PlugIns.Add(ControllerIDs.LandedCosting);
			PlugIns.AddJobInvoicing(Declaration.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.CartagePlugin);
			PlugIns.Add(ControllerIDs.DtbBooking);
			PlugIns.Add(ControllerIDs.Audit);
			if (ComplianceRiskHelper.IsCustomsEnabledComplianceWise)
			{
				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
			}
		}

		public virtual int RoutingTabIndex
		{
			get { return 1; }
		}

		public BaseCustomsBrokerageUserControl CustomsBrokerageUserControl
		{
			get { return fCustomsBrokerageUserControl; }
		}

		public override string FormCaption
		{
			get
			{
				string caption = FormCaptionCore;

				if (!this.IsDesignMode() && !Declaration.JE_DeclarationReference.IsEmpty)
				{
					caption += " - " + Declaration.JE_DeclarationReference;
				}
				return caption;
			}
		}

		protected virtual string FormCaptionCore
		{
			get { return Res.GetString("ADA33BC3-BDA9-4C57-8343-D37DB1E82986", "Customs Declaration"); }
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
		}

		#region Implementation

		protected BaseJobDeclaration fJobDeclaration;

		void Table_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e)
		{
			var row = ((INeedRow)fJobDeclaration)?.Row;
			if (row != null && e.Row == row)
			{
				ErrorReporter.ReportOnce("Declaration should not be deleted", FormattableString.Invariant($"Declaration row is being deleted (Action={e.Action}, State={row.RowState},PK={fJobDeclaration.PK})"));
			}
		}

		void ConstructMe(BaseJobDeclaration declaration)
		{
			declaration.ExternalFactoryRefreshEnabled = true;
			fJobDeclaration = declaration;
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(fJobDeclaration.Factory, fJobDeclaration.PK);
			var table = ((INeedRow)declaration).Row.Table;
			table.RowDeleting += Table_RowDeleting;
			LoadBrokerageUserControl();
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl);

			TopLevelMenu = (ZMenuItem)GetNewTopLevelMenu();
			((IEDIMenu)TopLevelMenu).Declaration = declaration;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), TopLevelMenu);

			declaration.LicenceLogin += Declaration_LicenceLogin;
			declaration.OnGetCusContainersToPrint += new CusContainersToPrintEventHandler(JobDeclaration_OnGetCusContainersToPrint);
			declaration.BondedWarehouseLicenceLogin += Declaration_LicenceLogin;
			declaration.UpdateFromRoutingTabOnLoadIfNoMessagesSentYet();
		}

		public BaseJobDeclaration Declaration
		{
			get { return fJobDeclaration; }
		}

		protected IEDIMenu GetNewTopLevelMenu()
		{
			if (fMenu == null)
			{
				fMenu = fJobDeclaration != null && fJobDeclaration.JE_IsCancelled ? new EDIMenuStub() : GetNewTopLevelMenuCore();
			}

			return fMenu;
		}
		IEDIMenu fMenu;

		protected virtual IEDIMenu GetNewTopLevelMenuCore()
		{
			return new EDIMenu();
		}

		protected override ZTabControl TopLevelTabControl
		{
			get
			{
				if (fCustomsBrokerageUserControl != null)
				{
					return fCustomsBrokerageUserControl.MainTabControl;
				}
				else
				{
					return null;
				}
			}
		}

		void LoadBrokerageUserControl()
		{
			fCustomsBrokerageUserControl = GetBrokerageUserControl();
			fCustomsBrokerageUserControl.Dock = DockStyle.Fill;
			fCustomsBrokerageUserControl.Name = "CustomsBrokerageUserControl";
			Controls.Add(fCustomsBrokerageUserControl);
			fCustomsBrokerageUserControl.JobDeclaration = (BaseJobDeclaration)this.BusinessEntity;
			fCustomsBrokerageUserControl.BringToFront();
		}

		protected virtual BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			return new BaseCustomsBrokerageUserControl();
		}

		protected virtual DeclarationXmlDataTransferExporter GetNewDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter adapter)
		{
			return new DeclarationXmlDataTransferExporter(adapter, true);
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (fJobDeclaration != null)
			{
				JobComInvoiceLinePartSynchronisationManager.StopManagingWhenActiveDeciderPKWasDisposed(fJobDeclaration.Factory, fJobDeclaration.PK);

				fJobDeclaration.ExternalFactoryRefreshEnabled = false;
				var table = ((INeedRow)fJobDeclaration).Row.Table;
				table.RowDeleting -= Table_RowDeleting;
			}

			if (disposing)
			{
				fMenu?.Dispose();
				if (fCustomsBrokerageUserControl != null)
				{
					fCustomsBrokerageUserControl.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
				if (Declaration != null)
				{
					Declaration.BondedWarehouseLicenceLogin -= Declaration_LicenceLogin;
					Declaration.LicenceLogin -= Declaration_LicenceLogin;
					Declaration.OnGetCusContainersToPrint -= new CusContainersToPrintEventHandler(JobDeclaration_OnGetCusContainersToPrint);
					if (Declaration.SupportSelectingLinesToPrint)
					{
						Declaration.OnGetLinesToPrint -= Declaration_OnGetLinesToPrint;
					}
					Declaration.OnGetTransportToPrint -= Declaration_OnGetTransportToPrint;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(MainMenu, TopLevelTabControl).Enabled = true;
				RegisterRoutingForLock();

				using (Declaration.SuspendSettingHasChanges())
				{
					if (Declaration is not IComplianceItemRiskStatusProvider provider || !provider.IsEnabledComplianceWise)
					{
						new DeniedPartyScreeningPresentationManager().ResynchronizeScreeningStatus(false, new[] { Declaration }, null);
					}
				}
			}
		}

		void RegisterRoutingForLock()
		{
			var lockManager = CustomsBrokerageUserControl?.LockManager;

			if (lockManager != null)
			{
				var plugin = PlugIns.GetPlugIn(ControllerIDs.Routing);
				lockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Routing, plugin);
			}
		}

		void Declaration_LicenceLogin(object sender, LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(this);
		}

		void JobDeclaration_OnGetCusContainersToPrint(object sender, CusContainersToPrintEventArgs e)
		{
			using (var form = new DocumentCusContainerForm(e.DocumentCusContainerCollectionHeader))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				e.ContinueToPrint = (form.DialogResult == DialogResult.Yes);
			}
		}

		List<MenuItem> ExportToXmlMenuItem
		{
			get
			{
				return new ExportToXmlMenuItemSet<BaseJobDeclaration>(() => Exporter, (BaseJobDeclaration)BusinessEntity);
			}
		}

		internal IXmlDataTransferExporter Exporter
		{
			get
			{
				var exportor = GetNewDeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter.New());
				exportor.DefaultFileName = Declaration.JE_DeclarationReference + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss");
				if (!(new ZString(SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value).IsEmpty))
				{
					exportor.InitialDirectory = SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value;
				}
				return exportor;
			}
		}

		#region SkipRecentItems

		public bool? SkipRecentItems { get; set; }

		protected override void SaveToRecentItems()
		{
			if (!SkipRecentItems.HasValue || !SkipRecentItems.Value)
			{
				base.SaveToRecentItems();
			}
		}

		#endregion

		#endregion

		#region Apportionment on saving

		protected virtual IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			var declaration = Declaration;
			yield return new MergePreSaveDialogStrategy(declaration);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = new BaseJobDeclarationSecurityAlertHelper(Declaration).ShowSecurityAlertMessage();

			if (result == ContinueWithSave.Yes)
			{
				if (Declaration.ApportionmentDirty)
				{
					using (var progressForm = new ApportionmentProgressForm(Declaration))
					{
						ZFormModaliser.Show(progressForm, this);
						Declaration.ResumeApportionment();
						progressForm.Close();
					}
				}

				if (Declaration.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship)
				{
					if (ShowConfirmationForNewSupplierBuyerRelationship() == DialogResult.Yes)
					{
						Declaration.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
					}
				}

				foreach (var preSaveDialogStrategy in GetPreSaveDialogStrategies())
				{
					result = preSaveDialogStrategy.ShowPreSaveDialogs(result);
					if (result == ContinueWithSave.No)
					{
						break;
					}
				}

				if (result == ContinueWithSave.Yes)
				{
					if (!Declaration.IsAmendmentDetectionSuspended)
					{
						var messageManageableDeclaration = Declaration as IMessageManageableBizObj;
						if (messageManageableDeclaration != null && messageManageableDeclaration.IsInAStatusAmendmentSendable)
						{
							var manager = messageManageableDeclaration.GetMessageManagerForAmendmentDetection();
							if (manager != null)//Depending on the message type, amendment might not be supported and thus, manager can be null
							{
								result = GetNewMessagingActionsController().DetermineRequiredMessagesAndSendThem(manager);
							}
						}
					}
					if (result == ContinueWithSave.Yes && Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed && !Declaration.IsDrawback)
					{
						if (Declaration.InvoicesMentionNewOrInactiveProducts
							&& DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(Declaration)
							&& DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(Declaration))
						{
							ZFormModaliser.ShowDialogAndDispose(new ProductCreationConfirmationForm(Declaration));
						}
						if (Declaration.InvoicesMentionProductsWithoutMatchingClassification)
						{
							ZFormModaliser.ShowDialogAndDispose(new ProductClassificationCreationConfirmationForm(Declaration));
						}
					}
				}

				if (result == ContinueWithSave.Yes)
				{
					ShowInvoiceLinesAreNotLinkedToTheContainerDialog();

					BorderWiseAsyncBatchTariffProcessorProvider.SendMessageAndDisposeConnectionIfNeeded(Declaration, CustomsBrokerageUserControl.GetInvoicePksAction(), true);
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				result = base.ShowPreSaveDialogs();
			}

			if (result == ContinueWithSave.Yes)
			{
				var declarationAndBrokerageCommon = GetShipmentAndBrokergeCommon(Declaration);
				result = declarationAndBrokerageCommon.IsSupervisorApproved();
			}

			CustomsBrokerageUserControl.ClearInvoicePksAction();

			return result;
		}

		protected virtual BaseShipmentAndBrokerageCommon GetShipmentAndBrokergeCommon(BaseJobDeclaration declaration)
		{
			return new BaseShipmentAndBrokerageCommon(declaration);
		}

		protected void ShowInvoiceLinesAreNotLinkedToTheContainerDialog()
		{
			if (Declaration.ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog)
			{
				var shouldMarkContainersAsIsForInvoiceLine = Globals.Message.Show(
					Res.GetString("9DC3B863-1850-43c9-B843-6D183A1B53B7", "There are Invoice Lines which are containerized but are not linked to the container.\r\nDo you want to link it automatically to the container?"),
					Res.GetString("CC69FF74-58FD-4a5a-8F48-90B6A32AC302", "Invoice Lines are not linked to the container"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (shouldMarkContainersAsIsForInvoiceLine == DialogResult.Yes)
				{
					Declaration.MarkContainersAsIsForInvoiceLine();
				}
			}
		}

		protected virtual SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return new SendsMessagesToCustomsGUI();
		}

		DialogResult ShowConfirmationForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(Res.GetString("EB1DF375-AD8C-46C3-82D0-014991900CF6", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"), Res.GetString("59EF6A40-E369-451D-8174-5B4E363B93BD", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
		}

		#endregion

		#region LineToPrint

		protected void Declaration_OnGetLinesToPrint(object sender, System.ComponentModel.CancelEventArgs e)
		{
			using (var form = new LineToPrintForm(Declaration))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
				e.Cancel = form.DialogResult != DialogResult.OK;
			}
		}

		#endregion

		#region TransportToPrint

		void Declaration_OnGetTransportToPrint(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var form = new TransportToPrintForm(Declaration);
			try
			{
				ZFormModaliser.ShowDialogAndDispose(form);
				e.Cancel = form.DialogResult != DialogResult.OK;
			}
			finally
			{
				form.Dispose();
			}
		}

		#endregion

		#region ITabVisibilityDeciderPersistence Members

		bool ITabVisibilityDeciderPersistence.HasTabVisiblePersisted
		{
			get { return !Declaration.JE_InvisibleTabsXML.IsEmpty; }
		}

		bool ITabVisibilityDeciderPersistence.RetrieveTabPageVisible(ZTabPage page)
		{
			return this.RetrieveTabPageVisible(page, Declaration, JobDeclarationSchema.JE_InvisibleTabsXML);
		}

		void ITabVisibilityDeciderPersistence.StoreTabVisible(ZTabPage page)
		{
			this.StoreTabVisible(page, Declaration, JobDeclarationSchema.JE_InvisibleTabsXML);
		}

		#endregion

		#region Control Lock

		protected internal virtual bool EnableControlLock
		{
			get { return true; }
		}

		#region Denied Party Screening

		void AddScreeningLogsTabPage()
		{
			if (!fCustomsBrokerageUserControl.EventTabPage.IsDisposed)
			{
				var screenStatusControl = new RelatedDeniedPartyScreeningStatusControl();
				screenStatusControl.SetBindingMember("RelatedOrgPartyScreeningStatusCollection");

				ComplianceLogTabHelper.AddLogTabIfNeeded(Declaration, fCustomsBrokerageUserControl.EventTabPage, screenStatusControl);
			}
		}

		void AddComplianceRiskMessageBannerIfNeeded()
		{
			ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(this);
		}

		#endregion

		#endregion

		#region ICustomerServiceMenuSectionCodeOverridable

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode
			=> TopLevelTabControl.SelectedTab.Name == ComplianceWiseConstants.ComplianceRiskTabPageName
				? ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise
				: ModuleTreeCustomerServiceMenuSectionList.Codes.Customs;

		#endregion

		bool IDevToolMessageBuilderMappingPathConfigurator.CanDisplayMessageBuilderMappingPath => true;
	}
}
