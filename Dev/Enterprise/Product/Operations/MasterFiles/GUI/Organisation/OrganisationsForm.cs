using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZOrganisationsForm : BaseOrganisationsForm, IWorkflowTaskNavigationOverridable
	{
		protected ZOrganisationsForm()
		{
		}

		internal MenuItem viewAllTabsMenuItem;
		internal MenuItem viewEnabledTabsOnlyMenuItem;
		internal MenuItem recalculateCodeMenuItem;
		readonly UpdateRelatedJobsInitializer updateRelatedJobsInitializer;
		readonly EDICommunicationsMode ediCommunicationsMode;

		public ZOrganisationsForm(EDICommunicationsMode ediCommunicationsMode)
			: this(ediCommunicationsMode.Factory.Load<OrgHeader>(ediCommunicationsMode.EK_ParentID))
		{
			this.ediCommunicationsMode = ediCommunicationsMode;
		}

		protected override void OnShown(EventArgs e)
		{
			SelectSingleEDICommunicationsModeInGrid(ediCommunicationsMode);
			base.OnShown(e);
		}

		public ZOrganisationsForm(OrgHeader organisation)
			: base(organisation)
		{
			SetupForm();
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItem);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("OrganisationsForm|ActionsMenu|ImportEDICodeMapping", "Import EDI Code Mapping from CSV File"), OnImportEDICodeMapping);
			recalculateCodeMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("OrganisationsForm|ActionsMenu|RecalculateCode", "&Recalculate Code"), RecalculateCodeClick);
			ChangeRecalculateCodeMenuItemVisibility();
			AddEnrollForElectronicBillOfLadingMenuItem();
			updateRelatedJobsInitializer = new UpdateRelatedJobsInitializer(organisation);
			updateRelatedJobsInitializer.CreateUpdateRelatedJobsMenuItem(ActionsMenuItem, (o, e) => { UpdateRelatedJobScreeningStatus(); });
			ZFormMenuStrategy.AddActionsMenuItem(this, "-", null);

			if (!DesignModeFinder.IsDesigning)
			{
				AddTabMenuItems();
			}

			// There may be a bad data in service levels which only a client can fix. In this way, we force the client to fix it,
			// i.e. the client won't be able to save an org form without fixing service level mapping issue if exists (quite rare).
			//
			// If you are in 2024, you can safely remove the code and the related test below. ALl clients should upgrade and fix
			// the data by 2024.
			organisation.MiscServ.CarrierServiceLevels?.MarkAsNeedingValidation();

			organisation.ShowMessage += Organisation_ShowMessage;
			organisation.OrgSaved += Organisation_OrgSaved;
			organisation.PatternMatchingRecalculator.RecalculateStart += Organisation_RegeneratStart;
			organisation.PatternMatchingRecalculator.Recalculating += Organisation_Regenerating;
			organisation.PatternMatchingRecalculator.Recalculated += Organisation_Regenerated;
			((IDeduplicatable)organisation).ShouldRunDeduplication = true;

			InitializeActivationMenuItems();

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("a7f90ecf-d1f8-44ea-9a43-a2580ef42daa", "Format All Contact Phone Numbers"), OnFormatAllContactPhoneNumbers);
			AddExportPatternOverrideMenuItem();
		}

		void ChangeRecalculateCodeMenuItemVisibility() => recalculateCodeMenuItem.Visible = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value.AllowRecalculatedOrgCodeByUser;

		void AddTabMenuItems()
		{
			viewAllTabsMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("OrganisationsForm|ActionsMenu|ViewAllTabls", "View &All Tabs"), ViewAllTabsClick);
			viewAllTabsMenuItem.Shortcut = Shortcut.CtrlShiftA;
			viewAllTabsMenuItem.ShowShortcut = true;
			viewEnabledTabsOnlyMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("OrganisationsForm|ActionsMenu|ViewEnabledTabsOnly", "View &Enabled Tabs Only"), ViewEnabledTabsOnlyClick);
			viewEnabledTabsOnlyMenuItem.Shortcut = Shortcut.CtrlShiftT;
			viewEnabledTabsOnlyMenuItem.ShowShortcut = true;
		}

		protected virtual DuplicateAlertControlHelper GetDeduplicationHelper()
		{
			return new DuplicateAlertControlHelper();
		}

#if DEBUG
		internal bool isrecalculateprogressFormShownForTest;
#endif
#if DEBUG
		internal
#endif
		ProgressForm recalculateprogressForm;

		void Organisation_RegeneratStart(object sender, EventArgs e)
		{
			recalculateprogressForm = new ProgressForm();
			recalculateprogressForm.Show(this);
			recalculateprogressForm.ShowCancelButton = false;
#if DEBUG
			if (Globals.IsTest)
			{
				isrecalculateprogressFormShownForTest = true;
			}
#endif
		}

		void Organisation_Regenerating(object sender, RecalculatingEventArgs e)
		{
			if (recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
			{
				recalculateprogressForm.SetStatusAndPercentComplete(e.ProgressText, e.Progress);
			}
		}

		void Organisation_Regenerated(object sender, RecalculatedEventArgs e)
		{
			if (recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
			{
				recalculateprogressForm.Close();
				recalculateprogressForm = null;
			}

			if (string.IsNullOrEmpty(e.ErrorMessage))
			{
				var effectStr = e.EffectiveCount <= 1 ? e.EffectiveCount + Res.GetString("CBBD41CE-BE07-44C4-9C71-FDFD0AE73B46", "{0}", " record affected.") : e.EffectiveCount + Res.GetString("605E3D3A-42C6-4CB7-B393-9B8759A57E49", "{0}", " records affected.");
				Organisation_ShowMessage(Res.GetString("6CBEC24B-A47E-4FC1-A3AA-39EB41045A2C", "Regeneration has completed successfully!"), Res.GetString("E3513AA3-A335-4ACB-809E-F04762CC379B", "Result: ") + effectStr);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("F915153D-69BD-49B6-9C80-B5E496294133", "Error Message: {0}", e.ErrorMessage), Res.GetString("AFC1B299-04D6-4016-A7D1-FD62AD5B1DFE", "Pattern tables regeneration failed"));
			}
		}

		protected override void ShowDeleteErrorInactiveMessage()
		{
			if (BusinessEntityForValidation is ICancellable cancellable && !cancellable.IsCancelled)
			{
				cancellable.IsCancelled = true;
				BusinessEntityForValidation.Factory.Save();
			}

			base.ShowDeleteErrorInactiveMessage();
		}

		protected override ZForm ReloadAndDeactivateInsteadOfDelete()
		{
			var newForm = base.ReloadAndDeactivateInsteadOfDelete();
			if (newForm is ZOrganisationsForm orgForm)
			{
				orgForm.DeActivateOrganisation();
			}
			return newForm;
		}

		void SelectSingleEDICommunicationsModeInGrid(EDICommunicationsMode ediCommunicationsMode)
		{
			if (ediCommunicationsMode == null)
			{
				return;
			}
			var control = Controls.Find("EdiCommsGrid", true);
			if (control?.Length > 0)
			{
				var grid = (ZGrid)control[0];
				grid.SelectSingleElement(ediCommunicationsMode);
			}
		}

		#if DEBUG
		public
		#endif
		BusinessObject[] GetSelectedEDICommunicationsModeFromGrid()
		{
			var control = Controls.Find("EdiCommsGrid", true);
			if (control?.Length > 0)
			{
				var grid = (ZGrid)control[0];
				return grid.SelectedElements;
			}
			return null;
		}

		#region Export To Xml

		List<MenuItem> ExportToXmlMenuItem
		{
			get
			{
				return new ExportToXmlMenuItemSet<OrgHeader>(() => Exporter, Organisation);
			}
		}

		public IXmlDataTransferExporter Exporter
		{
			get
			{
				if (exporter == null)
				{
					exporter = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
						?
						ObjectFactory.Get<IXmlDataTransferExporter>("CustomsOrganisationXmlDataTransferExporter")
						:
						ObjectFactory.Get<IXmlDataTransferExporter>("OrganisationXmlDataTransferExporter");
				}
				return exporter;
			}

			internal set
			{
				exporter = value;
			}
		}
		IXmlDataTransferExporter exporter;

		#endregion

		#region GUI Setup

		const string ReceivablesTabPageName = "ReceivablesTabPage";
		const string PayablesTabPageName = "PayablesTabPage";
		const string ConsignorTabPageName = "ConsignorTabPage";
		const string ConsigneeTabPageName = "ConsigneeTabPage";
		const string WhsFacilityTabPageName = "WhsFacilityTabPage";
		const string TransportTabPageName = "TransportTabPage";
		const string ForwarderTabPageName = "ForwarderTabPage";
		const string SalesTabPageName = "SalesTabPage";
		const string CompetitorTabPageName = "CompetitorTabPage";
		const string MiscServicesTabPageName = "MiscServicesTabPage";
		const string UserDefinedTabPageName = "UserDefinedTabPage";
		const string WorkflowTabPageName = "WorkflowTabPage";

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				ConsignorTabPage.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
				ConsignorTabPage.RunWhenBindingOrFirstShown(delegate
				{
					ConsignorNotSelectedLabel.Text = Res.GetString("ZOrganisationsForm|ConsignorNotSelectedLabel", "You must select an Organization type of {0} from the Details page to use this page.",
						FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
				});
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (OrgContactSupersedeHelper.RequireARContact(Organisation, null))
			{
				return ContinueWithSave.No;
			}

			if (!EPaymentHelper.AllowEmptyDefaultPaymentReason(Organisation))
			{
				return ContinueWithSave.No;
			}

			var isNewOrg = !Organisation.IsInDatabase;

			Organisation.PreValidateMainAddressForRegistry();
			ContinueWithSave baseContinueWithSave = base.ValidateAndSave();
			if (baseContinueWithSave == ContinueWithSave.Yes && Organisation.ShouldUpdateRelatedJobs)
			{
				PromptProgressBarAndUpdateRelatedJobs(this, Organisation);
				Organisation.ShouldUpdateRelatedJobs = false;
			}

			if (Organisation.IsInDatabase && isNewOrg)
			{
				Organisation.Factory.Save();
			}

			return baseContinueWithSave;
		}

		void PromptProgressBarAndUpdateRelatedJobs(Form parentForm, OrgHeader orgHeader)
		{
			var progressForm = new ProgressForm();

			try
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowModalTo(parentForm);

				ScreeningStatusUpdater.ProgressUpdaterDelegate progressUpdater = (string partyCode) =>
				{
					string msg = Res.GetString("b83f7d99-3364-40ce-bcdd-09d990696e24", "Updating screening status of jobs related to party: {0}", partyCode);
					progressForm.SetStatusAndPercentComplete(msg, 50);
				};

				ScreeningStatusUpdater.UpdateRelatedJobsForChangedParty(orgHeader, progressUpdater);
				progressForm.SetStatusAndPercentComplete(Res.GetString("403d203e-6506-4894-9ece-294e8b05072e", "Processed all jobs related to this party."), 100);
			}
			finally
			{
				progressForm.Close();
			}
		}

		public override string FormCaption
		{
			get { return Res.GetString("ZOrganisationsForm|FormCaption", "Organization") + base.FormCaption; }
		}

		void SetupForm()
		{
			if (!this.IsDesignMode())
			{
				AddAllTabPagesAndSetTheirOrder();
				WorkflowTabPage.Initialize(Organisation);

				if (!IsProductivityWiseModeEnabled)
				{
					PlugIns.Add(ControllerIDs.Customs.US.OrganisationCustomsMessaging);
					PlugIns.Add(ControllerIDs.Customs.AU.OrganisationCustomsMessaging);
					PlugIns.Add(ControllerIDs.Customs.CA.OrganisationCustomsMessaging);
					PlugIns.Add(ControllerIDs.Customs.GB.OrganisationCustomsMessaging);
				}

				SetupEventHandlers();
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);

				if (Env.Security.OrganisationModify.IsAllowed)
				{
					PlugIns.Add(ControllerIDs.Audit);
				}

				if (ControllerID == null)
				{
					ControllerID = ControllerIDs.Organisation;
				}

				if (Organisation != null && !Organisation.SecurityProvider.HasModifyDetailsSecurity)
				{
					WorkflowTabPage.SetReadOnlyIncludingChildren();
				}

				if (BusinessEntity is ICancellable cancellable && cancellable.IsCancelled)
				{
					BusinessEntity.Factory.SuspendValidation();
				}

				if (fApplyButton != null)
				{
					fApplyButton.Click += ApplyButtonClick;
				}

				if (fPostButton != null)
				{
					fPostButton.Click += PostButtonClick;
				}
			}
		}

		protected void ClearTabPageOrder()
		{
			TabPagesWithOrders.ForEach(d => OrganisationsTabControl.TabPages.Remove(d.Key));
		}

		protected void SetPageTabOrder()
		{
			TabPagesWithOrders.ForEach(d =>
			{
				if (d.Value >= 0)
				{
					OrganisationsTabControl.TabPages.Insert(d.Key, d.Value);
				}
				else
				{
					OrganisationsTabControl.TabPages.Add(d.Key);
				}
			});
		}

		protected void ConsistBindingContext()
		{
			TabPagesWithOrders.ForEach(d =>
			{
				if (d.Key.BindingContext == OrganisationsTabControl.BindingContext)
				{
					d.Key.BindingContext = OrganisationsTabControl.BindingContext;
				}
			});
		}

		void AddAllTabPagesAndSetTheirOrder()
		{
			ClearTabPageOrder();
			SetPageTabOrder();
			ConsistBindingContext();
		}

		/// <summary>
		/// Dynamic tabpages with orders, if you don't care about the order, please set order to -1
		/// </summary>
		protected virtual Dictionary<ZTabPage, int> TabPagesWithOrders => new Dictionary<ZTabPage, int>
		{
			{ AddressesTabPage, 1 },
			{ ContactsTabPage, 2 },
			{ ReceivablesTabPage, 3 },
			{ PayablesTabPage, 4 },
			{ ConsignorTabPage, 5 },
			{ ConsigneeTabPage, 6 },
			{ WhsFacilityTabPage, 7 },
			{ ForwarderTabPage, 8 },
			{ TransportTabPage, 9 },
			{ MiscServicesTabPage, 10 },
			{ SalesTabPage, 11 },
			{ CompetitorTabPage, 12 },
			{ WorkflowTabPage, 13 },
			{ UserDefinedTabPage, 14 }
		};

		void SetTabsReadonlynessBasedOnIsGlobalSupplierSecurity(bool removeReadonlyBinding)
		{
			if (Organisation != null && !Organisation.SecurityProvider.CanEditGlobalSupplier)
			{
				foreach (ZTabPage tabPage in OrganisationsTabControl.TabPages)
				{
					if (tabPage.Name != ReceivablesTabPageName
						&& tabPage.Name != PayablesTabPageName)
					{
						tabPage.SetReadOnlyIncludingChildren(removeReadonlyBinding, new List<string> { DetailsControl.OH_IsCreditorBoundCheckEdit.Name, DetailsControl.OH_IsDebtorBoundCheckEdit.Name });
					}
				}
			}
		}

		void SetupEventHandlers()
		{
			var organisationDocumentSupporter = (OrgHeaderDocumentSupporter)Organisation.DocumentSupporter;
			organisationDocumentSupporter.OnRoutingOrderRecommendationPrinted += new OrgHeaderDocumentSupporter.RoutingOrderRecommendationPrintedEventHandler(Organisation_OnRoutingOrderRecommendationPrinted);
			organisationDocumentSupporter.OnRoutingOrderPrintingNoAgents += new OrgHeaderDocumentSupporter.RoutingOrderPrintingNoAgentsEventHandler(Organisation_OnRoutingOrderPrintingNoAgents);
			organisationDocumentSupporter.OnRoutingOrderReplacementPrinted += new OrgHeaderDocumentSupporter.RoutingOrderReplacementPrintedEventHandler(Organisation_OnRoutingOrderReplacementPrinted);

			Organisation.OH_IsDebtorInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsCreditorInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsConsignorInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsConsigneeInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsWarehouseClientInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsShippingProviderInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsForwarderInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsSalesLeadInfo.ValueChanged += new EventHandler(OH_IsSalesLeadInfo_ValueChanged);
			Organisation.OH_IsCompetitorInfo.ValueChanged += new EventHandler(OH_IsCompetitorInfo_ValueChanged);
			Organisation.OH_IsMiscFreightServicesInfo.ValueChanged += new EventHandler(OrganisationTypeChangedForHidingTabs);

			Organisation.OH_RL_NKClosestPortInfo.ValueChanged += OH_RL_NKClosestPortInfo_ValueChanged;

			ButtonsUserControl.SaveButton.Click += CheckForAddressChange;
			ButtonsUserControl.SaveAndCloseButton.Click += CheckForAddressChange;
		}

		void UninstallEventHandlers()
		{
			var organisationDocumentSupporter = (OrgHeaderDocumentSupporter)Organisation.DocumentSupporter;
			organisationDocumentSupporter.OnRoutingOrderRecommendationPrinted -= new OrgHeaderDocumentSupporter.RoutingOrderRecommendationPrintedEventHandler(Organisation_OnRoutingOrderRecommendationPrinted);
			organisationDocumentSupporter.OnRoutingOrderPrintingNoAgents -= new OrgHeaderDocumentSupporter.RoutingOrderPrintingNoAgentsEventHandler(Organisation_OnRoutingOrderPrintingNoAgents);
			organisationDocumentSupporter.OnRoutingOrderReplacementPrinted -= new OrgHeaderDocumentSupporter.RoutingOrderReplacementPrintedEventHandler(Organisation_OnRoutingOrderReplacementPrinted);

			Organisation.OH_IsDebtorInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsCreditorInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsConsignorInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsConsigneeInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsWarehouseClientInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsShippingProviderInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsForwarderInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);
			Organisation.OH_IsSalesLeadInfo.ValueChanged -= new EventHandler(OH_IsSalesLeadInfo_ValueChanged);
			Organisation.OH_IsCompetitorInfo.ValueChanged -= new EventHandler(OH_IsCompetitorInfo_ValueChanged);
			Organisation.OH_IsMiscFreightServicesInfo.ValueChanged -= new EventHandler(OrganisationTypeChangedForHidingTabs);

			Organisation.OH_RL_NKClosestPortInfo.ValueChanged -= OH_RL_NKClosestPortInfo_ValueChanged;

			ButtonsUserControl.SaveButton.Click -= CheckForAddressChange;
			ButtonsUserControl.SaveAndCloseButton.Click -= CheckForAddressChange;
		}

		void OH_IsSalesLeadInfo_ValueChanged(object sender, EventArgs e)
		{
			if (SalesControl != null)
			{
				if (Organisation.OH_IsSalesLead)
				{
					SalesControl.SetupTabPageSecurity();
				}
				else
				{
					SalesControl.DetachLicenceCheckpoints();
				}
			}

			OrganisationTypeChangedForHidingTabs(sender, e);
		}

		void OH_IsCompetitorInfo_ValueChanged(object sender, EventArgs e)
		{
			if (CompetitorControl != null)
			{
				if (Organisation.OH_IsCompetitor)
				{
					CompetitorControl.SetupTabPageSecurity();
				}
				else
				{
					CompetitorControl.DetachLicenceCheckpoints();
				}
			}

			OrganisationTypeChangedForHidingTabs(sender, e);
		}

		void OH_RL_NKClosestPortInfo_ValueChanged(object sender, EventArgs e)
		{
			ZString taxType = ZString.Empty;
			ZTabPage selectedTabPage = null;

			if (Organisation.OH_IsDebtor && Organisation.OH_IsCreditor)
			{
				taxType = (NoResString)"AR and AP";
			}
			else if (Organisation.OH_IsDebtor)
			{
				taxType = "AR";
				selectedTabPage = ReceivablesTabPage;
			}
			else if (Organisation.OH_IsCreditor)
			{
				taxType = "AP";
				selectedTabPage = PayablesTabPage;
			}
			else
			{
				return;
			}

			if (e is ValueChangedEventArgs eventArgs)
			{
				var previousCountry = ((ZString)eventArgs.OldValue).SubstringSafe(0, 2);
				var newCountry = ((ZString)eventArgs.NewValue).SubstringSafe(0, 2);

				if (previousCountry != newCountry && GlbCompany.CurrentCompany.GC_IsGSTRegistered)
				{
					Organisation_ShowMessage(Res.GetString("55B5C3A3-2F77-4540-B60E-12BED6783A0F", "Changing UNLOCO"),
						Res.GetString("5D4056BE-E482-4769-9647-994889AA7AF9",
							@"You have changed the UNLOCO of this organization. This may have an effect on the tax treatment of this organization. It is strongly recommended that you review the {0} tax details of this organization.

Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.",
							taxType));

					if (selectedTabPage != null)
					{
						OrganisationsTabControl.SelectedTab = selectedTabPage;
					}
				}
			}
		}

		void RecalculateCodeClick(object sender, EventArgs e)
		{
			if (Env.Security.OrganisationCanRecalculateCodesOnDemand.IsAllowed)
			{
				if (Organisation.CanGenerateCode)
				{
					Organisation.GenerateProposedCode(false);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("0f03a7e2-f365-4668-8b45-190c80062809", "Please enter the organization details before recalculating the code"), Res.GetString("03fab424-f290-4f9d-85bb-48ca6ccbc21e", "Cannot Recalculate Code"));
				}
			}
			else
			{
				Globals.Message.ShowError(Env.Security.OrganisationCanRecalculateCodesOnDemand.ErrorMessageForNotAllowed);
			}
		}

		public void UpdateRelatedJobScreeningStatus()
		{
			if (DpsSecurityRights.IsGrantedUpdateRelatedJobWithShowError())
			{
				PromptProgressBarAndUpdateRelatedJobs(this, Organisation);
				Organisation.ScreeningLogCollection.AddUpdateRelatedJobScreeningStatusLog();
				ValidateAndSave();
			}
		}

#if DEBUG
		protected virtual
#endif
		void OnImportEDICodeMapping(object sender, EventArgs e)
		{
			if (!Organisation.IsDeleted)
			{
				new ImportEDICodeMappingFromCSVForm(Organisation).Show();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("a5b976fc-3fab-459e-9521-d197faf683df", "Please enter activate the organization or choose an active organization before importing"), Res.GetString("{4110FE4E-665E-4f2e-8CC6-CF1C8E652067}", "Organization is not Active"));
			}
		}

		#region Event Handlers

		void Organisation_OnRoutingOrderRecommendationPrinted(object sender, OrgHeaderDocumentSupporter.RoutingOrderRecommenationPrintingEventArgs e)
		{
			if (!e.Cancel)
			{
				BusinessObjectCollection collection = e.OrgType == OrgHeaderDocumentSupporter.SuppliersBuyersType.Buyers ? Organisation.BuyerLinks : Organisation.SupplierLinks;
				using (OrgSupplierBuyerLinksForm supplierBuyerForm = new OrgSupplierBuyerLinksForm(new OrgSupplierBuyerLinkCollectionReadOnlyView(collection), e.OrgType))
				{
					DialogResult result = supplierBuyerForm.ShowDialog(this);
					e.Cancel = result == DialogResult.Cancel;
				}
			}
		}

		void Organisation_OnRoutingOrderPrintingNoAgents(object sender, OrgHeaderDocumentSupporter.SuppliersBuyersWithNoAgentsEventArgs e)
		{
			ZString message = Res.GetString("2e305f39-c44a-4130-acca-802b76b209e7", @"The following organizations have no published agents in their port for the specified transport mode.
This will result in no Routing Order / Recommendation being printed.") + "\r\n\r\n";
			foreach (OrgHeader org in e.Parties)
			{
				message += "  - " + org.OH_FullNameTruncated + " " + Res.GetString("56ed2f6e-78cb-40a3-8811-89a635518775", "in port") + " " + (org.UNLOCO != null ? org.UNLOCO.Code.ToString() : Res.GetString("44df2f6e-7819-40c3-9511-89a605517972", "<no port>")) + System.Environment.NewLine;
			}

			message += "\r\n" + Res.GetString("1f5bcf8c-68a5-455d-9b7f-5db26b752315", "You should create a published agent for each of the ports mentioned above.\r\nThis will allow Routing Orders and Recommendations to be printed successfully.");

			Globals.Message.Show(message, Res.GetString("9618021c-e584-4c11-b949-82e829488ae5", "No Published Agents Found"), MessageBoxButtons.OK, DialogResult.OK);
		}

		void Organisation_OnRoutingOrderReplacementPrinted(object sender, OrgHeaderDocumentSupporter.RoutingOrderReplacementPrintedEventArgs e)
		{
			AgentSelectionBusinessObject agentSelectionBizo = new AgentSelectionBusinessObject(BusinessEntity.Factory);
			ZFormModaliser.ShowDialogAndDispose(new RoutingOrderReplacementAgentSelectionForm(agentSelectionBizo));
			e.ReplacementAgent = agentSelectionBizo.SelectedAgent;
		}

		#endregion

		#region Change Tab Pages

		protected override void ChangeTabPage(object sender, EventArgs e)
		{
			base.ChangeTabPage(sender, e);
			bool showControls = true;
			bool setControls = true;
			ZLabel selectedLabel = null;

			TabPage selectedTab = ((TabControl)sender).SelectedTab;

			if (selectedTab != null && !TabControlsAlreadySet)
			{
				switch (selectedTab.Name)
				{
					case ReceivablesTabPageName:
						showControls = Organisation.OH_IsDebtor;
						selectedLabel = ReceivablesNotSelectedLabel;
						break;

					case PayablesTabPageName:
						showControls = Organisation.OH_IsCreditor;
						selectedLabel = PayablesNotSelectedLabel;
						break;

					case ConsignorTabPageName:
						showControls = Organisation.OH_IsConsignor;
						selectedLabel = ConsignorNotSelectedLabel;
						break;

					case ConsigneeTabPageName:
						showControls = Organisation.OH_IsConsignee;
						selectedLabel = ConsigneeNotSelectedLabel;
						break;

					case WhsFacilityTabPageName:
						showControls = Organisation.OH_IsWarehouseClient;
						selectedLabel = WarehouseNotSelectedLabel;
						break;

					case TransportTabPageName:
						showControls = Organisation.OH_IsShippingProvider;
						selectedLabel = TransportNotSelectedLabel;
						break;

					case ForwarderTabPageName:
						showControls = Organisation.OH_IsForwarder;
						selectedLabel = ForwarderNotSelectedLabel;
						break;

					case SalesTabPageName:
						showControls = Organisation.OH_IsSalesLead;
						selectedLabel = SalesNotSelectedLabel;
						break;

					case CompetitorTabPageName:
						showControls = Organisation.OH_IsCompetitor;
						selectedLabel = CompetitorNotSelectedLabel;
						break;

					case MiscServicesTabPageName:
						showControls = Organisation.OH_IsMiscFreightServices;
						selectedLabel = MiscServicesNotSelectedLabel;
						break;

					case UserDefinedTabPageName:
						showControls = true;
						break;

					case WorkflowTabPageName:
						showControls = true;
						break;

					default:
						setControls = false;
						break;
				}

				if (setControls)
				{
					SetTabPageControls(selectedTab, selectedLabel, showControls);
				}
			}
		}

		#endregion

		#endregion

		#region Organisation_ShowMessage

		void Organisation_ShowMessage(string caption, string message)
		{
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		#region Saving

		protected bool IsSaveAndCloseButtonClicked { get; set; }

		void ApplyButtonClick(object sender, EventArgs e)
		{
			IsSaveAndCloseButtonClicked = false;
		}

		void PostButtonClick(object sender, EventArgs e)
		{
			IsSaveAndCloseButtonClicked = true;
		}

#if DEBUG
		internal bool isExternalValidationProgressFormShownForTest;
#endif
#if DEBUG
		internal
#endif
		ExternalValidationProgressForm progressForm;

		void Organisation_OrgSaved(object sender, EventArgs e)
		{
			Organisation.RefreshBindingIncludingChildren();
			if (DataRegistry.Instance.EnableExternalValidationService)
			{
				if (progressForm == null || progressForm.IsDisposed)
				{
#if DEBUG
					if (Globals.IsTest)
					{
						isExternalValidationProgressFormShownForTest = true;
						progressForm = new ExternalValidationProgressForm(Organisation, new ExternalValidationProgressForm.MockValidExternalValidationClient(), CloseProgressFormAndOrgFormIfNeeded, false);
					}
					else
#endif
					{
						progressForm = new ExternalValidationProgressForm(Organisation, CloseProgressFormAndOrgFormIfNeeded);
					}

					progressForm.FormClosed += ProgressForm_OnFormClosed;
					ZFormModaliser.Show(progressForm, this);
				}
			}
		}

		protected void CloseProgressFormAndOrgFormIfNeeded()
		{
			if (progressForm != null && !progressForm.IsDisposed)
			{
				progressForm.Close();
			}

			if (IsSaveAndCloseButtonClicked && !IsDisposed)
			{
				Close();
			}
		}

		void CheckForAddressChange(object sender, EventArgs e)
		{
			if (Organisation.TSAKnownAddressChanged)
			{
				Organisation.TSAKnownAddressChanged = false;
				Globals.Message.ShowWarning(Res.GetString("C4601821-9E2E-4934-86D2-479A6AEC4029", "A US TSA Known Shipper address has been modified. This may cause TSA record inconsistency. All previous approved TSA Known Shipper linked to this address are changed to not approved. Refer to the TSA Known Shipper tab which shows the details that have been last reviewed with TSA."));
			}
			if (Organisation.MIDAddressChanged)
			{
				Organisation.MIDAddressChanged = false;
				Globals.Message.ShowWarning(Res.GetString("79A87123-2F57-49FC-A4EC-E38FFC1F4F14", "An address related to a Manufacturer ID Number (MID) has been modified. The change to the address may affect the MID number."));
			}
		}

		void ProgressForm_OnFormClosed(object sender, FormClosedEventArgs formClosedEventArgs)
		{
			progressForm.FormClosed -= ProgressForm_OnFormClosed;

			var referenceForPassedOrFailed = string.Empty;
			bool hasErrors = false;
			bool hasWarning = false;

			if (progressForm.Result != null)
			{
				hasErrors = progressForm.Result.HasErrors;
				hasWarning = progressForm.Result.HasWarnings;

				referenceForPassedOrFailed = string.Format(CultureInfo.InvariantCulture, (NoResString)"Errors: {0}, Warnings: {1}.",
					hasErrors ? progressForm.Result.Errors.Length : 0,
					hasWarning ? progressForm.Result.Warnings.Length : 0);
			}

			switch (progressForm.ProgressResult)
			{
				case ExternalValidationProgressResult.Valid:
					AddExternalValidationEvent(AutoEvents.ExternalValidationPassed, referenceForPassedOrFailed);
					if (hasErrors || hasWarning)
					{
						resultForm = new ExternalValidationResultForm(Organisation, progressForm.Result);
						resultForm.Show();
					}
					break;
				case ExternalValidationProgressResult.Invalid:
					AddExternalValidationEvent(AutoEvents.ExternalValidationFailed, referenceForPassedOrFailed);
					if (hasErrors || hasWarning)
					{
						resultForm = new ExternalValidationResultForm(Organisation, progressForm.Result);
						resultForm.Show();
					}
					break;
				case ExternalValidationProgressResult.Cancelled:
					AddExternalValidationEvent(AutoEvents.ExternalValidationNotCompleted, (NoResString)"User Canceled");
					break;
				case ExternalValidationProgressResult.Timeout:
					AddExternalValidationEvent(AutoEvents.ExternalValidationNotCompleted, (NoResString)"Timeout");
					Globals.Message.ShowError(Res.GetString("ExternalValidationProgressForm|0b559690-9a7e-41cd-a5f0-f832c0587a2f", "The external validation process timed out."));
					break;
				case ExternalValidationProgressResult.OtherErrors:
					AddExternalValidationEvent(AutoEvents.ExternalValidationNotCompleted, progressForm.ExceptionMessage);
					Globals.Message.ShowError(Res.GetString("ExternalValidationProgressForm|B1954387-B704-4734-BC9D-B5A9B0FFD38D", "An error occurred during the external validation:\r\n{0}\r\n\r\nThe web service URL used is: {1}", progressForm.ExceptionMessage, DataRegistry.Instance.ExternalValidationServiceUrl));
					break;
			}
		}

		void AddExternalValidationEvent(Event @event, string reference = "")
		{
			BusinessObjectFactory eventFactory = new BusinessObjectFactory();
			((OrgHeader)eventFactory.ImportFromAnotherFactory(Organisation)).Logs.AddNew(@event, reference);
			eventFactory.Save();
		}

		ExternalValidationResultForm resultForm;

		#endregion

		#region IWorkflowForm Members

		void IWorkflowTaskNavigationOverridable.NavigateToWorkflowItem(ProcessTask task)
		{
			if (Organisation != null && task != null && task.P9_ParentID == Organisation.PK)
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
				WorkflowTabPage.NavigateToWorkflowItem(task);
			}
			else if (task != null)
			{
				TopLevelTabControl.SelectedTab = SalesTabPage;
				SalesControl.NavigateToWorkflowItem(task);
			}
			else
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
			}
		}

		void IWorkflowTaskNavigationOverridable.NavigateToWorkflowItem(IProcessHeader workflow)
		{
			if (Organisation != null && workflow.FH_ParentId == Organisation.PK)
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
				WorkflowTabPage.NavigateToWorkflowItem(workflow);
			}
			else
			{
				var task = workflow.Tasks.OfType<ProcessTask>().FirstOrDefault();

				if (task != null)
				{
					((IWorkflowTaskNavigationOverridable)this).NavigateToWorkflowItem(task);
				}
			}
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			try
			{
				if (isNotFinalizing)
				{
					OrganisationsTabControl.SelectedIndexChanged -= new EventHandler(ChangeTabPage);
					if (DetailsControl != null)
					{
						DetailsControl.DetailsTabControl.SelectedIndexChanged -= new EventHandler(ChangeTabPage);
					}
					Organisation.ShowMessage -= new OrgHeader.ShowMessageEventHandler(Organisation_ShowMessage);
					Organisation.OrgSaved -= new EventHandler(Organisation_OrgSaved);

					Organisation.PatternMatchingRecalculator.RecalculateStart -= Organisation_RegeneratStart;
					Organisation.PatternMatchingRecalculator.Recalculating -= Organisation_Regenerating;
					Organisation.PatternMatchingRecalculator.Recalculated -= Organisation_Regenerated;

					UninstallEventHandlers();

					if (progressForm != null && progressForm.Visible)
					{
						progressForm.Close();
					}

					if (resultForm != null && resultForm.Visible)
					{
						resultForm.Close();
					}

					if (recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
					{
						recalculateprogressForm.Close();
					}

					if (fApplyButton != null)
					{
						fApplyButton.Click -= ApplyButtonClick;
					}

					if (fPostButton != null)
					{
						fPostButton.Click -= PostButtonClick;
					}
				}
			}
			finally
			{
				base.Dispose(isNotFinalizing);
			}
		}

		#endregion

		#region Show More/Less

		void ZOrganisationsForm_Load(object sender, EventArgs e)
		{
			if (!this.IsDesignMode())
			{
				var isShowingAllTabs = !OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).Value;

				if (IsProductivityWiseModeEnabled || !isShowingAllTabs)
				{
					HideTabsThatShouldBeHidden(isShowingAllTabs);
				}
			}

			if (BusinessEntity is ICancellable cancellable && cancellable.IsCancelled)
			{
				BusinessEntity.IncrementReadOnlyIncludingChildren();
				SetReadOnlyIncludingChildren();
			}
			else
			{
				SetTabsReadonlynessBasedOnIsGlobalSupplierSecurity(true);
			}

			SalesTabPage.SetupSecurity(Env.Security.OrgSalesView);
			CompetitorTabPage.SetupSecurity(Env.Security.OrgCompetitorView);
			ReceivablesTabPage.SetupSecurity(Env.Security.OrgReceivablesView);
			PayablesTabPage.SetupSecurity(Env.Security.OrgPayablesView);

			if (!Organisation.SecurityProvider.HasModifyBranchProxies || !Organisation.SecurityProvider.HasModifyCompanyProxies)
			{
				if (DisplayMode != ODisplayMode.Delete)
				{
					DisableSaveButton();
				}

				DisableButtonOnTabPage(OrganisationsTabControl);
			}
		}

#if DEBUG
		internal
#endif

		bool enabledTabsOnly;

		void OrganisationTypeChangedForHidingTabs(object sender, EventArgs e)
		{
			if (enabledTabsOnly)
			{
				HideTabsThatShouldBeHidden(false);
			}
		}

#if DEBUG
		internal
#endif

		void ViewEnabledTabsOnlyClick(object sender, EventArgs e)
		{
			OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			HideTabsThatShouldBeHidden(false);
		}

		void HideTabsThatShouldBeHidden(bool isShowingAllTabs)
		{
			var selectedTab = OrganisationsTabControl.SelectedTab;
			OrganisationsTabControl.SelectedIndex = 0;
			AddAllTabPagesAndSetTheirOrder();

			var infos = new[]
			{
				new TabPageVisibilityInfo(ReceivablesTabPage, Organisation.OH_IsDebtor, true),
				new TabPageVisibilityInfo(PayablesTabPage, Organisation.OH_IsCreditor, true),
				new TabPageVisibilityInfo(ConsignorTabPage, Organisation.OH_IsConsignor, false),
				new TabPageVisibilityInfo(ConsigneeTabPage, Organisation.OH_IsConsignee, false),
				new TabPageVisibilityInfo(WhsFacilityTabPage, Organisation.OH_IsWarehouseClient, false),
				new TabPageVisibilityInfo(TransportTabPage, Organisation.OH_IsShippingProvider, false),
				new TabPageVisibilityInfo(ForwarderTabPage, Organisation.OH_IsForwarder, false),
				new TabPageVisibilityInfo(SalesTabPage, Organisation.OH_IsSalesLead, true),
				new TabPageVisibilityInfo(CompetitorTabPage, Organisation.OH_IsCompetitor, false),
				new TabPageVisibilityInfo(MiscServicesTabPage, Organisation.OH_IsMiscFreightServices, false),
			};

			foreach (var info in infos.Where(x => !x.IsEnabled))
			{
				RemoveTabPageIfPresent(info.TabPage);
			}

			if (IsProductivityWiseModeEnabled)
			{
				foreach (var info in infos.Where(x => !x.IsProductivityWiseSupported))
				{
					RemoveTabPageIfPresent(info.TabPage);
				}
			}

			if (!isShowingAllTabs)
			{
				foreach (var info in infos)
				{
					if (!info.IsOrgTypeEnabled)
					{
						RemoveTabPageIfPresent(info.TabPage);
					}
				}
			}

			if (OrganisationsTabControl.TabPages.Contains(selectedTab))
			{
				OrganisationsTabControl.SelectedTab = selectedTab;
			}
			else
			{
				OrganisationsTabControl.SelectedIndex = 0;
			}

			enabledTabsOnly = true;
		}

		void RemoveTabPageIfPresent(ZTabPage tabPage)
		{
			if (OrganisationsTabControl.TabPages.Contains(tabPage))
			{
				OrganisationsTabControl.TabPages.Remove(tabPage);
			}
		}

		class TabPageVisibilityInfo
		{
			public TabPageVisibilityInfo(ZTabPage tabPage, bool isOrgTypeEnabled, bool isProductivityWiseSupported, bool isEnabled = true)
			{
				TabPage = tabPage;
				IsOrgTypeEnabled = isOrgTypeEnabled;
				IsProductivityWiseSupported = isProductivityWiseSupported;
				IsEnabled = isEnabled;
			}

			public ZTabPage TabPage { get; }
			public bool IsOrgTypeEnabled { get; }
			public bool IsProductivityWiseSupported { get; }
			public bool IsEnabled { get; }
		}

#if DEBUG
		internal
#endif

		void ViewAllTabsClick(object sender, EventArgs e)
		{
			ZTabPage selectedTab = OrganisationsTabControl.SelectedTab;
			OrganisationsTabControl.SelectedIndex = 0;
			AddAllTabPagesAndSetTheirOrder();
			OrganisationsTabControl.SelectedTab = selectedTab;
			OrganisationsDataRegistry.Instance.GetShowEnabledOrganisationTabs(GlbStaff.CurrentUser.PK.ToGuid()).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			enabledTabsOnly = false;

			if (IsProductivityWiseModeEnabled)
			{
				HideTabsThatShouldBeHidden(isShowingAllTabs: true);
			}
		}

		#endregion

		#region Activate/Deactivate

		void InitializeActivationMenuItems()
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, "-", null);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ddad01b7-9c4d-4a86-ba44-645c09692857", "Activate"), ActivateMenuItemHandler);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("80d423ca-b52b-4fab-bf79-129f5348caf0", "Deactivate"), DeActivateMenuItemHandler);
		}

		void ActivateMenuItemHandler(object sender, EventArgs e)
		{
			if (!Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed)
			{
				Globals.Message.ShowInformation(Res.GetString("9d9816fa-f029-410d-b4b5-3aaf99fd1aa9", "You do not have rights to activate/deactivate organizations."));
			}
			else if (!Organisation.SecurityProvider.CanEditGlobalSupplier)
			{
				Globals.Message.ShowInformation(Res.GetString("5c139218-8c2c-4fa1-9162-69846f4e6b3e", "You do not have rights to modify organizations flagged as Global Supplier."));
			}
			else if (Organisation.OH_IsActive)
			{
				Globals.Message.ShowInformation(Res.GetString("661179e3-0381-41a2-a097-064e252d5fd7", "Organization is already active."));
			}
			else
			{
				ActivateOrganisation();
			}
		}

		public virtual void ActivateOrganisation()
		{
			this.Close();
			var organisationInNewFactory = new BusinessObjectFactory().Load<OrgHeader>(Organisation.PK);
			organisationInNewFactory.OH_IsActive = true;
			var newForm = new ZOrganisationsForm(organisationInNewFactory);
			newForm.ActivateOrganisationForForm();
			ZController.ShowModelessFormCore(newForm);
#if DEBUG
			formNewlyShown = newForm;
#endif
		}

#if DEBUG
		public ZOrganisationsForm formNewlyShown;
#endif

		public void ActivateOrganisationForForm()
		{
			DisplayMode = ODisplayMode.Edit;
			ButtonsUserControl.SaveButton.Visible = false;
			ButtonsUserControl.SaveAndCloseButton.Text = Res.GetString("ddad01b7-9c4d-4a86-ba44-645c09692857", "Activate");
			ButtonsUserControl.SaveAndCloseButton.BackColor = Color.Red;
			ButtonsUserControl.SaveAndCloseButton.Visible = true;
		}

		void DeActivateMenuItemHandler(object sender, EventArgs e)
		{
			OrgDeactivateHelper.SecurityCheckAndDeactivateOrganization(BusinessEntity, Organisation, DeActivateOrganisation);
		}

		public void DeActivateOrganisation()
		{
			Organisation.OH_IsActive = false;
			BusinessEntity.Factory.SuspendValidation();
			Organisation.SetReadOnlyIncludingChildren(true);
			SetReadOnlyIncludingChildren();
			ButtonsUserControl.SaveButton.Visible = false;
			ButtonsUserControl.SaveAndCloseButton.Text = Res.GetString("80d423ca-b52b-4fab-bf79-129f5348caf0", "Deactivate");
			ButtonsUserControl.SaveAndCloseButton.BackColor = Color.Red;
			ButtonsUserControl.SaveAndCloseButton.Visible = true;
		}

		#endregion

		#region Format All Contacts' Phone Numbers

		void OnFormatAllContactPhoneNumbers(object sender, EventArgs e)
		{
			Organisation.Contacts.FormatAllContactPhoneNumbers();
		}

		#endregion

		#region OrgProxies Serurity

		public override ODisplayMode DisplayMode
		{
			get
			{
				if (Organisation != null && !Organisation.IsDeleted &&
					(base.DisplayMode == ODisplayMode.Edit || base.DisplayMode == ODisplayMode.Browse) &&
					(!Organisation.SecurityProvider.HasModifyBranchProxies || !Organisation.SecurityProvider.HasModifyCompanyProxies))
				{
					return ODisplayMode.ReadOnly;
				}

				return base.DisplayMode;
			}
			set => base.DisplayMode = value;
		}

		void DisableSaveButton()
		{
			fPostButton.Visible = false;
			fApplyButton.Visible = false;
		}

		void DisableButtonOnTabPage(Control control)
		{
			if (control is ZTabPage page)
			{
				page.RunWhenTabInitialized(delegate
				{ DisableButtonOnTabPageCore(page); });
			}
			else
			{
				DisableButtonOnTabPageCore(control);
			}
		}

		void DisableButtonOnTabPageCore(Control control)
		{
			if (control is ZButton button)
			{
				button.Enabled = false;
			}
			else
			{
				foreach (Control child in control.Controls)
				{
					DisableButtonOnTabPage(child);
				}
			}
		}
		#endregion

		#region ExportPatternMatchOverride

		protected ExportPatternMatchOverridesUtil ExportPatternMatchOverridesUtil { get { return new ExportPatternMatchOverridesUtil(() => { return new List<OrgHeader>() { Organisation }; }); } }

		void AddExportPatternOverrideMenuItem()
		{
			ExportPatternMatchOverridesUtil.AddExportMenuItem(ActionsMenuItem, null);
		}

		#endregion ExportPatternMatchOverride

		#region Enroll for Electronic Bill of Lading

		CancellationTokenSource BoleroCancellationTokenSource { get; } = new CancellationTokenSource();
		internal bool BoleroEnrollmentRequestSent { get; set; }

		void AddEnrollForElectronicBillOfLadingMenuItem()
		{
			//TODO: Set BoleroEnrollmentFeature to be "ACTIVE" and apply the feature control in the logic in a subsequent WI, add Env.Security.EnrollForElectronicBillOfLading.IsAllowed
			if (Env.Security.EnrollForElectronicBillOfLading.IsAllowed && OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("OrganisationsForm|ActionsMenu|EnrollForElectronicBillOfLading", "Enroll For Electronic Bill Of Lading"), EnrollForElectronicBillOfLadingClick);
			}
		}

		async void EnrollForElectronicBillOfLadingClick(object sender, EventArgs e)
		{
			await EnrollForElectronicBillOfLading();
		}

		protected async Task EnrollForElectronicBillOfLading()
		{
			if (Organisation.IsProxyOrgOfAnyBranchOnly(false))
			{
				Globals.Message.ShowError(Res.GetString("A828CAB7-BB24-4439-97CE-4557CD5EB5A0", "Enrollment request not supported on internal organizations"));
				return;
			}

			if (Organisation.CustomsCodes.Any(c => ((OrgCusCode)c).OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID))
			{
				Globals.Message.Show(Res.GetString("2B7D04DA-2B0F-4823-96A6-9421E264454F", "The customer, {0}, is already onboarded with Bolero to use the Electronic Bill of Lading functionality. You are not allowed to send another enrollment request.", Organisation.OH_FullName));
				return;
			}

			if (BoleroEnrollmentRequestSent)
			{
				Globals.Message.Show(Res.GetString("72F1B0A5-6192-4AAF-B841-A0359C613F91", "You have already sent an enrollment request, and a response is awaited."));
			}
			else
			{
				var helper = new BoleroOnBoardingHelper(Organisation);
				var overrideFlag = false;

				if (helper.IsOverriddenRequest())
				{
					overrideFlag = true;
					var confirmationDialogResult = ShowConfirmationPrompt();

					if (confirmationDialogResult != DialogResult.Yes)
					{
						return;
					}
				}

				var invitationDetails = new BoleroInvitationDetails(Organisation);
				var boleroInvitationForm = new BoleroInvitationForm(invitationDetails);
				var result = ZFormModaliser.ShowDialogAndDispose(boleroInvitationForm);
				if (result == DialogResult.OK)
				{
					try
					{
						var requestDTO = boleroInvitationForm.OnboardingRequestDTO;
						if (requestDTO?.RequestMetaDataDto != null)
						{
							requestDTO.RequestMetaDataDto.OverrideFlag = overrideFlag;
						}

						BoleroEnrollmentRequestSent = true;
						var enrollmentResult = await helper.SendEnrollmentRequestAsync(requestDTO, BoleroCancellationTokenSource.Token);

						if (!IsDisposing && !IsDisposed && !string.IsNullOrEmpty(enrollmentResult))
						{
							Globals.Message.Show(enrollmentResult);
						}
					}
					finally
					{
						BoleroEnrollmentRequestSent = false;
					}
				}
			}
		}

		DialogResult ShowConfirmationPrompt()
		{
			var showMessage = Res.GetString("30A33D3A-428E-40A2-BB5E-2F6BE77209A0",
				@"The Bolero system is processing the previously submitted Enrollment Request. Resending may cause errors or duplication.
Do you want to re-submit this Enrollment Request?");
			var confirmationDialogResult = Globals.Message.Show(showMessage,
				Res.GetString("4B22E5A4-3FD1-4F82-95F4-1208DD71BBC8", "Enroll for Electronic Bills of Lading"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			return confirmationDialogResult;
		}

		#endregion

		#region OnClosing

		protected override void OnClosing(CancelEventArgs e)
		{
			if (BoleroEnrollmentRequestSent)
			{
				if (Globals.Message.Show(Res.GetString("d062ddbb-84d5-4233-acae-0cd973139635", "The Bolero system is processing the Enrollment Request you submitted. Closing the Organization form will lead to termination of the request.\r\nDo you want to still close the Organization form?"), Res.GetString("f96f35d2-2d9b-426e-ab75-a400c328c99e", "Confirmation"), MessageBoxButtons.YesNo, DialogResult.Cancel) == DialogResult.Yes)
				{
					BoleroCancellationTokenSource.Cancel();
					base.OnClosing(e);
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				base.OnClosing(e);
			}
		}

		#endregion
	}
}
