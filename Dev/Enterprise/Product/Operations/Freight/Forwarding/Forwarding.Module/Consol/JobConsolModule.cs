using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.GUI.Internal.EmbeddedModulePopup;

namespace Enterprise.Freight.Forwarding.Module
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public partial class JobConsolModule : TemplateRecordSupportedFilterGridModule, IOperationalActionSupportable
	{
		public JobConsolModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Actions Menu

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, ImportFromXml_Click);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] menuItems = base.GetNewStandardMenuItems();

			CopyMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.Copy.SingleCopy", "Single Copy"), HandleTemplateCopyClick));
			CopyMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.Copy.PerSchedule", "Per Schedule"), CopyPerSchedule_Click));

			return menuItems;
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem gatewayProfitShareRedistributionMenuItem = new ZMenuItem(Res.GetString("a0c21c98-2f16-4734-b59b-a97bbdfd4e65", "Redistribute G/W Consol Profit"));
			gatewayProfitShareRedistributionMenuItem.Click += gatewayProfitShareRedistributionMenuItem_Click;
			result.Add(gatewayProfitShareRedistributionMenuItem);

			MenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as MenuItem;
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			result.Add(postMenuItem);

			if (GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.UnitedArabEmirates)
			{
				MenuItem allocateInstalmentMenu = new ZMenuItem((NoResString)"Allocate UAE Installment Numbers"); // United Arab Emirates country specific does not need to be translated
				allocateInstalmentMenu.Click += new EventHandler(allocateInstalmentMenu_Click);
				result.Add(allocateInstalmentMenu);
			}

			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result);

			var loadListMenu = new ZMenuItem(HVLVOriginLoadListDescription);
			loadListMenu.MenuItems.Add(new ZMenuItem(ViewLoadListDescription, ViewLoadListClick));
			loadListMenu.MenuItems.Add(new ZMenuItem(CreateFromLoadListDescription, CreateFromELoadListMenuItemClick));
			result.Add(loadListMenu);

			AddSetInspectionStatusMenuItem(result);

			return result.ToArray();
		}

		#endregion

		void gatewayProfitShareRedistributionMenuItem_Click(object sender, EventArgs e)
		{
			gatewayConsolProfitShareRedistributionModuleModule = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.GatewayConsolProfitShareRedistribution);
			gatewayConsolProfitShareRedistributionModulePopup = new EmbeddedModulePopup(gatewayConsolProfitShareRedistributionModuleModule);
			gatewayConsolProfitShareRedistributionModulePopup.RequireAtLeastOneItemToBeSelected = false;
			gatewayConsolProfitShareRedistributionModulePopup.Show();
		}
		EmbeddedModulePopup gatewayConsolProfitShareRedistributionModulePopup;
		ZFilterModule gatewayConsolProfitShareRedistributionModuleModule;

		void allocateInstalmentMenu_Click(object sender, EventArgs e)
		{
			var allocate = new AllocateUAEInstalmentNumbers();
			ZString message = allocate.Allocate(SelectedBusinessObjects);

			if (!message.IsEmpty)
			{
				Globals.Message.Show(message);
			}
		}

		#region Creating consol from eLoadLists

		string CreateFromLoadListWithConcurrentErrorCaption => Res.GetString("67ca4bbe-967d-4822-ac63-7b94195ab712", "Unable to complete action");
		string CreateFromLoadListWithConcurrentErrorMessage => Res.GetString("140ef3e3-7d97-4c09-9545-6b30a363f84d", "While you have been working with this form, another user has made changes which cannot be merged. Please select another eLoadList or try again later.");

		string HVLVOriginLoadListDescription => ResString.GetMultilingualString("Forwarding.Consol.Actions.HVLVOriginLoadList", "HVLV Origin Load List");
		string CreateFromLoadListDescription => ResString.GetMultilingualString("Forwarding.Consol.Actions.CreateFromLoadList", "Create Consolidation from Origin Load List");
		string ViewLoadListDescription => ResString.GetMultilingualString("Forwarding.Consol.Actions.ScheduleCopyFromLoadList", "View Origin Load List");

		void CreateFromELoadListMenuItemClick(object sender, EventArgs e)
		{
			CreateFromELoadListMenuItemClick(ModuleIDs.HVLVOriginLoadList, "CreateConsolFromLoadListModuleDecisionProvider", CreateConsolFromLoadLists);
		}

		void ViewLoadListClick(object sender, EventArgs e)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.HVLVOriginLoadList))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.RequireAtLeastOneItemToBeSelected = false;
				ZFormModaliser.ShowDialogWithoutDispose(popup);
			}
		}

		ForwardingConsol CreateConsolFromLoadLists(IEnumerable<BusinessObject> loadLists)
		{
			var success = false;

			ForwardingConsol newConsol = null;
			var factory = new BusinessObjectFactory();
			var loadListsInNewFactory = loadLists.Select(o => (IHVLVOriginLoadList)factory.ImportFromAnotherFactory(o)).ToList();

			var logger = new SimpleLogger();
			var consolAllocator = ObjectFactory.Get<IHVLVOriginLoadListConsolAllocator>(nameof(IHVLVOriginLoadListConsolAllocator), logger);
			var loadListPKs = loadListsInNewFactory.Select(o => o.PK).ToList();
			if (!consolAllocator.TryAcquireApplicationLocks(loadListPKs,
				(lockedPKs) =>
				{
					var loadListsWithAppLock = loadListsInNewFactory.Where(o => lockedPKs.Any(x => x == o.PK));
					success = consolAllocator.TryCreateConsolAndAttachLoadLists(loadListsWithAppLock, out _);
					if (success)
					{
						newConsol = consolAllocator.AllocatedConsol as ForwardingConsol;
					}
				},

				out var _))
			{
				Globals.Message.Show(CreateFromLoadListWithConcurrentErrorMessage, CreateFromLoadListWithConcurrentErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}

			var loggerContent = logger.ToString().Trim();
			if (!loggerContent.IsNullOrEmpty())
			{
				Globals.Message.ShowWarning(loggerContent, AttachLoadListToConsolGUIHelper.AttachLoadListToConsolWarningCaption);
			}
			if (!success)
			{
				Globals.Message.ShowError(loggerContent, CreateFromLoadListWithConcurrentErrorCaption);
				return null;
			}

			return TrySavingFactory(factory) ? newConsol : null;
		}

		void CreateFromELoadListMenuItemClick(ModuleIdentifier moduleId, string moduleDecisionProviderName, Func<IEnumerable<BusinessObject>, ForwardingConsol> createConsolFunc)
		{
			ForwardingConsol newConsol = null;
			SelectedEventHandler loadListsSelected = (innerSender, innerEventArgs) =>
			{
				newConsol = createConsolFunc(innerEventArgs.SelectedBusinessObjects);
			};

			var findBox = new VirtualFindBox();
			var decisionProvider = ObjectFactory.Get<IModuleDecisionProvider>(moduleDecisionProviderName, Factory, findBox);

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleId))
			{
				module.OverrideModuleDecisionProvider(decisionProvider);

				using (var popup = new EmbeddedModulePopup(module))
				{
					popup.Selected += loadListsSelected;
					findBox.Initialize(decisionProvider.List, popup);
					ZFormModaliser.ShowDialogWithoutDispose(popup);
				}
			}

			if (newConsol != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
				controller.ShowEditForm(newConsol);
			}
		}

		bool TrySavingFactory(BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();

				return true;
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.Show(CreateFromLoadListWithConcurrentErrorMessage, CreateFromLoadListWithConcurrentErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

				return false;
			}
		}

		#endregion

		#region Copy Per Schedule

		void CopyPerSchedule_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedConsols = Grid.SelectedElements;

			if (selectedConsols.Length == 0)
			{
				ShowNoSelectedMessage();
			}
			else if (selectedConsols.Length > 1)
			{
				Globals.Message.Show(Res.GetString("b9b3f381-02d8-47f7-8d0d-8c9dafaacb65", "You can only select a single item to perform this action on as it requires user intervention to complete the action."));
			}
			else
			{
				var consol = (ForwardingConsol)selectedConsols[0];

				if (IsTemplateRecord(consol))
				{
					Globals.Message.ShowError(TemplateRecordCopyErrorMessage);
				}
				else if (consol.IsSea)
				{
					Globals.Message.Show(Res.GetString("f588edff-4923-4759-bdda-0c74a78a08a0", "You cannot copy SEA Consolidation because \"Per Schedule\" actions attempts to copy Schedules as well, however, the same vessel-voyage combination cannot be duplicated.  Use \"Single Copy\" instead to copy a Consol for the same Sailing Schedule."));
				}
				else if (!consol.IsBuyersConsol && !consol.IsAgentOrDirect)
				{
					Globals.Message.Show(Res.GetString("47789937-9e52-47ca-89fa-5627f2a11d4b", "You can only copy a Consol based on a Schedule for Direct, Agent, Gateway Agent, Courier or Buyers Consolidations."));
				}
				else if (consol.Transports.MostInterestingTransport == null || consol.Transports.MostInterestingTransport.Sailing == null)
				{
					Globals.Message.Show(Res.GetString("0476B939-752B-4060-A80E-57406EEF22B1", "You can only copy a Consol that is linked to a Schedule."));
				}
				else
				{
					using (var bulkCopyCriteria = new ForwardingBulkCopyCriteria(consol.Transports.MostInterestingTransport.Sailing.PK, true))
					{
						bulkCopyCriteria.ConsolDetails.TemplateConsolPK = consol.PK;
						bulkCopyCriteria.ConsolDetails.CreateConsol = true;
						bulkCopyCriteria.ConsolDetails.CreateConsol_ReadOnly = true;

						ZFormModaliser.ShowDialogAndDispose(new BulkScheduleCopyForm(bulkCopyCriteria));
						Grid.Refresh();
					}
				}
			}
		}

		#endregion

		#region Posting

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
#if DEBUG
			if (((IBulkPostingModuleInternalsForTesting)this).DontDoActualPosting)
			{
				SetLastUsedPostingOptionForTest(postingOption);
			}
			else
#endif
			{
				IJobCostingPlugIn[] selectedConsols = new IJobCostingPlugIn[Grid.SelectedElements.Length];

				for (int i = 0; i < Grid.SelectedElements.Length; i++)
				{
					selectedConsols[i] = Factory.Load<ForwardingConsol>(((IIdentified)Grid.SelectedElements[i]).Identifier);
				}

				BulkPostingHelper.PostTransactions(postingOption, selectedConsols);
			}
		}

		IBulkPostingModuleHelper BulkPostingHelper
		{
			get
			{
				if (fBulkPostingHelper == null)
				{
					fBulkPostingHelper = ObjectFactory.Get<IBulkPostingModuleHelper>();
					fBulkPostingHelper.Initialize("", true);
				}
				return fBulkPostingHelper;
			}
		}
		IBulkPostingModuleHelper fBulkPostingHelper;

#if DEBUG

		protected virtual void SetLastUsedPostingOptionForTest(JobInvoicingPostingOption postingOption)
		{
		}

#endif
		#endregion

		#region PrintJobProfitDocument

		void PrintJobProfitDocument()
		{
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, Grid.SelectedElements);
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper => fBulkJobProfitPrintingHelper ?? (fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>("BulkConsolProfitPrintingModuleHelper"));

		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		#region Implementation

		public override ModuleIdentifier ID => ModuleIDs.JobConsol;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Consol };

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.MaintainConsol;

		protected override SortInfo DefaultSortOrder => new SortInfo(CommonConsol.Schema.JK_UniqueConsignRef, ListSortDirection.Descending);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobConsol);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobConsolFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetupFactory(Factory);
			return new ForwardingModuleConsolCollection(Factory) { AllowTemplateRecords = AllowLoadTemplateRecords };
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetupFactory(factory);
			return factory;
		}

		void SetupFactory(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				SetGuiProviders(factory);
				ChildEditableService.SetState(factory, ChildEditableServiceStates.Consol);
				factory.SetFreightDomainContext(FreightDomainContext.Forwarding);
			}
		}

		void SetGuiProviders(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ForwardingShipmentDocumentSupporterGuiQueryProvider.Register(factory);
				ForwardingConsolDocumentSupporterGuiQueryProvider.Register(factory);
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobConsolFilterBusinessObject(AllowLoadTemplateRecords);
		}

		#endregion

		#region Template record

		string TemplateRecordCopyErrorMessage => Res.GetString("9af421a1-5d66-491e-88b5-06981b626b1b", "The Copy function cannot be used to copy template records. Instead, use the Universal Copy function to create new records from templates.");

		bool IsTemplateRecord(BusinessObject businessObject) => businessObject is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord;

		protected override bool SupportTemplateRecords => true;

		protected override string ModuleReferenceNumberFilterString => JobConsolFilterBusinessObject.Descriptions.ConsolNum;

		protected override BusinessObject GetNewTemplateRecordBusinessObjectCore()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			SetupFactory(factory);

			var newConsol = factory.New<ForwardingConsol>();

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ID.Name;

			factory.TemplateRecordProvider = newConsol;
			factory.TemplateRecordProvider.IsTemplateRecord = true;
			factory.TemplateRecordProvider.TemplateRecord = templateRecord;

			return newConsol;
		}

		protected override BusinessObject LoadFromTemplateRecordPkCore(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
		{
			SetupFactory(localFactory);

			var templateRecord = localFactory.Load<StmTemplateRecord>(templateRecordPk);
			if (templateRecord != null && templateRecord.STR_ModuleID == ID.Name)
			{
				var typeOfElement = GridCollection?.TypeOfElements ?? typeof(ForwardingConsol);
				var consol = localFactory.New(typeOfElement);
				var templateRecordProvider = consol as ITemplateRecordProvider;

				if (templateRecordProvider != null)
				{
					templateRecordProvider.LoadFromTemplateRecord(templateRecord);
					return consol;
				}
			}

			return null;
		}

		protected override void AddBusinessSpecificFiltersToTemplateQuery(ZDBOnlyQuery query)
		{
			var activeTemplateModuleFilters = FilterBusinessObject.ActiveModuleFilters.Where(filter => filter.GetXQuery != null || filter.XQueryInfo != null);
			var templateQuery_2011_11 = GetCombinedTemplateFilter(UniversalXmlInfo.Namespace_2011_11, activeTemplateModuleFilters);
			var templateQuery_2012_11 = GetCombinedTemplateFilter(UniversalXmlInfo.Namespace_2012_11, activeTemplateModuleFilters);
			templateQuery_2011_11.AddAsUnionQuery(templateQuery_2012_11, true);

			query.AddSubQuery(templateQuery_2011_11, JoinCondition.And);
		}

		protected override void AddBusinessSpecificFiltersToBaseQuery(ZQuery query)
		{
			var templateNameFilter = FilterBusinessObject
				.ActiveModuleFilters
				.OfType<ModuleTextFilter>()
				.FirstOrDefault(textFilter =>
					textFilter.IsActive &&
					textFilter.Description == FilterStripBusinessObject.TemplateRecordsTemplateName
				);

			if (templateNameFilter != null)
			{
				var templateNameSubQuery = new ZDBOnlySubQuery(
					typeof(StmTemplateRecord),
					StmTemplateRecordSchema.PK
				);

				templateNameSubQuery.AddToFilter(
					StmTemplateRecordSchema.STR_TemplateName,
					templateNameFilter.SqlComparisonOperator,
					templateNameFilter.Property
				);

				var consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
				consolQuery.AddSubQuery(JobConsolSchema.JK_STR, templateNameSubQuery, JoinCondition.And);

				query.AddToFilter(consolQuery);
			}
		}

		ZDBOnlySubQuery GetCombinedTemplateFilter(string namespaceVersion, IEnumerable<ModuleFilter> activeTemplateModuleFilters)
		{
			using (XQueryFilterHelper.SetNamespaceTemp(namespaceVersion))
			{
				var templateRecordsQuery = new ZDBOnlySubQuery(typeof(StmTemplateRecord), StmTemplateRecordSchema.PK);
				templateRecordsQuery.AddToFilter(ModuleFilterCombiner.GetCombinedTemplateFilter(activeTemplateModuleFilters));
				return templateRecordsQuery;
			}
		}

		ModuleFilterCombiner ModuleFilterCombiner
		{
			get { return moduleFilterCombiner ?? (moduleFilterCombiner = new ModuleFilterCombiner()); }
		}

		ModuleFilterCombiner moduleFilterCombiner;

		#endregion

		#region Import from XML

		void ImportFromXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new ForwardingConsolValueObjectDataAdapter(true), true).PromptUserAndImport(BillingInterfaceName.ConsolXmlImport); // Interface name for billing purposes
		}

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter => new ForwardingConsolActionSupporter();

		#endregion

		#region Set Inspection Status

		void AddSetInspectionStatusMenuItem(List<MenuItem> menu)
		{
			new PackLineInspectionTypeBulkUpdatePresentationManager().CreateMenus(this, menu, GetSelectedShipments);
		}

		ForwardingShipment[] GetSelectedShipments(ZFilterGridModule module)
		{
			var selectedBizObjs = module.GetSelectedBusinessObjects();
			var shipments = new List<ForwardingShipment>();
			foreach (BusinessObject bizObj in selectedBizObjs)
			{
				if (bizObj is ForwardingConsol consol)
				{
					foreach (ForwardingShipment shipment in consol.Shipments)
					{
						shipments.Add(shipment);
					}
				}
			}
			return shipments.ToArray();
		}

		#endregion
	}
}
