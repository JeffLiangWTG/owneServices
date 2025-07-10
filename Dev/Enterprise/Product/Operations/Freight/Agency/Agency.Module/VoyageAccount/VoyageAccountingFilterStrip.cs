using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public class VoyageAccountingFilterStrip : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter related")]
		public static class Descriptions
		{
			public const string JobNumber = "Job #";
			public const string VoyageVessel = "Voyage / Vessel";
			public const string Principal = "Principal";
			public const string TradeLane = "Trade Lane";
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleFilter jobNumberFilter = new ModuleFountainFilter(Descriptions.JobNumber, JobVoyAccountSchema.NA_JobNumber, "VA");
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|VoyageAccountingFilter|JobNumber", "Job #");

			return jobNumberFilter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			AddVoyageVesselFilters(collection);
			AddOrganisationsAndStaffFilters(collection);
			AddLocationFilters(collection);

			AccountingFilterStrip.AddBillingFilters(collection);
			AccountingFilterStrip.AddJobManagementFilters(collection, Env.Security.AgencyVoyageAccountJobInvoicing);

			return collection;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(base.Filter);
				result.AddToFilter(JobVoyAccountSchema.NA_GC, GlbCompany.CurrentCompany.PK);
				return result;
			}
		}

		#region AddVoyageVesselFilters

		void AddVoyageVesselFilters(ModuleFilterCollection collection)
		{
			var filter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, GetVoyageVesselFilter, new RefVesselCollection(Factory))
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|VoyageAccountingFilter|VoyageVessel", "Voyage / Vessel");
			filter.SubGroup = JobVoyageFilterProcessor;
			filter.Category = FilterCategories.NumbersAndReferences;
			collection.AddCustomFilter(filter);
		}

		ZQuery GetVoyageVesselFilter(SQLComparisonOperator opp, ZString voyageNumber, ZString vessel, ZBool includeArchived)
		{
			return VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(opp, voyageNumber, vessel, includeArchived, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
		}

		#endregion

		#region AddOrganisationsAndStaffFilters

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection collection)
		{
			ModuleGuidFilter principalFilter = collection.AddGuidFilter(Descriptions.Principal, ModuleIDs.Organisation, JobVoyAccountSchema.NA_OH, new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory));
			principalFilter.Category = FilterCategories.Organisations;
			principalFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|VoyageAccountingFilter|Principal", "Principal");

			if (!Env.Security.AgencyPrincipalAccess.IsAllowed)
			{
				principalFilter.Visibility = FilterVisibility.AlwaysVisible;
				principalFilter.PropertyValidation = PrincipalFilterValidation;
			}
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var tradeLaneFilter = filters.AddGuidFilter(Descriptions.TradeLane, ModuleIDs.TradeLane, JobTradeLaneVoyageSchema.NB_EJ, new JobTradeLaneCollection(Factory));
			tradeLaneFilter.Category = FilterCategories.Locations;
			tradeLaneFilter.SubGroup = TradeLaneFilterProcessor;
			tradeLaneFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|VoyageAccountingFilter|TradeLane", "Trade Lane");
		}

		#endregion

		#region Implementation

		#region Validation

		void PrincipalFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("a6bcc25e-00d4-493d-84d7-fc4fdc7cb10d", "Please select a principal to filter by"));
			}
		}

		#endregion

		#region Sub Groups

		ModuleFilterSubGroup TradeLaneFilterProcessor => tradeLaneFilterProcessor ?? (tradeLaneFilterProcessor = new TradeLaneSubGroup(JobVoyageFilterProcessor));
		TradeLaneSubGroup tradeLaneFilterProcessor;

		class TradeLaneSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var tradeLaneVoyageQuery = new ZDBOnlySubQuery(typeof(JobTradeLaneVoyage), JobTradeLaneVoyageSchema.NB_JV);
				tradeLaneVoyageQuery.AddToFilter(filter);

				var voyageFilter = new ZDBOnlyQuery(typeof(JobVoyage));
				voyageFilter.AddSubQuery(tradeLaneVoyageQuery, JoinCondition.And);

				return voyageFilter;
			}
		}

		ModuleFilterSubGroup JobVoyageFilterProcessor => jobVoyageFilterProcessor ?? (jobVoyageFilterProcessor = new JobVoyageSubGroup());
		JobVoyageSubGroup jobVoyageFilterProcessor;

		class JobVoyageSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyAccountSchema.NA_JV);
				voyageSubQuery.AddToFilter(filter);

				var query = new ZDBOnlyQuery(typeof(VoyageAccount));
				query.AddSubQuery(voyageSubQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#endregion

		#region IAccountingFilterStripHolder Members

		internal IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}
		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(VoyageAccount) },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Shipping|VoyageAccountingFilter|InvoiceStatus", "Invoice Status") }
		};

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(VoyageAccount));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion
	}
}

