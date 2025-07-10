using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public abstract class QuotedBookingModuleBase : TemplateRecordSupportedFilterGridModule, IDocumentBusinessContext
	{
		public abstract override ModuleIdentifier ID { get; }

		public override bool SupportsWorkflow => true;

		protected abstract override ZController GetNewController(BusinessObject selectedBusinessObject);

		protected abstract ZController GetNewController(QuotedBookingState state);

		protected override IFilterControl GetNewFilterControl() => new QuotedBookingFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => GetNewGridCollectionCore();

		protected abstract IBusinessObjectCollection GetNewGridCollectionCore();

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			RegisterGuiProviders(factory);
			return factory;
		}

		static void RegisterGuiProviders(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ShipmentDocumentSupporterGuiQueryProvider.Register(factory);
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			AddNewActionMenuItemsCore(result);

			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result, GetValidScreeningBusinessObjects);
			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();

			return result.ToArray();
		}

		protected virtual void AddNewActionMenuItemsCore(List<MenuItem> menu) { }

		protected (BusinessObject[] BusinessObjects, bool HasInvalidItems) GetValidScreeningBusinessObjects(ZFilterGridModule module)
		{
			var bizOs = module.GetSelectedBusinessObjects();
			var shipmentsBizOs = new List<BusinessObject>();
			var shouldNotify = false;
			foreach (var bizO in bizOs)
			{
				if (!(bizO is ViewQuotedBooking viewQuotedBooking))
				{
					continue;
				}

				if (viewQuotedBooking.QuotedBooking != null && viewQuotedBooking.QuotedBooking.Booking != null && !viewQuotedBooking.IsTemplate)
				{
					shipmentsBizOs.Add(viewQuotedBooking.QuotedBooking.Booking);
				}
				else
				{
					shouldNotify = true;
				}
			}

			return (shipmentsBizOs.ToArray(), shouldNotify);
		}

		public override string WorkflowType => WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode;

		protected abstract override FilterBusinessObject GetNewFilterBusinessObject();

		protected override bool CanBeReversed() => typeof(ITemplateReversible).IsAssignableFrom(typeof(QuotedBooking));

		protected override bool CanBeCopied() => typeof(ITemplateCopyable).IsAssignableFrom(typeof(QuotedBooking));

		#region Template record

		protected abstract override bool SupportTemplateRecords { get; }

		protected override string ModuleReferenceNumberFilterString => BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.BookingNumber;

		protected override BusinessObject GetNewTemplateRecordBusinessObjectCore()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			using (ShipmentFieldStateChange.InitializingShipment(factory))
			{
				var booking = QuotedBooking.CreateNewBooking(factory);
				var newBizO = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);

				var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
				templateRecord.STR_ModuleID = ID.Name;

				factory.TemplateRecordProvider = newBizO;
				factory.TemplateRecordProvider.IsTemplateRecord = true;
				factory.TemplateRecordProvider.TemplateRecord = templateRecord;

				return ViewQuotedBooking.LoadOrCreate(newBizO);
			}
		}

		protected override BusinessObject LoadFromTemplateRecordPkCore(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
		{
			var templateRecord = localFactory.Load<StmTemplateRecord>(templateRecordPk);
			if (templateRecord == null || templateRecord.STR_ModuleID != ID.Name)
			{
				return null;
			}

			var quotedBooking = QuotedBooking.New(localFactory, templateRecord); //We will load from the template inside of QuotedBooking.Initialize.
			var templateRecordProvider = quotedBooking as ITemplateRecordProvider;
			if (templateRecordProvider == null)
			{
				return null;
			}

			return quotedBooking;
		}

		protected override BusinessObject LoadTemplateRecordIntoCollection(BusinessObjectFactory factory, Type type, StmTemplateRecord template)
		{
			var quotedBooking = QuotedBooking.New(Factory, template); //We will load from the template inside of QuotedBooking.Initialize.
			return ViewQuotedBooking.LoadOrCreate(quotedBooking);
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

		ModuleFilterCombiner ModuleFilterCombiner => moduleFilterCombiner ?? (moduleFilterCombiner = new ModuleFilterCombiner());
		ModuleFilterCombiner moduleFilterCombiner;

		#endregion

		#region Universal Copy

		protected override void OnSetupAndGetGrid(ZDisplayGrid grid)
		{
			base.OnSetupAndGetGrid(grid);

			if (previousGrid?.ContextMenu != null)
			{
				previousGrid.ContextMenu.Popup -= ContextMenu_Popup;
			}

			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}

			previousGrid = grid;
		}

		ZDisplayGrid previousGrid;

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var universalCopyMenuItem = DisplayGrid.ContextMenu.MenuItems.FindByName("UniversalCopy");
			if (universalCopyMenuItem == null)
			{
				return;
			}

			universalCopyMenuItem.Visible = AllowUniversalCopy;
			universalCopyMenuItem.Enabled = AllowUniversalCopy;

			var copyScheduleMenuItem = universalCopyMenuItem.MenuItems.FindByName("CopySchedules");
			if (copyScheduleMenuItem != null)
			{
				copyScheduleMenuItem.Visible = false;
				copyScheduleMenuItem.Enabled = false;
			}

			var copyScheduleSplitterMenuItem = universalCopyMenuItem.MenuItems.FindByName("CopySchedulesSplitter");
			if (copyScheduleSplitterMenuItem != null)
			{
				copyScheduleSplitterMenuItem.Visible = false;
				copyScheduleSplitterMenuItem.Enabled = false;
			}
		}

		public override bool AllowUniversalCopy
		{
			get
			{
				if (!Globals.IsUserInteractive)
				{
					return true;
				}

				return base.GetSelectedBusinessObjects()
					.OfType<ViewQuotedBooking>()
					.All(qb => !qb.VB_IsConsolidated);
			}
		}

		#endregion

		#region ToolBarButtons

		protected override ResourceStringData NewTemplateRecordText =>
			Res.GetData("QuotedBookingModule.NewTemplateRecord", "New Quick Booking Template Record", "Creates a new Quick Booking template record.");

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();
			AddNewStandardMenuItemsCore();
			return result;
		}

		protected abstract void AddNewStandardMenuItemsCore();

		#endregion

		#region Security / Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Booking;

		public abstract override SecurityCheckpoint SecurityCheckpoint { get; }

		#endregion

		#region IDocumentBusinessContext members

		BusinessContext IDocumentBusinessContext.BusinessContext => BusinessContext.QuotedBooking;

		#endregion
	}
}
