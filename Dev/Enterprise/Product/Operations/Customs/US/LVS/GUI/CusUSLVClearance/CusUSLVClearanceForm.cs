using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class CusUSLVClearanceForm : ZTemplateForm
	{
		public CusUSLVClearanceForm(CusUSLVClearance cusUSLVClearance) : base(cusUSLVClearance)
		{
			InitializeComponent();
			Customs.Business.JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(cusUSLVClearance.Factory, cusUSLVClearance.PK);
			AddMessagingMenu();
			workflowTabPage.Initialize(cusUSLVClearance);
			AddMenuItemsIntoActions();
			if (!DesignModeFinder.IsDesigning)
			{
				MainTabPage.RunWhenBindingOrFirstShown((_, __) =>
				{
					gridHouseBills.ContextMenu.Popup += HouseBillContextMenu_Popup;
					gridHouseBills.DoubleClick += new EventHandler(gridHouseBills_DoubleClick);
				});
			}
		}

		void HouseBillContextMenu_Popup(object sender, EventArgs e)
		{
			if (!gridHouseBills.ContextMenu.MenuItems.Contains(convertPartyAddressesToNewOrgContextMenuItem))
			{
				gridHouseBills.ContextMenu.MenuItems.Add(ZMenuItem.Separator);
				gridHouseBills.ContextMenu.MenuItems.Add(convertPartyAddressesToNewOrgContextMenuItem ??= GetConvertPartyAddressesToNewOrgMenuItem());
			}
			if (menuItemConvertToFormalDeclaration == null)
			{
				menuItemConvertToFormalDeclaration = ConvertToStandAloneDeclarationHelper.CreateConvertToStandAloneDeclarationMenuItem((_, __) =>
				{
					ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationIndividual(BusinessEntity.Factory, gridHouseBills.SelectedElements, BusinessEntity.Logs, true);
				});

				gridHouseBills.ContextMenu.MenuItems.Add(menuItemConvertToFormalDeclaration);
			}

			if (menuItemMessaging == null)
			{
				menuItemMessaging = MessagingContextMenuItemHelper.CreateMessagingContextMenuItems(SendOriginalMessage_Click, SendReplacementMessage_Click, SendDeletionMessage_Click);
				gridHouseBills.ContextMenu.MenuItems.Add(menuItemMessaging);
			}

			menuItemConvertToFormalDeclaration.Visible = ConvertToStandAloneDeclarationHelper.SetMenuItemVisible(gridHouseBills);
		}

		void gridHouseBills_DoubleClick(object sender, EventArgs e)
		{
			if (gridHouseBills.SelectedElements.Length > 0)
			{
				var consignment = (CusUSLVConsignment)gridHouseBills.SelectedElements[0];
				ShowCusUSLVConsignmentForm(consignment);
			}
		}

		void ShowCusUSLVConsignmentForm(BusinessObject consignment)
		{
			if (consignment != null)
			{
				var form = new CusUSLVConsignmentForm((CusUSLVConsignment)consignment);
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		void SendOriginalMessage_Click(object sender, EventArgs e)
		{
			var (billsSent, totalBills) = SendMessages(UpdateActionCode.Add);
			Globals.Message.Show(Res.GetString("1a357627-3709-4b76-894e-e7820c323a22", "{0} original message(s) generated; {1} message(s) ignored.", billsSent, totalBills - billsSent), CustomsSendMessageHelper.GetDialogBoxCaption(UpdateActionCode.Add), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void SendReplacementMessage_Click(object sender, EventArgs e)
		{
			var (billsSent, totalBills) = SendMessages(UpdateActionCode.Replace);
			Globals.Message.Show(Res.GetString("d25996f2-9ce1-4af3-affc-de2d3a307868", "{0} replacement message(s) generated; {1} message(s) ignored.", billsSent, totalBills - billsSent), CustomsSendMessageHelper.GetDialogBoxCaption(UpdateActionCode.Replace), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void SendDeletionMessage_Click(object sender, EventArgs e)
		{
			SendMessages(UpdateActionCode.Delete);
		}

		MenuItem menuItemConvertToFormalDeclaration;
		MenuItem menuItemMessaging;

		public new CusUSLVClearance BusinessEntity
		{
			get { return (CusUSLVClearance)base.BusinessEntity; }
		}

		public override string FormCaption => BusinessEntity.HumanReadableName;

		void AddMessagingMenu()
		{
			var messagingMenuItem = new EDIMenu();

			messagingMenuItem.Header = BusinessEntity;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenuItem);

			messagingMenuItem.MenuItems.Add(new ZMenuItem("-"));

			var convertToStandAloneDeclarationMenuItems = ConvertToStandAloneDeclarationHelper.NavigateToConvertToStandAloneDeclarationMenuItem();
			convertToStandAloneDeclarationMenuItems.MenuItems.Add(ConvertToStandAloneDeclarationHelper.ManualSelection(ManualSelectionToConvertToStandAloneDeclaration));
			convertToStandAloneDeclarationMenuItems.MenuItems.Add(ConvertToStandAloneDeclarationHelper.WithMessageErrors(ConvertConsignmentsWithMessageErrorsToStandAloneDeclaration));
			convertToStandAloneDeclarationMenuItems.MenuItems.Add(ConvertToStandAloneDeclarationHelper.WithPGARequirements(ConvertConsignmentsWithPGARequirementsToStandAloneDeclaration));
			messagingMenuItem.MenuItems.Add(convertToStandAloneDeclarationMenuItems);

			var consolidatedSummaryMenuItem = messagingMenuItem.MenuItems.Add(ZString.Empty, OnClickConsolidatedSummaryMenuItem);
			messagingMenuItem.MenuItems.Add(consolidatedSummaryMenuItem);
			messagingMenuItem.Popup += (sender, e) => SetVisibilityAndCaptionForConsolidatedSummaryMenuItem(consolidatedSummaryMenuItem);
		}

		#region StandAloneDeclaration

		void ManualSelectionToConvertToStandAloneDeclaration(object sender, EventArgs e)
		{
			if (!Env.Security.USLVClearanceConvertToFormalDeclaration.IsAllowed)
			{
				Env.Security.USLVClearanceConvertToFormalDeclaration.ShowError();
			}
			else if (BusinessEntity.HasChanges)
			{
				Globals.Message.ShowInformation(ConvertToStandAloneDeclarationHelper.SaveFormFirstPrompt);
			}
			else
			{
				BusinessEntity.NonApplicableConsignments.Refresh();
				ZFormModaliser.ShowDialogAndDispose(new BulkConvertToStandAloneDeclarationForm(BusinessEntity), this);
			}
		}

		void ConvertConsignmentsWithMessageErrorsToStandAloneDeclaration(object sender, EventArgs e)
		{
			var message = Res.GetString("6c14d257-9780-491a-9103-ab1c3fe21cb0", "This will convert all consignments with a message error to a standalone declaration, please choose from the below:");
			using (var messageBox = new ZMessageBox(message, "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, Res.GetString("4114853c-089b-4a9c-aab2-51831fed87a5", "Combined"), Res.GetString("570b6cc1-b73f-4ed2-8a88-0089bc298841", "Individual")))
			{
				var result = ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
				if (result != DialogResult.Cancel)
				{
					BusinessEntity.RunPreSaveValidation();

					var consignmentsToConvert = BusinessEntity.CusUSLVConsignments.Cast<CusUSLVConsignment>().Where(c => c.HasMessageErrors && c.CE_EntryNum.IsEmpty && c.CanBeConvertedToStandaloneDeclaration).ToArray();

					try
					{
						if (result == DialogResult.Yes)
						{
							ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationCombined(BusinessEntity.Factory, consignmentsToConvert, BusinessEntity.Logs);
						}
						else if (result == DialogResult.No)
						{
							ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationIndividual(BusinessEntity.Factory, consignmentsToConvert, BusinessEntity.Logs, false);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.GetInnermostException().Message);
					}
				}
			}
		}

		void ConvertConsignmentsWithPGARequirementsToStandAloneDeclaration(object sender, EventArgs e)
		{
			var message = Res.GetString("72033dba-7afd-4c68-bd1b-90c087a858bc", "This will convert all consignments with PGA requirements into individual standalone declarations, do you wish to continue?");
			var result = Globals.Message.Show(message, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				var consignmentsToConvert = BusinessEntity.CusUSLVConsignments.Cast<CusUSLVConsignment>()
											.Where(c => c.ULB_HasPGAPending && c.CE_EntryNum.IsEmpty && c.CanBeConvertedToStandaloneDeclaration).ToArray();

				ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationIndividual(BusinessEntity.Factory, consignmentsToConvert, BusinessEntity.Logs, false);
			}
		}

		#endregion

		#region ConsolidatedEntrySummary

		static string OpenConsolidatedSummaryMenuCaption => Res.GetString("4c807772-8f15-4aa8-b73e-f57259b2a2d2", "Open Consolidated Summary");

		void SetVisibilityAndCaptionForConsolidatedSummaryMenuItem(MenuItem consolidatedSummaryMenuItem)
		{
			if (BusinessEntity.HasEntryTypeInformalFreeDutiableOnAnyConsignment)
			{
				string menuItemCaption;
				if (BusinessEntity.HasConsolidatedSummaryDeclarations)
				{
					menuItemCaption = OpenConsolidatedSummaryMenuCaption;
				}
				else
				{
					menuItemCaption = Res.GetString("84407080-ace8-40af-aaea-fe76d0651078", "Create Consolidated Summary");
				}

				consolidatedSummaryMenuItem.Text = menuItemCaption;
				consolidatedSummaryMenuItem.Visible = true;
			}
			else
			{
				consolidatedSummaryMenuItem.Visible = false;
			}
		}

		void OnClickConsolidatedSummaryMenuItem(object sender, EventArgs args)
		{
			if (BusinessEntity.HasChanges)
			{
				Globals.Message.ShowInformation(ConvertToStandAloneDeclarationHelper.SaveFormFirstPrompt);
			}
			else
			{
				var declarations = BusinessEntity.ConsolidatedSummaryDeclarations;
				if (declarations.IsNullOrEmpty())
				{
					var ediMenu = ((MenuItem)sender).Parent as MenuItem;
					var openConsolidatedSummaryMenuPath = $"{ediMenu.Text.Replace("&", "")} > {OpenConsolidatedSummaryMenuCaption}";

					CreateConsolidatedSummary(openConsolidatedSummaryMenuPath);
				}
				else
				{
					OpenConsolidatedSummary(declarations);
				}
			}
		}

		void CreateConsolidatedSummary(string openConsolidatedSummaryMenuPath)
		{
			var declarations = CreateConsolidatedSummaryCore();

			if (declarations.Count == 1)
			{
				ShowSingleDeclarationForm(declarations[0]);
			}
			else if (declarations.Count > 1)
			{
				FireSaveButton();
				Globals.Message.ShowInformation(Res.GetString("c4d7d664-4fe5-423e-9718-df174ff2ef5f", "{0} Consolidated Summaries are created, open each Consolidated Summary via {1}.", declarations.Count, openConsolidatedSummaryMenuPath));
			}
		}

		List<JobDeclaration> CreateConsolidatedSummaryCore()
		{
			var declarations = new List<JobDeclaration>();
			Action addCESLogActionOnFactorySaving = null;

			var progressForm = new ProgressForm();
			progressForm.ShowCancelButton = false;
			progressForm.CaptionResourceString = Res.GetData("346dfad2-6c46-4485-8eab-6c28fa1df14e", "Generating Consolidated Summaries");
			progressForm.ShowModalTo(this);
			progressForm.SetStatusAndPercentComplete(Res.GetString("cf70fb9f-0f62-4677-9fec-5f7058c5191d", "Preparing data source"), 0);

			var batchCount = BusinessEntity.CusUSLVConsignmentBatches.TotalBatchCount;
			if (batchCount == 1)
			{
				declarations.Add(ConvertSingleSummaryInNewFactory(progressForm, out addCESLogActionOnFactorySaving));
			}
			else if (UserConfirmToCreateMultipleConsolidatedEntrySummaries(batchCount))
			{
				declarations.AddRange(ConvertMultipleSummariesInCurrentFactory(progressForm, out addCESLogActionOnFactorySaving));
			}

			if (declarations.Count > 0)
			{
				foreach (var declaration in declarations)
				{
					declaration.AddUSLowValueTransferredLog(BusinessEntity.ULH_JobNumber);
				}

				var conversionFactory = declarations[0].Factory;

				conversionFactory.Saving -= FactorySaving;
				conversionFactory.Saving += FactorySaving;
			}

			progressForm.Close();

			return declarations;

			void FactorySaving(BusinessObjectFactory factory)
			{
				factory.Saving -= FactorySaving;

				var bulkCopyTables = new List<string>
					{
						OrgHeaderSchema.Constants.TableName,
						OrgAddressSchema.Constants.TableName,
						OrgMiscServSchema.Constants.TableName,
						OrgAddressCapabilitySchema.Constants.TableName,
						OrgCompanyDataSchema.Constants.TableName,
						OrgPatternMatchSchema.Constants.TableName,
						OrgInvoiceRollupOrGroupSchema.Constants.TableName,
						JobComInvoiceHeaderSchema.Constants.TableName,
						JobComInvoiceLineSchema.Constants.TableName,
						JobUSComInvoiceLineSchema.Constants.TableName,
						StmALogSchema.Constants.TableName,
					};
				factory.SetBulkCopyOnTables(bulkCopyTables, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, checkRowsShouldBePersistent: false, fireTriggers: true);

				addCESLogActionOnFactorySaving?.Invoke();
			}
		}

		JobDeclaration ConvertSingleSummaryInNewFactory(ProgressForm progressForm, out Action addCESLogAction)
		{
			var conversionFactory = new BusinessObjectFactory();
			var clearance = conversionFactory.Load<CusUSLVClearance>(BusinessEntity.PK);
			var conversionProgressInfoCollector = new LowValueEntriesToConsolidatedSummaryConvertTracker(clearance.CusUSLVConsignments.Count, (text, percetage) => UpdateProgress(text, percetage, progressForm));
			var declaration = default(JobDeclaration);

			var actionInfo = new ActionInfo(RecipientRoleType.BRO, clearance)
			{
				ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage
			};
			var dataContextManager = clearance.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var dataWritingManager = new DataWritingManager(actionInfo, conversionProgressInfoCollector);

			clearance.CusUSLVConsignmentBatches.MoveNextBatch();
			var clearanceDO = dataContextManager.GetShipmentDataObjectWriter(dataWritingManager).GetDataObject(clearance);

			UpdateProgress(Res.GetString("620faf02-c7e0-428c-8865-2b1dee4dd7bd", "Generating Consolidated Summary"), 0, progressForm);

			using (new DisposableAction(conversionFactory.SuspendValidation, conversionFactory.ResumeValidation))
			{
				var universalFactory = new UniversalObjectFactory(conversionFactory);

				var declarationReader = ObjectFactory.Get<ITopLevelDataObjectReader>("USLVConsolidatedDeclarationReader", clearanceDO, conversionProgressInfoCollector, universalFactory);
				declaration = declarationReader.ReadIntoTopLevelBusinessObject() as JobDeclaration;
			}

			addCESLogAction = () =>
			{
				declaration.PopulateJE_DeclarationReferenceIfNeeded();
				clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, declaration.JE_DeclarationReference));
			};

			return declaration;
		}

		List<JobDeclaration> ConvertMultipleSummariesInCurrentFactory(ProgressForm progressForm, out Action addCESLogAction)
		{
			var declarations = new List<JobDeclaration>();
			var clearance = BusinessEntity;
			var conversionFactory = BusinessEntity.Factory;
			var conversionProgressInfoCollector = new LowValueEntriesToConsolidatedSummaryConvertTracker(clearance.CusUSLVConsignments.Count, (text, percetage) => UpdateProgress(text, percetage, progressForm));

			var actionInfo = new ActionInfo(RecipientRoleType.BRO, clearance)
			{
				ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage
			};
			var dataContextManager = clearance.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var dataWritingManager = new DataWritingManager(actionInfo, conversionProgressInfoCollector);

			var clearanceDOByBatch = new List<ITopLevelDataObject>();
			while (clearance.CusUSLVConsignmentBatches.MoveNextBatch())
			{
				var clearanceDO = dataContextManager.GetShipmentDataObjectWriter(dataWritingManager).GetDataObject(clearance);
				clearanceDOByBatch.Add(clearanceDO);
			}

			using (new DisposableAction(conversionFactory.SuspendValidation, conversionFactory.ResumeValidation))
			{
				var universalFactory = new UniversalObjectFactory(conversionFactory);
				var processedCount = 0;
				var totalCount = clearanceDOByBatch.Count;

				UpdateProgress(Res.GetString("f4e456c9-f3d6-43fe-8973-47f92500d1a5", "[{0} / {1}] Consolidated Summaries generated ", processedCount, totalCount), 0, progressForm);

				foreach (var clearanceDO in clearanceDOByBatch)
				{
					var declarationReader = ObjectFactory.Get<ITopLevelDataObjectReader>("USLVConsolidatedDeclarationReader", clearanceDO, conversionProgressInfoCollector, universalFactory);
					var declaration = declarationReader.ReadIntoTopLevelBusinessObject() as JobDeclaration;
					declarations.Add(declaration);

					UpdateProgress(Res.GetString("f4e456c9-f3d6-43fe-8973-47f92500d1a5", "[{0} / {1}] Consolidated Summaries generated ", ++processedCount, totalCount), progressForm.PercentComplete, progressForm);
				}
			}

			addCESLogAction = () =>
			{
				foreach (var declaration in declarations)
				{
					declaration.PopulateJE_DeclarationReferenceIfNeeded();
					clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, declaration.JE_DeclarationReference));
				}
			};

			return declarations;
		}

		void UpdateProgress(ZString text, int percentage, ProgressForm progressForm)
		{
			if (text.IsEmpty)
			{
				text = progressForm.Status;
			}

			progressForm.SetStatusAndPercentComplete(text, percentage);
		}

		bool UserConfirmToCreateMultipleConsolidatedEntrySummaries(int numOfSummaries)
		{
			var messageContent = $"{numOfSummaries} Consolidated Summaries will be created as it exceeds the 999 invoice lines limit.";

			return DialogResult.OK == Globals.Message.Show(
					messageContent,
					Res.GetString("84407080-ace8-40af-aaea-fe76d0651078", "Create Consolidated Summary"),
					MessageBoxButtons.OKCancel,
					DialogResult.OK);
		}

		void OpenConsolidatedSummary(IList<JobDeclaration> declarations)
		{
			if (declarations.Count == 1)
			{
				ShowSingleDeclarationForm(declarations[0]);
			}
			else
			{
				ShowConsolidatedEntrySummaryList();
			}
		}

		void ShowSingleDeclarationForm(JobDeclaration declaration)
		{
			var declarationController = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			declarationController.ShowChildrenAsDialog = true;

			if (declaration.IsInDatabase)
			{
				declarationController.ShowEditForm(declaration);
			}
			else
			{
				declarationController.ShowFormOfGivenDisplayType(declaration, ZArchitecture.Core.ODisplayMode.New);
			}
		}

		void ShowConsolidatedEntrySummaryList()
		{
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration);
			var filterDefaults = new FilterBusinessObjectDefaults
			{
				new FilterBusinessObjectDefault("Entry Type", "Property", (ZString)EntryTypeList.Codes.InformalFreeDutiable),
				new FilterBusinessObjectDefault("Master Bill", "Property", BusinessEntity.ULH_MasterBill),
				new FilterBusinessObjectDefault("Shipment Type", "Property", (ZString)"IMP"),
				new FilterBusinessObjectDefault("Created Time", "PropertySearch", ModuleDateFilter.DateRangeSearchTexts.Last12Mths)
			};
			module.FilterBusinessObject.SetExternalDefaults(filterDefaults);

			using var modulePopup = new EmbeddedModulePopup(module);
			modulePopup.Shown += (sender, e) =>
			{
				if (sender is EmbeddedModulePopup popup)
				{
					var filterControl = popup.FindSingle<ZFilterStripControl>();
					filterControl?.FirePerformSearch();
				}
			};

			ZFormModaliser.ShowDialogAndDispose(modulePopup);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (BusinessEntity != null)
			{
				Enterprise.Customs.Business.JobComInvoiceLinePartSynchronisationManager.StopManagingWhenActiveDeciderPKWasDisposed(BusinessEntity.Factory, BusinessEntity.PK);
			}

			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		(int, int) SendMessages(UpdateActionCode updateAction)
		{
			var totalBills = gridHouseBills.SelectedElements.Length;
			var consignments = gridHouseBills.SelectedElements.OfType<CusUSLVConsignment>().ToList();
			var billsSent = 0;

			if (SavePendingChangesAndContinue() && CustomsSendMessageHelper.USLVClearanceImportMessagingIsAllowed())
			{
				CustomsSendMessageHelper.InitMessagingAction(consignments, updateAction);

				if (updateAction == UpdateActionCode.Delete)
				{
					var clearanceWrapper = new CusUSLVClearanceMessageWrapper(BusinessEntity, consignments);
					if (ZFormModaliser.ShowDialogAndDispose(new MessagesSubmitForm(clearanceWrapper, updateAction)) == DialogResult.Yes)
					{
						FireSaveButton();
					}
				}
				else
				{
					var billsToSend = CustomsSendMessageHelper.PrepareMessagesToSend(consignments, updateAction, false);
					CustomsSendMessageHelper.AllocateEntryNumbers(BusinessEntity, billsToSend);
					billsSent = CustomsSendMessageHelper.SendToCustoms(BusinessEntity, billsToSend, updateAction);
					if (FireSaveButton() == ContinueWithSave.No)
					{
						billsSent = 0;
					}
				}
			}

			return (billsSent, totalBills);
		}

		void AddMenuItemsIntoActions()
		{
			if (BusinessEntity is { } clearance)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
				ActionsMenuItem.MenuItems.Add(DisclaimApplicablePGAsHelper.AddDisclaimApplicablePGAsMenuItem((_, __) =>
				{
					if (SaveData())
					{
						var applicableConsignments = clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Where(x => x.ULB_HasPGAPending).ToArray();
						var applicator = new DisclaimApplicablePGAsApplicator(BusinessEntity.Factory);
						applicator.Build(applicableConsignments.Select(c => c.PK).ToArray());
						ZFormModaliser.ShowDialogAndDispose(new DisclaimApplicablePGAsChildForm(applicator, applicableConsignments));
					}
				}));

				ActionsMenuItem.MenuItems.Add(convertPartyAddressesToNewOrgActionMenuItem ??= GetConvertPartyAddressesToNewOrgMenuItem());
			}
		}

		bool SavePendingChangesAndContinue()
		{
			if (!BusinessEntity.IsInDatabase || BusinessEntity.HasChanges)
			{
				if (Globals.Message.Show(dataHasNotYetBeenSavedMessage, dataHasNotYetBeenSavedTitle, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					return FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		bool SaveData()
		{
			var result = true;

			if (BusinessEntity.HasChanges)
			{
				if (Globals.Message.Show(dataHasNotYetBeenSavedMessage, dataHasNotYetBeenSavedTitle, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					result = FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					result = false;
				}
			}

			return result;
		}

		ZMenuItem GetConvertPartyAddressesToNewOrgMenuItem()
		{
			return new ZMenuItem(convertPartyAddressesToNewOrganizationsCaption, HandleConvertPartyAddressesToNewOrgClick);

			void HandleConvertPartyAddressesToNewOrgClick(object sender, EventArgs e)
			{
				var consignments = GetConsignments(sender).ToList();
				if (!consignments.IsNullOrEmpty() && consignments.Any(c => !c.ConsigneeIsOrganisation || !c.ShipperIsOrganisation))
				{
					var conversion = new FreeTextAddressConversion<CusUSLVConsignment>(BusinessEntity.Factory);
					conversion.AddToList(consignments);

					using (var freeTextAddressConversionForm = new FreeTextAddressConversionForm<CusUSLVConsignment>(conversion, false, false, new FreeTextAddressGridLayoutProvider()))
					{
						ZFormModaliser.ShowDialogWithoutDispose(freeTextAddressConversionForm);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("f50c81b9-5612-49f5-9ef8-00268ea6387a", "All consignments have linked to organizations"));
				}
			}
		}

		IEnumerable<CusUSLVConsignment> GetConsignments(object sender)
		{
			IEnumerable<CusUSLVConsignment> result = null;
			if (sender == convertPartyAddressesToNewOrgContextMenuItem)
			{
				result = gridHouseBills.SelectedElements.OfType<CusUSLVConsignment>();
			}
			else if(sender == convertPartyAddressesToNewOrgActionMenuItem)
			{
				result = BusinessEntity.CusUSLVConsignments.OfType<CusUSLVConsignment>();
			}

			return result;
		}

		ZMenuItem convertPartyAddressesToNewOrgContextMenuItem;
		ZMenuItem convertPartyAddressesToNewOrgActionMenuItem;

		readonly ZString dataHasNotYetBeenSavedMessage = Res.GetString("37a867f6-ac9b-4d35-aab1-84479bc84f74", "The data has not yet been saved. Do you want to save and proceed?");
		readonly ZString dataHasNotYetBeenSavedTitle = Res.GetString("f60602c1-35e5-4d60-a565-ac719eb6c451", "Save Data");
		readonly ZString convertPartyAddressesToNewOrganizationsCaption = Res.GetString("4bb577b0-7f40-4db9-879d-0da6bf46e5de", "Convert Party Addresses to Organizations");
	}
}
