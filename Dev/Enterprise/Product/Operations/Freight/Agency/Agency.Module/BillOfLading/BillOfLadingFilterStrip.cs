using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Module.ImportReleaseOrder;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Module.JobShipmentFilterBusinessObject;

namespace Enterprise.Freight.Agency.Module
{
	public class BillOfLadingFilterStrip : AgencyShipmentFilterStrip
	{
		#region SuppressResourceStringsCheckRegion

		public new class Descriptions : AgencyShipmentFilterStrip.Descriptions
		{
			public const string EIDOStatus = "E-IDO Status";
			public const string DockReceiptNumber = "Dock Receipt #";
			public const string ImportReleaseOrderStatus = "Import Release Order Status";
			public const string PackLineExportReference = "Pack Line Export Reference #";
			public const string PackLineImportReference = "Pack Line Import Reference #";
		}

		#endregion
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery baseFilter = new ZQuery(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
				return new ZQuery(baseFilter, base.Filter);
			}
		}

		#region AddNumberFilters

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			var docReceiptFilter = filters.AddNumberFilter(Descriptions.DockReceiptNumber, JobContainerSchema.JC_DepartureDockReceipt);
			docReceiptFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|DockReceiptNumber", "Dock Receipt #");
			docReceiptFilter.SubGroup = ContainerFilterProcessor;

			var importRefNumberFilter = filters.AddTextFilter(Descriptions.PackLineImportReference, JobPackLinesSchema.JL_ImportRefNumber);
			importRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|PackLineImportRefNumber", "Pack Line Import Reference #");
			importRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			importRefNumberFilter.SubGroup = PackLineSubGroup;

			var exportRefNumberFilter = filters.AddTextFilter(Descriptions.PackLineExportReference, JobPackLinesSchema.JL_ExportRefNumber);
			exportRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|PackLineExportRefNumber", "Pack Line Export Reference #");
			exportRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			exportRefNumberFilter.SubGroup = PackLineSubGroup;
		}

		ModuleFilterSubGroup PackLineSubGroup => packLineSubGroup ?? (packLineSubGroup = new PackLineFilterSubGroup());
		PackLineFilterSubGroup packLineSubGroup;

		#endregion

		#region AddStatusAndFlagsFilters

		protected override void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			base.AddStatusAndFlagsFilters(filters);

			if (AgencyRegistry.Instance.EIDOMessagingDetails.Value.Identities.Count > 0)
			{
				ModuleTextFilter filter = filters.AddTextFilter(Descriptions.EIDOStatus, EIDOStatusFilter, new EIDOFilterList());
				filter.Category = FilterCategories.StatusAndFlags;
				filter.SubGroup = ContainerFilterProcessor;
				filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillOfLadingFilter|EIDOStatus", "E-IDO Status");
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.NewZealand)
			{
				var importReleaseOrderStatusFilter = filters.AddTextFilter(Descriptions.ImportReleaseOrderStatus, GetImportReleaseOrderFilter, new ImportReleaseOrderFilterList());
				importReleaseOrderStatusFilter.Category = FilterCategories.StatusAndFlags;
				importReleaseOrderStatusFilter.SubGroup = ContainerFilterProcessor;
				importReleaseOrderStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillOfLadingFilter|ImportReleaseOrderStatus", "Import Release Order Status");
			}
		}

		ZQuery EIDOStatusFilter(ZString mode)
		{
			return EIDOStatusFilterHelper.GetEIDOStatusFilter(mode);
		}

		ZQuery GetImportReleaseOrderFilter(ZString status)
		{
			return ImportReleaseOrderFilterHelper.GetFilterQuery(status);
		}

		#endregion

		#region Implementation

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelper(typeof(BillOfLading), WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode, Factory));

			return helpers;
		}

		protected override ZBool AllowSearchOfUnlocoOutsideLoginBranch
		{
			get { return Env.Security.AgencyBillOfLadingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed; }
		}

		protected override CodeDescriptionPairList NewShipmentStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair("BOL-All", Res.GetString("c4ac9f0f-4895-4b62-9b6c-ec91beb341cc", "Bills of Lading"));
			result.AddRange(new AgencyShipmentStatusList(true));
			return result;
		}

		protected override ZString DefaultShipmentStatusFilter
		{
			get { return "BOL-All"; }
		}

		protected override Security.SecurityCheckpoint JobInvoicingSecurity
		{
			get { return Env.Security.AgencyBillOfLadingJobInvoicing; }
		}

		readonly BillOfLadingCRMSecurityProvider SecurityProvider = new BillOfLadingCRMSecurityProvider();

		#endregion
	}
}



