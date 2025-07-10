using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GUI
{
	public abstract class BaseBrokeragePlugIn : ZMutexedPlugIn
	{
		bool IsBorderWiseMultiLineClassificationEnabled => ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.BorderWiseWeb
													   && ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient
													   && ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassification;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public BaseBrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
			this.Shipment = shipment;
			if (shipment == null)
			{
				ErrorReporter.ReportOnce("HostBusinessEntity Shipment is null in PlugIn", "HostBusinessEntity Shipment is null in PlugIn");
			}

			if (Env.Security.CustomsDeclarationEnquiryNew.IsAllowed)
			{
				shipment.JS_OH_ExportBrokerInfo.ValueChanged += new EventHandler(JS_OH_ExportBrokerInfo_ValueChanged);
				shipment.JS_OH_ImportBrokerInfo.ValueChanged += new EventHandler(JS_OH_ImportBrokerInfo_ValueChanged);
			}

			StartPlugInSynchroniser();
			Controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
		}

		#region PlugIn Override

		public override void UpdateTabPageMinimumAutoSized()
		{
			TabPage.MinimumAutoSizedWidth = UserControl.MinimumSize.Width;
			TabPage.MinimumAutoSizedHeight = UserControl.MinimumSize.Height;
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new BrokerageAutoSizedTabPagePlugIn(this);
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Broker; }
		}

		bool gUIHasBeenShown;

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			if (!gUIHasBeenShown)
			{
				foreach (MenuItem menu in TopLevelMenu.MenuItems)
				{
					menu.Select += new EventHandler(Menu_Select);
				}
			}
			gUIHasBeenShown = true;
		}

		void Menu_Select(object sender, EventArgs e)
		{
			checkRights = true;
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			checkRights = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Plugin Tab Name")]
		public const string BrokeragePlugInName = "Brokerage";
		public override string Name
		{
			get { return BrokeragePlugInName; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override sealed Control GetNewUserControl()
		{
			BaseCustomsBrokerageUserControl result = CreateBrokerageUserControl();
			if (result != null)
			{
				result.JobDeclaration = InternalJobDeclaration;
			}
			return result;
		}

		protected override sealed MenuItem GetNewTopLevelMenu()
		{
			if (fMenu == null)
			{
				var declaration = JobDeclaration;
				fMenu = declaration != null && declaration.JE_IsCancelled ? new EDIMenuStub() : GetNewTopLevelMenuCore();
			}

			return fMenu;
		}
		MenuItem fMenu;

		protected virtual MenuItem GetNewTopLevelMenuCore()
		{
			return new EDIMenu();
		}

		bool checkRights;

		protected virtual BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			return new BaseCustomsBrokerageUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return JobDeclaration;
		}

		#endregion

		#region JobDeclaration

		protected internal readonly ForwardingShipment Shipment;

		protected bool HasJobDeclarationBeenLoaded
		{
			get { return fJobDeclaration != null; }
		}

		public BaseJobDeclaration JobDeclaration
		{
			get { return InternalJobDeclaration; }
		}

		BaseJobDeclaration fJobDeclaration;
		protected internal BaseJobDeclaration InternalJobDeclaration
		{
			get
			{
				if (fJobDeclaration == null)
				{
					InternalJobDeclaration = GetDeclarationFromShipment();
				}
				return fJobDeclaration;
			}
			set
			{
				if (fJobDeclaration != value)
				{
					UnHookDeclarationEvents(fJobDeclaration);
				}

				fJobDeclaration = value;
				if (fJobDeclaration != null)
				{
					JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(fJobDeclaration.Factory, fJobDeclaration.PK);
					var shipmentPK = Shipment.PK;
					if (fJobDeclaration.JE_JS != shipmentPK)
					{
						fJobDeclaration.JE_JS = shipmentPK;
					}
					OnJobDeclarationSet();
					fJobDeclaration.LicenceLogin += DeclarationLicenceLogin;
					fJobDeclaration.BondedWarehouseLicenceLogin += DeclarationLicenceLogin;
					HookDeclarationEvents(fJobDeclaration);
				}
			}
		}

		void Table_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e)
		{
			var row = ((INeedRow)fJobDeclaration)?.Row;
			if (row != null && e.Row == row)
			{
				ErrorReporter.ReportOnce("Declaration on plugin should not be deleted", FormattableString.Invariant($"Declaration row is being deleted (Action={e.Action}, State={row.RowState},PK={fJobDeclaration.PK})"));
			}
		}

		void HookDeclarationEvents(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				var table = ((INeedRow)declaration).Row.Table;
				table.RowDeleting -= Table_RowDeleting;
				table.RowDeleting += Table_RowDeleting;
				HookDeclarationEventsCore(declaration);
			}
		}

		protected virtual void HookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
		}

		void UnHookDeclarationEvents(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				var table = ((INeedRow)declaration).Row.Table;
				table.RowDeleting -= Table_RowDeleting;
				UnHookDeclarationEventsCore(declaration);
			}
		}

		protected virtual void UnHookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			declaration.LicenceLogin -= DeclarationLicenceLogin;
			declaration.BondedWarehouseLicenceLogin -= DeclarationLicenceLogin;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				fMenu?.Dispose();

				if (Shipment != null)
				{
					Shipment.JS_OH_ExportBrokerInfo.ValueChanged -= new EventHandler(JS_OH_ExportBrokerInfo_ValueChanged);
					Shipment.JS_OH_ImportBrokerInfo.ValueChanged -= new EventHandler(JS_OH_ImportBrokerInfo_ValueChanged);
				}

				UnHookDeclarationEvents(fJobDeclaration);
			}

			if (fJobDeclaration != null)
			{
				JobComInvoiceLinePartSynchronisationManager.StopManagingWhenActiveDeciderPKWasDisposed(fJobDeclaration.Factory, fJobDeclaration.PK);
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Set BusinessObject of MenuItem, or hook events etc
		/// </summary>
		protected virtual void OnJobDeclarationSet()
		{
			InternalJobDeclaration.ExternalFactoryRefreshEnabled = true;
			IEDIMenu topLevelMenu = (IEDIMenu)TopLevelMenu;
			if (topLevelMenu != null)
			{
				topLevelMenu.Declaration = InternalJobDeclaration;
			}
		}

		protected BaseJobDeclaration GetDeclarationFromShipmentFromLocalCache()
		{
			ZQuery query = CreateJobDeclarationFilter();
			query.FetchOnlyFromLocalCache = true;
			BaseJobDeclaration[] declarations = Shipment.Factory.Load<BaseJobDeclaration>(query);
			return declarations.Length == 0 ? null : declarations[0];
		}

		protected internal BaseJobDeclaration GetDeclarationFromShipment()
		{
			return (BaseJobDeclaration)Shipment.GetDeclaration();
		}

		protected internal ZQuery CreateJobDeclarationFilter()
		{
			return JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, Shipment.PK);
		}

		#endregion

		#region Mutex

		protected ZGlobalMutex fMutex;
		public override ZGlobalMutex Mutex
		{
			get { return fMutex ?? (fMutex = Enterprise.Customs.Common.DeclarationBeingCreatedForShipmentMutexCreator.Create(Shipment.PK)); }
		}

		protected virtual IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			yield break;
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();

			if (checkRights)
			{
				if (JobDeclaration.HasChanges || !JobDeclaration.IsInDatabase)
				{
					result = new BaseJobDeclarationSecurityAlertHelper(JobDeclaration).ShowSecurityAlertMessage();
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				HandleApportionmentOnSaving();
				foreach (var preSaveDialogStrategy in GetPreSaveDialogStrategies())
				{
					result = preSaveDialogStrategy.ShowPreSaveDialogs(result);
				}

				BaseJobDeclaration declarationFromLocalCache = GetDeclarationFromShipmentFromLocalCache();
				if (declarationFromLocalCache != null)
				{
					if (declarationFromLocalCache.HasChanges)
					{
						result = new MergePreSaveDialogStrategy(JobDeclaration).ShowPreSaveDialogs(result);
					}

					if (result == ContinueWithSave.Yes && declarationFromLocalCache.HasChanges && !declarationFromLocalCache.IsAmendmentDetectionSuspended)
					{
						IMessageManageableBizObj messageManageableDeclaration = JobDeclaration as IMessageManageableBizObj;
						if (messageManageableDeclaration != null && messageManageableDeclaration.IsInAStatusAmendmentSendable)
						{
							IMessageManager manager = messageManageableDeclaration.GetMessageManagerForAmendmentDetection();

							if (manager != null)//Depending on the message type, amendment might not be supported and thus, manager can be null
							{
								result = GetNewMessagingActionsController().DetermineRequiredMessagesAndSendThem(manager);
							}
						}

						if (IsBorderWiseMultiLineClassificationEnabled && base.UserControl is BaseCustomsBrokerageUserControl customsBrokerageUserControl)
						{
							BorderWiseAsyncBatchTariffProcessorProvider.SendMessageAndDisposeConnectionIfNeeded(JobDeclaration, customsBrokerageUserControl.GetInvoicePksAction(), true);
							customsBrokerageUserControl.ClearInvoicePksAction();
						}
					}

					if (result == ContinueWithSave.Yes && Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed)
					{
						if (declarationFromLocalCache.InvoicesMentionNewOrInactiveProducts
							&& DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(declarationFromLocalCache)
							&& DeclarationForProductCreationHelperCollection.ShouldShowAutomaticProductCreationConfirmation(declarationFromLocalCache))
						{
							ZFormModaliser.ShowDialogAndDispose(new ProductCreationConfirmationForm(declarationFromLocalCache));
						}
						if (declarationFromLocalCache.InvoicesMentionProductsWithoutMatchingClassification)
						{
							ZFormModaliser.ShowDialogAndDispose(new ProductClassificationCreationConfirmationForm(declarationFromLocalCache));
						}
					}
				}
			}

			if (JobDeclaration != null && result == ContinueWithSave.Yes)
			{
				var declarationAndBrokerageCommon = GetShipmentAndBrokergeCommon(JobDeclaration);
				result = declarationAndBrokerageCommon.IsSupervisorApproved();
			}

			return result;
		}

		protected virtual BaseShipmentAndBrokerageCommon GetShipmentAndBrokergeCommon(BaseJobDeclaration declaration)
		{
			return new BaseShipmentAndBrokerageCommon(declaration);
		}

		protected virtual SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return new SendsMessagesToCustomsGUI();
		}

		#endregion

		#region Automatic Synchronisation

		protected virtual void StartPlugInSynchroniser()
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)declaration).SynchroniseWithShipmentIfNeeded();
			}
		}

		#endregion

		#region Implementation

		void DeclarationLicenceLogin(object sender, LicenceLoginEventArgs e)
		{
			if (gUIHasBeenShown)
			{
				e.LoginHasBeenAttempted = true;
				e.LicenceCheckPoint.Login(this);
			}
		}

		protected static string NoDeclarationExistsText
		{
			get { return Res.GetString("BrokeragePlugIn|NoDeclarationExistsText", "No declaration exists for this Login Company"); }
		}

		protected static string NotToCreateJobText
		{
			get { return Res.GetString("BrokeragePlugIn|NotToCreateJobText", "You have chosen not to create a declaration now.\r\nPlease change to another tab, then click back to this tab to create a Declaration for this Shipment."); }
		}

		protected internal static string MutexLockText
		{
			get { return Res.GetString("BrokeragePlugIn|MutexLockText", "Someone else is already in the process of creating a declaration for shipment.\r\nYou should be able to access the declaration when the person has saved the record. Please try later."); }
		}

		protected static string CreateDeclarationQuery
		{
			get { return Res.GetString("BrokeragePlugIn|CreateDeclarationQuery", "Are you sure you want to create a declaration now?"); }
		}

		protected static string CreateDeclarationWarningQuery
		{
			get { return Res.GetString("BrokeragePlugIn|CreateDeclarationWarningQuery", "Are you sure you want to create the Declaration?"); }
		}

		protected internal static string ImportDeclarationFromOtherCountryQuery
		{
			get { return Res.GetString("BrokeragePlugIn|ImportDeclarationFromOtherCountryQuery", "Do you wish to import data from this Declaration?"); }
		}

		protected static string NoLicense
		{
			get { return Res.GetString("7B46351C-235E-4A35-A027-EA3613DB5238", "License for the Brokerage module is not registered or expired."); }
		}

#if DEBUG
		public int QueryUserToCreateDeclarationCountForTesting;
#endif

		protected bool QueryUser(string question, string caption, bool showInTest)
		{
			DialogResult result = DialogResult.Yes;
			if (!Globals.IsTest || showInTest)
			{
				result = Globals.Message.Show(
					question,
					caption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);
			}
			return (result == DialogResult.Yes);
		}

		protected void HandleApportionmentOnSaving()
		{
			if (GetDeclarationFromShipmentFromLocalCache() != null &&
				JobDeclaration.ApportionmentDirty)
			{
				ZForm parentForm = null;
				if (UserControl != null)
				{
					parentForm = UserControl.FindForm() as ZForm;
				}

				using (ApportionmentProgressForm progressForm = new ApportionmentProgressForm(JobDeclaration))
				{
					if (parentForm != null)
					{
						ZFormModaliser.Show(progressForm, parentForm);
					}

					JobDeclaration.ResumeApportionment();
					progressForm.Close();
				}
			}
		}

		#endregion

		#region Validity

		public override ZString PlugInNotDisplayedMessage
		{
			get { return coveringLabelText; }
		}

		protected ZString coveringLabelText;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return InternalJobDeclaration != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			QueryAddDeclaration();
			return InternalJobDeclaration != null;
		}

		protected internal bool ShouldPluginDropdownMenuBeCreatedInternal() => ShouldPluginDropdownMenuBeCreated();
		protected override bool ShouldPluginDropdownMenuBeCreated()
		{
			return QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		protected void QueryAddDeclaration()
		{
			coveringLabelText = string.Empty;

			if (InternalJobDeclaration == null)
			{
				if (Shipment.ReadOnly)
				{
					coveringLabelText = NoDeclarationExistsText;
				}
				else
				{
					InternalJobDeclaration = CreateDeclarationHelper.CreateDeclaration(Shipment, Mutex, () => InternalJobDeclaration);
				}
			}
		}

		protected CreateDeclarationHelper CreateDeclarationHelper
		{
			get
			{
				if (fCreateDeclarationHelper == null)
				{
					fCreateDeclarationHelper = GetCreateDeclarationHelperCore();
					fCreateDeclarationHelper.ConfirmCreateNewDeclaration = true;
					fCreateDeclarationHelper.Notify = new CreateDeclarationHelper.NotifyDelegate((shipment, type) => { Notify(shipment, type); });
					fCreateDeclarationHelper.GetAnswer = new CreateDeclarationHelper.GetAnswerDelegate((shipment, questions, type) => { return QueryUser(shipment, questions, type); });
				}
				return fCreateDeclarationHelper;
			}
		}
		CreateDeclarationHelper fCreateDeclarationHelper;

		protected virtual CreateDeclarationHelper GetCreateDeclarationHelperCore()
		{
			return new CreateDeclarationHelper();
		}

		protected virtual void Notify(ForwardingShipment shipment, CreateDeclarationHelper.NotifyType notifyType)
		{
			switch (notifyType)
			{
				case (CreateDeclarationHelper.NotifyType.MutexLocked):
					coveringLabelText = MutexLockText;
					break;
				case (CreateDeclarationHelper.NotifyType.NotToCreateJob):
					coveringLabelText = NotToCreateJobText;
					break;
				case (CreateDeclarationHelper.NotifyType.SecurityError):
					Env.Security.CustomsDeclarationEnquiryNew.ShowError();
					break;
			}
		}

		protected virtual bool QueryUser(ForwardingShipment shipment, ICollection<CreateBrokerageQuestion> questions, CreateDeclarationHelper.QuestionType type)
		{
			var result = true;
			var messages = questions.Where(q => q.ConditionToAsk != null && q.ConditionToAsk(shipment) && !q.Answer).Select(q => q.Message);

			if (messages.Any())
			{
				string queryText = GetQueryText(messages.ToArray(), type);
				if (queryText != null)
				{
#if DEBUG
					QueryUserToCreateDeclarationCountForTesting++;
#endif
					result = type == Business.CreateDeclarationHelper.QuestionType.ImportDeclarationQuery
						? QueryUser(queryText, Res.GetString("63141947-93e3-4b11-8cb2-6b805827c776", "Import Declaration"), true)
						: QueryUser(queryText, Res.GetString("c0cb2317-c43f-4ca2-be66-92cfb20abdff", "Create Declaration for Shipment"), false);
				}
			}

			return result;
		}

		protected virtual string GetQueryText(string[] messages, CreateDeclarationHelper.QuestionType type)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (var message in messages)
			{
				builder.AppendIfNotEmpty(message);
			}
			switch (type)
			{
				case CreateDeclarationHelper.QuestionType.CreateDeclarationQuery:
					builder.Append(CreateDeclarationQuery);
					break;

				case CreateDeclarationHelper.QuestionType.CreateDeclarationWarning:
					builder.Append(CreateDeclarationWarningQuery);
					break;

				case CreateDeclarationHelper.QuestionType.ImportDeclarationQuery:
					builder.Append(ImportDeclarationFromOtherCountryQuery);
					break;
			}
			return builder.ToStringWithDelimiterBetweenAppends(" ");
		}

#if DEBUG
		protected virtual
#endif
		void JS_OH_ExportBrokerInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!Factory.IsInTransaction && Shipment.IsExport() && Shipment.JS_OH_ExportBroker == GlbBranch.CurrentBranch.GB_OH_OrgProxy)
			{
				QueryAddDeclaration();
			}
		}

#if DEBUG
		protected virtual
#endif
		void JS_OH_ImportBrokerInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!Factory.IsInTransaction && Shipment.IsImport() && Shipment.JS_OH_ImportBroker == GlbBranch.CurrentBranch.GB_OH_OrgProxy)
			{
				QueryAddDeclaration();
			}
		}

		#endregion
	}
}
