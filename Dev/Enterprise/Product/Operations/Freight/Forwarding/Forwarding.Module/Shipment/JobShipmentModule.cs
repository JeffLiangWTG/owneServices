using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobShipmentModule : TemplateRecordSupportedFilterGridModule, IOperationalActionSupportable
#if DEBUG
, IBulkPostingModuleInternalsForTesting
#endif
	{
		public JobShipmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Overrides

		public override ModuleIdentifier ID => ModuleIDs.JobShipment;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

		string NameOfSingleObject => Res.GetString("Forwarding|JobShipmentModule|NameOfSingleObject", "Shipment");

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Shipment };

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click));
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("-"));

			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			result.Add(postMenuItem);

			result.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.POD", "POD"), new EventHandler(HandlePODClick)));

			Func<IDtbBookingParent> selectedObjectGetter = () => (IDtbBookingParent)CurrentBusinessObjectInGrid;
			var dtbBookingProvider = ObjectFactory.New<IDtbBookingMenuProvider>(selectedObjectGetter);
			dtbBookingProvider.ShowFormsFromMainThread = ShowFormsFromMainThread;
			result.Add((MenuItem)dtbBookingProvider.ConstructMenu());

			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result);
			new PackLineInspectionTypeBulkUpdatePresentationManager().CreateMenus(this, result);

			return result.ToArray();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.MaintainShipment;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobShipment);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobShipmentFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
			=> new ForwardingModuleShipmentCollection(Factory) { AllowTemplateRecords = AllowLoadTemplateRecords };

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
				ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
				factory.SetFreightDomainContext(FreightDomainContext.Forwarding);
			}
		}

		void SetGuiProviders(BusinessObjectFactory factory)
		{
			ForwardingShipmentDocumentSupporterGuiQueryProvider.Register(factory);
			ServicesSelectionGuiProvider.Register(factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobShipmentFilterBusinessObject(AllowLoadTemplateRecords);
		}

		protected override SortInfo DefaultSortOrder => new SortInfo(JobShipmentSchema.Constants.JS_UniqueConsignRef, ListSortDirection.Descending);

		protected void HandlePODClick(object sender, EventArgs e)
		{
			OpenQuickPOD();
		}

		protected void OpenQuickPOD()
		{
			ZControllerFactory.Create(ControllerIDs.QuickPOD).ShowNewForm();
		}

		#endregion

		#region Posting

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
#if DEBUG
			if (((IBulkPostingModuleInternalsForTesting)this).DontDoActualPosting)
			{
				fLastUsedPostingOptionForTest = postingOption;
			}
			else
#endif
			{
				BusinessObjectFactory workFactory = new BusinessObjectFactory();
				{
					List<ZGuid> pks = new List<ZGuid>(Grid.SelectedElements.Select(element => element.PK));
					ForwardingShipment[] shipments = workFactory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, pks));
					BulkPostingHelper.PostTransactions(postingOption, shipments);
				}
				GCWrapper.ReclaimMemory(ref workFactory);
			}
		}

		IBulkPostingModuleHelper BulkPostingHelper
		{
			get
			{
				if (fBulkPostingHelper == null)
				{
					fBulkPostingHelper = ObjectFactory.Get<IBulkPostingModuleHelper>();
					fBulkPostingHelper.Initialize(Description, false);
				}

				return fBulkPostingHelper;
			}
		}
		IBulkPostingModuleHelper fBulkPostingHelper;

		#region IBulkPostingModuleInternalsForTesting Members

#if DEBUG
		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest => fLastUsedPostingOptionForTest;

		JobInvoicingPostingOption fLastUsedPostingOptionForTest;

		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				MenuItem menuItem = GetNewActionMenuItems().FindByText("&Post");
				IMenuItem iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new InvalidOperationException("Post menu item should have type ZMenuItem");
				}
				return iMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}
#endif

		#endregion

		#endregion

		#region PrintJobProfitDocument

		void PrintJobProfitDocument()
		{
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, Grid.SelectedElements);
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper => fBulkJobProfitPrintingHelper ?? (fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>());

		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		#region Menus

		protected void OnImportFromXml_Click(object sender, EventArgs e)
		{
			GetNewXmlDataTransferDirector(new ForwardingShipmentValueObjectDataAdapter(), true).PromptUserAndImport(BillingInterfaceName.ShipmentXmlImport); // Interface name for billing purposes
		}

#if DEBUG
		protected virtual
#endif
		XmlDataTransferDirector GetNewXmlDataTransferDirector(IValueObjectDataAdapter adapter, bool checkForLicence)
		{
			return new XmlDataTransferDirector(adapter, checkForLicence);
		}

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter => new ForwardingShipmentSupporter();

		#endregion

		#region Template record

		protected override bool SupportTemplateRecords => true;

		protected override string ModuleReferenceNumberFilterString => JobShipmentFilterBusinessObject.Descriptions.ShipmentNum;

		protected override BusinessObject GetNewTemplateRecordBusinessObjectCore()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			SetupFactory(factory);

			var newShipment = factory.New<ForwardingShipment>();

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ID.Name;

			factory.TemplateRecordProvider = newShipment;
			factory.TemplateRecordProvider.IsTemplateRecord = true;
			factory.TemplateRecordProvider.TemplateRecord = templateRecord;

			return newShipment;
		}

		protected override BusinessObject LoadFromTemplateRecordPkCore(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
		{
			SetupFactory(localFactory);

			var templateRecord = localFactory.Load<StmTemplateRecord>(templateRecordPk);
			if (templateRecord == null || templateRecord.STR_ModuleID != ID.Name)
			{
				return null;
			}

			var typeOfElement = GridCollection?.TypeOfElements ?? typeof(ForwardingShipment);
			var shipment = localFactory.New(typeOfElement);
			var templateRecordProvider = shipment as ITemplateRecordProvider;
			if (templateRecordProvider == null)
			{
				return null;
			}

			templateRecordProvider.LoadFromTemplateRecord(templateRecord);
			return shipment;
		}

		protected override void AddBusinessSpecificFiltersToTemplateQuery(ZDBOnlyQuery query)
		{
			var activeTemplateModuleFilters = FilterBusinessObject.ActiveModuleFilters.Where(filter => filter.GetXQuery != null || filter.XQueryInfo != null);
			var templateQuery_2011_11 = GetCombinedTemplateFilter(UniversalXmlInfo.Namespace_2011_11, activeTemplateModuleFilters);
			var templateQuery_2012_11 = GetCombinedTemplateFilter(UniversalXmlInfo.Namespace_2012_11, activeTemplateModuleFilters);
			templateQuery_2011_11.AddAsUnionQuery(templateQuery_2012_11, true);

			query.AddSubQuery(templateQuery_2011_11, JoinCondition.And);
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
	}
}
