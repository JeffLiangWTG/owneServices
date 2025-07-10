using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.JobDeclaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsBrokerageUserControl : BaseCustomsEntryUserControl, IDataGridLayoutIdentifierRoot, ISupportMultipleResourceStringDataSupporter
	{
		public BaseCustomsBrokerageUserControl()
			: this(null)
		{
		}

		public BaseCustomsBrokerageUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			InitializeLazyCreate();
		}

		bool decHasBeenSet;

		Dictionary<ZGuid, string> invoicePksAction;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				if (!decHasBeenSet)
				{
					if (value != null)
					{
						base.JobDeclaration = value;
						decHasBeenSet = true;

						JobDeclaration.MessageInitiator = NewSendsMessagesToCustomsGUI();
						LoadCustomsEntryUserControl();
						WorkflowTabPage.Initialize(value);

						var isPluggedIntoShipment = value.IsPluggedIntoShipment;

						if (isPluggedIntoShipment)
						{
							BrokerageStmNoteTabPage.Dispose();
							EventTabPage.Dispose();
							MainTabControl.SelectedTab = DeclarationTabPage;
						}
						JobDeclaration_JE_MessageTypeChanged(this, EventArgs.Empty);
						ChangeContainerTabVisibility();

						SetEntryInstructionsTabVisibility();

						var enableControlsLock = ((ParentForm as BaseJobDeclarationForm)?.EnableControlLock ?? false) || isPluggedIntoShipment;
						if (enableControlsLock && CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Any())
						{
							RegisterControlsForLock(value);
						}

						OnJobDeclarationSet();
					}
				}
			}
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageTypeInfo.ValueChanged -= JobDeclaration_JE_MessageTypeChanged;
			declaration.MergedSuccessfully -= JobDeclaration_MergedSuccessfully;
			declaration.JE_ApplicationCodeInfo.ValueChanged -= JobDeclaration_JE_ApplicationCodeChanged;
			declaration.OnTransportModeChanged -= ChangeContainerTabVisibility;
			declaration.JE_ContainerModeInfo.ValueChanged -= JE_ContainerModeInfo_ValueChanged;
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			declaration.JE_MessageTypeInfo.ValueChanged += JobDeclaration_JE_MessageTypeChanged;
			declaration.MergedSuccessfully += JobDeclaration_MergedSuccessfully;
			declaration.JE_ApplicationCodeInfo.ValueChanged += JobDeclaration_JE_ApplicationCodeChanged;
			declaration.OnTransportModeChanged += ChangeContainerTabVisibility;
			declaration.JE_ContainerModeInfo.ValueChanged += JE_ContainerModeInfo_ValueChanged;
		}

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => JobDeclaration;

		protected virtual void JobDeclaration_JE_ApplicationCodeChanged(object sender, EventArgs e)
		{
			SetEntryInstructionsTabVisibility();
		}

		protected virtual void SetEntryInstructionsTabVisibility()
		{
			EntryInstructionDetailsTabPage.TabRelevant = EntryInstructionsTabVisibleForCountry;
		}

		public virtual bool EntryInstructionsTabVisibleForCountry => false;

		protected virtual void OnJobDeclarationSet()
		{
		}

		public void SetDeclarationReadOnly(bool readOnly)
		{
			JobDeclaration.SetReadOnlyIncludingChildren(readOnly);
		}

		public IStmNoteControl GetStmNoteControl()
		{
			return BrokerageStmNoteTabPage;
		}

		public Dictionary<ZGuid, string> GetInvoicePksAction()
		{
			return invoicePksAction;
		}
		public void ClearInvoicePksAction()
		{
			invoicePksAction?.Clear();
		}

		#region WorkflowTabPage Visibility

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (FormHasWorkflowTabPage())
			{
				WorkflowTabPage.Dispose();
			}
		}

		public bool FormHasWorkflowTabPage()
		{
			Control current = Parent;
			while (current != null)
			{
				ZTabControl tabControl = current as ZTabControl;
				if (tabControl != null && TabControlHasWorkflowTabPage(tabControl))
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		bool TabControlHasWorkflowTabPage(ZTabControl tabControl)
		{
			foreach (ZTabPage tabPage in tabControl.TabPages)
			{
				if (tabPage is ZWorkflowTabPage)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Control Lock

		void RegisterControlsForLock(BaseJobDeclaration declaration)
		{
			lockManager = new CustomsControlLockManager(declaration, this);

			var ignoreControlsForDeclarationTabpage = new List<string>();

			var declarationUserControl = fBaseCustomsEntryUserControl as BaseCustomsDeclarationUserControl;
			if (declarationUserControl != null)
			{
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationCustom, declarationUserControl.ShipmentCustomFieldsPage);
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers, declarationUserControl.NumbersTabPage);
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrders, declarationUserControl.OrdersTabPage);
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrganizations, declarationUserControl.OrganisationsTabPage);
				LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationServices, declarationUserControl.DocsTabPage);

				ignoreControlsForDeclarationTabpage.Add(declarationUserControl.RightTabControl.Name);
			}

			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Declaration, DeclarationTabPage, ignoreControlsForDeclarationTabpage.ToArray());
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Containers, ContainerTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Packing, PackingTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceGroups, InvoiceGroupingTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceHeaders, InvoicesTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceLines, InvoiceLinesTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Misc, MiscOptionsTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.MessageOrEntries, MessagesTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationPickupOrDelivery, PickupTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationPickupOrDelivery, DeliveryTabPage);
			LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.EntryInstructions, EntryInstructionDetailsTabPage);

			OtherRegisters();
		}

		protected virtual void OtherRegisters()
		{
		}

		protected internal CustomsControlLockManager LockManager => lockManager;
		CustomsControlLockManager lockManager;

		#endregion

		#region Debug Control Properties

#if DEBUG
		[Browsable(false)]
		public BaseCustomsDeclarationUserControl DeclarationUserControlForTesting
		{
			get { return fBaseCustomsEntryUserControl as BaseCustomsDeclarationUserControl; }
		}

		public BaseCustomsEntryUserControl DeclarationUserControl
		{
			get { return fBaseCustomsEntryUserControl; }
		}

		[Browsable(false)]
		public BaseCustomsSupplierHeaderUserControl SupplierHeaderUserControl
		{
			get { return fCustomsSupplierHeader; }
		}

		[Browsable(false)]
		public BaseInvoiceGroupingUserControl InvoiceGroupUserControl
		{
			get { return fInvoiceGroupingControl; }
		}

		[Browsable(false)]
		public BaseInvoiceLineUserControl InvoiceLinesUserControl
		{
			get { return fInvoiceLines; }
		}

		[Browsable(false)]
		public BaseCustomsCusContainersUserControl ContainerUserControl
		{
			get { return fContainerUserControl; }
		}

		[Browsable(false)]
		public BaseMiscOptionsUserControl MiscOptions
		{
			get { return fMiscOptionsUserControl; }
		}

		[Browsable(false)]
		public DynamicMiscOptionsUserControl DynamicMiscOptions
		{
			get { return fDynamicMiscOptionsUserControl; }
		}

		[Browsable(false)]
		public IBasePackingControl Packing
		{
			get { return fPackingUserControl; }
		}

		[Browsable(false)]
		public BaseCustomsEntryUserControl MessageUserControl
		{
			get { return fMessageUserControl; }
		}

		[Browsable(false)]
		public BaseCustomsEntryUserControl CustomsEntryInstructionUserControl => fBaseCustomsEntryInstructionUserControl;

		[Browsable(false)]
		public BaseCustomsPickupUserControl CustomsPickupUserControl => fBaseCustomsPickupUserControl;

		[Browsable(false)]
		public BaseCustomsDeliveryUserControl CustomsDeliveryUserControl => fBaseCustomsDeliveryUserControl;
#endif
		#endregion

		#region Dialogs

		public bool ContinueWithDeleteAndWithdrawEntry()
		{
			return GetConfirmationForSendingWithdrawal() == DialogResult.OK;
		}

		#endregion

		#region Implementation

		#region OnLoad/Loading User Controls

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (JobDeclaration != null && JobDeclaration.Shipment != null && MainTabControl.PlugIns.GetPlugIn(ControllerIDs.LandedCosting) == null)
			{
				MainTabControl.PlugIns.Add(ControllerIDs.LandedCosting);
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (lockManager != null)
				{
					lockManager.Dispose();
				}

				if (fCusContainersGridLayoutPersister != null)
				{
					fCusContainersGridLayoutPersister.Dispose();
				}

				if (fContainerUserControl != null)
				{
					fContainerUserControl.Dispose();
				}

				if (fPackingUserControl != null)
				{
					fPackingUserControl.Dispose();
				}

				if (fBaseCustomsEntryUserControl != null)
				{
					fBaseCustomsEntryUserControl.Dispose();
				}

				if (fCustomsSupplierHeader != null)
				{
					fCustomsSupplierHeader.Dispose();
				}

				if (fMessageUserControl != null)
				{
					fMessageUserControl.Dispose();
				}

				if (fInvoiceLines != null)
				{
					fInvoiceLines.Dispose();
				}

				if (fMiscOptionsUserControl != null)
				{
					fMiscOptionsUserControl.Dispose();
				}

				if (fDynamicMiscOptionsUserControl != null)
				{
					fDynamicMiscOptionsUserControl.Dispose();
				}

				if (fBaseCustomsEntryInstructionUserControl != null)
				{
					fBaseCustomsEntryInstructionUserControl.Dispose();
				}

				if (fInvoiceLines != null || fCustomsSupplierHeader != null)
				{
					BorderWiseAsyncBatchTariffProcessorProvider.SendMessageAndDisposeConnectionIfNeeded(JobDeclaration, null);
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#region Container Tab Page

		public void LoadContainerTabPage()
		{
			if (ContainerTabPage.Controls.Count == 0 && ContainerTabPage.TabVisible)
			{
				fContainerUserControl = GetContainerUserControl();
				fContainerUserControl.JobDeclaration = JobDeclaration;
				fContainerUserControl.Dock = DockStyle.Fill;
				ContainerTabPage.Controls.Add(fContainerUserControl);

				BaseCustomsCusContainersWithTrackingUserControl control = fContainerUserControl as BaseCustomsCusContainersWithTrackingUserControl;
				control?.LoadPlugins();
				fContainerUserControl.BindingSource.ForceBinding(fContainerUserControl.CusContainersBoundGrid);
			}
		}

		protected virtual BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new BaseCustomsCusContainersWithTrackingUserControl();
		}

		#endregion

		#region Packing Tab Page

		public void LoadPackingTabPage()
		{
			if (PackingTabPage.Controls.Count == 0 && PackingTabPage.TabVisible)
			{
				fPackingUserControl = GetPackingUserControl();
				fPackingUserControl.JobDeclaration = JobDeclaration;
				fPackingUserControl.Dock = DockStyle.Fill;
				PackingTabPage.Controls.Add((UserControl)fPackingUserControl);
				var packingUserControl = fPackingUserControl as BaseCustomsPackingUserControl;
				if (packingUserControl != null)
				{
					packingUserControl.InitializeGridLayout();
				}
				fPackingUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		protected virtual IBasePackingControl GetPackingUserControl()
		{
			return new BaseCustomsPackingUserControl();
		}

		#endregion

		#region Pickup Tab Page

		public void LoadPickupTabPage()
		{
			if (PickupTabPage.Controls.Count == 0 && PickupTabPage.TabVisible)
			{
				fBaseCustomsPickupUserControl = GetCustomsPickupUserControl();
				fBaseCustomsPickupUserControl.JobDeclaration = JobDeclaration;
				fBaseCustomsPickupUserControl.Dock = DockStyle.Fill;
				PickupTabPage.Controls.Add(fBaseCustomsPickupUserControl);
				fBaseCustomsPickupUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		protected virtual BaseCustomsPickupUserControl GetCustomsPickupUserControl() => new BaseCustomsPickupUserControl();

		#endregion

		#region Delivery Tab Page

		public void LoadDeliveryTabPage()
		{
			if (DeliveryTabPage.Controls.Count == 0 && DeliveryTabPage.TabVisible)
			{
				fBaseCustomsDeliveryUserControl = GetCustomsDeliveryUserControl();
				fBaseCustomsDeliveryUserControl.JobDeclaration = JobDeclaration;
				fBaseCustomsDeliveryUserControl.Dock = DockStyle.Fill;
				DeliveryTabPage.Controls.Add(fBaseCustomsDeliveryUserControl);
				fBaseCustomsDeliveryUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		protected virtual BaseCustomsDeliveryUserControl GetCustomsDeliveryUserControl() => new BaseCustomsDeliveryUserControl();

		#endregion

		#region Invoice Grouping Tab Page

		public void LoadInvoiceGroupingTabPage()
		{
			if (fInvoiceGroupingControl == null)
			{
				fInvoiceGroupingControl = GetInvoiceGroupingUserControl();
				fInvoiceGroupingControl.Dock = DockStyle.Fill;
				InvoiceGroupingTabPage.Controls.Add(fInvoiceGroupingControl);
			}
		}

		protected virtual BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl()
		{
			return new BaseInvoiceGroupingUserControl();
		}

		#endregion

		#region Invoice Header Tab Page

		public void LoadInvoicesTabPage()
		{
			if (InvoicesTabPage.Controls.Count == 0 && InvoicesTabPage.TabVisible)
			{
				fCustomsSupplierHeader = GetSupplierHeaderUserControl();

				var declarationHandler = fCustomsSupplierHeader as IJobDeclarationGuiHandler;
				if (declarationHandler != null)
				{
					declarationHandler.JobDeclaration = JobDeclaration;
				}
				fCustomsSupplierHeader.Dock = DockStyle.Fill;
				InvoicesTabPage.Controls.Add(fCustomsSupplierHeader);
				if (declarationHandler != null)
				{
					declarationHandler.InitializeGridLayout();
					BorderWiseAsyncBatchTariffProcessor();
				}
				fCustomsSupplierHeader.SetDataBinding(JobDeclaration, "");

				JobDeclaration.Invoices.CollectionCountChange -= Invoices_CollectionCountChange;
				JobDeclaration.Invoices.CollectionCountChange += Invoices_CollectionCountChange;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Invoice Action")]
		void Invoices_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is not BaseJobComInvoiceHeader invoice)
			{
				return;
			}

			var action = invoice.IsDeleted ? BaseCustomsBrokerageConstants.Delete : (invoice.IsAttachedToPersistentDeclaration ? BaseCustomsBrokerageConstants.Attach : BaseCustomsBrokerageConstants.Detach);

			if (invoice.IsSavedByFactory && action == BaseCustomsBrokerageConstants.Attach)
			{
				return;
			}

			if (invoicePksAction == null)
			{
				invoicePksAction = new Dictionary<ZGuid, string>();
			}

			invoicePksAction[invoice.PK] = action;
		}

		protected virtual BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			return new LayoutCustomsSupplierHeaderUserControl();
		}

		#endregion

		#region Invoice Lines Tab Page

		public virtual void LoadInvoiceLinesTabPage()
		{
			if (InvoiceLinesTabPage.TabVisible)
			{
				if (InvoiceLinesTabPage.Controls.Count == 0)
				{
					fInvoiceLines = GetInvoiceLinesUserControl();

					var declarationControl = fInvoiceLines as DeclarationInvoiceLineUserControl;
					if (declarationControl != null)
					{
						declarationControl.JobDeclaration = JobDeclaration;
					}

					BorderWiseAsyncBatchTariffProcessor();

					fInvoiceLines.Dock = DockStyle.Fill;
					InvoiceLinesTabPage.Controls.Add(fInvoiceLines);

					if (declarationControl != null)
					{
						declarationControl.InitializeGridLayout();
					}
					fInvoiceLines.SetDataBinding(JobDeclaration, "");
				}
				else if (InvoiceLinesTabPage.IsBound)
				{
					fInvoiceLines?.fInvoiceLineGridLayoutPersister?.ConfigOrgChanged();
					fInvoiceLines?.fWorkflowCustomFieldsGridLayoutPersister?.UpdateGridLayout();
				}
			}
		}

		protected virtual BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			return new GeneralCountryInvoiceLineUserControl();
		}

		#endregion

		#region Message Tab Page

		public void LoadMessageTabPage()
		{
			if (MessagesTabPage.Controls.Count == 0 && MessagesTabPage.TabVisible)
			{
				fMessageUserControl = GetMessageUserControl();
				fMessageUserControl.JobDeclaration = JobDeclaration;
				fMessageUserControl.Dock = DockStyle.Fill;
				MessagesTabPage.Controls.Add(fMessageUserControl);
				fMessageUserControl.SetDataBinding(JobDeclaration, "");
			}

			if (MainTabControl.SelectedTab == MessagesTabPage) // bind unbound tab pages can cause the tab page to be loaded, although it is not shown
			{
				fMessageUserControl.OnShown();
			}
		}

		protected virtual BaseCustomsEntryUserControl GetMessageUserControl()
		{
			if (JobDeclaration.IsDeclarationIntegrated)
			{
				return new EntriesWithMessagesOnDeclarationUserControl(JobDeclaration);
			}
			else if (JobDeclaration.IsImport)
			{
				return new ImportMessageUserControl(JobDeclaration);
			}
			else
			{
				return new ExportMessageUserControl(JobDeclaration);
			}
		}

		#endregion

		#region Misc Options Tab Page

		IDeclarationFormLayoutProvider DeclarationFormLayoutProvider
		{
			get
			{
				if (declarationFormLayoutProvider == null)
				{
					declarationFormLayoutProvider = GUI.DeclarationFormLayoutProvider.GetLayoutProvider(JobDeclaration);
				}
				return declarationFormLayoutProvider;
			}
		}
		IDeclarationFormLayoutProvider declarationFormLayoutProvider;

		public void LoadMiscOptionsUserControl()
		{
			if (MiscOptionsTabPage.Controls.Count == 0 && MiscOptionsTabPage.TabVisible)
			{
				var newMiscOptionsLayout = DeclarationFormLayoutProvider?.GetMiscOptionsLayout(JobDeclaration);
				if (newMiscOptionsLayout != null)
				{
					fDynamicMiscOptionsUserControl = new DynamicMiscOptionsUserControl();
					fDynamicMiscOptionsUserControl.AutoScroll = true;
					fDynamicMiscOptionsUserControl.JobDeclaration = JobDeclaration;
					fDynamicMiscOptionsUserControl.Dock = DockStyle.Fill;
					fDynamicMiscOptionsUserControl.Visible = false;
					MiscOptionsTabPage.Controls.Add(fDynamicMiscOptionsUserControl);
					fDynamicMiscOptionsUserControl.Visible = true;
					fDynamicMiscOptionsUserControl.SetDataBinding(JobDeclaration, "");
					fDynamicMiscOptionsUserControl.SetMiscOptionsLayout(newMiscOptionsLayout);
				}
				else
				{
					fMiscOptionsUserControl = GetMiscOptionsUserControl();
					fMiscOptionsUserControl.AutoScroll = true;
					fMiscOptionsUserControl.JobDeclaration = JobDeclaration;
					fMiscOptionsUserControl.Dock = DockStyle.Fill;
					fMiscOptionsUserControl.Visible = false;
					MiscOptionsTabPage.Controls.Add(fMiscOptionsUserControl);
					fMiscOptionsUserControl.Visible = true;
					fMiscOptionsUserControl.SetDataBinding(JobDeclaration, "");
				}
			}

			MiscOptionsUserControlShown(JobDeclaration);
		}

		protected virtual BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new BaseMiscOptionsUserControl();
		}

		protected virtual void MiscOptionsUserControlShown(BaseJobDeclaration jobDeclaration)
		{
		}

		#endregion

		public void LoadCustomsEntryUserControl()
		{
			if (fBaseCustomsEntryUserControl == null)
			{
				fBaseCustomsEntryUserControl = GetDeclarationUserControl();
				fBaseCustomsEntryUserControl.AutoScroll = true;
				fBaseCustomsEntryUserControl.Visible = false;
				fBaseCustomsEntryUserControl.JobDeclaration = JobDeclaration;
				fBaseCustomsEntryUserControl.Dock = DockStyle.Fill;
				DeclarationTabPage.Controls.Add(fBaseCustomsEntryUserControl);
				fBaseCustomsEntryUserControl.Visible = true;
				fBaseCustomsEntryUserControl.BindingContext = new ZBindingContext();
				fBaseCustomsEntryUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		protected virtual BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new BaseCustomsDeclarationUserControl();
		}

		#region Entry Instruction Details Tab Page

		public void LoadEntryInstructionDetailsTabPage()
		{
			if (EntryInstructionDetailsTabPage.Controls.Count == 0 && EntryInstructionDetailsTabPage.TabVisible)
			{
				fBaseCustomsEntryInstructionUserControl = GetEntryInstructionUserControl();
				fBaseCustomsEntryInstructionUserControl.JobDeclaration = JobDeclaration;
				fBaseCustomsEntryInstructionUserControl.Dock = DockStyle.Fill;
				EntryInstructionDetailsTabPage.Controls.Add(fBaseCustomsEntryInstructionUserControl);
				fBaseCustomsEntryInstructionUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		protected virtual BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new BaseEntryInstructionDetailsUserControl();
		}

		#endregion

		#endregion

		protected virtual void InitializeLazyCreate()
		{
			this.InvoicesTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.DeclarationTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.PackingTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.InvoiceLinesTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.InvoiceGroupingTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.MiscOptionsTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.ContainerTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.MessagesTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			this.EntryInstructionDetailsTabPage.LazyCreateControls += this.LazyCreateControlsFired;
			PickupTabPage.LazyCreateControls += LazyCreateControlsFired;
			DeliveryTabPage.LazyCreateControls += LazyCreateControlsFired;
		}

		protected virtual void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == ContainerTabPage)
			{
				LoadContainerTabPage();
			}
			else if (sender == PackingTabPage)
			{
				LoadPackingTabPage();
			}
			else if (sender == InvoiceGroupingTabPage)
			{
				LoadInvoiceGroupingTabPage();
			}
			else if (sender == InvoicesTabPage)
			{
				LoadInvoicesTabPage();
			}
			else if (sender == MessagesTabPage)
			{
				LoadMessageTabPage();
			}
			else if (sender == InvoiceLinesTabPage)
			{
				LoadInvoiceLinesTabPage();
			}
			else if (sender == MiscOptionsTabPage)
			{
				LoadMiscOptionsUserControl();
			}
			else if (sender == EntryInstructionDetailsTabPage)
			{
				LoadEntryInstructionDetailsTabPage();
			}
			else if (sender == PickupTabPage)
			{
				LoadPickupTabPage();
			}
			else if (sender == DeliveryTabPage)
			{
				LoadDeliveryTabPage();
			}
		}

		protected virtual void JobDeclaration_JE_MessageTypeChanged(object sender, EventArgs e)
		{
			ShowOrHidePackingTabPage();
			ShowOrHidePickupTabPage();
			ShowOrHideDeliveryTabPage();
			RemoveUserControlOfEachTabPage();
			ChangeContainerTabVisibility();
		}

		protected virtual void ShowOrHidePickupTabPage()
		{
			PickupTabPage.TabRelevant = JobDeclaration != null && JobDeclaration.IsStandAlone && JobDeclaration.IsExportOrNonTransport;
		}

		protected virtual void ShowOrHideDeliveryTabPage()
		{
			DeliveryTabPage.TabRelevant = JobDeclaration == null || (JobDeclaration.IsStandAlone && !JobDeclaration.IsExportOrNonTransport);
		}

		protected virtual void ShowOrHidePackingTabPage()
		{
			PackingTabPage.TabRelevant = JobDeclaration != null && JobDeclaration.IsPackingInformationRelevant;
		}

		protected virtual void JobDeclaration_MergedSuccessfully()
		{
			if (MainTabControl.SelectedTab == MessagesTabPage && fMessageUserControl != null)
			{
				fMessageUserControl.OnShown();
			}
		}

		void JE_ContainerModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeContainerTabVisibility();
		}

		void ChangeContainerTabVisibility()
		{
			ShowOrHideContainerTabPage();
		}

		void BorderWiseAsyncBatchTariffProcessor()
		{
			if (!string.IsNullOrEmpty(JobDeclaration?.JobNumber) && JobDeclaration?.InvoiceLines.Count > 0)
			{
				BorderWiseAsyncBatchTariffProcessorProvider.ClassifyDeclarationJobInBorderWise(this.ParentForm, JobDeclaration);
			}
		}

		protected void ShowOrHideContainerTabPage()
		{
			ContainerTabPage.TabRelevant = JobDeclaration.ContainersRequired;
		}

		protected virtual void RemoveUserControlOfEachTabPage()
		{
			RemoveControl(InvoicesTabPage);
			RemoveControl(InvoiceLinesTabPage);
			RemoveControl(MessagesTabPage);
			RemoveControl(MiscOptionsTabPage);
		}

		protected void RemoveControl(Control controlToRemove)
		{
			if (controlToRemove != null)
			{
				foreach (Control currentControl in controlToRemove.Controls)
				{
					currentControl.Dispose();
				}
				controlToRemove.Controls.Clear();
				(controlToRemove as ZBindingTabPage)?.ResetBinding();
			}
		}

		protected DialogResult GetConfirmationForSendingWithdrawal()
		{
			string caption = Res.GetString("7f8da63c-2018-4b9a-8685-58990e730f89", "Send Withdrawal");
			string message = Res.GetString("64c1431b-4ccd-4bd3-927f-4a26f8d0cd31", "This declaration has already been sent to Customs. Withdrawal message(s) will result from deleting this declaration. Do you wish to continue?");

			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
		}

		protected DialogResult GetConfirmation(string caption, string message)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
		}

		protected virtual SendsMessagesToCustomsGUI NewSendsMessagesToCustomsGUI()
		{
			return new SendsMessagesToCustomsGUI();
		}

		#endregion

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				BaseJobDeclaration declaration = this.JobDeclaration;
				return declaration != null ? (string)declaration.CountryCode : EnvProxy.Instance.CurrentCompany.Country.Code;
			}
		}

		#endregion
	}
}
